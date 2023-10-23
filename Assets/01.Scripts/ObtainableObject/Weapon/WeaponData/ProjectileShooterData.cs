using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ProjectileShooterData", menuName = "ScriptableObjects/WeaponData/ProjectileShooter", order = 0)]
public class ProjectileShooterData : WeaponData
{
    [SerializeField] private DamageProjectile _projectilePrefab;
    [SerializeField] private bool _isMagicAttack;
    [SerializeField] private float _baseDamage, _statMultiply;

    protected virtual DamageProjectile GetProjectile(Player p, Weapon weapon)
    {
        var projectile = Instantiate(_projectilePrefab);
        projectile.AttackParams = _isMagicAttack ?
            p.Stat.GetMagicalAttackParams(_baseDamage + p.Stat.Get(StatType.MagicForce) * _statMultiply) :
            p.Stat.GetPhysicalAttackParams(_baseDamage + p.Stat.Get(StatType.Attack) * _statMultiply);
        return projectile;
    }

    protected virtual void Shoot(Player p, Weapon weapon)
    {
        Projectile.Shoot(() => GetProjectile(p, weapon), p, p.PlayerRenderer.transform.position, 
            p.PlayerRenderer.transform.eulerAngles.z, 1, 1, 0, 0, 1.2f);
    }

    protected override void OnUse(Player p, Weapon weapon)
    {
        base.OnUse(p, weapon);

        Shoot(p, weapon);
    }
}