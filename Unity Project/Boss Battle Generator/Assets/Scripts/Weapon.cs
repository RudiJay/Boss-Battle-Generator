/* 
 * Copyright (C) 2019 Rudi Jay Prentice - All right reserved
 */

using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponOrientationMode currentOrientationMode { get; set; }

    public Weapon mirrorPair { get; set; }

    public bool isCollidingWithOtherWeapon = false;

    [SerializeField]
    private LayerMask weaponSpriteLayer;

    public void SetWeaponRotation(float rot)
    {
        transform.rotation = Quaternion.Euler(0, 0, rot);
    }

    public void PerformAttack(GameObject projectileObj)
    {
        Instantiate(projectileObj, transform);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Weapon")
        {
            if (!isCollidingWithOtherWeapon)
            {
                isCollidingWithOtherWeapon = true;
            }
        }
    }
}
