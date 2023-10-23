using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomLeaveButton : MonoBehaviour
{


    public void LeaveRoom()
    {
        NetworkManager.Instance.LeaveRoom();
        SceneManager.LoadScene("LobbyScene");
    }
}