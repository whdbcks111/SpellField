using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomPlayerInfoUI : MonoBehaviour
{
    [HideInInspector] public string UID;
    public TextMeshProUGUI Name;
    public Image ProfileImage;
    public Image CrownIcon;
    [SerializeField] private Button _kickButton;
    [SerializeField] private Image _readyToggle;
    [SerializeField] private Color _readyColor, _nonReadyColor;

    private async UniTask SetSkinProfile()
    {
        if (NetworkManager.Instance.PingData.RoomState.TryGetValue(
            NetworkClient.GetPlayerSkinStateKey(UID), out var skinName))
        {
            var skinData = await PlayerSkinDatabase.GetSkin(skinName);
            ProfileImage.sprite = skinData.PlayerSprite;
        }
    }

    private void Update()
    {
        if (!NetworkManager.Instance.IsPingDataSetted) return;
        var isMasterClientInfo = UID.Equals(NetworkManager.Instance.PingData.Clients[0].UID);

        _kickButton.gameObject.SetActive(NetworkManager.Instance.PingData.IsMasterClient 
            && !isMasterClientInfo);

        CrownIcon.gameObject.SetActive(isMasterClientInfo);
        SetSkinProfile().Forget();
        _readyToggle.gameObject.SetActive(!isMasterClientInfo);
        _readyToggle.color = NetworkManager.Instance.PingData.RoomState.ContainsKey("ready__" + UID) ? _readyColor : _nonReadyColor;
    }

    public void KickPlayer()
    {
        NetworkManager.Instance.KickPlayer(UID);
    }
}
