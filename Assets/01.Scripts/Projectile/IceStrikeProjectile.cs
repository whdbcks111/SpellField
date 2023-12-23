using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceStrikeProjectile : Projectile
{
    [SerializeField] private ParticleSystem _particle;
    public float OverlapRadius;
    [HideInInspector] public AttackParams AttackParams;
    [HideInInspector] public int EffectLevel;
    [HideInInspector] public float EffectTime;


    protected override void OnCollision(Damageable damageable)
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position, OverlapRadius);
        ParticleManager.SpawnParticle(_particle, transform.position, OverlapRadius / 3f);
        foreach (var collider in colliders)
        {
            if(collider.TryGetComponent(out Damageable d) && d != Owner)
            {
                d.Damage(AttackParams, Owner);
                if(d is Player p) p.AddEffect(new Effect(EffectType.Frozen, EffectLevel, EffectTime, Owner));
            }
        }
    }
}
