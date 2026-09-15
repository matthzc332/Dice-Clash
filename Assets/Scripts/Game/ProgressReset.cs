using UnityEngine;
using UnityEngine.SceneManagement;

public static class ProgressReset
{
    public static void WipeAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        GameConfig.isTutorial = false;
        GameConfig.isAutoPlay = false;
        SceneManager.LoadScene("MainMenuScene");
    }
}
