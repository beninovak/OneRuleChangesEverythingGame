using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static PickUpController;

public class GameController : MonoBehaviour {

    public string          levelName;
    
    public GameObject      HUDCanvas;
    public Image           levelNameBackgroundImage;
    public Image           finalTimeBackgroundImage;

    [HideInInspector]
    public PickUp          statusEffect;
    public RawImage        statusEffectImage;
    public TextMeshProUGUI statusEffectText;
    public TextMeshProUGUI statusEffectRemainingDurationText;
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI levelTimeText;
    public TextMeshProUGUI finalTimeText;
    public GameObject      mainMessage;

    private bool           hasLevelFinished = false;
    private bool           shouldFadeLevelNameText = true; 
    private bool           hasActiveStatusEffect = false;
    private float          levelStartTimestamp;
    private float          levelFinalTime;
    private float          timeSincePickupUsed = 0f;
    private float          levelNameFadeDuration = 2f;
    private float          pickupNotificationFadeDuration = 2f;
    private float          timeAfterFinishBeforeSceneSwitch = 3f;

    private Color          HUDTextColorShown = new Color(1f, 1f, 1f, 1f);
    private Color          HUDTextColorHidden = new Color(1f, 1f, 1f, 0f);
    private Color          bigMessageBackgroundColorShown;
    private Color          bigMessageBackgroundColorHidden;
    
    /* General */
    private GameObject player;
    private PlayerController pc;
    private BoxCollider2D pcBC2D;
    private Rigidbody2D pcRb;

    /* Status effects */
    public bool isGravityReversed = false; 
    public bool isBadGood = false;
    
    private Image[]        abilityIcons = new Image[3];
    private Image[]        abilityIconBorders = new Image[3];
    private int            selectedItemIndex = -1;
    public List<PickUp>    availablePickups = new ();
    
    private void Awake() {
        // Debug.Log($"Previous best time: {GameVariables.SCENE_TIMES[SceneManager.GetActiveScene().buildIndex - 1]}");
        levelStartTimestamp = Time.time;
        player = GameObject.FindGameObjectWithTag("Player");
        pc = player.GetComponent<PlayerController>();
        pcBC2D = player.GetComponent<BoxCollider2D>();
        pcRb = player.GetComponent<Rigidbody2D>();
        pc.gc = this;
        
        //  TODO - check if pickups and finishLines can be merged into a single array??
        GameObject[] pickups = GameObject.FindGameObjectsWithTag("Pickup");
        foreach (GameObject pickup in pickups) {
            pickup.GetComponent<PickUpController>().gc = this;
        }
        
        GameObject[] finishLines = GameObject.FindGameObjectsWithTag("FinishLine");
        foreach (GameObject finishLine in finishLines) {
            finishLine.GetComponent<FinishLineController>().gc = this;
        }

        GameObject[] abilityIconGameObjects = GameObject.FindGameObjectsWithTag("AbilityIconUI").OrderBy(el => el.name).ToArray();;
        for (int i = 0; i < 3 ; i++) {
            abilityIcons[i] = abilityIconGameObjects[i].GetComponent<Image>();
        }
        
        // Have to order this because FindGameObjectsWithTag() doesn't order things consistently
        GameObject[] abilityIconBordersGameObjects = GameObject.FindGameObjectsWithTag("AbilityIconBorderUI").OrderBy(el => el.name).ToArray();
        for (int i = 0; i < 3 ; i++) {
            abilityIconBorders[i] = abilityIconBordersGameObjects[i].GetComponent<Image>();
        }


        bigMessageBackgroundColorShown = levelNameBackgroundImage.color;
        bigMessageBackgroundColorHidden = new Color(bigMessageBackgroundColorShown.r, bigMessageBackgroundColorShown.g, bigMessageBackgroundColorShown.b, 0f);
        levelNameText.text = levelName;
        GameVariables.CURRENT_LEVEL_NAME = levelName;
    }

