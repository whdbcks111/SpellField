using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnhanceTree : Structure
{
    [SerializeField] private float _time;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (LastAttacker != null)
        {
            LastAttacker.AddEffect(new Effect(EffectType.Enhance, 1, _time, LastAttacker));
            if (LastAttacker.IsSelf)
                GameManager.Instance.UIManager.ActionBar.ShowActionBar(
                    $"<color=yellow>{_time:0}초 동안 강화됩니다.</color>",
                    2f);
        }
    }
}
