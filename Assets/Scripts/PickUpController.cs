using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

public class PickUpController : MonoBehaviour {

    public enum PICK_UP_TYPES {
        NONE,
        BAD_IS_GOOD, 
        REVERSE_GRAVITY,
    };
    
    [Serializable]
    public struct PickUp {
        public PICK_UP_TYPES type;
        public string title;
        public Sprite sprite;
        public float duration;
    }
    
    public PickUp[] possiblePickups;
    private PickUp self;
    public PICK_UP_TYPES typeSelf;
    
    private double yPos, totalYPos, radiansForSin = 0f;
    private bool incrementRadians = true;
    private float bobbingSpeed = 3f, yRotationSpeed = 50f;
    private Random rand = new Random();

    [HideInInspector]
    public GameController gc;
    
    private void Start() {
        if (typeSelf == PICK_UP_TYPES.NONE) {
            Array values = Enum.GetValues(typeof(PICK_UP_TYPES)); 
            typeSelf = (PICK_UP_TYPES)values.GetValue(rand.Next(1, values.Length));
        }
        InitPickup(typeSelf);
    }

    private void FixedUpdate() {
        if (incrementRadians) {
            radiansForSin += Time.deltaTime;
            yPos = Math.Sin(radiansForSin * Time.deltaTime);
        } else {
            radiansForSin -= Time.deltaTime;
            yPos = -Math.Sin(radiansForSin * Time.deltaTime);
        }
        totalYPos += yPos;
        if (totalYPos is <= 0f or >= 1f) {
            incrementRadians = !incrementRadians;
        }
        transform.position += new Vector3(0, (float)yPos, 0);

        transform.Rotate(0, yRotationSpeed * Time.deltaTime, 0);
    }
    
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            OnPickup();
        }
    }
    
    private void OnPickup() {
        gc.ApplyStatusEffect(self);
        Destroy(gameObject);
    }

    private void InitPickup(PICK_UP_TYPES type) {
        foreach (PickUp pickup in possiblePickups) {
            if (pickup.type == type) {
                self.type = type;
                self.title = pickup.title;
                self.sprite = pickup.sprite;
                self.duration = pickup.duration;
                GetComponent<SpriteRenderer>().sprite = self.sprite;
            }
        }
    }
}
