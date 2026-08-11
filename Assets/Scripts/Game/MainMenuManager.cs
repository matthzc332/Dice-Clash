using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    private static readonly string[] species = { "Human", "Orc", "Beastfolk" };
    private static readonly string[] menuSpriteNames = { "menuHuman", "menuOrc", "menuBeast" };
    private int selected = 0;
    private Transform canvasTransform;
    private Canvas menuCanvas;
    private float canvasW = 1920f;
    private CampaignUI campaignUI;
    private ChestUI chestUI;
    private InsigniaUI insigniaUI;
    private ExhibidorUI exhibidorUI;
    private ModeSelectionUI modeSelectionUI;
    private Text collectAllLabel;

    static bool IsUnlocked(int index)
    {
        if (index == 0) return true;
        string key = "Unlocked_" + species[index];
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    static bool IsBeaten(int index)
    {
        string key = "Trophy_" + species[index];
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    void Start()
    {
        SetupEventSystem();
        SetupCamera();
        CreateMenu();
        if (EconomyManager.Instance == null)
            gameObject.AddComponent<EconomyManager>();
        if (DailyBonusUI.Instance == null)
            gameObject.AddComponent<DailyBonusUI>();
        if (TutorialProgress.HasPlayed())
            StartCoroutine(ShowDailyBonusDelayed());
        SoundManager.Instance.PlayMenuMusic();
    }

    void Update()
    {
    }

    void StartPlayButtonAura(RectTransform playRt, Transform parent)
    {
        GameObject auraObj = new GameObject("PlayAura");
        auraObj.transform.SetParent(parent, false);

        Image auraImg = auraObj.AddComponent<Image>();
        Texture2D auraTex = new Texture2D(128, 128);
        for (int y = 0; y < 128; y++)
        {
            for (int x = 0; x < 128; x++)
            {
                float dx = (x - 63.5f) / 63.5f;
                float dy = (y - 63.5f) / 63.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(1f - dist) * 0.5f;
                auraTex.SetPixel(x, y, new Color(0.3f, 0.7f, 1f, alpha));
            }
        }
        auraTex.Apply();
        Sprite auraSprite = Sprite.Create(auraTex, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 100);
        auraImg.sprite = auraSprite;
        auraImg.raycastTarget = false;

        RectTransform auraRt = auraObj.GetComponent<RectTransform>();
        auraRt.anchorMin = new Vector2(0.5f, 0.5f);
        auraRt.anchorMax = new Vector2(0.5f, 0.5f);
        auraRt.pivot = new Vector2(0.5f, 0.5f);
        auraRt.sizeDelta = new Vector2(180, 80);
        auraRt.anchoredPosition = playRt.anchoredPosition;

        StartCoroutine(AnimatePlayAura(auraRt, auraImg));
        StartCoroutine(AnimatePlayPulse(playRt));
    }

    IEnumerator AnimatePlayAura(RectTransform auraRt, Image auraImg)
    {
        while (true)
        {
            float pulse = Mathf.Sin(Time.time * 2.5f) * 0.15f + 0.25f;
            auraImg.color = new Color(auraImg.color.r, auraImg.color.g, auraImg.color.b, pulse);
            float scale = 1f + Mathf.Sin(Time.time * 2f) * 0.04f;
            auraRt.localScale = Vector3.one * scale;
            yield return null;
        }
    }

    IEnumerator AnimatePlayPulse(RectTransform rt)
    {
        Vector3 orig = rt.localScale;
        while (true)
        {
            float pulse = 1f + Mathf.Sin(Time.time * 3f) * 0.015f;
            rt.localScale = orig * pulse;
            yield return null;
        }
    }

    IEnumerator ButtonClickEffect(Button button)
    {
        RectTransform rt = button.GetComponent<RectTransform>();
        Vector3 orig = rt.localScale;
        rt.localScale = orig * 0.88f;
        yield return new WaitForSeconds(0.06f);
        rt.localScale = orig * 1.05f;
        yield return new WaitForSeconds(0.04f);
        rt.localScale = orig;
    }

    void SetupEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }

    void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = FindFirstObjectByType<Camera>();
            if (cam != null) cam.gameObject.tag = "MainCamera";
            else
            {
                GameObject obj = new GameObject("Main Camera");
                cam = obj.AddComponent<Camera>();
                obj.tag = "MainCamera";
            }
        }
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.06f, 0.04f);
        if (cam.GetComponent<AudioListener>() == null)
            cam.gameObject.AddComponent<AudioListener>();
    }

    void CreateMenu()
    {
        GameObject canvasObj = new GameObject("MenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(canvasW, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();
        canvasTransform = canvasObj.transform;
        menuCanvas = canvas;

        Font font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");

        Sprite bgSprite = Resources.Load<Sprite>("Sprites/Menu/Menu");
        if (bgSprite != null)
        {
            GameObject bgObj = new GameObject("Background", typeof(RectTransform));
            bgObj.transform.SetParent(canvasTransform, false);
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.sprite = bgSprite;
            bgImg.raycastTarget = false;
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
        }

        BuildTrophiesButton(font);

        if (CampaignManager.Instance == null)
        {
            GameObject cmObj = new GameObject("CampaignManager");
            cmObj.AddComponent<CampaignManager>();
        }

        BuildCarousel();

        CreateArrow(canvasTransform, font, -340, -211, "<", () => Cycle(-1));
        CreateArrow(canvasTransform, font, 340, -211, ">", () => Cycle(1));

        CreateCampaignButton(canvasTransform, font);
        CreateChestButton(canvasTransform, font);
        CreateInsigniaButton(canvasTransform, font);
        CreateCollectAllButton(canvasTransform, font);
        CreateResetButton(canvasTransform, font);

        Sprite[] playSprites = Resources.LoadAll<Sprite>("Sprites/Menu/botonPlay");
        Sprite playSprite = System.Array.Find(playSprites, s => s.name == "botonplay2_0");
        if (playSprite != null)
        {
            GameObject playObj = new GameObject("PlayButton", typeof(RectTransform));
            playObj.transform.SetParent(canvasTransform, false);
            Image playImg = playObj.AddComponent<Image>();
            playImg.sprite = playSprite;
            playImg.preserveAspect = true;
            RectTransform playRt = playObj.GetComponent<RectTransform>();
            playRt.anchorMin = new Vector2(0.5f, 0.5f);
            playRt.anchorMax = new Vector2(0.5f, 0.5f);
            playRt.pivot = new Vector2(0.5f, 0.5f);
            playRt.sizeDelta = new Vector2(85, 30);
            playRt.anchoredPosition = new Vector2(-13, -365);
            playRt.localScale = new Vector3(5.5f, 4.5f, 1f);

            Button playButton = playObj.AddComponent<Button>();
            playButton.targetGraphic = playImg;
            playButton.onClick.AddListener(OnPlayClicked);
            playButton.onClick.AddListener(() => SoundManager.Instance.PlayButton());
            playButton.onClick.AddListener(() => StartCoroutine(ButtonClickEffect(playButton)));

            StartPlayButtonAura(playRt, canvasTransform);
        }

        CreateTutorialButton(canvasTransform, font);
    }

    void CreateTutorialButton(Transform parent, Font font)
    {
        GameObject btnObj = new GameObject("TutorialButton", typeof(RectTransform));
        btnObj.transform.SetParent(parent, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.3f, 0.5f, 0.3f, 0.9f);
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0.5f);
        btnRt.anchorMax = new Vector2(0.5f, 0.5f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.sizeDelta = new Vector2(200, 50);
        btnRt.anchoredPosition = new Vector2(-13, -440);

        GameObject textObj = new GameObject("TutorialText", typeof(RectTransform));
        textObj.transform.SetParent(btnObj.transform, false);
        Text label = textObj.AddComponent<Text>();
        label.font = font;
        if (label.font == null) label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 14;
        label.alignment = TextAnchor.MiddleCenter;
        label.text = "TUTORIAL";
        label.color = Color.white;
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySelect();
            GameConfig.PlayTutorial();
        });

    }

    void CreateArrow(Transform parent, Font font, float xPos, float yPos, string symbol, System.Action onClick)
    {
        GameObject btnObj = new GameObject($"Arrow_{xPos}", typeof(RectTransform));
        btnObj.transform.SetParent(parent, false);
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.3f, 0.3f, 0.5f, 0.8f);
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0.5f);
        btnRt.anchorMax = new Vector2(0.5f, 0.5f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.sizeDelta = new Vector2(44, 44);
        btnRt.anchoredPosition = new Vector2(xPos, yPos);

        GameObject textObj = new GameObject("ArrowText", typeof(RectTransform));
        textObj.transform.SetParent(btnObj.transform, false);
        Text arrowText = textObj.AddComponent<Text>();
        arrowText.font = font;
        arrowText.fontSize = 22;
        arrowText.alignment = TextAnchor.MiddleCenter;
        arrowText.text = symbol;
        arrowText.color = Color.white;
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImage;
        btn.onClick.AddListener(() => onClick());
    }

    void Cycle(int direction)
    {
        SoundManager.Instance.PlaySelect();
        selected = (selected + direction + species.Length) % species.Length;
        DestroyOldCarousel();
        BuildCarousel();
    }

    void BuildTrophiesButton(Font font)
    {
        GameObject btnObj = new GameObject("TrophiesButton", typeof(RectTransform));
        btnObj.transform.SetParent(canvasTransform, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.5f, 0.4f, 0.15f, 0.9f);
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0.5f);
        btnRt.anchorMax = new Vector2(0.5f, 0.5f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.sizeDelta = new Vector2(200, 50);
        btnRt.anchoredPosition = new Vector2(814, 187);

        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(btnObj.transform, false);
        Text label = textObj.AddComponent<Text>();
        label.font = font;
        label.fontSize = 12;
        label.alignment = TextAnchor.MiddleCenter;
        label.text = "TROPHIES";
        label.color = new Color(1f, 0.9f, 0.6f);
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySelect();
            if (exhibidorUI == null)
            {
                if (menuCanvas != null)
                {
                    exhibidorUI = gameObject.AddComponent<ExhibidorUI>();
                    exhibidorUI.OnClose = () => { exhibidorUI = null; };
                    exhibidorUI.Show(menuCanvas);
                }
            }
            else
            {
                exhibidorUI.Close();
            }
        });

        int ribbonCount = RibbonManager.GetTotalRibbons();
        int cupCount = CampaignManager.Instance != null ? CampaignManager.Instance.GetCompletedCupCount() : 0;
        GameObject progressObj = new GameObject("Progress", typeof(RectTransform));
        progressObj.transform.SetParent(btnObj.transform, false);
        Text progressText = progressObj.AddComponent<Text>();
        progressText.font = font;
        progressText.fontSize = 8;
        progressText.alignment = TextAnchor.MiddleCenter;
        progressText.text = $"{cupCount}/4 cups | {ribbonCount}/22 ribbons";
        progressText.color = new Color(0.8f, 0.7f, 0.4f);
        RectTransform progRt = progressObj.GetComponent<RectTransform>();
        progRt.anchorMin = new Vector2(0f, -0.8f);
        progRt.anchorMax = new Vector2(1f, -0.2f);
        progRt.sizeDelta = Vector2.zero;
    }

    void DestroyOldCarousel()
    {
        string[] names = { "CenterSlot", "LeftSlot", "RightSlot" };
        foreach (string n in names)
        {
            Transform t = FindDirectChild(canvasTransform, n);
            if (t != null) Destroy(t.gameObject);
        }
    }

    void BuildCarousel()
    {
        float slotW = 140f;
        float slotH = 182f;

        BuildSlot("CenterSlot", -8, -211, slotW, slotH, species[selected], true);
        BuildSlot("LeftSlot", -228, -211, slotW * 0.7f, slotH * 0.7f, species[(selected + 2) % species.Length], false);
        BuildSlot("RightSlot", 212, -211, slotW * 0.7f, slotH * 0.7f, species[(selected + 1) % species.Length], false);
    }

    void BuildSlot(string name, float x, float y, float w, float h, string sp, bool center)
    {
        int spIdx = System.Array.IndexOf(species, sp);
        bool locked = spIdx < 0 || !IsUnlocked(spIdx);
        Font font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");

        GameObject slotObj = new GameObject(name, typeof(RectTransform));
        slotObj.transform.SetParent(canvasTransform, false);
        RectTransform slotRt = slotObj.GetComponent<RectTransform>();
        slotRt.anchorMin = new Vector2(0.5f, 0.5f);
        slotRt.anchorMax = new Vector2(0.5f, 0.5f);
        slotRt.pivot = new Vector2(0.5f, 0.5f);
        slotRt.sizeDelta = new Vector2(w, h);
        slotRt.anchoredPosition = new Vector2(x, y);

        string menuName = spIdx >= 0 ? menuSpriteNames[spIdx] : "menuHuman";
        Sprite menuSprite = Resources.Load<Sprite>($"Sprites/Menu/{menuName}");
        if (menuSprite == null)
        {
            Debug.LogWarning($"MainMenu: menu sprite not found for {sp} at Sprites/Menu/{menuName}");
        }
        else
        {
            GameObject imgObj = new GameObject("MenuImg", typeof(RectTransform));
            imgObj.transform.SetParent(slotObj.transform, false);
            Image img = imgObj.AddComponent<Image>();
            img.sprite = menuSprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.color = center
                ? (locked ? new Color(0.5f, 0.5f, 0.5f, 0.8f) : Color.white)
                : new Color(0.3f, 0.3f, 0.3f, 0.6f);
            RectTransform imgRt = imgObj.GetComponent<RectTransform>();
            imgRt.anchorMin = Vector2.zero;
            imgRt.anchorMax = Vector2.one;
            imgRt.sizeDelta = Vector2.zero;
        }

        GameObject labelObj = new GameObject("Label", typeof(RectTransform));
        labelObj.transform.SetParent(slotObj.transform, false);
        Text label = labelObj.AddComponent<Text>();
        label.font = font;
        label.fontSize = center ? 18 : 14;
        label.alignment = TextAnchor.MiddleCenter;
        label.text = locked ? "LOCKED" : sp.ToUpper();
        label.color = locked ? new Color(0.5f, 0.5f, 0.5f) : new Color(0.9f, 0.7f, 0.2f);
        RectTransform labelRt = labelObj.GetComponent<RectTransform>();
        labelRt.anchorMin = new Vector2(0f, 0f);
        labelRt.anchorMax = new Vector2(1f, 0f);
        labelRt.pivot = new Vector2(0.5f, 0f);
        labelRt.sizeDelta = new Vector2(0, 30);
        labelRt.anchoredPosition = new Vector2(0, 4);
    }

    Transform FindDirectChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
        }
        return null;
    }

    void OnPlayClicked()
    {
        Debug.LogWarning($"[Play] selected={selected} species={species[selected]} unlocked={IsUnlocked(selected)}");
        if (!IsUnlocked(selected)) return;
        SoundManager.Instance.PlaySelect();

        if (modeSelectionUI == null)
        {
            if (menuCanvas != null)
            {
                modeSelectionUI = gameObject.AddComponent<ModeSelectionUI>();
                modeSelectionUI.Show(menuCanvas);
            }
        }
    }

    void CreateCampaignButton(Transform parent, Font f)
    {
        GameObject btnObj = new GameObject("CampaignButton", typeof(RectTransform));
        btnObj.transform.SetParent(parent, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.55f, 0.35f, 0.15f, 0.9f);
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0.5f);
        btnRt.anchorMax = new Vector2(0.5f, 0.5f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.sizeDelta = new Vector2(200, 50);
        btnRt.anchoredPosition = new Vector2(-13, -510);

        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(btnObj.transform, false);
        Text label = textObj.AddComponent<Text>();
        label.font = f;
        label.fontSize = 13;
        label.alignment = TextAnchor.MiddleCenter;
        label.text = "CAMPAIGN";
        label.color = new Color(1f, 0.9f, 0.6f);
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySelect();
            if (campaignUI == null)
            {
                if (menuCanvas != null)
                {
                    campaignUI = gameObject.AddComponent<CampaignUI>();
                    campaignUI.OnClose = () => { campaignUI = null; };
                    campaignUI.Show(menuCanvas);
                }
            }
            else
            {
                campaignUI.Close();
            }
        });

        int completed = CampaignManager.Instance != null ? CampaignManager.Instance.GetCompletedCount() : 0;
        GameObject progressObj = new GameObject("Progress", typeof(RectTransform));
        progressObj.transform.SetParent(btnObj.transform, false);
        Text progressText = progressObj.AddComponent<Text>();
        progressText.font = f;
        progressText.fontSize = 9;
        progressText.alignment = TextAnchor.MiddleCenter;
        progressText.text = $"{completed}/22";
        progressText.color = new Color(0.8f, 0.7f, 0.4f);
        RectTransform progRt = progressObj.GetComponent<RectTransform>();
        progRt.anchorMin = new Vector2(0f, -0.1f);
        progRt.anchorMax = new Vector2(1f, -0.1f);
        progRt.sizeDelta = new Vector2(0, 20);
    }

    void CreateChestButton(Transform parent, Font f)
    {
        GameObject btnObj = new GameObject("ChestButton", typeof(RectTransform));
        btnObj.transform.SetParent(parent, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.5f, 0.4f, 0.15f, 0.9f);
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0.5f);
        btnRt.anchorMax = new Vector2(0.5f, 0.5f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.sizeDelta = new Vector2(170, 42);
        btnRt.anchoredPosition = new Vector2(260, -510);

        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(btnObj.transform, false);
        Text label = textObj.AddComponent<Text>();
        label.font = f;
        label.fontSize = 11;
        label.alignment = TextAnchor.MiddleCenter;
        label.text = "CHESTS";
        label.color = new Color(1f, 0.9f, 0.6f);
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySelect();
            if (chestUI == null)
            {
                if (menuCanvas != null)
                {
                    chestUI = gameObject.AddComponent<ChestUI>();
                    chestUI.Show(menuCanvas);
                }
            }
            else
            {
                chestUI.Hide();
                Destroy(chestUI);
                chestUI = null;
            }
        });
    }

    void CreateInsigniaButton(Transform parent, Font f)
    {
        GameObject btnObj = new GameObject("InsigniaButton", typeof(RectTransform));
        btnObj.transform.SetParent(parent, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.35f, 0.25f, 0.5f, 0.9f);
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0.5f);
        btnRt.anchorMax = new Vector2(0.5f, 0.5f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.sizeDelta = new Vector2(170, 42);
        btnRt.anchoredPosition = new Vector2(460, -510);

        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(btnObj.transform, false);
        Text label = textObj.AddComponent<Text>();
        label.font = f;
        label.fontSize = 10;
        label.alignment = TextAnchor.MiddleCenter;
        label.text = "BADGES";
        label.color = new Color(0.9f, 0.8f, 1f);
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySelect();
            if (insigniaUI == null)
            {
                if (menuCanvas != null)
                {
                    insigniaUI = gameObject.AddComponent<InsigniaUI>();
                    insigniaUI.Show(menuCanvas);
                }
            }
            else
            {
                insigniaUI.Hide();
                Destroy(insigniaUI);
                insigniaUI = null;
            }
        });
    }

    void CreateCollectAllButton(Transform parent, Font f)
    {
        GameObject btnObj = new GameObject("CollectAllButton", typeof(RectTransform));
        btnObj.transform.SetParent(parent, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.4f, 0.18f, 0.4f, 0.9f);
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0.5f);
        btnRt.anchorMax = new Vector2(0.5f, 0.5f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.sizeDelta = new Vector2(170, 42);
        btnRt.anchoredPosition = new Vector2(-280, -510);

        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(btnObj.transform, false);
        Text label = textObj.AddComponent<Text>();
        label.font = f;
        label.fontSize = 9;
        label.alignment = TextAnchor.MiddleCenter;
        label.text = "COLLECT ALL";
        label.color = new Color(1f, 0.8f, 1f);
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;
        collectAllLabel = label;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySelect();
            if (IsEverythingCollected())
            {
                ResetAllCollections();
                label.text = "COLLECT ALL";
            }
            else
            {
                GrantAllCollections();
                label.text = "RESET ALL";
            }
        });
    }

    void CreateResetButton(Transform parent, Font f)
    {
        GameObject btnObj = new GameObject("ResetButton", typeof(RectTransform));
        btnObj.transform.SetParent(parent, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.5f, 0.2f, 0.2f, 0.9f);
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0.5f);
        btnRt.anchorMax = new Vector2(0.5f, 0.5f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.sizeDelta = new Vector2(120, 42);
        btnRt.anchoredPosition = new Vector2(700, -510);

        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(btnObj.transform, false);
        Text label = textObj.AddComponent<Text>();
        label.font = f;
        label.fontSize = 9;
        label.alignment = TextAnchor.MiddleCenter;
        label.text = "RESET";
        label.color = new Color(1f, 0.7f, 0.7f);
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySelect();
            ResetAllCollections();
            if (collectAllLabel != null) collectAllLabel.text = "COLLECT ALL";
        });
    }

    bool IsEverythingCollected()
    {
        return InsigniaManager.GetCollectedCount() >= InsigniaManager.GetTotalCount()
            && RibbonManager.GetTotalRibbons() >= RibbonManager.GetTotalPossibleRibbons();
    }

    void GrantAllCollections()
    {
        CampaignDataWrapper data = CampaignData.Load();
        if (data == null) return;

        if (data.levels != null)
        {
            foreach (CampaignLevel level in data.levels)
            {
                PlayerPrefs.SetInt("Campaign_Level_" + level.id, 1);
                RibbonManager.GrantRibbon(level.id);
                InsigniaManager.GrantCampaignInsignia(level.id);
            }
        }

        InsigniaDataWrapper insigniaData = InsigniaData.Load();
        if (insigniaData != null && insigniaData.insignias != null)
        {
            foreach (Insignia ins in insigniaData.insignias)
            {
                InsigniaManager.GrantInsignia(ins.id);
            }
        }

        if (data.cups != null)
        {
            foreach (CampaignCup cup in data.cups)
            {
                PlayerPrefs.SetInt("Unlocked_" + cup.race, 1);
            }
        }

        foreach (string sp in species)
        {
            PlayerPrefs.SetInt("Unlocked_" + sp, 1);
            PlayerPrefs.SetInt("Trophy_" + sp, 1);
        }

        PlayerPrefs.Save();
        Debug.Log("CollectAll: all cups/insignias/ribbons granted");
    }

    void ResetAllCollections()
    {
        if (CampaignManager.Instance != null)
        {
            CampaignManager.Instance.ResetProgress();
        }
        else
        {
            CampaignDataWrapper data = CampaignData.Load();
            if (data != null && data.levels != null)
            {
                foreach (CampaignLevel level in data.levels)
                {
                    PlayerPrefs.DeleteKey("Campaign_Level_" + level.id);
                }
            }
        }

        CampaignDataWrapper data2 = CampaignData.Load();
        if (data2 != null && data2.levels != null)
        {
            foreach (CampaignLevel level in data2.levels)
            {
                PlayerPrefs.DeleteKey("Ribbon_Level_" + level.id);
            }
        }

        InsigniaManager.ResetAll();
        ChestManager.ResetAll();

        if (data2 != null && data2.cups != null)
        {
            foreach (CampaignCup cup in data2.cups)
            {
                PlayerPrefs.DeleteKey("Unlocked_" + cup.race);
            }
        }

        foreach (string sp in species)
        {
            PlayerPrefs.DeleteKey("Unlocked_" + sp);
            PlayerPrefs.DeleteKey("Trophy_" + sp);
        }

        PlayerPrefs.Save();
        Debug.Log("CollectAll: all progress reset");
    }

    IEnumerator ShowDailyBonusDelayed()
    {
        yield return null;
        if (DailyBonusUI.Instance != null)
            DailyBonusUI.Instance.ShowIfAvailable();
    }

}
