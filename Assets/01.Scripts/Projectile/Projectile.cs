using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class Projectile : MonoBehaviour
{
    public Player Owner;
    public Vector2 Direction
    {
        get => _direction;
        set
        {
            _direction = value;
            if (!FixedRotation) transform.right = Direction;
        }
    }
    public float Angle 
    { 
        get => Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg;
        set => Direction = new Vector2(Mathf.Cos(value * Mathf.Deg2Rad), Mathf.Sin(value * Mathf.Deg2Rad));
    }
    public float Speed, Accelerate;
    public int PenetrateCount = 0;
    public float LiveTime = 100f;
    public bool AllowNegativeSpeed = false;
    public bool OnlyTriggerWithPlayer = false;
    public bool FixedRotation = false;

    [HideInInspector] public float MaxLiveTime;
    [HideInInspector] public bool IsTriggerable = true;

    private readonly HashSet<Collider2D> _alreadyCollided = new();
    private Vector2 _direction;

    protected virtual void Awake()
    {
        MaxLiveTime = LiveTime;
    }


    protected virtual void Update()
    {
        MoveUpdate();

        if((LiveTime -= Time.deltaTime) < 0)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void MoveUpdate()
    {
        transform.position += Speed * Time.deltaTime * (Vector3)Direction;
        Speed += Accelerate * Time.deltaTime;
        if (!AllowNegativeSpeed && Speed < 0) Speed = 0f;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        
        if (!IsTriggerable) return;
        if (_alreadyCollided.Contains(collision)) return;
        _alreadyCollided.Add(collision);
        if(collision.TryGetComponent(out Damageable damageable) && 
            damageable != Owner && 
            (!OnlyTriggerWithPlayer || damageable is Player))
        {
            OnCollision(damageable);
            if(PenetrateCount-- == 0)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_alreadyCollided.Contains(collision)) 
            _alreadyCollided.Remove(collision);
    }

    public static void Shoot(Func<Projectile> projectileGetter, Player owner, Vector3 pos, float baseAngle,
        int shootCount, int angleCount, float angleSpan, float shootTimeSpan, float forward = 0f)
    {
        ShootTask(projectileGetter, owner, pos, baseAngle, 
            shootCount, angleCount, angleSpan, shootTimeSpan, forward).Forget();
    }

    private static async UniTask ShootTask(Func<Projectile> projectileGetter, Player owner, Vector3 pos, float baseAngle, 
        int shootCount, int angleCount, float angleSpan, float shootTimeSpan, float forward)
    {
        for (int i = 0; i < shootCount; i++)
        {
            for (int j = 0; j < angleCount; j++)
            {
                float angle = (j - (angleCount - 1) / 2f) * angleSpan + baseAngle;
                var projectile = projectileGetter();
                projectile.Angle = angle;
                projectile.transform.position = pos;
                projectile.transform.position += (Vector3)projectile.Direction * forward;
                projectile.Owner = owner;
            }
            await UniTask.Delay(TimeSpan.FromSeconds(shootTimeSpan));
        }
    }

    protected abstract void OnCollision(Damageable damageable);
}
