using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
    
    // ------ Movement variables ------ //
    private string  actionName;

    // TODO - make all these private
    public float    moveForceGrounded = 1.3f,
                    moveForceAirborne = 0.1f,
                    jumpForce = 23f, // 23f enables jumps of height 3, but not 3.1
                    startingDrag,
                    startingGravityScale,
                    fallingGravityScaleMultiplier,
                    timeFalling = 0f;
    
    private bool    canJump = true,
                    movingLeft = false,
                    movingRight = false,
                    wantsToJump = false,
                    isFalling = false,
                    isGrounded = true;

    private Vector2 rightVector = Vector2.right;
    
    private InputActionPhase actionPhase;
    
    // ------ Other ------ //
    public GameController gc;

    private Vector2 startingPosition;
    private string currentItem = "";
    private Rigidbody2D rb;
    private bool gravityDirection = true; // true for down, false for up
    
    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        startingDrag = rb.drag;
        startingGravityScale = rb.gravityScale;
        startingPosition = transform.position;
    }

    private void FixedUpdate() {
        if (movingRight) {
            rb.AddForce(rightVector * (isGrounded ? moveForceGrounded: moveForceAirborne), ForceMode2D.Impulse);
        } else if (movingLeft) {
            rb.AddForce(-rightVector * (isGrounded ? moveForceGrounded: moveForceAirborne), ForceMode2D.Impulse);
        } else {
            rb.velocityX = 0f;
        }

        if (wantsToJump && canJump) {
            canJump = false;
            wantsToJump = false;
            rb.AddForce((gc.isGravityReversed ? Vector2.down : Vector2.up) * jumpForce, ForceMode2D.Impulse);
        }

        float velocityY = rb.velocityY;
        isFalling = (!gc.isGravityReversed && velocityY < 0f) || (gc.isGravityReversed && velocityY > 0f);
        rb.drag = !isFalling ? startingDrag : 0f;

        if (isFalling) {
            timeFalling += Time.deltaTime; // TODO - this shouldn't happen in gravity scale is negative ( gravity is inverted )
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
                gameObject.transform.position = startingPosition;
                break;
        }
    }
    
    private void OnCollisionEnter2D(Collision2D other) {
        string colliderTag = other.gameObject.tag;

        switch (colliderTag) {
            case "Platform":
            case "Ceiling":
                if (isFalling) {
                    canJump = true;
                    rb.velocityX = 0f;
                }
                break;
            
            case "Spike":
                if (!gc.isBadGood) {
                    Destroy(other.gameObject);
                    Die();
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
                movingLeft = true;
                movingRight = false;
                break;
            
            case ("GoLeft", InputActionPhase.Canceled):
                movingLeft = false;
                break;
            
            case ("GoRight", InputActionPhase.Started):
                movingRight = true;
                movingLeft= false;
                break;
            
            case ("GoRight", InputActionPhase.Canceled):
                movingRight = false;
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

    public void DisableMovement() {
        gameObject.GetComponent<PlayerInput>().enabled = false;
    }

    public void UseItem() {
        Debug.Log($"Using item: {currentItem}");
    }
}
