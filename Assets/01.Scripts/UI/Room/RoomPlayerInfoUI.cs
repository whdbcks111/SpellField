using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomPlayerInfoUI : MonoBehaviour
{
    [HideInInspector] public string UID;
    public TextMeshProUGUI Name;
    [SerializeField] private Button _kickButton;
    [SerializeField] private Image _readyToggle;
    [SerializeField] private Color _readyColor, _nonReadyColor;

    private void Update()
    {
        if (!NetworkManager.Instance.IsPingDataSetted) return;
        _kickButton.gameObject.SetActive(NetworkManager.Instance.PingData.IsMasterClient && !NetworkManager.Instance.PingData.UID.Equals(UID));
        _readyToggle.gameObject.SetActive(!UID.Equals(NetworkManager.Instance.PingData.Clients[0].UID));
        _readyToggle.color = NetworkManager.Instance.PingData.RoomState.ContainsKey("ready__" + UID) ? _readyColor : _nonReadyColor;
    }

    public void KickPlayer()
    {
        NetworkManager.Instance.KickPlayer(UID);
    }
}
