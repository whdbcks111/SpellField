using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill 
{
    public PlayerSkillData Data;
    public float CurrentCooldown;
    public int Level;
    public float ObtainTime;
    public float ChargeTime;
    public bool IsCharging = false;

    private readonly Dictionary<string, object> _extraData = new();

    public PlayerSkill(PlayerSkillData data, int level = 1)
    {
        Data = data;
        CurrentCooldown = 0f;
        Level = level;
        ObtainTime = Time.time;
    }

    public T GetData<T>(string key, T defaultValue)
    {
        try
        {
            return GetData<T>(key);
        }
        catch { return defaultValue; }
    }

    public T GetData<T>(string key)
    {
        if (_extraData.TryGetValue(key, out object value) && value is T t) return t;
        throw new Exception("Key is doesn't matching with type T");
    }

    public void SetData<T>(string key, T value)
    {
        _extraData[key] = value;
    }

    public void StartCharge(Player p)
    {
        if (p.IsSelf)
        {
            if (CurrentCooldown > 0)
            {
                GameManager.Instance.UIManager.ActionBar.ShowActionBar("재사용 대기시간이 남았습니다.", 0.5f);
                return;
            }
            if (p.Mana < Data.GetManaCost(p, this))
            {
                GameManager.Instance.UIManager.ActionBar.ShowActionBar("마나가 부족합니다.", 0.5f);
                return;
            }
            p.Mana -= Data.GetManaCost(p, this);
            CurrentCooldown = Data.GetCooldown(p, this);
            NetworkManager.Instance.SendPacket("others", "start-charge-skill", new(Data.Name));
        }
        IsCharging = true;
        ChargeTime = 0f;
        Data.OnStartCharge(p, this);
    }

    public void Active(Player p)
    {
        if (!IsCharging) return;
        IsCharging = false;
        if(p.IsSelf)
        {
            NetworkManager.Instance.SendPacket("others", "active-skill", new(Data.Name));
            CurrentCooldown = Data.GetCooldown(p, this);
        }
        
        Data.OnActiveUse(p, this);
    }

    public void Update(Player p)
    {
        if(CurrentCooldown > 0f) CurrentCooldown -= Time.deltaTime;
        if (IsCharging)
        {
            ChargeTime += Time.deltaTime;
            CurrentCooldown = Data.GetCooldown(p, this);
            Data.OnCharging(p, this);
        }
        Data.OnPassiveUpdate(p, this);
    }
}
