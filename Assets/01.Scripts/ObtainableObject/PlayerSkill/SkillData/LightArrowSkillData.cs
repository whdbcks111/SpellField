using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "LightArrow SkillData", menuName = "ScriptableObjects/SkillData/LightArrow", order = 0)]
public class LightArrowSkillData : PlayerSkillData
{
    [SerializeField] private DamageProjectile _prefab;

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

    [Header("Shoot Count")]
    [SerializeField] private int _baseShootCount;
    [SerializeField] private float _shootCountGrowth;

    public override float GetCooldown(Player p, PlayerSkill skill)
    {
        return Mathf.Max(_minCooldown, _baseCooldown - (skill.Level - 1) * _cooldownShrink);
    }

    public int GetShootCount(Player p, PlayerSkill skill)
    {
        return (int)(_baseShootCount + (skill.Level - 1) * _shootCountGrowth);
    }

    public override string GetDescription(Player p, PlayerSkill skill)
    {
        var attackParams = GetProjectileParams(p, skill);
        return $"빛나는 화살을 바라보는 방향으로 짧은 간격으로 {GetShootCount(p, skill)}번 연사합니다.\n" +
            $"화살은 각각 {StringUtil.MagicalValue(attackParams.Damage)}의 마법 피해를 입힙니다.";
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
        return projectile;
    }

    public override void OnPassiveUpdate(Player p, PlayerSkill skill)
    {

    }

    public override void OnActiveUse(Player p, PlayerSkill skill)
    {
        Shoot(p, skill).Forget();
    }

    private async UniTask Shoot(Player p, PlayerSkill skill)
    {
        var count = GetShootCount(p, skill);
        for(int i = 0; i < count; i++)
        {
            Projectile.Shoot(() => GetProjectile(p, skill), p,
                p.PlayerRenderer.transform.position,
                p.PlayerRenderer.transform.eulerAngles.z,
                1, 1, 0, 0f, 1f);
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
        }
    }

    public override float GetManaCost(Player p, PlayerSkill skill)
    {
        return _baseManaCost + (skill.Level - 1) * _manaCostGrowth;
    }
}
