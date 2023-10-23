using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class Projectile : MonoBehaviour
{
    public Player Owner;
    public float Speed, Accelerate;
    public int PenetrateCount = 0;
    public float LiveTime = 100f;
    [HideInInspector] public float MaxLiveTime;

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
        transform.Translate(Speed * Time.deltaTime * Vector3.right);
        Speed += Accelerate * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out Damageable damageable) && 
            damageable != Owner)
        {
            OnCollision(damageable);
            if(PenetrateCount-- <= 0)
            {
                Destroy(gameObject);
            }
        }
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
                projectile.transform.position = pos;
                projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
                projectile.transform.position += projectile.transform.right * forward;
                projectile.Owner = owner;
            }
            await UniTask.Delay(TimeSpan.FromSeconds(shootTimeSpan));
        }
    }

    protected abstract void OnCollision(Damageable damageable);
}
