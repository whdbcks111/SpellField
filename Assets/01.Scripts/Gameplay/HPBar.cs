using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] private Image _hpBar, _backShieldBar, _frontShieldBar;

    public float HP, MaxHP, Shield;
    public Color HpColor;

    private void Awake()
    {
        HpColor = _hpBar.color;
    }

    private void Update()
    {
        _hpBar.fillAmount = Mathf.Clamp01(HP / Mathf.Max(Mathf.Epsilon, MaxHP));
        if(Shield / MaxHP <= 1 - _hpBar.fillAmount)
        {
            _backShieldBar.fillAmount = _hpBar.fillAmount + Shield / Mathf.Max(Mathf.Epsilon, MaxHP);
            _frontShieldBar.fillAmount = 0;
        }
        else
        {
            _backShieldBar.fillAmount = 0;
            _frontShieldBar.fillAmount = Shield / Mathf.Max(Mathf.Epsilon, MaxHP);
        }
    }
}
