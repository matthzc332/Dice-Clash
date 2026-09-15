using UnityEngine;

public static class ExpoConfig
{
    public const int StartingGold = 500;

    private static bool? cachedEnabled;

    public static bool Enabled
    {
        get
        {
            if (cachedEnabled == null)
                cachedEnabled = Resources.Load<TextAsset>("ExpoBuild") != null;
            return cachedEnabled.Value;
        }
    }

    public static void ApplyBootState()
    {
        if (!Enabled) return;

        TutorialProgress.MarkPlayed();

        int gold = PlayerPrefs.GetInt("TotalGold", 0);
        if (gold < StartingGold)
            PlayerPrefs.SetInt("TotalGold", StartingGold);

        PlayerPrefs.SetInt("Campaign_Level_1", 1);
        PlayerPrefs.SetInt("Campaign_Level_2", 1);
        PlayerPrefs.Save();
    }
}