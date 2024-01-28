

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomSceneManager : MonoBehaviour
{
    public static RoomSceneManager Instance { get; private set; }


    private readonly List<Action> _eventDisposeActions = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _eventDisposeActions.Add(NetworkManager.Instance.On("ready", OnPlayerReady));
        _eventDisposeActions.Add(NetworkManager.Instance.On("start-game", (_, _) => OnStartGame().Forget()));
    }

    private async UniTask OnStartGame()
    {
        Player.PlayerMap.Clear();
        Structure.StructureMap.Clear();
        Structure.StructureCounter = 0;

        await SceneManager.LoadSceneAsync("GameScene");
        await UniTask.WaitUntil(() => GameManager.Instance != null);
        GameManager.Instance.StartGame();
    }

    private void OnPlayerReady(string from, string message)
    {
        if (NetworkManager.Instance.PingData.RoomState.ContainsKey("ready__" + from))
        {
            NetworkManager.Instance.RemoveRoomState("ready__" + from);
        }
        else
        {
            NetworkManager.Instance.SetRoomState("ready__" + from, "");
        }
    }

    private void OnDestroy()
    {
        foreach (var dispose in _eventDisposeActions)
        {
            dispose();
        }
    }
}