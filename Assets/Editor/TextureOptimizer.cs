using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TextureOptimizer
{
    const string MenuPath = "Optimize/Apply Texture Optimization";

    public static void ApplyCLI()
    {
        Apply();
    }

    public static void ForceReimportCLI()
    {
        ForceReimport();
    }

    public static void OptimizeAudioCLI()
    {
        string root = Path.Combine(Application.dataPath, "Resources");
        string[] mp3 = Directory.GetFiles(root, "*.mp3", SearchOption.AllDirectories);

        string[] heavyMusic =
        {
            "deuslower-fantasy-medieval-ambient-237371", "medieval_horizons", "win-lose",
            "campaignTrackVolumes", "After_the_Last_Round", "Hearthside_at_Twilight",
            "Three_Fingers_of_Ale", "orcs"
        };

        int updated = 0;
        foreach (string full in mp3)
        {
            string path = FullToAsset(full);
            if (path == null) continue;
            string name = Path.GetFileNameWithoutExtension(full);

            AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null) continue;

            float targetQuality = 0.35f;
            foreach (string h in heavyMusic)
                if (name == h) { targetQuality = 0.3f; break; }

            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = targetQuality;
            importer.defaultSampleSettings = settings;

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            updated++;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"AudioOptimizer done: {updated} clips set to Vorbis quality 0.3-0.5.");
    }

    public static void FixIconSheetsCLI()
    {
        string[] icons =
        {
            "Sprites/PowerUps/Icon/Explosion.PNG",
            "Sprites/PowerUps/Icon/Fireball.PNG",
            "Sprites/PowerUps/Icon/Lightning.PNG",
            "Sprites/PowerUps/Icon/Shake.PNG"
        };

        int updated = 0;
        foreach (string path in icons)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning("[FixIconSheets] no importer at " + path);
                continue;
            }
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePivot = new Vector2(0.5f, 0.5f);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            updated++;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[FixIconSheets] done: {updated} icons set to Single sprite mode.");
    }

    static void ForceReimport()
    {
        string root = Path.Combine(Application.dataPath, "Resources");
        string[] all = Directory.GetFiles(root, "*.png", SearchOption.AllDirectories);
        string[] allUc = Directory.GetFiles(root, "*.PNG", SearchOption.AllDirectories);
        string[] mp3 = Directory.GetFiles(root, "*.mp3", SearchOption.AllDirectories);
        List<string> files = new List<string>();
        files.AddRange(all);
        files.AddRange(allUc);
        files.AddRange(mp3);

        foreach (string full in files)
        {
            string path = FullToAsset(full);
            if (path == null) continue;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"ForceReimport done: {files.Count} assets reimported.");
    }

    [MenuItem(MenuPath)]
    public static void Apply()
    {
        string root = Path.Combine(Application.dataPath, "Resources");
        if (!Directory.Exists(root))
        {
            Debug.LogError("Resources folder not found: " + root);
            return;
        }

        string[] platforms = { "Standalone", "Web", "Android", "iPhone" };

        string[] all = Directory.GetFiles(root, "*.png", SearchOption.AllDirectories);

        int updated = 0;
        foreach (string full in all)
        {
            string path = FullToAsset(full);
            if (path == null) continue;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            int maxSize = PickMaxSize(path);
            bool needsFix = false;

            importer.crunchedCompression = true;
            importer.textureCompression = TextureImporterCompression.Compressed;
            if (importer.maxTextureSize != maxSize)
            {
                importer.maxTextureSize = maxSize;
                needsFix = true;
            }

            foreach (string platform in platforms)
            {
                TextureImporterPlatformSettings ps = importer.GetPlatformTextureSettings(platform);
                if (ps == null) continue;
                if (ps.maxTextureSize != maxSize
                    || ps.textureCompression != TextureImporterCompression.Compressed
                    || ps.crunchedCompression != true)
                {
                    ps.overridden = true;
                    ps.maxTextureSize = maxSize;
                    ps.textureCompression = TextureImporterCompression.Compressed;
                    ps.crunchedCompression = true;
                    importer.SetPlatformTextureSettings(ps);
                    needsFix = true;
                }
            }

            if (needsFix)
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                updated++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"TextureOptimizer done: {updated} textures updated of {all.Length} total.");
    }

    static int PickMaxSize(string assetPath)
    {
        if (assetPath.Contains("/Floor/")) return 256;
        if (assetPath.Contains("/Liston/")) return 256;
        if (assetPath.Contains("/FightCloud")) return 256;
        if (assetPath.Contains("/Pieces/Back/")) return 256;
        if (assetPath.Contains("/pasos")) return 256;
        if (assetPath.Contains("/muneco")) return 256;
        if (assetPath.Contains("/Insignias/")) return 256;
        if (assetPath.Contains("/Background/")) return 512;
        if (assetPath.Contains("/Win/")) return 512;
        if (assetPath.Contains("/Tutorial/")) return 512;
        if (assetPath.Contains("/Dice/")) return 512;
        if (assetPath.Contains("/Emoji/")) return 512;
        if (assetPath.Contains("/PowerUps/")) return 512;
        if (assetPath.Contains("/Menu/")) return 512;
        if (assetPath.Contains("/Pieces/")) return 512;
        if (assetPath.Contains("/Decor/")) return 512;
        if (assetPath.Contains("/Efect/")) return 512;
        if (assetPath.Contains("/Estantes/")) return 512;
        if (assetPath.Contains("/Card/")) return 512;
        return 256;
    }

    static string FullToAsset(string fullPath)
    {
        string full = fullPath.Replace('\\', '/');
        string data = Application.dataPath.Replace('\\', '/');
        if (!full.StartsWith(data)) return null;
        return "Assets" + full.Substring(data.Length);
    }
}
