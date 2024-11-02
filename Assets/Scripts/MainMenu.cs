using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    public void StartGame() {
        Debug.Log($"RESUMING FROM LEVEL: {GameVariables.SCENE_INDEX_TO_RESUME_FROM}");
        SceneManager.LoadScene(GameVariables.SCENE_INDEX_TO_RESUME_FROM);
    }
    
    public void QuitGame() {
        Application.Quit();
    }
}
