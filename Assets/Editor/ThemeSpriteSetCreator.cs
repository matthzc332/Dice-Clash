using UnityEditor;
using UnityEngine;

public static class ThemeSpriteSetCreator
{
    [MenuItem("DiceClash/Themes/Create All ThemeSpriteSets")]
    public static void CreateAll()
    {
        CreateSet("Human");
        CreateSet("Orc");
        CreateSet("Beastfolk");
        CreateSet("Nigromantes");
        CreateSet("Tutorial");
        AssetDatabase.SaveAssets();
        Debug.Log("[ThemeSpriteSetCreator] 5 ThemeSpriteSets creados en Assets/ScriptableObjects/Themes");
    }

    static void CreateSet(string name)
    {
        string path = $"Assets/ScriptableObjects/Themes/{name}.asset";
        if (AssetDatabase.LoadAssetAtPath<ThemeSpriteSet>(path) != null)
            return;
        ThemeSpriteSet so = ScriptableObject.CreateInstance<ThemeSpriteSet>();
        so.themeName = name;
        AssetDatabase.CreateAsset(so, path);
    }
}