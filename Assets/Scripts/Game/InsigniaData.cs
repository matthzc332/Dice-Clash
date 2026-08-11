using System;
using UnityEngine;

[System.Serializable]
public class Insignia
{
    public string id;
    public string name;
    public string description;
    public string rarity;
    public string source;
    public int levelId;
}

[System.Serializable]
public class InsigniaDataWrapper
{
    public Insignia[] insignias;
}

public static class InsigniaData
{
    private static InsigniaDataWrapper _data;

    public static InsigniaDataWrapper Load()
    {
        if (_data != null) return _data;

        var jsonAsset = Resources.Load<TextAsset>("Data/InsigniaData");
        if (jsonAsset == null)
        {
            Debug.LogError("InsigniaData.json not found in Resources/Data/");
            return null;
        }

        _data = JsonUtility.FromJson<InsigniaDataWrapper>(jsonAsset.text);
        return _data;
    }

    public static Insignia GetInsignia(string insigniaId)
    {
        var data = Load();
        if (data == null) return null;

        foreach (var insignia in data.insignias)
        {
            if (insignia.id == insigniaId) return insignia;
        }
        return null;
    }

    public static Insignia[] GetBySource(string source)
    {
        var data = Load();
        if (data == null) return new Insignia[0];

        var list = new System.Collections.Generic.List<Insignia>();
        foreach (var insignia in data.insignias)
        {
            if (insignia.source == source) list.Add(insignia);
        }
        return list.ToArray();
    }

    public static Insignia[] GetByRarity(string rarity)
    {
        var data = Load();
        if (data == null) return new Insignia[0];

        var list = new System.Collections.Generic.List<Insignia>();
        foreach (var insignia in data.insignias)
        {
            if (insignia.rarity == rarity) list.Add(insignia);
        }
        return list.ToArray();
    }

    public static int GetTotalCount()
    {
        var data = Load();
        return data?.insignias?.Length ?? 0;
    }
}