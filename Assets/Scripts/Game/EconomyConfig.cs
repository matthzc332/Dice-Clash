using System;
using UnityEngine;

[System.Serializable]
public class StringIntPair
{
    public string key;
    public int value;
}

[System.Serializable]
public class DailyBonusConfig
{
    public int baseAmount;
    public int[] consecutiveDays;
    public int maxStreak;
}

[System.Serializable]
public class ChestConfig
{
    public int maxSlots;
    public int openTimeHours;
    public int insigniasPerChest;
}

[System.Serializable]
public class EconomyConfigWrapper
{
    public int startingGold;
    public DailyBonusConfig dailyBonus;
    public StringIntPair[] powerupCosts;
    public StringIntPair[] levelEntryCosts;
    public ChestConfig chestConfig;
    public int rerollCost;
}

public static class EconomyConfig
{
    private static EconomyConfigWrapper _data;

    public static EconomyConfigWrapper Load()
    {
        if (_data != null) return _data;

        var jsonAsset = Resources.Load<TextAsset>("Data/EconomyData");
        if (jsonAsset == null)
        {
            Debug.LogError("EconomyData.json not found in Resources/Data/");
            return null;
        }

        _data = JsonUtility.FromJson<EconomyConfigWrapper>(jsonAsset.text);
        return _data;
    }

    public static int GetPowerupCost(string powerupType)
    {
        var data = Load();
        if (data?.powerupCosts == null) return 0;

        foreach (var pair in data.powerupCosts)
        {
            if (pair.key == powerupType) return pair.value;
        }
        return 0;
    }

    public static int GetLevelEntryCost(int cupId)
    {
        var data = Load();
        if (data?.levelEntryCosts == null) return 0;

        string key = $"cup{cupId}";
        foreach (var pair in data.levelEntryCosts)
        {
            if (pair.key == key) return pair.value;
        }
        return 0;
    }

    public static int GetDailyBonusAmount(int streakDay)
    {
        var data = Load();
        if (data?.dailyBonus == null) return 25;

        int index = Mathf.Clamp(streakDay - 1, 0, data.dailyBonus.consecutiveDays.Length - 1);
        return data.dailyBonus.consecutiveDays[index];
    }
}