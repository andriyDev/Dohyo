using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState { Default, Charging, Decelerating, Dodging, Recovering };

[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class Player : MonoBehaviour
{
    [Header("Input")]
    public string horizontalAxisName;
    public string verticalAxisName;
    public string dodgeHorizAxisName;
    public string dodgeVertAxisName;
    public string chargeButtonName;

    [Header("Movement")]
    public float speed = 30;
    public float maxVelocity = 7;
    public float steerScale = 0.4f;
    public bool autoDecelerate = false;
    public float decelerationSpeed = 30;
    public Camera cam;
    public float chargeTime = 0.5f;
    public float chargeScale = 4f;
    public float dodgeTime = 0.1f;
    public float dodgeScale = 5f;
    public float dodgeCooldown = 2f;

    [Header("Model")]
    public float modelRotationSpeed = 10;
    public float modelHopTime = .5f;

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
    
    // Player state variables
    private PlayerState state;
    private bool hasCharge = true;
    private float chargeStart;
    private float dodgeStart;
    private float dodgeEnd = 0;


    private void Start ()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 1.0f;

        anim = GetComponent<Animator>();

        if(cam == null)
        {
            cam = Camera.main;
        }

        state = PlayerState.Default;
    }
	
	private void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        SyncModelToState();

        float stepHorizontal = Input.GetAxis(dodgeHorizAxisName);
        float stepVertical = Input.GetAxis(dodgeVertAxisName);
        Vector3 step = new Vector3(stepHorizontal, 0, stepVertical);
        step = LocalToGlobal(step);

        if (state == PlayerState.Default && Input.GetButtonDown(chargeButtonName) && hasCharge)
        {
            state = PlayerState.Charging;
          //  hasCharge = false;
            chargeStart = Time.time;
            rb.velocity = lastMove.normalized * maxVelocity * chargeScale;
        }

        else if (state == PlayerState.Default && step.magnitude > 0)
        {
            state = PlayerState.Dodging;
            dodgeStart = Time.time;
            rb.velocity = step.normalized * dodgeScale;
        }
    }

    private void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis(horizontalAxisName);
        float moveVertical = Input.GetAxis(verticalAxisName);
        switch (state)
        {
            case PlayerState.Default:
                lastMove = new Vector3(moveHorizontal, 0, moveVertical);
                lastMove = LocalToGlobal(lastMove);

                if (lastMove.sqrMagnitude == 0)
                {
                    rb.AddForce(-rb.velocity.normalized * decelerationSpeed * (rb.velocity.magnitude < Time.fixedDeltaTime ? rb.velocity.magnitude : Time.fixedDeltaTime));
                }
                else
                {
                    rb.AddForce(lastMove * speed * Time.fixedDeltaTime);
                }
                ClampVelocity(maxVelocity);
                break;
            case PlayerState.Charging:
                rb.velocity += LocalToGlobal(new Vector3(moveHorizontal, 0, moveVertical))*steerScale;
                ClampVelocity(maxVelocity*chargeScale);
                if (Time.time - chargeStart > chargeTime)
                {
                    state = PlayerState.Default;
                }
                break;
            case PlayerState.Dodging:
                if (Time.time - dodgeStart > dodgeTime)
                {
                    state = PlayerState.Recovering;
                    dodgeEnd = Time.time;
                    rb.velocity = new Vector3(0, 0, 0);

                }
                break;
            case PlayerState.Recovering:
                if (Time.time - dodgeEnd > dodgeCooldown)
                {
                    state = PlayerState.Default;

                }
                break;
        }
        
        
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

    private void ClampVelocity(float clamp)
    {
        rb.velocity = rb.velocity.normalized * Mathf.Min(rb.velocity.magnitude, clamp);
    }

    private Vector3 LocalToGlobal(Vector3 vector)
    {
        return Quaternion.Euler(0, Mathf.Atan2(cam.transform.forward.x, cam.transform.forward.z) * Mathf.Rad2Deg, 0) * vector;
    }


    private void Lost()
    {
        Debug.Log("You lose!!!");
    }
}
