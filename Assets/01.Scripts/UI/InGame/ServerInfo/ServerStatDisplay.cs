using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ServerStatDisplay : MonoBehaviour
{
    [SerializeField] private bool _isInGame = false;
    private TextMeshProUGUI _text;

    private float _latestFPSUpdate = 0f;
    private float _fps = 0f;

    private void Awake()
    {
        _fps = 1 / Time.unscaledDeltaTime;
        _latestFPSUpdate = Time.realtimeSinceStartup;
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

        string statText = $"{_fps:0} FPS\n" +
            $"지연율 <color={pingColor}>{ping}</color>ms\n";
        
        if(!_isInGame)
        {
            if (NetworkManager.Instance.IsInRoom)
            {
                statText += $"방ID {pingData.RoomID}\n" +
                    $"플레이어 {pingData.Clients.Length}/{(maxClientCount > 0 ? maxClientCount.ToString() : "무제한")}\n" +
                    (pingData.IsMasterClient ? "<color=#0f0>(Master)</color>" : "(Guest)");
            }
        }
        _text.SetText(statText);

        if(Time.realtimeSinceStartup > _latestFPSUpdate + 1f)
        {
            _latestFPSUpdate = Time.realtimeSinceStartup;
            _fps = 1 / Time.unscaledDeltaTime;
        }
    }
}
