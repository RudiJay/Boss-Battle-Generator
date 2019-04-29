using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool InputEnabled = false;

    [SerializeField]
    private Rigidbody2D rigidbody;

    [SerializeField]
    private float playerSpeed;

    [SerializeField]
    private Transform projectileSource;

    private float camBorderHeight;
    private float camBorderWidth;
    private Vector3 camPosition;

    // Start is called before the first frame update
    private void Start()
    {
        camBorderHeight = Camera.main.orthographicSize;
        camBorderWidth = camBorderHeight * Camera.main.aspect;
        camPosition = Camera.main.transform.position;
    }

    // Update is called once per frame
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
        float horizontalMove = Input.GetAxis("Horizontal");
        float verticalMove = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontalMove, verticalMove, 0.0f);
        rigidbody.velocity = movement.normalized * playerSpeed;

        //clamp player position
        rigidbody.position = new Vector3(
            Mathf.Clamp(rigidbody.position.x, camPosition.x - camBorderWidth, camPosition.x + camBorderWidth),
            Mathf.Clamp(rigidbody.position.y, camPosition.y - camBorderHeight, camPosition.y + camBorderHeight), 0.0f);
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
