﻿/* 
 * Copyright (C) 2018 Rudi Jay Prentice - All right reserved
 */

using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField]
    public WeaponOrientationMode currentOrientationMode { get; set; }

    public void SetWeaponRotation(float rot)
    {
        transform.rotation = Quaternion.Euler(0, 0, rot);
    }
}