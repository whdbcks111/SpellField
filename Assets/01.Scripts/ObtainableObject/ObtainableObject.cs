using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObtainableObject : ScriptableObject
{
    public Sprite Icon;
    public string Name; 
    [TextArea] public string DescriptionSummary;
}
