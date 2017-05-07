using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState { Default, Taunting, Charging, Decelerating, BeingCharged, AfterCharged, Dodging, DodgeBumped, Recovering, GameOver, Win };

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
    public int playerId;
    public float tauntLength = 3;
    public float maxSpeedForTaunt = 0.5f;
    public float dodgeTolerance = 3;
    public float timeBeforeMovingCamera = 2;
    public float timeForWinCamera = 1;
    public Vector3 camLocalDist = new Vector3(0, 1.5f, 5);
    public Vector3 winMenuLocalDist = new Vector3(0, 1.5f, 5);
    public float timeForRestart = 3;
    public float bumpScale = 1;

    [Header("Model")]
    public float modelRotationSpeed = 10;

    [Header("References")]
    public GameObject chargeMarker;
    public GameObject noChargeMarker;
    public GameObject birdStuffs;
    public ParticleSystem leftFootPS;
    public ParticleSystem rightFootPS;
    public ParticleSystem bellyPS;
    public ParticleSystem backPS;
    public ParticleSystem chargePS;
    public ParticleSystem dodgeSuccessPS;
    public ParticleSystem tauntPS;
    public ParticleSystem tauntCompletePS;

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
    private Vector3 chargePosition;
    private float tauntStart = 0;

    private Player chargedBy;
    private float afterChargeStartTime;

    private float winTime = 0;
    private bool startedWinAnimation = false;
    private Vector3 winCameraStartPos = Vector3.zero;
    private Vector3 winCameraStartRot = Vector3.zero;

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
        if(FindObjectOfType<Menu>() != null)
        {
            return;
        }

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
                chargePosition = transform.position;
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
            transform.forward = step.normalized;
            dodgePosition = transform.position;
            lastMove = step.normalized;
            anim.SetTrigger("Dodge");
        }

        if (state == PlayerState.Win)
        {
            if(Time.time - winTime < timeForWinCamera)
            {
                cam.transform.position = Vector3.Lerp(winCameraStartPos, transform.position + (Vector3)(transform.localToWorldMatrix * camLocalDist), (Time.time - winTime) / timeForWinCamera);
                cam.transform.forward = Vector3.Slerp(winCameraStartRot, -transform.forward, (Time.time - winTime) / timeForWinCamera);
            }
            else
            {
                cam.transform.position = transform.position + (Vector3)(transform.localToWorldMatrix * camLocalDist);
                cam.transform.forward = -transform.forward;

                if(!startedWinAnimation)
                {
                    startedWinAnimation = true;
                    anim.SetTrigger("Win");
                }
            }

            if(Time.time - winTime > timeForRestart)
            {
                Debug.Log("Hello");
                WinMenu w = FindObjectOfType<WinMenu>();
                w.winningPlayer = playerId;
                w.transform.position = transform.position + (Vector3)(transform.localToWorldMatrix * winMenuLocalDist);
                w.transform.forward = transform.forward;
            }
        }
    }

    private void FixedUpdate()
    {
        if (FindObjectOfType<Menu>() != null)
        {
            return;
        }

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
                    DoDeceleration(deceleration);
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

                DoDeceleration(decelerationSpeed);

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
                    Vector3 a = new Vector3(dodgePosition.x, dodgePosition.z, 1);
                    Vector3 b = new Vector3(transform.position.x, transform.position.z, 1);
                    Vector3 dodgeLine = Vector3.Cross(a, b);
                    Player[] players = FindObjectsOfType<Player>();
                    foreach (Player player in players)
                    {
                        if (player != this && player.state == PlayerState.Charging)
                        {
                            a = new Vector3(player.chargePosition.x, player.chargePosition.z, 1);
                            b = new Vector3(player.transform.position.x, player.transform.position.z, 1);
                            Vector3 chargeLine = Vector3.Cross(a, b);
                            Vector3 poi = Vector3.Cross(dodgeLine, chargeLine);
                            if (poi.z != 0)
                            {
                                poi = new Vector3(poi.x / poi.z, transform.position.y, poi.y / poi.z);
                                Debug.Log(poi);
                                if (((poi - transform.position).magnitude < (dodgePosition - transform.position).magnitude + dodgeTolerance)
                                    && ((poi - player.transform.position).magnitude < (player.chargePosition - player.transform.position).magnitude + dodgeTolerance))
                                    {
                                        Debug.Log("DODGED");
                                        dodged = true;
                                        break;
                                    }
                                }
                            
                        }
                    }

                }
                if (Time.time - dodgeStart > dodgeTime)
                {
                    if (dodged)
                    {
                        state = PlayerState.Default;
                        dodged = false;
                        hasCharge = true;
                        anim.SetTrigger("DodgeSuccessful");
                    }
                    else
                    {
                        state = PlayerState.Recovering;
                        anim.SetTrigger("DodgeFailure");
                    }
                    dodgeEnd = Time.time;
                }
                break;
            case PlayerState.DodgeBumped:
                DoDeceleration(decelerationSpeed);
                if (Time.time - dodgeStart > dodgeTime)
                {
                    state = PlayerState.Recovering;
                    anim.SetTrigger("DodgeFailure");
                    dodgeEnd = Time.time;
                }
                break;
            case PlayerState.Recovering:
                DoDeceleration(decelerationSpeed);
                if (Time.time - dodgeEnd > dodgeCooldown)
                {
                    state = PlayerState.Default;

                }
                break;
            case PlayerState.GameOver:
                DoDeceleration(decelerationSpeed);
                break;
            case PlayerState.Win:
                DoDeceleration(decelerationSpeed);
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
        if (collision.gameObject.tag == "Player")
        {
            Player other = collision.gameObject.GetComponent<Player>();
            if (other)
            {
                if (state == PlayerState.Charging ||  other.state != PlayerState.Charging)
                {
                    float bumpFactor = (state == PlayerState.Charging ? 0.2f : 1);
                    this.rb.velocity += (other.transform.position - this.transform.position).normalized * bumpScale * bumpFactor;
                }
                if (other.state == PlayerState.Charging && state != PlayerState.Charging)
                {
                    other.state = PlayerState.Default;
                    other.anim.SetBool("Charging", false);
                    state = PlayerState.BeingCharged;
                    birdStuffs.SetActive(true);
                    chargedBy = other;
                }
                if (state == PlayerState.Dodging)
                {
                    state = PlayerState.DodgeBumped;

                }
            }
        }
    }

    private void DoDeceleration(float amount)
    {
        if (rb.velocity.magnitude < Time.fixedDeltaTime * amount)
        {
            rb.velocity = Vector3.zero;
        }
        else
        {
            rb.AddForce(-rb.velocity.normalized * amount * Time.fixedDeltaTime, ForceMode.VelocityChange);
            ClampVelocity(maxVelocity);
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
        if(state == PlayerState.GameOver || state == PlayerState.Win)
        {
            return;
        }

        anim.SetBool("Charging", false);
        anim.SetFloat("Speed", 0);
        anim.SetBool("Walking", false);
        anim.SetBool("Taunting", false);

        anim.SetTrigger("Death");
        state = PlayerState.GameOver;

        int stillPlaying = 0;
        Player winner = null;
        Player[] players = FindObjectsOfType<Player>();
        for(int i = 0; i < players.Length && stillPlaying < 2; i++)
        {
            if(players[i].state != PlayerState.GameOver)
            {
                stillPlaying++;
                winner = players[i];
                if(stillPlaying > 1)
                {
                    break;
                }
            }
        }

        if(stillPlaying == 1)
        {
            winner.Win();
        }
    }

    private void Win()
    {
        anim.SetBool("Charging", false);
        anim.SetFloat("Speed", 0);
        anim.SetBool("Walking", false);
        anim.SetBool("Taunting", false);

        state = PlayerState.Win;

        winTime = Time.time + timeBeforeMovingCamera;
        winCameraStartPos = cam.transform.position;
        winCameraStartRot = cam.transform.forward;
    }

    private void EmitAtLeftFoot(int count) { leftFootPS.Emit(count); }
    private void EmitAtRightFoot(int count) { rightFootPS.Emit(count); }
    private void EmitAtBelly(int count) { bellyPS.Emit(count); }
    private void EmitAtBack(int count) { backPS.Emit(count); }
    private void EmitDodgeSuccess(int count) { dodgeSuccessPS.Emit(count); }
    private void EmitTauntParticles(int count) { tauntPS.Emit(count); }
    private void EmitTauntCompleteParticles(int count) { tauntCompletePS.Emit(count); }
}
