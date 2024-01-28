using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodStone : Structure
{
    [SerializeField] private float _hpGrowth;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (LastAttacker != null)
        {
            LastAttacker.Stat.SetBase(StatType.MaxHP, LastAttacker.Stat.GetBase(StatType.MaxHP) + _hpGrowth);
            LastAttacker.RunOnceLateUpdate(() =>
            {
                LastAttacker.HP += _hpGrowth;
            });
            if (LastAttacker.IsSelf)
                GameManager.Instance.UIManager.ActionBar.ShowActionBar(
                    $"{StatType.MaxHP.DisplayName}(��)�� {StringUtil.HealValue(_hpGrowth)} �����߽��ϴ�.",
                    1f);
        }
    }
}
