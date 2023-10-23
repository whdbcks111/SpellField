using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerStatUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private StatType.WrapperType _statType;
    private StatType _realStatType;

    private void Awake()
    {
        _realStatType = StatType.GetByWrapperType(_statType);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameManager.Instance.UIManager.StatHoverUI.Show(
            _realStatType.DisplayName, 
            _realStatType.GetDescription(GameManager.Instance.SelfPlayer.Stat));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameManager.Instance.UIManager.StatHoverUI.Hide();
    }
}
