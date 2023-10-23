using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class RemainTimeDisplay : MonoBehaviour
{
    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        _text.SetText(string.Format("{0:00}:{1:00}",
            (int)(GameManager.Instance.RemainGameTime / 60f), 
            (int)(GameManager.Instance.RemainGameTime % 60f)));
    }

}
