using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    private const string GOLD_KEY = "TotalGold";
    private const string DAILY_KEY = "LastDailyBonus";
    private const string DAILY_STREAK_KEY = "DailyStreak";

    private int totalGold;
    private int dailyStreak;
    private long lastDailyTimestamp;
    private Text goldText;
    private RectTransform bagRt;
    private Canvas hudCanvas;

    public int TotalGold => totalGold;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        totalGold = PlayerPrefs.GetInt(GOLD_KEY, EconomyConfig.Load()?.startingGold ?? 100);
        dailyStreak = PlayerPrefs.GetInt(DAILY_STREAK_KEY, 0);
        lastDailyTimestamp = long.Parse(PlayerPrefs.GetString(DAILY_KEY, "0"));
        CreateGoldHUD();
    }

    public bool SpendGold(int amount)
    {
        if (totalGold < amount) return false;
        totalGold -= amount;
        SaveGold();
        UpdateGoldHUD();
        StartCoroutine(BagShake());
        return true;
    }

    public void AddGold(int amount)
    {
        totalGold += amount;
        SaveGold();
        UpdateGoldHUD();
        StartCoroutine(AddGoldFX(amount));
    }

    IEnumerator AddGoldFX(int amount)
    {
        StartCoroutine(BagPulse());
        yield return StartCoroutine(SpawnCoinsToBag(amount));
        StartCoroutine(BagShake());
    }

    void SaveGold()
    {
        PlayerPrefs.SetInt(GOLD_KEY, totalGold);
        PlayerPrefs.Save();
    }

    bool CanClaimDaily()
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long elapsed = now - lastDailyTimestamp;
        return elapsed >= 86400;
    }

    public int ClaimDailyBonus()
    {
        if (!CanClaimDaily()) return 0;

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long elapsed = now - lastDailyTimestamp;

        if (elapsed < 172800)
            dailyStreak = Mathf.Min(dailyStreak + 1, 7);
        else
            dailyStreak = 1;

        int amount = EconomyConfig.GetDailyBonusAmount(dailyStreak);
        AddGold(amount);

        lastDailyTimestamp = now;
        PlayerPrefs.SetInt(DAILY_STREAK_KEY, dailyStreak);
        PlayerPrefs.SetString(DAILY_KEY, lastDailyTimestamp.ToString());
        PlayerPrefs.Save();

        return amount;
    }

    void CreateGoldHUD()
    {
        GameObject canvasObj = new GameObject("EconomyCanvas");
        hudCanvas = canvasObj.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = 50;

        CanvasScaler cs = canvasObj.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);

        GameObject panelObj = new GameObject("GoldPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.08f, 0.06f, 0.03f, 0.85f);
        panelImg.raycastTarget = false;
        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(1, 1);
        panelRt.anchorMax = new Vector2(1, 1);
        panelRt.pivot = new Vector2(1, 1);
        panelRt.sizeDelta = new Vector2(190, 48);
        panelRt.anchoredPosition = new Vector2(-20, -20);

        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(panelObj.transform, false);
        Image borderImg = borderObj.AddComponent<Image>();
        borderImg.color = new Color(0.7f, 0.55f, 0.2f, 0.5f);
        borderImg.raycastTarget = false;
        RectTransform borderRt = borderObj.GetComponent<RectTransform>();
        borderRt.anchorMin = Vector2.zero;
        borderRt.anchorMax = Vector2.one;
        borderRt.sizeDelta = new Vector2(3, 3);

        GameObject bagObj = new GameObject("GoldBag");
        bagObj.transform.SetParent(panelObj.transform, false);
        Image bagImg = bagObj.AddComponent<Image>();
        Sprite bagSprite = Resources.Load<Sprite>("Sprites/Menu/bolsa");
        if (bagSprite != null)
        {
            bagImg.sprite = bagSprite;
            bagImg.preserveAspect = true;
        }
        else
        {
            bagImg.sprite = CreateCoinSprite();
            bagImg.preserveAspect = true;
        }
        bagImg.raycastTarget = false;
        bagRt = bagObj.GetComponent<RectTransform>();
        bagRt.anchorMin = new Vector2(0, 0.5f);
        bagRt.anchorMax = new Vector2(0, 0.5f);
        bagRt.pivot = new Vector2(0.5f, 0.5f);
        bagRt.sizeDelta = new Vector2(32, 32);
        bagRt.anchoredPosition = new Vector2(24, 0);

        GameObject textObj = new GameObject("GoldText");
        textObj.transform.SetParent(panelObj.transform, false);
        goldText = textObj.AddComponent<Text>();
        goldText.font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        if (goldText.font == null) goldText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        goldText.fontSize = 16;
        goldText.alignment = TextAnchor.MiddleLeft;
        goldText.color = new Color(1f, 0.85f, 0.2f);
        goldText.text = totalGold.ToString();
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0, 0.5f);
        textRt.anchorMax = new Vector2(0, 0.5f);
        textRt.pivot = new Vector2(0, 0.5f);
        textRt.sizeDelta = new Vector2(130, 28);
        textRt.anchoredPosition = new Vector2(44, 0);

        StartCoroutine(BagIdlePulse());
    }

    void UpdateGoldHUD()
    {
        if (goldText != null)
            goldText.text = totalGold.ToString();
    }

    IEnumerator BagIdlePulse()
    {
        while (true)
        {
            float t = 0;
            while (t < 2f)
            {
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.04f;
                if (bagRt != null) bagRt.localScale = Vector3.one * scale;
                t += Time.deltaTime;
                yield return null;
            }
        }
    }

    IEnumerator BagPulse()
    {
        if (bagRt == null) yield break;
        float t = 0;
        while (t < 0.3f)
        {
            float scale = 1f + Mathf.Sin(t / 0.3f * Mathf.PI) * 0.25f;
            bagRt.localScale = Vector3.one * scale;
            t += Time.deltaTime;
            yield return null;
        }
        bagRt.localScale = Vector3.one;
    }

    IEnumerator BagShake()
    {
        if (bagRt == null) yield break;
        Vector3 orig = bagRt.anchoredPosition;
        float t = 0;
        while (t < 0.25f)
        {
            float x = orig.x + Mathf.Sin(t * 60f) * 3f * (1f - t / 0.25f);
            bagRt.anchoredPosition = new Vector2(x, orig.y);
            t += Time.deltaTime;
            yield return null;
        }
        bagRt.anchoredPosition = orig;
    }

    IEnumerator SpawnCoinsToBag(int amount)
    {
        if (bagRt == null || hudCanvas == null) yield break;

        RectTransform canvasRt = hudCanvas.transform as RectTransform;
        Vector2 bagLocal;
        Vector2 bagScreen = RectTransformUtility.WorldToScreenPoint(null, bagRt.position);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, bagScreen, null, out bagLocal)) yield break;

        int count = Mathf.Clamp(amount / 5, 3, 10);
        for (int i = 0; i < count; i++)
        {
            Vector2 startPos = bagLocal + new Vector2(Random.Range(-90f, 90f), Random.Range(50f, 130f));

            GameObject coin = new GameObject("CoinFly");
            coin.transform.SetParent(hudCanvas.transform, false);
            Image coinImg = coin.AddComponent<Image>();
            coinImg.sprite = CreateMiniCoin();
            coinImg.preserveAspect = true;
            coinImg.raycastTarget = false;
            RectTransform coinRt = coin.GetComponent<RectTransform>();
            coinRt.anchorMin = Vector2.zero;
            coinRt.anchorMax = Vector2.zero;
            coinRt.pivot = new Vector2(0.5f, 0.5f);
            coinRt.sizeDelta = new Vector2(14, 14);
            coinRt.anchoredPosition = startPos;

            StartCoroutine(AnimateCoinToBag(coinRt, startPos, bagLocal));
            yield return new WaitForSeconds(0.04f);
        }
        yield return new WaitForSeconds(0.55f);
    }

    IEnumerator AnimateCoinToBag(RectTransform coinRt, Vector2 start, Vector2 target)
    {
        yield return new WaitForSeconds(Random.Range(0f, 0.12f));
        if (coinRt == null) yield break;

        float arcHeight = Random.Range(25f, 55f);
        float duration = Random.Range(0.4f, 0.55f);
        float t = 0;

        while (t < duration)
        {
            if (coinRt == null) yield break;
            float p = t / duration;
            float x = Mathf.Lerp(start.x, target.x, p);
            float y = Mathf.Lerp(start.y, target.y, p) + arcHeight * Mathf.Sin(p * Mathf.PI);
            coinRt.anchoredPosition = new Vector2(x, y);

            Image img = coinRt.GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                c.a = 1f - p * p;
                img.color = c;
            }

            t += Time.deltaTime;
            yield return null;
        }
        if (coinRt != null) Destroy(coinRt.gameObject);
    }

    Sprite CreateMiniCoin()
    {
        int size = 16;
        Texture2D tex = new Texture2D(size, size);
        Color gold = new Color(1f, 0.8f, 0.1f);
        Color dark = new Color(0.7f, 0.55f, 0.05f);
        float radius = size / 2f - 1;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(size / 2f, size / 2f));
                if (dist <= radius && dist > radius - 2)
                    tex.SetPixel(x, y, dark);
                else if (dist <= radius - 2)
                    tex.SetPixel(x, y, gold);
                else
                    tex.SetPixel(x, y, Color.clear);
            }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    Sprite CreateCoinSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        Color gold = new Color(1f, 0.8f, 0.1f);
        Color dark = new Color(0.7f, 0.55f, 0.05f);
        Color center = new Color(1f, 0.9f, 0.3f);
        float radius = size / 2f - 2;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(size / 2f, size / 2f));
                if (dist <= radius && dist > radius - 3)
                    tex.SetPixel(x, y, dark);
                else if (dist <= radius - 3 && dist > radius - 8)
                    tex.SetPixel(x, y, gold);
                else if (dist <= radius - 8)
                    tex.SetPixel(x, y, center);
                else
                    tex.SetPixel(x, y, Color.clear);
            }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}