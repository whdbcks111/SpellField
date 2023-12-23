using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static UnityEngine.RuleTile.TilingRuleOutput;
using System;

public class Damageable : MonoBehaviour
{
    public readonly Stat Stat = new();
    private readonly List<ShieldAmount> _shields = new();
    [SerializeField] protected HPBar hpBar;
    [HideInInspector] public Player LastAttacker = null;

    private readonly Queue<Action> _lateTasks = new(); 

    protected float hp = StatType.MaxHP.DefaultValue;
    public virtual float HP
    {
        get { return hp; }
        set
        {
            hp = Mathf.Clamp(value, 0f, MaxHP);
        }
    }

    public virtual float MaxHP
    {
        get => Stat.Get(StatType.MaxHP);
    }

    protected float mana = StatType.MaxMana.DefaultValue;
    public virtual float Mana
    {
        get { return mana; }
        set
        {
            mana = Mathf.Clamp(value, 0f, MaxMana);
        }
    }

    public virtual float MaxMana
    {
        get => Stat.Get(StatType.MaxMana);
    }

    public float ShieldAmount
    {
        get
        {
            float amount = 0f;
            foreach (var shield in _shields) amount += shield.Value;
            return amount;
        }
    }

    public virtual void AddShield(float amount, float time)
    {
        _shields.Add(new()
        {
            Value = amount,
            Time = time
        });
    }

    public void RunOnceLateUpdate(Action action)
    {
        _lateTasks.Enqueue(action);
    }

    public void SyncHP(float hp)
    {
        this.hp = hp;
    }

    protected virtual void Start()
    {
        RunOnceLateUpdate(() => {
            HP = MaxHP;
            Mana = MaxMana;
        });
    }

    protected virtual void Update()
    {
        _shields.RemoveAll(shield => (shield.Time -= Time.deltaTime) < 0f);

        hpBar.HP = HP;
        hpBar.MaxHP = MaxHP;
        hpBar.Shield = ShieldAmount;
    }

    public virtual void Damage(AttackParams attackParams, Player attacker = null, bool showDamage = true)
    {
        DamageParams damageParams = Stat.GetDamageParams(attackParams, this);
        Damage(damageParams.TotalDamage, attacker);
        if (showDamage)
        {
            GameManager.Instance.ShowDamage(transform.position, damageParams);
        }
    }

    public virtual void Damage(float amount, Player attacker = null)
    {
        LastAttacker = attacker;
        foreach (var shield in _shields)
        {
            if (shield.Value > amount)
            {
                shield.Value -= amount;
                amount = 0f;
                break;
            }
            else
            {
                amount -= shield.Value;
                shield.Value = 0f;
            }
        }
        _shields.RemoveAll(shield => shield.Value <= 0f);

        HP -= amount;
    }

    protected virtual void LateUpdate()
    {
        Stat.LateUpdate();
        while(_lateTasks.Count > 0)
        {
            _lateTasks.Dequeue().Invoke();
        }
    }
}

public class ShieldAmount
{
    public float Value, Time;
}