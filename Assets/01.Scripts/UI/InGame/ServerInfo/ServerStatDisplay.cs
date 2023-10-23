using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ServerStatDisplay : MonoBehaviour
{
    [SerializeField] private bool _isInGame = false;
    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _text.SetText("");
    }

    private void Update()
    {
        if (!NetworkManager.Instance.IsPingDataSetted) return;
        var pingData = NetworkManager.Instance.PingData;
        int maxClientCount = pingData.MaxClientCount;
        int ping = pingData.Ping;
        string pingColor = "#aaaaaa";
        if (ping > 1000) pingColor = "#ff0000";
        else if (ping > 500) pingColor = "#ff4400";
        else if (ping > 200) pingColor = "#ff9900";
        else if (ping > 100) pingColor = "#ffff33";

        string statText = "������ <color=" + pingColor + ">" + ping + "</color>ms\n";
        
        if(!_isInGame)
        {
            statText += "�г��� <color=#aaa>" + pingData.Nickname + "</color>\n";
            if (NetworkManager.Instance.IsInRoom)
            {
                statText += "��ID " + pingData.RoomID + "\n" +
                    "�÷��̾� " + pingData.Clients.Length + "/" + (maxClientCount > 0 ? maxClientCount.ToString() : "������") + "\n" +
                    (pingData.IsMasterClient ? "<color=#0f0>(Master)</color>" : "(Guest)");
            }
        }
        _text.SetText(statText);
    }
}