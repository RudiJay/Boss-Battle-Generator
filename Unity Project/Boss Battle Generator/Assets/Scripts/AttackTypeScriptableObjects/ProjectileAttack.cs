/* 
 * Copyright (C) 2019 Rudi Jay Prentice - All right reserved
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Projectile attack type subclass
/// fires a number of Projectile objects from an assigned boss weapon when performed
/// </summary>
[CreateAssetMenu(fileName = "NewProjectileAttack", menuName = "AttackTypes/ProjectileAttack")]
public class ProjectileAttack : ScriptableObject, IAttackType
{
    [SerializeField]
    private float delayAfterAttack;

    [SerializeField][EnumFlags]
    private WeaponOrientationMode requiredWeaponTypes; 

    [SerializeField][EnumFlags]
    private BossTypeName compatibleBossTypes; 

    [SerializeField]
    private int projectilesPerShot = 1;
    [SerializeField]
    private float projectileShotSpread = 30;
    //TODO: number of shots per attack
    //TODO: shot interval(s)

    /// <summary>
    /// Data about the projectile this attack will fire
    /// </summary>
    [SerializeField]
    private ProjectileData projectileToFire;
    //TODO: allow multiple types of projectile to be fired by one attack

    //TODO: weapon movement behavior (spin weapon while shooting) (move 30 degrees between shots)

    /// <summary>
    /// The list of weapons this instance of the attack fires from
    /// </summary>
    private List<Weapon> assignedWeapons = new List<Weapon>();

    public float DelayAfterAttack
    {
        get
        {
            return delayAfterAttack;
        }
        set
        {
            delayAfterAttack = value;
        }
    }

    public WeaponOrientationMode GetRequiredWeaponTypes()
    {
        return requiredWeaponTypes;
    }

    public BossTypeName GetCompatibleBossTypes()
    {
        return compatibleBossTypes;
    }

    public void SetupAttack(GameObject performingObj)
    {
        Weapon weapon = performingObj.GetComponent<Weapon>();

        if (weapon != null)
        {
            assignedWeapons.Add(weapon);

            //if the weapon has a symmetrical pair, also add that weapon
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

        //offset projectile firing angle by hald if there are an even number of shots
        if (projectilesPerShot % 2 == 0)
        {
            firingAngle += projectileShotSpread / 2.0f;
        }

        return firingAngle + 180.0f;
    }

    private void FireProjectile(Weapon source, float firingAngle)
    {
        if (source != null)
        {
            ProjectileLogic projectile = ProjectileManager.Instance.GetProjectile();

            if (projectile != null)
            {
                projectile.SetupProjectileData(projectileToFire);
                projectile.transform.position = source.attackSource.position;
                projectile.transform.rotation = source.attackSource.rotation * Quaternion.Euler(0, 0, firingAngle);
                projectile.gameObject.SetActive(true);
            }
            else
            {
                Debug.Log("ERROR: Available Projectile Not Found");
            }
        }
    }
}
