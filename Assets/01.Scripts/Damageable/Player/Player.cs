using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : Damageable
{
    public static readonly float SyncDelay = 0.04f;
    public static readonly Dictionary<string, Player> PlayerMap = new();

    [HideInInspector] public ClientInfo ClientInfo;

    public bool IsSelf { get { return ClientInfo.UID.Equals(NetworkManager.Instance.PingData.UID); } }

    private float _beforeHp = 0f;
    public override float HP
    {
        get => base.HP;
        set
        {
            if (!IsSelf) return;
            base.HP = value;
        }
    }

    public override float MaxHP
    {
        get => base.MaxHP;
    }

    private float _beforeMana = 0f;
    public override float Mana
    {
        get => base.Mana;
        set
        {
            if (!IsSelf) return;
            base.Mana = value;
        }
    }

    public SpriteRenderer PlayerRenderer { get { return _playerRenderer; } }
    public PlayerHand Hand { get { return _hand; } }

    [SerializeField] public KeyCode[] SkillKeys;
    [HideInInspector] public PlayerSkill[] Skills;
    [HideInInspector] public Weapon MountedWeapon
    {
        get
        {
            return _mountedWeapon;
        }
        set
        {
            _mountedWeapon?.OnUnmount(this);
            _mountedWeapon = value;
            _mountedWeapon?.OnMount(this);
        }
    }

    [SerializeField] private TextMeshProUGUI _nameTextUI;
    [SerializeField] private SpriteRenderer _playerRenderer;
    [SerializeField] private Sprite _playerSelfSprite, _playerSelfHand, _playerOtherSprite, _playerOtherHand;
    [SerializeField] private PlayerHand _hand;
    private Weapon _mountedWeapon = null;

    private Rigidbody2D _rigid;
    private Collider2D _collider2D;
    private Vector3 _smoothPos, _smoothVel;
    private Vector3 _beforePos;
    private float _beforeRotZ, _smoothRotZ;
    private float _sendSyncPacketTimer = 0f;

    private readonly List<Effect> _effects = new();

    public static void SpawnPlayer(Player prefab, string id, Vector3 pos)
    {
        Player p = Instantiate(prefab, pos, Quaternion.identity);
        p.name = "Player_" + id;
        foreach (var info in NetworkManager.Instance.PingData.Clients)
        {
            if (info.UID.Equals(id))
            {
                p.ClientInfo = info;
                break;
            }
        }
        PlayerMap[id] = p;
    }

    private void Awake()
    {
        _collider2D = GetComponent<Collider2D>();
        _rigid = GetComponent<Rigidbody2D>();
    }

    private void OnDestroy()
    {
        PlayerMap.Remove(ClientInfo.UID);
    }

    protected override void Start()
    {
        base.Start();
        Skills = new PlayerSkill[SkillKeys.Length];
        for (int i = 0; i < Skills.Length; i++) Skills[i] = null;

        _beforePos = _smoothPos = transform.position;
        _beforeRotZ = _smoothRotZ = _playerRenderer.transform.rotation.eulerAngles.z;

        _nameTextUI.SetText(ClientInfo.Nickname);

        if (IsSelf)
        {
            RegisterSelf().Forget();
        }
        else
        {
            hpBar.HpColor = Color.red;
        }

        RemoveNearStructures();
    }

    public void RemoveNearStructures()
    {
        foreach(var collider in Physics2D.OverlapCircleAll(transform.position, 10f))
        {
            if(collider.TryGetComponent(out Structure structure))
            {
                Destroy(structure.gameObject);
            }
        }
    }

    public void AddEffect(Effect effect)
    {
        if (effect == null) return;
        Effect removeTarget = null;
        foreach(var eff in _effects)
        {
            if(eff.Type == effect.Type)
            {
                if (eff.Level > effect.Level || (eff.Level == effect.Level && eff.Duration > effect.Duration))
                    return;
                removeTarget = eff;
                break;
            }
        }
        if(removeTarget != null) RemoveEffect(removeTarget);
        _effects.Add(effect);
        effect.Type.OnStart(this, effect);
    }

    public void RemoveEffect(Predicate<Effect> filter) {
        foreach(var eff in _effects)
        {
            if (filter(eff)) eff.Type.OnFinish(this, eff);
        }
        _effects.RemoveAll(filter);
    }

    public void RemoveEffect(Effect effect)
    {
        RemoveEffect(eff => eff == effect);
    }

    public void RemoveEffect(EffectType type)
    {
        RemoveEffect(eff => eff.Type == type);
    }

    private void UpdateEffect()
    {
        foreach(var eff in _effects)
        {
            eff.Type.OnUpdate(this, eff);
        }
    }

    private async UniTask RegisterSelf()
    {
        await UniTask.WaitUntil(() => GameManager.Instance != null);
        GameManager.Instance.SelfPlayer = this;
    }

    protected override void Update()
    {
        base.Update();
        MountedWeapon?.Update(this);

        // is self player
        if (IsSelf)
        {
            SelfMoveUpdate();
            SelfRotationUpdate();
            SelfSkillUpdate();
            SelfUnsafeAreaUpdate();
            SelfWeaponUpdate();

            if ((_sendSyncPacketTimer -= Time.deltaTime) <= 0)
            {
                _sendSyncPacketTimer += SyncDelay;

                if ((transform.position - _beforePos).sqrMagnitude > 0.01f)
                    SyncPos();

                if (!Mathf.Approximately(_beforeMana, Mana))
                    SyncMana();

                if (!Mathf.Approximately(_beforeHp, HP))
                    SyncHP();

                if (!Mathf.Approximately(_beforeRotZ, _playerRenderer.transform.rotation.eulerAngles.z))
                    SyncRot();
            }

            if (HP <= 0 && NetworkManager.Instance.PingData.Clients.Length >= 2) NetworkManager.Instance.LeaveRoom();

            HP += Stat.Get(StatType.HPRegen) * Time.deltaTime;
            Mana += Stat.Get(StatType.ManaRegen) * Time.deltaTime;
        }
        else
        {
            MoveSyncUpdate();
        }

        RotationSmoothUpdate();
    }

    private void SelfWeaponUpdate() 
    {
        if (Input.GetMouseButton(0))
        {
            MountedWeapon?.Use(this);
        }
    }

    private void SelfUnsafeAreaUpdate()
    {
        var distance2d = Physics2D.Distance(_collider2D, GameManager.Instance.SafeArea.Collider);
        // 자기장 밖에 있을 때
        if (distance2d.distance > -_collider2D.bounds.size.x)
        {
            GameManager.Instance.UIManager.ShowDamageScreen(true);
            Damage((MaxHP * 0.05f) * Time.deltaTime);
        }
        else GameManager.Instance.UIManager.ShowDamageScreen(false);
    }
    private void SelfSkillUpdate()
    {
        for (int i = 0; i < SkillKeys.Length; ++i)
        {
            if (Input.GetKeyDown(SkillKeys[i])) Skills[i]?.Active(this);
            Skills[i]?.Update(this);
        }
    }

    private void SelfMoveUpdate()
    {
        float xAxis = Input.GetAxisRaw("Horizontal");
        float yAxis = Input.GetAxisRaw("Vertical");

        _rigid.velocity = new Vector3(xAxis, yAxis).normalized * Stat.Get(StatType.MoveSpeed);
    }

    private void SelfRotationUpdate()
    {
        Vector2 mouseDir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.transform.position;
        _smoothRotZ = Mathf.Atan2(mouseDir.y, mouseDir.x) * Mathf.Rad2Deg;
    }

    private void MoveSyncUpdate()
    {
        _rigid.velocity = Vector2.zero;
        transform.position = Vector3.SmoothDamp(transform.position, _smoothPos, ref _smoothVel, SyncDelay);
    }

    private void RotationSmoothUpdate()
    {
        var angles = _playerRenderer.transform.rotation.eulerAngles;
        angles.z = Mathf.MoveTowardsAngle(angles.z, _smoothRotZ, 360 * Time.deltaTime * 2);
        _playerRenderer.transform.rotation = Quaternion.Euler(angles);
        _playerRenderer.sprite = IsSelf ? _playerSelfSprite : _playerOtherSprite;
    }

    public void OnMove(Vector3 target)
    {
        _smoothPos = target;
    }

    public void OnRotate(float z)
    {
        _smoothRotZ = z;
    }

    public override void Damage(AttackParams attackParams, bool showDamage = true)
    {
        if (!IsSelf) return;
        base.Damage(attackParams, showDamage);
        GameManager.Instance.UIManager.ShowDamageScreen();
    }

    public override void Damage(float amount)
    {
        if (!IsSelf) return;
        base.Damage(amount);
    }

    private void SyncHP()
    {
        _beforeHp = HP;
        NetworkManager.Instance.SendPacket("others", "player-set-hp", string.Format("{0:0.000}", HP));
    }

    public void SyncMana(float mana)
    {
        this.mana = mana;
    } 

    private void SyncMana()
    {
        _beforeMana = Mana;
        NetworkManager.Instance.SendPacket("others", "player-set-mana", string.Format("{0:0.000}", Mana));
    }

    private void SyncPos()
    {
        _beforePos = transform.position;
        NetworkManager.Instance.SendPacket("others", "move-player",
            string.Format("{0:0.000}:{1:0.000}", transform.position.x, transform.position.y));
    }

    private void SyncRot()
    {
        _beforeRotZ = _playerRenderer.transform.rotation.eulerAngles.z;

        NetworkManager.Instance.SendPacket("others", "rotate-player",
            string.Format("{0:0.000}", _beforeRotZ));
    }

    public static Player[] GetPlayers()
    {
        return PlayerMap.Values.ToArray();
    }
}