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

    public static readonly StatType MaxHP = new(WrapperType.MaxHP, nameof(MaxHP), "최대 체력",
        stat => "가질 수 있는 최대 체력량입니다. 피해를 받을 때 소모됩니다.",
        "", "%", 500f, 1f, float.MaxValue, "<sprite=\"StatIcons\" name=\"MaxHP\">");

    public static readonly StatType MaxMana = new(WrapperType.MaxMana, nameof(MaxMana), "최대 마나",
        stat => "가질 수 있는 최대 마나량입니다. 일반적으로 스킬을 사용할 때 소모됩니다.",
        "", "%", 500f, 1f, float.MaxValue, "<sprite=\"StatIcons\" name=\"MaxMana\">");

    public static readonly StatType MoveSpeed = new(WrapperType.MoveSpeed, nameof(MoveSpeed), "이동 속도",
        stat => "1초 당 움직일 수 있는 속도입니다.",
        "", "%", 4f, 0f, 20f, "<sprite=\"StatIcons\" name=\"MoveSpeed\">");

    public static readonly StatType Attack = new(WrapperType.Attack, nameof(Attack), "물리 공격력",
        stat => "일반적으로 물리 피해를 가할 때의 공격력입니다.",
        "", "%", 30f, 1f, float.MaxValue, "<sprite=\"StatIcons\" name=\"Attack\">");

    public static readonly StatType MagicForce = new(WrapperType.MagicForce, nameof(MagicForce), "마법력",
        stat => "일반적으로 마법 공격에 사용되는 능력치입니다.",
        "", "%", 0f, 1f, float.MaxValue, "<sprite=\"StatIcons\" name=\"MagicForce\">");

    public static readonly StatType DefendPenetrate = new(WrapperType.DefendPenetrate, nameof(DefendPenetrate), "방어력 관통력",
        stat => $"물리 공격 시 대상의 {Defend.DisplayName}을(를) <color=white>{stat.Get(DefendPenetrate):0.}%</color>만큼 무시합니다.",
        "%p", "%", 0f, 0f, 100f, "<sprite=\"StatIcons\" name=\"DefendPenetrate\">");

    public static readonly StatType MagicPenetrate = new(WrapperType.MagicPenetrate, nameof(MagicPenetrate), "마법 관통력",
        stat => $"물리 공격 시 대상의 {MagicResistance.DisplayName}을(를) <color=white>{stat.Get(MagicPenetrate):0.}%</color>만큼 무시합니다.",
        "%p", "%", 0f, 0f, 100f, "<sprite=\"StatIcons\" name=\"MagicPenetrate\">");

    public static readonly StatType Defend = new(WrapperType.Defend, nameof(Defend), "물리 방어력",
        stat => $"받는 물리 피해의 <color=white>{Stat.GetDefendRatio(stat.Get(Defend)) * 100 :0.}%</color>를 감소시킵니다.",
        "", "%", 0f, 0f, 99f, "<sprite=\"StatIcons\" name=\"Defend\">");

    public static readonly StatType MagicResistance = new(WrapperType.MagicResistance, nameof(MagicResistance), "마법 저항력",
        stat => $"받는 마법 피해의 <color=white>{Stat.GetDefendRatio(stat.Get(MagicResistance)) * 100 :0.}%</color>를 감소시킵니다.",
        "", "%", 0f, 0f, 99f, "<sprite=\"StatIcons\" name=\"MagicResistance\">");

    public static readonly StatType CriticalChance = new(WrapperType.CriticalChance, nameof(CriticalChance), "치명타 확률",
        stat => $"{stat.Get(CriticalChance):0.}%의 확률로 <color=#ff4444>{stat.Get(CriticalDamage):0.}%</color>의 추가 피해를 입힙니다.", 
        "%p", "%", 5f, 0f, 100f, "<sprite=\"StatIcons\" name=\"CriticalChance\">");

    public static readonly StatType CriticalDamage = new(WrapperType.CriticalDamage, nameof(CriticalDamage), "치명타 피해량",
        stat => $"치명타 시 입히는 추가 피해량입니다.", 
        "%p", "%", 75f, 1f, float.MaxValue, "<sprite=\"StatIcons\" name=\"CriticalDamage\">");

    public static readonly StatType HPRegen = new(WrapperType.HPRegen, nameof(HPRegen), "체력 재생",
        stat => "1초 당 증가하는 체력의 양입니다.", 
        "", "%", 1f, 0f, float.MaxValue, "<sprite=\"StatIcons\" name=\"HPRegen\">");

    public static readonly StatType ManaRegen = new(WrapperType.ManaRegen, nameof(ManaRegen), "마나 재생",
        stat => "1초 당 증가하는 마나의 양입니다.", 
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
