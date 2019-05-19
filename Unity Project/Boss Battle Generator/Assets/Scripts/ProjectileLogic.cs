/* 
 * Copyright (C) 2019 Rudi Jay Prentice - All right reserved
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLogic : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private SpriteRenderer sr;

    [SerializeField]
    private ProjectileData data;

    private Transform targetTransform;

    private Vector3 travelVector;

    private float currentTime = 0.0f;

    public void SetTargetTransform(Transform transform)
    {
        targetTransform = transform;
    }

    public void SetupProjectileData(ProjectileData value)
    {
        data = value;

        sr.sprite = data.projectileSprite;
        sr.transform.localScale = data.scale;
    }

    private void OnEnable()
    {
        travelVector = transform.up;
        currentTime = 0.0f;
        Invoke("DisableProjectile", data.selfDestructTime);
    }

    private void OnDisable()
    {
        CancelInvoke("DisableProjectile");
    }

    private void FixedUpdate()
    {
        if (gameObject.activeInHierarchy)
        {
            currentTime += Time.fixedDeltaTime;

            if (data.tracksPlayer && targetTransform != null)
            {
                if (currentTime > data.trackingStartupTime && currentTime < data.trackingTime)
                {
                    float rotationStrength = Mathf.Min(data.rotationSpeed * Time.deltaTime, 1);

                    Vector3 dir = targetTransform.position - transform.position;
                    travelVector = transform.up + dir;
                }
            }

            rb.MovePosition(transform.position + (travelVector.normalized * data.travelSpeed * Time.fixedDeltaTime));
        }
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
