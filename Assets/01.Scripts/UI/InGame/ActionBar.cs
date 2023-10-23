using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ActionBar : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    private TextMeshProUGUI _text;
    private float _alpha = 0f;
    private float _showDuration = 1f;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _canvasGroup.alpha = 0f;
    }

    private void Update()
    {
        if(_alpha > 0f) _alpha -= Time.deltaTime / Mathf.Max(_showDuration, 0.1f);
        _canvasGroup.alpha = Mathf.Clamp01(_alpha);
    }

    public void ShowActionBar(string message, float duration)
    {
        _showDuration = duration;
        _text.SetText(message);
        _alpha = 1f;
    }
}
