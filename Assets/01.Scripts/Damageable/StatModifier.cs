using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StatModifier
{
    [SerializeField] private StatType.WrapperType _statType;
    [SerializeField] public Type ModifyType;
    [SerializeField] public float Value;

    public StatType StatType
    {
        get { return StatType.GetByWrapperType(_statType); }
    }

    public void Apply(Stat stat)
    {
        StatType realType = StatType;
        if (ModifyType == Type.Add)
            stat.Add(realType, Value);
        else
            stat.Multiply(realType, Value);
    }

    public enum Type
    {
        Add, Multiply
    }
}