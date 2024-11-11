using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLineController : MonoBehaviour {
    [HideInInspector]
    public GameController gc;
    
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            gc.FinishLevel();
            Destroy(gameObject);
        }
    }
}