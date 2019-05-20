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
    private Collider2D col;

    [SerializeField]
    private ProjectileData data; //serialised for runtime debugging

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

        if (data.isPlayerProjectile)
        {
            col.gameObject.layer = LayerMask.NameToLayer("PlayerAttack");
        }
        else
        {
            col.gameObject.layer = LayerMask.NameToLayer("BossAttack");
        }

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

            PlayerController player = other.attachedRigidbody.GetComponent<PlayerController>();
            if (player != null)
            {
                player.DamagePlayer();
            }
        }
        else if (other.tag == "Boss")
        {
            DisableProjectile();

            BossLogic bossLogic = other.attachedRigidbody.GetComponent<BossLogic>();
            if (bossLogic != null)
            {
                bossLogic.DamageBoss(data.damage);
            }
        }
    }

    private void DisableProjectile()
    {
        gameObject.SetActive(false);
    }
}
