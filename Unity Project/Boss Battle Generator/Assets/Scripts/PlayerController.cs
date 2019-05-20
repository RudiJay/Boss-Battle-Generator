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
    [SerializeField]
    private ProjectileData playerProjectile;

    private Transform bossTransform;

    [SerializeField]
    private float camBorderOffset = 2.5f;
    private float camBorderHeight;
    private float camBorderWidth;
    private Vector3 centrepoint;

    public void SetBossTransform(Transform transform)
    {
        bossTransform = transform;
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
        rigidbody.angularVelocity = 0.0f;

        if (InputEnabled)
        {
            float horizontalMove = Input.GetAxis("Horizontal");
            float verticalMove = Input.GetAxis("Vertical");

            Vector3 movement = new Vector3(horizontalMove, verticalMove, 0.0f);
            rigidbody.velocity = movement.normalized * playerMovementSpeed;

            //clamp player position
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, centrepoint.x - camBorderWidth + camBorderOffset, centrepoint.x + camBorderWidth - camBorderOffset),
                Mathf.Clamp(transform.position.y, centrepoint.y - camBorderHeight + camBorderOffset, centrepoint.y + camBorderHeight - camBorderOffset), 0.0f);

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

    public void DamagePlayer()
    {
        InputEnabled = false;
        gameObject.SetActive(false);
        GameManager.Instance.ExitPlayMode();
    }

    public void SetUpEdgeBoundaries(Vector3 position)
    {
        centrepoint = position;
        camBorderHeight = Camera.main.orthographicSize;
        camBorderWidth = camBorderHeight * Camera.main.aspect;
    }

    private void FireProjectile()
    {
        ProjectileLogic projectile = ProjectileManager.Instance.GetProjectile();

        if (projectile != null)
        {
            projectile.SetupProjectileData(playerProjectile);
            projectile.transform.position = projectileSource.position;
            projectile.transform.rotation = projectileSource.rotation;
            projectile.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("ERROR: Available Projectile Not Found");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.attachedRigidbody.tag == "Boss")
        {
            DamagePlayer();
        }
    }
}
