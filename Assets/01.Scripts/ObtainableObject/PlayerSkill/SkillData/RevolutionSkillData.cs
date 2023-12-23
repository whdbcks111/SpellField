using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Revolution", menuName = "ScriptableObjects/SkillData/Revolution", order = 0)]
public class RevolutionSkillData : PlayerSkillData
{
    [SerializeField] private DamageProjectile _prefab;

    [SerializeField] private int _baseProjectileCount;
    [SerializeField] private float _countGrowth;
    [SerializeField] private float _baseRevolutionSpeed;
    [SerializeField] private float _speedGrowth;
    [SerializeField] private float _distance;

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

    public int GetProjectileCount(PlayerSkill skill)
    {
        return _baseProjectileCount + (int)((skill.Level - 1) * _countGrowth);
    }

    public float GetRevolutionSpeed(PlayerSkill skill)
    {
        return _baseRevolutionSpeed + (skill.Level - 1) * _speedGrowth;
    }

    public override string GetDescription(Player p, PlayerSkill skill)
    {
        var attackParams = GetProjectileParams(p, skill);
        return $"기본 지속 효과 : 플레이어 주위로 공이 {GetRevolutionSpeed(skill):0.0}의 속도로 {GetProjectileCount(skill)}개 공전합니다.\n" +
            $"공이 다른 플레이어에게 적중 시 {StringUtil.PhysicalValue(attackParams.Damage)}의 물리 피해를 입힙니다.";
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

    public DamageProjectile GetProjectile(Player p, PlayerSkill skill)
    {
        var projectile = Instantiate(_prefab, p.PlayerRenderer.transform.position, Quaternion.identity);
        projectile.AttackParams = GetProjectileParams(p, skill);
        projectile.Owner = p;
        projectile.PenetrateCount = -1;
        return projectile;
    }

    public override void OnReplace(Player p, PlayerSkill skill)
    {

        var projectiles = skill.GetData<List<DamageProjectile>>("projectiles", null);
        if (projectiles == null) return;
        
        foreach(var projectile in projectiles)
        {
            Destroy(projectile.gameObject);
        }
    }

    public override void OnPassiveUpdate(Player p, PlayerSkill skill)
    {
        skill.CurrentCooldown = skill.Data.GetCooldown(p, skill);

        if (skill.GetData("startTime", -1f) < 0f)
        {
            skill.SetData("startTime", Time.time);
        }

        var projectiles = skill.GetData<List<DamageProjectile>>("projectiles", null);
        if (projectiles == null)
        {
            projectiles = new();
            skill.SetData("projectiles", projectiles);
        }

        while(projectiles.Count < GetProjectileCount(skill))
        {
            var pr = GetProjectile(p, skill);
            projectiles.Add(pr);
        }

        var speed = GetRevolutionSpeed(skill);
        for (int i = 0; i < projectiles.Count; i++)
        {
            var pr = projectiles[i];
            var angle = (360 / projectiles.Count) * i + (Time.time - skill.GetData("startTime", 0f)) * speed + 90f;

            if(pr == null)
            {
                projectiles.RemoveAt(i);
                i--;
                continue;
            }

            pr.Accelerate = 0f;
            pr.Speed = 0f;
            pr.LiveTime = pr.MaxLiveTime;

            pr.Angle = angle; ;

            var pos = p.PlayerRenderer.transform.position + pr.transform.right * _distance;
            pr.transform.position = pos;
        }

    }

    public override void OnActiveUse(Player p, PlayerSkill skill)
    {
    }
}
