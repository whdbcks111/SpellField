using UnityEngine;

[CreateAssetMenu(fileName = "New FlameDagger Data", menuName = "ScriptableObjects/WeaponData/FlameDagger", order = 0)]
public class FlameDaggerWeaponData : MeleeAttackWeaponData
{
    [SerializeField] private int _fireLevel;
    [SerializeField] private float _fireTime;
    [SerializeField] private float _fireChance;

    public override void OnDamage(Player self, Weapon weapon, Damageable damageable)
    {
        base.OnDamage(self, weapon, damageable);

        if(damageable is Player other && Random.value < _fireChance / 100f)
        {
            other.AddEffect(new Effect(EffectType.Fire, _fireLevel, _fireTime, self));
        }
    }
}