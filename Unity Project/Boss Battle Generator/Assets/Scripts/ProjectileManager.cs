﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance;

    [SerializeField]
    private int projectilePoolSize = 100;

    [SerializeField]
    private GameObject projectilePrefab;

    private List<GameObject> projectilePool;
    private int listIndex = 0;

    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        projectilePool = new List<GameObject>();

        for (int i = 0; i < projectilePoolSize; i++)
        {
            GameObject projectile = Instantiate(projectilePrefab, gameObject.transform);
            projectile.SetActive(false);
            projectilePool.Add(projectile);
        }
    }

    public GameObject GetProjectileObject()
    {
        for (int i = 0; i < projectilePool.Count; i++)
        {
            if (!projectilePool[listIndex].activeInHierarchy)
            {
                GameObject projectile = projectilePool[listIndex];

                return projectile;
            }

            listIndex++;
            if (listIndex >= projectilePool.Count)
            {
                listIndex = 0;
            }
        }

        Debug.Log("inactive projectile not found");
        return null;
    }

    public ProjectileLogic GetProjectile()
    {
        for (int i = 0; i < projectilePool.Count; i++)
        {
            if (!projectilePool[listIndex].activeInHierarchy)
            {
                ProjectileLogic projectile = projectilePool[listIndex].GetComponent<ProjectileLogic>();

                return projectile;
            }

            listIndex++;
            if (listIndex >= projectilePool.Count)
            {
                listIndex = 0;
            }
        }

        Debug.Log("inactive projectile not found");
        return null;
    }

    public void DisableAllProjectiles()
    {
        for (int i = 0; i < projectilePool.Count; i++)
        {
            projectilePool[i].SetActive(false);
        }
    }

    public void SendPlayerTransformToProjectiles(Transform transform)
    {
        for (int i = 0; i < projectilePool.Count; i++)
        {
            projectilePool[i].GetComponent<ProjectileLogic>().SetTargetTransform(transform);
        }
    }
}
