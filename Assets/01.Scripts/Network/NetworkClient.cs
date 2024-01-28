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
        _eventDisposeActions.Add(NetworkManager.Instance.On("set-skin", OnSetSkin));
    }

    private void OnDestroy()
    {
        foreach (var dispose in _eventDisposeActions)
        {
            dispose();
        }
    }

    public static string GetPlayerSkinStateKey(string uid)
    {
        return "skin__" + uid;
    }

    public void OnSetSkin(string from, string message)
    {
        if(NetworkManager.Instance.PingData.IsMasterClient)
            NetworkManager.Instance.SetRoomState(GetPlayerSkinStateKey(from), message);
    }


    public void OnJoinClient(string uid)
    {
        if(uid == NetworkManager.Instance.PingData.UID)
        {
            SceneManager.LoadScene("RoomScene");
            NetworkManager.Instance.SendPacket("master", "set-skin", 
                PlayerPrefs.GetString(PlayerSkinDatabase.LocalSkinDataKey, ""));
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
