/* 
 * Copyright (C) 2019 Rudi Jay Prentice - All right reserved
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLogic : MonoBehaviour
{
    private Vector3 travelVector;

    [SerializeField]
    private float travelSpeed = 1.0f;
    [SerializeField]
    private float rotationSpeed = 5.0f;

    [SerializeField]
    private bool trackPlayer = false;
    [SerializeField]
    private float trackingTime = 5.0f;
    [SerializeField]
    private float trackingStartupTime = 1.0f;
    [SerializeField]
    private float currentTime = 0.0f;
    
    [SerializeField]
    private float selfDestructTime = 10.0f;

    [SerializeField]
    private Rigidbody2D rb;

    private Transform targetTransform;

    public void SetTravelSpeed(float value)
    {
        travelSpeed = value;
    }

    public void SetRotationSpeed(float value)
    {
        rotationSpeed = value;
    }

    public void SetTargetTransform(Transform transform)
    {
        targetTransform = transform;
    }

    private void OnEnable()
    {
        travelVector = transform.up;
        currentTime = 0.0f;
        Invoke("DisableProjectile", selfDestructTime);
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

            if (trackPlayer && targetTransform != null)
            {
                if (currentTime > trackingStartupTime && currentTime < trackingTime)
                {
                    float rotationStrength = Mathf.Min(rotationSpeed * Time.deltaTime, 1);

                    Vector3 dir = targetTransform.position - transform.position;
                    travelVector = transform.up + dir;
                }
            }

            rb.MovePosition(transform.position + (travelVector.normalized * travelSpeed * Time.fixedDeltaTime));
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
