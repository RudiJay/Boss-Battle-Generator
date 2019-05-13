﻿/* 
 * Copyright (C) 2019 Rudi Jay Prentice - All right reserved
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLogic : MonoBehaviour
{
    public float speed = 1.0f;

    [SerializeField]
    private float selfDestructTime = 10.0f;

    [SerializeField]
    private Rigidbody2D rb;

    private void OnEnable()
    {
        if (rb != null)
        {
            rb.AddForce(transform.up * speed * 100);
        }

        Invoke("DisableProjectile", selfDestructTime);
    }

    private void OnDisable()
    {
        CancelInvoke("DisableProjectile");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            DisableProjectile();
        }
    }

    private void DisableProjectile()
    {
        gameObject.SetActive(false);
    }
}
