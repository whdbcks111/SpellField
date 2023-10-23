using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObtainPanelUI : MonoBehaviour
{
    public ObtainItemUI[] obtainItems;

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void Open()
    {
        OpenTask().Forget();
    }

    public async UniTask OpenTask()
    {
        await UniTask.WaitUntil(() => GameManager.Instance is not null && GameManager.Instance.SelfPlayer is not null);

        var skills = await PlayerSkillDatabase.GetAllData();
        var weapons = await WeaponDatabase.GetAllData();
        List<ObtainableObject> obtainables = new();

        foreach (var skill in skills) obtainables.Add(skill);
        foreach (var weapon in weapons) obtainables.Add(weapon);

        int count = Mathf.Min(obtainables.Count, obtainItems.Length);
        for (int i = 0; i < obtainItems.Length; ++i)
        {
            obtainItems[i].gameObject.SetActive(i < count);
            if (i >= count) continue;

            var idx = Random.Range(0, obtainables.Count);
            var obtainableObject = obtainables[idx];
            obtainables.RemoveAt(idx);

            obtainItems[i].DescriptionText.SetText(obtainableObject.DescriptionSummary);
            obtainItems[i].IconImage.sprite = obtainableObject.Icon;

            if (obtainableObject is PlayerSkillData skillData)
            {
                bool isUpgrade = false;
                int targetIdx = 0;
                for (int j = 0; j < GameManager.Instance.SelfPlayer.Skills.Length; j++)
                {
                    if (GameManager.Instance.SelfPlayer.Skills[j] is not null && GameManager.Instance.SelfPlayer.Skills[j].Data == skillData)
                    {
                        isUpgrade = true;
                        targetIdx = j;
                        break;
                    }
                }

                if (!isUpgrade)
                {
                    bool isNewSkill = false;
                    for (int j = 0; j < GameManager.Instance.SelfPlayer.Skills.Length; j++)
                    {
                        if (GameManager.Instance.SelfPlayer.Skills[j] is null)
                        {
                            isNewSkill = true;
                            targetIdx = j;
                            obtainItems[i].ObtainTypeText.SetText("스킬 (신규)");
                            break;
                        }
                        if (GameManager.Instance.SelfPlayer.Skills[targetIdx].ObtainTime > GameManager.Instance.SelfPlayer.Skills[j].ObtainTime)
                        {
                            targetIdx = j;
                        }
                    }

                    if (!isNewSkill)
                    {
                        obtainItems[i].ObtainTypeText.SetText("스킬 (교체-" + GameManager.Instance.SelfPlayer.Skills[targetIdx].Data.Name + ")");
                    }
                }
                else
                {
                    obtainItems[i].ObtainTypeText.SetText("스킬 (강화)");

                }

                obtainItems[i].OnObtain = () =>
                {
                    if (!isUpgrade) GameManager.Instance.SelfPlayer.Skills[targetIdx] = new PlayerSkill(skillData);
                    else GameManager.Instance.SelfPlayer.Skills[targetIdx].Level++;

                    NetworkManager.Instance.SendPacket("others", "change-skill", targetIdx + ":" + GameManager.Instance.SelfPlayer.Skills[targetIdx].Data.Name + ":" + GameManager.Instance.SelfPlayer.Skills[targetIdx].Level);
                    Close();
                };
            }
            else if (obtainableObject is WeaponData weaponData)
            {
                obtainItems[i].ObtainTypeText.SetText("무기 " + (GameManager.Instance.SelfPlayer.MountedWeapon is null ? "(신규)" : "(교체)"));

                obtainItems[i].OnObtain = () =>
                {
                    GameManager.Instance.SelfPlayer.MountedWeapon = new Weapon(weaponData);

                    NetworkManager.Instance.SendPacket("others", "change-weapon", weaponData.Name);
                    Close();
                };
            }
        }

        gameObject.SetActive(true);
    }
}
