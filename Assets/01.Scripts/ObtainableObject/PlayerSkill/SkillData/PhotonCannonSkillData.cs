using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PhotonCannon SkillData", menuName = "ScriptableObjects/SkillData/PhotonCannon", order = 0)]
public class PhotonCannonSkillData : PlayerSkillData
{
    [SerializeField] private DamageProjectile _prefab;
    [SerializeField] private ParticleSystem _compressParticle;
    [SerializeField] private AudioClip _shootSound;
    [SerializeField] private float _shootSoundVolume = 1f;
    [SerializeField] private float _shootSoundPitch = 1f;
    [SerializeField] private AudioClip _chargeSound;
    [SerializeField] private float _chargeSoundVolume = 1f;
    [SerializeField] private float _chargeSoundPitch = 1f;

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

    [Header("Charge")]
    [SerializeField] private float _chargeTime;
    [SerializeField] private float _minSize, _maxSize;
    [SerializeField] private float _minDamageRate;

    public override float GetCooldown(Player p, PlayerSkill skill)
    {
        return Mathf.Max(_minCooldown, _baseCooldown - (skill.Level - 1) * _cooldownShrink);
    }

    private float GetChargeRate(PlayerSkill skill)
    {
        return Mathf.Clamp01(skill.ChargeTime / _chargeTime);
    }

    private float GetDamageRate(PlayerSkill skill)
    {
        return GetChargeRate(skill) * (1f - _minDamageRate) + _minDamageRate;
    }

    private float GetSize(PlayerSkill skill)
    {
        return GetChargeRate(skill) * (_maxSize - _minSize) + _minSize;
    }

    public override string GetDescription(Player p, PlayerSkill skill)
    {
        var attackParams = GetProjectileParams(p, skill);
        return $"광양자포를 충전하여 전방으로 발사합니다. 충전 시간에 비례해 피해량과 크기가 커집니다.\n" +
            $"피해량은 최소 {StringUtil.MagicalValue(attackParams.Damage * _minDamageRate)} ~ " +
            $"최대 {StringUtil.MagicalValue(attackParams.Damage)}의 마법 피해를 입힙니다.";
    }

    public AttackParams GetProjectileParams(Player p, PlayerSkill skill)
    {
        return p.Stat.GetMagicalAttackParams(_baseDamage + (skill.Level - 1) * _damageGrowth +
            p.Stat.Get(StatType.MagicForce) * _damageMagicForceCoefficient);
    }

    public DamageProjectile GetProjectile(Player p, PlayerSkill skill)
    {
        var projectile = Instantiate(_prefab);
        projectile.AttackParams = GetProjectileParams(p, skill);
        return projectile;
    }

    public override void OnStartCharge(Player p, PlayerSkill skill)
    {
        if (p.IsSelf) GameManager.Instance.UIManager.ChargeBar.Show(Name, Color.white);
        var projectile = GetProjectile(p, skill);
        projectile.transform.position = p.PlayerRenderer.transform.position + p.PlayerRenderer.transform.right * 1.3f;
        var particle = Instantiate(_compressParticle, projectile.transform);
        skill.SetData("speed", projectile.Speed);
        skill.SetData("maxDamage", projectile.AttackParams.Damage);
        skill.SetData("projectile", projectile);
        skill.SetData("particle", particle);
        skill.SetData("sfxController", SoundManager.Instance.PlayLoopSFX(
            _chargeSound, p.transform.position,
            _chargeSoundVolume, _chargeSoundPitch, p.transform
            ));
    }

    public override void OnCharging(Player p, PlayerSkill skill)
    {
        var progress = GetChargeRate(skill);
        if (p.IsSelf) GameManager.Instance.UIManager.ChargeBar.Progress = progress;
        var projectile = skill.GetData<DamageProjectile>("projectile");
        var maxDamage = skill.GetData("maxDamage", 0f);
        if (projectile == null) return;


        var chargeSFXController = skill.GetData<SFXController>("sfxController");
        if(chargeSFXController != null)
        {
            chargeSFXController.Volume = progress * 0.8f + 0.6f;
        }


        projectile.transform.position = p.PlayerRenderer.transform.position + p.PlayerRenderer.transform.right * 1.3f;
        projectile.Direction = p.PlayerRenderer.transform.right;
        var attackParams = projectile.AttackParams;
        attackParams.Damage = maxDamage * GetDamageRate(skill);
        projectile.AttackParams = attackParams;
        projectile.LiveTime = projectile.MaxLiveTime;
        projectile.Owner = p;
        projectile.Speed = 0f;
        projectile.IsTriggerable = false;
        projectile.transform.localScale = Vector2.one * GetSize(skill);
    }

    public override void OnActiveUse(Player p, PlayerSkill skill)
    {
        if (p.IsSelf) GameManager.Instance.UIManager.ChargeBar.Hide();
        var projectile = skill.GetData<DamageProjectile>("projectile");
        var particle = skill.GetData<ParticleSystem>("particle");
        var speed = skill.GetData("speed", 0f);
        var chargeSFXController = skill.GetData<SFXController>("sfxController");
        if (projectile == null) return;
        if (particle != null) Destroy(particle.gameObject);

        chargeSFXController.Stop();
        SoundManager.Instance.PlaySFX(_shootSound, p.transform.position, _shootSoundVolume, _shootSoundPitch);
        projectile.Speed = speed;
        projectile.IsTriggerable = true;
    }

    public override float GetManaCost(Player p, PlayerSkill skill)
    {
        return _baseManaCost + (skill.Level - 1) * _manaCostGrowth;
    }
}
