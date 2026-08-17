using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DailyBonusUI : MonoBehaviour
{
    public static DailyBonusUI Instance { get; private set; }

    private GameObject popupObj;
    private Canvas ownCanvas;
    private Text amountText;
    private Text streakText;

    void Awake()
    {
        Instance = this;
    }

    public void ShowIfAvailable()
    {
        if (EconomyManager.Instance == null) return;

        int amount = EconomyManager.Instance.ClaimDailyBonus();
        if (amount > 0)
            ShowPopup(amount);
    }

    void ShowPopup(int goldAmount)
    {
        if (popupObj != null) return;

        GameObject canvasObj = new GameObject("DailyBonusCanvas", typeof(RectTransform));
        ownCanvas = canvasObj.AddComponent<Canvas>();
        ownCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        ownCanvas.sortingOrder = 220;

        CanvasScaler cs = canvasObj.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        popupObj = new GameObject("DailyBonusPopup");
        popupObj.transform.SetParent(canvasObj.transform);

        Image overlay = popupObj.AddComponent<Image>();
        overlay.color = new Color(0, 0, 0, 0.7f);
        RectTransform overlayRt = popupObj.GetComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero;
        overlayRt.anchorMax = Vector2.one;
        overlayRt.offsetMin = new Vector2(2, -23);
        overlayRt.offsetMax = new Vector2(-2, 23);

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(popupObj.transform);
        Image panelBg = panel.AddComponent<Image>();
        Sprite panelSprite = LoadFirstSprite("Sprites/Menu/panelCartaBlue", "panelCartaBlue");
        if (panelSprite != null)
        {
            panelBg.sprite = panelSprite;
            panelBg.color = Color.white;
        }
        else
        {
            panelBg.color = new Color(0.15f, 0.1f, 0.05f, 0.95f);
        }
        RectTransform panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(500, 320);
        panelRt.anchoredPosition = new Vector2(0, 170);

        Font font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform);
        Text title = titleObj.AddComponent<Text>();
        title.font = font;
        title.fontSize = 24;
        title.alignment = TextAnchor.MiddleCenter;
        title.color = new Color(1f, 0.85f, 0.2f);
        title.text = "DAILY BONUS";
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 1);
        titleRt.anchorMax = new Vector2(0.5f, 1);
        titleRt.pivot = new Vector2(0.5f, 1);
        titleRt.sizeDelta = new Vector2(400, 50);
        titleRt.anchoredPosition = new Vector2(0, -20);

        GameObject iconObj = new GameObject("CoinIcon");
        iconObj.transform.SetParent(panel.transform);
        Image icon = iconObj.AddComponent<Image>();
        icon.sprite = CreateCoinSprite();
        icon.preserveAspect = true;
        RectTransform iconRt = iconObj.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.5f, 0.5f);
        iconRt.anchorMax = new Vector2(0.5f, 0.5f);
        iconRt.sizeDelta = new Vector2(64, 64);
        iconRt.anchoredPosition = new Vector2(0, 30);

        GameObject amountObj = new GameObject("Amount");
        amountObj.transform.SetParent(panel.transform);
        amountText = amountObj.AddComponent<Text>();
        amountText.font = font;
        amountText.fontSize = 32;
        amountText.alignment = TextAnchor.MiddleCenter;
        amountText.color = new Color(1f, 0.85f, 0.2f);
        amountText.text = $"+{goldAmount} GOLD";
        RectTransform amountRt = amountObj.GetComponent<RectTransform>();
        amountRt.anchorMin = new Vector2(0.5f, 0.5f);
        amountRt.anchorMax = new Vector2(0.5f, 0.5f);
        amountRt.sizeDelta = new Vector2(400, 50);
        amountRt.anchoredPosition = new Vector2(0, -30);

        GameObject streakObj = new GameObject("Streak");
        streakObj.transform.SetParent(panel.transform);
        streakText = streakObj.AddComponent<Text>();
        streakText.font = font;
        streakText.fontSize = 14;
        streakText.alignment = TextAnchor.MiddleCenter;
        streakText.color = new Color(0.7f, 0.7f, 0.7f);
        int streak = PlayerPrefs.GetInt("DailyStreak", 1);
        streakText.text = $"Day {streak} streak";
        RectTransform streakRt = streakObj.GetComponent<RectTransform>();
        streakRt.anchorMin = new Vector2(0.5f, 0.5f);
        streakRt.anchorMax = new Vector2(0.5f, 0.5f);
        streakRt.sizeDelta = new Vector2(400, 30);
        streakRt.anchoredPosition = new Vector2(0, -70);

        GameObject btnObj = new GameObject("OKButton");
        btnObj.transform.SetParent(panel.transform);
        Image btnBg = btnObj.AddComponent<Image>();
        Sprite openSprite = LoadFirstSprite("Sprites/Menu/botin ui/botonOpen", "botonOpen_0");
        if (openSprite != null)
        {
            btnBg.sprite = openSprite;
            btnBg.preserveAspect = true;
            btnBg.color = Color.white;
        }
        else
        {
            btnBg.color = new Color(0.2f, 0.5f, 0.2f);
        }
        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(Close);
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0);
        btnRt.anchorMax = new Vector2(0.5f, 0);
        btnRt.pivot = new Vector2(0.5f, 0);
        btnRt.sizeDelta = new Vector2(170, 60);
        btnRt.anchoredPosition = new Vector2(0, 20);
    }

    public void Close()
    {
        if (popupObj != null) Destroy(popupObj);
        if (ownCanvas != null) Destroy(ownCanvas.gameObject);
        popupObj = null;
        ownCanvas = null;
    }

    Sprite LoadFirstSprite(string path, string name)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);
        if (sprites == null || sprites.Length == 0) return null;
        foreach (var s in sprites)
        {
            if (s.name == name) return s;
        }
        return sprites[0];
    }

    Sprite CreateCoinSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        Color gold = new Color(1f, 0.8f, 0.1f);
        Color dark = new Color(0.7f, 0.55f, 0.05f);
        Color center = new Color(1f, 0.9f, 0.3f);
        float radius = size / 2f - 4;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(size / 2f, size / 2f));
                if (dist <= radius && dist > radius - 4)
                    tex.SetPixel(x, y, dark);
                else if (dist <= radius - 4 && dist > radius - 12)
                    tex.SetPixel(x, y, gold);
                else if (dist <= radius - 12)
                    tex.SetPixel(x, y, center);
                else
                    tex.SetPixel(x, y, Color.clear);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}