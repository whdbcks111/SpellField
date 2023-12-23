using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureBox : Structure
{
    private void OnDestroy()
    {
        if (LastAttacker != null && LastAttacker.IsSelf)
        {
            GameManager.Instance.ObtainableCount++;
        }
    }
}
