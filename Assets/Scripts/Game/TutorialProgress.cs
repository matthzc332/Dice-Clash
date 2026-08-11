using UnityEngine;

public static class TutorialProgress
{
    private const string KEY_PLAYED = "TutorialPlayed";

    public static void MarkPlayed()
    {
        PlayerPrefs.SetInt(KEY_PLAYED, 1);
        PlayerPrefs.Save();
    }

    public static bool HasPlayed()
    {
        return PlayerPrefs.GetInt(KEY_PLAYED, 0) == 1;
    }
}
