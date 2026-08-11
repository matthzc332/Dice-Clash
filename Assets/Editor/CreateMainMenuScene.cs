using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CreateMainMenuScene
{
    [MenuItem("Tools/Create MainMenu Scene")]
    public static void Create()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject obj = new GameObject("MainMenuManager");
        obj.AddComponent<MainMenuManager>();
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenuScene.unity");
        Debug.Log("MainMenuScene created at Assets/Scenes/MainMenuScene.unity");
    }
}
