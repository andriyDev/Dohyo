using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState {  }

[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class Player : MonoBehaviour
{
    [Header("Input")]
    public string horizontalAxisName;
    public string verticalAxisName;
    public string dodgeHorizAxisName;
    public string dodgeVertAxisName;
    public string chargeAxisName;

    [Header("Movement")]
    public float speed = 30;
    public float maxVelocity = 7;
    public bool autoDecelerate = false;
    public float decelerationSpeed = 30;
    public Camera cam;

    [Header("Model")]
    public float modelRotationSpeed = 10;
    public float modelHopTime = .5f;

    [Header("Status")]
    public bool hasCharge = true;

    [Header("References")]
    public GameObject chargeMarker;

    // Movement variables
    private Vector3 lastMove = Vector3.zero;

    private Rigidbody rb;

    // Hop variables
    private bool hopping = false;
    private Vector3 endHop = Vector3.zero;
    private Vector3 startHop = Vector3.zero;
    private float currHopTime = 0;

    // Animation variables
    private Animator anim;
    
	private void Start ()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 1.0f;

        anim = GetComponent<Animator>();

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
        lastMove = new Vector3(moveHorizontal, 0, moveVertical);
        lastMove = Quaternion.Euler(0, Mathf.Atan2(cam.transform.forward.x, cam.transform.forward.z) * Mathf.Rad2Deg, 0) * lastMove;

        if (lastMove.sqrMagnitude == 0)
        {
            rb.AddForce(-rb.velocity.normalized * decelerationSpeed * (rb.velocity.magnitude < Time.fixedDeltaTime ? rb.velocity.magnitude : Time.fixedDeltaTime));
        }
        else
        {
            rb.AddForce(lastMove * speed * Time.fixedDeltaTime);
        }
        ClampVelocity();
    }

    private void SyncModelToState()
    {
        if (chargeMarker != null)
        {
            chargeMarker.SetActive(hasCharge);
        }

        if (hopping)
        {
            currHopTime += Time.deltaTime;
            if (currHopTime >= modelHopTime)
            {
                transform.forward = endHop;
                hopping = false;
            }
            else
            {
                transform.forward = Vector3.Slerp(startHop, endHop, currHopTime / modelHopTime);
            }
        }
        else
        {
            if (lastMove.sqrMagnitude > 0)
            {
                float angleBetween = Vector3.Angle(transform.forward, lastMove);

                if (angleBetween > 90)
                {
                    hopping = true;
                    startHop = transform.forward.normalized;
                    endHop = lastMove.normalized;

                    anim.SetTrigger("Hop");

                    currHopTime = 0;
                }
                else
                {
                    transform.forward = Vector3.RotateTowards(transform.forward, lastMove.normalized, modelRotationSpeed * Time.deltaTime, 0);
                }
            }
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
        return rb.velocity = rb.velocity.normalized * Mathf.Min(rb.velocity.magnitude, maxVelocity);
    }

    private void Lost()
    {
        Debug.Log("You lose!!!");
    }
}
