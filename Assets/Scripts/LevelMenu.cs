using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMenu : MonoBehaviour
{
    public GameObject settingsPanel;
    private static bool isGamePaused = false;

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            isGamePaused = !isGamePaused;
            PauseUnpauseGame();
        }
    }
    
    public static void PauseUnpauseGame () {
        Time.timeScale = isGamePaused ? 0f : 1f;
    }
    
    public void BackToMainMenu() {
        SceneManager.LoadScene("MainMenu");
    }
}
