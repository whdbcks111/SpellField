using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;
public class FontChanger
{
    public const string PATH_FONT_ASSET = "Assets/TextMesh Pro/Resources/Fonts & Materials/esamanru SDF.asset";

    [MenuItem("FontUtil/Change TMP UI Fonts")]
    public static void ChangeFonts()
    {
        TextMeshProUGUI[] fonts = GameObject.FindObjectsOfType<TextMeshProUGUI>();
        foreach(TextMeshProUGUI font in fonts)
        {
            font.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(PATH_FONT_ASSET);
        }
    }
}