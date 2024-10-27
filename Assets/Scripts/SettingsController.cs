using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour {
    [SerializeField] private Slider masterVolumeSlider;
    private GameObject mainMenuPanel, settingsPanel, audioPanel, currentPanel, previousPanel;

    private void Start() {
        audioPanel = GameObject.FindGameObjectWithTag("AudioMenu");
        settingsPanel = GameObject.FindGameObjectWithTag("SettingsMenu");
        mainMenuPanel = GameObject.FindGameObjectWithTag("MainMenuPanel");
        
        AudioListener.volume = 0.5f;
        masterVolumeSlider.value = 0.5f;
    }

    public void AdjustVolume() {
        AudioListener.volume = masterVolumeSlider.value;
    }

    public void OpenSettings() {
        settingsPanel.SetActive(true);
        currentPanel = settingsPanel;
        
        if (SceneManager.GetActiveScene().buildIndex == 0) { // In main menu
            previousPanel = mainMenuPanel;
        } else {
            previousPanel = null;
        }
    }
    
    public void OpenAudio() {
        audioPanel.SetActive(true);
        currentPanel = audioPanel;
        previousPanel = settingsPanel;
    }
    
    public void GoBack() {
        currentPanel.SetActive(false);

        if (previousPanel != null) {
            previousPanel.SetActive(true);
        } else {
            LevelMenu.PauseUnpauseGame();
        }
    }
}
