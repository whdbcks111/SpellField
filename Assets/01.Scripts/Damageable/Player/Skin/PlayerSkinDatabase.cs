using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;

public static class PlayerSkinDatabase
{
    public const string LocalSkinDataKey = "skin";
    private static Dictionary<string, SkinData> dataMap = null;

    private static async UniTask LoadAllData()
    {
        if (dataMap is null)
        {
            var loadTask = await Addressables.LoadAssetsAsync<SkinData>("skins", _ => { }).Task;
            dataMap = new();
            foreach (var data in loadTask)
            {
                if (data.Name is not null) dataMap[data.Name] = data;
            }
        }
    }

    public static async UniTask<SkinData[]> GetAllData()
    {
        await LoadAllData();
        return dataMap.Values.ToArray();
    }

    public static async UniTask<SkinData> GetSkin(string skinName)
    {
        await LoadAllData();
        return dataMap.TryGetValue(skinName, out var data) ? data : null;
    }
}