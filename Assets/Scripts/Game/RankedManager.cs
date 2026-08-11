using UnityEngine;

public static class RankedManager
{
    private const string KEY_RANKED_UNLOCKED = "RankedUnlocked";

    public static bool IsUnlocked()
    {
        return PlayerPrefs.GetInt(KEY_RANKED_UNLOCKED, 0) == 1;
    }

    public static void Unlock()
    {
        PlayerPrefs.SetInt(KEY_RANKED_UNLOCKED, 1);
        PlayerPrefs.Save();
        Debug.Log("[Ranked] Mode unlocked");
    }

    public static string GetRandomEnemyRace()
    {
        string[] races = new[] { "Human", "Orc", "Wolf", "NewRace" };
        return races[Random.Range(0, races.Length)];
    }

    public static string[] GetPowerupsForRace(string enemyRace)
    {
        switch (enemyRace)
        {
            case "Human":
                return new[] { "Shake", "Explosion" };
            case "Orc":
                return new[] { "Fireball", "Lightning" };
            case "Wolf":
                return new[] { "Lightning", "MAGIC" };
            case "NewRace":
                return new[] { "Fireball", "MAGIC" };
            default:
                return new[] { "Shake", "Explosion" };
        }
    }

    public static string GetRandomObstacle()
    {
        string[] obstacles = new[] { "Roca", "DestroyedCell", "Glue", "Mine" };
        return obstacles[Random.Range(0, obstacles.Length)];
    }

    public static int GetRandomEnemyCount()
    {
        return Random.Range(4, 8);
    }

    public static int GetGoldReward()
    {
        return Random.Range(50, 101);
    }
}