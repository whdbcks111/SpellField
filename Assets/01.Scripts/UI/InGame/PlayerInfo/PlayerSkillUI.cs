using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerSkillUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private int _index;
    [SerializeField] private Image _cooldownUIBackground;
    [SerializeField] private Image _cooldownUI;

    public void OnPointerEnter(PointerEventData eventData)
    {
        var player = GameManager.Instance.SelfPlayer;
        if (GameManager.Instance == null || player == null) return;
        var skill = player.Skills[_index];
        if(skill == null) return;
        GameManager.Instance.UIManager.SkillHoverUI.Show(
            $"[Lv.<color=yellow>{skill.Level:0}</color>] {skill.Data.Name}", 
            skill.Data.GetDescription(player, skill),
            $"마나 {StringUtil.ManaValue(skill.Data.GetManaCost(player, skill)):0.} 소모</size>"
            );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.UIManager.SkillHoverUI.Hide();
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.SelfPlayer == null) return;
        var skill = GameManager.Instance.SelfPlayer.Skills[_index];

        if(skill != null)
        {
            _cooldownUI.sprite = _cooldownUIBackground.sprite = skill.Data.Icon;
            _cooldownUI.color = Color.white;
            _cooldownUI.fillAmount = 1 - skill.CurrentCooldown / skill.Data.GetCooldown(GameManager.Instance.SelfPlayer, skill);
        }
        else
        {
            _cooldownUI.fillAmount = 1f;
            _cooldownUI.color = Color.black;
        }
    }
}
