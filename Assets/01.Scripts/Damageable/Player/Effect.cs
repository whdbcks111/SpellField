using System.Collections;
using System.Collections.Generic;
using System;

public class Effect
{
    public readonly EffectType Type;
    public readonly int Level;
    public float Duration;
    public readonly float MaxDuration;
    public readonly Player Caster;

    public Effect(EffectType type, int level, float duration, Player caster = null)
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

        },
        onUpdate: (p, eff) =>
        {
            
        },
        onFinish: (p, eff) =>
        {

        });
}