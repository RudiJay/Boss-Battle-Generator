﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 1.0f;

    [SerializeField]
    private float selfDestructTime = 10.0f;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.AddForce(-transform.up * 100);
        }

        Invoke("DestroyProjectile", selfDestructTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        DestroyProjectile();
    }

    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}
