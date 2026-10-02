using UnityEditor;
using UnityEngine;

public static class ThemeSpriteSetFiller
{
    [MenuItem("DiceClash/Themes/Auto-Fill All Themes")]
    public static void FillAll()
    {
        Fill("Human"); Fill("Orc"); Fill("Beastfolk"); Fill("Nigromantes"); Fill("Tutorial");
        AssetDatabase.SaveAssets();
        Debug.Log("[ThemeSpriteSetFiller] Auto-fill completado");
    }
    static void Fill(string theme)
    {
        string path = $"Assets/ScriptableObjects/Themes/{theme}.asset";
        ThemeSpriteSet ts = AssetDatabase.LoadAssetAtPath<ThemeSpriteSet>(path);
        if (ts == null) return;
        string folder = MapFolder(theme);
        string resPieces = $"Sprites/{folder}/Pieces";
        ts.peonIdleFront = LoadAll($"{resPieces}/PeonIdleFront");
        ts.knightIdleFront = LoadAll($"{resPieces}/CaballeroIdleFront");
        ts.ninjaIdleFront = LoadAll($"{resPieces}/NinjaIdleFront");
        ts.paladinIdleFront = LoadAll($"{resPieces}/PaladinIdleFront");
        ts.peonIdleBack = LoadAll($"{resPieces}/PeonIdleBack");
        ts.knightIdleBack = LoadAll($"{resPieces}/CaballeroIdleBack");
        ts.ninjaIdleBack = LoadAll($"{resPieces}/NinjaIdleBack");
        ts.paladinIdleBack = LoadAll($"{resPieces}/PaladinIdleBack");
        ts.peonMoveFront = LoadAll($"{resPieces}/PeonMoveFront");
        ts.knightMoveFront = LoadAll($"{resPieces}/CaballeroMoveFront");
        ts.ninjaMoveFront = LoadAll($"{resPieces}/NinjaMoveFront");
        ts.paladinMoveFront = LoadAll($"{resPieces}/PaladinMoveFront");
        ts.peonMoveBack = LoadAll($"{resPieces}/PeonMoveBack");
        ts.knightMoveBack = LoadAll($"{resPieces}/CaballeroMoveBack");
        ts.ninjaMoveBack = LoadAll($"{resPieces}/NinjaMoveBack");
        ts.paladinMoveBack = LoadAll($"{resPieces}/PaladinMoveBack");
        ts.peonAttackFront = LoadAll($"{resPieces}/PeonAttackFront");
        ts.knightAttackFront = LoadAll($"{resPieces}/CaballeroAttackFront");
        ts.paladinAttackFront = LoadAll($"{resPieces}/PaladinAttackFront");
        ts.peonAttackBack = LoadAll($"{resPieces}/PeonAttackBack");
        ts.knightAttackBack = LoadAll($"{resPieces}/CaballeroAttackBack");
        ts.paladinAttackBack = LoadAll($"{resPieces}/PaladinAttackBack");
        if (folder == "Nigromantes")
        {
            ts.peonNecroMoveFront = LoadSingle($"{resPieces}/PeonNecroMoveFront");
            ts.knightNecroMoveFront = LoadSingle($"{resPieces}/caballerofrontnigro");
            ts.paladinNecroMoveFront2 = LoadSingle($"{resPieces}/PaladinMoveFrontNigro2");
        }
        string resBg = $"Sprites/{folder}/Background";
        string resDecor = $"Sprites/{folder}/Decor";
        ts.backgroundMain = LoadSingle($"{resBg}/{MapBgMain(folder,theme)}");
        ts.backgroundMid = LoadSingle($"{resBg}/fondo2");
        ts.backgroundFondo3 = LoadSingle($"{resBg}/fondo3");
        ts.backgroundFondoTuto = LoadSingle("Sprites/Tutorial/fondoTuto");
        ts.skipTurn = LoadSingle($"{resDecor}/SkipTurn");
        ts.quitButton = LoadSingle($"{resDecor}/QuitButton");
        EditorUtility.SetDirty(ts);
    }
    static string MapFolder(string t){ switch(t){ case "Orc": return "Orc"; case "Beastfolk": return "Beastfolk"; case "Nigromantes": return "Nigromantes"; case "Tutorial": return "Tutorial"; default: return "Human"; } }
    static string MapBgMain(string f,string t){ if(f=="Orc") return "BackgroundOrco"; if(f=="Beastfolk") return "BeastFolk"; if(f=="Nigromantes") return "Nigromantes"; if(f=="Human") return "Human"; if(f=="Tutorial") return "fondoTuto"; return t; }
    static Sprite[] LoadAll(string p){ if(string.IsNullOrEmpty(p)) return new Sprite[0]; var a=Resources.LoadAll<Sprite>(p); return a??new Sprite[0]; }
    static Sprite LoadSingle(string p){ if(string.IsNullOrEmpty(p)) return null; return Resources.Load<Sprite>(p); }
}