/* 
 * Copyright (C) 2018 Rudi Jay Prentice - All right reserved
 */

using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField]
    public WeaponOrientationMode currentOrientationMode { get; set; }

    public bool isCollidingWithOtherWeapon = false;

    [SerializeField]
    private LayerMask weaponSpriteLayer;

    public void SetWeaponRotation(float rot)
    {
        transform.rotation = Quaternion.Euler(0, 0, rot);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Weapon")
        {
            //isCollidingWithOtherWeapon = true;
        }
    }

    public bool CheckIfCollidingWithOtherWeapons()
    {
        RaycastHit2D[] results = new RaycastHit2D[20];

        int length = gameObject.GetComponent<Rigidbody2D>().Cast(Vector2.zero, results, 0.0f);

        for (int i = 0; i < length; i++)
        {
            if (results[i].collider.tag == "Weapon" && results[i].collider != gameObject.GetComponentInChildren<Collider2D>())
            {
                Debug.Log(results[i].point);
                isCollidingWithOtherWeapon = true;
                break;
            }
        }

        return isCollidingWithOtherWeapon;
    }
}
