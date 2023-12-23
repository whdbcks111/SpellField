using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChargeBar : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Image _filledImage;
    [SerializeField] private TextMeshProUGUI _title;

    [HideInInspector] public bool IsVisible = false;
    private float _vel = 0f;

    public float Progress
    {
        get => _filledImage.fillAmount;
        set => _filledImage.fillAmount = value;
    }

    public Color BarColor
    {
        get => _filledImage.color;
        set => _filledImage.color = value;
    }

    public string Title
    {
        get => _title.text;
        set => _title.SetText(value);
    }

    public void Show(string title, Color barColor)
    {
        Title = title;
        BarColor = barColor;
        IsVisible = true;
    }

    public void Hide()
    {
        IsVisible = false;
    }

    private void Awake()
    {
        _canvasGroup.alpha = 0f;
    }

    private void Update()
    {
        _canvasGroup.alpha = Mathf.SmoothDamp(_canvasGroup.alpha, IsVisible ? 1f : 0f, ref _vel, 0.1f);
    }

}
    