using Cinemachine;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

#if UNITY_EDITOR
    public const bool AllowSingleplay = true;
#else
    public const bool AllowSingleplay = false;
#endif

    public const float DefaultGameTime = 8 * 60;
    public const float DefaultMapRadius = 200f;
    public const float MinAreaRadius = 10f;
    public const float DefaultObtainSpan = 40f;

    public static GameManager Instance { get; private set; }

    [HideInInspector] public Player SelfPlayer = null;
    [HideInInspector] public int Seed;

    [SerializeField] private TextMeshProUGUI _bounceTextPrefab;
    [SerializeField] private CinemachineVirtualCamera _virtualCam;
    [SerializeField] private SafeAreaControl _safeArea;
    [SerializeField] public StructureGenerator _structureGenerator;
    [SerializeField] private UIManager _uiManager;

    private float _remainGameTime = 0f, _maxGameTime = 1f, _mapRadius = 0f;
    public float MapRadius { get => _mapRadius; }
    private float _startTime = 0f;
    private float _obtainTime = 0f, _obtainSpan = 0f;

    private bool _isPlayerSpawned = false;

    private readonly CancellationTokenSource _destroyCancelToken = new();
    private readonly Dictionary<string, System.Random> _seedRandomMap = new();

    public StructureGenerator StructureGenerator { get => _structureGenerator; }
    public UIManager UIManager { get => _uiManager; }
    public SafeAreaControl SafeArea { get => _safeArea; }
    public float RemainGameTime { get => _remainGameTime; }

    private void Awake()
    {
        Instance = this;
    }

    public System.Random GetSeedRandom(string key)
    {
        if(!_seedRandomMap.ContainsKey(key)) _seedRandomMap[key] = new(Seed + key.GetHashCode());
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

    public void StartGame()
    {
        _startTime = Time.realtimeSinceStartup;

        var roomState = NetworkManager.Instance.PingData.RoomState;
        if (int.TryParse(roomState.GetValueOrDefault("server_seed", "0"), out int seed)) {
            Seed = seed;
        }

        _obtainTime = _remainGameTime = _maxGameTime = DefaultGameTime;

        if(roomState.TryGetValue("option_gametime", out string timeStr) 
            && float.TryParse(timeStr, out float time))
        {
            _obtainTime = _remainGameTime = _maxGameTime = time;
        }

        _obtainSpan = DefaultObtainSpan;
        if (roomState.TryGetValue("option_obtain_time", out string obtainSpanStr)
            && float.TryParse(obtainSpanStr, out float obtainSpan))
        {
            _obtainSpan = obtainSpan;
        }

        _mapRadius = DefaultMapRadius;
        SafeArea.Center = Vector3.zero;
        SafeArea.Radius = _mapRadius;

        GenerateStructures();


        if (NetworkManager.Instance.PingData.IsMasterClient)
        {
            foreach (ClientInfo client in NetworkManager.Instance.PingData.Clients)
            {
                float angle = Random.Range(0, 2 * Mathf.PI);
                Vector2 pos = Random.Range(0, _mapRadius) * new Vector2(
                    Mathf.Cos(angle),
                    Mathf.Sin(angle)
                    );
                NetworkManager.Instance.SendPacket("all", "spawn-player",
                    string.Format("{0}:{1:0.000}:{2:0.000}", client.UID, pos.x, pos.y));
            }
        }

    }

    private void GenerateStructures()
    {
        StructureGenerator.Generate(_mapRadius);
    }

    private void Update()
    {
        _remainGameTime = Mathf.Max(0, _maxGameTime - (Time.realtimeSinceStartup - _startTime));

        if(SelfPlayer is not null)
        {
            if(!_isPlayerSpawned)
            {
                _isPlayerSpawned = true;
                _virtualCam.transform.position = SelfPlayer.transform.position;
            }
            _virtualCam.Follow = SelfPlayer.transform;
        }

        SafeArea.CurrentRadius = Mathf.Lerp(MinAreaRadius, _mapRadius, Mathf.Clamp01(_remainGameTime / Mathf.Max(Mathf.Epsilon, _maxGameTime)));

        if(_remainGameTime < _obtainTime)
        {
            _obtainTime -= _obtainSpan;
            foreach (var p in Player.GetPlayers())
            {
                p.AddShield(p.MaxHP, 10f);
            }
            ShowObtainPanel();
        }

        if (NetworkManager.Instance.PingData.IsMasterClient &&
            !NetworkManager.Instance.PingData.RoomState.ContainsKey("is_single") &&
            NetworkManager.Instance.PingData.RoomState.ContainsKey("is_started") &&
            NetworkManager.Instance.PingData.Clients.Length == 1)
        {
            NetworkManager.Instance.RemoveRoomState("is_started");
            SceneManager.LoadSceneAsync("RoomScene");
        }

        if (AllowSingleplay && Input.GetKeyDown(KeyCode.O)) ShowObtainPanel();
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
        NetworkManager.Instance.SendPacket("all", "spawn-bounce-text", string.Format("{0:0.000}:{1:0.000}:{2}", pos.x, pos.y, text));
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
        for(float t = 0f; t < time; t += Time.deltaTime)
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
        _destroyCancelToken.Cancel();
        _destroyCancelToken.Dispose();
    }
}
