using UnityEngine;

[CreateAssetMenu(fileName = "New ThrowingWeapon Data", menuName = "ScriptableObjects/WeaponData/ThrowingWeapon", order = 0)]
public class ThrowingWeaponData : WeaponData
{
    [SerializeField] private DamageProjectile _projectilePrefab;
    [SerializeField] private bool _isMagicAttack;
    [SerializeField] private float _baseDamage, _statMultiply;

    protected virtual DamageProjectile GetProjectile(Player p, Weapon weapon)
    {
        var projectile = Instantiate(_projectilePrefab);
        projectile.AttackParams = _isMagicAttack ?
            p.Stat.GetMagicalAttackParams(_baseDamage + p.Stat.Get(StatType.MagicForce) * _statMultiply) :
            p.Stat.GetPhysicalAttackParams(_baseDamage + p.Stat.Get(StatType.Attack) * _statMultiply);
        projectile.Owner = p;
        projectile.PenetrateCount = -1;
        return projectile;
    }

    protected virtual void Shoot(Player p, Weapon weapon)
    {
        var projectile = GetProjectile(p, weapon);
        projectile.transform.position = weapon.Display.transform.position;
        projectile.Direction = p.PlayerRenderer.transform.right;
        weapon.Display.SpriteRenderer.enabled = false;
        weapon.SetData("projectile", projectile);
        weapon.CurrentCooldown = 0.3f;
    }

    protected override void OnUpdate(Player p, Weapon weapon)
    {
        base.OnUpdate(p, weapon);

        var projectile = weapon.GetData<DamageProjectile>("projectile", null);
        var isReturning = weapon.GetData("isReturning", false);

        if(isReturning)
        {
            if(projectile != null)
            {
                projectile.Direction = (p.PlayerRenderer.transform.position - projectile.transform.position).normalized;
                if((projectile.transform.position - p.PlayerRenderer.transform.position).sqrMagnitude < 2f * 2f)
                {
                    Destroy(projectile.gameObject);
                    RechargeWeapon(weapon);
                }
            }
        }

        if(projectile == null)
        {
            RechargeWeapon(weapon);
        }
    }

    private void RechargeWeapon(Weapon weapon)
    {
        weapon.Display.SpriteRenderer.enabled = true;
        weapon.SetData("isReturning", false);
        weapon.SetData<DamageProjectile>("projectile", null);
    }

    private void ReturnProjectile(Weapon weapon)
    {
        weapon.SetData("isReturning", true);
    }

    protected override void OnUse(Player p, Weapon weapon)
    {
        base.OnUse(p, weapon);

        var projectile = weapon.GetData<DamageProjectile>("projectile", null);
        var isReturning = weapon.GetData("isReturning", false);

        if (projectile != null) ReturnProjectile(weapon);
        else if(!isReturning) Shoot(p, weapon);
    }

}