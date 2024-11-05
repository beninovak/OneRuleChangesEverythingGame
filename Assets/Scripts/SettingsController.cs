using System;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour {
    [SerializeField] 
    private Slider      masterVolumeSlider;
    private GameObject  currentPanel, 
                        previousPanel;
    public GameObject   otherCanvas = null, // When on main menu, this is the MainMenuCanvas, otherwise HUDCanvas 
                        settingsCanvas, 
                        settingsButtonPanel, 
                        audioPanel, 
                        videoPanel;

    public TextMeshProUGUI levelNameText;

    public bool isMainMenu = false;
    private bool isGamePaused = false;
    
    private void Start() {
        AudioListener.volume = 0.5f;
        masterVolumeSlider.value = 0.5f;

        levelNameText.text = GameVariables.CURRENT_LEVEL_NAME;
    }

    public void RequestPauseUnpauseGame(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Started) {
            PauseUnpauseGame();
        }
    }

    private void PauseUnpauseGame () {
        isGamePaused = !isGamePaused;
        if (isGamePaused) {
            Time.timeScale = 0f;
            OpenCloseSettings(true);
        } else {
            Time.timeScale = 1f;
            OpenCloseSettings(false);
        }
    }
    
    public void OpenCloseSettings(bool open) {
        settingsCanvas.SetActive(open);
        // goBackButton.SetActive(open);
        if (open) {
            currentPanel = settingsCanvas;
            previousPanel = isMainMenu ? otherCanvas : null;  // TODO - asses if necessary
        }
        
        otherCanvas.SetActive(!open);
    }

    public void GoBack() {
        // Debug.Log($"From: {currentPanel} to {previousPanel}: {previousPanel == null}");
        currentPanel.SetActive(false);
        
        if (isMainMenu) {
            previousPanel.SetActive(true);
            if (previousPanel == otherCanvas) {
                settingsCanvas.SetActive(false);
            } else if (previousPanel == settingsCanvas) {
                previousPanel = otherCanvas;
                settingsButtonPanel.SetActive(true);
            }
        } else if (previousPanel == null || currentPanel == settingsCanvas) {
            PauseUnpauseGame();
        } else if (previousPanel != null) {
            previousPanel.SetActive(true);
            settingsButtonPanel.SetActive(true);
            currentPanel = previousPanel;
        }
    }

    public void OpenAudio() {
        audioPanel.SetActive(true);
        currentPanel = audioPanel;
        previousPanel = settingsCanvas;
        settingsButtonPanel.SetActive(false);
    }

    public void OpenVideo() {
        videoPanel.SetActive(true);
        currentPanel = videoPanel;
        previousPanel = settingsCanvas;
        settingsButtonPanel.SetActive(false);
    }

    // TODO - add to settings canvas
    public void BackToMainMenu() {

        // Only remember last scene if its not the last one ( End_Scene )
        if (SceneManager.GetActiveScene().buildIndex < SceneManager.sceneCountInBuildSettings - 1) {
            GameVariables.SCENE_INDEX_TO_RESUME_FROM = SceneManager.GetActiveScene().buildIndex;
        }
        
        // Debug.Log($"SAVING SCENE WITH INDEX {GameVariables.SCENE_INDEX_TO_RESUME_FROM}");
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
    
    public void AdjustVolume() {
        AudioListener.volume = masterVolumeSlider.value;
    }
}
