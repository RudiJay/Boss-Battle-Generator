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

    public bool CheckIfCollidingWithOtherWeapons()
    {
        RaycastHit2D[] results = new RaycastHit2D[20];

        Vector3 boundSize = gameObject.GetComponentInChildren<SpriteRenderer>().bounds.size;
        float xSize = boundSize.x;
        float ySize = boundSize.y;
        Vector2 size = new Vector2(xSize, ySize);

        int length = Physics2D.BoxCast(transform.position, size, transform.rotation.eulerAngles.z, Vector2.zero, new ContactFilter2D().NoFilter(), results, 0.0f);

        for (int i = 0; i < length; i++)
        {
            if (results[i].collider.tag == "Weapon")
            {
                //Debug.Log("hit " + results[i].collider.gameObject.transform.position);
                isCollidingWithOtherWeapon = true;
                break;
            }
        }

        return isCollidingWithOtherWeapon;
    }
}
