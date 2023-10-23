using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHPDisplay : MonoBehaviour
{
    [SerializeField] private HPBar _hpBar;
    [SerializeField] private TextMeshProUGUI _hpText;

    private void Update()
    {
        if (GameManager.Instance is not null && GameManager.Instance.SelfPlayer is not null)
        {
            Player player = GameManager.Instance.SelfPlayer;
            _hpBar.HP = player.HP;
            _hpBar.MaxHP = player.MaxHP;
            _hpBar.Shield = player.ShieldAmount;

            _hpText.SetText(string.Format("{0:0.} / {1:0.}", player.HP, player.MaxHP));
        }
    }
}
