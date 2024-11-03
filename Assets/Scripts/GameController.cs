using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PickUpController;

public class GameController : MonoBehaviour {

    public string          levelName;
    
    public GameObject      HUDCanvas;
    public Image           levelNameBackgroundImage;

    [HideInInspector]
    public PickUp          statusEffect;
    public RawImage        statusEffectImage;
    public TextMeshProUGUI statusEffectText;
    public TextMeshProUGUI statusEffectRemainingDurationText;
    public TextMeshProUGUI levelNameText;

    private bool           shouldFadeLevelNameText = true; 
    private bool           hasActiveStatusEffect = false;
    private float          timeSincePickup = 0f;
    private float          pickupNotificationFadeDuration = 2f;
    private float          levelStartTimestamp;

    private Color          HUDTextColorShown = new Color(1f, 1f, 1f, 1f);
    private Color          HUDTextColorHidden = new Color(1f, 1f, 1f, 0f);
    private Color          levelNameBackgroundColorShown;
    private Color          levelNameBackgroundColorHidden;
    
    private void Awake() {
        levelStartTimestamp = Time.time;
        // GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().gc = this;
        
        //  TODO - check if pickups and finishLines can be merged into a single array??
        
        GameObject[] pickups = GameObject.FindGameObjectsWithTag("Pickup");
        foreach (GameObject pickup in pickups) {
            pickup.GetComponent<PickUpController>().gc = this;
        }
        
        GameObject[] finishLines = GameObject.FindGameObjectsWithTag("FinishLine");
        foreach (GameObject finishLine in finishLines) {
            finishLine.GetComponent<FinishLineController>().gc = this;
        }

        levelNameBackgroundColorShown = levelNameBackgroundImage.color;
        levelNameBackgroundColorHidden = new Color(levelNameBackgroundColorShown.r, levelNameBackgroundColorShown.g, levelNameBackgroundColorShown.b, 0f);
        levelNameText.text = levelName;
        GameVariables.CURRENT_LEVEL_NAME = levelName;
    }

    private void Update() {

        if (shouldFadeLevelNameText) {
            HUDFadeInOut("levelName");
        }
        
        if (hasActiveStatusEffect) {
            HUDFadeInOut("statusEffect");
        }
    }

    public void FinishLevel() {
        Debug.Log("FINISH GAME");
    }

    private void HUDFadeInOut(string name) {

        switch (name) {
            
            case "levelName":
                if (Time.time - 2f >= levelStartTimestamp) {
                    shouldFadeLevelNameText = false;
                    levelNameText.color = new Color(1f, 1f, 1f, 0);
                    levelNameBackgroundImage.color = new Color(1f, 1f, 1f, 0);
                } else {
                    levelNameText.color = Color.Lerp(levelNameText.color, HUDTextColorHidden, pickupNotificationFadeDuration * Time.deltaTime);
                    levelNameBackgroundImage.color = Color.Lerp(levelNameBackgroundImage.color, levelNameBackgroundColorHidden, pickupNotificationFadeDuration * Time.deltaTime);
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
        }
    }
    
    public void ApplyStatusEffect(PickUp effect) {
        HUDCanvas.SetActive(true);
        statusEffect = effect;
        hasActiveStatusEffect = true;
        statusEffectImage.texture = ConvertSpriteToTexture2D(effect.sprite);
        statusEffectImage.color = new Color(1f, 1f, 1f, 1f);
        statusEffectText.text = effect.title;
    }

    private void RemoveStatusEffect() {
        hasActiveStatusEffect = false;
        timeSincePickup = 0f;
        // statusEffect = new PickUp();
        statusEffectImage.texture = null;
        statusEffectImage.color = new Color(1f, 1f, 1f, 0f);
        statusEffectRemainingDurationText.text = string.Empty;
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
