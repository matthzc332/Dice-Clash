using UnityEngine;

public static class TutorialCollectibles
{
    private const string KEY_EARNED = "TutorialCollectiblesEarned";

    public static void Grant()
    {
        PlayerPrefs.SetInt(KEY_EARNED, 1);
        PlayerPrefs.Save();
    }

    public static bool HasEarned()
    {
        return PlayerPrefs.GetInt(KEY_EARNED, 0) == 1;
    }

    public static Sprite GetCupSprite()
    {
        return LoadFirst("Tutorial/copaTuto");
    }

    public static Sprite GetInsigniaSprite()
    {
        return LoadFirst("Tutorial/InsigniaTutorial");
    }

    public static Sprite GetRibbonSprite()
    {
        return LoadFirst("Tutorial/ListonTutorial");
    }

    private static Sprite LoadFirst(string path)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);
        if (sprites != null && sprites.Length > 0)
            return sprites[0];
        return null;
    }
}
