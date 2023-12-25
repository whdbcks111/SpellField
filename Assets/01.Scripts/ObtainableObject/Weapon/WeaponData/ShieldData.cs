using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

[CreateAssetMenu(fileName = "New Shield Data", menuName = "ScriptableObjects/WeaponData/Shield", order = 0)]
public class ShieldData : WeaponData
{
    [SerializeField] private ShieldObject _shieldObject;
    [SerializeField] private float _shieldTime;
    [SerializeField] private float _shieldAmount;

    [SerializeField] private AudioClip _activeSound;
    [SerializeField] private float _activeSoundVolume = 1f;
    [SerializeField] private float _activeSoundPitch = 1f;

    protected override void OnUse(Player p, Weapon weapon)
    {
        base.OnUse(p, weapon);
        UseShield(p).Forget();
    }

    private async UniTask UseShield(Player p)
    {
        SoundManager.Instance.PlaySFX(_activeSound, p.transform.position, _activeSoundVolume, _activeSoundPitch);
        var shield = Instantiate(_shieldObject, p.PlayerRenderer.transform);
        p.AddShield(_shieldAmount, _shieldTime);
        await UniTask.Delay(TimeSpan.FromSeconds(_shieldTime));
        shield.Destroy();
    }
}