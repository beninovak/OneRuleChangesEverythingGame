using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
    
    private string  actionName;
    private InputActionPhase actionPhase;
    
    // TODO - make all these private
    [Header("Movement")] // ------ Movement variables ------ //
    public float    moveForceGrounded = 3f;
    public float    moveForceAirborne = 0.1f;
    public float    jumpForce = 23f; // 23f enables jumps of height 3, but not 3.1
    public float    dashForce = 25f;
    public float    startingDrag;
    public float    startingGravityScale;
    public float    fallingGravityScaleMultiplier;
    public float    timeFalling = 0f;
    public float    maxVelocityX = 15f;
    
    private bool    canJump = true;
    public bool     hasDoubleJump = false;
    private bool    wantsToMoveLeft = false;
    private bool    wantsToMoveRight = false;
    private bool    wantsToJump = false;
    private bool    isFalling = false;
    private bool    isGrounded = true;

    public  int     dashCount = 10;
    private Vector2 rightVector = Vector2.right;
    
    [Header("General")] // ------ Other ------ //
    [HideInInspector]
    public  GameController gc;
    private Rigidbody2D    rb;
    private ParticleSystem ps;
    // private string         currentItem = "";
    private Vector2        startingPosition;
    private float          particleSpeed = 50f;

    [Header("Audio")] 
    public  AudioClip    dashSoundEffect;
    private AudioSource  audioSource;
    
    private void Start() {
        ps = GetComponent<ParticleSystem>();
        ps.Stop();
        
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        startingDrag = rb.drag;
        startingGravityScale = rb.gravityScale;
        startingPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (wantsToMoveRight) {
            rb.AddForce(rightVector * (isGrounded ? moveForceGrounded: moveForceAirborne), ForceMode2D.Impulse);
        } else if (wantsToMoveLeft) {
            rb.AddForce(-rightVector * (isGrounded ? moveForceGrounded: moveForceAirborne), ForceMode2D.Impulse);
        } else {
            rb.velocityX = 0f; // TODO - asses for dashing...currently dash is "cancelled" if player stops moving after dash
        }

        if (rb.velocityX > maxVelocityX) {
            rb.velocityX = maxVelocityX;
        } else if (rb.velocityX < maxVelocityX * -1) {
            rb.velocityX = maxVelocityX * -1;
        }

        if (wantsToJump && canJump) {
            if (hasDoubleJump) {
                canJump = true;
                hasDoubleJump = false;
            } else {
                canJump = false;
            }
            
            wantsToJump = false;
            rb.AddForce((gc.isGravityReversed ? Vector2.down : Vector2.up) * jumpForce, ForceMode2D.Impulse);
        }

        float velocityY = rb.velocityY;
        isFalling = (!gc.isGravityReversed && velocityY < 0f) || (gc.isGravityReversed && velocityY > 0f);
        rb.drag = !isFalling ? startingDrag : 0f;

        if (isFalling) {
            timeFalling += Time.deltaTime;
            rb.gravityScale += fallingGravityScaleMultiplier * Time.deltaTime;
        } else {
            timeFalling = 0f;
            rb.gravityScale = startingGravityScale;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        string colliderTag = other.gameObject.tag;
        switch (colliderTag) {
            case "SuicideNet":
                Die();
                // gameObject.transform.position = startingPosition;
                break;
        }
    }
    
    private void OnCollisionEnter2D(Collision2D col) {
        string colliderTag = col.gameObject.tag;

        switch (colliderTag) {
            case "Platform":
            case "Ceiling":
                if (isFalling) {
                    canJump = true;
                    hasDoubleJump = true;
                    rb.velocityX = 0f;
                }
                break;
            
            case "Wall":
                CheckCanJumpConditionsOnWallCollide(col);
                break;
            
            case "Spike":
                if (!gc.isBadGood) {
                    // Destroy(col.gameObject);
                    Die();
                } else if (isFalling) {
                    canJump = true;
                    rb.velocityX = 0f;
                }
                break;
        }
        
        // if (isFalling && other.gameObject.CompareTag("Platform")) {
        //     canJump = true;
        //     rb.velocityX = 0f;
        //     // isGrounded = true;
        //     // rb.drag = startingDrag;
        //     // Debug.Log($"Grounded: {isGrounded}. Name: {other.gameObject.name}");
        // }
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        string colliderTag = col.gameObject.tag;

        switch (colliderTag)
        {
            case "Wall":
                CheckCanJumpConditionsOnWallCollide(col);
                break;
        }
    }

    private void CheckCanJumpConditionsOnWallCollide(Collision2D col)
    {
        float wallTopY = col.gameObject.transform.position.y + (col.gameObject.transform.localScale.y / 2);

        List<ContactPoint2D> cps = new List<ContactPoint2D>();
        col.GetContacts(cps);

        float lowestContactPointY = float.MaxValue;
        float leftmostContactPointX = float.MaxValue;
        float rightmostContactPointX = float.MinValue;
        foreach (ContactPoint2D cp in cps) {
            if (cp.point.y < lowestContactPointY) {
                lowestContactPointY = cp.point.y;
            }
            if (cp.point.x < leftmostContactPointX) {
                leftmostContactPointX = cp.point.x;
            }
            if (cp.point.x > rightmostContactPointX) {
                rightmostContactPointX = cp.point.x;
            }
        }

        if (lowestContactPointY >= wallTopY) {
            canJump = true;
            // float wallLeftX = col.gameObject.transform.position.x - (col.gameObject.transform.localScale.x / 2);
            // float wallRightX = col.gameObject.transform.position.x + (col.gameObject.transform.localScale.x / 2);
            // float playerLeftX = transform.position.x - (transform.localScale.x / 2);
            // float playerRightX = transform.position.x + (transform.localScale.x / 2);
            //
            // if (playerLeftX < wallRightX || playerRightX > wallLeftX) {
            // }
        }
    }

    // private void OnCollisionExit2D(Collision2D other) {
    //     if (other.gameObject.CompareTag("Platform")) {
    //         isGrounded = false;
    //         // Debug.Log($"Grounded: {isGrounded}. Name: {other.gameObject.name}");
    //     }
    // }
    
    private void Die() {
        gc.KillPlayer();
        Destroy(gameObject);
    }
    
    public void Move(InputAction.CallbackContext context) {
        actionName = context.action.name;
        actionPhase = context.action.phase;
        // Debug.Log(actionName);
        // Debug.Log(actionPhase);
        //  InputActionPhase is an enum with values:
        //      - Started = 2
        //      - Performed = 3 
        //      - Canceled = 4 

        switch (actionName, actionPhase) {
            case ("GoLeft", InputActionPhase.Started):
                wantsToMoveLeft = true;
                wantsToMoveRight = false;
                break;
            
            case ("GoLeft", InputActionPhase.Canceled):
                wantsToMoveLeft = false;
                break;
            
            case ("GoRight", InputActionPhase.Started):
                wantsToMoveRight = true;
                wantsToMoveLeft= false;
                break;
            
            case ("GoRight", InputActionPhase.Canceled):
                wantsToMoveRight = false;
                break;
            
            case ("Jump", InputActionPhase.Started):
                wantsToJump = true;
                break;
            
            case ("Jump", InputActionPhase.Performed):
                // wantsToJump = false;
                break;
            
            case ("Jump", InputActionPhase.Canceled):
                wantsToJump = false;
                break;
            
            // TODO - "JumpDown" case??? Consider...
            
            // case ("Jump", InputActionPhase.Canceled):
                // canJump = true; // TODO Set this to false when player falls back to the ground 
                // break;
        }
    }

    public void Dash(InputAction.CallbackContext context) {
        if (context.phase != InputActionPhase.Started || dashCount <= 0) return;
        Vector2 direction = rb.velocityX < 0f ? Vector2.left : Vector2.right;

        var main = ps.main;
        main.startSpeed = particleSpeed * (rb.velocityX < 0f ? 1 : -1);
        ps.Play();
        Invoke(nameof(DisableParticleSystem), 0.5f);
        
        rb.AddForce(dashForce * direction, ForceMode2D.Impulse);
        dashCount--;
        gc.UpdateDashCountText(dashCount);

        CancelInvoke();
        audioSource.resource = dashSoundEffect;
        audioSource.Play();
        Invoke(nameof(OnAudioClipFinishedPlaying), dashSoundEffect.length);
    }

    public void DisableMovement() {
        gameObject.GetComponent<PlayerInput>().enabled = false;
    }

    private void DisableParticleSystem() {
        Debug.Log("STOPPING");
        ps.Stop();
    }

    public void CycleItems(InputAction.CallbackContext context) {
        if (context.phase != InputActionPhase.Started) return;
        gc.CycleItems();
    }
    
    public void UseItem(InputAction.CallbackContext context) {
        if (context.phase != InputActionPhase.Started) return;
        gc.ApplyStatusEffect();
    }

    private void OnAudioClipFinishedPlaying() {
        audioSource.Pause();
        audioSource.resource = null;
    }
}