    private void Update() {
        if (shouldFadeLevelNameText) {
            HUDFadeInOut("levelName");
        }

        if (hasLevelFinished) {
            HUDFadeInOut("finalTime");
        } else {
            levelTimeText.text = $"{Time.time - levelStartTimestamp:0.00}s";
            
            if (hasActiveStatusEffect) {
                HUDFadeInOut("statusEffect");
            }
        }
    }

    // TODO - REMOVE
    // TEMP DON'T NEED THIS
    private void FixedUpdate() {
        Debug.Log(isBadGood);
    }

    public void KillPlayer() {
        mainMessage.GetComponent<TextMeshProUGUI>().text = "YOU DIED";
        mainMessage.SetActive(true);
        
        Invoke(nameof(ResetLevel), 2f);
    }

    private void ResetLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void FinishLevel() {
        pc.DisableMovement();
        hasLevelFinished = true;
        levelFinalTime = Time.time - levelStartTimestamp;
        finalTimeText.text = $"{levelFinalTime:0.00}s";
        GameVariables.SCENE_TIMES[SceneManager.GetActiveScene().buildIndex - 1] = levelFinalTime; // TODO - only for testing...store this in a file
        
        // TODO - Check final time against previous final time and show "gz" message in UI
        
        Invoke(nameof(LoadNextScene), timeAfterFinishBeforeSceneSwitch);
    }

    private void LoadNextScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void HUDFadeInOut(string name) { // TODO - maybe rename to something more sensible, given what the "statusEffect" case does

        switch (name) {
            
            case "levelName":
                if (Time.time - levelNameFadeDuration >= levelStartTimestamp) {
                    shouldFadeLevelNameText = false;
                    levelNameText.color = new Color(1f, 1f, 1f, 0);
                    levelNameBackgroundImage.color = new Color(1f, 1f, 1f, 0);
                } else {
                    levelNameText.color = Color.Lerp(levelNameText.color, HUDTextColorHidden, levelNameFadeDuration * Time.deltaTime);
                    levelNameBackgroundImage.color = Color.Lerp(levelNameBackgroundImage.color, bigMessageBackgroundColorHidden, levelNameFadeDuration * Time.deltaTime);
                }
                break;
            
            case "statusEffect":
                timeSincePickupUsed += Time.deltaTime;
                statusEffectRemainingDurationText.text = $"{statusEffect.duration - timeSincePickupUsed:0.0}s"; // Wacky formatting ( string interpolation )
        
                if (timeSincePickupUsed >= statusEffect.duration) {
                    RemoveStatusEffect();
                    return;
                }
        
                if (timeSincePickupUsed <= pickupNotificationFadeDuration) {
                    statusEffectText.color = Color.Lerp(statusEffectText.color, HUDTextColorShown, pickupNotificationFadeDuration * Time.deltaTime);
                    return;
                } 
        
                // TODO - could probably optimize this if else
                if (timeSincePickupUsed <= 2 * pickupNotificationFadeDuration) {
                    statusEffectText.color = Color.Lerp(statusEffectText.color, HUDTextColorHidden, pickupNotificationFadeDuration * Time.deltaTime);
                } else {
                    statusEffectText.color = HUDTextColorHidden;
                }
                break;
            
            case "finalTime":
                finalTimeText.color = Color.Lerp(finalTimeText.color, HUDTextColorShown, timeAfterFinishBeforeSceneSwitch * Time.deltaTime);
                finalTimeBackgroundImage.color = Color.Lerp(finalTimeBackgroundImage.color, bigMessageBackgroundColorShown, timeAfterFinishBeforeSceneSwitch * Time.deltaTime);
                break;
        }
    }

    public bool AddItemToHotbar(PickUp item) {
        if (availablePickups.Count >= 3) return false;
        
        abilityIcons[availablePickups.Count].sprite = item.sprite;
        abilityIcons[availablePickups.Count].color = new Color(1f, 1f, 1f, 1f);
        availablePickups.Insert(availablePickups.Count, item);
        return true;
    }
    
