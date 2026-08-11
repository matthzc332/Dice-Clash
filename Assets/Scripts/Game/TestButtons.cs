using UnityEngine;
using UnityEngine.UI;

public class TestButtons : MonoBehaviour
{
    private GameObject levelPanel;
    private bool panelOpen;

    void Start()
    {
        GameObject canvasObj = new GameObject("TestCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        Font font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");

        CreateButton(canvasObj.transform, font, "WIN", new Vector2(-840, -415), Color.white, () => ScoreboardUI.Instance.Show(Team.Blue));
        CreateButton(canvasObj.transform, font, "LOSE", new Vector2(-673, -424), Color.white, () => ScoreboardUI.Instance.Show(Team.Red));
        CreateButton(canvasObj.transform, font, "SCORE", new Vector2(-840, -361), Color.white, () => ScoreboardUI.Instance.Show());
        CreateButton(canvasObj.transform, font, "LVL 9", new Vector2(-673, -361), new Color(1f, 0.6f, 0.2f), () => GameConfig.PlayCampaign(9));

        CreateLevelMenu(canvasObj.transform, font);
    }

    void CreateLevelMenu(Transform parent, Font font)
    {
        GameObject burgerBtn = new GameObject("BurgerBtn", typeof(RectTransform));
        burgerBtn.transform.SetParent(parent, false);
        Image burgerImg = burgerBtn.AddComponent<Image>();
        burgerImg.color = new Color(0.6f, 0.3f, 0.1f, 0.9f);
        RectTransform burgerRt = burgerBtn.GetComponent<RectTransform>();
        burgerRt.anchorMin = new Vector2(0.5f, 0.5f);
        burgerRt.anchorMax = new Vector2(0.5f, 0.5f);
        burgerRt.pivot = new Vector2(0.5f, 0.5f);
        burgerRt.sizeDelta = new Vector2(120, 40);
        burgerRt.anchoredPosition = new Vector2(-673, -310);

        GameObject burgerText = new GameObject("Label", typeof(RectTransform));
        burgerText.transform.SetParent(burgerBtn.transform, false);
        Text bt = burgerText.AddComponent<Text>();
        bt.font = font;
        bt.fontSize = 10;
        bt.alignment = TextAnchor.MiddleCenter;
        bt.text = "LVLS";
        bt.color = Color.yellow;
        RectTransform btRt = burgerText.GetComponent<RectTransform>();
        btRt.anchorMin = Vector2.zero;
        btRt.anchorMax = Vector2.one;
        btRt.sizeDelta = Vector2.zero;

        Button burgerButton = burgerBtn.AddComponent<Button>();
        burgerButton.targetGraphic = burgerImg;
        burgerButton.onClick.AddListener(() => ToggleLevelPanel(parent, font));

        levelPanel = new GameObject("LevelPanel", typeof(RectTransform));
        levelPanel.transform.SetParent(parent, false);
        Image panelImg = levelPanel.AddComponent<Image>();
        panelImg.color = new Color(0.1f, 0.1f, 0.18f, 0.92f);
        RectTransform panelRt = levelPanel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0, 0);
        panelRt.anchorMax = new Vector2(0, 0);
        panelRt.pivot = new Vector2(0, 0);
        panelRt.sizeDelta = new Vector2(260, 580);
        panelRt.anchoredPosition = new Vector2(10, 50);
        levelPanel.SetActive(false);

        string[] cupNames = { "Intro", "Copa 1 Human", "Copa 2 Orc", "Copa 3 Beast", "Copa 4 Nigro" };
        CampaignDataWrapper data = CampaignData.Load();
        if (data == null) return;

        float y = -10;
        int lastCup = -1;

