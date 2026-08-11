using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    private const string COINS_KEY = "TotalCoins";
    private const string VICTORIES_KEY = "CupVictories";
    private const int VICTORIES_NEEDED = 2;
    private const int PTS_NORMAL = 100;
    private const int PTS_PALADIN = 130;
    private const int PTS_POWERUP = 110;

    private int totalCoins;
    private int cupVictories;
    private GameObject cupPanel;
    private Image cupImage;
    private Text coinText;
    private RectTransform cupPanelRt;
    private RectTransform cupImageRt;
    private Canvas particleCanvas;

    public bool IsCupFull => cupVictories >= VICTORIES_NEEDED;
    public int TotalCoins => totalCoins;
    public int CupMax => VICTORIES_NEEDED;
    public int CupVictories => cupVictories;
    public bool IsWorldUnlocked => cupVictories >= VICTORIES_NEEDED;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        totalCoins = PlayerPrefs.GetInt(COINS_KEY, 0);
        cupVictories = PlayerPrefs.GetInt(VICTORIES_KEY, 0);
        CreateParticleCanvas();
        CreateCupUI();
    }

    void CreateParticleCanvas()
    {
        GameObject go = new GameObject("CoinParticleCanvas");
        particleCanvas = go.AddComponent<Canvas>();
        particleCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        particleCanvas.sortingOrder = 99;

        CanvasScaler cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
    }

    public void AwardKill(Team killerTeam, Vector3 worldPos, PieceType type, bool isPowerUp)
    {
        if (killerTeam != Team.Blue) return;

        int points = isPowerUp ? PTS_POWERUP : (type == PieceType.Paladin ? PTS_PALADIN : PTS_NORMAL);
        totalCoins += points;
        PlayerPrefs.SetInt(COINS_KEY, totalCoins);
        PlayerPrefs.Save();

        StartCoroutine(CoinFlyToCup(worldPos, points));
    }

    public void RecordMatchWin()
    {
        if (cupVictories >= VICTORIES_NEEDED) return;
        cupVictories++;
        PlayerPrefs.SetInt(VICTORIES_KEY, cupVictories);
        PlayerPrefs.Save();
    }

    IEnumerator CoinFlyToCup(Vector3 worldPos, int amount)
    {
        Vector3 screenKill = Camera.main.WorldToScreenPoint(worldPos);
        Vector2 fromPos = new Vector2(
            screenKill.x - Screen.width / 2f,
            screenKill.y - Screen.height / 2f + 60f);

        Vector2 toPos = cupPanelRt != null ? cupPanelRt.anchoredPosition : new Vector2(791, 289);

        SoundManager.Instance.PlayCoin();
        GameObject txtObj = CreateCoinText(fromPos, amount);

        CameraShake(0.08f, 0.15f);

        yield return new WaitForSeconds(0.5f);
        if (txtObj != null) Destroy(txtObj);

        int numParticles = Mathf.Min(amount * 2, 20);
        for (int i = 0; i < numParticles; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 40f;
            GameObject p = CreatePixelParticle(fromPos + offset);
            StartCoroutine(AnimatePixel(p, toPos));
            yield return new WaitForSeconds(0.03f);
        }

        yield return new WaitForSeconds(0.6f);

        CameraShake(0.12f, 0.2f);
        StartCoroutine(CoinHitShake(toPos));

        if (cupImage != null)
        {
            float prev = cupImage.fillAmount;
            float target = Mathf.Clamp01((float)cupVictories / VICTORIES_NEEDED);
            float pt = 0;
            while (pt < 0.3f)
            {
                cupImage.fillAmount = Mathf.Lerp(prev, target, pt / 0.3f);
                pt += Time.deltaTime;
                yield return null;
            }
            cupImage.fillAmount = target;
            StartCoroutine(CupPulse());
        }

        if (coinText != null)
            coinText.text = $"W {cupVictories}/{VICTORIES_NEEDED}";
    }

    IEnumerator CoinHitShake(Vector2 cupPos)
    {
        Canvas canvas = cupPanel.GetComponent<Canvas>();
        if (canvas == null) yield break;
        RectTransform canvasRt = canvas.GetComponent<RectTransform>();
        Vector2 origPos = canvasRt.anchoredPosition;
        float t = 0;
        while (t < 0.2f)
        {
            canvasRt.anchoredPosition = origPos + Random.insideUnitCircle * 3f;
            t += Time.deltaTime;
            yield return null;
        }
        canvasRt.anchoredPosition = origPos;
    }

    IEnumerator CupPulse()
    {
        if (cupImageRt == null) yield break;
        Vector3 orig = cupImageRt.localScale;
        float t = 0;
        while (t < 0.3f)
        {
            float s = 1f + Mathf.Sin(t * 20.9f) * 0.12f;
            cupImageRt.localScale = s * orig;
            t += Time.deltaTime;
            yield return null;
        }
        cupImageRt.localScale = orig;
    }

    void CameraShake(float intensity, float duration)
    {
        StartCoroutine(DoCameraShake(intensity, duration));
    }

    IEnumerator DoCameraShake(float intensity, float duration)
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;
        Vector3 orig = cam.transform.position;
        float t = 0;
        while (t < duration)
        {
            cam.transform.position = orig + (Vector3)Random.insideUnitCircle * intensity;
            t += Time.deltaTime;
            yield return null;
        }
        cam.transform.position = orig;
    }

    GameObject CreateCoinText(Vector2 pos, int amount)
    {
        GameObject obj = new GameObject("CoinPopupText");
        obj.transform.SetParent(particleCanvas.transform, false);

        Text txt = obj.AddComponent<Text>();
        Font font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.font = font;
        txt.fontSize = 38;
        txt.fontStyle = FontStyle.Bold;
        txt.color = new Color(1f, 0.85f, 0f);
        txt.alignment = TextAnchor.MiddleCenter;
        txt.text = $"+{amount}";

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(180, 50);
        rt.anchoredPosition = pos;

        StartCoroutine(AnimateText(obj, txt));
        return obj;
    }

    IEnumerator AnimateText(GameObject obj, Text txt)
    {
        RectTransform rt = obj.GetComponent<RectTransform>();
        Vector3 origScale = rt.localScale;
        float t = 0;
        while (t < 0.6f)
        {
            if (obj == null) yield break;
            Color c = txt.color;
            c.a = 1f - t / 0.6f;
            txt.color = c;
            float s = 1f + Mathf.Sin(t * 15.7f) * 0.3f;
            rt.localScale = origScale * s;
            rt.anchoredPosition += new Vector2(0, 60f) * Time.deltaTime;
            t += Time.deltaTime;
            yield return null;
        }
        rt.localScale = origScale;
    }

    GameObject CreatePixelParticle(Vector2 pos)
    {
        GameObject obj = new GameObject("CoinPixel");
        obj.transform.SetParent(particleCanvas.transform, false);

        Image img = obj.AddComponent<Image>();
        img.color = new Color(1f, 0.85f + Random.value * 0.1f, 0f, 1f);

        RectTransform rt = obj.GetComponent<RectTransform>();
        float size = Random.Range(10f, 20f);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(size, size);
        rt.anchoredPosition = pos;

        return obj;
    }

    IEnumerator AnimatePixel(GameObject obj, Vector2 target)
    {
        if (obj == null) yield break;
        RectTransform rt = obj.GetComponent<RectTransform>();
        Vector2 start = rt.anchoredPosition;
        Image img = obj.GetComponent<Image>();
        float duration = Random.Range(0.5f, 0.8f);
        float t = 0;

        Vector2 mid = Vector2.Lerp(start, target, 0.5f) + Vector2.up * Random.Range(80f, 150f);

        while (t < duration)
        {
            if (obj == null) yield break;
            float p = t / duration;
            Vector2 a = Vector2.Lerp(start, mid, p);
            Vector2 b = Vector2.Lerp(mid, target, p);
            rt.anchoredPosition = Vector2.Lerp(a, b, p);

            if (img != null)
            {
                Color c = img.color;
                c.a = Mathf.Lerp(1f, 0f, Mathf.Pow(p, 2f));
                img.color = c;
                float sparkle = 0.8f + Mathf.Sin(t * 50f) * 0.2f;
                img.color = new Color(c.r * sparkle, c.g * sparkle, c.b * sparkle, c.a);
            }

            t += Time.deltaTime;
            yield return null;
        }
        if (obj != null) Destroy(obj);
    }

    void CreateCupUI()
    {
        string copaPath = GameConfig.isTutorial
            ? "Sprites/Menu/copaTuto"
            : "Sprites/Menu/copaHuman";
        Sprite cupSprite = Resources.Load<Sprite>(copaPath);
        if (cupSprite == null) cupSprite = Resources.Load<Sprite>("Sprites/Menu/copaMenu");

        cupPanel = new GameObject("CoinCupPanel");
        Canvas panelCanvas = cupPanel.AddComponent<Canvas>();
        panelCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        panelCanvas.sortingOrder = 50;

        CanvasScaler cs = cupPanel.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);

        GameObject container = new GameObject("CupContainer");
        container.transform.SetParent(cupPanel.transform, false);
        cupPanelRt = container.AddComponent<RectTransform>();
        cupPanelRt.anchorMin = new Vector2(0.5f, 0.5f);
        cupPanelRt.anchorMax = new Vector2(0.5f, 0.5f);
        cupPanelRt.pivot = new Vector2(0.5f, 0.5f);
        cupPanelRt.sizeDelta = new Vector2(145, 187);
        cupPanelRt.anchoredPosition = new Vector2(791, 289);

        GameObject cupBgObj = new GameObject("CupBg");
        cupBgObj.transform.SetParent(container.transform, false);
        Image cupBg = cupBgObj.AddComponent<Image>();
        cupBg.sprite = cupSprite;
        cupBg.preserveAspect = true;
        cupBg.color = new Color(0.25f, 0.25f, 0.25f, 0.4f);
        RectTransform cupBgRt = cupBgObj.GetComponent<RectTransform>();
        cupBgRt.anchorMin = new Vector2(0.5f, 0.5f);
        cupBgRt.anchorMax = new Vector2(0.5f, 0.5f);
        cupBgRt.pivot = new Vector2(0.5f, 0.5f);
        cupBgRt.sizeDelta = new Vector2(145, 187);
        cupBgRt.anchoredPosition = new Vector2(0, 0);

        GameObject cupObj = new GameObject("CopaImage");
        cupObj.transform.SetParent(container.transform, false);
        cupImage = cupObj.AddComponent<Image>();
        cupImage.sprite = cupSprite;
        cupImage.preserveAspect = true;
        cupImage.type = Image.Type.Filled;
        cupImage.fillMethod = Image.FillMethod.Vertical;
        cupImage.fillOrigin = (int)Image.OriginVertical.Bottom;
        cupImage.color = new Color(1f, 0.85f, 0f, 0.95f);
        cupImageRt = cupObj.GetComponent<RectTransform>();
        cupImageRt.anchorMin = new Vector2(0.5f, 0.5f);
        cupImageRt.anchorMax = new Vector2(0.5f, 0.5f);
        cupImageRt.pivot = new Vector2(0.5f, 0.5f);
        cupImageRt.sizeDelta = new Vector2(145, 187);
        cupImageRt.anchoredPosition = new Vector2(0, 0);

        GameObject textObj = new GameObject("CoinText");
        textObj.transform.SetParent(container.transform, false);
        coinText = textObj.AddComponent<Text>();
        Font font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        coinText.font = font;
        coinText.fontSize = 15;
        coinText.alignment = TextAnchor.MiddleCenter;
        coinText.color = Color.white;
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0.5f, 0.5f);
        textRt.anchorMax = new Vector2(0.5f, 0.5f);
        textRt.pivot = new Vector2(0.5f, 0.5f);
        textRt.sizeDelta = new Vector2(120, 36);
        textRt.anchoredPosition = new Vector2(-12, -5);

        float fill = Mathf.Clamp01((float)cupVictories / VICTORIES_NEEDED);
        cupImage.fillAmount = fill;
        coinText.text = $"W {cupVictories}/{VICTORIES_NEEDED}";
    }

    void OnDestroy()
    {
        PlayerPrefs.Save();
    }
}
