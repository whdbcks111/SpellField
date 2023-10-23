using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon
{
    public WeaponData Data;
    public float CurrentCooldown;

    private readonly Dictionary<string, object> _extraData = new();

    public Weapon(WeaponData data)
    {
        Data = data;
        CurrentCooldown = 0f;
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

    public void OnMount(Player p)
    {
        WeaponDisplay display = UnityEngine.Object.Instantiate(Data.DisplayObject, p.Hand.transform);
        display.OnTriggerStay2DEvent = collider2d => Data.OnWeaponTriggerStay(collider2d, p, this);
        Data.OnMount(p, this);
    }

    public void OnUnmount(Player p)
    {
        Data.OnUnmount(p, this);
        foreach(Transform t in p.Hand.transform)
        {
            UnityEngine.Object.Destroy(t.gameObject);
        }
    }

    public void Use(Player p)
    {
        if (p.IsSelf)
        {
            if (CurrentCooldown > 0)
            {
                return;
            }
            CurrentCooldown = Data.Cooldown;
            NetworkManager.Instance.SendPacket("others", "use-weapon", "");
            Data.Use(p, this);
        }
        else Data.Use(p, this);
    }

    public void Update(Player p)
    {
        if (CurrentCooldown > 0f) CurrentCooldown -= Time.deltaTime;
        Data.UpdateWeapon(p, this);
    }
}
