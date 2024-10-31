using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour {
    [SerializeField] private Slider masterVolumeSlider;
    private GameObject currentPanel, previousPanel;
    public GameObject mainMenuCanvas = null, settingsCanvas, settingsButtonPanel, audioPanel, videoPanel, goBackButton;
    private Canvas settingsCanvasCanvas;

    public bool isMainMenu = false;

    private bool isGamePaused = false;
    
    private void Start() {
        AudioListener.volume = 0.5f;
        masterVolumeSlider.value = 0.5f;
        settingsCanvasCanvas = settingsCanvas.GetComponent<Canvas>();
    }

    void Update() {
        // Debug.Log("UPDATING");
        if (Input.GetKeyDown(KeyCode.Escape) && !isMainMenu) {
            Debug.Log("PRESSING ESCAPE");
            isGamePaused = !isGamePaused;
            PauseUnpauseGame();
        }
    }
    
    private void PauseUnpauseGame () { // TODO - check why multiple pause/unpauses don't work
        if (isGamePaused) {
            Debug.Log("PAUSING GAME");
            Time.timeScale = 0f;
            OpenCloseSettings(true);
        } else {
            Debug.Log("UNPAUSING GAME");
            Time.timeScale = 1f;
            OpenCloseSettings(false);
        }
    }
    
    public void OpenCloseSettings(bool open) {
        if (open) {
            settingsCanvas.SetActive(true);
            settingsCanvasCanvas.enabled = true;
            goBackButton.SetActive(true);
            currentPanel = settingsCanvas;
            
            if (isMainMenu) {
                previousPanel = mainMenuCanvas; // TODO - asses if necessary
                mainMenuCanvas.SetActive(false);
            } else {
                previousPanel = null;
            }
        } else {
            settingsCanvas.SetActive(false);
            goBackButton.SetActive(false);
            
            if (isMainMenu) {
                mainMenuCanvas.SetActive(false);
            }
        }
    }

    public void GoBack() {
        Debug.Log("Current: " + currentPanel);
        Debug.Log("Previous: " + previousPanel);
        currentPanel.SetActive(false);
        
        if (isMainMenu) {
            previousPanel.SetActive(true);
            if (previousPanel == mainMenuCanvas) {
                settingsCanvas.SetActive(false);
            } else if (previousPanel == settingsCanvas) {
                previousPanel = mainMenuCanvas;
                settingsButtonPanel.SetActive(true);
            }
        } else if (previousPanel == null || currentPanel == settingsCanvas) {
            isGamePaused = false;
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

    public void BackToMainMenu() {
        SceneManager.LoadScene("MainMenu");
    }
    
    public void AdjustVolume() {
        AudioListener.volume = masterVolumeSlider.value;
    }
}
