using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkinChangeUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _skinNameText;
    [SerializeField] private Image _skinImage;
    [SerializeField] private SkinData _defaultSkin;

    private int _curIndex = 0;
    private SkinData[] _skinDatas = null;

    private async void Awake()
    {
        _skinDatas = await PlayerSkinDatabase.GetAllData();

        if(PlayerPrefs.HasKey(PlayerSkinDatabase.LocalSkinDataKey))
        {
            var skinName = PlayerPrefs.GetString(PlayerSkinDatabase.LocalSkinDataKey, "");
            bool flag = false;
            for(int i = 0; i < _skinDatas.Length; i++)
            {
                var skinData = _skinDatas[i];
                if(skinData.Name == skinName)
                {
                    SetSkinIndex(i);
                    flag = true;
                    break;
                }
            }
            if(!flag)
            {
                SetDefault();
            }
        }
        else
        {
            SetDefault();
        }
    }

    public void SetDefault()
    {
        for (int i = 0; i < _skinDatas.Length; i++)
        {
            var skinData = _skinDatas[i];
            if (skinData == _defaultSkin)
            {
                SetSkinIndex(i);
                break;
            }
        }
    }

    public void SetSkinIndex(int index)
    {
        _curIndex = index;
        if (_skinDatas == null) return;

        _skinImage.sprite = _skinDatas[_curIndex].PlayerSprite;
        _skinNameText.SetText(_skinDatas[_curIndex].Name);
        PlayerPrefs.SetString(PlayerSkinDatabase.LocalSkinDataKey, _skinDatas[_curIndex].Name);
    }

    public void AddSkinIndex(int amount)
    {
        if (_skinDatas == null) return;
        SetSkinIndex(((_curIndex + amount) % _skinDatas.Length + _skinDatas.Length) 
            % _skinDatas.Length);
    }
}
