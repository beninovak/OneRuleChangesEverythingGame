using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PickUpController;

public class GameController : MonoBehaviour {

    public GameObject      HUDCanvas;

    [HideInInspector]
    public PickUp          statusEffect;
    public RawImage        statusEffectImage;
    public TextMeshProUGUI statusEffectText;
    public TextMeshProUGUI statusEffectRemainingDurationText;

    private bool           hasActiveStatusEffect = false;
    private float          timeSincePickup = 0f;
    private float          pickupNotificationFadeDuration = 2f;

    private Color          HUDTextColorShown = new Color(1f, 1f, 1f, 1f);
    private Color          HUDTextColorHidden = new Color(1f, 1f, 1f, 0f);
    
    private void Awake() {
        // GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().gc = this;
        GameObject[] pickups = GameObject.FindGameObjectsWithTag("Pickup");
        foreach (GameObject pickup in pickups) {
            pickup.GetComponent<PickUpController>().gc = this;
        }
    }

    private void Update() {
        if (hasActiveStatusEffect) {
            UIFadeInOut();
        }
    }

    private void UIFadeInOut() {
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
        // statusEffect = new PickUpController.PickUp() {
        //         type = PickUpController.PICK_UP_TYPES.NONE,
        //         title = "",
        //         sprite = null,
        //         duration = 0f,
        // };
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
