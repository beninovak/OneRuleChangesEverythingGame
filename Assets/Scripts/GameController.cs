using System;
using System.Collections.Generic;
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
    private float          timeSincePickup = 0f;
    private float          levelNameFadeDuration = 2f;
    private float          pickupNotificationFadeDuration = 2f;
    private float          timeAfterFinishBeforeSceneSwitch = 3f;

    private Color          HUDTextColorShown = new Color(1f, 1f, 1f, 1f);
    private Color          HUDTextColorHidden = new Color(1f, 1f, 1f, 0f);
    private Color          bigMessageBackgroundColorShown;
    private Color          bigMessageBackgroundColorHidden;

    private GameObject player;
    private PlayerController pc;
    private Rigidbody2D pcRb;

    /* Status effects */
    public bool isGravityReversed = false; 
    public bool isBadGood = false; 
    
    private void Awake() {
        // Debug.Log($"Previous best time: {GameVariables.SCENE_TIMES[SceneManager.GetActiveScene().buildIndex - 1]}");
        levelStartTimestamp = Time.time;
        player = GameObject.FindGameObjectWithTag("Player");
        pc = player.GetComponent<PlayerController>();
        pc.gc = this;
        pcRb = pc.GetComponent<Rigidbody2D>();
        
        //  TODO - check if pickups and finishLines can be merged into a single array??
        GameObject[] pickups = GameObject.FindGameObjectsWithTag("Pickup");
        foreach (GameObject pickup in pickups) {
            pickup.GetComponent<PickUpController>().gc = this;
        }
        
        GameObject[] finishLines = GameObject.FindGameObjectsWithTag("FinishLine");
        foreach (GameObject finishLine in finishLines) {
            finishLine.GetComponent<FinishLineController>().gc = this;
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
                timeSincePickup += Time.deltaTime;
                statusEffectRemainingDurationText.text = $"{statusEffect.duration - timeSincePickup:0.0}s"; // Wacky formatting ( string interpolation )
        
                if (timeSincePickup >= statusEffect.duration) {
                    RemoveStatusEffect();
                    return;
                }
        
                if (timeSincePickup <= pickupNotificationFadeDuration) {
                    statusEffectText.color = Color.Lerp(statusEffectText.color, HUDTextColorShown, pickupNotificationFadeDuration * Time.deltaTime);
                    return;
                } 
        
                // TODO - could probably optimize this if else
                if (timeSincePickup <= 2 * pickupNotificationFadeDuration) {
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
    
    public void ApplyStatusEffect(PickUp effect) {
        HUDCanvas.SetActive(true);
        statusEffect = effect;
        hasActiveStatusEffect = true;
        statusEffectImage.texture = ConvertSpriteToTexture2D(effect.sprite);
        statusEffectImage.color = new Color(1f, 1f, 1f, 1f);
        statusEffectText.text = effect.title;

        switch (effect.type) {
            case PICK_UP_TYPES.REVERSE_GRAVITY:
                ReverseGravity();
                break;
            
            case PICK_UP_TYPES.BAD_IS_GOOD:
                isBadGood = true;
                break;
        }
    }

    private void RemoveStatusEffect() {
        hasActiveStatusEffect = false;
        timeSincePickup = 0f;
        // statusEffect = new PickUp();
        statusEffectImage.texture = null;
        statusEffectImage.color = new Color(1f, 1f, 1f, 0f);
        statusEffectRemainingDurationText.text = string.Empty;
        
        switch (statusEffect.type) {
            case PICK_UP_TYPES.REVERSE_GRAVITY:
                ReverseGravity();
                break;
            
            case PICK_UP_TYPES.BAD_IS_GOOD:
                // TODO - Check if touching spike here...kill player if so
                isBadGood = false;
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