    public void CycleItems() {
        if (availablePickups.Count == 0) return;
        
        selectedItemIndex++;
        if (selectedItemIndex >= availablePickups.Count) { // Max three items
            selectedItemIndex = 0;
        }
        
        for (int i = 0; i < availablePickups.Count; i++) {
            abilityIconBorders[i].color = new Color(1f, 1f, 1f, 1f);
            if (i == selectedItemIndex) {
                abilityIconBorders[selectedItemIndex].color = new Color(1f, 0.25f, 0.25f, 1f);
            }
        }
    }
    
    public void ApplyStatusEffect() {
        if (availablePickups.Count == 0 || selectedItemIndex < 0) return;
        
        timeSincePickupUsed = 0f;
        HUDCanvas.SetActive(true);
        statusEffect = availablePickups.ElementAt(selectedItemIndex);
        hasActiveStatusEffect = true;
        statusEffectImage.texture = ConvertSpriteToTexture2D(statusEffect.sprite);
        statusEffectImage.color = new Color(1f, 1f, 1f, 1f);
        statusEffectText.text = statusEffect.title;

        switch (statusEffect.type) {
            case PICK_UP_TYPES.REVERSE_GRAVITY:
                ReverseGravity();
                break;
            
            case PICK_UP_TYPES.BAD_IS_GOOD:
                isBadGood = true;
                break;
        }
        
        // UI and general cleanup after "consuming" the pickup 
        abilityIconBorders[selectedItemIndex].color = new Color(1f, 1f, 1f, 1f);
        // Adjust sprites of hotbar to reflect an item being used        
        for (int i = selectedItemIndex; i < availablePickups.Count; i++) {

            // Last pickup box
            if (i == availablePickups.Count - 1) {
                abilityIcons[i].sprite = null;
                abilityIcons[i].color = new Color(1f, 1f, 1f, 0f);
                break;
            }
            
            abilityIcons[i].sprite = abilityIcons[i + 1].sprite;
        }
            
        availablePickups.RemoveAt(selectedItemIndex);

        if (availablePickups.Count == 0) {
            selectedItemIndex = 0;
        } else {
            selectedItemIndex--;
        }
    }

    private void RemoveStatusEffect() {
        hasActiveStatusEffect = false;
        timeSincePickupUsed = 0f;
        // statusEffect = new PickUp();
        statusEffectImage.texture = null;
        statusEffectImage.color = new Color(1f, 1f, 1f, 0f);
        statusEffectRemainingDurationText.text = string.Empty;
        
        switch (statusEffect.type) {
            case PICK_UP_TYPES.REVERSE_GRAVITY:
                ReverseGravity();
                break;
            
            case PICK_UP_TYPES.BAD_IS_GOOD:
                isBadGood = !isBadGood;
                List<Collider2D> colliders = new List<Collider2D>();
                Physics2D.OverlapCollider(pcBC2D, colliders);
                foreach (var collider in colliders) {
                    Debug.Log(collider.gameObject.name);
                    if (!isBadGood && collider.gameObject.CompareTag("Spike")) {
                        KillPlayer();
                        Destroy(pc.gameObject);
                    }
                }
                break;
        }
    }

    private void ReverseGravity() {
        isGravityReversed = !isGravityReversed;
        // Physics.gravity = (direction ? Vector2.down : Vector2.up) * Physics.gravity.magnitude;
        // pcRb.gravityScale = pc.startingGravityScale * -1;
        pc.startingGravityScale *= -1;
        pc.fallingGravityScaleMultiplier *= -1;
    }
    
    private Texture2D ConvertSpriteToTexture2D(Sprite sprite) {
        var texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        
        var pixels = sprite.texture.GetPixels(
                (int)sprite.textureRect.x, 
                (int)sprite.textureRect.y, 
                (int)sprite.textureRect.width, 
                (int)sprite.textureRect.height
        );
        
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
}
