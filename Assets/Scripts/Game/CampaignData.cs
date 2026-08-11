using System;
using UnityEngine;

[System.Serializable]
public class CampaignLevel
{
    public int id;
    public int cup;
    public string name;
    public string enemyRace;
    public string[] powerups;
    public string[] obstacles;
    public int enemyCount;
    public int goldReward;
    public string insigniaId;
}

[System.Serializable]
public class CampaignCup
{
    public int id;
    public string name;
    public string race;
    public int[] levels;
}

[System.Serializable]
public class CampaignDataWrapper
{
    public CampaignLevel[] levels;
    public CampaignCup[] cups;
}

public static class CampaignData
{
    private static CampaignDataWrapper _data;

    public static CampaignDataWrapper Load()
    {
        if (_data != null) return _data;

        var jsonAsset = Resources.Load<TextAsset>("Data/CampaignData");
        if (jsonAsset == null)
        {
            Debug.LogError("CampaignData.json not found in Resources/Data/");
            return null;
        }

        _data = JsonUtility.FromJson<CampaignDataWrapper>(jsonAsset.text);
        return _data;
    }

    public static CampaignLevel GetLevel(int levelId)
    {
        var data = Load();
        if (data == null) return null;

        foreach (var level in data.levels)
        {
            if (level.id == levelId) return level;
        }
        return null;
    }

    public static CampaignCup GetCup(int cupId)
    {
        var data = Load();
        if (data == null) return null;

        foreach (var cup in data.cups)
        {
            if (cup.id == cupId) return cup;
        }
        return null;
    }

    public static int GetTotalLevels()
    {
        var data = Load();
        return data?.levels?.Length ?? 0;
    }

    public static int GetTotalCups()
    {
        var data = Load();
        return data?.cups?.Length ?? 0;
    }
}