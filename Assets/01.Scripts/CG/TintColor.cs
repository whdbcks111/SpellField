using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TintColor : MonoBehaviour
{
    private Renderer _renderer;
    public Color Color;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    public void Update()
    {
        _renderer.material.SetColor("_TintColor", Color);
    }
}
