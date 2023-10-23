using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkClient : MonoBehaviour, IClient
{
    [SerializeField] private Player _playerPrefab;
    private static NetworkClient _instance = null;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        NetworkManager.Instance.Client = this;
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

    public void OnEvent(string from, string eventName, string message)
    {
        switch (eventName)
        {
            case "ready":
                if (NetworkManager.Instance.PingData.RoomState.ContainsKey("ready__" + from))
                {
                    NetworkManager.Instance.RemoveRoomState("ready__" + from);
                }
                else
                {
                    NetworkManager.Instance.SetRoomState("ready__" + from, "");
                }
                break;
            case "start-game":
                OnStartGame().Forget();
                break;
            case "spawn-player":
                {
                    string[] splitResult = message.Split(':', 3);
                    if (float.TryParse(splitResult[1], out float x) && float.TryParse(splitResult[2], out float y))
                    {
                        SpawnPlayer(splitResult[0], new(x, y)).Forget();
                    }
                }
                break;
            case "move-player":
                {
                    string[] splitResult = message.Split(':', 2);
                    if (Player.PlayerMap.TryGetValue(from, out var player) &&
                        float.TryParse(splitResult[0], out float x) &&
                        float.TryParse(splitResult[1], out float y))
                    {
                        player.OnMove(new(x, y));
                    }
                }
                break;
            case "rotate-player":
                {
                    if (Player.PlayerMap.TryGetValue(from, out var player) &&
                        float.TryParse(message, out float z))
                    {
                        player.OnRotate(z);
                    }
                }
                break;
            case "player-set-hp":
                {
                    if (Player.PlayerMap.TryGetValue(from, out var player) && float.TryParse(message, out var value))
                        player.SyncHP(value);
                }
                break;
            case "structure-set-hp":
                {
                    string[] splitResult = message.Split(':', 2);
                    if (int.TryParse(splitResult[0], out var id) &&
                        Structure.StructureMap.TryGetValue(id, out var structure) &&
                        float.TryParse(splitResult[1], out var value))
                    {
                        structure.SyncHP(value);
                    }
                }
                break;
            case "player-set-mana":
                {
                    if (Player.PlayerMap.TryGetValue(from, out var player) && float.TryParse(message, out var value))
                        player.SyncMana(value);
                }
                break;
            case "show-damage":
                {
                    string[] splitResult = message.Split(':', 3);
                    if (Player.PlayerMap.TryGetValue(from, out var player) &&
                        float.TryParse(splitResult[0], out float x) &&
                        float.TryParse(splitResult[1], out float y) &&
                        float.TryParse(splitResult[2], out float amount))
                    {
                        //GameManager.Instance.ShowDamage(new Vector3(x, y), amount);
                    }
                }
                break;
            case "active-skill":
                {
                    if (Player.PlayerMap.TryGetValue(from, out var player))
                    {
                        foreach (var skill in player.Skills)
                        {
                            if (skill is not null && skill.Data.Name.Equals(message)) skill.Active(player);
                        }
                    }
                }
                break;
            case "use-weapon":
                {
                    if (Player.PlayerMap.TryGetValue(from, out var player))
                    {
                        player.MountedWeapon?.Use(player);
                    }
                }
                break;
            case "change-weapon":
                {
                    if (Player.PlayerMap.TryGetValue(from, out var player))
                    {
                        ChangeWeapon(player, message).Forget();
                    }
                }
                break;
            case "change-skill":
                {
                    string[] splitResult = message.Split(':', 3);
                    if (Player.PlayerMap.TryGetValue(from, out var player) &&
                        int.TryParse(splitResult[0], out var idx) &&
                        int.TryParse(splitResult[2], out var level))
                    {
                        ChangeSkill(player, idx, splitResult[1], level).Forget();
                    }
                }
                break;
            case "spawn-bounce-text":
                {
                    string[] splitResult = message.Split(':', 3);
                    if (float.TryParse(splitResult[0], out var x) &&
                       float.TryParse(splitResult[1], out var y))
                    {
                        GameManager.Instance.SpawnBounceText(new(x, y), splitResult[2]);
                    }
                }
                break;
        }
    }

    private async UniTask ChangeSkill(Player player, int index, string skillName, int level)
    {
        PlayerSkillData data = await PlayerSkillDatabase.GetSkill(skillName);
        if (data is not null) player.Skills[Mathf.Clamp(index, 0, player.Skills.Length - 1)] = new PlayerSkill(data, level);
    }

    private async UniTask ChangeWeapon(Player player, string weaponName)
    {
        WeaponData data = await WeaponDatabase.GetWeapon(weaponName);
        if (data is not null) player.MountedWeapon = new Weapon(data);
    }

    private async UniTask SpawnPlayer(string uid, Vector3 pos)
    {
        await UniTask.WaitUntil(() => SceneManager.GetActiveScene().name.Equals("GameScene"));
        Player.SpawnPlayer(_playerPrefab, uid, pos);
    }

    public void OnJoinClient(string uid)
    {
        if(uid == NetworkManager.Instance.PingData.UID)
        {
            SceneManager.LoadScene("RoomScene");
        }
        if(NetworkManager.Instance.PingData.IsMasterClient && 
            NetworkManager.Instance.PingData.RoomState.ContainsKey("is_started"))
        {
            NetworkManager.Instance.KickPlayer(uid);
        }
    }

    public void OnLeaveClient(string uid)
    {
        if (uid.Equals(NetworkManager.Instance.PingData.UID))
        {
            SceneManager.LoadSceneAsync("LobbyScene");
        }
        else if(Player.PlayerMap.TryGetValue(uid, out var player))
        {
            Destroy(player.gameObject);
            Player.PlayerMap.Remove(uid);
        }
    }

    public void OnJoinFailed(string reason)
    {

    }
}
