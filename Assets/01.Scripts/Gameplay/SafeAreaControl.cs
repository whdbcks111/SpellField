using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[ExecuteInEditMode]
[RequireComponent(typeof(CircleCollider2D), typeof(LineRenderer))]
public class SafeAreaControl : MonoBehaviour
{
    public Vector3 Center = Vector3.zero;
    public float Radius;

    private Vector2 _scaleVel = Vector2.zero;
    private Vector3 _moveVel;

    private LineRenderer _lineRenderer;
    private Collider2D _collider;

    private float _multiplier;
    private Vector2 _targetScale;

    public Collider2D Collider { get { return _collider; } }

    public float CurrentRadius
    {
        get
        {
            var multiplier = transform.localScale.x / Mathf.Max(Mathf.Epsilon, transform.lossyScale.x);
            return transform.localScale.x / 2f / multiplier;
        }

        set
        {
            Radius = value;
            var multiplier = transform.localScale.x / Mathf.Max(Mathf.Epsilon, transform.lossyScale.x);
            transform.localScale = 2 * value * multiplier * Vector2.one;
        }
    }

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _lineRenderer = GetComponent<LineRenderer>();
    }

    private void FixedUpdate()
    {
        _multiplier = transform.localScale.x / Mathf.Max(Mathf.Epsilon, transform.lossyScale.x);
        _targetScale = 2 * Radius * _multiplier * Vector2.one;

        Vector2 currentScale = transform.localScale;
        currentScale = Vector2.SmoothDamp(currentScale, _targetScale, ref _scaleVel, 0.5f, 100f, Time.fixedDeltaTime);
        transform.localScale = new(currentScale.x, currentScale.y, transform.localScale.z);

        transform.position = Vector3.SmoothDamp(transform.position, Center, ref _moveVel, 0.5f, 100f, Time.fixedDeltaTime);

        var vertexCount = Mathf.Max((int)(CurrentRadius * 2f * Mathf.PI * 4f), 100);
        _lineRenderer.positionCount = vertexCount;

        float angle;
        Vector3 pos;
        for (int i = 0; i < vertexCount; ++i)
        {
            angle = (float)i / vertexCount * 2f * Mathf.PI;
            pos = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * (CurrentRadius + _lineRenderer.startWidth / 2f);
            _lineRenderer.SetPosition(i, pos);
        }
    }
}
