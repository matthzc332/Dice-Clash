using System.Collections.Generic;
using UnityEngine;

public static class RibbonManager
{
    private static readonly Color[] HumanColors = new Color[]
    {
        new Color(0.4f, 0.6f, 1f),
        new Color(0.3f, 0.5f, 0.9f),
        new Color(0.2f, 0.4f, 0.8f),
        new Color(0.15f, 0.3f, 0.7f),
        new Color(0.1f, 0.2f, 0.6f)
    };

    private static readonly Color[] OrcColors = new Color[]
    {
        new Color(0.3f, 0.8f, 0.4f),
        new Color(0.25f, 0.7f, 0.35f),
        new Color(0.2f, 0.6f, 0.3f),
        new Color(0.15f, 0.5f, 0.25f),
        new Color(0.1f, 0.4f, 0.2f)
    };

    private static readonly Color[] BeastfolkColors = new Color[]
    {
        new Color(0.7f, 0.6f, 0.4f),
        new Color(0.6f, 0.5f, 0.35f),
        new Color(0.5f, 0.4f, 0.3f),
        new Color(0.4f, 0.35f, 0.25f),
        new Color(0.35f, 0.3f, 0.2f)
    };

    private static readonly Color[] NigromantesColors = new Color[]
    {
        new Color(0.7f, 0.4f, 0.9f),
        new Color(0.6f, 0.35f, 0.85f),
        new Color(0.5f, 0.3f, 0.8f),
        new Color(0.4f, 0.25f, 0.75f),
        new Color(0.3f, 0.2f, 0.7f)
    };

    public static void GrantRibbon(int levelId)
    {
        PlayerPrefs.SetInt($"Ribbon_Level_{levelId}", 1);
        PlayerPrefs.Save();
    }

    public static bool HasRibbon(int levelId)
    {
        return PlayerPrefs.GetInt($"Ribbon_Level_{levelId}", 0) == 1;
    }

    public static List<int> GetRibbonsByCup(int cupId)
    {
        List<int> ribbons = new List<int>();
        CampaignCup cup = CampaignData.GetCup(cupId);
        if (cup == null) return ribbons;

        foreach (int levelId in cup.levels)
        {
            if (HasRibbon(levelId))
                ribbons.Add(levelId);
        }
        return ribbons;
    }

    public static int GetTotalRibbons()
    {
        int total = 0;
        CampaignDataWrapper data = CampaignData.Load();
        if (data == null) return 0;

        foreach (CampaignLevel level in data.levels)
        {
            if (HasRibbon(level.id))
                total++;
        }
        return total;
    }

    public static int GetTotalPossibleRibbons()
    {
        CampaignDataWrapper data = CampaignData.Load();
        return data?.levels?.Length ?? 0;
    }

    public static Color GetRibbonColor(int levelId)
    {
        CampaignLevel level = CampaignData.GetLevel(levelId);
        if (level == null) return Color.white;

        int cupIndex = level.cup;
        int levelInCup = 0;

        CampaignCup cup = CampaignData.GetCup(level.cup);
        if (cup != null)
        {
            for (int i = 0; i < cup.levels.Length; i++)
            {
                if (cup.levels[i] == levelId)
                {
                    levelInCup = i;
                    break;
                }
            }
        }

        Color[] colors = GetColorsForCup(cupIndex);
        float t = cupIndex == 0 ? 0f : (float)levelInCup / Mathf.Max(1, colors.Length - 1);
        int idx = Mathf.Clamp(Mathf.RoundToInt(t * (colors.Length - 1)), 0, colors.Length - 1);
        return colors[idx];
    }

    private static Color[] GetColorsForCup(int cupIndex)
    {
        switch (cupIndex)
        {
            case 0: return HumanColors;
            case 1: return HumanColors;
            case 2: return OrcColors;
            case 3: return BeastfolkColors;
            case 4: return NigromantesColors;
            default: return HumanColors;
        }
    }

    public static string GetRibbonSpritePath(int levelId)
    {
        CampaignLevel level = CampaignData.GetLevel(levelId);
        if (level == null) return "Sprites/Liston/ListonHuman";
        switch (level.cup)
        {
            case 2: return "Sprites/Liston/ListonOrc";
            case 3: return "Sprites/Liston/ListonBeast";
            case 4: return "Sprites/Liston/ListonNigromante";
            default: return "Sprites/Liston/ListonHuman";
        }
    }

    public static Sprite GetRibbonSprite(int levelId)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(GetRibbonSpritePath(levelId));
        if (sprites != null && sprites.Length > 0)
            return sprites[0];
        return null;
    }

    public static Sprite CreateRibbonSprite(Color color)
    {
        Texture2D tex = new Texture2D(20, 32);
        Color transparent = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[20 * 32];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;
        tex.SetPixels(pixels);

        for (int y = 0; y < 24; y++)
        {
            for (int x = 4; x < 16; x++)
            {
                float edgeDist = Mathf.Min(x - 4, 15 - x);
                float alpha = Mathf.Clamp01(edgeDist / 2f);
                tex.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
            }
        }

        for (int x = 4; x < 10; x++)
        {
            tex.SetPixel(x, 24, new Color(color.r * 0.8f, color.g * 0.8f, color.b * 0.8f, 1f));
            tex.SetPixel(x + 6, 24, new Color(color.r * 0.8f, color.g * 0.8f, color.b * 0.8f, 1f));
            tex.SetPixel(x, 25, new Color(color.r * 0.7f, color.g * 0.7f, color.b * 0.7f, 1f));
            tex.SetPixel(x + 6, 25, new Color(color.r * 0.7f, color.g * 0.7f, color.b * 0.7f, 1f));
        }

        for (int x = 2; x < 18; x++)
        {
            tex.SetPixel(x, 28, new Color(color.r * 0.9f, color.g * 0.9f, color.b * 0.9f, 1f));
            tex.SetPixel(x, 29, new Color(color.r, color.g, color.b, 1f));
            tex.SetPixel(x, 30, new Color(color.r * 0.9f, color.g * 0.9f, color.b * 0.9f, 1f));
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 20, 32), new Vector2(0.5f, 1f), 20);
    }
}