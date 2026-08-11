using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardUI : MonoBehaviour
{
    private static ScoreboardUI _instance;
    public static ScoreboardUI Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("ScoreboardUI");
                _instance = obj.AddComponent<ScoreboardUI>();
            }
            return _instance;
        }
    }

    private GameObject canvasObj;
    private GameObject finBatallaPanel, finConteoBlue, finConteoRed;
    private GameObject bannerObj;
    private Text blueTotalText, redTotalText, titleText;
    private Sprite circleSprite;
    private List<GameObject> pieceIcons = new();
    private bool isShowing;
    private Coroutine scoreRoutine;
    private Team? pendingWinner;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        CreateSprites();
        CreateUI();
        gameObject.SetActive(false);
    }

    void CreateSprites()
    {
        Texture2D circleTex = new Texture2D(16, 16);
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
            {
                float dx = x - 7.5f;
                float dy = y - 7.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                circleTex.SetPixel(x, y, dist <= 7.5f ? Color.white : Color.clear);
            }
        circleTex.Apply();
        circleSprite = Sprite.Create(circleTex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 100);
    }

    Sprite LoadFirstSprite(string path)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);
        return sprites.Length > 0 ? sprites[0] : null;
    }

    void CreateUI()
    {
        canvasObj = new GameObject("ScoreboardCanvas");
        canvasObj.transform.SetParent(transform);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 199;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Dark overlay
        Image bgOverlay = canvasObj.AddComponent<Image>();
        bgOverlay.color = new Color(0f, 0f, 0f, 0.7f);
        RectTransform bgRt = bgOverlay.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // FinBatallaPanel — top title banner
        finBatallaPanel = MakeImagePanel(canvasObj.transform, "FinBatallaPanel",
            "Sprites/Menu/Score/FinBatallaPanel", new Vector2(0, 400), new Vector2(900, 155));

        titleText = MakeLabel(finBatallaPanel.transform, "TitleText", "BATTLE OVER", 33, new Color(0.15f, 0.1f, 0.05f),
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(700, 50));

        // FinConteoBluePanel — blue score panel
        finConteoBlue = MakeImagePanel(canvasObj.transform, "FinConteoBluePanel",
            "Sprites/Menu/Score/FinConteoBluePanel", new Vector2(-320, 190), new Vector2(400, 160));

        blueTotalText = MakeLabel(finConteoBlue.transform, "BlueTotal", "", 26, new Color(1f, 1f, 1f),
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -45), new Vector2(300, 40));

        // FinConteoRedPanel — red score panel
        finConteoRed = MakeImagePanel(canvasObj.transform, "FinConteoRedPanel",
            "Sprites/Menu/Score/FinConteoRedPanel", new Vector2(320, 190), new Vector2(390, 160));

        redTotalText = MakeLabel(finConteoRed.transform, "RedTotal", "", 26, new Color(1f, 1f, 1f),
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -45), new Vector2(300, 40));
    }

    GameObject MakeImagePanel(Transform parent, string name, string spritePath, Vector2 pos, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);

        Image img = obj.AddComponent<Image>();
        Sprite s = LoadFirstSprite(spritePath);
        if (s != null)
        {
            img.sprite = s;
            img.preserveAspect = true;
        }
        else
        {
            img.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        }

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;

        return obj;
    }

    void ClearIcons()
    {
        foreach (GameObject icon in pieceIcons)
        {
            if (icon != null) Destroy(icon);
        }
        pieceIcons.Clear();
    }

    public void Show(Team? forcedWinner = null)
    {
        if (isShowing) return;
        isShowing = true;
        pendingWinner = forcedWinner;
        gameObject.SetActive(true);
        ClearIcons();

        if (TimerManager.Instance != null)
            TimerManager.Instance.Stop();

        GameObject turnCanvas = GameObject.Find("TurnCanvas");
        GameObject diceCanvas = GameObject.Find("DiceCanvas");
        GameObject statsCanvas = GameObject.Find("StatsCanvas");
        if (turnCanvas != null) turnCanvas.SetActive(false);
        if (diceCanvas != null) diceCanvas.SetActive(false);
        if (statsCanvas != null) statsCanvas.SetActive(false);

        BoardManager board = FindFirstObjectByType<BoardManager>();
        if (board == null) return;

        List<PieceData> bluePieces = new();
        List<PieceData> redPieces = new();

        for (int r = 0; r < board.rows; r++)
            for (int c = 0; c < board.cols; c++)
            {
                Cell cell = board.GetCell(r, c);
                if (cell != null && cell.IsOccupied && cell.pieceData.HasValue)
                {
                    if (cell.pieceData.Value.team == Team.Blue)
                        bluePieces.Add(cell.pieceData.Value);
                    else
                        redPieces.Add(cell.pieceData.Value);
                }
            }

        scoreRoutine = StartCoroutine(AnimateScoreboard(bluePieces, redPieces));
    }

    void SetPieceSprite(Image img, Sprite sprite)
    {
        img.sprite = sprite;
        img.preserveAspect = true;
    }

    IEnumerator AnimateScoreboard(List<PieceData> bluePieces, List<PieceData> redPieces)
    {
        BoardManager board = FindFirstObjectByType<BoardManager>();

        int blueRunning = 0, redRunning = 0;
        int blueTotal = 0, redTotal = 0;
        foreach (var p in bluePieces) blueTotal += p.PointValue;
        foreach (var p in redPieces) redTotal += p.PointValue;
        Team winner = pendingWinner ?? (bluePieces.Count == 0 && redPieces.Count > 0
            ? Team.Red
            : redPieces.Count == 0 && bluePieces.Count > 0
                ? Team.Blue
                : blueTotal >= redTotal ? Team.Blue : Team.Red);

        // Create one idle piece below each score panel (both front-facing)
        GameObject blueIcon = new GameObject("BluePiece");
        blueIcon.transform.SetParent(finConteoBlue.transform);
        RectTransform blueRt = blueIcon.AddComponent<RectTransform>();
        blueRt.anchorMin = new Vector2(0.5f, 0.5f);
        blueRt.anchorMax = new Vector2(0.5f, 0.5f);
        blueRt.pivot = new Vector2(0.5f, 0.5f);
        blueRt.sizeDelta = new Vector2(230, 230);
        blueRt.anchoredPosition = new Vector2(0, -300);
        Image blueImg = blueIcon.AddComponent<Image>();
        SetPieceSprite(blueImg, board.GetSpeciesFrontIdleSprite(PieceType.Paladin, board.speciesTheme));
        if (blueImg.sprite == null)
            SetPieceSprite(blueImg, board.GetPieceIdleSprite(PieceType.Paladin, Team.Blue));
        blueImg.color = Color.white;
        pieceIcons.Add(blueIcon);

        GameObject redIcon = new GameObject("RedPiece");
        redIcon.transform.SetParent(finConteoRed.transform);
        RectTransform redRt = redIcon.AddComponent<RectTransform>();
        redRt.anchorMin = new Vector2(0.5f, 0.5f);
        redRt.anchorMax = new Vector2(0.5f, 0.5f);
        redRt.pivot = new Vector2(0.5f, 0.5f);
        redRt.sizeDelta = new Vector2(230, 230);
        redRt.anchoredPosition = new Vector2(0, -300);
        Image redImg = redIcon.AddComponent<Image>();
        SetPieceSprite(redImg, board.GetSpeciesFrontIdleSprite(PieceType.Paladin, board.scenarioTheme));
        if (redImg.sprite == null)
            SetPieceSprite(redImg, board.GetPieceIdleSprite(PieceType.Paladin, Team.Red));
        redImg.color = Color.white;
        pieceIcons.Add(redIcon);

        yield return new WaitForSeconds(GameConfig.isAutoPlay ? 0.05f : 0.5f);

        for (int i = 0; i < bluePieces.Count; i++)
        {
            if (!GameConfig.isAutoPlay) SoundManager.Instance.PlayHammer();
            StartCoroutine(HitEffect(finConteoBlue.transform));
            blueRunning += bluePieces[i].PointValue;
            blueTotalText.text = $"{blueRunning} pts";
            yield return new WaitForSeconds(GameConfig.isAutoPlay ? 0.02f : 0.4f);
        }

        blueTotalText.text = $"{blueTotal} pts";

        yield return new WaitForSeconds(GameConfig.isAutoPlay ? 0.02f : 0.3f);

        for (int i = 0; i < redPieces.Count; i++)
        {
            if (!GameConfig.isAutoPlay) SoundManager.Instance.PlayHammer();
            StartCoroutine(HitEffect(finConteoRed.transform));
            redRunning += redPieces[i].PointValue;
            redTotalText.text = $"{redRunning} pts";
            yield return new WaitForSeconds(GameConfig.isAutoPlay ? 0.02f : 0.4f);
        }

        redTotalText.text = $"{redTotal} pts";

        yield return new WaitForSeconds(GameConfig.isAutoPlay ? 0.05f : 0.5f);

        // Winner gets pose + fanfare, loser gets darkened + X
        if (winner == Team.Blue)
        {
            SetPieceSprite(blueImg, board.GetPiecePoseSprite(PieceType.Paladin, Team.Blue));
            StartCoroutine(FanfareSparkles(blueRt));
            blueImg.color = Color.white;

            redImg.color = new Color(0.5f, 0.5f, 0.5f, 0.4f);
            CreateXOverlay(redRt);
        }
        else
        {
            SetPieceSprite(redImg, board.GetPiecePoseSprite(PieceType.Paladin, Team.Red));
            StartCoroutine(FanfareSparkles(redRt));
            redImg.color = Color.white;

            blueImg.color = new Color(0.5f, 0.5f, 0.5f, 0.4f);
            CreateXOverlay(blueRt);
        }

        if (winner == Team.Blue)
            StartCoroutine(VictoryBanner());
        else
            StartCoroutine(DefeatBanner());

        yield return new WaitForSeconds(GameConfig.isAutoPlay ? 0.1f : 2f);

        isShowing = false;
        if (winner == Team.Blue)
        {
            if (!GameConfig.isAutoPlay && CoinManager.Instance != null)
                CoinManager.Instance.RecordMatchWin();
            if (!GameConfig.isAutoPlay && GameConfig.isCampaign && CampaignManager.Instance != null)
                CampaignManager.Instance.CompleteLevel(GameConfig.selectedLevel);
        }

        if (GameConfig.isAutoPlay)
        {
            BoardManager bm = FindFirstObjectByType<BoardManager>();
            int blueAlive = bm != null ? bm.CountAlive(Team.Blue) : 0;
            int redAlive = bm != null ? bm.CountAlive(Team.Red) : 0;
            AutoPlayManager.Instance?.OnMatchOver(winner == Team.Blue ? "Blue" : "Red", blueAlive, redAlive);
            gameObject.SetActive(false);
            yield break;
        }

        GameOverUI.Instance.Show(winner);
        gameObject.SetActive(false);
    }

    void CreateXOverlay(RectTransform parentRt)
    {
        GameObject xObj = new GameObject("XOverlay");
        xObj.transform.SetParent(parentRt);
        Text xText = xObj.AddComponent<Text>();
        Font font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        xText.font = font;
        xText.fontSize = 20;
        xText.alignment = TextAnchor.MiddleCenter;
        xText.color = new Color(1f, 0.2f, 0.2f, 0.9f);
        xText.text = "X";
        xText.fontStyle = FontStyle.Bold;
        RectTransform xRt = xObj.GetComponent<RectTransform>();
        xRt.anchorMin = new Vector2(0.5f, 0.5f);
        xRt.anchorMax = new Vector2(0.5f, 0.5f);
        xRt.pivot = new Vector2(0.5f, 0.5f);
        xRt.sizeDelta = new Vector2(30, 30);
        xRt.anchoredPosition = Vector2.zero;

        StartCoroutine(PulseX(xObj));
    }

    IEnumerator PulseX(GameObject xObj)
    {
        float elapsed = 0f;
        while (elapsed < 1.5f)
        {
            if (xObj == null) yield break;
            RectTransform rt = xObj.GetComponent<RectTransform>();
            rt.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 6f) * 0.15f);
            Text txt = xObj.GetComponent<Text>();
            Color c = txt.color;
            c.a = 0.6f + Mathf.Sin(Time.time * 4f) * 0.3f;
            txt.color = c;
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (xObj != null)
        {
            Text txt = xObj.GetComponent<Text>();
            Color c = txt.color;
            c.a = 0.8f;
            txt.color = c;
        }
    }

    IEnumerator FanfareSparkles(RectTransform spriteRt)
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject spark = new GameObject("Sparkle");
            spark.transform.SetParent(spriteRt);
            Image sparkImg = spark.AddComponent<Image>();
            sparkImg.sprite = circleSprite;
            sparkImg.color = new Color(1f, 0.85f, 0.2f, 1f);
            RectTransform sparkRt = spark.GetComponent<RectTransform>();
            sparkRt.anchorMin = new Vector2(0.5f, 0.5f);
            sparkRt.anchorMax = new Vector2(0.5f, 0.5f);
            sparkRt.pivot = new Vector2(0.5f, 0.5f);
            sparkRt.sizeDelta = new Vector2(6, 6);
            Vector2 offset = Random.insideUnitCircle * 20f;
            sparkRt.anchoredPosition = offset;
            Vector2 vel = offset.normalized * Random.Range(30f, 60f);
            StartCoroutine(AnimateSparkle(spark, vel));
            yield return new WaitForSeconds(0.12f);
        }
    }

    IEnumerator AnimateSparkle(GameObject spark, Vector2 vel)
    {
        float t = 0;
        float lifetime = 0.6f;
        Image img = spark.GetComponent<Image>();
        while (t < lifetime)
        {
            t += Time.deltaTime;
            if (spark == null) yield break;
            RectTransform rt = spark.GetComponent<RectTransform>();
            rt.anchoredPosition += vel * Time.deltaTime;
            float life01 = t / lifetime;
            img.color = new Color(1f, 0.85f, 0.2f, Mathf.Clamp01(1f - life01));
            rt.localScale = Vector3.one * (1f + life01 * 2f);
            yield return null;
        }
        if (spark != null) Destroy(spark);
    }

    IEnumerator HitEffect(Transform section)
    {
        RectTransform secRt = section.GetComponent<RectTransform>();
        Vector2 orig = secRt.anchoredPosition;
        float t = 0;
        while (t < 0.15f)
        {
            float offset = Mathf.Sin(t / 0.15f * Mathf.PI) * 6f;
            secRt.anchoredPosition = orig + new Vector2(offset, 0);
            t += Time.deltaTime;
            yield return null;
        }
        secRt.anchoredPosition = orig;
    }

    IEnumerator VictoryBanner()
    {
        if (bannerObj != null) Destroy(bannerObj);

        bannerObj = MakeImagePanel(canvasObj.transform, "FinVictoriaPanel",
            "Sprites/Menu/Score/FinVictoriaPanel", new Vector2(0, -440), new Vector2(500, 60));

        Text bannerText = MakeLabel(bannerObj.transform, "BannerText", "BLUE WINS!", 28,
            new Color(0.3f, 0.7f, 1f), TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400, 40));

        RectTransform bannerRt = bannerObj.GetComponent<RectTransform>();
        float elapsed = 0f;
        while (elapsed < 2f)
        {
            bannerRt.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 2.5f) * 0.03f);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator DefeatBanner()
    {
        if (bannerObj != null) Destroy(bannerObj);

        bannerObj = MakeImagePanel(canvasObj.transform, "FinVictoriaPanel",
            "Sprites/Menu/Score/FinVictoriaPanel", new Vector2(0, -440), new Vector2(500, 60));

        Text bannerText = MakeLabel(bannerObj.transform, "BannerText", "DEFEAT!", 28,
            new Color(1f, 0.3f, 0.3f), TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400, 40));

        float elapsed = 0f;
        while (elapsed < 2f)
        {
            for (int i = 0; i < 2; i++)
            {
                GameObject smokePuff = new GameObject("SmokePuff");
                smokePuff.transform.SetParent(canvasObj.transform);
                Image puffImg = smokePuff.AddComponent<Image>();
                puffImg.sprite = circleSprite;
                puffImg.color = new Color(0.65f, 0.15f, 0.25f, 0.5f);
                RectTransform puffRt = smokePuff.GetComponent<RectTransform>();
                puffRt.anchorMin = new Vector2(0.5f, 0.5f);
                puffRt.anchorMax = new Vector2(0.5f, 0.5f);
                puffRt.pivot = new Vector2(0.5f, 0.5f);
                puffRt.sizeDelta = new Vector2(Random.Range(30, 60), Random.Range(30, 60));
                puffRt.anchoredPosition = new Vector2(Random.Range(-200, 200), -440);
                Vector2 vel = new Vector2(Random.Range(-40f, 40f), Random.Range(60f, 120f));
                StartCoroutine(AnimateSmokePuff(smokePuff, vel, 1.5f));
            }
            elapsed += 0.3f;
            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator AnimateSmokePuff(GameObject obj, Vector2 vel, float lifetime)
    {
        float t = 0;
        Image img = obj.GetComponent<Image>();
        while (t < lifetime)
        {
            t += Time.deltaTime;
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition += vel * Time.deltaTime;
            float life01 = t / lifetime;
            img.color = new Color(img.color.r, img.color.g, img.color.b, Mathf.Clamp01(1f - life01) * 0.5f);
            rt.localScale = Vector3.one * (1f + life01 * 3f);
            yield return null;
        }
        if (obj != null) Destroy(obj);
    }

    Text MakeLabel(Transform parent, string name, string text, int size, Color color, TextAnchor anchor, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 sizeDelta)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);

        Text label = obj.AddComponent<Text>();
        label.font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        label.fontSize = size;
        label.alignment = anchor;
        label.color = color;
        label.text = text;
        label.fontStyle = FontStyle.Bold;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = pos;

        return label;
    }
}
