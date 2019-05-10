/* 
 * Copyright (C) 2019 Rudi Jay Prentice - All right reserved
 */

using UnityEngine;

public class Weapon : MonoBehaviour
{
    private bool generatorActive = false;
    private bool behaviourActive = false;

    [SerializeField]
    private WeaponOrientationMode currentOrientationMode;
    private bool rotatable = false;

    [SerializeField]
    private Transform pivot;
    public Transform sprite;
    public Transform attackSource;

    private Transform target;
    private bool tracksPlayer = false;

    [SerializeField]
    private Transform fakeTarget;
    [SerializeField]
    private float randomiseSwivelThreshold = 5f;

    [SerializeField]
    private float rotationSpeed = 2.5f, trackPlayerSpeed = 5f;

    public Weapon mirrorPair { get; set; }

    private bool isCollidingWithOtherWeapon = false;

    public void CheckGeneratorActive()
    {
        generatorActive = GameManager.Instance.GetGeneratorActive();
    }

    public void SetTargetTransform(Transform value)
    {
        target = value;
    }

    public void SetRotatable(bool inTracksPlayer = false)
    {
        rotatable = true;
        tracksPlayer = inTracksPlayer;
    }

    public bool GetCollidingWithOtherWeapon()
    {
        return isCollidingWithOtherWeapon;
    }

    private void OnEnable()
    {
        CheckGeneratorActive();
        //apply as listener to generator activation on gamemanager
        GameManager.Instance.RegisterAsGeneratorListener(CheckGeneratorActive);

        if (target == null)
        {
            target = GameManager.Instance.GetPlayerTransform();
        }

        mirrorPair = null;

        pivot.rotation = Quaternion.identity;
        rotatable = false;
        tracksPlayer = false;
        isCollidingWithOtherWeapon = false;
    }

    private void OnDisable()
    {
        GameManager.Instance.DeregisterAsGeneratorListener(CheckGeneratorActive);        
    }

    private void Update()
    {
        if (rotatable)
        {
            float targetAngle = 0.0f;
            float speed = 0.0f;

            Vector3 dir = Vector3.zero;

            if (generatorActive || !tracksPlayer)
            {
                speed = rotationSpeed;
                dir = fakeTarget.position - pivot.position;

                float targetDelta = Vector3.Angle(-pivot.up, dir);

                if (targetDelta <= randomiseSwivelThreshold)
                {
                    RandomiseFakeTargetPosition();
                }
            }
            else if (!generatorActive && tracksPlayer && target != null)
            {
                speed = trackPlayerSpeed;
                dir = target.position - pivot.position;
            }

            float rotationStrength = Mathf.Min(speed * Time.deltaTime, 1);

            targetAngle = Vector3.SignedAngle(Vector3.down, dir, Vector3.forward);

            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

            pivot.rotation = Quaternion.Lerp(pivot.rotation, targetRotation, rotationStrength);
        }
    }

    public WeaponOrientationMode CurrentOrientationMode
    {
        get { return currentOrientationMode; }
        set { currentOrientationMode = value; }
    }

    private void RandomiseFakeTargetPosition()
    {
        float x = Random.Range(-10, 10);
        float y = Random.Range(-10, 10);

        fakeTarget.position = new Vector3(x, y, fakeTarget.position.z);
    }

    public void SetWeaponRotation(float rot)
    {
        transform.rotation = Quaternion.Euler(0, 0, rot);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Weapon")
        {
            if (!isCollidingWithOtherWeapon)
            {
                isCollidingWithOtherWeapon = true;
            }
        }
    }
}
