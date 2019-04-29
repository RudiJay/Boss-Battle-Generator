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
}
