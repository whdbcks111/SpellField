using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDisplay : MonoBehaviour
{
    public delegate void Trigger2DDelegate(Collider2D collider);

    public SpriteRenderer SpriteRenderer;
    public Trigger2DDelegate OnTriggerStay2DEvent;

    private void OnTriggerStay2D(Collider2D collider)
    {
        OnTriggerStay2DEvent?.Invoke(collider);
    }
}
