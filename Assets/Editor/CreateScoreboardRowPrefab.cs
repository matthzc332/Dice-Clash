using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public static class CreateScoreboardRowPrefab
{
    [MenuItem("Tools/Create Scoreboard Row Prefab")]
    public static void Create()
    {
        GameObject root = new GameObject("ScoreboardRow");

        RectTransform rootRt = root.AddComponent<RectTransform>();
        rootRt.sizeDelta = new Vector2(380, 30);

        ScoreboardRow row = root.AddComponent<ScoreboardRow>();

        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(root.transform);
        Image iconImg = iconObj.AddComponent<Image>();
        iconImg.preserveAspect = true;
        RectTransform iconRt = iconObj.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0f, 0.5f);
        iconRt.anchorMax = new Vector2(0f, 0.5f);
        iconRt.pivot = new Vector2(0.5f, 0.5f);
        iconRt.sizeDelta = new Vector2(28, 28);
        iconRt.anchoredPosition = Vector2.zero;
        row.icon = iconImg;

        GameObject nameObj = new GameObject("NameLabel");
        nameObj.transform.SetParent(root.transform);
        Text nameText = nameObj.AddComponent<Text>();
        nameText.font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        nameText.fontSize = 14;
        nameText.alignment = TextAnchor.MiddleLeft;
        nameText.color = new Color(0.15f, 0.1f, 0.05f);
        nameText.text = "Peon";
        RectTransform nameRt = nameObj.GetComponent<RectTransform>();
        nameRt.anchorMin = new Vector2(0f, 0.5f);
        nameRt.anchorMax = new Vector2(0f, 0.5f);
        nameRt.pivot = new Vector2(0.5f, 0.5f);
        nameRt.sizeDelta = new Vector2(180, 25);
        nameRt.anchoredPosition = new Vector2(36, 0);
        row.nameLabel = nameText;

        GameObject scoreObj = new GameObject("ScoreLabel");
        scoreObj.transform.SetParent(root.transform);
        Text scoreText = scoreObj.AddComponent<Text>();
        scoreText.font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        scoreText.fontSize = 14;
        scoreText.alignment = TextAnchor.MiddleRight;
        scoreText.color = Color.black;
        scoreText.text = "+3";
        RectTransform scoreRt = scoreObj.GetComponent<RectTransform>();
        scoreRt.anchorMin = new Vector2(1f, 0.5f);
        scoreRt.anchorMax = new Vector2(1f, 0.5f);
        scoreRt.pivot = new Vector2(0.5f, 0.5f);
        scoreRt.sizeDelta = new Vector2(50, 25);
        scoreRt.anchoredPosition = new Vector2(-10, 0);
        row.scoreLabel = scoreText;

        string path = "Assets/Resources/Prefabs/ScoreboardRow.prefab";
        string dir = System.IO.Path.GetDirectoryName(path);
        if (!System.IO.Directory.Exists(dir))
            System.IO.Directory.CreateDirectory(dir);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);

        EditorUtility.DisplayDialog("Success", $"Prefab created at:\n{path}", "OK");
    }
}
