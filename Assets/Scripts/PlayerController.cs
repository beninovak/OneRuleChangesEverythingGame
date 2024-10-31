using System;
using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
    
    // ------ Movement variables ------ //
    private string  actionName;
    private float   moveForce = 10f, jumpForce = 10f;
    private bool    movingLeft = false, 
                    movingRight = false, 
                    canJump = true, 
                    wantsToJump = false, 
                    isFalling = true ; // TODO - change to false, because player will start on the ground
    
    private InputActionPhase actionPhase;
    
    // ------ Other ------ //
    private string currentItem = "";
    private Rigidbody2D rb;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {
        Debug.Log(rb.linearVelocityY);
        if (movingRight) {
            rb.AddForce(transform.right * moveForce);
        } else if (movingLeft) {
            rb.AddForce(-transform.right * moveForce);
        }

        if (wantsToJump && canJump) {
            
            Debug.Log("TRYING JUMP");
            
            canJump = false;
            wantsToJump = false;
            
            rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        Debug.Log($"COLLIDING WITH: {other.gameObject.name}");
        Debug.Log($"COLLIDING WITH TAG: {other.gameObject.tag}");

        if (isFalling && other.gameObject.CompareTag("Player")) {
            canJump = true;
            Debug.Log("CAN JUMP AGAIN");
        }
    }

    public void Move(InputAction.CallbackContext context) {
        actionName = context.action.name;
        actionPhase = context.action.phase;
        Debug.Log(actionName);
        Debug.Log(actionPhase);
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
