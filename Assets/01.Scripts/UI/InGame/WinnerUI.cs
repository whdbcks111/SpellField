using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WinnerUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _winnerText;

    private bool _isVisible = false; 

    private void Awake()
    {
        _canvasGroup.alpha = 0f;
    }

    public void Show(string winner)
    {
        _winnerText.SetText(winner);
        _isVisible = true;
    }

    public void Hide() 
    {
        _isVisible = false;
    }

    private void Update()
    {
        _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, _isVisible ? 1f : 0f, Time.deltaTime / 0.3f);
    }
}
