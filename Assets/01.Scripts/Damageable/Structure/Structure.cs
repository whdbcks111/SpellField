using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Structure : Damageable
{
    public const float SyncDelay = 0.2f;

    public static readonly Dictionary<int, Structure> StructureMap = new();
    public static int StructureCounter = 0;

    [SerializeField] private float baseHP;
    [SerializeField] private float baseDefend;
    [SerializeField] private float baseMagicResistance;

    private float _beforeHp = 0f;
    private float _sendSyncPacketTimer = 0f;

    private readonly List<Action> _eventDisposeActions = new();

    public int StructureId;

    public override float HP
    {
        get => base.HP;
        set
        {
            if (!NetworkManager.Instance.PingData.IsMasterClient) return;
            base.HP = value;
        }
    }

    public static void SpawnStructure(Structure prefab, Vector2 pos)
    {
        Instantiate(prefab, pos, Quaternion.identity);
    }

    private void Awake()
    {
        Stat.SetBase(StatType.MaxHP, baseHP);
        Stat.SetBase(StatType.Defend, baseDefend);
        Stat.SetBase(StatType.MagicResistance, baseMagicResistance);

        StructureId = StructureCounter++;
        StructureMap[StructureId] = this;
    }

    protected override void Start()
    {
        base.Start();

        _eventDisposeActions.Add(NetworkManager.Instance.On("structure-set-hp", OnSetHPEvent));
        _eventDisposeActions.Add(NetworkManager.Instance.On("structure-death", OnDeathEvent));
        _eventDisposeActions.Add(NetworkManager.Instance.On("damage-structure", OnDamageEvent));
    }

    private void OnDamageEvent(string _, Packet packet)
    {
        var targetId = packet.NextInt();
        var amount = packet.NextFloat();
        var attackerId = packet.NextString();

        if (StructureId == targetId &&
            Player.PlayerMap.TryGetValue(attackerId, out var attacker))
        {
            Damage(amount, attacker, true);
        }
    }

    private void OnSetHPEvent(string _, Packet packet)
    {
        var targetId = packet.NextInt();
        var amount = packet.NextFloat();
        if (StructureId == targetId)
        {
            SyncHP(amount);
        }
    }

    private void OnDeathEvent(string _, Packet packet)
    {
        var targetId = packet.NextInt();
        if (StructureId == targetId)
        {
            OnDeath();
        }
    }

    protected virtual void OnDestroy()
    {
        foreach (var dispose in _eventDisposeActions)
        {
            dispose();
        }
        StructureMap.Remove(StructureId);
    }

    private void SyncHP()
    {
        _beforeHp = HP;
        NetworkManager.Instance.SendPacket("others", "structure-set-hp", new Packet(StructureId, HP));
    }

    protected override void Update()
    {
        base.Update();

        hpBar.gameObject.SetActive(HP < MaxHP);

        if(NetworkManager.Instance.PingData.IsMasterClient)
        {
            if ((_sendSyncPacketTimer -= Time.deltaTime) <= 0)
            {
                _sendSyncPacketTimer += SyncDelay;

                if (!Mathf.Approximately(_beforeHp, HP))
                    SyncHP();
            }

            if (HP <= 0)
            {
                NetworkManager.Instance.SendPacket("others", "structure-death", new(StructureId));
                OnDeath();
            }
        }
    }

    public void OnDeath()
    {
        Destroy(gameObject);
    }

    public override void Damage(AttackParams attackParams, Player attacker = null, bool showDamage = true)
    {
        base.Damage(attackParams, attacker, showDamage && NetworkManager.Instance.PingData.IsMasterClient);
    }

    public override void Damage(float amount, Player attacker = null)
    {
        Damage(amount, attacker, true);
    }

    public void Damage(float amount, Player attacker = null, bool sync = false)
    {
        if (!sync)
        {
            if (NetworkManager.Instance.PingData.IsMasterClient)
            {
                NetworkManager.Instance.SendPacket("others", "damage-structure", 
                    new(StructureId, amount, attacker == null ? null : attacker.ClientInfo.UID));
            }
            else return;
        }
        base.Damage(amount, attacker);
    }
}