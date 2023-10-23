using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HoverUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _titleText, _descriptionText, _rightAlignedTitleText;
    [SerializeField] private Image _bgImage;
    [SerializeField] private float _minHeight;

    private void Awake()
    {
        Hide();
    }

    public void Show(string title, string description, string rightTitle = null)
    {
        _titleText.text = title;
        _descriptionText.text = description;
        if(rightTitle != null && _rightAlignedTitleText != null) 
            _rightAlignedTitleText.text = rightTitle;
        _bgImage.rectTransform.sizeDelta = new(
            _bgImage.rectTransform.sizeDelta.x, 
            Mathf.Max(_titleText.preferredHeight + _descriptionText.preferredHeight 
                - _titleText.rectTransform.offsetMax.y * 2, _minHeight));
        _canvasGroup.alpha = 1f;
    }

    public void Hide()
    {
        _canvasGroup.alpha = 0f;
    }
}
