using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewProjectileAttack", menuName = "AttackTypes/ProjectileAttack")]
public class ProjectileAttack : ScriptableObject, IAttackType
{
    public WeaponOrientationMode requiredWeaponType { get; }

    public BossTypeName requiredBossType { get; }

    //number of projectiles per shot
    //projectile spread
    //number of shots
    //shot interval(s)

    [SerializeField]
    private GameObject projectileObj;
    //scale of projectiles
    //projectile sprite
    //projectile color(?)

    //projectile behavior (burst?)
    //projectile damage

    //weapon movement behavior (spin weapon while shooting) (move 30 degrees between shots)

    private List<Weapon> assignedWeapons;

    public void SetupAttack(GameObject performingObj)
    {
        Weapon weapon = performingObj.GetComponent<Weapon>();

        if (weapon != null)
        {
            assignedWeapons.Add(weapon);

            Weapon mirror = weapon.mirrorPair;
            if (mirror != null)
            {
                assignedWeapons.Add(mirror);
            }
        }
    }

    public void PerformAttack()
    {
        foreach (Weapon weapon in assignedWeapons)
        {
            FireProjectile(weapon);
        }
    }

    private void FireProjectile(Weapon source)
    {
        if (source != null)
        {
            source.PerformAttack(projectileObj);
        }
    }
}
