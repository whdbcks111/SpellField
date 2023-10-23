using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Stat
{
    private readonly Dictionary<StatType, float> 
        baseValues = new(), 
        addValues = new(), 
        multiplyValues = new(), 
        currentValues = new();

    public Stat()
    {
        foreach (var statType in StatType.GetAll())
        {
            baseValues[statType] = statType.DefaultValue;
            currentValues[statType] = baseValues[statType];
        }
        InitStat();
    }

    private void InitStat()
    {
        foreach (var statType in StatType.GetAll())
        {
            addValues[statType] = 0f;
            multiplyValues[statType] = 1f;
        }
    }

    public void LateUpdate()
    {
        foreach (var statType in StatType.GetAll())
        {
            currentValues[statType] = (baseValues[statType] + addValues[statType]) * multiplyValues[statType];
            addValues[statType] = 0f;
            multiplyValues[statType] = 1f;
        }
    }

    public static bool CriticalCheck(float chance)
    {
        return chance >= 100f || UnityEngine.Random.Range(0f, 100f) < chance;
    }

    public AttackParams GetPhysicalAttackParams()
    {
        return GetPhysicalAttackParams(Get(StatType.Attack));
    }

    public AttackParams GetPhysicalAttackParams(float damage)
    {
        var attackParams = GetAttackParams(damage);

        attackParams.DamageType = DamageType.Physical;
        attackParams.Penetrate = Get(StatType.DefendPenetrate);

        return attackParams;
    }

    public AttackParams GetMagicalAttackParams(float damage)
    {
        var attackParams = GetAttackParams(damage);

        attackParams.DamageType = DamageType.Magical;
        attackParams.Penetrate = Get(StatType.MagicPenetrate);

        return attackParams;
    }

    public AttackParams GetAttackParams(float damage)
    {
        return new()
        {
            CriticalChance = Get(StatType.CriticalChance),
            CriticalDamage = Get(StatType.CriticalDamage),
            Damage = damage,
            Penetrate = 0,
            DamageType = DamageType.Fixed
        };
    }

    public static DamageParams GetDamageParams(AttackParams attackParams, Damageable target)
    {
        var damage = attackParams.Damage;
        float defend = 0f;
        if (attackParams.DamageType == DamageType.Magical) defend = target.Stat.Get(StatType.MagicResistance);
        else if (attackParams.DamageType == DamageType.Physical) defend = target.Stat.Get(StatType.Defend);

        damage *= 1 - GetDefendRatio(Mathf.Max(0, defend - attackParams.Penetrate));

        var isCritical = CriticalCheck(attackParams.CriticalChance);
        var criticalDamage = isCritical ? damage * attackParams.CriticalDamage / 100f : 0f;

        return new()
        {
            IsCriticalAttack = isCritical,
            NormalDamage = damage,
            CriticalDamage = criticalDamage,
            TotalDamage = damage + criticalDamage,
            Type = attackParams.DamageType
        };
    }

    public static float GetDefendRatio(float defend)
    {
        // 1 is 100% defend, 0 is 0% defend (same)
        return defend / (100 + defend);
    }

    public void Add(StatType statType, float value)
    {
        addValues[statType] += value;
    }

    public void Multiply(StatType statType, float value)
    {
        multiplyValues[statType] *= value;
    }

    public void SetBase(StatType statType, float value)
    {
        baseValues[statType] = value;
    }

    public float Get(StatType statType)
    {
        return Mathf.Clamp(currentValues[statType], statType.MinValue, statType.MaxValue);
    }
}

public class StatType : Enumeration<StatType>
{
    public delegate string StatDescriptor(Stat stat);

    public readonly string DisplayName, AddSuffix, MultiplySuffix, TextIcon;
    public readonly float DefaultValue, MinValue, MaxValue;
    private readonly WrapperType _wrapperType;
    public readonly StatDescriptor GetDescription;

    private StatType(WrapperType type, string name, string displayname, StatDescriptor descriptor, string addSuffix, string multiplySuffix,
        float defaultValue, float minValue, float maxValue, string textIcon) : base(name)
    {
        _wrapperType = type;
        GetDescription = descriptor;
        DisplayName = displayname;
        AddSuffix = addSuffix;
        MultiplySuffix = multiplySuffix;
        DefaultValue = defaultValue;
        MinValue = minValue;
        MaxValue = maxValue;
        TextIcon = textIcon;
    }

    public static StatType GetByWrapperType(WrapperType type)
    {
        return GetAll().Find(t => t._wrapperType == type);
    }

    public static readonly StatType MaxHP = new(WrapperType.MaxHP, nameof(MaxHP), "�ִ� ü��",
        stat => "���� �� �ִ� �ִ� ü�·��Դϴ�. ���ظ� ���� �� �Ҹ�˴ϴ�.",
        "", "%", 500f, 1f, float.MaxValue, "<sprite=\"StatIcons\" name=\"MaxHP\">");

    public static readonly StatType MaxMana = new(WrapperType.MaxMana, nameof(MaxMana), "�ִ� ����",
        stat => "���� �� �ִ� �ִ� �������Դϴ�. �Ϲ������� ��ų�� ����� �� �Ҹ�˴ϴ�.",
        "", "%", 500f, 1f, float.MaxValue, "<sprite=\"StatIcons\" name=\"MaxMana\">");

