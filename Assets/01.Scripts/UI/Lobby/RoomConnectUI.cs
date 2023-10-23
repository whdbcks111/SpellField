using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomConnectUI : MonoBehaviour
{
    [SerializeField] GameObject _roomJoinUI, _roomCreateUI;
    [SerializeField] TMP_InputField _roomIdInput, _nicknameInput;

    private void Awake()
    {
        InitNickname().Forget();
    }

    private async UniTask InitNickname()
    {
        await UniTask.WaitUntil(() => NetworkManager.Instance.IsPingDataSetted);
        _nicknameInput.text = PlayerPrefs.HasKey("used_nickname") ? PlayerPrefs.GetString("used_nickname") : NetworkManager.Instance.PingData.Nickname;
    }

    public void QuickJoin()
    {
        var roomInfos = NetworkManager.Instance.RoomInfos;
        if (roomInfos.Length == 0)
        {
            CreateRoom();
        }
        else
        {
            RoomInfo target = roomInfos[0];
            for (var i = 1; i < roomInfos.Length; i++)
            {
                if(target.ClientCount < roomInfos[i].ClientCount)
                {
                    target = roomInfos[i];
                }
            }
            print(target.UID);
            NetworkManager.Instance.JoinRoom(target.UID);
        }
    }

    public void CreateRoom()
    {
        NetworkManager.Instance.CreateRoom();
    }

    public void JoinRoom()
    {
        NetworkManager.Instance.JoinRoom(_roomIdInput.text);
    }

    public void ChangeNickname()
    {
        NetworkManager.Instance.ChangeNickname(_nicknameInput.text);
        PlayerPrefs.SetString("used_nickname", _nicknameInput.text);
    }
}
