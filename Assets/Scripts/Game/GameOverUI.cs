using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Gamanbit;

public class GameOverUI : MonoBehaviour
{
    private static GameOverUI _instance;
    public static GameOverUI Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("GameOverUI");
                _instance = obj.AddComponent<GameOverUI>();
            }
            return _instance;
        }
    }

    private GameObject canvasObj;
    private GameObject particleCanvasObj;
    private List<GameObject> buttons = new();
    private List<GameObject> particleObjs = new();
    private List<Coroutine> activeCoroutines = new();
    private Sprite circleSprite;
    private Sprite glowSprite;
    private bool rewardFlowRunning;

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
        CreateParticleCanvas();
        gameObject.SetActive(false);
    }

    void CreateSprites()
    {
        Texture2D circleTex = new Texture2D(16, 16);
        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                float dx = x - 7.5f;
                float dy = y - 7.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                circleTex.SetPixel(x, y, dist <= 7.5f ? Color.white : Color.clear);
            }
        }
        circleTex.Apply();
        circleSprite = Sprite.Create(circleTex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 100);

        Texture2D glowTex = new Texture2D(128, 128);
        for (int y = 0; y < 128; y++)
        {
            for (int x = 0; x < 128; x++)
            {
                float dx = (x - 63.5f) / 63.5f;
                float dy = (y - 63.5f) / 63.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(1f - dist) * 0.4f;
                glowTex.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }
        glowTex.Apply();
        glowSprite = Sprite.Create(glowTex, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 100);
    }

    void CreateUI()
    {
        canvasObj = new GameObject("GameOverCanvas");
        canvasObj.transform.SetParent(transform);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject bgObj = new GameObject("OverlayBg");
        bgObj.transform.SetParent(canvasObj.transform);

        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.75f);
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        GameObject winObj = new GameObject("WinImage");
        winObj.transform.SetParent(bgObj.transform);
        winObj.AddComponent<Image>();
        RectTransform winRt = winObj.GetComponent<RectTransform>();
        winRt.anchorMin = Vector2.zero;
        winRt.anchorMax = Vector2.one;
        winRt.offsetMin = Vector2.zero;
        winRt.offsetMax = Vector2.zero;
    }

    void CreateParticleCanvas()
    {
        particleCanvasObj = new GameObject("ParticleCanvas");
        particleCanvasObj.transform.SetParent(transform);

        Canvas canvas = particleCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 201;
    }

    void ClearButtons()
    {
        foreach (GameObject b in buttons)
        {
            if (b != null) Destroy(b);
        }
        buttons.Clear();
    }

    void ClearParticles()
    {
        foreach (Coroutine c in activeCoroutines)
        {
            if (c != null) StopCoroutine(c);
        }
        activeCoroutines.Clear();

        foreach (GameObject p in particleObjs)
        {
            if (p != null) Destroy(p);
        }
        particleObjs.Clear();
    }

    public void Dismiss()
    {
        if (rankedSparkCoroutine != null) { StopCoroutine(rankedSparkCoroutine); rankedSparkCoroutine = null; }
        ClearParticles();
        ClearButtons();
        if (particleCanvasObj != null) particleCanvasObj.SetActive(false);
        Transform overlay = canvasObj.transform.Find("OverlayBg");
        if (overlay != null) overlay.gameObject.SetActive(false);
        SoundManager.Instance.StopMusic();
    }

    GameObject CreateButton(string name, Sprite sprite, Vector2 pos, Vector2 size, Vector3 scale, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(canvasObj.transform);

        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.sprite = sprite;
        btnImage.preserveAspect = true;

        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0.5f);
        btnRt.anchorMax = new Vector2(0.5f, 0.5f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.sizeDelta = size;
        btnRt.anchoredPosition = pos;
        btnRt.localScale = scale;

        Button button = btnObj.AddComponent<Button>();
        button.targetGraphic = btnImage;
        button.onClick.AddListener(() => { SoundManager.Instance.PlayButton(); action?.Invoke(); });

        buttons.Add(btnObj);
        return btnObj;
    }

    Sprite LoadSprite(string path, string name)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);
        if (sprites == null || sprites.Length == 0) return null;
        return System.Array.Find(sprites, s => s.name == name);
    }

    Sprite LoadFirstSprite(string path)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);
        return sprites != null && sprites.Length > 0 ? sprites[0] : null;
    }

    GameObject CreateParticleImage(string name, Vector2 pos, Vector2 size, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(particleCanvasObj.transform);
        Image img = obj.AddComponent<Image>();
        img.sprite = circleSprite;
        img.color = color;
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        particleObjs.Add(obj);
        return obj;
    }

    void TrackRetry()
    {
        string mode = GameConfig.isCampaign ? "campaign" : (GameConfig.isRanked ? "ranked" : "level");
        GamanbitAnalytics.Instance?.TrackRetryEvent($"retry_{mode}", PlayerPrefs.GetInt("retry_count", 0) + 1);
        PlayerPrefs.SetInt("retry_count", PlayerPrefs.GetInt("retry_count", 0) + 1);
        PlayerPrefs.Save();
    }

    public void Show(Team winner)
    {
        CrazySDKIntegration.GameplayStop();
        if (winner == Team.Blue)
            CrazySDKIntegration.HappyTime();

        GameObject turnCanvas = GameObject.Find("TurnCanvas");
        GameObject diceCanvas = GameObject.Find("DiceCanvas");
        GameObject statsCanvas = GameObject.Find("StatsCanvas");
        if (turnCanvas != null) turnCanvas.SetActive(false);
        if (diceCanvas != null) diceCanvas.SetActive(false);
        if (statsCanvas != null) statsCanvas.SetActive(false);

        gameObject.SetActive(true);
        if (particleCanvasObj != null) particleCanvasObj.SetActive(true);
        ClearParticles();
        rewardFlowRunning = false;

        string spriteName = winner == Team.Blue ? "BlueWin" : "RedWin";
        Sprite[] winSprites = Resources.LoadAll<Sprite>($"Sprites/Win/{spriteName}");
        Sprite sprite = winSprites != null && winSprites.Length > 0 ? winSprites[0] : null;
        Transform bgObj = canvasObj.transform.Find("OverlayBg");
        if (bgObj != null) bgObj.gameObject.SetActive(true);
        Transform winObj = bgObj.Find("WinImage");
        if (winObj != null)
        {
            Image img = winObj.GetComponent<Image>();
            if (img != null && sprite != null)
                img.sprite = sprite;
        }

        if (winner == Team.Blue)
            SoundManager.Instance.PlayVictory();
        else
            SoundManager.Instance.PlayDefeat();

        SoundManager.Instance.PlayWinLoseMusic();

        ClearButtons();

        Sprite nextSprite = LoadSprite("Sprites/Menu/Next", "Next_0");
        Sprite retrySprite = LoadSprite("Sprites/Menu/Retry", "Retry_0");
        Sprite quitSprite = Resources.Load<Sprite>("Sprites/Menu/botonQuit_0");

        if (winner == Team.Blue)
        {
            if (GameConfig.isCampaign)
            {
                int nextLevel = CampaignManager.Instance != null ? CampaignManager.Instance.GetNextUncompletedCupLevel() : -1;

                if (nextLevel > 0)
                {
                    GameObject nextBtn = CreateButton("NextButton", nextSprite, new Vector2(-167, -374), new Vector2(260, 90), new Vector3(1.7f, 1.6f, 1),
                        () => {
                            OpenCampaignMap(nextLevel);
                        });
                    activeCoroutines.Add(StartCoroutine(AnimateButtonPulse(nextBtn, 0.03f)));

                    if (quitSprite != null)
                    {
                        CreateButton("QuitButton", quitSprite, new Vector2(156, -378), new Vector2(120, 42), new Vector3(1.7f, 1.7f, 1),
                            () => SceneManager.LoadScene("MainMenuScene"));
                    }
                }
                else
                {
                    Font font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
                    if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                    GameObject completeObj = new GameObject("CampaignComplete");
                    completeObj.transform.SetParent(canvasObj.transform, false);
                    Text completeText = completeObj.AddComponent<Text>();
                    completeText.font = font;
                    completeText.fontSize = 16;
                    completeText.alignment = TextAnchor.MiddleCenter;
                    completeText.text = "CAMPAIGN COMPLETE!";
                    completeText.color = new Color(1f, 0.84f, 0f);
                    RectTransform compRt = completeObj.GetComponent<RectTransform>();
                    compRt.anchorMin = new Vector2(0.5f, 0.5f);
                    compRt.anchorMax = new Vector2(0.5f, 0.5f);
                    compRt.sizeDelta = new Vector2(500, 50);
                    compRt.anchoredPosition = new Vector2(0, -280);
                    buttons.Add(completeObj);

                    GameObject mapBtn = CreateButton("MapButton", nextSprite, new Vector2(-167, -374), new Vector2(260, 90), new Vector3(1.7f, 1.6f, 1),
                        () => OpenCampaignMap(-1));
                    activeCoroutines.Add(StartCoroutine(AnimateButtonPulse(mapBtn, 0.03f)));

                    if (quitSprite != null)
                    {
                        CreateButton("QuitButton", quitSprite, new Vector2(156, -378), new Vector2(120, 42), new Vector3(1.7f, 1.7f, 1),
                            () => SceneManager.LoadScene("MainMenuScene"));
                    }

                    if (PlayerPrefs.GetInt("RankedUnlockPopupShown", 0) == 0)
                    {
                        PlayerPrefs.SetInt("RankedUnlockPopupShown", 1);
                        PlayerPrefs.Save();
                        StartCoroutine(RankedUnlockSequence(canvasObj.transform, font));
                    }
                }
            }
            else if (GameConfig.isTutorial)
            {
                GamanbitAnalytics.Instance?.TrackFtueStep("tutorial_complete", Time.timeSinceLevelLoad);
                GameObject nextBtn = CreateButton("NextButton", nextSprite, new Vector2(-167, -374), new Vector2(260, 90), new Vector3(1.7f, 1.6f, 1),
                    () => {
                        if (!rewardFlowRunning)
                        {
                            rewardFlowRunning = true;
                            StartCoroutine(ShowRewardThenProceed());
                        }
                    });
                activeCoroutines.Add(StartCoroutine(AnimateButtonPulse(nextBtn, 0.03f)));
            }
            else if (GameConfig.isRanked)
            {
                int goldReward = RankedManager.GetGoldReward();
                if (EconomyManager.Instance != null)
                    EconomyManager.Instance.AddGold(goldReward);

                Font rFont = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
                if (rFont == null) rFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                GameObject rewardObj = new GameObject("RankedReward");
                rewardObj.transform.SetParent(canvasObj.transform, false);
                Text rewardText = rewardObj.AddComponent<Text>();
                rewardText.font = rFont;
                rewardText.fontSize = 14;
                rewardText.alignment = TextAnchor.MiddleCenter;
                rewardText.text = $"+{goldReward} GOLD!";
                rewardText.color = new Color(1f, 0.84f, 0f);
                RectTransform rRt = rewardObj.GetComponent<RectTransform>();
                rRt.anchorMin = new Vector2(0.5f, 0.5f);
                rRt.anchorMax = new Vector2(0.5f, 0.5f);
                rRt.sizeDelta = new Vector2(400, 40);
                rRt.anchoredPosition = new Vector2(0, -280);
                buttons.Add(rewardObj);

                GameObject retryBtnRanked = CreateButton("RetryButton", retrySprite, new Vector2(-170, -374), new Vector2(260, 90), new Vector3(1.7f, 1.6f, 1),
                    () => { TrackRetry(); GameConfig.PlayRanked(GameConfig.currentPowerupMode); });
                activeCoroutines.Add(StartCoroutine(AnimateButtonPulse(retryBtnRanked, 0.03f)));

                Sprite menuSpriteRanked = quitSprite != null ? quitSprite : nextSprite;
                GameObject menuBtn = CreateButton("MenuButton", menuSpriteRanked, new Vector2(170, -374), new Vector2(260, 90), new Vector3(1.7f, 1.6f, 1),
                    () => SceneManager.LoadScene("MainMenuScene"));
                activeCoroutines.Add(StartCoroutine(AnimateButtonPulse(menuBtn, 0.03f)));
            }
            else
            {
                GameObject retryBtn = CreateButton("RetryButton", retrySprite, new Vector2(-167, -374), new Vector2(260, 90), new Vector3(1.7f, 1.6f, 1),
                    () => { TrackRetry(); SceneManager.LoadScene(SceneManager.GetActiveScene().name); });
                activeCoroutines.Add(StartCoroutine(AnimateButtonPulse(retryBtn, 0.03f)));

                GameObject menuBtn = CreateButton("MenuButton", nextSprite, new Vector2(0, -374), new Vector2(260, 90), new Vector3(1.7f, 1.6f, 1),
                    () => SceneManager.LoadScene("MainMenuScene"));
                activeCoroutines.Add(StartCoroutine(AnimateButtonPulse(menuBtn, 0.03f)));
            }
            activeCoroutines.Add(StartCoroutine(AnimateVictoryParticles()));
        }
        else
        {
            if (GameConfig.isRanked)
            {
                GameObject retryBtnRanked = CreateButton("RetryButton", retrySprite, new Vector2(-280, -374), new Vector2(260, 90), new Vector3(1.7f, 1.6f, 1),
                    () => { TrackRetry(); GameConfig.PlayRanked(GameConfig.currentPowerupMode); });
                activeCoroutines.Add(StartCoroutine(AnimateButtonPulse(retryBtnRanked, 0.04f)));

                if (quitSprite != null)
                {
                    GameObject menuBtn = CreateButton("MenuButton", quitSprite, new Vector2(280, -374), new Vector2(260, 90), new Vector3(1.7f, 1.6f, 1),
                        () => SceneManager.LoadScene("MainMenuScene"));
                    activeCoroutines.Add(StartCoroutine(AnimateButtonPulse(menuBtn, 0.04f)));
                }
            }
            else
            {
                GameObject retryBtn = CreateButton("RetryButton", retrySprite, new Vector2(-280, -374), new Vector2(260, 90), new Vector3(1.7f, 1.6f, 1),
                    () => { TrackRetry(); SceneManager.LoadScene(SceneManager.GetActiveScene().name); });
                if (quitSprite != null)
                {
                    CreateButton("QuitButton", quitSprite, new Vector2(260, -378), new Vector2(120, 42), new Vector3(1.7f, 1.7f, 1),
                        () => SceneManager.LoadScene("MainMenuScene"));
                }
                else
                {
                    GameObject qb = CreateButton("QuitButton", null, new Vector2(260, -378), new Vector2(120, 42), new Vector3(1.7f, 1.7f, 1),
                        () => SceneManager.LoadScene("MainMenuScene"));
                    Image qi = qb.GetComponent<Image>();
                    qi.color = new Color(0.5f, 0.1f, 0.1f, 1f);
                }
                activeCoroutines.Add(StartCoroutine(AnimateButtonPulse(retryBtn, 0.04f)));
            }
            activeCoroutines.Add(StartCoroutine(AnimateDefeatParticles()));
        }
    }

    IEnumerator DelayedButtonSound(float delay)
    {
        yield return new WaitForSeconds(delay);
        SoundManager.Instance.PlayButton();
    }

    IEnumerator AnimateVictoryParticles()
    {
        Coroutine confetti = StartCoroutine(AnimateConfettiLoop());
        Coroutine sparks = StartCoroutine(AnimateSparksLoop());
        Coroutine glow = StartCoroutine(AnimateGlowLoop());
        activeCoroutines.Add(confetti);
        activeCoroutines.Add(sparks);
        activeCoroutines.Add(glow);
        yield break;
    }

    IEnumerator AnimateDefeatParticles()
    {
        Coroutine ashes = StartCoroutine(AnimateAshesLoop());
        Coroutine smoke = StartCoroutine(AnimateRedSmokeLoop());
        Coroutine dark = StartCoroutine(AnimateDarkParticlesLoop());
        activeCoroutines.Add(ashes);
        activeCoroutines.Add(smoke);
        activeCoroutines.Add(dark);
        yield break;
    }

    IEnumerator AnimateConfettiLoop()
    {
        while (true)
        {
            int count = 8;
            for (int i = 0; i < count; i++)
            {
                Color col = new Color(Random.Range(0.2f, 0.5f), Random.Range(0.4f, 0.7f), Random.Range(0.8f, 1f), 1f);
                GameObject obj = CreateParticleImage("Confetti",
                    new Vector2(Random.Range(-700, 700), 550),
                    new Vector2(Random.Range(8, 14), Random.Range(4, 8)), col);
                Vector2 vel = new Vector2(Random.Range(-60, 60), Random.Range(-120, -260));
                float life = Random.Range(2f, 4f);
                StartCoroutine(AnimateParticle(obj, vel, life, true));
            }
            yield return new WaitForSeconds(0.6f);
        }
    }

    IEnumerator AnimateSparksLoop()
    {
        while (true)
        {
            int count = 4;
            for (int i = 0; i < count; i++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float speed = Random.Range(200f, 500f);
                Color col = new Color(1f, Random.Range(0.7f, 1f), Random.Range(0f, 0.3f), 1f);
                GameObject obj = CreateParticleImage("Spark",
                    new Vector2(Random.Range(-100, 100), Random.Range(-50, 50)),
                    new Vector2(4, 4), col);
                Vector2 vel = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed);
                float life = Random.Range(0.8f, 1.5f);
                StartCoroutine(AnimateParticle(obj, vel, life, false));
            }
            yield return new WaitForSeconds(0.4f);
        }
    }

    IEnumerator AnimateGlowLoop()
    {
        GameObject glowObj = new GameObject("Glow");
        glowObj.transform.SetParent(particleCanvasObj.transform);
        Image glowImg = glowObj.AddComponent<Image>();
        glowImg.sprite = glowSprite;
        glowImg.color = new Color(0.3f, 0.5f, 1f, 0.3f);
        RectTransform rt = glowObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(800, 800);
        rt.anchoredPosition = Vector2.zero;
        particleObjs.Add(glowObj);

        while (true)
        {
            float pulse = Mathf.Sin(Time.time * 2f) * 0.15f + 0.25f;
            glowImg.color = new Color(glowImg.color.r, glowImg.color.g, glowImg.color.b, pulse);
            rt.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 1.5f) * 0.05f);
            yield return null;
        }
    }

    IEnumerator AnimateParticle(GameObject obj, Vector2 vel, float lifetime, bool rotate)
    {
        float elapsed = 0f;
        while (elapsed < lifetime)
        {
            if (obj == null) yield break;
            float dt = Time.deltaTime;
            Image img = obj.GetComponent<Image>();
            img.rectTransform.anchoredPosition += vel * dt;
            Color c = img.color;
            c.a = Mathf.Clamp01(1f - elapsed / lifetime);
            if (rotate)
                img.rectTransform.localRotation *= Quaternion.Euler(0, 0, 180f * dt);
            img.color = c;
            elapsed += dt;
            yield return null;
        }
        if (obj != null) Destroy(obj);
    }

    IEnumerator AnimateAshesLoop()
    {
        while (true)
        {
            int count = 8;
            for (int i = 0; i < count; i++)
            {
                float grey = Random.Range(0.3f, 0.5f);
                GameObject obj = CreateParticleImage("Ash",
                    new Vector2(Random.Range(-400, 400), -250),
                    new Vector2(Random.Range(4, 9), Random.Range(4, 9)), new Color(grey, grey, grey, 1f));
                Vector2 vel = new Vector2(Random.Range(-40, 40), Random.Range(50, 120));
                float life = Random.Range(3f, 5f);
                StartCoroutine(AnimateParticle(obj, vel, life, false));
            }
            yield return new WaitForSeconds(0.7f);
        }
    }

    IEnumerator AnimateRedSmokeLoop()
    {
        while (true)
        {
            int count = 5;
            for (int i = 0; i < count; i++)
            {
                int dir = (i % 2 == 0) ? 1 : -1;
                GameObject obj = CreateParticleImage("RedSmoke",
                    new Vector2(Random.Range(-120, 120), -320),
                    new Vector2(Random.Range(40, 65), Random.Range(40, 65)),
                    new Color(0.65f, 0.15f, 0.25f, 0.55f));
                Vector2 vel = new Vector2(Random.Range(20, 50) * dir, Random.Range(60, 130));
                float life = Random.Range(3f, 5f);
                StartCoroutine(AnimateSmokeParticle(obj, vel, life));
            }
            yield return new WaitForSeconds(0.6f);
        }
    }

    IEnumerator AnimateSmokeParticle(GameObject obj, Vector2 vel, float lifetime)
    {
        float elapsed = 0f;
        while (elapsed < lifetime)
        {
            if (obj == null) yield break;
            float dt = Time.deltaTime;
            Image img = obj.GetComponent<Image>();
            img.rectTransform.anchoredPosition += vel * dt;
            float life01 = elapsed / lifetime;
            Color c = img.color;
            c.a = Mathf.Clamp01(1f - life01) * 0.6f;
            img.color = c;
            float grow = 1f + life01 * 4f;
            img.rectTransform.localScale = new Vector3(grow, grow, 1f);
            elapsed += dt;
            yield return null;
        }
        if (obj != null) Destroy(obj);
    }

    IEnumerator AnimateDarkParticlesLoop()
    {
        while (true)
        {
            int count = 5;
            for (int i = 0; i < count; i++)
            {
                float dark = Random.Range(0.05f, 0.2f);
                GameObject obj = CreateParticleImage("DarkParticle",
                    new Vector2(Random.Range(-500, 500), Random.Range(-200, 300)),
                    new Vector2(Random.Range(3, 8), Random.Range(3, 8)), new Color(dark, dark, dark, 0.6f));
                Vector2 vel = new Vector2(Random.Range(-15, 15), Random.Range(-10, 20));
                float life = Random.Range(3f, 6f);
                StartCoroutine(AnimateParticle(obj, vel, life, false));
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator ShowGoblinThenProceed()
    {
        GameObject goblinObj = new GameObject("GoblinDialogue");
        goblinObj.transform.SetParent(transform);
        GoblinDialogue goblin = goblinObj.AddComponent<GoblinDialogue>();
        goblin.Show();
        while (!GoblinDialogue.HasShown())
        {
            yield return null;
        }
        Destroy(goblinObj);
        ShowCampaignModeSelection();
    }

    void ShowModeSelection()
    {
        Canvas gameOverCanvas = canvasObj != null ? canvasObj.GetComponent<Canvas>() : null;
        ModeSelectionUI modeUI = gameObject.AddComponent<ModeSelectionUI>();
        if (gameOverCanvas != null)
            modeUI.Show(gameOverCanvas);
        else
            modeUI.Show(GameObject.Find("TurnCanvas")?.GetComponent<Canvas>());
    }

    void ShowCampaignModeSelection()
    {
        Dismiss();
        Canvas gameOverCanvas = canvasObj != null ? canvasObj.GetComponent<Canvas>() : null;
        ModeSelectionUI modeUI = gameObject.AddComponent<ModeSelectionUI>();
        Canvas target = gameOverCanvas != null ? gameOverCanvas : GameObject.Find("TurnCanvas")?.GetComponent<Canvas>();
        int nextLevel = CampaignManager.Instance != null ? CampaignManager.Instance.GetNextUncompletedCupLevel() : -1;
        if (target != null)
            modeUI.ShowCampaign(target, nextLevel);
    }

    void OpenCampaignMap(int targetLevel)
    {
        Dismiss();
        Canvas gameOverCanvas = canvasObj != null ? canvasObj.GetComponent<Canvas>() : null;
        Canvas target = gameOverCanvas != null ? gameOverCanvas : GameObject.Find("TurnCanvas")?.GetComponent<Canvas>();
        if (target == null)
        {
            GameConfig.PlayCampaign(targetLevel);
            return;
        }
        CampaignMapUI mapUI = gameObject.AddComponent<CampaignMapUI>();
        mapUI.loadsSceneOnClose = true;
        mapUI.OnClose = () => SceneManager.LoadScene("MainMenuScene");
        mapUI.ShowForNextLevel(target, targetLevel);
    }

    IEnumerator ShowRewardThenProceed()
    {
        if (!TutorialRewardUI.HasClaimed())
        {
            GameObject rewardObj = new GameObject("TutorialReward");
            rewardObj.transform.SetParent(transform);
            TutorialRewardUI reward = rewardObj.AddComponent<TutorialRewardUI>();
            reward.Show();
            while (!TutorialRewardUI.HasClaimed())
            {
                yield return null;
            }
            if (rewardObj != null) Destroy(rewardObj);
        }

        if (!GoblinDialogue.HasShown())
        {
            yield return StartCoroutine(ShowGoblinThenProceed());
        }
        else
        {
            ShowCampaignModeSelection();
        }
    }

    IEnumerator AnimateButtonPulse(GameObject btn, float intensity)
    {
        if (btn == null) yield break;
        RectTransform rt = btn.GetComponent<RectTransform>();
        if (rt == null) yield break;
        Vector3 orig = rt.localScale;
        while (true)
        {
            if (btn == null || !btn) yield break;
            rt = btn.GetComponent<RectTransform>();
            if (rt == null) yield break;
            float pulse = 1f + Mathf.Sin(Time.time * 3.5f) * intensity;
            rt.localScale = orig * pulse;
            yield return null;
        }
    }

    IEnumerator RankedUnlockSequence(Transform parent, Font font)
    {
        yield return new WaitForSecondsRealtime(0.6f);

        GameObject dimObj = new GameObject("RankedUnlockDim");
        dimObj.transform.SetParent(parent, false);
        Image dimImg = dimObj.AddComponent<Image>();
        dimImg.color = new Color(0, 0, 0, 0);
        dimImg.raycastTarget = false;
        RectTransform dimRt = dimImg.GetComponent<RectTransform>();
        dimRt.anchorMin = Vector2.zero;
        dimRt.anchorMax = Vector2.one;
        dimRt.offsetMin = Vector2.zero;
        dimRt.offsetMax = Vector2.zero;
        dimRt.SetAsFirstSibling();

        float dimT = 0f;
        while (dimT < 0.25f)
        {
            dimT += Time.unscaledDeltaTime;
            dimImg.color = new Color(0, 0, 0, Mathf.Lerp(0f, 0.65f, dimT / 0.25f));
            yield return null;
        }
        dimImg.color = new Color(0, 0, 0, 0.65f);

        GameObject panelObj = new GameObject("RankedUnlockPanel");
        panelObj.transform.SetParent(parent, false);
        Image panelImg = panelObj.AddComponent<Image>();
        Sprite panelSpr = LoadFirstSprite("Sprites/Menu/Score/FinVictoriaPanel");
        if (panelSpr != null) { panelImg.sprite = panelSpr; panelImg.preserveAspect = false; }
        else { panelImg.color = new Color(0.16f, 0.1f, 0.24f, 0.97f); }
        RectTransform panelRt = panelImg.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(720, 420);
        panelRt.anchoredPosition = new Vector2(0, 40);

        GameObject titleObj = new GameObject("RankedTitle");
        titleObj.transform.SetParent(panelObj.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.font = font;
        titleText.text = "RANKED\nUNLOCKED!";
        titleText.fontSize = 30;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(1f, 0.84f, 0f);
        Outline titleOl = titleObj.AddComponent<Outline>();
        titleOl.effectColor = new Color(0.2f, 0.05f, 0f, 0.95f);
        titleOl.effectDistance = new Vector2(2, -2);
        RectTransform titleRt = titleText.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 0.5f);
        titleRt.anchorMax = new Vector2(0.5f, 0.5f);
        titleRt.sizeDelta = new Vector2(620, 110);
        titleRt.anchoredPosition = new Vector2(0, 120);

        Sprite rankedSprite = LoadSprite("Sprites/Menu/ranked", "ranked_0");
        if (rankedSprite == null)
            rankedSprite = Resources.Load<Sprite>("Sprites/Menu/botonOpen_0");

        GameObject rankedBtnObj = new GameObject("RankedUnlockBtn");
        rankedBtnObj.transform.SetParent(panelObj.transform, false);
        Image rankedImg = rankedBtnObj.AddComponent<Image>();
        if (rankedSprite != null) { rankedImg.sprite = rankedSprite; rankedImg.preserveAspect = true; rankedImg.color = Color.white; }
        else { rankedImg.color = new Color(0.6f, 0.2f, 0.8f); }
        RectTransform rankedRt = rankedImg.GetComponent<RectTransform>();
        rankedRt.anchorMin = new Vector2(0.5f, 0.5f);
        rankedRt.anchorMax = new Vector2(0.5f, 0.5f);
        rankedRt.sizeDelta = new Vector2(320, 90);
        rankedRt.anchoredPosition = new Vector2(0, 10);

        Text rankedLabel = null;
        if (rankedSprite == null)
        {
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(rankedBtnObj.transform, false);
            rankedLabel = labelObj.AddComponent<Text>();
            rankedLabel.font = font;
            rankedLabel.text = "RANKED";
            rankedLabel.fontSize = 20;
            rankedLabel.alignment = TextAnchor.MiddleCenter;
            rankedLabel.color = Color.white;
            RectTransform lblRt = labelObj.GetComponent<RectTransform>();
            lblRt.anchorMin = Vector2.zero;
            lblRt.anchorMax = Vector2.one;
            lblRt.sizeDelta = Vector2.zero;
        }

        Text descText = null;
        GameObject descObj = new GameObject("RankedDesc");
        descObj.transform.SetParent(panelObj.transform, false);
        descText = descObj.AddComponent<Text>();
        descText.font = font;
        descText.text = "FIGHT RANDOM ARMIES!";
        descText.fontSize = 13;
        descText.alignment = TextAnchor.MiddleCenter;
        descText.color = new Color(1f, 1f, 1f, 0.92f);
        RectTransform descRt = descText.GetComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0.5f, 0.5f);
        descRt.anchorMax = new Vector2(0.5f, 0.5f);
        descRt.sizeDelta = new Vector2(620, 40);
        descRt.anchoredPosition = new Vector2(0, -80);

        CanvasGroup cg = panelObj.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        panelRt.localScale = Vector3.one * 0.2f;

        SoundManager.Instance.PlayVictory();

        float appearT = 0f;
        while (appearT < 0.3f)
        {
            appearT += Time.unscaledDeltaTime;
            float p = appearT / 0.3f;
            cg.alpha = p;
            panelRt.localScale = Vector3.one * Mathf.Lerp(0.2f, 1f, Mathf.SmoothStep(0f, 1f, p));
            yield return null;
        }
        cg.alpha = 1f;

        panelRt.localScale = Vector3.one * 1.08f;
        yield return new WaitForSecondsRealtime(0.05f);
        panelRt.localScale = Vector3.one * 0.96f;
        yield return new WaitForSecondsRealtime(0.04f);
        panelRt.localScale = Vector3.one * 1.04f;
        yield return new WaitForSecondsRealtime(0.03f);
        panelRt.localScale = Vector3.one;

        if (rankedSprite != null)
        {
            rankedRt.localScale = Vector3.one * 1.4f;
            yield return new WaitForSecondsRealtime(0.05f);
            rankedRt.localScale = Vector3.one * 0.95f;
            yield return new WaitForSecondsRealtime(0.04f);
            rankedRt.localScale = Vector3.one * 1.05f;
            yield return new WaitForSecondsRealtime(0.03f);
            rankedRt.localScale = Vector3.one;
        }

        float burstDelay = 0f;
        for (int i = 0; i < 4; i++)
        {
            yield return new WaitForSecondsRealtime(burstDelay);
            SpawnUnlockSparks(parent, new Vector2(0, 40), i * 90f + 15f, 10);
            burstDelay = 0.12f;
        }

        Button rankedBtn = rankedBtnObj.AddComponent<Button>();
        rankedBtn.targetGraphic = rankedImg;
        rankedBtn.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlayButton();
            StopCoroutine(rankedSparkCoroutine);
            Destroy(dimObj);
            Destroy(panelObj);
            Dismiss();
            ModeSelectionUI modeUI = gameObject.AddComponent<ModeSelectionUI>();
            Canvas c = canvasObj != null ? canvasObj.GetComponent<Canvas>() : null;
            if (c == null) c = GameObject.Find("TurnCanvas")?.GetComponent<Canvas>();
            if (c != null) modeUI.Show(c);
        });

        StartCoroutine(AnimateButtonPulse(rankedBtnObj, 0.03f));

        rankedSparkCoroutine = StartCoroutine(RankedSparksLoop(parent));
    }

    Coroutine rankedSparkCoroutine;

    IEnumerator RankedSparksLoop(Transform parent)
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(0.6f);
            SpawnUnlockSparks(parent, new Vector2(0, 40), Random.Range(0f, 360f), 3);
        }
    }

    void SpawnUnlockSparks(Transform parent, Vector2 origin, float baseAngle, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (baseAngle + (i / (float)count) * 360f) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            GameObject spark = new GameObject("UnlockSpark");
            spark.transform.SetParent(parent, false);
            Image sparkImg = spark.AddComponent<Image>();
            sparkImg.color = new Color(0.85f, 0.6f, 1f, 1f);
            sparkImg.raycastTarget = false;
            RectTransform sparkRt = spark.GetComponent<RectTransform>();
            sparkRt.anchorMin = new Vector2(0.5f, 0.5f);
            sparkRt.anchorMax = new Vector2(0.5f, 0.5f);
            sparkRt.sizeDelta = new Vector2(12, 12);
            sparkRt.anchoredPosition = origin;
            StartCoroutine(AnimateUnlockSpark(spark, origin, dir * Random.Range(90f, 200f)));
        }
    }

    IEnumerator AnimateUnlockSpark(GameObject spark, Vector2 origin, Vector2 velocity)
    {
        RectTransform rt = spark.GetComponent<RectTransform>();
        Image img = spark.GetComponent<Image>();
        float life = 0.6f;
        float t = 0f;
        while (t < life)
        {
            if (spark == null) yield break;
            t += Time.unscaledDeltaTime;
            float p = t / life;
            rt.anchoredPosition = origin + velocity * p;
            if (img != null) img.color = new Color(0.8f, 0.4f, 1f, 1f - p);
            float s = 1f + Mathf.Sin(p * Mathf.PI) * 0.5f;
            spark.transform.localScale = Vector3.one * s;
            yield return null;
        }
        if (spark != null) Destroy(spark);
    }
}
