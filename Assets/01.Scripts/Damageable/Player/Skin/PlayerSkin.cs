
using UnityEngine;

[CreateAssetMenu(fileName = "New Skin", menuName = "ScriptableObjects/SkinData", order = 0)]
public class SkinData : ScriptableObject
{
    public string Name;
    public Sprite PlayerSprite, HandSprite;
}