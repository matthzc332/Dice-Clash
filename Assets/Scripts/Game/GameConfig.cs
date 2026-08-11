using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameMode { Campaign, Ranked }
public enum PowerupMode { WithPowerups, WithoutPowerups }

public static class GameConfig
{
    public static string selectedSpecies
    {
        get => PlayerPrefs.GetString("SelectedSpecies", "Human");
        set => PlayerPrefs.SetString("SelectedSpecies", value);
    }
    public static string selectedScenario
    {
        get => PlayerPrefs.GetString("SelectedScenario", "Human");
        set => PlayerPrefs.SetString("SelectedScenario", value);
    }

    public static bool isTutorial { get; set; }

    public static bool isAutoPlay { get; set; }
    public static int autoPlayMatches { get; set; } = 10;

    public static bool isCampaign
    {
        get => PlayerPrefs.GetInt("IsCampaign", 0) == 1;
        set => PlayerPrefs.SetInt("IsCampaign", value ? 1 : 0);
    }

    public static bool isRanked
    {
        get => PlayerPrefs.GetInt("IsRanked", 0) == 1;
        set => PlayerPrefs.SetInt("IsRanked", value ? 1 : 0);
    }

    public static int selectedLevel
    {
        get => PlayerPrefs.GetInt("SelectedLevel", 0);
        set => PlayerPrefs.SetInt("SelectedLevel", value);
    }

    public static GameMode currentGameMode { get; set; } = GameMode.Campaign;
    public static PowerupMode currentPowerupMode { get; set; } = PowerupMode.WithPowerups;

    public static void Play(string species)
    {
        selectedSpecies = species;
        selectedScenario = species;
        isTutorial = false;
        isCampaign = false;
        isRanked = false;
        isAutoPlay = false;
        currentGameMode = GameMode.Campaign;
        currentPowerupMode = PowerupMode.WithPowerups;
        PlayerPrefs.Save();
        SceneManager.LoadScene("SampleScene");
    }

    public static void PlayTutorial()
    {
        selectedSpecies = "Human";
        selectedScenario = "Human";
        isTutorial = true;
        isCampaign = false;
        isRanked = false;
        isAutoPlay = false;
        currentGameMode = GameMode.Campaign;
        currentPowerupMode = PowerupMode.WithPowerups;
        PlayerPrefs.Save();
        SceneManager.LoadScene("SampleScene");
    }

    public static void PlayAutoPlay(int matches)
    {
        selectedSpecies = "Human";
        selectedScenario = "Human";
        isTutorial = false;
        isCampaign = false;
        isRanked = false;
        isAutoPlay = true;
        autoPlayMatches = matches;
        currentGameMode = GameMode.Campaign;
        currentPowerupMode = PowerupMode.WithPowerups;
        PlayerPrefs.Save();
        SceneManager.LoadScene("SampleScene");
    }

    public static void PlayCampaign(int levelId, PowerupMode powerupMode = PowerupMode.WithPowerups)
    {
        var level = CampaignData.GetLevel(levelId);
        if (level == null) return;
        selectedLevel = levelId;
        selectedSpecies = "Human";
        selectedScenario = level.enemyRace;
        isTutorial = false;
        isCampaign = true;
        isRanked = false;
        isAutoPlay = false;
        currentGameMode = GameMode.Campaign;
        currentPowerupMode = powerupMode;
        PlayerPrefs.Save();
        SceneManager.LoadScene("SampleScene");
    }

    public static void PlayRanked(PowerupMode powerupMode = PowerupMode.WithPowerups)
    {
        string[] races = new[] { "Human", "Orc", "Wolf", "NewRace" };
        string enemyRace = races[Random.Range(0, races.Length)];

        selectedLevel = 0;
        selectedSpecies = "Human";
        selectedScenario = enemyRace;
        isTutorial = false;
        isCampaign = false;
        isRanked = true;
        isAutoPlay = false;
        currentGameMode = GameMode.Ranked;
        currentPowerupMode = powerupMode;
        PlayerPrefs.Save();
        SceneManager.LoadScene("SampleScene");
    }
}
