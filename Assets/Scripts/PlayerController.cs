using System;
using Mono.Cecil.Cil;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
    
    // ------ Movement variables ------ //
    private string  actionName;
    private float   moveForce = 0.8f,
                    jumpForce = 10f,
                    zRotationDegrees = 0,
                    startingDrag;

    private bool    movingLeft = false, 
                    movingRight = false,
                    canMoveHorizontally = true,
                    canJump = true, 
                    wantsToJump = false, 
                    isFalling = false,
                    isGrounded = true;
    
    private InputActionPhase actionPhase;
    
    // ------ Other ------ //
    private string currentItem = "";
    private Rigidbody2D rb;
    
    private void Start() {
        // Physics.defaultContactOffset = 0.1f;
        
        // Debug.Log(Physics.defaultContactOffset);
        rb = GetComponent<Rigidbody2D>();
        startingDrag = rb.drag;
    }

    private void FixedUpdate() {
        // Debug.Log(rb.velocityY);
        zRotationDegrees = transform.eulerAngles.z;
        // canMoveHorizontally = (isGrounded && zRotationDegrees < 15f || zRotationDegrees > 345f); // 0 degrees at top
        canMoveHorizontally = (isGrounded && zRotationDegrees < 45f || zRotationDegrees > 315f); // 0 degrees at top
        
        if (movingRight && canMoveHorizontally) {
            rb.AddForce(Vector2.right * moveForce, ForceMode2D.Impulse);
        } else if (movingLeft && canMoveHorizontally) {
            rb.AddForce(-Vector2.right * moveForce, ForceMode2D.Impulse);
        }

        if (wantsToJump && canJump) {
            canJump = false;
            wantsToJump = false;
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        
        isFalling = rb.velocityY < 0f;
        rb.drag = isGrounded ? startingDrag : 0f;
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (isFalling && other.gameObject.CompareTag("Platform")) {
            canJump = true;
            isGrounded = true;
            rb.drag = startingDrag;
            Debug.Log($"Grounded: {isGrounded}. Name: {other.gameObject.name}");
        }
    }

    private void OnCollisionExit2D(Collision2D other) {
        if (other.gameObject.CompareTag("Platform")) {
            isGrounded = false;
            Debug.Log($"Grounded: {isGrounded}. Name: {other.gameObject.name}");
        }
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
            
            // case ("Jump", InputActionPhase.Canceled):
                // canJump = true; // TODO Set this to false when player falls back to the ground 
                // break;
        }
    }

    public void UseItem() {
        Debug.Log($"Using item: {currentItem}");
    }
}
