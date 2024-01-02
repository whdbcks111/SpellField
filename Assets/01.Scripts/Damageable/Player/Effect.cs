using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Effect
{
    public readonly EffectType Type;
    public readonly int Level;
    public float Duration;
    public readonly float MaxDuration;
    public readonly Player Caster;

    public Effect(EffectType type, int level, float duration, Player caster)
    {
        Type = type;
        Level = level;
        MaxDuration = Duration = duration;
        Caster = caster;
    }
}

public class EffectType : Enumeration<EffectType>
{
    public readonly string DisplayName;
    public readonly Action<Player, Effect> OnStart, OnUpdate, OnFinish;

    public EffectType(string name, string displayName, 
        Action<Player, Effect> onStart, 
        Action<Player, Effect> onUpdate, 
        Action<Player, Effect> onFinish) : base(name)
    {
        DisplayName = displayName;
        OnStart = onStart;
        OnUpdate = onUpdate;
        OnFinish = onFinish;
    }

    public static readonly EffectType Fire = new("fire", "È­¿°",
        onStart: (p, eff) =>
        {
            p.FlameParticle.Play();
        },
        onUpdate: (p, eff) =>
        {
            p.Damage(eff.Level * 10 * Time.deltaTime);
            p.AddColorModifier("effect_fire", new Color(1f, 0.7f, 0.3f), Time.deltaTime + 0.1f);
        },
        onFinish: (p, eff) =>
        {
            p.FlameParticle.Stop();
        });

    public static readonly EffectType Blood = new("blood", "ÃâÇ÷",
        onStart: (p, eff) =>
        {
            p.BloodParticle.Play();
        },
        onUpdate: (p, eff) =>
        {
            p.Damage(eff.Level * 0.01f * p.MaxHP * Time.deltaTime);
            p.AddColorModifier("effect_blood", new Color(1f, 0.7f, 0.7f), 0.1f);
        },
        onFinish: (p, eff) =>
        {
            p.BloodParticle.Stop();
        });

    public static readonly EffectType Frozen = new("frozen", "ºù°á",
        onStart: (p, eff) =>
        {

        },
        onUpdate: (p, eff) =>
        {
            p.Stat.Multiply(StatType.MoveSpeed, Mathf.Pow(0.9f, eff.Level));
            p.AddColorModifier("effect_frozen", new Color(0.5f, 0.9f, 1f), 0.1f);
        },
        onFinish: (p, eff) =>
        {

        });

    public static readonly EffectType Stun = new("stun", "±âÀý",
        onStart: (p, eff) =>
        {

        },
        onUpdate: (p, eff) =>
        {
            p.AddState(PlayerState.CannotMove, 0.1f);
            p.AddState(PlayerState.CannotUseSkill, 0.1f);
            p.AddState(PlayerState.CannotUseWeapon, 0.1f);
            p.AddColorModifier("effect_stun", new Color(0.8f, 0.8f, 0.8f), 0.1f);
        },
        onFinish: (p, eff) =>
        {

        });
    public static readonly EffectType Invisibility = new("invisibility", "Åõ¸íÈ­",
        onStart: (p, eff) =>
        {

        },
        onUpdate: (p, eff) =>
        {
            p.AddState(PlayerState.Invisible, 0.1f);
        },
        onFinish: (p, eff) =>
        {

        });
    public static readonly EffectType Enhance = new("enhance", "°­È­",
        onStart: (p, eff) =>
        {

        },
        onUpdate: (p, eff) =>
        {
            var col1 = new Color(0.6f, 1f, 1f, 0.6f);
            var col2 = new Color(1f, 1f, 0.5f, 0.6f);
            p.AddTintColorModifier("eff_strength", Color.Lerp(col1, col2, Mathf.Sin(Time.time * Mathf.PI / 0.35f) * 0.5f + 0.5f), 0.1f);
            p.Stat.Multiply(StatType.Attack, 1 + eff.Level * 0.2f);
            p.Stat.Multiply(StatType.MagicForce, 1 + eff.Level * 0.2f);
            p.Stat.Add(StatType.Attack, eff.Level * 10f);
            p.Stat.Add(StatType.MagicForce, eff.Level * 10f);
            p.Stat.Multiply(StatType.MoveSpeed, 1 + eff.Level * 0.2f);
        },
        onFinish: (p, eff) =>
        {

        });
}