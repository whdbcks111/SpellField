using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomChattingPanel : MonoBehaviour
{
    public static RoomChattingPanel Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI _chatText;
    [SerializeField] private TMP_InputField _inputField;

    private void Awake()
    {
        Instance = this;
    }

    public void SendChat()
    {
        var text = _inputField.text;
        if (text.Trim().Length == 0) return;
        NetworkManager.Instance.SendPacket("all", "room-chat", $"[{NetworkManager.Instance.PingData.Nickname}] {text.Replace('\0', '\n')}");

        _inputField.selectionStringAnchorPosition = 0;
        _inputField.text = "";
        _inputField.ActivateInputField();
    }

    public void AddChat(string text)
    {
        _chatText.text += "\n" + text;
    }
}
