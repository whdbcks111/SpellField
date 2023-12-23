using System;
using UnityEngine;

public class MeleeAttackWeaponDisplay : WeaponDisplay
{
   public TrailInfo[] TrailInfos;
}

[Serializable]
public struct TrailInfo
{
    public Transform TrailPoint;
    public TrailRenderer TrailPrefab;
}