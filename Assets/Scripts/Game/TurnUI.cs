using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TurnUI : MonoBehaviour
{
    [Header("References")]
    public TurnManager turnManager;

    private Text turnText;
    private Text timerText;
    private Text turnTimerText;
    private Button endTurnButton;
    private Image flagImage;
    private Image soundIcon;
    private Sprite blueFlagSprite;
    private Sprite redFlagSprite;
    private Sprite soundOnSprite;
    private Sprite soundOffSprite;
    private bool soundEnabled = true;
    private Image skipBtnImage;
    private BoardManager board;

    void Start()
    {
        board = FindFirstObjectByType<BoardManager>();
        blueFlagSprite = Resources.Load<Sprite>("Sprites/Decor/BlueFlag");
        redFlagSprite = Resources.Load<Sprite>("Sprites/Decor/RedFlag");
        soundOnSprite = Resources.Load<Sprite>("Sprites/Human/Decor/soundOn");
        soundOffSprite = Resources.Load<Sprite>("Sprites/Human/Decor/soundOff");
        CreateUI();
        if (turnManager != null)
            turnManager.OnTurnChanged += UpdateUI;

        UpdateUI(turnManager?.currentTurn ?? TurnState.BlueTurn);
        if (turnManager != null)
            turnManager.OnTurnTimerWarning += OnWarning;
    }

    Sprite CreateCircleSprite(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - (size - 1) * 0.5f;
                float dy = y - (size - 1) * 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                tex.SetPixel(x, y, dist <= size * 0.45f ? color : Color.clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
    }

    void OnSpeciesButtonClicked()
    {
        BoardManager board = FindFirstObjectByType<BoardManager>();
        if (board != null)
        {
            string[] species = { "Human", "Orc", "Beastfolk" };
            int idx = System.Array.IndexOf(species, board.speciesTheme);
            if (idx < 0) idx = 0;
            string next = species[(idx + 1) % species.Length];
            board.SwitchSpecies(next);
        }
    }

    void OnScenarioButtonClicked()
    {
        BoardManager board = FindFirstObjectByType<BoardManager>();
        if (board != null)
        {
            string[] scenarios = { "Human", "Orc", "Beastfolk" };
            int idx = System.Array.IndexOf(scenarios, board.scenarioTheme);
            if (idx < 0) idx = 0;
            string next = scenarios[(idx + 1) % scenarios.Length];
            board.SwitchScenario(next);
        }
    }

    void OnDestroy()
    {
        if (turnManager != null)
        {
            turnManager.OnTurnChanged -= UpdateUI;
            turnManager.OnTurnTimerWarning -= OnWarning;
        }
    }

    void CreateUI()
    {
        GameObject canvasObj = new GameObject("TurnCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        Sprite diceTableSprite = null;
        BoardManager board = FindFirstObjectByType<BoardManager>();
        if (board != null)
        {
            string themePath = $"Sprites/{board.scenarioTheme}/Decor/DiceTable";
            diceTableSprite = Resources.Load<Sprite>(themePath);
        }
        if (diceTableSprite == null)
            diceTableSprite = Resources.Load<Sprite>("Sprites/Common/Decor/DiceTable");
        if (diceTableSprite == null)
            diceTableSprite = Resources.Load<Sprite>("Sprites/Decor/DiceTable");

        GameObject infoPanel = new GameObject("InfoPanel");
        infoPanel.transform.SetParent(canvasObj.transform);
        Image panelImg = infoPanel.AddComponent<Image>();
        panelImg.color = new Color(0.82f, 0.78f, 0.70f, 0.6f);
        RectTransform panelRt = infoPanel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(1f, 0.5f);
        panelRt.anchorMax = new Vector2(1f, 0.5f);
        panelRt.pivot = new Vector2(1f, 0.5f);
        panelRt.sizeDelta = new Vector2(320, 400);
        panelRt.anchoredPosition = new Vector2(-82, 60);
        panelRt.localScale = new Vector3(0.64f, 0.64f, 1f);

        GameObject diceObj = new GameObject("DiceTablePanel");
        diceObj.transform.SetParent(canvasObj.transform);
        Image diceImg = diceObj.AddComponent<Image>();
        diceImg.sprite = diceTableSprite;
        diceImg.preserveAspect = true;
        RectTransform diceRt = diceObj.GetComponent<RectTransform>();
        diceRt.anchorMin = new Vector2(1f, 0.5f);
        diceRt.anchorMax = new Vector2(1f, 0.5f);
        diceRt.pivot = new Vector2(1f, 0.5f);
        diceRt.sizeDelta = new Vector2(300, 370);
        diceRt.anchoredPosition = new Vector2(-40, 60);

        GameObject flagObj = new GameObject("TurnFlag");
        flagObj.transform.SetParent(infoPanel.transform);
        flagImage = flagObj.AddComponent<Image>();
        flagImage.preserveAspect = true;
        RectTransform flagRt = flagObj.GetComponent<RectTransform>();
        flagRt.anchorMin = new Vector2(0.5f, 1f);
        flagRt.anchorMax = new Vector2(0.5f, 1f);
        flagRt.pivot = new Vector2(0.5f, 1f);
        flagRt.sizeDelta = new Vector2(62, 62);
        flagRt.anchoredPosition = new Vector2(0, -28);

        GameObject textObj = new GameObject("TurnText");
        textObj.transform.SetParent(infoPanel.transform);

        turnText = textObj.AddComponent<Text>();
        turnText.font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        turnText.fontSize = 48;
        turnText.alignment = TextAnchor.MiddleCenter;
        turnText.color = Color.white;

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0.5f, 0.5f);
        textRt.anchorMax = new Vector2(0.5f, 0.5f);
        textRt.pivot = new Vector2(0.5f, 0.5f);
        textRt.sizeDelta = new Vector2(300, 56);
        textRt.anchoredPosition = new Vector2(0, 30);

        GameObject timerObj = new GameObject("TimerText");
        timerObj.transform.SetParent(infoPanel.transform);
        timerText = timerObj.AddComponent<Text>();
        timerText.font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        timerText.fontSize = 34;
        timerText.alignment = TextAnchor.MiddleCenter;
        timerText.color = Color.white;
        RectTransform timerRt = timerObj.GetComponent<RectTransform>();
        timerRt.anchorMin = new Vector2(0.5f, 0.5f);
        timerRt.anchorMax = new Vector2(0.5f, 0.5f);
        timerRt.pivot = new Vector2(0.5f, 0.5f);
        timerRt.sizeDelta = new Vector2(240, 44);
        timerRt.anchoredPosition = new Vector2(0, -40);
        if (GameConfig.isTutorial) timerObj.SetActive(false);

        GameObject turnTimerObj = new GameObject("TurnTimerText");
        turnTimerObj.transform.SetParent(infoPanel.transform);
        turnTimerText = turnTimerObj.AddComponent<Text>();
        turnTimerText.font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        turnTimerText.fontSize = 25;
        turnTimerText.alignment = TextAnchor.MiddleCenter;
        turnTimerText.color = Color.white;
        RectTransform turnTimerRt = turnTimerObj.GetComponent<RectTransform>();
        turnTimerRt.anchorMin = new Vector2(0.5f, 0f);
        turnTimerRt.anchorMax = new Vector2(0.5f, 0f);
        turnTimerRt.pivot = new Vector2(0.5f, 0f);
        turnTimerRt.sizeDelta = new Vector2(150, 34);
        turnTimerRt.anchoredPosition = new Vector2(0, 24);
        if (GameConfig.isTutorial) turnTimerObj.SetActive(false);

        Sprite skipSprite = null;
        if (board != null)
        {
            string skipPath = $"Sprites/{board.scenarioTheme}/Decor/SkipTurn";
            skipSprite = Resources.Load<Sprite>(skipPath);
        }
        if (skipSprite == null)
            skipSprite = Resources.Load<Sprite>("Sprites/Human/Decor/SkipTurn");

        GameObject btnObj = new GameObject("SkipTurnButton");
        btnObj.transform.SetParent(canvasObj.transform);

        skipBtnImage = btnObj.AddComponent<Image>();
        skipBtnImage.sprite = skipSprite;
        skipBtnImage.preserveAspect = true;
        if (skipSprite == null)
            skipBtnImage.color = new Color(0.4f, 0.3f, 0.15f, 0.85f);

        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0f, 1f);
        btnRt.anchorMax = new Vector2(0f, 1f);
        btnRt.pivot = new Vector2(0f, 1f);
        btnRt.sizeDelta = new Vector2(600, 235);
        btnRt.anchoredPosition = new Vector2(1555, -640);
        btnRt.localScale = Vector3.one;

        endTurnButton = btnObj.AddComponent<Button>();
        endTurnButton.targetGraphic = skipBtnImage;
        endTurnButton.onClick.AddListener(OnEndTurnClicked);
        endTurnButton.onClick.AddListener(() => SoundManager.Instance.PlayButton());
        if (GameConfig.isTutorial) endTurnButton.gameObject.SetActive(false);

        CreateSoundToggle(canvasObj.transform);
        CreateQuitButton(canvasObj.transform);
        if (!GameConfig.isTutorial)
        {
            CreateScenarioButton(canvasObj.transform);
            CreateSpeciesButton(canvasObj.transform);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            PlayerPrefs.DeleteAll();
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        if (timerText != null && TimerManager.Instance.isRunning)
        {
            timerText.text = TimerManager.Instance.GetTimeString();
            if (TimerManager.Instance.timeRemaining <= 60f)
                timerText.color = Color.Lerp(Color.red, Color.white, Mathf.PingPong(Time.time * 2f, 1f));
            else
        timerText.color = new Color(1f, 0.9f, 0.4f);
        }

        if (turnTimerText != null && turnManager != null)
        {
            if (turnManager.currentTurn == TurnState.BlueTurn && turnManager.timerRunning)
            {
                int secs = Mathf.CeilToInt(turnManager.turnTimeRemaining);
                turnTimerText.text = $"{secs}s";
                turnTimerText.gameObject.SetActive(true);
                if (secs <= 5)
                    turnTimerText.color = Color.Lerp(Color.red, Color.white, Mathf.PingPong(Time.time * 4f, 1f));
                else
                    turnTimerText.color = Color.white;
            }
            else
            {
                turnTimerText.gameObject.SetActive(false);
            }
        }
    }

    void CreateSoundToggle(Transform parent)
    {
        GameObject btnObj = new GameObject("SoundToggle");
        btnObj.transform.SetParent(parent);

        soundIcon = btnObj.AddComponent<Image>();
        soundIcon.sprite = soundOnSprite;

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.sizeDelta = new Vector2(44, 44);
        rt.anchoredPosition = new Vector2(-10, -10);

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = soundIcon;
        btn.onClick.AddListener(ToggleSound);
        btn.onClick.AddListener(() => SoundManager.Instance.PlayButton());
    }

    void CreateQuitButton(Transform parent)
    {
        bool isWeb = Application.platform == RuntimePlatform.WebGLPlayer;
        GameObject btnObj = new GameObject("QuitButton");
        btnObj.transform.SetParent(parent);

        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.6f, 0.2f, 0.2f, 0.9f);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.sizeDelta = new Vector2(100, 36);
        rt.anchoredPosition = new Vector2(-10, -60);

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImage;
        btn.onClick.AddListener(() => Application.Quit());
        btn.onClick.AddListener(() => SoundManager.Instance.PlayButton());

        btnObj.SetActive(!isWeb);
    }

    void ToggleSound()
    {
        soundEnabled = !soundEnabled;
        AudioListener.volume = soundEnabled ? 1f : 0f;
        if (soundIcon != null)
            soundIcon.sprite = soundEnabled ? soundOnSprite : soundOffSprite;
    }

    void CreateScenarioButton(Transform parent)
    {
        GameObject btnObj = new GameObject("ScenarioButton");
        btnObj.transform.SetParent(parent);

        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.3f, 0.4f, 0.6f, 0.9f);

        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(1f, 0f);
        btnRt.anchorMax = new Vector2(1f, 0f);
        btnRt.pivot = new Vector2(1f, 0f);
        btnRt.sizeDelta = new Vector2(140, 40);
        btnRt.anchoredPosition = new Vector2(-10, 10);

        Button button = btnObj.AddComponent<Button>();
        button.targetGraphic = btnImage;
        button.onClick.AddListener(OnScenarioButtonClicked);
        button.onClick.AddListener(() => SoundManager.Instance.PlayButton());
    }

    void CreateSpeciesButton(Transform parent)
    {
        GameObject btnObj = new GameObject("SpeciesButton");
        btnObj.transform.SetParent(parent);

        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.3f, 0.6f, 0.3f, 0.9f);

        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(1f, 0f);
        btnRt.anchorMax = new Vector2(1f, 0f);
        btnRt.pivot = new Vector2(1f, 0f);
        btnRt.sizeDelta = new Vector2(140, 40);
        btnRt.anchoredPosition = new Vector2(-10, 60);

        GameObject btnTextObj = new GameObject("BtnText");
        btnTextObj.transform.SetParent(btnObj.transform);

        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        btnText.fontSize = 18;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.text = "ESPECIE";
        btnText.color = Color.white;

        RectTransform btnTextRt = btnTextObj.GetComponent<RectTransform>();
        btnTextRt.anchorMin = Vector2.zero;
        btnTextRt.anchorMax = Vector2.one;
        btnTextRt.sizeDelta = Vector2.zero;

        Button button = btnObj.AddComponent<Button>();
        button.targetGraphic = btnImage;
        button.onClick.AddListener(OnSpeciesButtonClicked);
        button.onClick.AddListener(() => SoundManager.Instance.PlayButton());
    }

    void OnEndTurnClicked()
    {
        if (turnManager != null)
            turnManager.EndTurn();
    }

    void OnWarning()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySelect();
    }

    void UpdateUI(TurnState state)
    {
        if (turnText == null || flagImage == null) return;

        bool forceBlue = GameConfig.isTutorial && board != null && !board.isShadowPhase;
        if (forceBlue || state == TurnState.BlueTurn)
        {
            turnText.text = "BLUE TURN";
            turnText.color = new Color(0.4f, 0.6f, 1f);
            if (blueFlagSprite != null) flagImage.sprite = blueFlagSprite;
            if (turnManager != null && !GameConfig.isTutorial) turnManager.ResetTurnTimer();
        }
        else
        {
            turnText.text = "RED TURN";
            turnText.color = new Color(1f, 0.4f, 0.4f);
            if (redFlagSprite != null) flagImage.sprite = redFlagSprite;
        }

        if (endTurnButton != null)
            endTurnButton.gameObject.SetActive(state == TurnState.BlueTurn && !GameConfig.isTutorial);
    }
}
