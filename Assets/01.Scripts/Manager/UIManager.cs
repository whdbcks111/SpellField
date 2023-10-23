using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    [SerializeField] private ActionBar _actionBar;
    public ActionBar ActionBar { get => _actionBar; }

    [SerializeField] private HoverUI _skillHoverUI;
    public HoverUI SkillHoverUI { get => _skillHoverUI; }

    [SerializeField] private HoverUI _statHoverUI;
    public HoverUI StatHoverUI { get => _statHoverUI; }

    [SerializeField] private HoverUI _weaponHoverUI;
    public HoverUI WeaponHoverUI { get => _weaponHoverUI; }

    [SerializeField] private Canvas _worldCanvas;
    public Canvas WorldCanvas { get => _worldCanvas; }

    [SerializeField] private ObtainPanelUI _obtainPanel;
    public ObtainPanelUI ObtainPanel { get => _obtainPanel; }

    [SerializeField] private Image _damageScreen;

    private float _damageScreenTime = 0f, _maxDamageScreenTime = 1f;
    private bool _toggleDamageScreen = false;

    public void ShowDamageScreen(float time = 0.5f)
    {
        _damageScreenTime = _maxDamageScreenTime = time;
    }

    public void ShowDamageScreen(bool toggle)
    {
        _toggleDamageScreen = toggle;
    }

    private void Update()
    {
        var damageScreenCol = _damageScreen.color;
        if (_toggleDamageScreen)
        {
            damageScreenCol.a += Time.deltaTime;
        }
        else
        {
            float targetAlpha = _damageScreenTime / _maxDamageScreenTime;
            if (damageScreenCol.a < targetAlpha)
            {
                damageScreenCol.a = targetAlpha;
            }
            else
            {
                damageScreenCol.a = Mathf.MoveTowards(damageScreenCol.a, targetAlpha, Time.deltaTime);
            }
            _damageScreenTime -= Time.deltaTime;
        }
        damageScreenCol.a = Mathf.Clamp01(damageScreenCol.a);
        _damageScreen.color = damageScreenCol;
    }
}
