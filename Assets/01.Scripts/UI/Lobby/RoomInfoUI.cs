using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomInfoUI : MonoBehaviour
{
    [HideInInspector] public string RoomID;
    public TextMeshProUGUI Name;

    public void JoinRoom()
    {
        NetworkManager.Instance.JoinRoom(RoomID);
    }
}