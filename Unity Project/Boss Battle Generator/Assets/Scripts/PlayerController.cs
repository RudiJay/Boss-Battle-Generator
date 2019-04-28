using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool InputEnabled = false;

    [SerializeField]
    private Rigidbody2D rigidbody;

    // Start is called before the first frame update
    private void Start()
    {
        
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
        rigidbody.AddForce(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")), ForceMode2D.Force);
    }
}
