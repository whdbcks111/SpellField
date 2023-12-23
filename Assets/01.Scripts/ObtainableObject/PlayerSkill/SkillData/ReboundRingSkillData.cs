using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ReboundRing SkillData", menuName = "ScriptableObjects/SkillData/ReboundRing", order = 0)]
public class ReboundRingSkillData : PlayerSkillData
{
    [SerializeField] private DamageProjectile _prefab;

    [Header("Damage")]
    [SerializeField] private float _baseDamage;
    [SerializeField] private float _damageAttackCoefficient;
    [SerializeField] private float _damageGrowth;

    [Header("Mana Cost")]
    [SerializeField] private float _baseManaCost;
    [SerializeField] private float _manaCostGrowth;

    [Header("Cooldown")]
    [SerializeField] private float _baseCooldown;
    [SerializeField] private float _cooldownShrink;
    [SerializeField] private float _minCooldown;

    [Header("Angle Count")]
    [SerializeField] private int _baseAngleCount;
    [SerializeField] private float _angleCountGrowth;
    [SerializeField] private int _maxAngleCount;
    [SerializeField] private float _angleSpan;

    [Header("Effect")]
    [SerializeField] private int _bloodEffectLevel;
    [SerializeField] private float _bloodEffectTime;
    [SerializeField] private float _effectLevelGrowth;

    public override float GetCooldown(Player p, PlayerSkill skill)
    {
        return Mathf.Max(_minCooldown, _baseCooldown - (skill.Level - 1) * _cooldownShrink);
    }

    public int GetAngleCount(Player p, PlayerSkill skill)
    {
        return Mathf.Min((int)(_baseAngleCount + (skill.Level - 1) * _angleCountGrowth), _maxAngleCount);
    }

    public int GetEffectLevel(PlayerSkill skill)
    {
        return (int)(_bloodEffectLevel + _effectLevelGrowth * (skill.Level - 1));
    }

    public override string GetDescription(Player p, PlayerSkill skill)
    {
        var attackParams = GetProjectileParams(p, skill);
        return $"역방향으로 가속하는 고리를 보고 있는 방향으로 {GetAngleCount(p, skill)}개 발사합니다.\n" +
            $"고리는 각각 {StringUtil.PhysicalValue(attackParams.Damage)}의 물리 피해를 입힙니다.";
    }

    public override float GetManaCost(Player p, PlayerSkill skill)
    {
        return _baseManaCost + (skill.Level - 1) * _manaCostGrowth;
    }

    public AttackParams GetProjectileParams(Player p, PlayerSkill skill)
    {
        return p.Stat.GetMagicalAttackParams(_baseDamage + (skill.Level - 1) * _damageGrowth +
            p.Stat.Get(StatType.Attack) * _damageAttackCoefficient);
    }

    public Projectile GetProjectile(Player p, PlayerSkill skill)
    {
        var projectile = Instantiate(_prefab);
        projectile.AttackParams = GetProjectileParams(p, skill);
        projectile.RegisterCollisionEvent(damageable =>
        {
            if (damageable is Player player) player.AddEffect(new(EffectType.Blood, GetEffectLevel(skill), _bloodEffectTime, p));
        });
        return projectile;
    }

    public override void OnActiveUse(Player p, PlayerSkill skill)
    {
        Projectile.Shoot(() => GetProjectile(p, skill), p, 
            p.PlayerRenderer.transform.position, 
            p.PlayerRenderer.transform.eulerAngles.z,
            1, GetAngleCount(p, skill), _angleSpan, 0f, 1f);
    }

    public override void OnPassiveUpdate(Player p, PlayerSkill skill)
    {

    }
}
