using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField]
    private WeaponOrientationMode currentOrientationMode;

    public void SetOrientationMode(WeaponOrientationMode weaponOrientationMode)
    {
        currentOrientationMode = weaponOrientationMode;
    }
}
