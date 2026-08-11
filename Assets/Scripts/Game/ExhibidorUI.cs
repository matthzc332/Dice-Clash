using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExhibidorUI : MonoBehaviour
{
    private GameObject panel;
    private Canvas canvas;
    private Font pressStart;
    private Sprite shelfSprite;
    public System.Action OnClose;

    public void Show(Canvas parentCanvas)
    {
        canvas = parentCanvas;
        pressStart = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        if (pressStart == null) pressStart = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        CreatePanel();
    }

    Sprite GetShelfSprite()
    {
        if (shelfSprite != null) return shelfSprite;
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Estantes/Estantes");
        if (sprites != null && sprites.Length > 0)
        {
            shelfSprite = System.Array.Find(sprites, s => s.name == "Estantes_0");
            if (shelfSprite == null) shelfSprite = sprites[0];
        }
        return shelfSprite;
    }

    void CreateShelf(Transform parent, Vector2 pos, float width)
    {
        GameObject shelfObj = new GameObject("Shelf");
        shelfObj.transform.SetParent(parent, false);

        RectTransform rt = shelfObj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(width, 22);

        Image img = shelfObj.AddComponent<Image>();
        img.raycastTarget = false;
        Sprite sprite = GetShelfSprite();
        if (sprite != null)
        {
            img.sprite = sprite;
            img.color = new Color(0.72f, 0.5f, 0.28f);
        }
        else
        {
            img.color = new Color(0.45f, 0.3f, 0.16f);
        }

        GameObject edgeObj = new GameObject("Edge");
        edgeObj.transform.SetParent(shelfObj.transform, false);
        Image edge = edgeObj.AddComponent<Image>();
        edge.color = new Color(1f, 1f, 1f, 0.15f);
        edge.raycastTarget = false;
        RectTransform edgeRt = edgeObj.GetComponent<RectTransform>();
        edgeRt.anchorMin = new Vector2(0f, 1f);
        edgeRt.anchorMax = new Vector2(1f, 1f);
        edgeRt.pivot = new Vector2(0.5f, 1f);
        edgeRt.sizeDelta = new Vector2(0, 3);
        edgeRt.anchoredPosition = Vector2.zero;
    }

    void CreatePanel()
    {
        panel = new GameObject("ExhibidorPanel");
        panel.transform.SetParent(canvas.transform, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.06f, 0.12f, 0.95f);

        CreateBackButton();
        CreateTitle();
        CreateCupsSection();
        CreateRibbonsSection();
        CreateTutorialSection();
        CreateInsigniasSection();
        CreateCounters();
    }

    void CreateBackButton()
    {
        GameObject btnObj = CreateButton("BACK", new Vector2(-480, 310), new Vector2(120, 40));
        btnObj.GetComponent<Button>().onClick.AddListener(Close);
    }

    public void Close()
    {
        if (panel != null) Destroy(panel);
        panel = null;
        OnClose?.Invoke();
    }

    void CreateTitle()
    {
        CreateText("TROPHIES", new Vector2(0, 310), 24, Color.white);
    }

    void CreateCupsSection()
    {
        GameObject cupsPanel = new GameObject("CupsSection");
        cupsPanel.transform.SetParent(panel.transform, false);

        RectTransform rt = cupsPanel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.1f);
        rt.anchorMax = new Vector2(0.4f, 0.9f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        CreateText("CUPS", new Vector2(0, 160), 14, Color.white, cupsPanel.transform);

        CampaignDataWrapper data = CampaignData.Load();
        if (data == null) return;

        float yOffset = 130f;
        for (int i = 0; i < data.cups.Length; i++)
        {
            CampaignCup cup = data.cups[i];
            bool completed = CampaignManager.Instance != null && CampaignManager.Instance.IsCupCompleted(cup.id);

            GameObject cupObj = CreateCupVisual(cup.name, GetCupSprite(i), completed, new Vector2(0, yOffset - i * 95), cupsPanel.transform);

            if (completed)
            {
                GameObject glow = new GameObject("Glow");
                glow.transform.SetParent(cupObj.transform, false);
                Image glowImg = glow.AddComponent<Image>();
                glowImg.color = new Color(1f, 0.84f, 0f, 0.2f);
                RectTransform glowRt = glow.GetComponent<RectTransform>();
                glowRt.sizeDelta = new Vector2(120, 130);
            }
        }
    }

    Sprite GetCupSprite(int cupIndex)
    {
        string[] paths = {
            "Sprites/Menu/copaHuman",
            "Sprites/Menu/copaOrc",
            "Sprites/Menu/copaBeast",
            "Sprites/Menu/copaMenu"
        };
        if (cupIndex < 0 || cupIndex >= paths.Length) return null;
        Sprite[] sprites = Resources.LoadAll<Sprite>(paths[cupIndex]);
        if (sprites == null || sprites.Length == 0) return null;
        foreach (var s in sprites)
        {
            if (s.name.EndsWith("_0")) return s;
        }
        return sprites[0];
    }

    void CreateRibbonsSection()
    {
        GameObject ribbonsPanel = new GameObject("RibbonsSection");
        ribbonsPanel.transform.SetParent(panel.transform, false);

        RectTransform rt = ribbonsPanel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.4f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.95f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        CreateText("RIBBONS", new Vector2(0, 60), 14, Color.white, ribbonsPanel.transform);

        CampaignDataWrapper data = CampaignData.Load();
        if (data == null) return;

        int col = 0;
        int row = 0;
        int maxCols = 5;

        for (int i = 0; i < data.levels.Length; i++)
        {
            CampaignLevel level = data.levels[i];

            if (col == 0)
            {
                float rowY = 20 - row * 50;
                CreateShelf(ribbonsPanel.transform, new Vector2(0, rowY - 27), 220);
            }

            bool has = RibbonManager.HasRibbon(level.id);
            Color color = has ? RibbonManager.GetRibbonColor(level.id) : new Color(0.3f, 0.3f, 0.3f, 0.5f);

            float x = -80 + col * 40;
            float y = 20 - row * 50;

            GameObject ribbonObj = new GameObject($"Ribbon_{level.id}");
            ribbonObj.transform.SetParent(ribbonsPanel.transform, false);

            RectTransform ribbonRt = ribbonObj.AddComponent<RectTransform>();
            ribbonRt.anchoredPosition = new Vector2(x, y);
            ribbonRt.sizeDelta = new Vector2(30, 45);

            Image ribbonImg = ribbonObj.AddComponent<Image>();
            ribbonImg.sprite = RibbonManager.GetRibbonSprite(level.id);
            ribbonImg.preserveAspect = true;
            ribbonImg.color = color;

            if (!has)
            {
                ribbonImg.color = new Color(color.r, color.g, color.b, 0.3f);
            }

            col++;
            if (col >= maxCols)
            {
                col = 0;
                row++;
            }
        }
    }

    void CreateTutorialSection()
    {
        GameObject tutPanel = new GameObject("TutorialSection");
        tutPanel.transform.SetParent(panel.transform, false);

        RectTransform rt = tutPanel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.35f, 0.55f);
        rt.anchorMax = new Vector2(0.65f, 0.85f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        CreateText("TUTORIAL", new Vector2(0, 55), 14, Color.white, tutPanel.transform);

        bool earned = TutorialCollectibles.HasEarned();
        CreateTutorialItem(tutPanel.transform, new Vector2(-150, -5), TutorialCollectibles.GetCupSprite(), new Color(1f, 0.84f, 0f), new Vector2(80, 80), "CUP", earned);
        CreateTutorialItem(tutPanel.transform, new Vector2(0, -5), TutorialCollectibles.GetRibbonSprite(), new Color(0.62f, 0.42f, 0.24f), new Vector2(130, 42), "RIBBON", earned);
        CreateTutorialItem(tutPanel.transform, new Vector2(150, -5), TutorialCollectibles.GetInsigniaSprite(), Color.white, new Vector2(70, 70), "BADGE", earned);
    }

    void CreateTutorialItem(Transform parent, Vector2 pos, Sprite sprite, Color tint, Vector2 size, string label, bool earned)
    {
        GameObject obj = new GameObject($"Tut_{label}");
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.color = earned ? tint : new Color(tint.r, tint.g, tint.b, 0.3f);

        CreateText(label, pos + new Vector2(0, -size.y * 0.5f - 16), 8, earned ? Color.white : new Color(0.6f, 0.6f, 0.6f), parent);
    }

    void CreateInsigniasSection()
    {
        GameObject insigniasPanel = new GameObject("InsigniasSection");
        insigniasPanel.transform.SetParent(panel.transform, false);

        RectTransform rt = insigniasPanel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.4f, 0.05f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        CreateText("INSIGNIAS", new Vector2(0, 60), 14, Color.white, insigniasPanel.transform);

        InsigniaDataWrapper insigniaData = InsigniaData.Load();
        if (insigniaData == null) return;

        int col = 0;
        int row = 0;
        int maxCols = 5;

        for (int i = 0; i < insigniaData.insignias.Length; i++)
        {
            Insignia badge = insigniaData.insignias[i];

            if (col == 0)
            {
                float rowY = 20 - row * 45;
                CreateShelf(insigniasPanel.transform, new Vector2(0, rowY - 24), 220);
            }

            bool has = InsigniaManager.HasInsignia(badge.id);
            Color badgeColor = has ? GetRarityColor(badge.rarity) : new Color(0.3f, 0.3f, 0.3f, 0.5f);

            float x = -80 + col * 40;
            float y = 20 - row * 45;

            GameObject badgeObj = new GameObject($"Badge_{badge.id}");
            badgeObj.transform.SetParent(insigniasPanel.transform, false);

            RectTransform badgeRt = badgeObj.AddComponent<RectTransform>();
            badgeRt.anchoredPosition = new Vector2(x, y);
            badgeRt.sizeDelta = new Vector2(35, 35);

            Image badgeImg = badgeObj.AddComponent<Image>();
            badgeImg.sprite = InsigniaSprites.Get(badge);
            badgeImg.preserveAspect = true;
            badgeImg.color = badgeColor;

            if (!has)
            {
                badgeImg.color = new Color(badgeColor.r, badgeColor.g, badgeColor.b, 0.3f);
            }

            if (has)
            {
                Button badgeBtn = badgeObj.AddComponent<Button>();
                badgeBtn.targetGraphic = badgeImg;
                Insignia captured = badge;
                badgeBtn.onClick.AddListener(() => InsigniaDetailPopup.Show(canvas, pressStart, captured, true));
            }

            col++;
            if (col >= maxCols)
            {
                col = 0;
                row++;
            }
        }
    }

    void CreateCounters()
    {
        int cupCount = CampaignManager.Instance != null ? CampaignManager.Instance.GetCompletedCupCount() : 0;
        int totalCups = CampaignData.GetTotalCups();
        int ribbonCount = RibbonManager.GetTotalRibbons();
        int totalRibbons = RibbonManager.GetTotalPossibleRibbons();
        int insigniaCount = InsigniaManager.GetCollectedCount();
        int totalInsignias = InsigniaManager.GetTotalCount();

        EconomyManager econ = EconomyManager.Instance;
        int gold = econ != null ? econ.TotalGold : 0;

        CreateText($"{cupCount}/{totalCups} CUPS", new Vector2(-240, -150), 10, Color.white);
        CreateText($"{ribbonCount}/{totalRibbons} RIBBONS", new Vector2(240, 60), 10, Color.white);
        CreateText($"{insigniaCount}/{totalInsignias} INSIGNIAS", new Vector2(240, -130), 10, Color.white);
        CreateText($"GOLD: {gold}", new Vector2(0, -200), 12, new Color(1f, 0.84f, 0f));
    }

    GameObject CreateCupVisual(string cupName, Sprite sprite, bool completed, Vector2 position, Transform parent)
    {
        GameObject cupObj = new GameObject($"Cup_{cupName}");
        cupObj.transform.SetParent(parent, false);

        RectTransform rt = cupObj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(90, 115);

        Image img = cupObj.AddComponent<Image>();
        if (sprite != null)
        {
            img.sprite = sprite;
            img.preserveAspect = true;
        }
        img.color = completed ? Color.white : new Color(0.35f, 0.35f, 0.35f);

        GameObject textObj = new GameObject("Name");
        textObj.transform.SetParent(cupObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.font = pressStart;
        text.text = cupName;
        text.fontSize = 7;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = completed ? new Color(1f, 0.9f, 0.5f) : Color.gray;
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0.5f, 0f);
        textRt.anchorMax = new Vector2(0.5f, 0f);
        textRt.pivot = new Vector2(0.5f, 1f);
        textRt.sizeDelta = new Vector2(160, 20);
        textRt.anchoredPosition = new Vector2(0, -14);

        return cupObj;
    }

    GameObject CreateText(string content, Vector2 position, int fontSize, Color color, Transform parent = null)
    {
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(parent != null ? parent : panel.transform, false);

        Text text = textObj.AddComponent<Text>();
        text.font = pressStart;
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;

        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);

        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(300, fontSize * 2);

        return textObj;
    }

    GameObject CreateButton(string label, Vector2 position, Vector2 size)
    {
        GameObject btnObj = new GameObject(label);
        btnObj.transform.SetParent(panel.transform, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.3f);

        btnObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(btnObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.font = pressStart;
        text.text = label;
        text.fontSize = 10;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        return btnObj;
    }

    Color GetRarityColor(string rarity)
    {
        switch (rarity?.ToLower())
        {
            case "common": return new Color(0.6f, 0.6f, 0.6f);
            case "rare": return new Color(0.2f, 0.5f, 1f);
            case "epic": return new Color(0.6f, 0.2f, 0.8f);
            case "legendary": return new Color(1f, 0.84f, 0f);
            default: return Color.white;
        }
    }
}