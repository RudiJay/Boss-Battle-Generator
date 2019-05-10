/* 
 * Copyright (C) 2019 Rudi Jay Prentice - All right reserved
 */

using UnityEngine;

public class Weapon : MonoBehaviour
{
    private bool generatorActive = false;

    [SerializeField]
    private WeaponOrientationMode currentOrientationMode;

    [SerializeField]
    private Transform pivot;
    public Transform sprite;
    public Transform attackSource;

    private Transform target;
    private bool trackTarget = false;

    [SerializeField]
    private Transform fakeTarget;
    [SerializeField]
    private float randomiseSwivelThreshold = 5f;

    [SerializeField]
    private float rotationSpeed = 1.0f;

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

    public bool GetCollidingWithOtherWeapon()
    {
        return isCollidingWithOtherWeapon;
    }

    private void Start()
    {
        CheckGeneratorActive();
        //apply as listener to generator activation on gamemanager
        GameManager.Instance.RegisterAsGeneratorListener(CheckGeneratorActive);

        isCollidingWithOtherWeapon = false;

        RandomiseFakeTargetPosition();
    }

    private void OnDestroy()
    {
        GameManager.Instance.DeregisterAsGeneratorListener(CheckGeneratorActive);
    }

    private void Update()
    {
        if (trackTarget)
        {
            float targetAngle = 0.0f;
            float rotationStrength = Mathf.Min(rotationSpeed * Time.deltaTime, 1);

            Vector3 dir = Vector3.zero;

            if (generatorActive)
            {
                dir = fakeTarget.position - pivot.position;
                targetAngle = Vector3.SignedAngle(Vector3.down, dir, Vector3.forward);

                float targetDelta = Vector3.Angle(-pivot.up, dir);

                if (targetDelta <= randomiseSwivelThreshold)
                {
                    RandomiseFakeTargetPosition();
                }
            }

            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

            pivot.rotation = Quaternion.Lerp(pivot.rotation, targetRotation, rotationStrength);
        }
    }

    public WeaponOrientationMode CurrentOrientationMode
    {
        get { return currentOrientationMode; }

        set
        {
            currentOrientationMode = value;

            if (currentOrientationMode == WeaponOrientationMode.ROTATABLE)
            {
                Debug.Log("BUG");
                trackTarget = true;
            }
        }
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
