using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            OnDamage(self, weapon, damageable);
        }
    }

    public virtual void OnDamage(Player self, Weapon weapon, Damageable damageable)
    {
        damageable.Damage(self.Stat.GetPhysicalAttackParams(), self);
    }

    private async UniTask AttackTask(float time, Weapon weapon)
    {
        var isAttacking = weapon.GetData<bool>("isAttacking");
        var attackedObjects = weapon.GetData<HashSet<Damageable>>("attackedObjects");

        if (isAttacking) return;
        attackedObjects.Clear();

        TrailRenderer[] trails = null;
        if(weapon.Display is MeleeAttackWeaponDisplay display)
        {
            trails = new TrailRenderer[display.TrailInfos.Length];

            for (int i = 0; i < trails.Length; i++)
            {
                trails[i] = Instantiate(display.TrailInfos[i].TrailPrefab, 
                    display.TrailInfos[i].TrailPoint.position, Quaternion.identity, display.TrailInfos[i].TrailPoint);
            }
        }
        

        weapon.SetData("isAttacking", true);
        await UniTask.Delay(TimeSpan.FromSeconds(time));
        weapon.SetData("isAttacking", false);

        if (trails != null)
        {
            for (int i = 0; i < trails.Length; i++)
            {
                if (trails[i] != null)
                    trails[i].transform.SetParent(null);
            }
            await UniTask.WaitWhile(() => trails.Any(trail => trail != null && trail.isVisible));
            for (int i = 0; i < trails.Length; i++)
            {
                if (trails[i] != null)
                    Destroy(trails[i].gameObject);
            }
        }
    }

    protected override void OnUse(Player p, Weapon weapon)
    {
        base.OnUse(p, weapon);

        var swingTime = Mathf.Clamp(weapon.Data.Cooldown * 0.9f, 0.05f, 0.2f);
        p.Hand.Swing(_swingRotation, swingTime);

        AttackTask(swingTime, weapon).Forget();
    }
}