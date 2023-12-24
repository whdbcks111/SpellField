using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Protector", menuName = "ScriptableObjects/SkillData/Protector", order = 0)]
public class ProtectorSkillData : PlayerSkillData
{
    [SerializeField] private SpriteRenderer _shieldRenderer;
    [SerializeField] private AudioClip _activeSound;
    [SerializeField] private float _activeSoundVolume = 1f;
    [SerializeField] private float _activeSoundPitch = 1f;

    [SerializeField] private AudioClip _reflectSound;
    [SerializeField] private float _reflectSoundVolume = 1f;
    [SerializeField] private float _reflectSoundPitch = 1f;

    [Header("Protect")]
    [SerializeField] private float _baseShieldSize;
    [SerializeField] private float _shieldSizeGrowth;
    [SerializeField] private float _shieldShrinkSpeed;
    [SerializeField] private float _shieldShrinkSpeedShrink;
    [SerializeField] private float _minShieldShrinkSpeed;

    [Header("Mana Cost")]
    [SerializeField] private float _baseManaCost;
    [SerializeField] private float _manaCostGrowth;

    [Header("Cooldown")]
    [SerializeField] private float _baseCooldown;
    [SerializeField] private float _cooldownShrink;
    [SerializeField] private float _minCooldown;

    public override float GetCooldown(Player p, PlayerSkill skill)
    {
        return Mathf.Max(_minCooldown, _baseCooldown - (skill.Level - 1) * _cooldownShrink);
    }

    public float GetShieldSize(PlayerSkill skill)
    {
        return _baseShieldSize + (skill.Level - 1) * _shieldSizeGrowth;
    }

    public float GetShrinkSpeed(PlayerSkill skill)
    {
        return Mathf.Max(_minShieldShrinkSpeed, _shieldShrinkSpeed - (skill.Level - 1) * _shieldShrinkSpeedShrink);
    }

    public override string GetDescription(Player p, PlayerSkill skill)
    {
        return $"주위에 크기 {GetShieldSize(skill):0.0}의 방어막을 생성합니다.\n" +
            $"방어막 안에 들어오는 움직이는 투사체를 모두 반대 방향으로 튕겨냅니다.\n" +
            $"또한 방어막은 {GetShrinkSpeed(skill):0.0}m/s의 속도로 작아지다가 사라집니다.";
    }

    public override void OnPassiveUpdate(Player p, PlayerSkill skill)
    {
        var shield = skill.GetData<SpriteRenderer>("shield", null);
        if(shield != null)
        {
            var shrinkSpeed = GetShrinkSpeed(skill);
            var ls = shield.transform.localScale;
            ls.x -= shrinkSpeed * Time.deltaTime;
            ls.y -= shrinkSpeed * Time.deltaTime;

            if (ls.x < 0f || ls.y < 0f) ls.x = ls.y = 0f;
            shield.transform.localScale = ls;

            var result = Physics2D.OverlapCircleAll(p.PlayerRenderer.transform.position, shield.bounds.size.x / 2f);
            foreach(var collider in result)
            {
                if(collider.TryGetComponent(out Projectile projectile) && projectile.Owner != p && Mathf.Abs(projectile.Speed) > 0.1f)
                {
                    projectile.AllowNegativeSpeed = true;
                    projectile.Owner = p;
                    projectile.LiveTime = projectile.MaxLiveTime;
                    projectile.Direction = -projectile.Direction;
                    SoundManager.Instance.PlaySFX(_reflectSound, p.transform.position, _reflectSoundVolume, _reflectSoundPitch);
                }
            }

            if(ls.x <= 1f)
            {
                Destroy(shield.gameObject);
                skill.SetData<SpriteRenderer>("shield", null);
            }

            skill.CurrentCooldown = skill.Data.GetCooldown(p, skill);
        }
    }

    public override void OnActiveUse(Player p, PlayerSkill skill)
    {
        var shield = Instantiate(_shieldRenderer, p.PlayerRenderer.transform.position, Quaternion.identity, p.PlayerRenderer.transform);
        var shieldSize = GetShieldSize(skill);
        SoundManager.Instance.PlaySFX(_activeSound, p.transform.position, _activeSoundVolume, _activeSoundPitch);
        shield.transform.localScale = new Vector3(shieldSize, shieldSize, 1f);
        skill.SetData("shield", shield);
    }

    public override float GetManaCost(Player p, PlayerSkill skill)
    {
        return _baseManaCost + (skill.Level - 1) * _manaCostGrowth;
    }
}
