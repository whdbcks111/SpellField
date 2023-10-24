using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BombProjectile : DamageProjectile
{
    [SerializeField] ParticleSystem _bombParticle;
    [SerializeField] Light2D _light;

    protected override void Update()
    {
        base.Update();

        _light.intensity = (Mathf.Sin(Time.time * 2 * Mathf.PI) + 1) / 2f * 1.5f;
    }

    protected override void OnCollision(Damageable damageable)
    {
        base.OnCollision(damageable);

        Instantiate(_bombParticle, transform.position, Quaternion.identity);
    }
}