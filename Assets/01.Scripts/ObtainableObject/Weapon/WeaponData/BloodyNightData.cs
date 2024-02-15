using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

[CreateAssetMenu(fileName = "Bloody Night", menuName = "ScriptableObjects/WeaponData/BloodyNight", order = 0)]
public class BloodyNightData : MeleeAttackWeaponData
{
    [Header("Projectile Settings")]
    [SerializeField] private DamageProjectile _projectilePrefab;
    [SerializeField] private bool _isMagicAttack;
    [SerializeField] private float _baseDamage, _statMultiply;
    [SerializeField] private int _shootCount = 1, _angleCount = 1;
    [SerializeField] private float _angleSpan = 0f, _shootTimeSpan = 0f;
    [SerializeField] private float _hpAbsorbPercentage = 30;
    [SerializeField] private float _shootCooldown = 5;

    [SerializeField] private AudioClip _activeSound;
    [SerializeField] private float _activeSoundVolume = 1f;
    [SerializeField] private float _activeSoundPitch = 1f;

    protected virtual DamageProjectile GetProjectile(Player p, Weapon weapon)
    {
        var projectile = Instantiate(_projectilePrefab);
        projectile.AttackParams = _isMagicAttack ?
            p.Stat.GetMagicalAttackParams(_baseDamage + p.Stat.Get(StatType.MagicForce) * _statMultiply) :
            p.Stat.GetPhysicalAttackParams(_baseDamage + p.Stat.Get(StatType.Attack) * _statMultiply);
        projectile.RegisterPreCollisionEvent(damageable =>
        {
            if(damageable is Player)
                damageable.AddDamageMiddleware(damageParams =>
                {
                    p.HP += damageParams.TotalDamage * _hpAbsorbPercentage / 100f;
                    return damageParams;
                }, true);
        });
        return projectile;
    }

    protected virtual void Shoot(Player p, Weapon weapon)
    {
        ShootTask(p, weapon).Forget();
    }

    private async UniTask ShootTask(Player p, Weapon weapon)
    {
        for (int i = 0; i < _shootCount; i++)
        {
            SoundManager.Instance.PlaySFX(_activeSound, p.transform.position, _activeSoundVolume, _activeSoundPitch);
            Projectile.Shoot(() => GetProjectile(p, weapon), p,
                weapon.Display.transform.position,
                p.PlayerRenderer.transform.eulerAngles.z, _angleCount, _angleSpan);
            await UniTask.Delay(TimeSpan.FromSeconds(_shootTimeSpan));
        }
    }

    public override void OnMount(Player p, Weapon weapon)
    {
        base.OnMount(p, weapon);
        weapon.SetData("shootTimer", 0f);
    }

    protected override void OnUse(Player p, Weapon weapon)
    {
        base.OnUse(p, weapon);
        if(weapon.GetData("shootTimer", 0f) <= 0f)
        {
            Shoot(p, weapon);
            weapon.SetData("shootTimer", _shootCooldown);
        }
    }

    protected override void OnUpdate(Player p, Weapon weapon)
    {
        base.OnUpdate(p, weapon);
        var timer = weapon.GetData("shootTimer", 0f);
        if (timer > 0f) 
            weapon.SetData("shootTimer", timer - Time.deltaTime);
    }
}