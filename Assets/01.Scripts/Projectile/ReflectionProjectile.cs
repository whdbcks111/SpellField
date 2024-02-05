using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectionProjectile : DamageProjectile
{

    protected override void OnCollision(Damageable damageable)
    {
        base.OnCollision(damageable);
        Angle += 180 + UnityEngine.Random.Range(-45f, 45f);
    }
}
