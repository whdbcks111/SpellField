using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageProjectile : Projectile
{
    [HideInInspector] public AttackParams AttackParams;
    private Action<Damageable> OnCollisionEvent = _ => { };
    private Action<Damageable> OnPreCollisionEvent = _ => { };

    public void RegisterCollisionEvent(Action<Damageable> eventAction)
    {
        OnCollisionEvent += eventAction;
    }
    public void RegisterPreCollisionEvent(Action<Damageable> eventAction)
    {
        OnPreCollisionEvent += eventAction;
    }

    protected override void OnCollision(Damageable damageable)
    {
        OnPreCollisionEvent?.Invoke(damageable);
        damageable.Damage(AttackParams, Owner);
        OnCollisionEvent?.Invoke(damageable);
    }
}
