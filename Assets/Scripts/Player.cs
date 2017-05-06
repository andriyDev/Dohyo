using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState { Default, Taunting, Charging, Decelerating, BeingCharged, AfterCharged, Dodging, Recovering };

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
    public float afterChargedLength = 2;
    public float afterChargedDecelerationAmt = 350;
    public float afterChargedSpeed = 350;

    [Header("Extra")]
    public float tauntLength = 3;
    public float maxSpeedForTaunt = 0.5f;
    public float dodgeTolerance = 3;

    [Header("Model")]
    public float modelRotationSpeed = 10;

    [Header("References")]
    public GameObject chargeMarker;
    public GameObject noChargeMarker;
    public GameObject birdStuffs;

    // Movement variables
    private Vector3 lastMove = Vector3.zero;

    private Rigidbody rb;

    // Animation variables
    private Animator anim;
    
    // Player state variables
    private PlayerState state;
    private bool hasCharge = true;
    private float chargeStart;
    private float dodgeStart;
    private float dodgeEnd = 0;
    private bool dodged = false;
    private Vector3 dodgePosition;
    private float tauntStart = 0;

    private Player chargedBy;
    private float afterChargeStartTime;

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

        if (state == PlayerState.Default && Input.GetButtonDown(chargeButtonName))
        {
            if (hasCharge)
            {
                state = PlayerState.Charging;
                anim.SetBool("Charging", true);
                hasCharge = false;
                chargeStart = Time.time;
                if (lastMove.magnitude == 0)
                {
                    lastMove = transform.forward.normalized;
                }
                rb.velocity = lastMove.normalized * maxVelocity * chargeScale;
            }
            else if (rb.velocity.magnitude < maxSpeedForTaunt)
            {
                state = PlayerState.Taunting;

                tauntStart = Time.time;
            }
        }

        else if (state == PlayerState.Default && step.magnitude > 0.8)
        {
            state = PlayerState.Dodging;
            dodgeStart = Time.time;
            rb.velocity = step.normalized * dodgeScale;
            dodgePosition = transform.position;
        }
    }

    private void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis(horizontalAxisName);
        float moveVertical = Input.GetAxis(verticalAxisName);

        switch (state)
        {
            case PlayerState.Default:
            case PlayerState.AfterCharged:
                lastMove = new Vector3(moveHorizontal, 0, moveVertical);
                lastMove = LocalToGlobal(lastMove);
                float deceleration = (state == PlayerState.AfterCharged ? afterChargedDecelerationAmt : decelerationSpeed);
                float usedSpeed = (state == PlayerState.AfterCharged ? afterChargedSpeed : speed);
                if (lastMove.sqrMagnitude == 0)
                {
                    if (rb.velocity.magnitude < Time.fixedDeltaTime * deceleration)
                    {
                        rb.velocity = Vector3.zero;
                    }
                    else
                    {
                        rb.AddForce(-rb.velocity.normalized * deceleration * Time.fixedDeltaTime, ForceMode.VelocityChange);
                        ClampVelocity(maxVelocity);
                    }
                }
                else
                {
                    rb.AddForce(lastMove * usedSpeed * Time.fixedDeltaTime);
                    ClampVelocity(maxVelocity);
                }

                if (state == PlayerState.AfterCharged && Time.time - afterChargeStartTime > afterChargedLength)
                {
                    state = PlayerState.Default;
                    birdStuffs.SetActive(false);
                }

                break;
            case PlayerState.Taunting:

                if (rb.velocity.magnitude < Time.fixedDeltaTime * decelerationSpeed)
                {
                    rb.velocity = Vector3.zero;
                }
                else
                {
                    rb.AddForce(-rb.velocity.normalized * decelerationSpeed * Time.fixedDeltaTime, ForceMode.VelocityChange);
                    ClampVelocity(maxVelocity);
                }

                if (Time.time - tauntStart > tauntLength)
                {
                    state = PlayerState.Default;
                    hasCharge = true;
                }
                break;
            case PlayerState.Charging:
                rb.velocity += LocalToGlobal(new Vector3(moveHorizontal, 0, moveVertical)) * steerScale;
                ClampVelocity(maxVelocity * chargeScale);
                if (Time.time - chargeStart > chargeTime)
                {
                    state = PlayerState.Default;

                    anim.SetBool("Charging", false);
                }
                break;
            case PlayerState.BeingCharged:
                if (chargedBy.state != PlayerState.Charging)
                {
                    state = PlayerState.AfterCharged;
                    afterChargeStartTime = Time.time;
                }
                break;
            case PlayerState.Dodging:
                if (!dodged)
                {
                    Vector3 dir = dodgePosition - transform.position;
                    RaycastHit hitInfo;
                    if (Physics.BoxCast(transform.position + dir / 2, new Vector3(dodgeTolerance, 0.5f, dir.magnitude/2), dir, out hitInfo)) {
                        Debug.Log("DODGED!");
                        Player target = hitInfo.rigidbody.GetComponent<Player>();
                        dodged = target != this && target.state == PlayerState.Charging;
                    }
                }
                if (Time.time - dodgeStart > dodgeTime)
                {
                    if (dodged)
                    {
                        state = PlayerState.Default;
                        dodged = false;
                        hasCharge = true;
                    }
                    else
                    {
                        state = PlayerState.Recovering;
                    }
                    dodgeEnd = Time.time;
                    rb.velocity = new Vector3(0, 0, 0);
                }
                break;
            case PlayerState.Recovering:
                Debug.Log("Recovering");
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
        if (noChargeMarker != null)
        {
            noChargeMarker.SetActive(!hasCharge);
        }
        float speedVar = rb.velocity.magnitude / maxVelocity;
        anim.SetFloat("Speed", speedVar);

        anim.SetBool("Taunting", state == PlayerState.Taunting);

        if (lastMove.sqrMagnitude > 0)
        {
            transform.forward = Vector3.RotateTowards(transform.forward, lastMove.normalized, modelRotationSpeed * Time.deltaTime, 0);
        }

        anim.SetBool("Walking", lastMove.sqrMagnitude > 0);
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Ring")
        {
            Lost();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collided");
        if (collision.gameObject.tag == "Player")
        {
            Player other = collision.gameObject.GetComponent<Player>();
            if (other)
            {
                if (other.state == PlayerState.Charging && state != PlayerState.Charging)
                {
                    state = PlayerState.BeingCharged;
                    birdStuffs.SetActive(true);
                    chargedBy = other;
                }
            }
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
