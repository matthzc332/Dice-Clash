using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    private enum Step
    {
        Welcome,
        AttackDummies,
        ShadowPhase,
        Victory
    }

    private class PowerUpSlot
    {
        public int row;
        public int col;
        public PowerUpType type;
        public GameObject visual;
        public bool collected;
    }

    private Step currentStep;
    private GameObject overlayObj;
    private Text instructionText;
    private BoardManager board;
    private InputManager inputManager;
    private TurnManager turnManager;
    private int lastDummyCount;
    private int lastShadowCount;
    private PowerUpManager powerUp;
    private List<PowerUpSlot> powerUpSlots = new();
    private bool powerUpsExplained;
    private bool shadowPowerUpSpawned;
    private bool shadowSpawned;
    private AIController shadowAI;
    private Image panelImg;
    private RectTransform panelRt;
    private Sprite dialogueSprite;
    private Sprite dialogueSustoSprite;
    private Sprite dialogueFelizSprite;
    private Sprite dialogueBienSprite;
    private Vector2 smallPos = new Vector2(-684.4f, -406.7f);
    private Vector3 smallScale = new Vector3(0.83f, 0.83f, 1f);
    private RectTransform textRt;
    private bool shadowMidMessageShown;

    void Start()
    {
        board = FindFirstObjectByType<BoardManager>();
        inputManager = FindFirstObjectByType<InputManager>();
        turnManager = FindFirstObjectByType<TurnManager>();
        powerUp = FindFirstObjectByType<PowerUpManager>();

        if (powerUp == null)
        {
            GameObject puObj = new GameObject("PowerUpManager");
            powerUp = puObj.AddComponent<PowerUpManager>();
        }

        board.suppressVictoryCheck = true;

        lastDummyCount = 0;
        for (int r = 0; r < board.rows; r++)
            for (int c = 0; c < board.cols; c++)
            {
                Cell cell = board.GetCell(r, c);
                if (cell.IsOccupied && cell.pieceData?.team == Team.Red)
                    lastDummyCount++;
            }

        turnManager.OnTurnChanged += OnTurnChanged;
        CreateOverlay();
        StartCoroutine(WelcomeCoroutine());
    }

    void OnTurnChanged(TurnState state)
    {
        if (state == TurnState.RedTurn && GameConfig.isTutorial && currentStep != Step.ShadowPhase)
        {
            if (turnManager != null)
            {
                turnManager.currentTurn = TurnState.BlueTurn;
                turnManager.ResetTurnTimer();
            }
        }
    }

    void CreateOverlay()
    {
        overlayObj = new GameObject("TutorialOverlay");

        Canvas oc = overlayObj.AddComponent<Canvas>();
        oc.renderMode = RenderMode.ScreenSpaceOverlay;
        oc.sortingOrder = 200;

        Font dialogFont = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        if (dialogFont == null)
            dialogFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        dialogueSprite = Resources.Load<Sprite>("Tutorial/dialogue");
        dialogueSustoSprite = Resources.Load<Sprite>("Tutorial/dialogueSusto");
        dialogueFelizSprite = Resources.Load<Sprite>("Tutorial/dialogueFeliz");
        dialogueBienSprite = Resources.Load<Sprite>("Tutorial/dialogueBien");

        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(overlayObj.transform, false);
        panelImg = panelObj.AddComponent<Image>();
        if (dialogueSprite != null)
        {
            panelImg.sprite = dialogueSprite;
            panelImg.preserveAspect = true;
            panelImg.type = Image.Type.Sliced;
        }
        else
        {
            panelImg.color = new Color(1f, 0.97f, 0.88f, 0.92f);
        }
        panelRt = panelObj.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(667, 248);
        panelRt.anchoredPosition = smallPos;
        panelObj.transform.localScale = smallScale;

        GameObject textObj = new GameObject("InstructionText");
        textObj.transform.SetParent(panelObj.transform, false);
        instructionText = textObj.AddComponent<Text>();
        instructionText.font = dialogFont;
        instructionText.fontSize = 18;
        instructionText.alignment = TextAnchor.MiddleCenter;
        instructionText.color = Color.black;
        instructionText.text = "";
        textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(210, 12);
        textRt.offsetMax = new Vector2(-10, -128);

        GameObject skipObj = new GameObject("SkipButton");
        skipObj.transform.SetParent(overlayObj.transform, false);
        Image skipImg = skipObj.AddComponent<Image>();
        skipImg.color = new Color(0.6f, 0.2f, 0.2f, 0.9f);
        RectTransform skipRt = skipObj.GetComponent<RectTransform>();
        skipRt.anchorMin = new Vector2(1, 1);
        skipRt.anchorMax = new Vector2(1, 1);
        skipRt.pivot = new Vector2(1, 1);
        skipRt.sizeDelta = new Vector2(120, 40);
        skipRt.anchoredPosition = new Vector2(-20, -20);

        GameObject skipTextObj = new GameObject("SkipLabel");
        skipTextObj.transform.SetParent(skipObj.transform, false);
        Text skipLabel = skipTextObj.AddComponent<Text>();
        skipLabel.font = dialogFont;
        skipLabel.fontSize = 14;
        skipLabel.alignment = TextAnchor.MiddleCenter;
        skipLabel.text = "SKIP";
        skipLabel.color = Color.white;
        RectTransform skipLabelRt = skipTextObj.GetComponent<RectTransform>();
        skipLabelRt.anchorMin = Vector2.zero;
        skipLabelRt.anchorMax = Vector2.one;
        skipLabelRt.sizeDelta = Vector2.zero;

        Button skipButton = skipObj.AddComponent<Button>();
        skipButton.targetGraphic = skipImg;
        skipButton.onClick.AddListener(OnSkipClicked);
        skipButton.onClick.AddListener(() => SoundManager.Instance.PlayButton());

        CreatePowerUpSquares();
    }

    void CreatePowerUpSquares()
    {
        PowerUpType[] types = new PowerUpType[]
        {
            PowerUpType.Shake,
            PowerUpType.Explosion,
            PowerUpType.Fireball,
            PowerUpType.Lightning
        };

        for (int i = types.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = types[i]; types[i] = types[j]; types[j] = tmp;
        }

        Vector2Int[] positions = new Vector2Int[]
        {
            new Vector2Int(3, 0),
            new Vector2Int(3, 6),
            new Vector2Int(0, 3),
            new Vector2Int(6, 0),
        };

        Sprite[] allIcons = Resources.LoadAll<Sprite>("Sprites/PowerUps/Icon");

        for (int i = 0; i < positions.Length; i++)
        {
            var pos = positions[i];
            PowerUpType type = types[i % types.Length];
            Color color = PowerUpManager.GetColor(type);

            Vector3 worldPos = board.CellToWorld(pos.x, pos.y);
            worldPos.y -= 0.05f;

            GameObject container = new GameObject($"PowerUp_{type}");
            container.transform.position = worldPos;

            Sprite iconSprite = null;
            if (allIcons != null && allIcons.Length > 0)
            {
                foreach (var s in allIcons)
                    if (s.name == type.ToString() || s.name == type.ToString() + "_0") { iconSprite = s; break; }
            }
            if (iconSprite == null)
            {
                Texture2D[] texs = Resources.LoadAll<Texture2D>("Sprites/PowerUps/Icon");
                if (texs != null)
                    foreach (var t in texs)
                        if (t.name == type.ToString())
                            iconSprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
            }

            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(container.transform, false);
            SpriteRenderer sr = iconObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 14;

            if (iconSprite != null)
            {
                sr.sprite = iconSprite;
                sr.color = Color.white;
            }
            else
            {
                sr.sprite = CreatePowerUpIcon(type, color);
                sr.color = Color.white;
            }
            iconObj.transform.localScale = Vector3.one * 0.25f;

            GameObject glowObj = new GameObject("Glow");
            glowObj.transform.SetParent(container.transform, false);
            SpriteRenderer gsr = glowObj.AddComponent<SpriteRenderer>();
            gsr.sprite = CreateCircleSprite();
            gsr.color = new Color(color.r, color.g, color.b, 0.25f);
            gsr.sortingOrder = 13;
            glowObj.transform.localScale = Vector3.one * 0.9f;

            PowerUpSlot slot = new PowerUpSlot
            {
                row = pos.x,
                col = pos.y,
                type = type,
                visual = container,
                collected = false
            };
            powerUpSlots.Add(slot);

            StartCoroutine(AnimatePowerUp(container, iconObj, glowObj, color));
        }
    }

    IEnumerator AnimatePowerUp(GameObject container, GameObject icon, GameObject glow, Color color)
    {
        Vector3 basePos = container.transform.position;
        float t = 0;
        while (container != null)
        {
            float floatOff = Mathf.Sin(t * 2f) * 0.08f;
            container.transform.position = new Vector3(basePos.x, basePos.y + floatOff, basePos.z);

            if (icon != null)
            {
                float pulse = 1f + Mathf.Sin(t * 3f) * 0.1f;
                icon.transform.localScale = Vector3.one * 0.25f * pulse;
            }

            if (glow != null)
            {
                SpriteRenderer gsr = glow.GetComponent<SpriteRenderer>();
                if (gsr != null)
                {
                    float glowAlpha = 0.15f + Mathf.Sin(t * 2.5f) * 0.1f;
                    gsr.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(glowAlpha));
                    float glowScale = 0.45f + Mathf.Sin(t * 1.8f) * 0.08f;
                    glow.transform.localScale = Vector3.one * glowScale;
                }
            }

            t += Time.deltaTime;
            yield return null;
        }
    }

    void DrawThickLine(Texture2D tex, int x1, int y1, int x2, int y2, int thick, Color color)
    {
        int steps = Mathf.Max(Mathf.Abs(x2 - x1), Mathf.Abs(y2 - y1));
        for (int s = 0; s <= steps; s++)
        {
            float t = steps > 0 ? (float)s / steps : 0;
            int px = Mathf.RoundToInt(Mathf.Lerp(x1, x2, t));
            int py = Mathf.RoundToInt(Mathf.Lerp(y1, y2, t));
            for (int dy = -thick / 2; dy <= thick / 2; dy++)
                for (int dx = -thick / 2; dx <= thick / 2; dx++)
                {
                    int sx = px + dx, sy = py + dy;
                    if (sx >= 0 && sx < 64 && sy >= 0 && sy < 64)
                        tex.SetPixel(sx, sy, color);
                }
        }
    }

    void DrawDot(Texture2D tex, int cx, int cy, int radius, Color color)
    {
        for (int dy = -radius; dy <= radius; dy++)
            for (int dx = -radius; dx <= radius; dx++)
            {
                if (dx * dx + dy * dy <= radius * radius)
                {
                    int px = cx + dx, py = cy + dy;
                    if (px >= 0 && px < 64 && py >= 0 && py < 64)
                        tex.SetPixel(px, py, color);
                }
            }
    }

    Sprite CreatePowerUpIcon(PowerUpType type, Color color)
    {
        Texture2D tex = new Texture2D(64, 64);
        Color clear = Color.clear;

        for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
                tex.SetPixel(x, y, clear);

        switch (type)
        {
            case PowerUpType.Shake:
                DrawShakeIcon(tex, color);
                break;
            case PowerUpType.Explosion:
                DrawExplosionIcon(tex, color);
                break;
            case PowerUpType.Fireball:
                DrawFireballIcon(tex, color);
                break;
            case PowerUpType.Lightning:
                DrawLightningIcon(tex, color);
                break;
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
    }

    void DrawShakeIcon(Texture2D tex, Color color)
    {
        int cx = 32, cy = 32;
        for (int a = -1; a <= 1; a++)
            DrawThickLine(tex, cx - 10, cy + a * 12, cx + 10, cy + a * 12, 3, color);
        for (int i = 0; i < 5; i++)
        {
            tex.SetPixel(cx - 10 - i, cy - 12 + i, color);
            tex.SetPixel(cx + 10 + i, cy - 12 + i, color);
            tex.SetPixel(cx - 10 - i, cy + 12 - i, color);
            tex.SetPixel(cx + 10 + i, cy + 12 - i, color);
        }
    }

    void DrawExplosionIcon(Texture2D tex, Color color)
    {
        int cx = 32, cy = 32;
        for (int a = 0; a < 8; a++)
        {
            float angle = a * 3.14159f / 4;
            int ex = cx + (int)(Mathf.Cos(angle) * 14);
            int ey = cy + (int)(Mathf.Sin(angle) * 14);
            DrawThickLine(tex, cx, cy, ex, ey, 3, color);
        }
        DrawDot(tex, cx, cy, 6, color);
    }

    void DrawFireballIcon(Texture2D tex, Color color)
    {
        int cx = 32, cy = 32;
        for (int dy = -14; dy <= 14; dy++)
        {
            int halfW = Mathf.Max(0, 14 - Mathf.Abs(dy) / 2 - 2);
            DrawThickLine(tex, cx - halfW, cy + dy, cx + halfW, cy + dy, 3, color);
        }
        for (int i = 0; i < 4; i++)
        {
            tex.SetPixel(cx, cy - 18 - i, color);
            tex.SetPixel(cx - 1, cy - 18 - i, color);
            tex.SetPixel(cx + 1, cy - 18 - i, color);
        }
    }

    void DrawLightningIcon(Texture2D tex, Color color)
    {
        int cx = 32, cy = 32;
        int[] xs = { 0, -8, 8, -4, 12 };
        int[] ys = { -16, -6, 0, 8, 16 };
        for (int s = 0; s < xs.Length - 1; s++)
            DrawThickLine(tex, cx + xs[s], cy + ys[s], cx + xs[s + 1], cy + ys[s + 1], 3, color);
        DrawDot(tex, cx + xs[2], cy + ys[2], 3, color);
    }

    Sprite CreateCircleSprite()
    {
        Texture2D tex = new Texture2D(16, 16);
        Color[] pixels = new Color[256];
        for (int i = 0; i < pixels.Length; i++)
        {
            int x = i % 16;
            int y = i / 16;
            float dx = (x + 0.5f) / 16f - 0.5f;
            float dy = (y + 0.5f) / 16f - 0.5f;
            pixels[i] = (dx * dx + dy * dy < 0.25f) ? Color.white : Color.clear;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
    }

    void SetInstruction(string text)
    {
        if (instructionText != null)
            instructionText.text = text;
    }

    IEnumerator MovePanelBig()
    {
        if (panelRt == null) yield break;
        float dur = 0.35f;
        float t = 0;
        Vector2 fromPos = panelRt.anchoredPosition;
        Vector3 fromScale = panelRt.localScale;
        while (t < dur)
        {
            float raw = t / dur;
            float e = raw * raw * (3f - 2f * raw);
            panelRt.anchoredPosition = Vector2.Lerp(fromPos, new Vector2(0, -120), e);
            panelRt.localScale = Vector3.Lerp(fromScale, new Vector3(2.4f, 2.4f, 1f), e);
            t += Time.deltaTime;
            yield return null;
        }
        panelRt.anchoredPosition = new Vector2(0, -120);
        panelRt.localScale = new Vector3(2.4f, 2.4f, 1f);
        if (textRt != null)
        {
            textRt.offsetMin = new Vector2(150, 12);
            textRt.offsetMax = new Vector2(-10, -128);
        }
    }

    IEnumerator MovePanelSmall()
    {
        if (panelRt == null) yield break;
        float dur = 0.5f;
        float t = 0;
        Vector2 fromPos = panelRt.anchoredPosition;
        Vector3 fromScale = panelRt.localScale;
        while (t < dur)
        {
            float raw = t / dur;
            float e = raw * raw * (3f - 2f * raw);
            panelRt.anchoredPosition = Vector2.Lerp(fromPos, smallPos, e);
            panelRt.localScale = Vector3.Lerp(fromScale, smallScale, e);
            t += Time.deltaTime;
            yield return null;
        }
        panelRt.anchoredPosition = smallPos;
        panelRt.localScale = smallScale;
        if (textRt != null)
        {
            textRt.offsetMin = new Vector2(240f, 12);
            textRt.offsetMax = new Vector2(-48.5f, -128);
        }
    }

    IEnumerator WelcomeCoroutine()
    {
        currentStep = Step.Welcome;

        if (panelRt != null)
        {
            panelRt.anchoredPosition = new Vector2(0, -120);
            panelRt.localScale = new Vector3(2.4f, 2.4f, 1f);
        }
        SetInstruction("Choose a champion\nand move");
        yield return new WaitForSeconds(3f);

        yield return StartCoroutine(MovePanelSmall());

        currentStep = Step.AttackDummies;
        StartCoroutine(IdleBlinkLoop());
    }

    bool HasSelection()
    {
        return inputManager != null && inputManager.selectedCell != null;
    }

    IEnumerator IdleBlinkLoop()
    {
        while (true)
        {
            while (currentStep != Step.AttackDummies || HasSelection())
                yield return null;

            float idle = 0f;
            while (currentStep == Step.AttackDummies && !HasSelection() && idle < 4f)
            {
                idle += Time.deltaTime;
                yield return null;
            }

            if (currentStep != Step.AttackDummies || HasSelection()) continue;

            List<SpriteRenderer> blueRenderers = new();
            for (int r = 0; r < board.rows; r++)
                for (int c = 0; c < board.cols; c++)
                {
                    Cell cell = board.GetCell(r, c);
                    if (cell.IsOccupied && cell.pieceData?.team == Team.Blue && cell.pieceVisual != null)
                    {
                        SpriteRenderer sr = cell.pieceVisual.GetComponent<SpriteRenderer>();
                        if (sr != null) blueRenderers.Add(sr);
                    }
                }

            if (blueRenderers.Count == 0) continue;

            float t = 0;
            while (currentStep == Step.AttackDummies && !HasSelection() && t < 3f)
            {
                float brightness = 0.3f + (Mathf.Sin(t * 8f) * 0.5f + 0.5f) * 0.7f;
                foreach (var sr in blueRenderers)
                {
                    if (sr != null)
                        sr.color = new Color(brightness, brightness, brightness, 1f);
                }
                t += Time.deltaTime;
                yield return null;
            }

            foreach (var sr in blueRenderers)
            {
                if (sr != null)
                    sr.color = Color.white;
            }
        }
    }

    void Update()
    {
        if (currentStep == Step.AttackDummies)
        {
            CheckDummyKilled();
            if (!powerUpsExplained)
                CheckShowPowerUpHint();
            CheckPowerUpCollection();
        }
        if (currentStep == Step.ShadowPhase)
        {
            CheckShadowEnemyKilled();
        }
    }

    void CheckShowPowerUpHint()
    {
        for (int r = 0; r < board.rows; r++)
            for (int c = 0; c < board.cols; c++)
            {
                Cell cell = board.GetCell(r, c);
                if (cell.IsOccupied && cell.pieceData?.team == Team.Blue)
                {
                    // Check if any blue piece is adjacent to a power-up
                    foreach (var slot in powerUpSlots)
                    {
                        if (slot.collected) continue;
                        if (Mathf.Abs(r - slot.row) <= 1 && Mathf.Abs(c - slot.col) <= 1)
                        {
                            powerUpsExplained = true;
                            SetInstruction("Step on the\nPower square!");
                            return;
                        }
                    }
                }
            }
    }

    void CheckPowerUpCollection()
    {
        foreach (var slot in powerUpSlots)
        {
            if (slot.collected) continue;

            Cell cell = board.GetCell(slot.row, slot.col);
            if (cell != null && cell.IsOccupied && cell.pieceData?.team == Team.Blue)
            {
                slot.collected = true;
                if (slot.visual != null) Destroy(slot.visual);

                powerUp.Execute(slot.type, slot.row, slot.col);
                SetInstruction("Let's go!");
            }
        }
    }

    void CheckDummyKilled()
    {
        int alive = 0;
        for (int r = 0; r < board.rows; r++)
            for (int c = 0; c < board.cols; c++)
            {
                Cell cell = board.GetCell(r, c);
                if (cell.IsOccupied && cell.pieceData?.team == Team.Red)
                    alive++;
            }

        if (alive < lastDummyCount && alive > 0 && alive <= 3)
            SetInstruction($"Nice! only {alive} left");

        lastDummyCount = alive;

        if (alive == 0)
        {
            StartCoroutine(ShadowPhaseTransition());
        }
    }

    IEnumerator ShadowPhaseTransition()
    {
        currentStep = Step.ShadowPhase;
        shadowSpawned = false;
        shadowPowerUpSpawned = false;
        SetInstruction("Preparing new enemies...");
        yield return new WaitForSeconds(1.5f);

        board.isShadowPhase = true;
        board.suppressVictoryCheck = false;
        board.ShowTutorialMidground();
        board.DarkenScene();
        board.ResetToNormalLayout();

        if (shadowAI == null)
        {
            GameObject aiObj = new GameObject("ShadowAI");
            shadowAI = aiObj.AddComponent<AIController>();
        }

        ClearTutorialPowerUps();
        SpawnShadowEnemies();
        shadowPowerUpSpawned = true;

        instructionText.text = "";
        if (panelImg != null && dialogueSustoSprite != null)
            panelImg.sprite = dialogueSustoSprite;
        yield return StartCoroutine(MovePanelBig());
        yield return new WaitForSeconds(0.3f);
        SetInstruction("Shadows? Defeat them!!");
        yield return new WaitForSeconds(1.2f);

        StartCoroutine(ShakeCamera(0.15f));

        yield return StartCoroutine(MovePanelSmall());
        if (panelImg != null && dialogueSprite != null)
            panelImg.sprite = dialogueSprite;
        SetInstruction("Use the powers to defeat them!");
        yield return new WaitForSeconds(2.5f);
        powerUp.SpawnOnBoard();
        yield return new WaitForSeconds(2f);
        powerUp.SpawnOnBoard();
        yield return new WaitForSeconds(2f);
        powerUp.SpawnOnBoard();
        yield return new WaitForSeconds(1f);
        SetInstruction("Move onto glowing squares\nfor power-ups!");
    }

    void ClearTutorialPowerUps()
    {
        foreach (var slot in powerUpSlots)
        {
            if (slot.visual != null) Destroy(slot.visual);
        }
        powerUpSlots.Clear();
    }

    void SpawnShadowEnemies()
    {
        int spawned = 0;
        foreach (var entry in InitialPosition.Entries)
        {
            if (entry.team != Team.Red) continue;

            Cell cell = board.GetCell(entry.row, entry.col);
            if (cell == null || cell.IsOccupied) continue;

            board.SpawnShadowPieceAt(entry.row, entry.col, entry.type);
            spawned++;
        }

        lastShadowCount = spawned;
        shadowSpawned = true;
        shadowMidMessageShown = false;
    }

    void CheckShadowPowerUpCollection()
    {
        powerUp.CheckCollectionForTeam(Team.Blue);
    }

    void CheckShadowEnemyKilled()
    {
        if (!shadowSpawned) return;

        int alive = 0;
        for (int r = 0; r < board.rows; r++)
            for (int c = 0; c < board.cols; c++)
            {
                Cell cell = board.GetCell(r, c);
                if (cell.IsOccupied && cell.pieceData?.team == Team.Red)
                    alive++;
            }

        if (alive < lastShadowCount && alive == 1 && !shadowMidMessageShown)
        {
            shadowMidMessageShown = true;
            SetInstruction("keep going!");
        }

        lastShadowCount = alive;

        if (alive == 0 && shadowPowerUpSpawned)
        {
            if (currentStep == Step.Victory) return;
            currentStep = Step.Victory;
            TutorialCollectibles.Grant();
            TutorialProgress.MarkPlayed();
            powerUp.ClearAllSpawned();
            StartCoroutine(VictoryDialogueSequence());
        }
    }

    IEnumerator VictoryDialogueSequence()
    {
        ScoreboardUI sb = FindFirstObjectByType<ScoreboardUI>();
        if (sb != null && sb.gameObject.activeInHierarchy)
        {
            CleanupShadowPhase();
            DestroyOverlay();
            yield break;
        }

        yield return StartCoroutine(MovePanelBig());

        if (panelImg != null && dialogueFelizSprite != null)
            panelImg.sprite = dialogueFelizSprite;
        SetInstruction("Victory! Well done!");
        yield return new WaitForSeconds(2f);

        if (panelImg != null && dialogueBienSprite != null)
            panelImg.sprite = dialogueBienSprite;
        if (textRt != null)
            textRt.offsetMin = new Vector2(240, 12);
        SetInstruction("See you in battle!");
        yield return new WaitForSeconds(2f);

        StartCoroutine(VictoryTransition());
    }

    IEnumerator VictoryTransition()
    {
        CleanupShadowPhase();
        DestroyOverlay();

        TimerManager.Instance.Stop();

        ScoreboardUI.Instance.Show();
        yield break;
    }

    void CleanupShadowPhase()
    {
        if (shadowAI != null)
        {
            Destroy(shadowAI.gameObject);
            shadowAI = null;
        }
        if (board != null)
            board.isShadowPhase = false;
    }

    void OnSkipClicked()
    {
        CleanupShadowPhase();
        GameConfig.isTutorial = false;
        TutorialProgress.MarkPlayed();
        DestroyOverlay();
        GameConfig.Play("Human");
    }

    void DestroyOverlay()
    {
        if (overlayObj != null)
            Destroy(overlayObj);
    }

    IEnumerator ShakeCamera(float intensity)
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;
        Vector3 orig = cam.transform.position;
        float t = 0;
        while (t < 0.2f)
        {
            Vector3 offset = Random.insideUnitCircle * intensity;
            cam.transform.position = orig + new Vector3(offset.x, offset.y, 0);
            t += Time.deltaTime;
            yield return null;
        }
        cam.transform.position = orig;
    }

    void OnDestroy()
    {
        if (turnManager != null)
            turnManager.OnTurnChanged -= OnTurnChanged;
    }
}
