using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class RoomPlayerListUI : MonoBehaviour
{

    [SerializeField] private RoomPlayerInfoUI _playerInfoPrefab;
    [SerializeField] private Button _startButton, _readyButton, _singlePlayButton;

    private ScrollRect _scrollRect;

    private void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
        _startButton.gameObject.SetActive(false);
        _readyButton.gameObject.SetActive(false);
        _singlePlayButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!NetworkManager.Instance.IsPingDataSetted) return;
        var clients = NetworkManager.Instance.PingData.Clients;
        while (_scrollRect.content.childCount < clients.Length)
        {
            Instantiate(_playerInfoPrefab, _scrollRect.content);
        }

        while (_scrollRect.content.childCount > clients.Length)
        {
            DestroyImmediate(_scrollRect.content.GetChild(0).gameObject);
        }

        for(int i = 0; i < clients.Length; ++i)
        {
            var client = clients[i];
            var child = _scrollRect.content.GetChild(i);
            if(child.TryGetComponent<RoomPlayerInfoUI>(out var infoUI))
            {
                infoUI.UID = client.UID;
                infoUI.Name.SetText(client.Nickname);
            }
        }

        bool startBtnActive = NetworkManager.Instance.PingData.IsMasterClient && clients.Length > 1;
        if (startBtnActive)
        {
            bool isFirst = true;
            foreach (ClientInfo client in clients)
            {
                if (isFirst)
                {
                    isFirst = false;
                    continue;
                }
                if (!NetworkManager.Instance.PingData.RoomState.ContainsKey("ready__" + client.UID))
                {
                    startBtnActive = false;
                    break;
                }
            }
        }
        _startButton.gameObject.SetActive(startBtnActive);

        _readyButton.gameObject.SetActive(clients.Length > 0 && !NetworkManager.Instance.PingData.IsMasterClient);

        _singlePlayButton.gameObject.SetActive(clients.Length > 0 && GameManager.AllowSingleplay);
    }

    public void Ready()
    {
        NetworkManager.Instance.SendPacket("master", "ready", "");
    }

    public void StartGame()
    {
        var clients = NetworkManager.Instance.PingData.Clients;
        NetworkManager.Instance.SetRoomState("is_started", "");
        NetworkManager.Instance.SetRoomState("server_seed", Random.Range(int.MinValue, int.MaxValue).ToString());
        NetworkManager.Instance.SendPacket("all", "start-game", "");

        foreach (ClientInfo client in clients)
        {
            NetworkManager.Instance.RemoveRoomState("ready__" + client.UID);
        }
    }

    public void StartSingleGame()
    {
        NetworkManager.Instance.SetRoomState("is_single", "");
        StartGame();
    }
}
