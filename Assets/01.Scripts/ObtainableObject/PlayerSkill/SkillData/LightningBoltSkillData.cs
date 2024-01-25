using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Lightning Bolt", menuName = "ScriptableObjects/SkillData/LightningBolt", order = 0)]
public class LightningBoltSkillData : PlayerSkillData
{
    public const string RandomKey = "lightning_bolt";

    [SerializeField] private DamageProjectile _prefab;
    [SerializeField] private ParticleSystem _compressParticle;
    [SerializeField] private AudioClip _shootSound;
    [SerializeField] private float _shootSoundVolume = 1f;
    [SerializeField] private float _shootSoundPitch = 1f;
    [SerializeField] private AudioClip _chargeSound;
    [SerializeField] private float _chargeSoundVolume = 1f;
    [SerializeField] private float _chargeSoundPitch = 1f;

    [SerializeField] private float _randomShootRange;

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
    [SerializeField] private float _baseStunTime;
    [SerializeField] private float _stunTimeGrowth;
    [SerializeField] private float _maxStunTime;

    [Header("Charge")]
    [SerializeField] private float _chargeTime;
    [SerializeField] private float _minCount, _maxCount, _maxCountGrowth;
    [SerializeField] private float _minDamageRate;

    public override float GetCooldown(Player p, PlayerSkill skill)
    {
        return Mathf.Max(_minCooldown, _baseCooldown - (skill.Level - 1) * _cooldownShrink);
    }

    private float GetChargeRate(PlayerSkill skill)
    {
        return Mathf.Clamp01(skill.ChargeTime / _chargeTime);
    }

    private float GetStunTime(PlayerSkill skill)
    {
        return Mathf.Clamp(_baseStunTime + (skill.Level - 1) * _stunTimeGrowth, 0, _maxStunTime);
    }

    private float GetDamageRate(PlayerSkill skill)
    {
        return GetChargeRate(skill) * (1f - _minDamageRate) + _minDamageRate;
    }

    private int GetCount(PlayerSkill skill)
    {
        return (int)(GetChargeRate(skill) * (_maxCount + (skill.Level - 1) * _maxCountGrowth - _minCount) + _minCount);
    }

    public override string GetDescription(Player p, PlayerSkill skill)
    {
        var attackParams = GetProjectileParams(p, skill, 1);
        return $"전기 에너지를 충전하여 전방으로 다수 발사합니다. 충전 시간에 비례해 피해량이 커지고 개수가 많아집니다.\n" +
            $"피해량은 최소 {StringUtil.MagicalValue(attackParams.Damage * _minDamageRate)} ~ " +
            $"최대 {StringUtil.MagicalValue(attackParams.Damage)}의 마법 피해를 입힙니다.\n" +
            $"최대로 충전했을 경우 모든 투사체에 {GetStunTime(skill)}초의 기절 효과가 적용됩니다.";
    }

    public AttackParams GetProjectileParams(Player p, PlayerSkill skill, float rate)
    {
        return p.Stat.GetMagicalAttackParams((_baseDamage + (skill.Level - 1) * _damageGrowth +
            p.Stat.Get(StatType.MagicForce) * _damageMagicForceCoefficient) * rate);
    }

    public DamageProjectile GetProjectile(Player p, PlayerSkill skill)
    {
        var projectile = Instantiate(_prefab);
        projectile.AttackParams = GetProjectileParams(p, skill, GetDamageRate(skill));
        if(GetChargeRate(skill) > 0.999f)
        {
            projectile.RegisterCollisionEvent(damageable =>
            {
                if(damageable is Player other)
                    other.AddEffect(new Effect(EffectType.Stun, 1, GetStunTime(skill), p));
            });
        }
        return projectile;
    }

    public override void OnStartCharge(Player p, PlayerSkill skill)
    {
        if (p.IsSelf) GameManager.Instance.UIManager.ChargeBar.Show(Name, Color.white);
        var particle = Instantiate(_compressParticle, 
            p.PlayerRenderer.transform.position + p.PlayerRenderer.transform.right * 1.3f, 
            Quaternion.identity);
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
        var particle = skill.GetData<ParticleSystem>("particle", null);
        if(particle == null) return;

        var chargeSFXController = skill.GetData<SFXController>("sfxController");
        if(chargeSFXController != null)
        {
            chargeSFXController.Volume = progress * 0.8f + 0.6f;
        }

        particle.transform.position = p.PlayerRenderer.transform.position + p.PlayerRenderer.transform.right * 1.3f;
    }

    public override void OnActiveUse(Player p, PlayerSkill skill)
    {
        if (p.IsSelf) GameManager.Instance.UIManager.ChargeBar.Hide();
        var particle = skill.GetData<ParticleSystem>("particle");
        var chargeSFXController = skill.GetData<SFXController>("sfxController");
        if (particle != null) Destroy(particle.gameObject);

        chargeSFXController.Stop();
        Shoot(p, skill).Forget();
    }
    private async UniTask Shoot(Player p, PlayerSkill skill)
    {
        var directionZ = p.PlayerRenderer.transform.eulerAngles.z;
        var count = GetCount(skill);
        for (int i = 0; i < count; i++)
        {
            Projectile.Shoot(() => GetProjectile(p, skill), p,
                p.PlayerRenderer.transform.position,
                directionZ + GameManager.Instance.GetSeedRandomRange(RandomKey, -_randomShootRange, _randomShootRange),
                1, 1, 0, 0f, 1f);
            SoundManager.Instance.PlaySFX(_shootSound, p.transform.position, _shootSoundVolume, _shootSoundPitch);
            await UniTask.Delay(TimeSpan.FromSeconds(0.05f));
        }
    }

    public override float GetManaCost(Player p, PlayerSkill skill)
    {
        return _baseManaCost + (skill.Level - 1) * _manaCostGrowth;
    }
}
