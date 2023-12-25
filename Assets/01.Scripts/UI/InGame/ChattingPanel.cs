using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChattingPanel : MonoBehaviour
{
    [SerializeField] private float _minAlpha = 0.2f, _panelShowTime = 5f, _panelHideSpeed = 1f;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _chatText;
    [SerializeField] private TMP_InputField _inputField;

    private float _panelShowTimer = 0f;
    private float _panelHideTimer = 0f;

    private void Awake()
    {
        _canvasGroup.alpha = _minAlpha;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !_inputField.isFocused)
        {
            _inputField.selectionStringAnchorPosition = 0;
            _inputField.text = "";
            _inputField.Select();
        }

        if (_inputField.isFocused) _panelShowTimer = _panelShowTime;

        if (_panelShowTimer > 0f)
        {
            _panelShowTimer -= Time.deltaTime;
            _panelHideTimer = 1f;
            _canvasGroup.alpha = 1;
        }

        if (_panelShowTimer <= 0f && _panelHideTimer > 0f)
        {
            _panelHideTimer -= Time.deltaTime * _panelHideSpeed;
            _canvasGroup.alpha = _minAlpha + _panelHideTimer * (1 - _minAlpha);
        }
    }

    public void SendChat()
    {
        var text = _inputField.text;
        if (text.Trim().Length == 0) return;
        NetworkManager.Instance.SendPacket("all", "chat", $"[{NetworkManager.Instance.PingData.Nickname}] {text.Replace('\0', '\n')}");
        _inputField.text = "";
    }

    public void AddChat(string text)
    {
        _chatText.text += "\n" + text;
        _panelShowTimer = _panelShowTime;
    }
}
