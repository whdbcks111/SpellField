using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class RemainPlayerInfoUI : MonoBehaviour
{
    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (NetworkManager.Instance is null || !NetworkManager.Instance.IsPingDataSetted) return;
        _text.SetText(NetworkManager.Instance.PingData.Clients.Length + "¸í »ýÁ¸");
    }
}