    public static readonly StatType MoveSpeed = new(WrapperType.MoveSpeed, nameof(MoveSpeed), "�̵� �ӵ�",
        stat => "1�� �� ������ �� �ִ� �ӵ��Դϴ�.",
        "", "%", 4f, 0f, 20f, "<sprite=\"StatIcons\" name=\"MoveSpeed\">");

    public static readonly StatType Attack = new(WrapperType.Attack, nameof(Attack), "���� ���ݷ�",
        stat => "�Ϲ������� ���� ���ظ� ���� ���� ���ݷ��Դϴ�.",
        "", "%", 30f, 1f, float.MaxValue, "<sprite=\"StatIcons\" name=\"Attack\">");

    public static readonly StatType MagicForce = new(WrapperType.MagicForce, nameof(MagicForce), "������",
        stat => "�Ϲ������� ���� ���ݿ� ���Ǵ� �ɷ�ġ�Դϴ�.",
        "", "%", 0f, 1f, float.MaxValue, "<sprite=\"StatIcons\" name=\"MagicForce\">");

    public static readonly StatType DefendPenetrate = new(WrapperType.DefendPenetrate, nameof(DefendPenetrate), "���� �����",
        stat => $"���� ���� �� ����� {Defend.DisplayName}��(��) <color=white>{stat.Get(DefendPenetrate):0.}%</color>��ŭ �����մϴ�.",
        "%p", "%", 0f, 0f, 100f, "<sprite=\"StatIcons\" name=\"DefendPenetrate\">");

    public static readonly StatType MagicPenetrate = new(WrapperType.MagicPenetrate, nameof(MagicPenetrate), "���� �����",
        stat => $"���� ���� �� ����� {MagicResistance.DisplayName}��(��) <color=white>{stat.Get(MagicPenetrate):0.}%</color>��ŭ �����մϴ�.",
        "%p", "%", 0f, 0f, 100f, "<sprite=\"StatIcons\" name=\"MagicPenetrate\">");

    public static readonly StatType Defend = new(WrapperType.Defend, nameof(Defend), "���� ����",
        stat => $"�޴� ���� ������ <color=white>{Stat.GetDefendRatio(stat.Get(Defend)) * 100 :0.}%</color>�� ���ҽ�ŵ�ϴ�.",
        "", "%", 0f, 0f, 99f, "<sprite=\"StatIcons\" name=\"Defend\">");

    public static readonly StatType MagicResistance = new(WrapperType.MagicResistance, nameof(MagicResistance), "���� ���׷�",
        stat => $"�޴� ���� ������ <color=white>{Stat.GetDefendRatio(stat.Get(MagicResistance)) * 100 :0.}%</color>�� ���ҽ�ŵ�ϴ�.",
        "", "%", 0f, 0f, 99f, "<sprite=\"StatIcons\" name=\"MagicResistance\">");

    public static readonly StatType CriticalChance = new(WrapperType.CriticalChance, nameof(CriticalChance), "ġ��Ÿ Ȯ��",
        stat => $"{stat.Get(CriticalChance):0.}%�� Ȯ���� <color=#ff4444>{stat.Get(CriticalDamage):0.}%</color>�� �߰� ���ظ� �����ϴ�.", 
        "%p", "%", 5f, 0f, 100f, "<sprite=\"StatIcons\" name=\"CriticalChance\">");

    public static readonly StatType CriticalDamage = new(WrapperType.CriticalDamage, nameof(CriticalDamage), "ġ��Ÿ ���ط�",
        stat => $"ġ��Ÿ �� ������ �߰� ���ط��Դϴ�.", 
        "%p", "%", 75f, 1f, float.MaxValue, "<sprite=\"StatIcons\" name=\"CriticalDamage\">");

    public static readonly StatType HPRegen = new(WrapperType.HPRegen, nameof(HPRegen), "ü�� ���",
        stat => "1�� �� �����ϴ� ü���� ���Դϴ�.", 
        "", "%", 1f, 0f, float.MaxValue, "<sprite=\"StatIcons\" name=\"HPRegen\">");

    public static readonly StatType ManaRegen = new(WrapperType.ManaRegen, nameof(ManaRegen), "���� ���",
        stat => "1�� �� �����ϴ� ������ ���Դϴ�.", 
        "", "%", 15f, 0f, float.MaxValue, "<sprite=\"StatIcons\" name=\"ManaRegen\">");

    public enum WrapperType
    {
        MaxHP, 
        MaxMana, 
        MoveSpeed, 
        Attack, 
        MagicForce, 
        DefendPenetrate, 
        MagicPenetrate, 
        Defend, 
        MagicResistance, 
        CriticalChance, 
        CriticalDamage, 
        HPRegen, 
        ManaRegen
    }
}

public struct DamageParams
{
    public bool IsCriticalAttack;
    public float TotalDamage;
    public float CriticalDamage;
    public float NormalDamage;
    public DamageType Type;
}

public struct AttackParams
{
    public float CriticalChance;
    public float CriticalDamage;
    public float Penetrate;
    public float Damage;
    public DamageType DamageType;
}

public class DamageType : Enumeration<DamageType>
{
    public DamageType(string name) : base(name)
    {
    }

    public static readonly DamageType Fixed = new(nameof(Fixed));
    public static readonly DamageType Physical = new(nameof(Physical));
    public static readonly DamageType Magical = new(nameof(Magical));
}
