using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : Damageable
{
    public static readonly string PlayerLayer = "Player", InvisibleLayer = "Invisible";

    public static readonly float SyncDelay = 0.03f;
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

    [SerializeField] private CanvasGroup _worldCanvasGroup;
    [SerializeField] public KeyCode[] SkillKeys;
    [HideInInspector] private PlayerSkill[] _skills;

    public int SkillLength
    {
        get => _skills.Length;
    }

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

    public ParticleSystem FlameParticle, BloodParticle;
    [SerializeField] private TintColor _tint;
    [SerializeField] private float _damageTintTime;
    [SerializeField] private TextMeshProUGUI _nameTextUI;
    [SerializeField] private SpriteRenderer _playerRenderer;
    [SerializeField] private PlayerHand _hand;
    [SerializeField] private GameObject _survivalObject, _spectatorObject;
    private Weapon _mountedWeapon = null;

    private Rigidbody2D _rigid;
    private Collider2D _collider2D;
    private Vector3 _smoothPos, _smoothVel;
    private Vector3 _beforePos;
    private float _beforeRotZ, _smoothRotZ;
    private float _sendSyncPacketTimer = 0f;
    private float _damageTintTimer = 0f;
    private Vector2 _knockbackForce = Vector2.zero;

    private Color _originalColor;
    private readonly Dictionary<string, ColorModifier> _colorModifiers = new();
    private readonly Dictionary<string, ColorModifier> _tintModifiers = new();
    private readonly Dictionary<PlayerState, float> _states = new();

    private readonly List<Effect> _effects = new();

    private readonly List<Action> _eventDisposeActions = new();

    public GameMode Mode = GameMode.Survival;

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
        _originalColor = _playerRenderer.color;
        _collider2D = GetComponent<Collider2D>();
        _rigid = GetComponent<Rigidbody2D>();

        InitSkin().Forget();
    }

    private async UniTask InitSkin()
    {
        await UniTask.WaitUntil(() => ClientInfo.UID?.Length > 0);
        if (NetworkManager.Instance.PingData.RoomState.TryGetValue(
            NetworkClient.GetPlayerSkinStateKey(ClientInfo.UID), out var skinName))
        {
            SetSkin(skinName).Forget();
        }
    }

    private async UniTask SetSkin(string skinName)
    {
        var skinData = await PlayerSkinDatabase.GetSkin(skinName);
        if(skinData != null) SetSkin(skinData);
    }

    public void AddState(PlayerState state, float duration)
    {
        if(_states.ContainsKey(state))
        {
            _states[state] = Mathf.Max(_states[state], duration);
        }
        else
        {
            _states.Add(state, duration);
        }
    }

    public void SetSkill(int idx, PlayerSkill skill)
    {
        if (_skills[idx] != null && _skills[idx].Data == skill?.Data)
        {
            _skills[idx].Level = skill.Level;
        }
        else
        {
            _skills[idx]?.Data.OnReplace(this, _skills[idx]);
            _skills[idx] = skill;
            _skills[idx]?.Data.OnObtain(this, _skills[idx]);
        }
    }

    public PlayerSkill[] GetSkills()
    {
        PlayerSkill[] clone = new PlayerSkill[_skills.Length];
        for (int i = 0; i < _skills.Length; i++) clone[i] = _skills[i];
        return clone;
    }

    public bool IsInState(PlayerState state)
    {
        return _states.ContainsKey(state);
    }

    public void AddColorModifier(string key, Color color, float duration)
    {
        if (_colorModifiers.ContainsKey(key))
        {
            _colorModifiers[key].Color = color;
            _colorModifiers[key].Duration = duration;
        }
        else _colorModifiers[key] = new() { Color = color, Duration = duration };
    }

    public void AddTintColorModifier(string key, Color color, float duration)
    {
        if (_tintModifiers.ContainsKey(key))
        {
            _tintModifiers[key].Color = color;
            _tintModifiers[key].Duration = duration;
        }
        else _tintModifiers[key] = new() { Color = color, Duration = duration };
    }

    public void RemoveColorModifier(string key)
    {
        _colorModifiers.Remove(key);
    }

    public void RemoveTintColorModifier(string key)
    {
        _tintModifiers.Remove(key);
    }

    public override void AddShield(float amount, float time)
    {
        AddShield(amount, time, true);
    }

    public void AddShield(float amount, float time, bool sync)
    {
        if (!sync)
        {
            if (IsSelf)
            {
                NetworkManager.Instance.SendPacket("others", "add-shield", new(amount, time));
            }
            else return;
        }
        base.AddShield(amount, time);
    }

    protected override void Start()
    {
        base.Start();
        _skills = new PlayerSkill[SkillKeys.Length];
        for (int i = 0; i < _skills.Length; i++) _skills[i] = null;

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

        _eventDisposeActions.Add(NetworkManager.Instance.On("move-player", OnMoveEvent));
        _eventDisposeActions.Add(NetworkManager.Instance.On("player-death", OnDeathEvent));
        _eventDisposeActions.Add(NetworkManager.Instance.On("rotate-player", OnRotateEvent));
        _eventDisposeActions.Add(NetworkManager.Instance.On("player-set-hp", OnSetHPEvent));
        _eventDisposeActions.Add(NetworkManager.Instance.On("player-set-mana", OnSetManaEvent));
        _eventDisposeActions.Add(NetworkManager.Instance.On("active-skill", OnActiveSkillEvent));
        _eventDisposeActions.Add(NetworkManager.Instance.On("add-effect", OnAddEffectEvent));
        _eventDisposeActions.Add(NetworkManager.Instance.On("add-shield", OnAddShieldEvent));
        _eventDisposeActions.Add(NetworkManager.Instance.On("damage-player", OnDamageEvent));
        _eventDisposeActions.Add(NetworkManager.Instance.On("start-charge-skill", OnStartChargeSkillEvent));
        _eventDisposeActions.Add(NetworkManager.Instance.On("use-weapon", OnUseWeaponEvent));
        _eventDisposeActions.Add(NetworkManager.Instance.On("change-weapon", OnChangeWeaponEvent));
        _eventDisposeActions.Add(NetworkManager.Instance.On("change-skill", OnChangeSkillEvent));
    }

    private void OnChangeWeaponEvent(string from, Packet packet)
    {
        if (from != ClientInfo.UID) return;

        ChangeWeapon(packet.NextString()).Forget();
    }

    private void OnChangeSkillEvent(string from, Packet packet)
    {
        if (from != ClientInfo.UID) return;

        var idx = packet.NextInt();
        var skillName = packet.NextString();
        var level = packet.NextInt();

        ChangeSkill(idx, skillName, level).Forget();
    }

    private async UniTask ChangeSkill(int index, string skillName, int level)
    {
        PlayerSkillData data = await PlayerSkillDatabase.GetSkill(skillName);
        if (data != null)
        {
            SetSkill(Mathf.Clamp(index, 0, SkillLength - 1), new PlayerSkill(data, level));
        }
    }

    private void OnUseWeaponEvent(string from, Packet packet)
    {
        if (from != ClientInfo.UID) return;

        MountedWeapon?.Use(this);
    }

    private async UniTask ChangeWeapon(string weaponName)
    {
        WeaponData data = await WeaponDatabase.GetWeapon(weaponName);
        if (data != null) MountedWeapon = new Weapon(data);
    }

    private void OnStartChargeSkillEvent(string from, Packet packet)
    {
        if (from != ClientInfo.UID) return;
        var message = packet.NextString();

        foreach (var skill in GetSkills())
        {
            if (skill is not null && skill.Data.Name.Equals(message)) 
                skill.StartCharge(this);
        }
    }

    private void OnDamageEvent(string from, Packet packet)
    {
        if (from != ClientInfo.UID) return;
        var amount = packet.NextFloat();
        var attackerId = packet.NextString();

        if (PlayerMap.TryGetValue(attackerId, out var attacker))
        {
            Damage(amount, attacker, true);
        }
    }

    private void OnAddEffectEvent(string from, Packet packet)
    {
        if (from != ClientInfo.UID) return;
        var effTypeName = packet.NextString();
        var effType = EffectType.GetByName(effTypeName);
        var level = packet.NextInt();
        var duration = packet.NextFloat();
        var casterId = packet.NextString();

        if (effType != null)
        {
            PlayerMap.TryGetValue(casterId, out Player caster);
            AddEffect(new(effType, level, duration, caster), true);
        }
    }

    private void OnAddShieldEvent(string from, Packet packet)
    {
        if (from != ClientInfo.UID) return;

        var amount = packet.NextFloat();
        var time = packet.NextFloat();

        AddShield(amount, time, true);
    }

    private void OnActiveSkillEvent(string from, Packet packet)
    {
        if (from != ClientInfo.UID) return;
        var skillName = packet.NextString();

        foreach (var skill in GetSkills())
        {
            if (skill is not null && skill.Data.Name.Equals(skillName)) skill.Active(this);
        }
    }

    private void OnMoveEvent(string from, Packet packet)
    {
        if (from != ClientInfo.UID) return;

        _smoothPos = packet.NextVector2();
    }

    private void OnSetHPEvent(string from, Packet packet)
    {
        if (from != ClientInfo.UID) return;

        SyncHP(packet.NextFloat());
    }

    private void OnSetManaEvent(string from, Packet packet)
    {
        if (from != ClientInfo.UID) return;

        SyncMana(packet.NextFloat());
    }

    private void OnRotateEvent(string from, Packet packet)
    {
        if (from != ClientInfo.UID) return;

        OnRotate(packet.NextFloat());

    }

    private void OnDeathEvent(string from, Packet _)
    {
        if (from != ClientInfo.UID) return;

        OnDeath();
    }

    private void OnDestroy()
    {
        PlayerMap.Remove(ClientInfo.UID);

        foreach (var dispose in _eventDisposeActions)
        {
            dispose();
        }
    }

    public void AddEffect(Effect effect, bool sync = false)
    {
        if (effect == null) return;
        if(!sync)
        {
            if (IsSelf)
            {
                NetworkManager.Instance.SendPacket("others", "add-effect", 
                    new(effect.Type.Name, effect.Level, effect.Duration,
                        effect.Caster == null ? null : effect.Caster.ClientInfo.UID));
            }
            else return;
        }

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

    public bool HasEffect(EffectType type)
    {
        return _effects.Any(e => e.Type == type);
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

    private void UpdateEffects()
    {
        foreach(var eff in _effects)
        {
            eff.Duration -= Time.deltaTime;
            eff.Type.OnUpdate(this, eff);
        }

        RemoveEffect(eff => eff.Duration < 0f);
    }

    private async UniTask RegisterSelf()
    {
        await UniTask.WaitUntil(() => GameManager.Instance != null);
        GameManager.Instance.SelfPlayer = this;
    }

    protected override void Update()
    {
        base.Update();
        if (IsSelf) gameObject.name = "Player_SELF";


        _collider2D.enabled = Mode == GameMode.Survival;
        _survivalObject.SetActive(Mode == GameMode.Survival);
        _spectatorObject.SetActive(Mode == GameMode.Spectator);

        if (Mode != GameMode.Spectator) MountedWeapon?.Update(this);
        else MountedWeapon = null;

        if(Mode != GameMode.Spectator)
        {
            SkillUpdate();
        }
        else
        {
            for (int i = 0; i < _skills.Length; i++)
            {
                if (_skills[i] != null)
                    SetSkill(i, null);
            }
        }

        // is self player
        if (IsSelf)
        {
            SelfMoveUpdate();
            SelfRotationUpdate();
            if (Mode != GameMode.Spectator)
            {
                SelfUnsafeDamageCheck();
                SelfWeaponUpdate();
            }

            if ((_sendSyncPacketTimer -= Time.deltaTime) <= 0)
            {
                _sendSyncPacketTimer += SyncDelay;

                if (!Mathf.Approximately(_beforeMana, Mana))
                    SyncMana();

                if (!Mathf.Approximately(_beforeHp, HP))
                    SyncHP();

                if ((_beforePos - transform.position).sqrMagnitude > 0.01f)
                    SyncPos();

                if (!Mathf.Approximately(_beforeRotZ, _playerRenderer.transform.rotation.eulerAngles.z))
                    SyncRot();
            }

            if (HP <= 0 && Mode != GameMode.Spectator)
            {
                NetworkManager.Instance.SendPacket("others", "player-death", new(""));
                OnDeath();
            }

            if (Mode == GameMode.Spectator)
            {
                HP = 0;
                Mana = 0;
                Stat.Add(StatType.MoveSpeed, 10);
            }
            else
            {
                HP += Stat.Get(StatType.HPRegen) * Time.deltaTime;
                Mana += Stat.Get(StatType.ManaRegen) * Time.deltaTime;
            }
        }
        else
        {
            MoveSyncUpdate();
        }

        UpdateVisible();
        UpdateEffects();
        RotationSmoothUpdate();
        UpdateColorModifiers();
        UpdatePlayerStates();
        UpdateDamageTint();
        UpdateTintColorModifiers();
    }

    public void SetSkin(SkinData data)
    {
        PlayerRenderer.sprite = data.PlayerSprite;
        Hand.Renderer.sprite = data.HandSprite;
    }

    private void UpdateVisible()
    {
        bool isInvisible = IsInState(PlayerState.Invisible);
        float alpha = 1f;
        if(isInvisible)
        {
            if (IsSelf) alpha = 0.1f;
            else alpha = 0f;
        }


        AddColorModifier("visibility", new Color(1f, 1f, 1f, alpha), 0f);
        SetRendererAlpha(Hand.Renderer, alpha);
        _worldCanvasGroup.alpha = alpha;
        if(MountedWeapon != null) SetRendererAlpha(MountedWeapon.Display.SpriteRenderer, alpha);
        
    }

    private void SetRendererAlpha(SpriteRenderer renderer, float alpha)
    {
        var col = renderer.color;
        col.a = alpha;
        renderer.color = col;
    }

    private void UpdateDamageTint()
    {
        if(_damageTintTimer > 0f)
        {
            _damageTintTimer -= Time.deltaTime;
            var col = _tint.Color;
            col.a = 0.9f * Mathf.Clamp01(_damageTintTimer / _damageTintTime);
            AddTintColorModifier("damage", col, 0.1f);
        }
    }

    private void UpdateTintColorModifiers()
    {
        _tint.Color = new Color(1f, 1f, 1f, 0f);
        float alpha = 0f;
        foreach (var key in _tintModifiers.Keys.ToArray())
        {
            var modifier = _tintModifiers[key];
            modifier.Duration -= Time.deltaTime;
            _tint.Color *= modifier.Color;
            if(alpha < modifier.Color.a) alpha = modifier.Color.a;

            if (modifier.Duration <= 0) _tintModifiers.Remove(key);
        }
        _tint.Color += new Color(0, 0, 0, Mathf.Clamp01(alpha));
    }

    private void UpdatePlayerStates()
    {
        foreach(var key in _states.Keys.ToArray())
        {
            _states[key] -= Time.deltaTime;
            if (_states[key] <= 0f) _states.Remove(key);
        }
    }

    private void UpdateColorModifiers()
    {
        _playerRenderer.color = _originalColor;
        foreach (var key in _colorModifiers.Keys.ToArray())
        {
            var modifier = _colorModifiers[key];
            modifier.Duration -= Time.deltaTime;
            _playerRenderer.color *= modifier.Color;

            if (modifier.Duration <= 0) _colorModifiers.Remove(key);
        }

    }

    public void OnDeath()
    {
        Mode = GameMode.Spectator;
        MountedWeapon = null;
        for(int i = 0; i < _skills.Length; i++)
        {
            _skills[i]?.Data.OnReplace(this, _skills[i]);
            _skills[i] = null;
        }

        GameManager.Instance.UIManager.ActionBar.ShowActionBar(
            $"{(LastAttacker == null ? null : LastAttacker.ClientInfo.Nickname)} -> " +
            $"<color=red>{ClientInfo.Nickname}</color>", 1f);
    }

    private void SelfWeaponUpdate() 
    {
        if (Input.GetMouseButton(0) && !IsInState(PlayerState.CannotUseWeapon))
        {
            MountedWeapon?.Use(this);
        }
    }

    private void SelfUnsafeDamageCheck()
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
    private void SkillUpdate()
    {
        for (int i = 0; i < SkillKeys.Length; ++i)
        {
            if (_skills[i] == null) continue;
            if(IsSelf && !IsInState(PlayerState.CannotUseSkill))
            {
                if (Input.GetKeyDown(SkillKeys[i])) _skills[i].StartCharge(this);
                if (Input.GetKeyUp(SkillKeys[i])) _skills[i].Active(this);
            }
            _skills[i].Update(this);
        }
    }

    private void SelfMoveUpdate()
    {
        float xAxis = Input.GetAxisRaw("Horizontal");
        float yAxis = Input.GetAxisRaw("Vertical");

        if (IsInState(PlayerState.CannotMove))
        {
            xAxis = 0f;
            yAxis = 0f;
        }

        _knockbackForce = Vector2.MoveTowards(_knockbackForce, Vector2.zero, Time.deltaTime * Physics2D.gravity.magnitude);

        _rigid.velocity = Stat.Get(StatType.MoveSpeed) * new Vector2(xAxis, yAxis).normalized + _knockbackForce;
    }

    public void Knockback(Vector2 force)
    {
        if(_knockbackForce.magnitude < force.magnitude)
            _knockbackForce = force;
    }

    private void SelfRotationUpdate()
    {
        Vector2 mouseDir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.transform.position;

        if (!IsInState(PlayerState.CannotMove))
        {
            _smoothRotZ = Mathf.Atan2(mouseDir.y, mouseDir.x) * Mathf.Rad2Deg;
        }
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
    }

    public void OnRotate(float z)
    {
        _smoothRotZ = z;
    }

    public override void Damage(AttackParams attackParams, Player attacker = null, bool showDamage = true)
    {
        base.Damage(attackParams, attacker, showDamage && IsSelf);
        if (showDamage) _damageTintTimer = _damageTintTime;
        if(IsSelf) GameManager.Instance.UIManager.ShowDamageScreen();
    }

    public override void Damage(float amount, Player attacker = null)
    {
        Damage(amount, attacker, true);
    }

    public void Damage(float amount, Player attacker = null, bool sync = false)
    {
        if (!sync)
        {
            if (IsSelf)
            {
                NetworkManager.Instance.SendPacket("others", "damage-player", new(amount, attacker == null ? null : attacker.ClientInfo.UID));
            }
            else return;
        }
        base.Damage(amount, attacker);
    }

    private void SyncHP()
    {
        _beforeHp = HP;
        NetworkManager.Instance.SendPacket("others", "player-set-hp", new(HP));
    }

    public void SyncMana(float mana)
    {
        this.mana = mana;
    } 

    private void SyncMana()
    {
        _beforeMana = Mana;
        NetworkManager.Instance.SendPacket("others", "player-set-mana", new(Mana));
    }

    private void SyncPos()
    {
        var curPos = transform.position;
        NetworkManager.Instance.SendPacket("others", "move-player",
            new((Vector2)curPos));
        _beforePos = curPos;
    }

    private void SyncRot()
    {
        var curRotZ = _playerRenderer.transform.rotation.eulerAngles.z;

        NetworkManager.Instance.SendPacket("others", "rotate-player", new(curRotZ));
        _beforeRotZ = curRotZ;
    }

    public static Player[] GetPlayers()
    {
        return PlayerMap.Values.ToArray();
    }
}

public enum GameMode
{
    Survival, Spectator
}

public class ColorModifier
{
    public float Duration;
    public Color Color;
}

public enum PlayerState
{ 
    Invisible,
    CannotMove,
    CannotUseWeapon, 
    CannotUseSkill
}