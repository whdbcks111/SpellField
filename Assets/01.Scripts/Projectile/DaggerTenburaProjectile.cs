using UnityEngine;

public class DaggerTenburaProjectile : DamageProjectile
{
    public const string RandomKey = "tenbura";
    [HideInInspector] public bool IsWaiting = true;
    [HideInInspector] public Transform Target = null;
    [SerializeField] public float WaitRotateSpeed;

    private Vector3 _startPos, _curveOffset;
    private float _bezierProgress = 0f;

    protected override void MoveUpdate()
    {
        if(IsWaiting)
        {
            _startPos = transform.position;
            if (GameManager.Instance != null) 
                _curveOffset = new Vector3(
                    GameManager.Instance.GetSeedRandomRange(RandomKey, 2f, 10f),
                    GameManager.Instance.GetSeedRandomRange(RandomKey, 2f, 10f)
                );
            transform.Rotate(Time.deltaTime * WaitRotateSpeed * Vector3.forward);
        }
        else if(Target != null)
        {
            var distance = (Target.position - transform.position).magnitude;
            var nextPos = ExtraMath.GetBezierPoint(new Vector2[]
            {
                _startPos, _startPos + _curveOffset, Target.position
            }, Mathf.Clamp01(_bezierProgress));
            _bezierProgress += (1 - _bezierProgress) * Time.deltaTime * Speed / Mathf.Max(distance, 1f);
            transform.right = (nextPos - (Vector2)transform.position).normalized;
            transform.position = nextPos;
        }
        else
        {
            base.MoveUpdate();
        }
    }

    protected override void OnCollision(Damageable damageable)
    {
        if (IsWaiting) return;
        base.OnCollision(damageable);
    }
}