using System;
using UnityEngine;

public class GameController : MonoBehaviour {
    public string statusEffect;
    
    private void Awake() {
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().gc = this;
        GameObject[] pickups = GameObject.FindGameObjectsWithTag("Pickup");
        foreach (GameObject pickup in pickups) {
            pickup.GetComponent<PickUpController>().gc = this;
        }
    }
}
