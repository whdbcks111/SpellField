
using UnityEngine;

[CreateAssetMenu(fileName = "New NeonSpear Data", menuName = "ScriptableObjects/WeaponData/NeonSpear", order = 0)]
public class NeonSpearData : ThrowingWeaponData
{
    [Header("Blood")]
    [SerializeField] private int _bloodLevel;
    [SerializeField] private float _bloodTime;

    protected override DamageProjectile GetProjectile(Player p, Weapon weapon)
    {
        var projectile = base.GetProjectile(p, weapon);
        projectile.RegisterCollisionEvent(d =>
        {
            if (d is Player other) other.AddEffect(new Effect(EffectType.Blood, _bloodLevel, _bloodTime, p));
        });
        return projectile;
    }
}