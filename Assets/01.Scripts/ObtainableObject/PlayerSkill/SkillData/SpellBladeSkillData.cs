using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "SpellBlade SkillData", menuName = "ScriptableObjects/SkillData/SpellBlade", order = 0)]
public class SpellBladeSkillData : PlayerSkillData
{
    [SerializeField] private DamageProjectile _prefab;
    [SerializeField] private float _bladeDistance;

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

    [Header("Stun")]
    [SerializeField] private float _stunTime;

    [Header("MoveSpeed Stack")]
    [SerializeField] private float _stackDuration;
    [SerializeField] private float _moveSpeedAddition;

    public override float GetCooldown(Player p, PlayerSkill skill)
    {
        return Mathf.Max(_minCooldown, _baseCooldown - (skill.Level - 1) * _cooldownShrink);
    }

    public override string GetDescription(Player p, PlayerSkill skill)
    {
        var attackParams = GetProjectileParams(p, skill);
        return $"기본 지속 효과 : 플레이어를 적중시킬 때 마다 {_stackDuration:0.0}초 간 지속되는 스택이 1 쌓이며,\n" +
            $"스택 하나 당 {StatType.MoveSpeed.DisplayName}(이)가 {_moveSpeedAddition} 만큼 증가합니다.\n\n" +
            $"전방에 마법의 검을 생성해 원형으로 휘두릅니다.\n" +
            $"적중한 적에게 {StringUtil.PhysicalValue(attackParams.Damage)}의 물리 피해를 입히고, " +
            $"{_stunTime:0.0}초 동안 기절시킵니다.";
    }

    public AttackParams GetProjectileParams(Player p, PlayerSkill skill)
    {
        return p.Stat.GetPhysicalAttackParams(_baseDamage + (skill.Level - 1) * _damageGrowth +
            p.Stat.Get(StatType.Attack) * _damageAttackCoefficient);
    }

    public DamageProjectile GetProjectile(Player p, PlayerSkill skill)
    {
        var projectile = Instantiate(_prefab);
        projectile.AttackParams = GetProjectileParams(p, skill);
        return projectile;
    }

    public override void OnPassiveUpdate(Player p, PlayerSkill skill)
    {
        var stackCount = skill.GetData("stack", 0);
        p.Stat.Add(StatType.MoveSpeed, stackCount * _moveSpeedAddition);
    }

    public override void OnActiveUse(Player p, PlayerSkill skill)
    {
        var projectile = GetProjectile(p, skill);
        projectile.Owner = p;
        projectile.PenetrateCount = -1;
        projectile.Speed = 0f;

        SwingTask(projectile, p, skill).Forget();
    }

    private async UniTask SwingTask(DamageProjectile projectile, Player p, PlayerSkill skill)
    {
        projectile.RegisterCollisionEvent(damageable =>
        {
            if (damageable is Player other)
            {
                StackTask(skill).Forget();
                other.AddEffect(new Effect(EffectType.Stun, 1, _stunTime, p));
            }
        });
        var originalZ = p.PlayerRenderer.transform.eulerAngles.z;
        while (projectile.LiveTime > 0f)
        {
            var angle = Mathf.Clamp01(projectile.LiveTime / projectile.MaxLiveTime) * 360f + originalZ;

            projectile.Angle = angle;
            projectile.transform.position = p.PlayerRenderer.transform.position + projectile.transform.right * _bladeDistance;
            await UniTask.Yield();
        }
    }

    private async UniTask StackTask(PlayerSkill skill)
    {

        skill.SetData("stack", skill.GetData("stack", 0) + 1);
        await UniTask.Delay(TimeSpan.FromSeconds(_stackDuration));
        skill.SetData("stack", skill.GetData("stack", 0) - 1);
    }

    public override float GetManaCost(Player p, PlayerSkill skill)
    {
        return _baseManaCost + (skill.Level - 1) * _manaCostGrowth;
    }
}
