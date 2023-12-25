using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BombProjectile : DamageProjectile
{
    [SerializeField] private float _waitTime;
    [SerializeField] private Color _waitColor, _detectColor;

    [SerializeField] private AudioClip _activeSound;
    [SerializeField] private float _activeSoundVolume = 1f;
    [SerializeField] private float _activeSoundPitch = 1f;

    [Header("References")]
    [SerializeField] private SpriteRenderer _bombRenderer;
    [SerializeField] private ParticleSystem _explodeParticle;
    [SerializeField] private ParticleSystem _smokeParticle;
    [SerializeField] private Light2D _light;
    [SerializeField] private SpriteRenderer _collisionArea;

    protected override void Awake()
    {
        base.Awake();
        OnlyTriggerWithPlayer = true;
    }

    protected override void Update()
    {
        base.Update();
        var colorMultiplier = _waitTime > 0 || Owner.IsSelf ? Color.white : new Color(1, 1, 1, 0.05f);

        _bombRenderer.color = colorMultiplier;

        if (_waitTime > 0f) _waitTime -= Time.deltaTime;

        IsTriggerable = _waitTime <= 0f;

        var targetColor = _waitTime > 0f ? _waitColor : _detectColor;
        _light.color = targetColor * colorMultiplier;
        _collisionArea.color = new Color(targetColor.r, targetColor.g, targetColor.b, _collisionArea.color.a) * colorMultiplier;

        _light.intensity = (Mathf.Sin(Time.time * 2 * Mathf.PI) + 1) / 2f * 1.5f;
    }

    protected override void OnCollision(Damageable damageable)
    {
        base.OnCollision(damageable);

        SoundManager.Instance.PlaySFX(_activeSound, transform.position, _activeSoundVolume, _activeSoundPitch);
        ParticleManager.SpawnParticle(_explodeParticle, transform.position);
        ParticleManager.SpawnParticle(_smokeParticle, transform.position);
    }
}