using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New MeleeAttackWeaponData", menuName = "ScriptableObjects/WeaponData/Melee", order = 0)]
public class MeleeAttackWeaponData : WeaponData
{

    [SerializeField] private float _swingRotation = 30f;

    public override void OnMount(Player p, Weapon weapon)
    {
        base.OnMount(p, weapon);
        weapon.SetData("attackedObjects", new HashSet<Damageable>());
        weapon.SetData("isAttacking", false);
    }

    protected override void OnUpdate(Player p, Weapon weapon)
    {
        base.OnUpdate(p, weapon);
    }

    public override void OnWeaponTriggerStay(Collider2D collider, Player self, Weapon weapon)
    {
        base.OnWeaponTriggerStay(collider, self, weapon);

        var attackedObjects = weapon.GetData<HashSet<Damageable>>("attackedObjects");
        var isAttacking = weapon.GetData<bool>("isAttacking");

        if (isAttacking && collider.TryGetComponent(out Damageable damageable) && damageable != self && 
            !attackedObjects.Contains(damageable))
        {
            attackedObjects.Add(damageable);
            damageable.Damage(self.Stat.GetPhysicalAttackParams());
        }
    }

    private async UniTask AttackTask(float time, Weapon weapon)
    {
        var isAttacking = weapon.GetData<bool>("isAttacking");
        var attackedObjects = weapon.GetData<HashSet<Damageable>>("attackedObjects");

        if (isAttacking) return;
        attackedObjects.Clear();

        weapon.SetData("isAttacking", true);
        await UniTask.Delay(TimeSpan.FromSeconds(time));
        weapon.SetData("isAttacking", false);
    }

    protected override void OnUse(Player p, Weapon weapon)
    {
        base.OnUse(p, weapon);

        var swingTime = Mathf.Clamp(weapon.Data.Cooldown * 0.6f, 0.05f, 0.15f);
        p.Hand.Swing(_swingRotation, swingTime);

        AttackTask(swingTime, weapon).Forget();
    }
}