using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Tetoris SkillData", menuName = "ScriptableObjects/SkillData/Tetoris", order = 0)]
public class TetorisSkillData : PlayerSkillData
{
    public const string RandomKey = nameof(TetorisSkillData);

    [SerializeField] private DamageProjectile[] _prefabs;
    [SerializeField] private AudioClip _shootSound;
    [SerializeField] private float _shootVolume = 1f;
    [SerializeField] private float _shootPitch = 1f;

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

    [Header("Grab")]
    [SerializeField] private float _grabForce;
    [SerializeField] private float _hpRegenAddition;

    public override float GetCooldown(Player p, PlayerSkill skill)
    {
        return Mathf.Max(_minCooldown, _baseCooldown - (skill.Level - 1) * _cooldownShrink);
    }

    public override string GetDescription(Player p, PlayerSkill skill)
    {
        var attackParams = GetProjectileParams(p, skill);
        return $"기본 지속 효과 : HP 재생이 {_hpRegenAddition:0.0} 증가합니다.\n\n" +
            $"보고 있는 방향으로 랜덤한 모양의 테트리스를 발사합니다.\n" +
            $"테트리스는 각각 {StringUtil.MagicalValue(attackParams.Damage)}의 마법 피해를 입히고, 플레이어를 끌어당깁니다.";
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
        var projectile = Instantiate(_prefabs[GameManager.Instance.GetSeedRandomRange(RandomKey, 0, _prefabs.Length - 1)]);
        projectile.AttackParams = GetProjectileParams(p, skill);
        projectile.transform.eulerAngles = new(0, 0, GameManager.Instance.GetSeedRandomRange(RandomKey, 0, 3) * 90);
        projectile.RegisterCollisionEvent(damageable =>
        {
            if(damageable is Player other)
                other.Knockback((p.PlayerRenderer.transform.position -
                    other.PlayerRenderer.transform.position).normalized * _grabForce);
        });
        return projectile;
    }

    public override void OnActiveUse(Player p, PlayerSkill skill)
    {
        SoundManager.Instance.PlaySFX(_shootSound, p.transform.position, _shootVolume, _shootPitch);
        Projectile.Shoot(() => GetProjectile(p, skill), p, 
            p.PlayerRenderer.transform.position, 
            p.PlayerRenderer.transform.eulerAngles.z,
            1, 0, 1f);
    }

    public override void OnPassiveUpdate(Player p, PlayerSkill skill)
    {
        p.Stat.Add(StatType.HPRegen, _hpRegenAddition);
    }
}
