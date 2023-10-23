using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ObtainItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image IconImage;
    public Image Outline;
    public TextMeshProUGUI ObtainTypeText;
    public TextMeshProUGUI DescriptionText;
    public Action OnObtain;

    private void Awake()
    {
        Outline.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        Outline.gameObject.SetActive(false);
    }

    public void OnClick()
    {
        if(OnObtain is not null) OnObtain();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Outline.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Outline.gameObject.SetActive(false);
    }
}
