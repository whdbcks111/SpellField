using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "LavaBurn SkillData", menuName = "ScriptableObjects/SkillData/LavaBurn", order = 0)]
public class LavaBurnSkillData : PlayerSkillData
{
    [SerializeField] private DamageProjectile _prefab;
    [SerializeField] private AudioClip _shootSound;

    [Header("Damage")]
    [SerializeField] private float _baseDamage;
    [SerializeField] private float _damageMagicForceCoefficient;
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

    [Header("Fire")]
    [SerializeField] private int _baseFireLevel;
    [SerializeField] private float _fireLevelGrowth;
    [SerializeField] private float _fireTime;

    public override float GetCooldown(Player p, PlayerSkill skill)
    {
        return Mathf.Max(_minCooldown, _baseCooldown - (skill.Level - 1) * _cooldownShrink);
    }

    public int GetAngleCount(Player p, PlayerSkill skill)
    {
        return Mathf.Min((int)(_baseAngleCount + (skill.Level - 1) * _angleCountGrowth), _maxAngleCount);
    }

    public int GetFireLevel(PlayerSkill skill)
    {
        return _baseFireLevel + (int)(_fireLevelGrowth * (skill.Level - 1));
    }

    public override string GetDescription(Player p, PlayerSkill skill)
    {
        var attackParams = GetProjectileParams(p, skill);
        return $"용암 불똥을 방사형으로 {GetAngleCount(p, skill)}개 발사합니다.\n" +
            $"불똥은 각각 {StringUtil.MagicalValue(attackParams.Damage)}의 마법 피해를 입히고, " +
            $"Lv.{GetFireLevel(skill)} {EffectType.Fire.DisplayName} 효과를 {_fireTime:0.0}초간 부여합니다.";
    }

    public override float GetManaCost(Player p, PlayerSkill skill)
    {
        return _baseManaCost + (skill.Level - 1) * _manaCostGrowth;
    }

    public AttackParams GetProjectileParams(Player p, PlayerSkill skill)
    {
        return p.Stat.GetMagicalAttackParams(_baseDamage + (skill.Level - 1) * _damageGrowth +
            p.Stat.Get(StatType.MagicForce) * _damageMagicForceCoefficient);
    }

    public Projectile GetProjectile(Player p, PlayerSkill skill)
    {
        var projectile = Instantiate(_prefab);
        projectile.AttackParams = GetProjectileParams(p, skill);
        projectile.RegisterCollisionEvent(damageable =>
        {
            if(damageable is Player other)
            {
                other.AddEffect(new Effect(EffectType.Fire, GetFireLevel(skill), _fireTime, p));
            }
        });
        return projectile;
    }

    public override void OnActiveUse(Player p, PlayerSkill skill)
    {
        SoundManager.Instance.PlaySFX(_shootSound, p.transform.position, 1f, 1f);
        Projectile.Shoot(() => GetProjectile(p, skill), p, 
            p.PlayerRenderer.transform.position, 
            p.PlayerRenderer.transform.eulerAngles.z,
            1, GetAngleCount(p, skill), 360f / GetAngleCount(p, skill), 0f, 1.3f);
    }

    public override void OnPassiveUpdate(Player p, PlayerSkill skill)
    {

    }
}
