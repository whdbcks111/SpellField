using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Stealth SkillData", menuName = "ScriptableObjects/SkillData/Stealth", order = 0)]
public class StealthSkillData : PlayerSkillData
{
    [SerializeField] private ParticleSystem _stealthParticle;
    [SerializeField] private AudioClip _activeSound;
    [SerializeField] private float _activeSoundVolume = 1f;
    [SerializeField] private float _activeSoundPitch = 1f;

    [Header("Stealth")]
    [SerializeField] private float _stealthTime;
    [SerializeField] private float _stealthTimeGrowth;
    [SerializeField] private float _speedUpTime;
    [SerializeField] private float _moveSpeedUpPercentage;

    [Header("Mana Cost")]
    [SerializeField] private float _baseManaCost;
    [SerializeField] private float _manaCostGrowth;

    [Header("Cooldown")]
    [SerializeField] private float _baseCooldown;
    [SerializeField] private float _cooldownShrink;
    [SerializeField] private float _minCooldown;

    public override float GetCooldown(Player p, PlayerSkill skill)
    {
        return Mathf.Max(_minCooldown, _baseCooldown - (skill.Level - 1) * _cooldownShrink);
    }

    public float GetStealthTime(PlayerSkill skill)
    {
        return _stealthTime + _stealthTimeGrowth * (skill.Level - 1);
    }

    public override string GetDescription(Player p, PlayerSkill skill)
    {
        return $"{_stealthTime:0.0}초 동안 투명화 상태로 변합니다.\n" +
            $"스킬 사용 후 {_speedUpTime:0.0}초 동안 {StatType.MoveSpeed.DisplayName}(이)가 증가합니다.\n" +
            $"투명화 상태 중에는 재사용 대기시간이 감소하지 않습니다.";
    }

    public override void OnPassiveUpdate(Player p, PlayerSkill skill)
    {
        var timer = skill.GetData("speedUpTimer", 0f);
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            skill.SetData("speedUpTimer", timer);
            p.Stat.Multiply(StatType.MoveSpeed, 1 + _moveSpeedUpPercentage / 100f);
        }

        if(skill.CurrentCooldown > 0f && p.HasEffect(EffectType.Invisibility))
        {
            skill.CurrentCooldown = skill.Data.GetCooldown(p, skill);
        }
    }

    public override void OnActiveUse(Player p, PlayerSkill skill)
    {
        SoundManager.Instance.PlaySFX(_activeSound, p.transform.position, _activeSoundVolume, _activeSoundPitch);
        skill.SetData("speedUpTimer", _speedUpTime);
        ParticleManager.SpawnParticle(_stealthParticle, p.PlayerRenderer.transform);
        p.AddEffect(new Effect(EffectType.Invisibility, 1, GetStealthTime(skill), p));
    }

    public override float GetManaCost(Player p, PlayerSkill skill)
    {
        return _baseManaCost + (skill.Level - 1) * _manaCostGrowth;
    }
}
