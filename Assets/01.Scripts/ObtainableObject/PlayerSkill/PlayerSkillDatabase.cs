using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class PlayerSkillDatabase
{
    private static Dictionary<string, PlayerSkillData> dataMap = null; 

    private static async UniTask LoadAllData()
    {
        if (dataMap is null)
        {
            var loadTask = await Addressables.LoadAssetsAsync<PlayerSkillData>("skills", _ => { }).Task;
            dataMap = new();
            foreach (var data in loadTask)
            {
                if(data.Name is not null) dataMap[data.Name] = data;
            }
        }
    }

    public static async UniTask<PlayerSkillData[]> GetAllData()
    {
        await LoadAllData();
        return dataMap.Values.ToArray();
    }

    public static async UniTask<PlayerSkillData> GetSkill(string skillName)
    {
        await LoadAllData();
        return dataMap.TryGetValue(skillName, out var data) ? data : null;
    }
}
