using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkClient : MonoBehaviour
{
    public static NetworkClient Instance { get; private set; }

    private readonly List<Action> _eventDisposeActions = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        _eventDisposeActions.Add(NetworkManager.Instance.OnJoinClient(OnJoinClient));
        _eventDisposeActions.Add(NetworkManager.Instance.OnJoinFailedClient(OnJoinFailed));
        _eventDisposeActions.Add(NetworkManager.Instance.OnLeaveClient(OnLeaveClient));
        _eventDisposeActions.Add(NetworkManager.Instance.On("spawn-player", OnSpawnPlayer));
        _eventDisposeActions.Add(NetworkManager.Instance.On("set-skin", OnSetSkin));
    }

    private void OnDestroy()
    {
        foreach (var dispose in _eventDisposeActions)
        {
            dispose();
        }
    }

    private void OnSpawnPlayer(string from, Packet packet)
    {
        OnSpawnPlayerTask(from, packet).Forget();
    }

    private async UniTask OnSpawnPlayerTask(string from, Packet packet)
    {
        await UniTask.WaitUntil(() => GameManager.Instance != null);
        GameManager.Instance.OnSpawnPlayer(from, packet);
    }

    public static string GetPlayerSkinStateKey(string uid)
    {
        return "skin__" + uid;
    }

    public void OnSetSkin(string from, Packet packet)
    {
        if(NetworkManager.Instance.PingData.IsMasterClient)
            NetworkManager.Instance.SetRoomState(GetPlayerSkinStateKey(from), packet.NextString());
    }


    public void OnJoinClient(string uid)
    {
        if(uid == NetworkManager.Instance.PingData.UID)
        {
            SceneManager.LoadScene("RoomScene");
            NetworkManager.Instance.SendPacket("master", "set-skin", 
                new(PlayerPrefs.GetString(PlayerSkinDatabase.LocalSkinDataKey, "")));
        }
        if(NetworkManager.Instance.PingData.IsMasterClient && 
            NetworkManager.Instance.PingData.RoomState.ContainsKey("is_started"))
        {
            NetworkManager.Instance.KickPlayer(uid);
        }
    }

    public void OnLeaveClient(string uid)
    {
        if (uid.Equals(NetworkManager.Instance.PingData.UID))
        {
            SceneManager.LoadSceneAsync("LobbyScene");
        }
        else if(Player.PlayerMap.TryGetValue(uid, out var player))
        {
            Destroy(player.gameObject);
            Player.PlayerMap.Remove(uid);
        }

        if(NetworkManager.Instance.PingData.IsMasterClient)
        {
            NetworkManager.Instance.RemoveRoomState(GetPlayerSkinStateKey(uid));
        }
    }

    public void OnJoinFailed(string reason)
    {

    }
}
