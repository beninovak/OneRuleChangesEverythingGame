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
        DOUBLE_JUMP,
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
    public float durationSelf;
    public bool applySelfOnPickup;
    
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
        InitPickup(typeSelf, durationSelf);
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
        PickUp pickUpClone = new PickUp {
            type = self.type,
            title = self.title,
            sprite = self.sprite,
            duration = self.duration,
        };

        if (applySelfOnPickup) {
            gc.ApplyStatusEffect(pickUpClone);
            Destroy(gameObject);
        } else if (gc.AddItemToHotbar(pickUpClone)) {
            Destroy(gameObject);
        }
    }

    private void InitPickup(PICK_UP_TYPES type, float duration = 0f) {
        foreach (PickUp pickup in possiblePickups) {
            if (pickup.type == type) {
                self.type = type;
                self.title = pickup.title;
                self.sprite = pickup.sprite;
                self.duration = (duration > 0f ? duration : pickup.duration);
                GetComponent<SpriteRenderer>().sprite = self.sprite;
            }
        }
    }
}