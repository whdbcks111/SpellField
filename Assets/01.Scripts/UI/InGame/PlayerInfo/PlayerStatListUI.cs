using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerStatListUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _attackStatText, _magicForceStatText, _moveSpeedStatText, _criticalStatText,
        _defendStatText, _magicResistStatText, _defendPenetStatText, _magicPenetStatText;

    private void Update()
    {
        if(GameManager.Instance != null && GameManager.Instance.SelfPlayer != null)
        {
            Player player = GameManager.Instance.SelfPlayer;
            // 공격력 텍스트
            _attackStatText.SetText(string.Format("{0:0.}", 
                player.Stat.Get(StatType.Attack)));
            // 마법력 텍스트
            _magicForceStatText.SetText(string.Format("{0:0.}", 
                player.Stat.Get(StatType.MagicForce)));
            // 이동 속도 텍스트
            _moveSpeedStatText.SetText(string.Format("{0:0.0}m/s", 
                player.Stat.Get(StatType.MoveSpeed)));
            // 치명타(확률+피해량) 텍스트
            _criticalStatText.SetText(string.Format("{0:0.}% <color=#f44>*{1:0.}%</color>", 
                player.Stat.Get(StatType.CriticalChance), player.Stat.Get(StatType.CriticalDamage)));
            // 방어력(수치+감소율) 텍스트
            _defendStatText.SetText(string.Format("{0:0.} (={1:0.}%)", 
                player.Stat.Get(StatType.Defend), Stat.GetDefendRatio(player.Stat.Get(StatType.Defend)) * 100));
            // 마법 저항력(수치+감소율) 텍스트
            _magicResistStatText.SetText(string.Format("{0:0.} (={1:0.}%)",
                player.Stat.Get(StatType.MagicResistance), Stat.GetDefendRatio(player.Stat.Get(StatType.MagicResistance)) * 100));
            // 방어력 관통력 텍스트
            _defendPenetStatText.SetText(string.Format("{0:0.}", player.Stat.Get(StatType.DefendPenetrate)));
            // 마법 관통력 텍스트
            _magicPenetStatText.SetText(string.Format("{0:0.}", player.Stat.Get(StatType.MagicPenetrate)));
        }
    }
}
