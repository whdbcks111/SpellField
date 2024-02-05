using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

#if UNITY_EDITOR
    public const bool AllowSingleplay = true;
#else
    public const bool AllowSingleplay = false;
#endif

    public const string GameTimeOptionKey = "option_gametime";
    public const string MapRadiusOptionKey = "option_map_radius";
    public const string ObtainSpanOptionKey = "option_obtain_time";

    public const float DefaultGameTime = 8 * 60;
    public const float DefaultMapRadius = 200f;
    public const float MinAreaRadius = 5f;
    public const float DefaultObtainSpan = 40f;

    public static GameManager Instance { get; private set; }

    [HideInInspector] public Player SelfPlayer = null;
    [HideInInspector] public int Seed;

    [SerializeField] private Player _playerPrefab;
    [SerializeField] private AudioClip _bgm;
    [SerializeField] private TextMeshProUGUI _bounceTextPrefab;
    [SerializeField] private CinemachineVirtualCamera _virtualCam;
    [SerializeField] private SafeAreaControl _safeArea;
    [SerializeField] public StructureGenerator _structureGenerator;
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private GameObject _survivalUI;

    private float _remainGameTime = 0f, _maxGameTime = 1f, _mapRadius = 0f;

    public float MapRadius { get => _mapRadius; }
    private float _startTime = 0f;
    private float _obtainTime = 0f, _obtainSpan = 0f;
    public float RemainObtainTime { get => _remainGameTime - _obtainTime; }
    public float ObtainSpan { get => _obtainSpan; }

    private bool _isPlayerSpawned = false;
    private bool _isFinished = false;

    private readonly List<Action> _eventDisposeActions = new();
    private readonly CancellationTokenSource _destroyCancelToken = new();
    private readonly Dictionary<string, System.Random> _seedRandomMap = new();

    public int ObtainableCount = 0;

    public StructureGenerator StructureGenerator { get => _structureGenerator; }
    public UIManager UIManager { get => _uiManager; }
    public SafeAreaControl SafeArea { get => _safeArea; }
    public float RemainGameTime { get => _remainGameTime; }
    public int RemainPlayerCount
    {
        get
        {
            int survivalCount = 0;
            foreach (var p in Player.GetPlayers())
            {
                if (p.Mode == GameMode.Survival) survivalCount++;
            }
            return survivalCount;
        }
    }

    public Player AnyRemainPlayer
    {
        get
        {
            foreach (var p in Player.GetPlayers())
            {
                if (p.Mode == GameMode.Survival) return p;
            }
            return null;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _eventDisposeActions.Add(NetworkManager.Instance.On("chat", OnChat));
        _eventDisposeActions.Add(NetworkManager.Instance.On("spawn-bounce-text", OnSpawnBounceText));
    }

    private void OnSpawnBounceText(string from, Packet packet)
    {
        SpawnBounceText(packet.NextVector2(), packet.NextString());
    }

    public void OnSpawnPlayer(string _, Packet packet)
    {
        SpawnPlayer(packet.NextString(), packet.NextVector2()).Forget();
    }

    private async UniTask SpawnPlayer(string uid, Vector3 pos)
    {
        await UniTask.WaitUntil(() => SceneManager.GetActiveScene().name.Equals("GameScene"));
        Player.SpawnPlayer(_playerPrefab, uid, pos);
    }

    private void OnChat(string _, Packet packet)
    {
        UIManager.ChattingPanel.AddChat(packet.NextString());
    }

    public System.Random GetSeedRandom(string key)
    {
        if (!_seedRandomMap.ContainsKey(key)) _seedRandomMap[key] = new(Seed + key.GetHashCode());
        return _seedRandomMap[key];
    }

    public float GetSeedRandomRange(string key, float minInclusive, float maxExclusive)
    {
        return minInclusive + (float)(GetSeedRandom(key).NextDouble() * (maxExclusive - minInclusive));
    }

    public int GetSeedRandomRange(string key, int minInclusive, int maxInclusive)
    {
        return GetSeedRandom(key).Next(minInclusive, maxInclusive);
    }

    private void ApplyRoomOptions()
    {
        var roomState = NetworkManager.Instance.PingData.RoomState;
        if (int.TryParse(roomState.GetValueOrDefault("server_seed", "0"), out int seed))
        {
            Seed = seed;
        }

        _obtainTime = _remainGameTime = _maxGameTime = DefaultGameTime;

        if (roomState.TryGetValue(GameTimeOptionKey, out string timeStr)
            && float.TryParse(timeStr, out float time))
        {
            _obtainTime = _remainGameTime = _maxGameTime = time;
        }

        _obtainSpan = DefaultObtainSpan;
        if (roomState.TryGetValue(ObtainSpanOptionKey, out string obtainSpanStr)
            && float.TryParse(obtainSpanStr, out float obtainSpan))
        {
            _obtainSpan = obtainSpan;
        }

        _mapRadius = DefaultMapRadius;
        if (roomState.TryGetValue(MapRadiusOptionKey, out string mapRadiusStr)
            && float.TryParse(mapRadiusStr, out float mapRadius))
        {
            _mapRadius = mapRadius;
        }
    }

    public void StartGame()
    {
        ObtainableCount = 0;
        _startTime = Time.realtimeSinceStartup;

        ApplyRoomOptions();

        SafeArea.Center = Vector3.zero;
        SafeArea.Radius = _mapRadius;

        GenerateStructures();

        if (NetworkManager.Instance.PingData.IsMasterClient)
        {
            foreach (ClientInfo client in NetworkManager.Instance.PingData.Clients)
            {
                Vector2 pos;
                do
                {
                    pos = new(UnityEngine.Random.Range(-_mapRadius, _mapRadius),
                        UnityEngine.Random.Range(-_mapRadius, _mapRadius));
                }
                while (pos.magnitude > _mapRadius);

                NetworkManager.Instance.SendPacket("all", "spawn-player",
                    new(client.UID, pos));
            }

            var clients = NetworkManager.Instance.PingData.Clients;
            foreach (ClientInfo client in clients)
            {
                NetworkManager.Instance.RemoveRoomState("ready__" + client.UID);
            }
        }

        SoundManager.Instance.PlayBGM(_bgm);
    }

    private void GenerateStructures()
    {
        StructureGenerator.Generate(_mapRadius);
    }

    private void Update()
    {
        _remainGameTime = Mathf.Max(0, _maxGameTime - (Time.realtimeSinceStartup - _startTime));

        if (SelfPlayer != null)
        {
            if (!_isPlayerSpawned)
            {
                _isPlayerSpawned = true;
                _virtualCam.transform.position = SelfPlayer.transform.position;
            }
            _virtualCam.Follow = SelfPlayer.transform;
        }

        SafeArea.CurrentRadius = Mathf.Lerp(MinAreaRadius, _mapRadius, 
            Mathf.Clamp01(_remainGameTime / Mathf.Max(Mathf.Epsilon, _maxGameTime)));

        if (_remainGameTime < _obtainTime)
        {
            _obtainTime -= _obtainSpan;
            foreach (var p in Player.GetPlayers())
            {
                p.AddShield(p.MaxHP, 10f);
            }
            ObtainableCount++;
        }

        if (ObtainableCount > 0 && !UIManager.ObtainPanel.IsOpened)
        {
            ObtainableCount--;
            ShowObtainPanel();
        }

        if (!NetworkManager.Instance.PingData.RoomState.ContainsKey("is_single") &&
            SceneManager.GetActiveScene().name == "GameScene" &&
            Player.GetPlayers().Length >= NetworkManager.Instance.PingData.Clients.Length &&
            RemainPlayerCount <= 1 &&
            !_isFinished)
        {
            _isFinished = true;
            NetworkManager.Instance.RemoveRoomState("is_started");
            GameFinishTask(AnyRemainPlayer).Forget();
        }

        if (AllowSingleplay && Input.GetKeyDown(KeyCode.O))
        {
            ShowObtainPanel();
        }

        if (SelfPlayer != null)
        {
            _survivalUI.SetActive(SelfPlayer.Mode == GameMode.Survival);
        }
    }

    private async UniTask GameFinishTask(Player remainPlayer)
    {
        UIManager.WinnerUI.Show(remainPlayer.ClientInfo.Nickname);
        remainPlayer.AddShield(remainPlayer.MaxHP * 10, 10f, true);
        await UniTask.Delay(TimeSpan.FromSeconds(4f));
        await SceneManager.LoadSceneAsync("RoomScene");
    }

    public void ShowObtainPanel()
    {
        UIManager.ObtainPanel.Open();
    }

    public void ShowDamage(Vector3 pos, DamageParams damageParams)
    {
        string text = string.Format("{0:0.0}", damageParams.NormalDamage) +
            (damageParams.IsCriticalAttack ?
                string.Format("<color=#ff4444>(+{0:0.0})</color>", damageParams.CriticalDamage)
                : "");
        NetworkManager.Instance.SendPacket("all", "spawn-bounce-text", new((Vector2)pos, text));
    }

    public void SpawnBounceText(Vector3 pos, string text)
    {
        SpawnBounceTextTask(pos, text, 0.5f, 10f, 35f).Forget();
    }

    private async UniTask SpawnBounceTextTask(Vector3 pos, string text, float time, float bounceForce, float gravityForce)
    {
        var bounceText = Instantiate(_bounceTextPrefab, pos, Quaternion.identity);
        bounceText.transform.SetParent(UIManager.WorldCanvas.transform);
        bounceText.transform.localScale = Vector3.one;
        bounceText.SetText(text);

        float yVel = bounceForce;
        for (float t = 0f; t < time; t += Time.deltaTime)
        {
            await UniTask.Yield(cancellationToken: _destroyCancelToken.Token);
            if (_destroyCancelToken.IsCancellationRequested) return;
            bounceText.transform.position += yVel * Time.deltaTime * Vector3.up;
            yVel -= gravityForce * Time.deltaTime;
        }
        Destroy(bounceText.gameObject);
    }

    private void OnDestroy()
    {
        foreach (var dispose in _eventDisposeActions)
        {
            dispose();
        }
        _destroyCancelToken.Cancel();
        _destroyCancelToken.Dispose();
    }
}
