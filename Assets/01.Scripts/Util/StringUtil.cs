using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringUtil
{
    public static string MagicalValue(float value)
    {
        return $"<color=#aa55ff>{value:0.}</color>";
    }
    public static string ManaValue(float value)
    {
        return $"<color=#4444ff>{value:0.}</color>";
    }

    public static string HealValue(float value)
    {
        return $"<color=#55ff55>{value:0.}</color>";
    }
    public static string PhysicalValue(float value)
    {
        return $"<color=#ff8855>{value:0.}</color>";
    }
    public static string FixedValue(float value)
    {
        return $"<color=#cccccc>{value:0.}</color>";
    }
}