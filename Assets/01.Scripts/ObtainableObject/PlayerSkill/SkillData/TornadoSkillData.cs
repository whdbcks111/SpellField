using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Tornado", menuName = "ScriptableObjects/SkillData/Tornado", order = 0)]
public class TornadoSkillData : PlayerSkillData
{
    [SerializeField] private DamageProjectile _prefab;
    [SerializeField] private AudioClip _activeSound;
    [SerializeField] private float _activeSoundVolume = 1f;
    [SerializeField] private float _activeSoundPitch = 1f;

    [Header("Damage")]
    [SerializeField] private float _baseDamage;
    [SerializeField] private float _damageMagicForceCoefficient;
    [SerializeField] private float _damageGrowth;
    [SerializeField] private float _baseKnockback;
    [SerializeField] private float _knockbackGrowth;

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

    public override float GetCooldown(Player p, PlayerSkill skill)
    {
        return Mathf.Max(_minCooldown, _baseCooldown - (skill.Level - 1) * _cooldownShrink);
    }

    public int GetAngleCount(Player p, PlayerSkill skill)
    {
        return Mathf.Min((int)(_baseAngleCount + (skill.Level - 1) * _angleCountGrowth), _maxAngleCount);
    }

    public float GetKnockbackForce(PlayerSkill skill)
    {
        return _baseKnockback + (skill.Level - 1) * _knockbackGrowth;
    }

    public override string GetDescription(Player p, PlayerSkill skill)
    {
        var attackParams = GetProjectileParams(p, skill);
        return $"바람 회오리를 방사형으로 {GetAngleCount(p, skill)}개 발사합니다.\n" +
            $"회오리는 각각 {StringUtil.MagicalValue(attackParams.Damage)}의 마법 피해를 입히고, " +
            $"플레이어에게 적중 시 {GetKnockbackForce(skill):0.0}의 힘으로 밀쳐냅니다.";
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
                other.Knockback(projectile.transform.right * GetKnockbackForce(skill));
            }
        });
        return projectile;
    }

    public override void OnActiveUse(Player p, PlayerSkill skill)
    {
        SoundManager.Instance.PlaySFX(_activeSound, p.transform.position, _activeSoundVolume, _activeSoundPitch);
        Projectile.Shoot(() => GetProjectile(p, skill), p, 
            p.PlayerRenderer.transform.position, 
            p.PlayerRenderer.transform.eulerAngles.z,
            GetAngleCount(p, skill), 360f / GetAngleCount(p, skill), 1f);
    }

    public override void OnPassiveUpdate(Player p, PlayerSkill skill)
    {

    }
}
