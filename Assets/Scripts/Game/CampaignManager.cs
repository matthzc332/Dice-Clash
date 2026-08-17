using System.Collections.Generic;
using UnityEngine;

public class CampaignManager : MonoBehaviour
{
    public static CampaignManager Instance { get; private set; }

    const string KEY_PREFIX = "Campaign_Level_";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool IsLevelCompleted(int levelId)
    {
        return PlayerPrefs.GetInt(KEY_PREFIX + levelId, 0) == 1;
    }

    public bool IsLevelUnlocked(int levelId)
    {
        if (levelId <= 1) return true;

        var level = CampaignData.GetLevel(levelId);
        if (level == null) return false;

        var allLevels = CampaignData.Load().levels;
        int prevId = -1;
        for (int i = 0; i < allLevels.Length; i++)
        {
            if (allLevels[i].id == levelId)
            {
                if (i > 0) prevId = allLevels[i - 1].id;
                break;
            }
        }

        if (prevId == -1) return true;
        return IsLevelCompleted(prevId);
    }

    public string CompleteLevel(int levelId)
    {
        if (IsLevelCompleted(levelId)) return null;

        PlayerPrefs.SetInt(KEY_PREFIX + levelId, 1);
        PlayerPrefs.Save();

        var level = CampaignData.GetLevel(levelId);
        if (level != null && level.goldReward > 0 && EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddGold(level.goldReward);
        }

        InsigniaManager.GrantCampaignInsignia(levelId);
        RibbonManager.GrantRibbon(levelId);
        ChestManager.TryGrantChestAfterLevel(levelId);

        string cupRace = CheckCupCompletion(levelId);

        Debug.Log($"CampaignManager: Level {levelId} completed");
        return cupRace;
    }

    public int GetNextUncompletedLevel()
    {
        var data = CampaignData.Load();
        if (data == null) return 1;

        foreach (var level in data.levels)
        {
            if (!IsLevelCompleted(level.id) && IsLevelUnlocked(level.id))
                return level.id;
        }
        return -1;
    }

    public int GetCompletedCount()
    {
        var data = CampaignData.Load();
        if (data == null) return 0;

        int count = 0;
        foreach (var level in data.levels)
        {
            if (IsLevelCompleted(level.id)) count++;
        }
        return count;
    }

    public void ResetProgress()
    {
        var data = CampaignData.Load();
        if (data == null) return;

        foreach (var level in data.levels)
        {
            PlayerPrefs.DeleteKey(KEY_PREFIX + level.id);
        }
        PlayerPrefs.Save();
    }

    string CheckCupCompletion(int levelId)
    {
        CampaignLevel level = CampaignData.GetLevel(levelId);
        if (level == null) return null;

        CampaignCup cup = CampaignData.GetCup(level.cup);
        if (cup == null) return null;

        bool allCompleted = true;
        foreach (int lid in cup.levels)
        {
            if (!IsLevelCompleted(lid))
            {
                allCompleted = false;
                break;
            }
        }

        if (allCompleted)
        {
            string raceKey = $"Unlocked_{cup.race}";
            if (PlayerPrefs.GetInt(raceKey, 0) == 0)
            {
                PlayerPrefs.SetInt(raceKey, 1);
                PlayerPrefs.Save();
                Debug.Log($"CampaignManager: Unlocked {cup.race} from cup {cup.name}");
                return cup.race;
            }
        }
        return null;
    }

    public bool IsCupCompleted(int cupId)
    {
        CampaignCup cup = CampaignData.GetCup(cupId);
        if (cup == null) return false;

        foreach (int lid in cup.levels)
        {
            if (!IsLevelCompleted(lid))
                return false;
        }
        return true;
    }

    public int GetCompletedCupCount()
    {
        int count = 0;
        CampaignDataWrapper data = CampaignData.Load();
        if (data == null) return 0;

        foreach (CampaignCup cup in data.cups)
        {
            if (IsCupCompleted(cup.id))
                count++;
        }
        return count;
    }
}
