using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnhanceTree : Structure
{
    [SerializeField] private float _time;

    private void OnDestroy()
    {
        if (LastAttacker != null)
        {
            LastAttacker.AddEffect(new Effect(EffectType.Enhance, 1, _time, LastAttacker));
            if (LastAttacker.IsSelf)
                GameManager.Instance.UIManager.ActionBar.ShowActionBar(
                    $"<color=yellow>{_time:0}�� ���� ��ȭ�˴ϴ�.</color>",
                    2f);
        }
    }
}
