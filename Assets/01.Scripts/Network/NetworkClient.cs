using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;

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

    private async UniTask SetStructureHP(int id, float value)
    {
        Structure structure = null;
        await UniTask.WaitUntil(() => Structure.StructureMap.TryGetValue(id, out structure));
        structure.SyncHP(value);
    }

    private async UniTask SyncStructureDeath(int id)
    {
        Structure structure = null;
        await UniTask.WaitUntil(() => Structure.StructureMap.TryGetValue(id, out structure));
        structure.OnDeath();
    }

    private async UniTask SetSkinTask(string from, string message)
    {
        Player p = null;
        await UniTask.WaitUntil(() => Player.PlayerMap.TryGetValue(from, out p));
        var skinData = await PlayerSkinDatabase.GetSkin(message);
        if (skinData != null)
        {
            p.SetSkin(skinData);
        }
    }

    public void OnEvent(string from, string eventName, string message)
    {
        switch (eventName)
        {
            case "chat":
                {
                    if (GameManager.Instance != null)
                        GameManager.Instance.UIManager.ChattingPanel.AddChat(message);
                }
                break;
            case "room-chat":
                {
                    if (RoomChattingPanel.Instance != null)
                        RoomChattingPanel.Instance.AddChat(message);
                }
                break;
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
            case "set-skin":
                {
                    SetSkinTask(from, message).Forget();
                }
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
            case "player-death":
                {
                    if (Player.PlayerMap.TryGetValue(from, out var player))
                    {
                        player.OnDeath();
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
                        float.TryParse(splitResult[1], out var value))
                    {
                        SetStructureHP(id, value).Forget();
                    }
                }
                break;
            case "structure-death":
                {
                    if (int.TryParse(message, out var id))
                    {
                        SyncStructureDeath(id).Forget();
                    }
                }
                break;
            case "player-set-mana":
                {
                    if (Player.PlayerMap.TryGetValue(from, out var player) && float.TryParse(message, out var value))
                        player.SyncMana(value);
                }
                break;
            case "active-skill":
                {
                    if (Player.PlayerMap.TryGetValue(from, out var player))
                    {
                        foreach (var skill in player.GetSkills())
                        {
                            if (skill is not null && skill.Data.Name.Equals(message)) skill.Active(player);
                        }
                    }
                }
                break;
            case "add-effect":
                {
                    string[] splitResult = message.Split(':', 4);
                    var effType = EffectType.GetByName(splitResult[0]);
                    if (Player.PlayerMap.TryGetValue(from, out var target) &&
                        effType != null &&
                        int.TryParse(splitResult[1], out var level) &&
                        float.TryParse(splitResult[2], out var duration))
                    {
                        Player.PlayerMap.TryGetValue(splitResult[3], out Player caster);
                        target.AddEffect(new(effType, level, duration, caster), true);
                    }
                }
                break;
            case "add-shield":
                {
                    string[] splitResult = message.Split(':', 2);
                    if (Player.PlayerMap.TryGetValue(from, out var target) &&
                        float.TryParse(splitResult[0], out var amount) &&
                        float.TryParse(splitResult[1], out var time))
                    {
                        target.AddShield(amount, time, true);
                    }
                }
                break;
            case "damage-player":
                {
                    string[] splitResult = message.Split(':', 2);
                    if (Player.PlayerMap.TryGetValue(from, out var target) &&
                        float.TryParse(splitResult[0], out var amount) &&
                        Player.PlayerMap.TryGetValue(splitResult[1], out var attacker))
                    {
                        target.Damage(amount, attacker, true);
                    }
                }
                break;
            case "damage-structure":
                {
                    string[] splitResult = message.Split(':', 3);
                    if (int.TryParse(splitResult[0], out var id) &&
                        Structure.StructureMap.TryGetValue(id, out var structure) &&
                        float.TryParse(splitResult[1], out var amount) &&
                        Player.PlayerMap.TryGetValue(splitResult[2], out var attacker))
                    {
                        structure.Damage(amount, attacker, true);
                    }
                }
                break;
            case "start-charge-skill":
                {
                    if (Player.PlayerMap.TryGetValue(from, out var player))
                    {
                        foreach (var skill in player.GetSkills())
                        {
                            if (skill is not null && skill.Data.Name.Equals(message)) skill.StartCharge(player);
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
        if (data != null)
        {
            player.SetSkill(Mathf.Clamp(index, 0, player.SkillLength - 1), new PlayerSkill(data, level));
        }
    }

    private async UniTask ChangeWeapon(Player player, string weaponName)
    {
        WeaponData data = await WeaponDatabase.GetWeapon(weaponName);
        if (data != null) player.MountedWeapon = new Weapon(data);
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
