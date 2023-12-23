using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerSkillData : ObtainableObject
{
    public virtual void OnStartCharge(Player p, PlayerSkill skill) { }
    public virtual void OnCharging(Player p, PlayerSkill skill) { }
    public virtual void OnPassiveUpdate(Player p, PlayerSkill skill) { }
    public virtual void OnObtain(Player p, PlayerSkill skill) { }
    public virtual void OnReplace(Player p, PlayerSkill skill) { }
    public abstract void OnActiveUse(Player p, PlayerSkill skill);
    public abstract string GetDescription(Player p, PlayerSkill skill);
    public abstract float GetCooldown(Player p, PlayerSkill skill);
    public abstract float GetManaCost(Player p, PlayerSkill skill);
}