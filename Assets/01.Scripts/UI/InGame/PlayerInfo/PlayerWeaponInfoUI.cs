using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerWeaponInfoUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image _iconBG, _icon;
    [SerializeField] private CanvasGroup _canvasGroup;

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.SelfPlayer == null) return;
        var weapon = GameManager.Instance.SelfPlayer.MountedWeapon;
        _canvasGroup.alpha = weapon == null ? 0 : 1;

        if( weapon != null )
        {
            _iconBG.sprite = _icon.sprite = weapon.Data.Icon;
            _icon.fillAmount = 1 - Mathf.Clamp01(weapon.CurrentCooldown / weapon.Data.Cooldown);
            _icon.color = weapon.CurrentCooldown <= 0f ? Color.white : Color.Lerp(Color.white, _iconBG.color, 0.4f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var weapon = GameManager.Instance.SelfPlayer.MountedWeapon;
        if (weapon == null) return;

        string statModifierInfo = "";
        foreach(var modifier in weapon.Data.StatModifiers)
        {
            statModifierInfo += $"\n{modifier.StatType.DisplayName} {modifier.StatType.TextIcon} ";
            switch(modifier.ModifyType)
            {
                case StatModifier.Type.Add:
                    statModifierInfo += $"{(modifier.Value > 0 ? "+" : "-")}" +
                        $"{Mathf.Abs(modifier.Value) :0.}" +
                        $"{modifier.StatType.AddSuffix}";
                    break;
                case StatModifier.Type.Multiply:
                    statModifierInfo += $"{(modifier.Value > 1 ? "+" : "-")}" +
                        $"{Mathf.Abs(modifier.Value * 100 - 100):0.}" +
                        $"{modifier.StatType.MultiplySuffix}";
                    break;
            }
        }

        var description = weapon.Data.DescriptionSummary?.Length > weapon.Data.Description?.Length ? 
            weapon.Data.DescriptionSummary : weapon.Data.Description;

        GameManager.Instance.UIManager.WeaponHoverUI.Show(
            weapon.Data.Name, 
            description + "\n" + statModifierInfo
            );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameManager.Instance.UIManager.WeaponHoverUI.Hide();
    }
}
