using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChestUI : MonoBehaviour
{
    private GameObject panelObj;
    private Canvas canvas;
    private Font font;
    private Text[] slotTimerTexts = new Text[2];
    private Image[] slotProgressBars = new Image[2];
    private Button[] slotButtons = new Button[2];
    private GameObject[] slotObjs = new GameObject[2];
    private bool updating;
    private Sprite[] chestSprites;
    private Sprite circleSprite;

    void LoadChestSprites()
    {
        chestSprites = Resources.LoadAll<Sprite>("Sprites/Cofre");
        if (chestSprites == null || chestSprites.Length == 0)
        {
            Texture2D tex = new Texture2D(64, 64);
            chestSprites = new Sprite[5];
            for (int i = 0; i < 5; i++) chestSprites[i] = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        }
    }

    Sprite GetChestSprite(int frame)
    {
        int idx = Mathf.Clamp(frame, 0, 4);
        foreach (var s in chestSprites)
        {
            if (s.name == $"cofre{idx}" || s.name.StartsWith($"cofre{idx}_")) return s;
        }
        return chestSprites.Length > 0 ? chestSprites[0] : null;
    }

    public void Show(Canvas parentCanvas)
    {
        font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        canvas = parentCanvas;
        if (circleSprite == null)
        {
            Texture2D ct = new Texture2D(16, 16);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    float dx = x - 7.5f;
                    float dy = y - 7.5f;
                    ct.SetPixel(x, y, Mathf.Sqrt(dx * dx + dy * dy) <= 7.5f ? Color.white : Color.clear);
                }
            ct.Apply();
            circleSprite = Sprite.Create(ct, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 100);
        }
        LoadChestSprites();
        BuildPanel();
        updating = true;
        StartCoroutine(UpdateTimers());
    }

    public void Hide()
    {
        updating = false;
        if (panelObj != null)
        {
            Destroy(panelObj);
            panelObj = null;
        }
    }

    void BuildPanel()
    {
        panelObj = new GameObject("ChestPanel");
        panelObj.transform.SetParent(canvas.transform, false);

        Image bg = panelObj.AddComponent<Image>();
        bg.color = new Color(0.06f, 0.04f, 0.03f, 0.95f);
        RectTransform bgRt = panelObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;

        CreateTitle();
        CreateSlots();
        CreateTestButton();
        CreateBackButton();
    }

    void CreateTestButton()
    {
        GameObject btnObj = new GameObject("TestChestButton");
        btnObj.transform.SetParent(panelObj.transform, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.3f, 0.2f, 0.45f, 0.9f);
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0f, 0f);
        btnRt.anchorMax = new Vector2(0f, 0f);
        btnRt.pivot = new Vector2(0f, 0f);
        btnRt.sizeDelta = new Vector2(150, 40);
        btnRt.anchoredPosition = new Vector2(15, 15);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        Text btnText = textObj.AddComponent<Text>();
        btnText.font = font;
        btnText.fontSize = 10;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.text = "TEST CHEST";
        btnText.color = Color.white;
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySelect();
            if (ChestManager.AddReadyChestForTest())
                RefreshSlots();
        });
    }

    void CreateTitle()
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelObj.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.font = font;
        titleText.fontSize = 22;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.text = "CHESTS";
        titleText.color = new Color(0.9f, 0.75f, 0.3f);
        RectTransform tRt = titleObj.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0f, 1f);
        tRt.anchorMax = new Vector2(1f, 1f);
        tRt.pivot = new Vector2(0.5f, 1f);
        tRt.sizeDelta = new Vector2(0, 50);
        tRt.anchoredPosition = new Vector2(0, -10);

        GameObject descObj = new GameObject("Desc");
        descObj.transform.SetParent(panelObj.transform, false);
        Text descText = descObj.AddComponent<Text>();
        descText.font = font;
        descText.fontSize = 10;
        descText.alignment = TextAnchor.MiddleCenter;
        descText.text = "Complete campaigns to earn chests!";
        descText.color = new Color(0.6f, 0.55f, 0.45f);
        RectTransform dRt = descObj.GetComponent<RectTransform>();
        dRt.anchorMin = new Vector2(0f, 1f);
        dRt.anchorMax = new Vector2(1f, 1f);
        dRt.pivot = new Vector2(0.5f, 1f);
        dRt.sizeDelta = new Vector2(0, 30);
        dRt.anchoredPosition = new Vector2(0, -55);
    }

    void CreateSlots()
    {
        float[] slotX = { -220f, 220f };

        for (int i = 0; i < 2; i++)
        {
            ChestSlot slot = ChestManager.Slots[i];
            GameObject slotObj = new GameObject($"Slot_{i}");
            slotObj.transform.SetParent(panelObj.transform, false);
            slotObjs[i] = slotObj;
            RectTransform slotRt = slotObj.AddComponent<RectTransform>();
            slotRt.anchorMin = new Vector2(0.5f, 0.5f);
            slotRt.anchorMax = new Vector2(0.5f, 0.5f);
            slotRt.pivot = new Vector2(0.5f, 0.5f);
            slotRt.sizeDelta = new Vector2(400, 430);
            slotRt.anchoredPosition = new Vector2(slotX[i], 20);

            Image slotBg = slotObj.AddComponent<Image>();
            slotBg.color = new Color(0.15f, 0.1f, 0.06f, 0.9f);

            CreateSlotBorder(slotObj.transform);

            if (!slot.occupied)
            {
                CreateEmptySlot(slotObj.transform, i);
            }
            else if (slot.ready)
            {
                CreateReadySlot(slotObj.transform, i);
            }
            else
            {
                CreateLockedSlot(slotObj.transform, i);
            }
        }
    }

    void CreateSlotBorder(Transform parent)
    {
        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(parent, false);
        Image borderImg = borderObj.AddComponent<Image>();
        borderImg.color = new Color(0.6f, 0.45f, 0.2f, 0.6f);
        borderImg.raycastTarget = false;
        RectTransform bRt = borderObj.GetComponent<RectTransform>();
        bRt.anchorMin = Vector2.zero;
        bRt.anchorMax = Vector2.one;
        bRt.sizeDelta = new Vector2(6, 6);
    }

    void CreateEmptySlot(Transform parent, int index)
    {
        GameObject emptyObj = new GameObject("Empty");
        emptyObj.transform.SetParent(parent, false);
        Text emptyText = emptyObj.AddComponent<Text>();
        emptyText.font = font;
        emptyText.fontSize = 16;
        emptyText.alignment = TextAnchor.MiddleCenter;
        emptyText.text = "EMPTY";
        emptyText.color = new Color(0.4f, 0.35f, 0.3f);
        RectTransform eRt = emptyObj.GetComponent<RectTransform>();
        eRt.anchorMin = new Vector2(0f, 0.3f);
        eRt.anchorMax = new Vector2(1f, 0.7f);
        eRt.sizeDelta = Vector2.zero;
    }

    void CreateReadySlot(Transform parent, int index)
    {
        GameObject auraObj = new GameObject("Aura", typeof(RectTransform));
        auraObj.transform.SetParent(parent, false);
        Image auraImg = auraObj.AddComponent<Image>();
        auraImg.color = new Color(1f, 0.85f, 0.2f, 0.16f);
        auraImg.raycastTarget = false;
        RectTransform auraRt = auraObj.GetComponent<RectTransform>();
        auraRt.anchorMin = new Vector2(0.5f, 0.5f);
        auraRt.anchorMax = new Vector2(0.5f, 0.5f);
        auraRt.sizeDelta = new Vector2(220, 220);
        auraRt.anchoredPosition = new Vector2(0, 75);
        StartCoroutine(PulseReadyAura(auraImg));

        GameObject chestObj = new GameObject("ChestIcon", typeof(RectTransform));
        chestObj.transform.SetParent(parent, false);
        Image chestImg = chestObj.AddComponent<Image>();
        chestImg.sprite = GetChestSprite(4);
        chestImg.preserveAspect = true;
        RectTransform cRt = chestObj.GetComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0.5f, 0.5f);
        cRt.anchorMax = new Vector2(0.5f, 0.5f);
        cRt.sizeDelta = new Vector2(170, 170);
        cRt.anchoredPosition = new Vector2(0, 75);

        EventTrigger trigger = parent.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry enter = new EventTrigger.Entry();
        enter.eventID = EventTriggerType.PointerEnter;
        enter.callback.AddListener((BaseEventData data) => StartCoroutine(HoverGrow(chestObj.transform, 1.25f)));
        trigger.triggers.Add(enter);
        EventTrigger.Entry exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;
        exit.callback.AddListener((BaseEventData data) => StartCoroutine(HoverGrow(chestObj.transform, 1f)));
        trigger.triggers.Add(exit);

        GameObject labelObj = new GameObject("ReadyLabel");
        labelObj.transform.SetParent(parent, false);
        Text labelText = labelObj.AddComponent<Text>();
        labelText.font = font;
        labelText.fontSize = 18;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.text = "READY!";
        labelText.color = new Color(0.3f, 1f, 0.3f);
        RectTransform lRt = labelObj.GetComponent<RectTransform>();
        lRt.anchorMin = new Vector2(0f, 0.45f);
        lRt.anchorMax = new Vector2(1f, 0.55f);
        lRt.sizeDelta = Vector2.zero;

        CreateOpenButton(parent, index, -90);
    }

    IEnumerator PulseReadyAura(Image aura)
    {
        float t = 0;
        while (aura != null)
        {
            t += Time.deltaTime;
            float a = 0.12f + Mathf.Sin(t * 3f) * 0.08f;
            aura.color = new Color(1f, 0.85f, 0.2f, Mathf.Max(0.05f, a));
            yield return null;
        }
    }

    IEnumerator HoverGrow(Transform t, float target)
    {
        if (t == null) yield break;
        float from = t.localScale.x;
        float dur = 0.12f;
        float t0 = 0f;
        while (t0 < dur)
        {
            if (t == null) yield break;
            t0 += Time.deltaTime;
            float s = Mathf.Lerp(from, target, t0 / dur);
            t.localScale = new Vector3(s, s, 1);
            yield return null;
        }
        if (t != null) t.localScale = new Vector3(target, target, 1);
    }

    void CreateLockedSlot(Transform parent, int index)
    {
        GameObject chestObj = new GameObject("ChestIcon", typeof(RectTransform));
        chestObj.transform.SetParent(parent, false);
        Image chestImg = chestObj.AddComponent<Image>();
        chestImg.sprite = GetChestSprite(0);
        chestImg.preserveAspect = true;
        RectTransform cRt = chestObj.GetComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0.5f, 0.5f);
        cRt.anchorMax = new Vector2(0.5f, 0.5f);
        cRt.sizeDelta = new Vector2(140, 140);
        cRt.anchoredPosition = new Vector2(0, 70);

        GameObject timerObj = new GameObject("Timer");
        timerObj.transform.SetParent(parent, false);
        Text timerText = timerObj.AddComponent<Text>();
        timerText.font = font;
        timerText.fontSize = 12;
        timerText.alignment = TextAnchor.MiddleCenter;
        timerText.color = new Color(0.8f, 0.7f, 0.4f);
        slotTimerTexts[index] = timerText;
        RectTransform tRt = timerObj.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0f, 0.35f);
        tRt.anchorMax = new Vector2(1f, 0.48f);
        tRt.sizeDelta = Vector2.zero;

        GameObject progressBg = new GameObject("ProgressBg");
        progressBg.transform.SetParent(parent, false);
        Image pBgImg = progressBg.AddComponent<Image>();
        pBgImg.color = new Color(0.2f, 0.15f, 0.1f);
        pBgImg.raycastTarget = false;
        RectTransform pBgRt = progressBg.GetComponent<RectTransform>();
        pBgRt.anchorMin = new Vector2(0.15f, 0.28f);
        pBgRt.anchorMax = new Vector2(0.85f, 0.32f);
        pBgRt.sizeDelta = Vector2.zero;

        GameObject progressFill = new GameObject("ProgressFill");
        progressFill.transform.SetParent(progressBg.transform, false);
        Image pFillImg = progressFill.AddComponent<Image>();
        pFillImg.color = new Color(0.8f, 0.6f, 0.1f);
        pFillImg.raycastTarget = false;
        slotProgressBars[index] = pFillImg;
        RectTransform pFillRt = progressFill.GetComponent<RectTransform>();
        pFillRt.anchorMin = Vector2.zero;
        pFillRt.anchorMax = new Vector2(0, 1);
        pFillRt.sizeDelta = Vector2.zero;
    }

    void CreateOpenButton(Transform parent, int index, float yOffset)
    {
        GameObject btnObj = new GameObject("OpenBtn");
        btnObj.transform.SetParent(parent, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.3f, 0.6f, 0.3f, 0.9f);
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0f);
        btnRt.anchorMax = new Vector2(0.5f, 0f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.sizeDelta = new Vector2(200, 45);
        btnRt.anchoredPosition = new Vector2(0, 50 + yOffset);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        Text btnText = textObj.AddComponent<Text>();
        btnText.font = font;
        btnText.fontSize = 14;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.text = "OPEN";
        btnText.color = Color.white;
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        int capturedIndex = index;
        btn.onClick.AddListener(() => OnOpenClicked(capturedIndex));
    }

    void OnOpenClicked(int slotIndex)
    {
        SoundManager.Instance.PlaySelect();
        ChestReward reward = ChestManager.OpenChest(slotIndex);
        if (reward.insignias.Count > 0 || reward.gold > 0)
        {
            StartCoroutine(ShowRewardSequence(reward));
            RefreshSlots();
        }
    }

    IEnumerator ShowRewardSequence(ChestReward reward)
    {
        GameObject popupObj = new GameObject("RewardPopup", typeof(RectTransform));
        popupObj.transform.SetParent(canvas.transform, false);
        RectTransform popupRt = popupObj.GetComponent<RectTransform>();
        popupRt.anchorMin = Vector2.zero;
        popupRt.anchorMax = Vector2.one;
        popupRt.sizeDelta = Vector2.zero;

        Image overlay = popupObj.AddComponent<Image>();
        overlay.color = new Color(0, 0, 0, 0.75f);

        GameObject contentObj = new GameObject("Content", typeof(RectTransform));
        contentObj.transform.SetParent(popupObj.transform, false);
        Image contentBg = contentObj.AddComponent<Image>();
        contentBg.color = new Color(0.12f, 0.08f, 0.04f, 0.95f);
        RectTransform contentRt = contentObj.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0.5f, 0.5f);
        contentRt.anchorMax = new Vector2(0.5f, 0.5f);
        contentRt.sizeDelta = new Vector2(680, 680);

        GameObject chestAnimObj = new GameObject("ChestAnimation", typeof(RectTransform));
        chestAnimObj.transform.SetParent(contentObj.transform, false);
        Image chestAnimImg = chestAnimObj.AddComponent<Image>();
        chestAnimImg.preserveAspect = true;
        RectTransform caRt = chestAnimObj.GetComponent<RectTransform>();
        caRt.anchorMin = new Vector2(0.5f, 0.5f);
        caRt.anchorMax = new Vector2(0.5f, 0.5f);
        caRt.sizeDelta = new Vector2(170, 170);
        caRt.anchoredPosition = new Vector2(0, 90);

        for (int f = 0; f <= 4; f++)
        {
            chestAnimImg.sprite = GetChestSprite(f);
            if (f == 4)
            {
                SoundManager.Instance.PlayCoin();
                for (int s = 0; s < 16; s++)
                {
                    GameObject spark = new GameObject("BurstSpark", typeof(RectTransform));
                    spark.transform.SetParent(contentObj.transform, false);
                    Image sparkImg = spark.AddComponent<Image>();
                    sparkImg.sprite = circleSprite;
                    sparkImg.color = new Color(1f, 0.84f, 0f);
                    sparkImg.raycastTarget = false;
                    RectTransform sr = spark.GetComponent<RectTransform>();
                    sr.anchorMin = new Vector2(0.5f, 0.5f);
                    sr.anchorMax = new Vector2(0.5f, 0.5f);
                    sr.sizeDelta = new Vector2(9, 9);
                    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    float dist = Random.Range(80f, 170f);
                    Vector2 target = new Vector2(Mathf.Cos(angle) * dist, 90 + Mathf.Sin(angle) * dist);
                    StartCoroutine(AnimateSparkBurst(sr, target));
                }
            }
            yield return new WaitForSeconds(0.12f);
        }
        Destroy(chestAnimObj);

        yield return new WaitForSeconds(0.15f);

        for (int g = 0; g < 15; g++)
        {
            GameObject goldParticle = new GameObject("GoldParticle", typeof(RectTransform));
            goldParticle.transform.SetParent(contentObj.transform, false);
            Image gpImg = goldParticle.AddComponent<Image>();
            gpImg.sprite = circleSprite;
            gpImg.color = new Color(1f, 0.8f, 0f, 0.9f);
            gpImg.raycastTarget = false;
            RectTransform gpr = goldParticle.GetComponent<RectTransform>();
            gpr.anchorMin = new Vector2(0.5f, 0.5f);
            gpr.anchorMax = new Vector2(0.5f, 0.5f);
            gpr.sizeDelta = new Vector2(Random.Range(6f, 12f), Random.Range(6f, 12f));
            gpr.anchoredPosition = new Vector2(Random.Range(-80f, 80f), Random.Range(80f, 130f));
            float gLife = Random.Range(0.4f, 0.8f);
            Vector2 gDir = new Vector2(Random.Range(-40f, 40f), Random.Range(-80f, -40f));
            StartCoroutine(AnimateFloatingParticle(gpr, gDir, gLife));
        }

        GameObject titleObj = new GameObject("Title", typeof(RectTransform));
        titleObj.transform.SetParent(contentObj.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.font = font;
        titleText.fontSize = 18;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.text = "CHEST OPENED!";
        titleText.color = new Color(0.9f, 0.75f, 0.2f);
            RectTransform tRt = titleObj.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0f, 0.9f);
            tRt.anchorMax = new Vector2(1f, 1f);
            tRt.sizeDelta = Vector2.zero;
        Vector3 titleOrig = tRt.localScale;
        tRt.localScale = Vector3.zero;
        float titleT = 0;
        while (titleT < 0.3f)
        {
            titleT += Time.deltaTime;
            float s = Mathf.Lerp(0f, 1f, Mathf.Sin(titleT / 0.3f * Mathf.PI * 0.5f));
            tRt.localScale = new Vector3(s, s, 1);
            yield return null;
        }
        tRt.localScale = titleOrig;

        yield return new WaitForSeconds(0.3f);

        if (reward.gold > 0)
        {
            GameObject goldObj = new GameObject("GoldReward", typeof(RectTransform));
            goldObj.transform.SetParent(contentObj.transform, false);
            Text goldText = goldObj.AddComponent<Text>();
            goldText.font = font;
            goldText.fontSize = 26;
            goldText.alignment = TextAnchor.MiddleCenter;
            goldText.text = $"+{reward.gold} GOLD!";
            goldText.color = new Color(1f, 0.84f, 0f);
            RectTransform gRt = goldObj.GetComponent<RectTransform>();
            gRt.anchorMin = new Vector2(0f, 0.8f);
            gRt.anchorMax = new Vector2(1f, 0.9f);
            gRt.sizeDelta = Vector2.zero;

            Outline goldOutline = goldObj.AddComponent<Outline>();
            goldOutline.effectColor = new Color(0.5f, 0.3f, 0f);
            goldOutline.effectDistance = new Vector2(2, -2);

            Vector3 gOrig = gRt.localScale;
            float gBounce = 0;
            while (gBounce < 0.5f)
            {
                gBounce += Time.deltaTime;
                float s = 1f + Mathf.Sin(gBounce / 0.5f * Mathf.PI * 2f) * 0.2f * (1f - gBounce / 0.5f);
                gRt.localScale = new Vector3(s, s, 1);
                yield return null;
            }
            gRt.localScale = gOrig;

            for (int g = 0; g < 8; g++)
            {
                GameObject gp = new GameObject("GoldCoin", typeof(RectTransform));
                gp.transform.SetParent(contentObj.transform, false);
                Image gpImg = gp.AddComponent<Image>();
                gpImg.sprite = circleSprite;
                gpImg.color = new Color(1f, 0.84f, 0f, 0.8f);
                gpImg.raycastTarget = false;
                RectTransform gprt = gp.GetComponent<RectTransform>();
                gprt.anchorMin = new Vector2(0.5f, 0.5f);
                gprt.anchorMax = new Vector2(0.5f, 0.5f);
                gprt.sizeDelta = new Vector2(Random.Range(8f, 14f), Random.Range(8f, 14f));
                gprt.anchoredPosition = new Vector2(Random.Range(-50f, 50f), Random.Range(60f, 90f));
                Vector2 gDir2 = new Vector2(Random.Range(-30f, 30f), Random.Range(-100f, -50f));
                StartCoroutine(AnimateFloatingParticle(gprt, gDir2, Random.Range(0.5f, 0.9f)));
            }

            SoundManager.Instance.PlayCoin();
            yield return new WaitForSeconds(0.8f);
        }

        int bonusPowerups = Random.Range(1, 4);
        Color[] pwColors = {
            new Color(0.4f, 0.9f, 0.4f),
            new Color(1f, 0.5f, 0.2f),
            new Color(1f, 0.3f, 0.2f),
            new Color(0.5f, 0.7f, 1f),
            new Color(0.6f, 0.2f, 1f)
        };
        string[] pwNames = { "SHAKE", "EXPLOSION", "FIREBALL", "LIGHTNING", "MAGIC" };

        for (int p = 0; p < bonusPowerups; p++)
        {
            int pwIdx = Random.Range(0, pwNames.Length);
            Color pwCol = pwColors[pwIdx];

            GameObject pwObj = new GameObject($"Powerup_{p}", typeof(RectTransform));
            pwObj.transform.SetParent(contentObj.transform, false);
            Image pwIcon = pwObj.AddComponent<Image>();
            pwIcon.sprite = circleSprite;
            pwIcon.color = pwCol;
            pwIcon.raycastTarget = false;
            RectTransform pwIconRt = pwObj.GetComponent<RectTransform>();
            pwIconRt.anchorMin = new Vector2(0.5f, 0.5f);
            pwIconRt.anchorMax = new Vector2(0.5f, 0.5f);
            pwIconRt.sizeDelta = new Vector2(16, 16);
            pwIconRt.anchoredPosition = new Vector2(-170, 75 - p * 26);

            GameObject pwLabel = new GameObject("Label", typeof(RectTransform));
            pwLabel.transform.SetParent(pwObj.transform, false);
            Text pwText = pwLabel.AddComponent<Text>();
            pwText.font = font;
            pwText.fontSize = 10;
            pwText.alignment = TextAnchor.MiddleLeft;
            pwText.text = pwNames[pwIdx];
            pwText.color = pwCol;
            RectTransform pwLabelRt = pwLabel.GetComponent<RectTransform>();
            pwLabelRt.anchorMin = new Vector2(0f, 0f);
            pwLabelRt.anchorMax = new Vector2(1f, 1f);
            pwLabelRt.offsetMin = new Vector2(22, 0);
            pwLabelRt.offsetMax = new Vector2(0, 0);

            Vector3 pwOrig = pwObj.transform.localScale;
            pwObj.transform.localScale = Vector3.zero;
            float pwScale = 0;
            while (pwScale < 0.25f)
            {
                pwScale += Time.deltaTime;
                float s = Mathf.Lerp(0f, 1f, pwScale / 0.25f);
                pwObj.transform.localScale = new Vector3(s, s, 1);
                yield return null;
            }
            pwObj.transform.localScale = pwOrig;

            for (int sp = 0; sp < 4; sp++)
            {
                GameObject pwSpark = new GameObject("PwSpark", typeof(RectTransform));
                pwSpark.transform.SetParent(contentObj.transform, false);
                Image psImg = pwSpark.AddComponent<Image>();
                psImg.sprite = circleSprite;
                psImg.color = pwCol;
                psImg.raycastTarget = false;
                RectTransform psRt = pwSpark.GetComponent<RectTransform>();
                psRt.anchorMin = new Vector2(0.5f, 0.5f);
                psRt.anchorMax = new Vector2(0.5f, 0.5f);
                psRt.sizeDelta = new Vector2(5, 5);
                psRt.anchoredPosition = pwIconRt.anchoredPosition;
                Vector2 psDir = new Vector2(Random.Range(-30f, 30f), Random.Range(-20f, 20f));
                StartCoroutine(AnimateSparkBurst(psRt, psRt.anchoredPosition + psDir));
            }

            yield return new WaitForSeconds(0.15f);
        }

        float yStart = -30f;
        for (int i = 0; i < reward.insignias.Count; i++)
        {
            var insignia = InsigniaData.GetInsignia(reward.insignias[i]);
            if (insignia == null) continue;

            Color rarityColor = GetRarityColor(insignia.rarity);
            bool isDup = reward.isDuplicate[i];

            GameObject itemObj = new GameObject($"Item_{i}", typeof(RectTransform));
            itemObj.transform.SetParent(contentObj.transform, false);
            Image itemBg = itemObj.AddComponent<Image>();
            itemBg.color = new Color(rarityColor.r * 0.2f, rarityColor.g * 0.2f, rarityColor.b * 0.2f, 0.55f);
            RectTransform itemRt = itemObj.GetComponent<RectTransform>();
            itemRt.anchorMin = new Vector2(0.5f, 0.5f);
            itemRt.anchorMax = new Vector2(0.5f, 0.5f);
            itemRt.pivot = new Vector2(0.5f, 1f);
            itemRt.sizeDelta = new Vector2(520, 68);
            itemRt.anchoredPosition = new Vector2(0, yStart - i * 85);
            Vector3 itemOrigScale = itemRt.localScale;
            itemRt.localScale = new Vector3(0.5f, 0.5f, 1);
            float itemS = 0;
            while (itemS < 0.2f)
            {
                itemS += Time.deltaTime;
                float s = Mathf.Lerp(0.5f, 1f, Mathf.Sin(itemS / 0.2f * Mathf.PI * 0.5f));
                itemRt.localScale = new Vector3(s, s, 1);
                yield return null;
            }
            itemRt.localScale = itemOrigScale;

            Sprite insSprite = InsigniaSprites.Get(insignia);
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform));
            iconObj.transform.SetParent(itemObj.transform, false);
            Image iconImg = iconObj.AddComponent<Image>();
            if (insSprite != null) iconImg.sprite = insSprite;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            RectTransform iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0f, 0.1f);
            iconRt.anchorMax = new Vector2(0f, 0.9f);
            iconRt.pivot = new Vector2(0.5f, 0.5f);
            iconRt.sizeDelta = new Vector2(52, 52);
            iconRt.anchoredPosition = new Vector2(42, 0);

            GameObject nameObj = new GameObject("Name", typeof(RectTransform));
            nameObj.transform.SetParent(itemObj.transform, false);
            Text nameText = nameObj.AddComponent<Text>();
            nameText.font = font;
            nameText.fontSize = 12;
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.text = insignia.name.ToUpper();
            nameText.color = rarityColor;
            RectTransform nRt = nameObj.GetComponent<RectTransform>();
            nRt.anchorMin = new Vector2(0.2f, 0.35f);
            nRt.anchorMax = new Vector2(0.95f, 0.85f);
            nRt.sizeDelta = Vector2.zero;

            GameObject rarityObj = new GameObject("Rarity", typeof(RectTransform));
            rarityObj.transform.SetParent(itemObj.transform, false);
            Text rarityText = rarityObj.AddComponent<Text>();
            rarityText.font = font;
            rarityText.fontSize = 8;
            rarityText.alignment = TextAnchor.MiddleCenter;
            rarityText.text = insignia.rarity.ToUpper();
            rarityText.color = new Color(rarityColor.r * 0.7f, rarityColor.g * 0.7f, rarityColor.b * 0.7f);
            RectTransform rRt = rarityObj.GetComponent<RectTransform>();
            rRt.anchorMin = new Vector2(0.2f, 0.02f);
            rRt.anchorMax = new Vector2(0.95f, 0.28f);
            rRt.sizeDelta = Vector2.zero;

            if (isDup)
            {
                GameObject dupObj = new GameObject("Duplicate", typeof(RectTransform));
                dupObj.transform.SetParent(itemObj.transform, false);
                Text dupText = dupObj.AddComponent<Text>();
                dupText.font = font;
                dupText.fontSize = 16;
                dupText.alignment = TextAnchor.MiddleCenter;
                dupText.text = "DUPLICADA!";
                dupText.color = new Color(1f, 0.2f, 0.2f);
                RectTransform dRt = dupObj.GetComponent<RectTransform>();
                dRt.anchorMin = new Vector2(0f, 0f);
                dRt.anchorMax = new Vector2(1f, 1f);
                dRt.sizeDelta = Vector2.zero;

                Outline dupOutline = dupObj.AddComponent<Outline>();
                dupOutline.effectColor = Color.black;
                dupOutline.effectDistance = new Vector2(1, -1);
            }

            GameObject flashObj = new GameObject("Flash", typeof(RectTransform));
            flashObj.transform.SetParent(itemObj.transform, false);
            Image flash = flashObj.AddComponent<Image>();
            flash.color = new Color(1f, 1f, 1f, 0f);
            flash.raycastTarget = false;
            RectTransform flashRt = flashObj.GetComponent<RectTransform>();
            flashRt.anchorMin = Vector2.zero;
            flashRt.anchorMax = Vector2.one;
            flashRt.sizeDelta = Vector2.zero;

            flash.color = new Color(1f, 1f, 1f, 0.8f);
            float flashT = 0;
            while (flashT < 0.3f)
            {
                flashT += Time.deltaTime;
                flash.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.8f, 0f, flashT / 0.3f));
                yield return null;
            }
            flash.color = new Color(1f, 1f, 1f, 0f);

            float itemCenterY = yStart - i * 85 - 34f;
            for (int s2 = 0; s2 < 8; s2++)
            {
                GameObject insSpark = new GameObject("InsSpark", typeof(RectTransform));
                insSpark.transform.SetParent(contentObj.transform, false);
                Image isImg = insSpark.AddComponent<Image>();
                isImg.sprite = circleSprite;
                isImg.color = rarityColor;
                isImg.raycastTarget = false;
                RectTransform isRt = insSpark.GetComponent<RectTransform>();
                isRt.anchorMin = new Vector2(0.5f, 0.5f);
                isRt.anchorMax = new Vector2(0.5f, 0.5f);
                isRt.sizeDelta = new Vector2(6, 6);
                isRt.anchoredPosition = new Vector2(Random.Range(-250f, 250f), itemCenterY + Random.Range(-35f, 35f));
                Vector2 isDir = new Vector2(Random.Range(-20f, 20f), Random.Range(-30f, 30f));
                StartCoroutine(AnimateSparkBurst(isRt, isRt.anchoredPosition + isDir));
            }

            yield return new WaitForSeconds(0.45f);
        }

        GameObject closeBtnObj = new GameObject("CloseBtn", typeof(RectTransform));
        closeBtnObj.transform.SetParent(contentObj.transform, false);
        Image closeBtnImg = closeBtnObj.AddComponent<Image>();
        closeBtnImg.color = new Color(0.4f, 0.2f, 0.1f, 0.85f);
        RectTransform closeBtnRt = closeBtnObj.GetComponent<RectTransform>();
        closeBtnRt.anchorMin = new Vector2(0.5f, 0f);
        closeBtnRt.anchorMax = new Vector2(0.5f, 0f);
        closeBtnRt.pivot = new Vector2(0.5f, 0.5f);
        closeBtnRt.sizeDelta = new Vector2(160, 40);
        closeBtnRt.anchoredPosition = new Vector2(0, 25);
        Vector3 cbOrig = closeBtnRt.localScale;
        closeBtnRt.localScale = Vector3.zero;
        float cbT = 0;
        while (cbT < 0.25f)
        {
            cbT += Time.deltaTime;
            float s = Mathf.Lerp(0f, 1f, cbT / 0.25f);
            closeBtnRt.localScale = new Vector3(s, s, 1);
            yield return null;
        }
        closeBtnRt.localScale = cbOrig;

        GameObject closeTextObj = new GameObject("Text", typeof(RectTransform));
        closeTextObj.transform.SetParent(closeBtnObj.transform, false);
        Text closeText = closeTextObj.AddComponent<Text>();
        closeText.font = font;
        closeText.fontSize = 12;
        closeText.alignment = TextAnchor.MiddleCenter;
        closeText.text = "OK";
        closeText.color = Color.white;
        RectTransform ctRt = closeTextObj.GetComponent<RectTransform>();
        ctRt.anchorMin = Vector2.zero;
        ctRt.anchorMax = Vector2.one;
        ctRt.sizeDelta = Vector2.zero;

        Button closeBtn = closeBtnObj.AddComponent<Button>();
        closeBtn.targetGraphic = closeBtnImg;
        closeBtn.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlayButton();
            Destroy(popupObj);
        });
    }

    IEnumerator AnimateSparkBurst(RectTransform rt, Vector2 target)
    {
        if (rt == null) yield break;
        Vector2 start = rt.anchoredPosition;
        Image img = rt.GetComponent<Image>();
        if (img == null) yield break;
        float t = 0;
        float dur = Random.Range(0.25f, 0.45f);
        Color c = img.color;
        while (t < dur)
        {
            if (rt == null) yield break;
            t += Time.deltaTime;
            rt.anchoredPosition = Vector2.Lerp(start, target, t / dur);
            img.color = new Color(c.r, c.g, c.b, Mathf.Lerp(1f, 0f, t / dur));
            yield return null;
        }
        if (rt != null) Destroy(rt.gameObject);
    }

    IEnumerator AnimateFloatingParticle(RectTransform rt, Vector2 direction, float life)
    {
        if (rt == null) yield break;
        Image img = rt.GetComponent<Image>();
        if (img == null) yield break;
        float t = 0;
        Vector2 start = rt.anchoredPosition;
        Color c = img.color;
        while (t < life)
        {
            if (rt == null) yield break;
            t += Time.deltaTime;
            float p = t / life;
            rt.anchoredPosition = start + direction * p + new Vector2(0, Mathf.Sin(p * Mathf.PI * 3f) * 15f);
            img.color = new Color(c.r, c.g, c.b, Mathf.Lerp(1f, 0f, p));
            yield return null;
        }
        if (rt != null) Destroy(rt.gameObject);
    }

    Color GetRarityColor(string rarity)
    {
        switch (rarity)
        {
            case "common": return new Color(0.7f, 0.7f, 0.7f);
            case "rare": return new Color(0.3f, 0.5f, 1f);
            case "epic": return new Color(0.7f, 0.3f, 0.9f);
            case "legendary": return new Color(1f, 0.75f, 0.1f);
            default: return Color.white;
        }
    }

    void RefreshSlots()
    {
        if (slotObjs[0] != null) Destroy(slotObjs[0]);
        if (slotObjs[1] != null) Destroy(slotObjs[1]);
        slotTimerTexts[0] = null;
        slotTimerTexts[1] = null;
        slotProgressBars[0] = null;
        slotProgressBars[1] = null;
        CreateSlots();
    }

    IEnumerator UpdateTimers()
    {
        while (updating)
        {
            for (int i = 0; i < 2; i++)
            {
                if (slotTimerTexts[i] != null)
                {
                    float hours = ChestManager.GetSlotTimeRemainingHours(i);
                    if (hours <= 0)
                    {
                        slotTimerTexts[i].text = "READY!";
                        slotTimerTexts[i].color = new Color(0.3f, 1f, 0.3f);
                        RefreshSlots();
                    }
                    else
                    {
                        int h = Mathf.FloorToInt(hours);
                        int m = Mathf.FloorToInt((hours - h) * 60f);
                        slotTimerTexts[i].text = $"{h}h {m}m";
                    }
                }
                if (slotProgressBars[i] != null)
                {
                    float progress = ChestManager.GetSlotProgress(i);
                    slotProgressBars[i].rectTransform.anchorMax = new Vector2(progress, 1);
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    void CreateBackButton()
    {
        Sprite[] backSprites = Resources.LoadAll<Sprite>("Sprites/Menu/Retry");
        Sprite backSprite = System.Array.Find(backSprites, s => s.name == "Retry_0");

        GameObject btnObj = new GameObject("BackButton");
        btnObj.transform.SetParent(panelObj.transform, false);
        RectTransform btnRt = btnObj.AddComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0f, 1f);
        btnRt.anchorMax = new Vector2(0f, 1f);
        btnRt.pivot = new Vector2(0f, 1f);
        btnRt.sizeDelta = new Vector2(80, 45);
        btnRt.anchoredPosition = new Vector2(15, -10);

        Image btnImg = btnObj.AddComponent<Image>();
        if (backSprite != null)
            btnImg.sprite = backSprite;
        else
            btnImg.color = new Color(0.4f, 0.2f, 0.1f, 0.85f);
        btnImg.preserveAspect = true;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.font = font;
        text.fontSize = 12;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = "BACK";
        text.color = Color.white;
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(() => { SoundManager.Instance.PlayButton(); Hide(); });
    }
}
