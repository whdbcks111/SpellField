using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Blink SkillData", menuName = "ScriptableObjects/SkillData/Blink", order = 0)]
public class BlinkSkillData : PlayerSkillData
{
    [SerializeField] private ParticleSystem _blinkParticle;
    [SerializeField] private TrailRenderer _blinkTrail;

    [Header("Blink")]
    [SerializeField] private float _baseMoveLength;
    [SerializeField] private float _lengthGrowth;
    [SerializeField] private float _maxMoveLength;
    [SerializeField] private float _moveSpeedDownPercentage;
    [SerializeField] private float _speedDownTime;

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

    public float GetMoveLength(PlayerSkill skill)
    {
        return Mathf.Clamp(_baseMoveLength + (skill.Level - 1) * _lengthGrowth, 0f, _maxMoveLength);
    }

    public override string GetDescription(Player p, PlayerSkill skill)
    {
        return $"전방으로 {GetMoveLength(skill):0.0}만큼 순간이동합니다.\n" +
            $"순간이동 후 {_speedDownTime:0.0}초 동안 {StatType.MoveSpeed.DisplayName}(이)가 감소합니다.";
    }

    public override void OnPassiveUpdate(Player p, PlayerSkill skill)
    {
        var timer = skill.GetData("speedDownTimer", 0f);
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            skill.SetData("speedDownTimer", timer);
            p.Stat.Multiply(StatType.MoveSpeed, 1 - _moveSpeedDownPercentage / 100f);
        }
    }

    public override void OnActiveUse(Player p, PlayerSkill skill)
    {
        skill.SetData("speedDownTimer", _speedDownTime);
        ParticleManager.SpawnParticle(_blinkParticle, p.PlayerRenderer.transform);
        BlinkTask(p, skill).Forget();
    }

    private async UniTask BlinkTask(Player p, PlayerSkill skill)
    {
        var trail = Instantiate(_blinkTrail, p.PlayerRenderer.transform.position, Quaternion.identity, p.PlayerRenderer.transform);
        await UniTask.Yield();
        if (p.IsSelf) p.transform.position += p.PlayerRenderer.transform.right * GetMoveLength(skill);
        await UniTask.Delay(TimeSpan.FromSeconds(0.4f));
        Destroy(trail.gameObject);
    }

    public override float GetManaCost(Player p, PlayerSkill skill)
    {
        return _baseManaCost + (skill.Level - 1) * _manaCostGrowth;
    }
}
