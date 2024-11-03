using UnityEngine;
using UnityEngine.SceneManagement;

public class GameVariables {
    public static float MASTER_VOLUME = 0.5f; // Maybe?
    public static bool USE_SOUND_EFFECTS = true; // Asses...
    
    
    public static string CURRENT_LEVEL_NAME;
    public static int SCENE_INDEX_TO_RESUME_FROM = 1;
    public static float[] SCENE_TIMES = new float[SceneManager.sceneCountInBuildSettings - 2]; // -2 because of main menu and end scene
}