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

    private Player _player;

    private void Update()
    {
        if(GameManager.Instance != null && GameManager.Instance.SelfPlayer != null)
        {
            _player = GameManager.Instance.SelfPlayer;
            // ���ݷ� �ؽ�Ʈ
            _attackStatText.SetText(string.Format("{0:0.}", 
                _player.Stat.Get(StatType.Attack)));
            // ������ �ؽ�Ʈ
            _magicForceStatText.SetText(string.Format("{0:0.}", 
                _player.Stat.Get(StatType.MagicForce)));
            // �̵� �ӵ� �ؽ�Ʈ
            _moveSpeedStatText.SetText(string.Format("{0:0.0}m/s", 
                _player.Stat.Get(StatType.MoveSpeed)));
            // ġ��Ÿ(Ȯ��+���ط�) �ؽ�Ʈ
            _criticalStatText.SetText(string.Format("{0:0.}% <color=#f44>*{1:0.}%</color>", 
                _player.Stat.Get(StatType.CriticalChance), _player.Stat.Get(StatType.CriticalDamage)));
            // ����(��ġ+������) �ؽ�Ʈ
            _defendStatText.SetText(string.Format("{0:0.} (={1:0.}%)", 
                _player.Stat.Get(StatType.Defend), Stat.GetDefendRatio(_player.Stat.Get(StatType.Defend)) * 100));
            // ���� ���׷�(��ġ+������) �ؽ�Ʈ
            _magicResistStatText.SetText(string.Format("{0:0.} (={1:0.}%)",
                _player.Stat.Get(StatType.MagicResistance), Stat.GetDefendRatio(_player.Stat.Get(StatType.MagicResistance)) * 100));
            // ���� ����� �ؽ�Ʈ
            _defendPenetStatText.SetText(string.Format("{0:0.}", _player.Stat.Get(StatType.DefendPenetrate)));
            // ���� ����� �ؽ�Ʈ
            _magicPenetStatText.SetText(string.Format("{0:0.}", _player.Stat.Get(StatType.MagicPenetrate)));
        }
    }
}
