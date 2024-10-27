using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    public void StartGame() {
        SceneManager.LoadScene("Level_1");
    }
    
    public void QuitGame() {
        Application.Quit();
    }
}
