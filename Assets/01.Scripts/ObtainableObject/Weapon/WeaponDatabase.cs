using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class WeaponDatabase
{
    private static Dictionary<string, WeaponData> dataMap = null;

    private static async UniTask LoadAllData()
    {
        if (dataMap is null)
        {
            var loadTask = await Addressables.LoadAssetsAsync<WeaponData>("weapons", _ => { }).Task;
            dataMap = new();
            foreach (var data in loadTask)
            {
                if (data.Name is not null) dataMap[data.Name] = data;
            }
        }
    }

    public static async UniTask<WeaponData[]> GetAllData()
    {
        await LoadAllData();
        return dataMap.Values.ToArray();
    }

    public static async UniTask<WeaponData> GetWeapon(string weaponName)
    {
        await LoadAllData();
        return dataMap.TryGetValue(weaponName, out var data) ? data : null;
    }
}
