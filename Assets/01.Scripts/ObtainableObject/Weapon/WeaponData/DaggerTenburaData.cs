using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

[CreateAssetMenu(fileName = "New Dagger Shooter Data", menuName = "ScriptableObjects/WeaponData/DaggerShooter", order = 0)]
public class DaggerTenburaData : WeaponData
{
    [SerializeField] private AudioClip _spawnSound;
    [SerializeField] private float _spawnSoundVolume = 1f;
    [SerializeField] private float _spawnSoundPitch = 1f;

    [SerializeField] private AudioClip _activeSound;
    [SerializeField] private float _activeSoundVolume = 1f;
    [SerializeField] private float _activeSoundPitch = 1f;

    [SerializeField] private DaggerTenburaProjectile _projectilePrefab;
    [SerializeField] private float _baseDamage, _attackCoefficient;
    [SerializeField] private int _maxProjectileCount;

    public override void OnMount(Player p, Weapon weapon)
    {
        base.OnMount(p, weapon);
        weapon.SetData("list", new List<DaggerTenburaProjectile>());
    }

    public override void OnUnmount(Player p, Weapon weapon)
    {
        base.OnUnmount(p, weapon);
        var list = weapon.GetData<List<DaggerTenburaProjectile>>("list");
        foreach (var projectile in list)
        {
            Destroy(projectile.gameObject);
        }
    }

    protected override void OnUse(Player p, Weapon weapon)
    {
        base.OnUse(p, weapon);

        var list = weapon.GetData<List<DaggerTenburaProjectile>>("list");

        var dagger = Instantiate(_projectilePrefab, p.PlayerRenderer.transform.position, Quaternion.identity);
        dagger.AttackParams = p.Stat.GetPhysicalAttackParams(p.Stat.Get(StatType.Attack) * _attackCoefficient + _baseDamage);
        dagger.Direction = p.PlayerRenderer.transform.right;
        dagger.Owner = p;

        if (list.Count < _maxProjectileCount)
        {
            dagger.IsWaiting = true;
            list.Add(dagger);
            weapon.CurrentCooldown *= 0.3f;
            SoundManager.Instance.PlaySFX(_spawnSound, p.transform.position, _spawnSoundVolume, _spawnSoundPitch);
        }
        else
        {
            SoundManager.Instance.PlaySFX(_activeSound, p.transform.position, _activeSoundVolume, _activeSoundPitch);
            dagger.IsWaiting = false;
            foreach(var projectile in list)
            {
                projectile.IsWaiting = false;
                projectile.Target = dagger.transform;
            }
            list.Clear();
        }
    }
}