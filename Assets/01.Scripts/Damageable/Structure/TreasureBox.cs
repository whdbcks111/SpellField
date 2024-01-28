using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureBox : Structure
{
    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (LastAttacker != null && LastAttacker.IsSelf)
        {
            GameManager.Instance.ObtainableCount++;
        }
    }
}
