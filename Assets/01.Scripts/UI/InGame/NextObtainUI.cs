using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextObtainUI : MonoBehaviour
{
    [SerializeField] private Image _filledBar;

    private void Update()
    {
        _filledBar.fillAmount = 1f - Mathf.Clamp01(GameManager.Instance.RemainObtainTime / Mathf.Max(.1f, GameManager.Instance.ObtainSpan));
    }
}
