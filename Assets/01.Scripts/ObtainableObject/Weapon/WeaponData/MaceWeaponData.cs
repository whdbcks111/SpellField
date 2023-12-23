using UnityEngine;

[CreateAssetMenu(fileName = "New Mace Data", menuName = "ScriptableObjects/WeaponData/Mace", order = 0)]
public class MaceWeaponData : MeleeAttackWeaponData
{
    [SerializeField] private int _knockback;

    public override void OnDamage(Player self, Weapon weapon, Damageable damageable)
    {
        base.OnDamage(self, weapon, damageable);

        if(damageable is Player other)
        {
            other.Knockback((other.PlayerRenderer.transform.position - 
                self.PlayerRenderer.transform.position).normalized * _knockback);
        }
    }
}