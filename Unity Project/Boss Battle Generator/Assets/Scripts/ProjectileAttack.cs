using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewProjectileAttack", menuName = "AttackTypes/ProjectileAttack")]
public class ProjectileAttack : ScriptableObject, IAttackType
{
    [SerializeField][EnumFlags]
    private WeaponOrientationMode requiredWeaponTypes;

    [SerializeField][EnumFlags]
    private BossTypeName compatibleBossTypes;

    [SerializeField]
    private int projectilesPerShot = 1;
    [SerializeField]
    private float projectileShotSpread = 30;
    //number of shots per attack
    //shot interval(s)

    [SerializeField]
    private GameObject projectileObj;
    //scale of projectiles
    //projectile sprite
    //projectile color(?)

    //projectile behavior (burst?)
    //projectile damage

    //weapon movement behavior (spin weapon while shooting) (move 30 degrees between shots)

    private List<Weapon> assignedWeapons = new List<Weapon>();

    public WeaponOrientationMode GetRequiredWeaponTypes()
    {
        return requiredWeaponTypes;
    }

    public BossTypeName GetCompatibleBossTypes()
    {
        return compatibleBossTypes;
    }

    public void ResetAttack()
    {
        assignedWeapons.Clear();
    }

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
        for (int i = 0; i < assignedWeapons.Count; i++)
        {
            for (int j = 0; j < projectilesPerShot; j++)
            {
                float angle = GetFiringAngle(j);

                FireProjectile(assignedWeapons[i], angle);
            }
        }
    }

    private float GetFiringAngle(int projectileInShot)
    {
        float firingAngle = (projectileInShot * projectileShotSpread) - (projectilesPerShot / 2) * projectileShotSpread;

        if (projectilesPerShot % 2 == 0)
        {
            firingAngle += projectileShotSpread / 2.0f;
        }

        return firingAngle;
    }

    private void FireProjectile(Weapon source, float firingAngle)
    {
        if (source != null)
        {
            GameObject projectile = ProjectileManager.Instance.GetProjectileObject();

            projectile.transform.position = source.transform.position;
            projectile.transform.rotation = source.transform.rotation * Quaternion.Euler(0, 0, firingAngle);
            projectile.SetActive(true);
        }
    }
}
