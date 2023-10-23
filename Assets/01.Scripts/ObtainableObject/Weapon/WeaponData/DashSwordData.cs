using UnityEngine;
using Cysharp.Threading.Tasks;

[CreateAssetMenu(fileName = "New DashSword Data", menuName = "ScriptableObjects/WeaponData/DashSword", order = 0)]
public class DashSwordData : MeleeAttackWeaponData
{
    [SerializeField] private float _dashSpeed, _dashAccelerate, _dashTime;

    protected override void OnUse(Player p, Weapon weapon)
    {
        base.OnUse(p, weapon);
        Dash(p).Forget();
    }

    private async UniTask Dash(Player p)
    {
        Vector3 dir = p.PlayerRenderer.transform.right;
        float speed = _dashSpeed;
        float time = _dashTime;
        while (time > 0 && speed > 0)
        {
            await UniTask.Yield();
            time -= Time.deltaTime;
            speed += _dashAccelerate * Time.deltaTime;
            p.transform.position += speed * Time.deltaTime * dir;
        }
    }
}