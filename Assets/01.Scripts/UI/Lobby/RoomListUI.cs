using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class RoomListUI : MonoBehaviour
{

    [SerializeField] private RoomInfoUI _roomInfoPrefab;

    private ScrollRect _scrollRect;

    private void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
    }

    private void Update()
    {
        if (!NetworkManager.Instance.IsPingDataSetted) return;
        var rooms = NetworkManager.Instance.RoomInfos;
        while (_scrollRect.content.childCount < rooms.Length)
        {
            Instantiate(_roomInfoPrefab, _scrollRect.content);
        }

        while (_scrollRect.content.childCount > rooms.Length)
        {
            DestroyImmediate(_scrollRect.content.GetChild(0).gameObject);
        }

        for (int i = 0; i < rooms.Length; ++i)
        {
            var room = rooms[i];
            var child = _scrollRect.content.GetChild(i);
            if (child.TryGetComponent<RoomInfoUI>(out var infoUI))
            {
                infoUI.RoomID = room.UID;
                infoUI.Name.SetText(room.MasterClientNickname + "¥‘¿« πÊ");
            }
        }
    }
}
