using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField]
    private GameObject weaponParentObject;

    [SerializeField]
    private int weaponPoolSize = 20;

    [SerializeField]
    private GameObject weaponPrefab;

    private List<GameObject> weaponObjPool;
    private int listIndex = 0;
    
    void Start()
    {
        weaponObjPool = new List<GameObject>();
        
        for (int i = 0; i < weaponPoolSize; i++)
        {
            GameObject weapon = Instantiate(weaponPrefab, weaponParentObject.transform);
            weapon.SetActive(false);
            weaponObjPool.Add(weapon);
        }
    }
    
    public GameObject GetWeaponObject()
    {
        for (int i = 0; i < weaponObjPool.Count; i++)
        {
            if (!weaponObjPool[listIndex].activeInHierarchy)
            {
                GameObject weapon = weaponObjPool[listIndex];

                return weapon;
            }

            listIndex++;
            if (listIndex >= weaponObjPool.Count)
            {
                listIndex = 0;
            }
        }

        Debug.Log("inactive weapon obj not found");
        return null;
    }

    public void DisableAllWeapons()
    {
        for (int i = 0; i < weaponObjPool.Count; i++)
        {
            weaponObjPool[i].SetActive(false);
        }
    }
}
