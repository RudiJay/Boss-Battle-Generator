/* 
 * Copyright (C) 2019 Rudi Jay Prentice - All right reserved
 */

using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField]
    private WeaponOrientationMode currentOrientationMode;
    public WeaponOrientationMode CurrentOrientationMode
    {
        get { return currentOrientationMode; }
        set { currentOrientationMode = value; }
    }

    public Weapon mirrorPair { get; set; }

    private bool isCollidingWithOtherWeapon = false;

    public void SetWeaponRotation(float rot)
    {
        transform.rotation = Quaternion.Euler(0, 0, rot);
    }

    public void PerformAttack(GameObject projectileObj)
    {
        Instantiate(projectileObj, transform);
    }

    public bool GetCollidingWithOtherWeapon()
    {
        return isCollidingWithOtherWeapon;
    }

    private void Start()
    {
        isCollidingWithOtherWeapon = false;
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
