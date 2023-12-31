﻿using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Heal SkillData", menuName = "ScriptableObjects/SkillData/Heal", order = 0)]
public class HealSkillData : PlayerSkillData
{
    [SerializeField] private ParticleSystem _healParticle;
    [SerializeField] private AudioClip _healSound;
    [SerializeField] private float _healSoundVolume = 1f;
    [SerializeField] private float _healSoundPitch = 1f;

    [Header("Heal")]
    [SerializeField] private float _baseHealAmount;
    [SerializeField] private float _healMagicForceCoefficient;
    [SerializeField] private float _healGrowth;
    [SerializeField] private float _moveSpeedAddPercentage;
    [SerializeField] private float _speedUpTime;

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

    public float GetHealAmount(Player p, PlayerSkill skill)
    {
        return _baseHealAmount + (skill.Level - 1) * _healGrowth + p.Stat.Get(StatType.MagicForce) * _healMagicForceCoefficient;
    }

    public override string GetDescription(Player p, PlayerSkill skill)
    {
        return $"체력을 {StringUtil.HealValue(GetHealAmount(p, skill))}만큼 회복합니다.\n" +
            $"회복 후 {_speedUpTime:0.0}초 동안 {StatType.MoveSpeed.DisplayName}(이)가 증가합니다.";
    }

    public override void OnPassiveUpdate(Player p, PlayerSkill skill)
    {
        var timer = skill.GetData<float>("speedUpTimer", 0f);
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            skill.SetData("speedUpTimer", timer);
            p.Stat.Multiply(StatType.MoveSpeed, 1 + _moveSpeedAddPercentage / 100f);
        }
    }

    public override void OnActiveUse(Player p, PlayerSkill skill)
    {
        skill.SetData("speedUpTimer", _speedUpTime);
        SoundManager.Instance.PlaySFX(_healSound, p.transform.position, _healSoundVolume, _healSoundPitch);
        p.HP += GetHealAmount(p, skill);
        ParticleManager.SpawnParticle(_healParticle, p.PlayerRenderer.transform);
    }

    public override float GetManaCost(Player p, PlayerSkill skill)
    {
        return _baseManaCost + (skill.Level - 1) * _manaCostGrowth;
    }
}
