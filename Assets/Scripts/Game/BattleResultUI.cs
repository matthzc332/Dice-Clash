using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleResultUI : MonoBehaviour
{
    private static BattleResultUI _instance;
    public static BattleResultUI Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("BattleResultUI");
                _instance = obj.AddComponent<BattleResultUI>();
            }
            return _instance;
        }
    }

    public bool IsClosed { get; private set; }
    private bool winnerClicked;

    private Sprite[] diceSprites;
    private Sprite[] cupSprites;
    private Sprite panelDadosSprite, panelVictoriaSprite, panelVictoriaFlagSprite;
    private Sprite blueFlagSprite, redFlagSprite;

    private GameObject panelObj, victoriaObj;
    private Image cupImage;
    private Image atkDiceImage, atkDiceImage2, defDiceImage, defDiceImage2;
    private Text atkAbilityText, defAbilityText;
    private Image atkSpriteImage, defSpriteImage;
    private Text atkNameText, defNameText;
    private Button winnerButton;
    private Image winnerFlagImage;
    private Text winnerNameText;
    private Image flashOverlay;
    private Image atkIconImage, defIconImage;
    private Text atkStatText, defStatText;
    private Text blueResultText, redResultText;

    private Color blueColor = new Color(0.4f, 0.6f, 1f);
    private Color redColor = new Color(1f, 0.4f, 0.4f);

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        LoadSprites();
        CreateUI();
    }

    void LoadSprites()
    {
        diceSprites = new Sprite[6];
        for (int i = 0; i < 6; i++)
        {
            Sprite[] loaded = Resources.LoadAll<Sprite>($"Sprites/Dice/Dado{i + 1}");
            diceSprites[i] = loaded.Length > 0 ? loaded[0] : null;
        }

        cupSprites = new Sprite[7];
        for (int i = 0; i < 7; i++)
        {
            Sprite[] loaded = Resources.LoadAll<Sprite>($"Sprites/Dice/vaso{i + 1}");
            cupSprites[i] = loaded.Length > 0 ? loaded[0] : null;
        }
        bool hasAny = false;
        foreach (var s in cupSprites) if (s != null) { hasAny = true; break; }
        if (!hasAny)
        {
            for (int i = 0; i < 7; i++)
                cupSprites[i] = CreateCircleSprite(32, new Color(0.8f, 0.6f, 0.3f));
        }
        else
        {
            for (int i = 0; i < 7; i++)
            {
                if (cupSprites[i] != null)
                    cupSprites[i] = Sprite.Create(cupSprites[i].texture, cupSprites[i].rect, new Vector2(0.5f, 0.5f), cupSprites[i].pixelsPerUnit);
            }
        }

        panelDadosSprite = LoadFirstSprite("Sprites/Menu/panelDados");
        panelVictoriaSprite = LoadFirstSprite("Sprites/Menu/panelVictoria");
        panelVictoriaFlagSprite = LoadSpriteByName("Sprites/Menu/panelVictoria", "panelVictoria_1");
        blueFlagSprite = Resources.Load<Sprite>("Sprites/Decor/BlueFlag");
        redFlagSprite = Resources.Load<Sprite>("Sprites/Decor/RedFlag");
    }

    void CreateUI()
    {
        GameObject canvasObj = new GameObject("BattleCanvas");
        canvasObj.transform.SetParent(transform);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 88;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        canvasObj.AddComponent<CanvasGroup>();

        // --- Combat panel with panelDados ---
        panelObj = new GameObject("BattlePanel");
        panelObj.transform.SetParent(canvasObj.transform);

        Image panelImage = panelObj.AddComponent<Image>();
        if (panelDadosSprite != null)
        {
            panelImage.sprite = panelDadosSprite;
            panelImage.preserveAspect = true;
        }
        panelImage.raycastTarget = false;
        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(960, 480);
        panelRt.anchoredPosition = new Vector2(0, -20);

        // --- Attacker side (left half) ---
        atkSpriteImage = MakeImage(panelObj, "AtkSprite", new Vector2(-240, 227), new Vector2(200, 200));
        atkNameText = MakeLabel(panelObj, "AtkName", "", 16, blueColor, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-240, 95), new Vector2(300, 24));

        atkIconImage = MakeImage(panelObj, "AtkIcon", new Vector2(-380, -80), new Vector2(32, 32));

        atkDiceImage = MakeImage(panelObj, "AtkDice1", new Vector2(-275, -20), new Vector2(72, 72));
        atkDiceImage2 = MakeImage(panelObj, "AtkDice2", new Vector2(-205, -20), new Vector2(72, 72));
        atkDiceImage.rectTransform.sizeDelta = new Vector2(72, 72);
        atkDiceImage2.rectTransform.sizeDelta = new Vector2(72, 72);

        atkStatText = MakeLabel(panelObj, "AtkStat", "ATK 2d6+1", 14, Color.white, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-240, -100), new Vector2(300, 22));

        atkAbilityText = MakeLabel(panelObj, "AtkAbility", "", 13, Color.yellow, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-240, -130), new Vector2(300, 20));

        // --- Defender side (right half) ---
        defSpriteImage = MakeImage(panelObj, "DefSprite", new Vector2(240, 227), new Vector2(200, 200));
        defNameText = MakeLabel(panelObj, "DefName", "", 16, redColor, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(240, 95), new Vector2(300, 24));

        defIconImage = MakeImage(panelObj, "DefIcon", new Vector2(380, -80), new Vector2(32, 32));

        defDiceImage = MakeImage(panelObj, "DefDice1", new Vector2(205, -20), new Vector2(72, 72));
        defDiceImage2 = MakeImage(panelObj, "DefDice2", new Vector2(275, -20), new Vector2(72, 72));
        defDiceImage.rectTransform.sizeDelta = new Vector2(72, 72);
        defDiceImage2.rectTransform.sizeDelta = new Vector2(72, 72);

        defStatText = MakeLabel(panelObj, "DefStat", "DEF 2d6+0", 14, Color.white, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(240, -100), new Vector2(300, 22));

        defAbilityText = MakeLabel(panelObj, "DefAbility", "", 13, Color.yellow, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(240, -130), new Vector2(300, 20));

        // --- Team result texts ---
        blueResultText = MakeLabel(panelObj, "BlueResult", "", 14, Color.white, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-251, -199), new Vector2(100, 22));
        redResultText = MakeLabel(panelObj, "RedResult", "", 14, Color.white, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(237, -187), new Vector2(100, 22));

        // --- Flash overlay ---
        GameObject flashObj = new GameObject("FlashOverlay");
        flashObj.transform.SetParent(panelObj.transform);
        Image flashImg = flashObj.AddComponent<Image>();
        flashImg.color = new Color(1, 1, 1, 0);
        RectTransform flashRt = flashObj.GetComponent<RectTransform>();
        flashRt.anchorMin = Vector2.zero;
        flashRt.anchorMax = Vector2.one;
        flashRt.offsetMin = Vector2.zero;
        flashRt.offsetMax = Vector2.zero;
        flashOverlay = flashImg;

        // --- Dice cup (hidden initially) ---
        cupImage = MakeImage(panelObj, "CupImage", new Vector2(0, 45), new Vector2(170, 210));
        cupImage.gameObject.SetActive(false);

        // --- Victoria banner (hidden initially) ---
        victoriaObj = new GameObject("VictoriaPanel");
        victoriaObj.transform.SetParent(canvasObj.transform);

        Image victoriaImage = victoriaObj.AddComponent<Image>();
        if (panelVictoriaSprite != null)
        {
            victoriaImage.sprite = panelVictoriaSprite;
            victoriaImage.preserveAspect = true;
        }
        RectTransform victoriaRt = victoriaObj.GetComponent<RectTransform>();
        victoriaRt.anchorMin = new Vector2(0.5f, 0.5f);
        victoriaRt.anchorMax = new Vector2(0.5f, 0.5f);
        victoriaRt.pivot = new Vector2(0.5f, 0.5f);
        victoriaRt.sizeDelta = new Vector2(900, 155);
        victoriaRt.anchoredPosition = new Vector2(0, -343);

        winnerButton = victoriaObj.AddComponent<Button>();
        winnerButton.targetGraphic = victoriaImage;
        winnerButton.onClick.AddListener(() => { SoundManager.Instance.PlayButton(); winnerClicked = true; });

        winnerFlagImage = MakeImage(victoriaObj, "WinnerFlag", new Vector2(-200, 0), new Vector2(40, 40));

        winnerNameText = MakeLabel(victoriaObj, "WinnerName", "", 22, Color.white, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(500, 36));

        victoriaObj.SetActive(false);

        gameObject.SetActive(false);
    }

    Image MakeImage(GameObject parent, string name, Vector2 pos, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform);
        Image img = obj.AddComponent<Image>();
        img.preserveAspect = true;
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        return img;
    }

    Text MakeLabel(GameObject parent, string name, string text, int size, Color color, TextAnchor anchor, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 sizeDelta)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform);

        Text label = obj.AddComponent<Text>();
        label.font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        label.fontSize = size;
        label.alignment = anchor;
        label.color = color;
        label.text = text;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = pos;

        return label;
    }

    Sprite LoadFirstSprite(string path)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);
        return sprites.Length > 0 ? sprites[0] : null;
    }

    Sprite LoadSpriteByName(string path, string name)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);
        if (sprites == null || sprites.Length == 0) return null;
        return System.Array.Find(sprites, s => s.name == name);
    }

    void LoadCardSprite(Image spriteImage, PieceType type, Team team)
    {
        string name = type switch
        {
            PieceType.Pawn => "Peon",
            PieceType.Ninja => "Ninja",
            PieceType.Knight => "Caballero",
            PieceType.Paladin => "Paladin",
            _ => "Peon"
        };
        Sprite[] sprites = Resources.LoadAll<Sprite>($"Sprites/Card/{name}Carta");
        if (sprites.Length > 0)
        {
            spriteImage.sprite = sprites[0];
            spriteImage.preserveAspect = true;
        }
        spriteImage.color = team == Team.Red ? new Color(0.7f, 0.7f, 0.7f) : Color.white;
    }

    private List<GameObject> uiParticles = new();

    public void ShowResult(CombatOutcome outcome, PieceData attacker, PieceData defender)
    {
        StopAllCoroutines();
        IsClosed = false;
        ClearUIParticles();
        winnerClicked = false;
        gameObject.SetActive(true);
        panelObj.transform.localScale = Vector3.one;
        victoriaObj.transform.localScale = Vector3.one;
        victoriaObj.SetActive(false);
        if (cupImage != null) cupImage.gameObject.SetActive(false);

        bool blueAttacks = attacker.team == Team.Blue;
        PieceData bluePiece = blueAttacks ? attacker : defender;
        PieceData redPiece = blueAttacks ? defender : attacker;

        LoadCardSprite(atkSpriteImage, bluePiece.type, bluePiece.team);
        LoadCardSprite(atkIconImage, bluePiece.type, bluePiece.team);
        atkNameText.text = $"{bluePiece.type}";
        atkNameText.color = blueColor;

        LoadCardSprite(defSpriteImage, redPiece.type, redPiece.team);
        LoadCardSprite(defIconImage, redPiece.type, redPiece.team);
        defNameText.text = $"{redPiece.type}";
        defNameText.color = redColor;

        if (blueAttacks)
        {
            atkStatText.text = "ATK 2d6+1";
            defStatText.text = $"DEF 2d6+{redPiece.defBonus}";
        }
        else
        {
            atkStatText.text = $"DEF 2d6+{bluePiece.defBonus}";
            defStatText.text = "ATK 2d6+1";
        }

        atkAbilityText.text = "";
        defAbilityText.text = "";

        atkDiceImage.color = Color.white;
        atkDiceImage2.color = Color.white;
        defDiceImage.color = new Color(0.7f, 0.7f, 0.7f);
        defDiceImage2.color = new Color(0.7f, 0.7f, 0.7f);
        atkDiceImage.gameObject.SetActive(false);
        atkDiceImage2.gameObject.SetActive(false);
        defDiceImage.gameObject.SetActive(false);
        defDiceImage2.gameObject.SetActive(false);

        victoriaObj.SetActive(false);

        blueResultText.text = "";
        redResultText.text = "";

        StartCoroutine(AnimateEntry(outcome, attacker, defender));
    }

    IEnumerator AnimateEntry(CombatOutcome outcome, PieceData attacker, PieceData defender)
    {
        float slideDuration = 0.3f;
        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        Vector2 targetPos = panelRt.anchoredPosition;
        panelRt.anchoredPosition = new Vector2(targetPos.x, -600);
        float t = 0;
        while (t < slideDuration)
        {
            float raw = t / slideDuration;
            float eased = raw * raw * (3f - 2f * raw);
            panelRt.anchoredPosition = Vector2.Lerp(new Vector2(targetPos.x, -600), targetPos, eased);
            t += Time.deltaTime;
            yield return null;
        }
        panelRt.anchoredPosition = targetPos;

        yield return StartCoroutine(AnimateCupShake());
        yield return StartCoroutine(AnimateDiceClash(outcome, attacker, defender));

        yield return new WaitForSeconds(0.3f);

        bool atkWin = outcome.result == CombatResult.AttackerWins;
        if (atkWin)
            StartCoroutine(SpawnUIConfetti());
        else
            StartCoroutine(SpawnUISmoke());

        SoundManager.Instance.PlayVictory();

        // Show victoria banner
        PieceData winner = atkWin ? attacker : defender;
        if (winner.team == Team.Blue)
            SoundManager.Instance.PlayTrumpet();
        winnerNameText.text = $"WINNER: {winner.type}";
        winnerNameText.color = winner.team == Team.Blue ? Color.white : Color.black;
        winnerFlagImage.sprite = winner.team == Team.Blue ? blueFlagSprite : redFlagSprite;

        victoriaObj.SetActive(true);
        RectTransform victoriaRt = victoriaObj.GetComponent<RectTransform>();
        Vector2 vTarget = victoriaRt.anchoredPosition;
        victoriaRt.anchoredPosition = new Vector2(vTarget.x, -600);
        float vt = 0;
        float vDur = 0.3f;
        while (vt < vDur)
        {
            float raw = vt / vDur;
            float eased = raw * raw * (3f - 2f * raw);
            victoriaRt.anchoredPosition = Vector2.Lerp(new Vector2(vTarget.x, -600), vTarget, eased);
            vt += Time.deltaTime;
            yield return null;
        }
        victoriaRt.anchoredPosition = vTarget;
        Coroutine pulse = StartCoroutine(PulseVictoria());

        // Wait for click
        if (GameConfig.isAutoPlay)
        {
            yield return new WaitForSeconds(0.15f);
            winnerClicked = true;
        }
        else
        {
            while (!winnerClicked) yield return null;
        }
        if (pulse != null) StopCoroutine(pulse);

        SoundManager.Instance.PlaySelect();

        float fadeDuration = 0.2f;
        float ft = 0;
        while (ft < fadeDuration)
        {
            panelObj.transform.localScale = Vector3.one * (1f - ft / fadeDuration);
            victoriaObj.transform.localScale = Vector3.one * (1f - ft / fadeDuration);
            ft += Time.deltaTime;
            yield return null;
        }

        ClearUI();
    }

    IEnumerator PulseVictoria()
    {
        if (victoriaObj == null) yield break;
        RectTransform rt = victoriaObj.GetComponent<RectTransform>();
        float duration = 0.6f;
        while (true)
        {
            float t = 0;
            while (t < duration)
            {
                float pulse = 1f + Mathf.Sin(t / duration * Mathf.PI * 2f) * 0.05f;
                rt.localScale = Vector3.one * pulse;
                t += Time.deltaTime;
                yield return null;
            }
        }
    }

    IEnumerator AnimateCupShake()
    {
        if (cupSprites == null || cupSprites.Length == 0) yield break;
        if (cupImage == null) yield break;

        cupImage.gameObject.SetActive(true);
        cupImage.sprite = cupSprites[0];
        cupImage.color = Color.white;
        cupImage.rectTransform.localRotation = Quaternion.identity;
        cupImage.rectTransform.anchoredPosition = new Vector2(0, 45);
        RectTransform cupRt = cupImage.rectTransform;
        Vector2 startPos = cupRt.anchoredPosition;
        Camera cam = Camera.main;

        // Start dice roll sound immediately with cup
        SoundManager.Instance.PlayDiceRoll();

        // Slide cup down from above
        cupRt.anchoredPosition = new Vector2(startPos.x, 350);
        float revealDuration = 0.25f;
        float rt = 0;
        while (rt < revealDuration)
        {
            float raw = rt / revealDuration;
            float eased = raw * raw * (3f - 2f * raw);
            cupRt.anchoredPosition = Vector2.Lerp(new Vector2(startPos.x, 350), startPos, eased);
            rt += Time.deltaTime;
            yield return null;
        }
        cupRt.anchoredPosition = startPos;

        SoundManager.Instance.PlayCupShake();

        // Shake animation with frame cycling
        float shakeDuration = 0.9f;
        float st = 0;
        int frameIndex = 0;
        float frameTimer = 0f;
        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        Vector2 panelOrigPos = panelRt.anchoredPosition;

        while (st < shakeDuration)
        {
            frameTimer += Time.deltaTime;
            float frameRate = Mathf.Lerp(0.08f, 0.12f, Mathf.Sin(st * 3f) * 0.5f + 0.5f);
            if (frameTimer >= frameRate)
            {
                frameTimer = 0;
                frameIndex = (frameIndex + 1) % cupSprites.Length;
                // Skip null sprites to avoid disappearance
                if (cupSprites[frameIndex] != null)
                    cupImage.sprite = cupSprites[frameIndex];
            }

            float shakeX = Mathf.Sin(st * 35f) * 5f;
            float shakeY = Mathf.Sin(st * 28f) * 2.5f;
            float rotation = Mathf.Sin(st * 22f) * 4f;
            cupRt.anchoredPosition = startPos + new Vector2(shakeX, shakeY);
            cupRt.localRotation = Quaternion.Euler(0, 0, rotation);

            panelRt.anchoredPosition = panelOrigPos + Random.insideUnitCircle * 1.5f;

            st += Time.deltaTime;
            yield return null;
        }
        panelRt.anchoredPosition = panelOrigPos;

        SoundManager.Instance.PlayCupPour();

        // Explosive pour — cup tilts and moves up, dice drops
        float pourDuration = 0.25f;
        float pt = 0;
        Vector3 camOrig = cam != null ? cam.transform.position : Vector3.zero;
        while (pt < pourDuration)
        {
            float raw = pt / pourDuration;
            cupRt.anchoredPosition = startPos + new Vector2(0, -raw * 30f);
            cupRt.localRotation = Quaternion.Euler(0, 0, -raw * 50f);
            cupImage.color = new Color(1, 1, 1, 1f - raw * 0.6f);

            if (cam != null)
                cam.transform.position = camOrig + (Vector3)Random.insideUnitCircle * 0.05f;

            // Spawn sparkle particles during pour
            if (Random.value < 0.5f)
            {
                float sx = Random.Range(-1f, 1f);
                float sy = Random.Range(-1f, 1f);
                Vector3 sparklePos = new Vector3(sx, sy, 0) * 30f;
                GameObject spark = new GameObject("CupSparkle");
                spark.transform.SetParent(panelObj.transform);
                Image sparkImg = spark.AddComponent<Image>();
                sparkImg.sprite = CreateCircleSprite(8, new Color(1f, 0.9f, 0.3f, 1f));
                RectTransform sparkRt = spark.GetComponent<RectTransform>();
                sparkRt.anchorMin = new Vector2(0.5f, 0.5f);
                sparkRt.anchorMax = new Vector2(0.5f, 0.5f);
                sparkRt.pivot = new Vector2(0.5f, 0.5f);
                sparkRt.sizeDelta = new Vector2(Random.Range(6, 14), Random.Range(6, 14));
                sparkRt.anchoredPosition = startPos + new Vector2(sx * 40f, sy * 40f - raw * 50f);
                Color sparkCol = new Color(1f, 0.8f, 0.2f, 1f);
                sparkImg.color = sparkCol;
                uiParticles.Add(spark);
                Vector2 sparkVel = new Vector2(sx * 60f, sy * 60f + 30f);
                StartCoroutine(AnimateUIParticle(spark, sparkVel, 0.3f));
            }

            pt += Time.deltaTime;
            yield return null;
        }

        if (cam != null) cam.transform.position = camOrig;

        cupImage.gameObject.SetActive(false);
        cupImage.color = Color.white;
        cupRt.localRotation = Quaternion.identity;
    }

    IEnumerator AnimateDiceClash(CombatOutcome outcome, PieceData attacker, PieceData defender)
    {
        atkDiceImage.gameObject.SetActive(true);
        atkDiceImage2.gameObject.SetActive(true);
        defDiceImage.gameObject.SetActive(true);
        defDiceImage2.gameObject.SetActive(true);

        RectTransform atkRt = atkDiceImage.rectTransform;
        RectTransform atkRt2 = atkDiceImage2.rectTransform;
        RectTransform defRt = defDiceImage.rectTransform;
        RectTransform defRt2 = defDiceImage2.rectTransform;

        Vector2 atkStart = atkRt.anchoredPosition;
        Vector2 atkStart2 = atkRt2.anchoredPosition;
        Vector2 defStart = defRt.anchoredPosition;
        Vector2 defStart2 = defRt2.anchoredPosition;
        Vector2 centerPos = new Vector2(0, -20);

        // Move dice to center
        float moveDuration = 0.5f;
        float t = 0;
        while (t < moveDuration)
        {
            float raw = t / moveDuration;
            float eased = raw * raw * (3f - 2f * raw);
            atkRt.anchoredPosition = Vector2.Lerp(atkStart, centerPos + new Vector2(-80, 0), eased);
            atkRt2.anchoredPosition = Vector2.Lerp(atkStart2, centerPos + new Vector2(-30, 0), eased);
            defRt.anchoredPosition = Vector2.Lerp(defStart, centerPos + new Vector2(30, 0), eased);
            defRt2.anchoredPosition = Vector2.Lerp(defStart2, centerPos + new Vector2(80, 0), eased);
            atkDiceImage.sprite = diceSprites[Random.Range(0, 6)];
            atkDiceImage2.sprite = diceSprites[Random.Range(0, 6)];
            defDiceImage.sprite = diceSprites[Random.Range(0, 6)];
            defDiceImage2.sprite = diceSprites[Random.Range(0, 6)];
            t += Time.deltaTime;
            yield return null;
        }

        // Collision
        SoundManager.Instance.PlayHit();
        StartCoroutine(ShakePanel(0.3f, 15f));
        yield return StartCoroutine(FlashOverlay(0.15f));

        // Spin after collision
        float spinDuration = 1.2f;
        float st = 0;
        while (st < spinDuration)
        {
            float speed = Mathf.Lerp(800f, 100f, st / spinDuration);
            atkRt.Rotate(0, 0, speed * Time.deltaTime);
            atkRt2.Rotate(0, 0, speed * Time.deltaTime);
            defRt.Rotate(0, 0, -speed * Time.deltaTime);
            defRt2.Rotate(0, 0, -speed * Time.deltaTime);
            if (Random.value < 0.1f)
            {
                atkDiceImage.sprite = diceSprites[Random.Range(0, 6)];
                atkDiceImage2.sprite = diceSprites[Random.Range(0, 6)];
                defDiceImage.sprite = diceSprites[Random.Range(0, 6)];
                defDiceImage2.sprite = diceSprites[Random.Range(0, 6)];
            }
            st += Time.deltaTime;
            yield return null;
        }

        // Final dice sprites (left = blue, right = red)
        bool blueAttacks = attacker.team == Team.Blue;
        int blueDie1 = blueAttacks ? outcome.atkDie1 : outcome.defDie1;
        int blueDie2 = blueAttacks ? outcome.atkDie2 : outcome.defDie2;
        int redDie1 = blueAttacks ? outcome.defDie1 : outcome.atkDie1;
        int redDie2 = blueAttacks ? outcome.defDie2 : outcome.atkDie2;
        atkDiceImage.sprite = diceSprites[Mathf.Clamp(blueDie1 - 1, 0, 5)];
        atkDiceImage2.sprite = diceSprites[Mathf.Clamp(blueDie2 - 1, 0, 5)];
        defDiceImage.sprite = diceSprites[Mathf.Clamp(redDie1 - 1, 0, 5)];
        defDiceImage2.sprite = diceSprites[Mathf.Clamp(redDie2 - 1, 0, 5)];
        atkRt.localRotation = Quaternion.identity;
        atkRt2.localRotation = Quaternion.identity;
        defRt.localRotation = Quaternion.identity;
        defRt2.localRotation = Quaternion.identity;

        // Return dice to original positions
        float returnDuration = 0.3f;
        float rt2 = 0;
        while (rt2 < returnDuration)
        {
            float raw = rt2 / returnDuration;
            float eased = raw * raw * (3f - 2f * raw);
            atkRt.anchoredPosition = Vector2.Lerp(centerPos + new Vector2(-80, 0), atkStart, eased);
            atkRt2.anchoredPosition = Vector2.Lerp(centerPos + new Vector2(-30, 0), atkStart2, eased);
            defRt.anchoredPosition = Vector2.Lerp(centerPos + new Vector2(30, 0), defStart, eased);
            defRt2.anchoredPosition = Vector2.Lerp(centerPos + new Vector2(80, 0), defStart2, eased);
            rt2 += Time.deltaTime;
            yield return null;
        }

        // Show ability texts (blue left, red right)
        bool atkWin = outcome.result == CombatResult.AttackerWins;
        if (blueAttacks)
        {
            atkAbilityText.text = outcome.atkAbilityName != null ? $"> {outcome.atkAbilityName} +{outcome.atkBonus}" : "";
            atkAbilityText.color = atkWin ? Color.yellow : Color.gray;
            defAbilityText.text = outcome.defAbilityName != null ? $"> {outcome.defAbilityName} +{outcome.defBonus}" : "";
            defAbilityText.color = !atkWin ? Color.yellow : Color.gray;
        }
        else
        {
            atkAbilityText.text = outcome.defAbilityName != null ? $"> {outcome.defAbilityName} +{outcome.defBonus}" : "";
            atkAbilityText.color = !atkWin ? Color.yellow : Color.gray;
            defAbilityText.text = outcome.atkAbilityName != null ? $"> {outcome.atkAbilityName} +{outcome.atkBonus}" : "";
            defAbilityText.color = atkWin ? Color.yellow : Color.gray;
        }

        // Show result text (blue left, red right)
        if (blueAttacks)
        {
            blueResultText.text = $"ATK {outcome.atkTotal}";
            redResultText.text = $"DEF {outcome.defTotal}";
        }
        else
        {
            blueResultText.text = $"DEF {outcome.defTotal}";
            redResultText.text = $"ATK {outcome.atkTotal}";
        }
    }

    IEnumerator ShakePanel(float duration, float intensity)
    {
        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        Vector2 origPos = panelRt.anchoredPosition;
        float t = 0;
        while (t < duration)
        {
            Vector2 offset = Random.insideUnitCircle * intensity;
            panelRt.anchoredPosition = origPos + offset;
            t += Time.deltaTime;
            yield return null;
        }
        panelRt.anchoredPosition = origPos;
    }

    IEnumerator FlashOverlay(float duration)
    {
        flashOverlay.color = new Color(1, 1, 1, 0.6f);
        float t = 0;
        while (t < duration)
        {
            flashOverlay.color = new Color(1, 1, 1, Mathf.Lerp(0.6f, 0, t / duration));
            t += Time.deltaTime;
            yield return null;
        }
        flashOverlay.color = new Color(1, 1, 1, 0);
    }

    Sprite CreateCircleSprite(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
        {
            int x = i % size;
            int y = i / size;
            float dx = (x + 0.5f) / size - 0.5f;
            float dy = (y + 0.5f) / size - 0.5f;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            pixels[i] = dist < 0.4f ? color : Color.Lerp(color, Color.clear, (dist - 0.4f) * 10f);
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
    }

    IEnumerator SpawnUIConfetti()
    {
        System.Random rng = new System.Random();
        for (int i = 0; i < 30; i++)
        {
            Color col = new Color((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble(), 1f);
            GameObject dot = new GameObject("UIConfetti");
            dot.transform.SetParent(panelObj.transform);
            Image img = dot.AddComponent<Image>();
            img.sprite = CreateCircleSprite(12, col);
            img.color = col;
            RectTransform rt = dot.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(Random.Range(8, 16), Random.Range(8, 16));
            rt.anchoredPosition = new Vector2(Random.Range(-400, 400), Random.Range(-200, 200));
            Vector2 vel = new Vector2((float)(rng.NextDouble() - 0.5f) * 150f, Random.Range(50f, 150f));
            float life = Random.Range(1f, 2f);
            uiParticles.Add(dot);
            StartCoroutine(AnimateUIParticle(dot, vel, life));
        }
        yield break;
    }

    IEnumerator SpawnUISmoke()
    {
        System.Random rng = new System.Random();
        for (int i = 0; i < 12; i++)
        {
            Color col = new Color(0.7f, 0.7f, 0.7f, 0.8f);
            GameObject dot = new GameObject("UISmoke");
            dot.transform.SetParent(panelObj.transform);
            Image img = dot.AddComponent<Image>();
            img.sprite = CreateCircleSprite(16, col);
            img.color = col;
            RectTransform rt = dot.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(Random.Range(20, 40), Random.Range(20, 40));
            rt.anchoredPosition = new Vector2(Random.Range(-350, 350), Random.Range(-250, 100));
            Vector2 vel = new Vector2((float)(rng.NextDouble() - 0.5f) * 120f, Random.Range(60f, 180f));
            float life = Random.Range(1f, 2f);
            uiParticles.Add(dot);
            StartCoroutine(AnimateUIParticle(dot, vel, life));
        }
        yield break;
    }

    IEnumerator AnimateUIParticle(GameObject obj, Vector2 vel, float lifetime)
    {
        Image img = obj.GetComponent<Image>();
        float t = 0f;
        while (t < lifetime)
        {
            t += Time.deltaTime;
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition += vel * Time.deltaTime;
            if (img != null)
            {
                Color c = img.color;
                c.a = Mathf.Lerp(1f, 0f, t / lifetime);
                img.color = c;
            }
            yield return null;
        }
        uiParticles.Remove(obj);
        Destroy(obj);
    }

    void ClearUIParticles()
    {
        foreach (GameObject p in uiParticles)
        {
            if (p != null) Destroy(p);
        }
        uiParticles.Clear();
    }

    public void ClearUI()
    {
        ClearUIParticles();
        IsClosed = true;
        gameObject.SetActive(false);
    }
}
