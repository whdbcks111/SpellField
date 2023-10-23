using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponData : ObtainableObject
{
    [TextArea] public string Description;
    public WeaponDisplay DisplayObject;
    public float Cooldown;
    public StatModifier[] StatModifiers;

    public void UpdateWeapon(Player p, Weapon weapon)
    {
        foreach(var modifier in StatModifiers)
        {
            modifier.Apply(p.Stat);
        }

        OnUpdate(p, weapon);
    }

    public void Use(Player p, Weapon weapon)
    {
        OnUse(p, weapon);
    }

    public virtual void OnWeaponTriggerStay(Collider2D collider, Player self, Weapon weapon) { }
    public virtual void OnMount(Player p, Weapon weapon) { }
    public virtual void OnUnmount(Player p, Weapon weapon) { }
    protected virtual void OnUse(Player p, Weapon weapon) { }
    protected virtual void OnUpdate(Player p, Weapon weapon) { }
}
