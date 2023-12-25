using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "IceStrike SkillData", menuName = "ScriptableObjects/SkillData/IceStrike", order = 0)]
public class IceStrikeSkillData : PlayerSkillData
{
    [SerializeField] private IceStrikeProjectile _prefab;
    [SerializeField] private AudioClip _shootSound;
    [SerializeField] private float _shootSoundVolume = 1f;
    [SerializeField] private float _shootSoundPitch = 1f;

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

    [Header("Effect")]
    [SerializeField] private int _baseEffectLevel;
    [SerializeField] private float _effectLevelGrowth;
    [SerializeField] private float _effectTime;

    public override float GetCooldown(Player p, PlayerSkill skill)
    {
        return Mathf.Max(_minCooldown, _baseCooldown - (skill.Level - 1) * _cooldownShrink);
    }

    private int GetEffectLevel(PlayerSkill skill)
    {
        return (int)(_baseEffectLevel + _effectLevelGrowth * (skill.Level - 1));
    }

    public override string GetDescription(Player p, PlayerSkill skill)
    {
        var attackParams = GetProjectileParams(p, skill);
        return $"빙결 에너지를 보고 있는 방향으로 발사합니다.\n" +
            $"적중 시 주변 모든 물체에 {StringUtil.MagicalValue(attackParams.Damage)}의 마법 피해를 입히고,\n" +
            $"{_effectTime:0.0}초간 Lv.{GetEffectLevel(skill):0} {EffectType.Frozen.DisplayName} 효과를 부여합니다.";
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

    public IceStrikeProjectile GetProjectile(Player p, PlayerSkill skill)
    {
        var projectile = Instantiate(_prefab);
        projectile.AttackParams = GetProjectileParams(p, skill);
        projectile.EffectLevel = GetEffectLevel(skill);
        projectile.EffectTime = _effectTime;
        return projectile;
    }

    public override void OnActiveUse(Player p, PlayerSkill skill)
    {
        SoundManager.Instance.PlaySFX(_shootSound, p.transform.position, _shootSoundVolume, _shootSoundPitch);
        Projectile.Shoot(() => GetProjectile(p, skill), p, 
            p.PlayerRenderer.transform.position, 
            p.PlayerRenderer.transform.eulerAngles.z,
            1, 1, 0, 0f, 1f);
    }

    public override void OnPassiveUpdate(Player p, PlayerSkill skill)
    {

    }
}
