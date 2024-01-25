using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObtainPanelUI : MonoBehaviour
{
    public ObtainItemUI[] obtainItems;

    public bool IsOpened { get => gameObject.activeSelf || _isOpening; }

    private bool _isOpening = false;
     
    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void Open()
    {
        if(_isOpening) return;
        _isOpening = true;
        OpenTask().Forget();
    }

    public async UniTask OpenTask()
    {
        await UniTask.WaitUntil(() => GameManager.Instance != null && GameManager.Instance.SelfPlayer != null);

        var alreadyOwnedSkills = new HashSet<PlayerSkillData>();

        foreach(var skill in GameManager.Instance.SelfPlayer.GetSkills())
        {
            if (skill == null) continue;
            alreadyOwnedSkills.Add(skill.Data);
        }

        var skills = await PlayerSkillDatabase.GetAllData();
        var weapons = await WeaponDatabase.GetAllData();
        List<ObtainableObject> obtainables = new();

        foreach (var skillData in skills)
        {
            if(alreadyOwnedSkills.Contains(skillData)) continue;
            obtainables.Add(skillData);
        }
        foreach (var weaponData in weapons)
        {
            if (weaponData == GameManager.Instance.SelfPlayer.MountedWeapon?.Data) continue;
            obtainables.Add(weaponData);
        }

        int upgradeIndex = -1;
        if (Random.value < 0.3f) upgradeIndex = Random.Range(0, obtainItems.Length);

        int count = Mathf.Min(obtainables.Count, obtainItems.Length);
        var playerSkills = GameManager.Instance.SelfPlayer.GetSkills();
        for (int i = 0; i < obtainItems.Length; ++i)
        {
            obtainItems[i].gameObject.SetActive(i < count);
            if (i >= count) continue;

            var idx = Random.Range(0, obtainables.Count);
            var obtainableObject = obtainables[idx];
            obtainables.RemoveAt(idx);

            // 30% 확률로 칸 중 하나를 업그레이드용 칸으로 교체
            if (i == upgradeIndex && alreadyOwnedSkills.Count > 0)
            {
                var ownedSkillDataArr = alreadyOwnedSkills.ToArray();
                var upgradeData = ownedSkillDataArr[Random.Range(0, ownedSkillDataArr.Length)];
                obtainableObject = upgradeData;
                alreadyOwnedSkills.Remove(upgradeData);
            }

            obtainItems[i].DescriptionText.SetText(obtainableObject.DescriptionSummary);
            obtainItems[i].IconImage.sprite = obtainableObject.Icon;
            obtainItems[i].OutlinedIconImage.sprite = obtainableObject.Icon;

            obtainItems[i].IconImage.gameObject.SetActive(obtainableObject is not PlayerSkillData);
            obtainItems[i].OutlinedIconObject.gameObject.SetActive(obtainableObject is PlayerSkillData);

            if (obtainableObject is PlayerSkillData skillData)
            {
                bool isUpgrade = false;
                int targetIdx = 0;
                for (int j = 0; j < GameManager.Instance.SelfPlayer.SkillLength; j++)
                {
                    if (playerSkills[j] is not null && playerSkills[j].Data == skillData)
                    {
                        isUpgrade = true;
                        targetIdx = j;
                        break;
                    }
                }

                if (!isUpgrade)
                {
                    bool isNewSkill = false;
                    for (int j = 0; j < playerSkills.Length; j++)
                    {
                        if (playerSkills[j] is null)
                        {
                            isNewSkill = true;
                            targetIdx = j;
                            obtainItems[i].ObtainTypeText.SetText("스킬 (신규)");
                            break;
                        }
                        if (playerSkills[targetIdx].ObtainTime > playerSkills[j].ObtainTime)
                        {
                            targetIdx = j;
                        }
                    }

                    if (!isNewSkill)
                    {
                        obtainItems[i].ObtainTypeText.SetText($"스킬 (교체-{GameManager.Instance.SelfPlayer.SkillKeys[targetIdx]})");
                    }
                }
                else
                {
                    obtainItems[i].ObtainTypeText.SetText("스킬 (강화)");

                }

                obtainItems[i].OnObtain = () =>
                {
                    if (!isUpgrade) GameManager.Instance.SelfPlayer.SetSkill(targetIdx, new PlayerSkill(skillData));
                    else playerSkills[targetIdx].Level++;

                    var newSkill = GameManager.Instance.SelfPlayer.GetSkills()[targetIdx];
                    NetworkManager.Instance.SendPacket("others", "change-skill", targetIdx + ":" + newSkill.Data.Name + ":" + newSkill.Level);
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
        _isOpening = false;
    }
}