        foreach (var level in data.levels)
        {
            if (level.cup != lastCup)
            {
                lastCup = level.cup;
                GameObject cupLabel = new GameObject($"Cup_{level.cup}", typeof(RectTransform));
                cupLabel.transform.SetParent(levelPanel.transform, false);
                Text cupText = cupLabel.AddComponent<Text>();
                cupText.font = font;
                cupText.fontSize = 8;
                cupText.alignment = TextAnchor.MiddleLeft;
                cupText.text = cupNames.Length > level.cup ? cupNames[level.cup] : $"Copa {level.cup}";
                cupText.color = new Color(1f, 0.85f, 0.3f);
                RectTransform cupRt = cupLabel.GetComponent<RectTransform>();
                cupRt.anchorMin = new Vector2(0, 1);
                cupRt.anchorMax = new Vector2(1, 1);
                cupRt.pivot = new Vector2(0, 1);
                cupRt.anchoredPosition = new Vector2(8, y);
                cupRt.sizeDelta = new Vector2(0, 16);
                y -= 18;
            }

            int lvlId = level.id;
            string lvlName = level.name;
            string obstacles = level.obstacles != null && level.obstacles.Length > 0 ? string.Join("+", level.obstacles) : "none";

            GameObject row = new GameObject($"Level_{lvlId}", typeof(RectTransform));
            row.transform.SetParent(levelPanel.transform, false);
            Image rowImg = row.AddComponent<Image>();
            rowImg.color = new Color(0.2f, 0.2f, 0.35f, 0.6f);
            RectTransform rowRt = row.GetComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0, 1);
            rowRt.anchorMax = new Vector2(1, 1);
            rowRt.pivot = new Vector2(0, 1);
            rowRt.anchoredPosition = new Vector2(5, y);
            rowRt.sizeDelta = new Vector2(-10, 20);

            GameObject labelText = new GameObject("Label", typeof(RectTransform));
            labelText.transform.SetParent(row.transform, false);
            Text lt = labelText.AddComponent<Text>();
            lt.font = font;
            lt.fontSize = 7;
            lt.alignment = TextAnchor.MiddleLeft;
            lt.text = $"{lvlId}. {lvlName}";
            lt.color = Color.white;
            RectTransform ltRt = labelText.GetComponent<RectTransform>();
            ltRt.anchorMin = new Vector2(0, 0);
            ltRt.anchorMax = new Vector2(0.65f, 1);
            ltRt.sizeDelta = Vector2.zero;
            ltRt.anchoredPosition = new Vector2(6, 0);

            GameObject obsText = new GameObject("Obs", typeof(RectTransform));
            obsText.transform.SetParent(row.transform, false);
            Text ot = obsText.AddComponent<Text>();
            ot.font = font;
            ot.fontSize = 6;
            ot.alignment = TextAnchor.MiddleRight;
            ot.text = obstacles;
            ot.color = obstacles == "none" ? new Color(0.5f, 0.5f, 0.5f) : new Color(1f, 0.6f, 0.3f);
            RectTransform otRt = obsText.GetComponent<RectTransform>();
            otRt.anchorMin = new Vector2(0.65f, 0);
            otRt.anchorMax = new Vector2(1, 1);
            otRt.sizeDelta = Vector2.zero;
            otRt.anchoredPosition = new Vector2(-6, 0);

            Button rowBtn = row.AddComponent<Button>();
            rowBtn.targetGraphic = rowImg;
            int captureId = lvlId;
            rowBtn.onClick.AddListener(() =>
            {
                GameConfig.PlayCampaign(captureId);
            });

            y -= 22;
        }
    }

    void ToggleLevelPanel(Transform parent, Font font)
    {
        panelOpen = !panelOpen;
        levelPanel.SetActive(panelOpen);
    }

    void CreateButton(Transform parent, Font font, string label, Vector2 pos, Color color, System.Action onClick)
    {
        float x = pos.x;
        float y = pos.y;

        GameObject btnObj = new GameObject($"Btn_{label}", typeof(RectTransform));
        btnObj.transform.SetParent(parent, false);
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.3f, 0.3f, 0.5f, 0.8f);
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0.5f);
        btnRt.anchorMax = new Vector2(0.5f, 0.5f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.sizeDelta = new Vector2(120, 40);
        btnRt.anchoredPosition = new Vector2(x, y);

        GameObject textObj = new GameObject("Label", typeof(RectTransform));
        textObj.transform.SetParent(btnObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.font = font;
        text.fontSize = 14;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = label;
        text.color = color;
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImage;
        btn.onClick.AddListener(() => onClick());
    }
}
