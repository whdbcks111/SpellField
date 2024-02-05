using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomChattingPanel : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI _chatText;
    [SerializeField] private TMP_InputField _inputField;

    private Action _chatEventDispose;

    private void Start()
    {
        _chatEventDispose = NetworkManager.Instance.On("room-chat", (_, packet) =>
        {
            AddChat(packet.NextString());
        });
    }

    private void OnDestroy()
    {
        _chatEventDispose();
    }

    public void SendChat()
    {
        var text = _inputField.text;
        if (text.Trim().Length == 0) return;
        NetworkManager.Instance.SendPacket("all", "room-chat", 
            new($"[{NetworkManager.Instance.PingData.Nickname}] {text.Replace('\0', '\n')}"));

        _inputField.selectionStringAnchorPosition = 0;
        _inputField.text = "";
        _inputField.ActivateInputField();
    }

    public void AddChat(string text)
    {
        _chatText.text += "\n" + text;
    }
}
