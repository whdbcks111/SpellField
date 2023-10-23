using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New MeleeAttackWeaponData", menuName = "ScriptableObjects/WeaponData/Melee", order = 0)]
public class MeleeAttackWeaponData : WeaponData
{

    [SerializeField] private float _swingRotation = 30f;

    private bool _isAttacking = false;
    private readonly HashSet<Damageable> _attackedObjects = new();

    public override void OnMount(Player p, Weapon weapon)
    {
        base.OnMount(p, weapon);
    }

    protected override void OnUpdate(Player p, Weapon weapon)
    {
        base.OnUpdate(p, weapon);
    }

    public override void OnWeaponTriggerStay(Collider2D collider, Player self, Weapon weapon)
    {
        base.OnWeaponTriggerStay(collider, self, weapon);
        if(_isAttacking && collider.TryGetComponent(out Damageable damageable) && damageable != self && 
            !_attackedObjects.Contains(damageable))
        {
            _attackedObjects.Add(damageable);
            damageable.Damage(self.Stat.GetPhysicalAttackParams());
        }
    }

    private async UniTask AttackTask(float time)
    {
        if (_isAttacking) return;
        _attackedObjects.Clear();
        _isAttacking = true;
        await UniTask.Delay(TimeSpan.FromSeconds(time));
        _isAttacking = false;
    }

    protected override void OnUse(Player p, Weapon weapon)
    {
        base.OnUse(p, weapon);
        var swingTime = Mathf.Clamp(weapon.Data.Cooldown * 0.6f, 0.05f, 0.15f);
        p.Hand.Swing(_swingRotation, swingTime);

        AttackTask(swingTime).Forget();
    }
}