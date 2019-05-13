/* 
 * Copyright (C) 2019 Rudi Jay Prentice - All right reserved
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool InputEnabled = false;

    [SerializeField]
    private Rigidbody2D rigidbody;

    [SerializeField]
    private float playerMovementSpeed = 1.0f;
    [SerializeField]
    private float rotationSpeed = 0.5f;

    [SerializeField]
    private Transform projectileSource;

    private Transform bossTransform;

    private float camBorderHeight;
    private float camBorderWidth;
    private Vector3 centrepoint;

    public void SetBossTransform(Transform transform)
    {
        bossTransform = transform;
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (InputEnabled)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                FireProjectile();
            }
        }
    }

    private void FixedUpdate()
    {
        if (InputEnabled)
        {
            float horizontalMove = Input.GetAxis("Horizontal");
            float verticalMove = Input.GetAxis("Vertical");

            Vector3 movement = new Vector3(horizontalMove, verticalMove, 0.0f);
            rigidbody.velocity = movement.normalized * playerMovementSpeed;

            //clamp player position
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, centrepoint.x - camBorderWidth, centrepoint.x + camBorderWidth),
                Mathf.Clamp(transform.position.y, centrepoint.y - camBorderHeight, centrepoint.y + camBorderHeight), 0.0f);

            if (bossTransform != null)
            {
                float rotationStrength = Mathf.Min(rotationSpeed * Time.deltaTime, 1);

                Vector3 bossDirection = bossTransform.position - transform.position;
                float targetAngle = Vector3.SignedAngle(Vector3.up, bossDirection, Vector3.forward);
                Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationStrength);
            }
        }
    }

    public void SetUpEdgeBoundaries(Vector3 position)
    {
        centrepoint = position;
        camBorderHeight = Camera.main.orthographicSize;
        camBorderWidth = camBorderHeight * Camera.main.aspect;
    }

    private void FireProjectile()
    {
        GameObject projectile = ProjectileManager.Instance.GetProjectileObject();

        if (projectile != null)
        {
            projectile.transform.position = projectileSource.position;
            projectile.transform.rotation = projectileSource.rotation;
            projectile.SetActive(true);
        }
        else
        {
            Debug.Log("ERROR: Available Projectile Not Found");
        }
    }
}
