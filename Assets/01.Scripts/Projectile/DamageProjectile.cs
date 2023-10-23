using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageProjectile : Projectile
{
    [HideInInspector] public AttackParams AttackParams;
    private Action<Damageable> OnCollisionEvent = _ => { };

    public void RegisterCollisionEvent(Action<Damageable> eventAction) {
        OnCollisionEvent += eventAction;
    }

    protected override void OnCollision(Damageable damageable)
    {
        damageable.Damage(AttackParams);
        OnCollisionEvent?.Invoke(damageable);
    }
}
