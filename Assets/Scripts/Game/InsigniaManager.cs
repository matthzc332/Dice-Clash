using System.Collections.Generic;
using UnityEngine;

public static class InsigniaManager
{
    private const string KEY_PREFIX = "Insignia_";
    private static HashSet<string> _collected;

    public static HashSet<string> Collected
    {
        get
        {
            if (_collected == null) LoadAll();
            return _collected;
        }
    }

    static void LoadAll()
    {
        _collected = new HashSet<string>();
        var data = InsigniaData.Load();
        if (data == null || data.insignias == null) return;

        foreach (var ins in data.insignias)
        {
            if (PlayerPrefs.GetInt(KEY_PREFIX + ins.id, 0) == 1)
                _collected.Add(ins.id);
        }
    }

    public static bool HasInsignia(string id)
    {
        return Collected.Contains(id);
    }

    public static void GrantInsignia(string id)
    {
        if (HasInsignia(id)) return;

        Collected.Add(id);
        PlayerPrefs.SetInt(KEY_PREFIX + id, 1);
        PlayerPrefs.Save();
        Debug.Log($"[Insignia] Granted: {id}");
    }

    public static int GetCollectedCount()
    {
        return Collected.Count;
    }

    public static int GetTotalCount()
    {
        return InsigniaData.GetTotalCount();
    }

    public static int GetCollectedBySource(string source)
    {
        int count = 0;
        var insignias = InsigniaData.GetBySource(source);
        if (insignias == null) return 0;

        foreach (var ins in insignias)
        {
            if (HasInsignia(ins.id)) count++;
        }
        return count;
    }

    public static int GetTotalBySource(string source)
    {
        var insignias = InsigniaData.GetBySource(source);
        return insignias?.Length ?? 0;
    }

    public static int GetCollectedByRarity(string rarity)
    {
        int count = 0;
        var insignias = InsigniaData.GetByRarity(rarity);
        if (insignias == null) return 0;

        foreach (var ins in insignias)
        {
            if (HasInsignia(ins.id)) count++;
        }
        return count;
    }

    public static void GrantCampaignInsignia(int levelId)
    {
        string id = $"camp_{levelId:D2}";
        var insignia = InsigniaData.GetInsignia(id);
        if (insignia != null)
        {
            GrantInsignia(id);
        }
    }

    public static void ResetAll()
    {
        var data = InsigniaData.Load();
        if (data != null && data.insignias != null)
        {
            foreach (var ins in data.insignias)
            {
                PlayerPrefs.DeleteKey(KEY_PREFIX + ins.id);
            }
        }
        PlayerPrefs.Save();
        _collected = null;
    }
}
