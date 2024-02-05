using Cysharp.Threading.Tasks;
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
    [SerializeField] private int _shootCount = 1, _angleCount = 1;
    [SerializeField] private float _angleSpan = 0f, _shootTimeSpan = 0f;

    [SerializeField] private AudioClip _activeSound;
    [SerializeField] private float _activeSoundVolume = 1f;
    [SerializeField] private float _activeSoundPitch = 1f;

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
        ShootTask(p, weapon).Forget();
    }

    private async UniTask ShootTask(Player p, Weapon weapon)
    {
        for(int i = 0; i < _shootCount; i++)
        {
            SoundManager.Instance.PlaySFX(_activeSound, p.transform.position, _activeSoundVolume, _activeSoundPitch);
            Projectile.Shoot(() => GetProjectile(p, weapon), p,
                (weapon.Display is ProjectileShooterDisplay psData ? psData.ShootPoint.position : p.PlayerRenderer.transform.position),
                p.PlayerRenderer.transform.eulerAngles.z, _angleCount, _angleSpan);
            await UniTask.Delay(TimeSpan.FromSeconds(_shootTimeSpan));
        }
    }

    protected override void OnUse(Player p, Weapon weapon)
    {
        base.OnUse(p, weapon);

        Shoot(p, weapon);
    }
}