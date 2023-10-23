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
        print(prefab);
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

    private void OnDestroy()
    {
        StructureMap[StructureId] = null;
    }

    private void SyncHP()
    {
        _beforeHp = HP;
        NetworkManager.Instance.SendPacket("others", "structure-set-hp", $"{StructureId}:{HP:0.000}");
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
        }

        if (HP <= 0) Destroy(gameObject);
    }

    public override void Damage(AttackParams attackParams, bool showDamage = true)
    {
        if (!NetworkManager.Instance.PingData.IsMasterClient) return;
        base.Damage(attackParams, showDamage);
    }

    public override void Damage(float amount)
    {
        if (!NetworkManager.Instance.PingData.IsMasterClient) return;
        base.Damage(amount);
    }
}