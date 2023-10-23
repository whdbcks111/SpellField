using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManaDisplay : MonoBehaviour
{
    [SerializeField] private Image _mpBar;
    [SerializeField] private TextMeshProUGUI _mpText;

    private void Update()
    {
        if (GameManager.Instance is not null && GameManager.Instance.SelfPlayer is not null)
        {
            Player player = GameManager.Instance.SelfPlayer;
            _mpBar.fillAmount = player.Mana / Mathf.Max(Mathf.Epsilon, player.MaxMana);
            _mpText.SetText(string.Format("{0:0.} / {1:0.}", player.Mana, player.MaxMana));
        }
    }
}
