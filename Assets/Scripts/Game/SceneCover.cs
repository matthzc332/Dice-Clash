using UnityEngine;
using UnityEngine.UI;

public static class SceneCover
{
    private static GameObject cover;

    public static void Show()
    {
        if (cover != null) return;
        cover = new GameObject("SceneLoadCover");
        Object.DontDestroyOnLoad(cover);
        Canvas canvas = cover.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        cover.AddComponent<CanvasScaler>();
        RectTransform rt = cover.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        Image img = cover.AddComponent<Image>();
        img.color = Color.black;
        img.raycastTarget = true;
    }

    public static void Clear()
    {
        if (cover == null) return;
        Object.Destroy(cover);
        cover = null;
    }
}