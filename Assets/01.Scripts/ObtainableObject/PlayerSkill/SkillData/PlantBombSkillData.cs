using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PlantBomb SkillData", menuName = "ScriptableObjects/SkillData/PlantBomb", order = 0)]
public class PlantBombSkillData : PlayerSkillData
{
    [SerializeField] private BombProjectile _prefab;

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

    public override float GetCooldown(Player p, PlayerSkill skill)
    {
        return Mathf.Max(_minCooldown, _baseCooldown - (skill.Level - 1) * _cooldownShrink);
    }

    public AttackParams GetProjectileParams(Player p, PlayerSkill skill)
    {
        return p.Stat.GetMagicalAttackParams(_baseDamage + (skill.Level - 1) * _damageGrowth +
            p.Stat.Get(StatType.Attack) * _damageAttackCoefficient);
    }

    public Projectile GetProjectile(Player p, PlayerSkill skill)
    {
        var projectile = Instantiate(_prefab);
        projectile.AttackParams = GetProjectileParams(p, skill);
        return projectile;
    }

    public override string GetDescription(Player p, PlayerSkill skill)
    {
        var attackParams = GetProjectileParams(p, skill);
        return $"현재 위치에 {_prefab.LiveTime}초 간 유지되는 지뢰 폭탄을 설치합니다.\n" +
            $"지뢰는 각각 {StringUtil.PhysicalValue(attackParams.Damage)}의 물리 피해를 입힙니다.";
    }

    public override void OnActiveUse(Player p, PlayerSkill skill)
    {
        var projectile = GetProjectile(p, skill);
        projectile.transform.position = p.PlayerRenderer.transform.position;
        projectile.Owner = p;
    }

    public override float GetManaCost(Player p, PlayerSkill skill)
    {
        return _baseManaCost + (skill.Level - 1) * _manaCostGrowth;
    }

    public override void OnPassiveUpdate(Player p, PlayerSkill skill)
    {
    }
}
