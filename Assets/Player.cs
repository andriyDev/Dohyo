using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [Header("Input")]
    public string horizontalAxisName;
    public string verticalAxisName;

    [Header("Movement")]
    public float speed = 30;
    public float max_vel = 7;
    public Camera cam;

    [Header("Status")]
    public bool hasCharge = true;

    [Header("References")]
    public GameObject chargeMarker;

    // Movement variables
    private Rigidbody rb;
    
	private void Start ()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 1.0f;

        if(cam == null)
        {
            cam = Camera.main;
        }
    }
	
	private void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        SyncModelToState();
	}

    private void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis(horizontalAxisName);
        float moveVertical = Input.GetAxis(verticalAxisName);
        Vector3 movement = new Vector3(moveHorizontal, 0, moveVertical);
        movement = Quaternion.Euler(0, Mathf.Atan2(cam.transform.forward.x, cam.transform.forward.z) * Mathf.Rad2Deg, 0) * movement;
        rb.AddForce(movement * speed);
        ClampVelocity();
    }

    private void SyncModelToState()
    {
        if (chargeMarker != null)
        {
            chargeMarker.SetActive(hasCharge);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Ring")
        {
            Lost();
        }
    }

    private Vector3 ClampVelocity()
    {
        return rb.velocity = rb.velocity.normalized * Mathf.Min(rb.velocity.magnitude, max_vel);
    }

    private void Lost()
    {
        Debug.Log("You lose!!!");
    }
}
