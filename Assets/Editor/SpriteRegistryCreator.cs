using UnityEditor;
using UnityEngine;

public static class SpriteRegistryCreator
{
    [MenuItem("DiceClash/Create SpriteRegistry Prefab")]
    public static void CreatePrefab()
    {
        GameObject go = new GameObject("SpriteRegistry");
        go.AddComponent<SpriteRegistry>();
        string prefabPath = "Assets/Prefabs/Systems/SpriteRegistry.prefab";
        string folder = System.IO.Path.GetDirectoryName(prefabPath);
        if (!System.IO.Directory.Exists(folder)) System.IO.Directory.CreateDirectory(folder);
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);
        Debug.Log("[SpriteRegistryCreator] SpriteRegistry.prefab creado");
    }
}