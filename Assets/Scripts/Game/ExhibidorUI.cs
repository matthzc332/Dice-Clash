using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ExhibidorUI : MonoBehaviour
{
    private GameObject rootPanel;
    private GameObject contentRoot;
    private Canvas canvas;
    private Font pressStart;
    private string currentView = "";
    public System.Action OnClose;

    private GameObject[] slotObjs = new GameObject[2];
    private Text[] slotTimerTexts = new Text[2];
    private Image[] slotProgressBars = new Image[2];
    private bool updatingChests;
    private Sprite[] chestSprites;

    private string currentBadgeFilter = "all";

    public void Show(Canvas parentCanvas)
    {
        canvas = parentCanvas;
        pressStart = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        if (pressStart == null) pressStart = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        CreatePanel();
        ShowView("badges");
        updatingChests = true;
        StartCoroutine(UpdateChestTimers());
    }

    public void Close()
    {
        updatingChests = false;
        if (rootPanel != null) Destroy(rootPanel);
        rootPanel = null;
        contentRoot = null;
        OnClose?.Invoke();
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

    void CreatePanel()
    {
        rootPanel = new GameObject("BotinPanel");
        rootPanel.transform.SetParent(canvas.transform, false);

        RectTransform rt = rootPanel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image bg = rootPanel.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.06f, 0.12f, 0.95f);

        CreateMainPanel();
        CreateLeftButtons();
    }

    void CreateMainPanel()
    {
        GameObject panelObj = new GameObject("PanelTotal");
        panelObj.transform.SetParent(rootPanel.transform, false);

        RectTransform rt = panelObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1000, 661);
        rt.anchoredPosition = new Vector2(80, 0);

        Image img = panelObj.AddComponent<Image>();
        img.sprite = LoadFirstSprite("Sprites/Menu/botin ui/panel total", "panel total_0");
        img.preserveAspect = true;

        contentRoot = new GameObject("Content");
        contentRoot.transform.SetParent(panelObj.transform, false);
        RectTransform crt = contentRoot.AddComponent<RectTransform>();
        crt.anchorMin = Vector2.zero;
        crt.anchorMax = Vector2.one;
        crt.offsetMin = new Vector2(40, 30);
        crt.offsetMax = new Vector2(-40, -30);
    }

    void CreateLeftButtons()
    {
        CreateNavButton("Sprites/Menu/botin ui/panel total chests", "panel total chests_0", new Vector2(-700, 260), () => ShowView("chests"));
        CreateNavButton("Sprites/Menu/botin ui/panel total trofeos", "panel total trofeos_0", new Vector2(-700, 140), () => ShowView("trophies"));
        CreateNavButton("Sprites/Menu/botin ui/panel total ribbons", "panel total ribbons_0", new Vector2(-700, 20), () => ShowView("ribbons"));
        CreateNavButton("Sprites/Menu/botin ui/panel total badges", "panel total badges_0", new Vector2(-700, -100), () => ShowView("badges"));
        CreateNavButton("Sprites/Menu/botin ui/panel total settings", "panel total settings_0", new Vector2(-700, -220), () => ShowView("settings"));
        CreateNavButton("Sprites/Menu/botin ui/panel total back", "panel total back_0", new Vector2(-700, 370), Close);
    }

    void CreateNavButton(string path, string spriteName, Vector2 pos, System.Action action)
    {
        GameObject btnObj = new GameObject("NavButton");
        btnObj.transform.SetParent(rootPanel.transform, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(280, 135);
        rt.anchoredPosition = pos;

        Image img = btnObj.AddComponent<Image>();
        img.sprite = LoadFirstSprite(path, spriteName);
        img.preserveAspect = true;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySelect();
            action();
        });
    }

    void ShowView(string view)
    {
        if (currentView == view) return;
        currentView = view;
        ClearContent();
        switch (view)
        {
            case "badges": BuildBadgesView(); break;
            case "chests": BuildChestsView(); break;
            case "trophies": BuildTrophiesView(); break;
            case "ribbons": BuildRibbonsView(); break;
            case "settings": BuildSettingsView(); break;
        }
        CreateBlueFlagCover();
    }

    void ClearContent()
    {
        if (contentRoot == null) return;
        foreach (Transform child in contentRoot.transform)
            Destroy(child.gameObject);
    }

    void BuildBadgesView()
    {
        ClearContent();

        CreateViewTitle("BADGES");

        int collected = InsigniaManager.GetCollectedCount();
        int total = InsigniaManager.GetTotalCount();
        CreateText($"{collected}/{total}", new Vector2(0, 225), 12, new Color(0.8f, 0.7f, 0.4f));

        CreateBadgeFilters();
        CreateBadgeScroll();
    }

    void CreateBadgeFilters()
    {
        string[] filters = { "all", "campaign", "chest" };
        string[] labels = { "ALL", "CAMPAIGN", "CHESTS" };
        float startX = -210f;
        float spacing = 210f;

        for (int i = 0; i < filters.Length; i++)
        {
            string filter = filters[i];
            GameObject btnObj = CreateButton(labels[i], new Vector2(startX + i * spacing, 165), new Vector2(180, 35), contentRoot.transform);
            Image btnImg = btnObj.GetComponent<Image>();
            btnImg.color = currentBadgeFilter == filter
                ? new Color(0.5f, 0.4f, 0.2f, 0.9f)
                : new Color(0.2f, 0.15f, 0.1f, 0.7f);
            Text label = btnObj.GetComponentInChildren<Text>();
            if (label != null) label.color = currentBadgeFilter == filter ? new Color(1f, 0.85f, 0.3f) : new Color(0.6f, 0.55f, 0.45f);

            btnObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySelect();
                currentBadgeFilter = filter;
                BuildBadgesView();
            });
        }
    }

    void CreateBadgeScroll()
    {
        GameObject scrollObj = new GameObject("BadgeScroll");
        scrollObj.transform.SetParent(contentRoot.transform, false);
        RectTransform scrollRt = scrollObj.AddComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0f, 0f);
        scrollRt.anchorMax = new Vector2(1f, 1f);
        scrollRt.offsetMin = new Vector2(20, 90);
        scrollRt.offsetMax = new Vector2(-20, -160);

        ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.scrollSensitivity = 25f;

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObj.transform, false);
        RectTransform vpRt = viewport.AddComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.sizeDelta = Vector2.zero;
        vpRt.offsetMin = Vector2.zero;
        vpRt.offsetMax = Vector2.zero;
        Image vpImg = viewport.AddComponent<Image>();
        vpImg.color = new Color(0, 0, 0, 0.01f);
        vpImg.raycastTarget = false;
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        scroll.viewport = vpRt;

        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewport.transform, false);
        RectTransform crt = contentObj.AddComponent<RectTransform>();
        crt.anchorMin = new Vector2(0f, 1f);
        crt.anchorMax = new Vector2(1f, 1f);
        crt.pivot = new Vector2(0.5f, 1f);
        crt.sizeDelta = new Vector2(0, 0);
        scroll.content = crt;

        PopulateBadgeGrid(crt);
    }

    void CreateBlueFlagCover()
    {
        Sprite flagSprite = Resources.Load<Sprite>("Sprites/Decor/BlueFlag");
        if (flagSprite == null) return;

        GameObject flagObj = new GameObject("BlueFlagCover");
        flagObj.transform.SetParent(contentRoot.transform, false);
        Image flagImg = flagObj.AddComponent<Image>();
        flagImg.sprite = flagSprite;
        flagImg.color = Color.white;
        flagImg.raycastTarget = false;
        RectTransform flagRt = flagObj.GetComponent<RectTransform>();
        flagRt.anchorMin = new Vector2(0.5f, 0f);
        flagRt.anchorMax = new Vector2(0.5f, 0f);
        flagRt.pivot = new Vector2(0.5f, 0f);
        flagRt.sizeDelta = new Vector2(326, 269);
        flagRt.anchoredPosition = new Vector2(-1, -204);
    }

    void PopulateBadgeGrid(RectTransform content)
    {
        Insignia[] allInsignias;
        bool showTutorial = currentBadgeFilter == "all";
        if (currentBadgeFilter == "all")
        {
            var data = InsigniaData.Load();
            allInsignias = data?.insignias ?? new Insignia[0];
        }
        else
        {
            allInsignias = InsigniaData.GetBySource(currentBadgeFilter);
        }

        int totalItems = allInsignias.Length + (showTutorial ? 1 : 0);
        if (totalItems == 0)
        {
            CreateText("No badges found.", new Vector2(0, 0), 12, Color.gray, content);
            return;
        }

        float cardW = 150f;
        float cardH = 90f;
        float gapX = 10f;
        float gapY = 10f;
        int cols = 5;

        float totalWidth = cols * cardW + (cols - 1) * gapX;
        float startX = -totalWidth / 2f + cardW / 2f;

        for (int idx = 0; idx < totalItems; idx++)
        {
            int localIndex = idx;
            int row = localIndex / cols;
            int col = localIndex % cols;

            float x = startX + col * (cardW + gapX);
            float y = -row * (cardH + gapY) - 10f;

            if (showTutorial && idx == 0)
            {
                CreateTutorialBadgeCard(content, x, y, cardW, cardH);
            }
            else
            {
                int insIndex = showTutorial ? idx - 1 : idx;
                Insignia ins = allInsignias[insIndex];
                bool collected = InsigniaManager.HasInsignia(ins.id);
                CreateBadgeCard(content, ins, collected, x, y, cardW, cardH);
            }
        }

        int cardsOnPage = totalItems;
        int totalRows = Mathf.CeilToInt((float)cardsOnPage / cols);
        float contentHeight = totalRows * (cardH + gapY) + 20f;
        if (contentHeight < 10) contentHeight = 10; // Avoid tiny content heights
        content.sizeDelta = new Vector2(0, contentHeight);
    }

    void CreateTutorialBadgeCard(Transform parent, float x, float y, float w, float h)
    {
        bool earned = TutorialCollectibles.HasEarned();

        GameObject cardObj = new GameObject("Card_tutorial");
        cardObj.transform.SetParent(parent, false);
        Image cardBg = cardObj.AddComponent<Image>();
        cardBg.sprite = LoadFirstSprite("Sprites/Menu/panelCartaBlue", "panelCartaBlue");
        cardBg.color = earned ? Color.white : new Color(0.35f, 0.35f, 0.35f, 0.8f);
        RectTransform cardRt = cardObj.GetComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 1f);
        cardRt.anchorMax = new Vector2(0.5f, 1f);
        cardRt.pivot = new Vector2(0.5f, 1f);
        cardRt.sizeDelta = new Vector2(w, h);
        cardRt.anchoredPosition = new Vector2(x, y);

        Button cardBtn = cardObj.AddComponent<Button>();
        cardBtn.targetGraphic = cardBg;
        if (earned)
        {
            Insignia tutorialBadge = new Insignia
            {
                id = "tutorial",
                name = "Training",
                description = "Complete the Tutorial",
                rarity = "common",
                source = "tutorial"
            };
            cardBtn.onClick.AddListener(() => InsigniaDetailPopup.Show(canvas, pressStart, tutorialBadge, true));
        }

        if (earned)
        {
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(cardObj.transform, false);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = TutorialCollectibles.GetInsigniaSprite();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            RectTransform iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.06f, 0.2f);
            iconRt.anchorMax = new Vector2(0.3f, 0.85f);
            iconRt.sizeDelta = Vector2.zero;
        }

        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(cardObj.transform, false);
        Text nameText = nameObj.AddComponent<Text>();
        nameText.font = pressStart;
        nameText.fontSize = 7;
        nameText.alignment = earned ? TextAnchor.MiddleLeft : TextAnchor.MiddleCenter;
        nameText.text = earned ? "TUTORIAL" : "???";
        nameText.color = earned ? new Color(0.9f, 0.8f, 0.4f) : new Color(0.3f, 0.3f, 0.3f);
        RectTransform nRt = nameObj.GetComponent<RectTransform>();
        if (earned)
        {
            nRt.anchorMin = new Vector2(0.34f, 0.5f);
            nRt.anchorMax = new Vector2(0.95f, 0.9f);
        }
        else
        {
            nRt.anchorMin = new Vector2(0f, 0.3f);
            nRt.anchorMax = new Vector2(1f, 0.7f);
        }
        nRt.sizeDelta = Vector2.zero;

        if (earned)
        {
            GameObject descObj = new GameObject("Desc");
            descObj.transform.SetParent(cardObj.transform, false);
            Text descText = descObj.AddComponent<Text>();
            descText.font = pressStart;
            descText.fontSize = 5;
            descText.alignment = TextAnchor.MiddleLeft;
            descText.text = "Complete the Tutorial";
            descText.color = new Color(0.6f, 0.55f, 0.45f);
            RectTransform dRt = descObj.GetComponent<RectTransform>();
            dRt.anchorMin = new Vector2(0.34f, 0.05f);
            dRt.anchorMax = new Vector2(0.95f, 0.5f);
            dRt.sizeDelta = Vector2.zero;
        }

        GameObject rarityObj = new GameObject("Rarity");
        rarityObj.transform.SetParent(cardObj.transform, false);
        Text rarityText = rarityObj.AddComponent<Text>();
        rarityText.font = pressStart;
        rarityText.fontSize = 5;
        rarityText.alignment = TextAnchor.MiddleCenter;
        rarityText.text = "TUTORIAL";
        rarityText.color = earned
            ? new Color(0.75f, 0.68f, 0.45f)
            : new Color(0.25f, 0.25f, 0.25f);
        RectTransform rRt = rarityObj.GetComponent<RectTransform>();
        rRt.anchorMin = new Vector2(0f, 0f);
        rRt.anchorMax = new Vector2(1f, 0.18f);
        rRt.sizeDelta = Vector2.zero;

        if (earned)
        {
            GameObject checkObj = new GameObject("Check");
            checkObj.transform.SetParent(cardObj.transform, false);
            Text checkText = checkObj.AddComponent<Text>();
            checkText.font = pressStart;
            checkText.fontSize = 9;
            checkText.alignment = TextAnchor.MiddleCenter;
            checkText.text = "\u2713";
            checkText.color = new Color(0.3f, 0.9f, 0.3f);
            RectTransform checkRt = checkObj.GetComponent<RectTransform>();
            checkRt.anchorMin = new Vector2(0.82f, 0.72f);
            checkRt.anchorMax = new Vector2(1f, 1f);
            checkRt.sizeDelta = Vector2.zero;
        }
    }

    void CreateBadgeCard(Transform parent, Insignia ins, bool collected, float x, float y, float w, float h)
    {
        Color rarityColor = GetRarityColor(ins.rarity);

        GameObject cardObj = new GameObject($"Card_{ins.id}");
        cardObj.transform.SetParent(parent, false);
        Image cardBg = cardObj.AddComponent<Image>();
        cardBg.sprite = LoadFirstSprite("Sprites/Menu/panelCartaBlue", "panelCartaBlue");
        if (collected)
        {
            if (ins.rarity.ToLower() == "common")
                cardBg.color = new Color(0.65f, 0.65f, 0.65f, 0.9f);
            else
                cardBg.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, 0.85f);
        }
        else
        {
            cardBg.color = new Color(0.35f, 0.35f, 0.35f, 0.8f);
        }
        Outline cardOutline = cardObj.AddComponent<Outline>();
        cardOutline.effectColor = collected ? rarityColor : new Color(0.3f, 0.3f, 0.3f);
        cardOutline.effectDistance = new Vector2(2, -2);
        RectTransform cardRt = cardObj.GetComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 1f);
        cardRt.anchorMax = new Vector2(0.5f, 1f);
        cardRt.pivot = new Vector2(0.5f, 1f);
        cardRt.sizeDelta = new Vector2(w, h);
        cardRt.anchoredPosition = new Vector2(x, y);

        Button cardBtn = cardObj.AddComponent<Button>();
        cardBtn.targetGraphic = cardBg;
        cardBtn.onClick.AddListener(() => InsigniaDetailPopup.Show(canvas, pressStart, ins, collected));

        if (collected)
        {
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(cardObj.transform, false);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = InsigniaSprites.Get(ins);
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            RectTransform iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.06f, 0.2f);
            iconRt.anchorMax = new Vector2(0.3f, 0.85f);
            iconRt.sizeDelta = Vector2.zero;
        }

        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(cardObj.transform, false);
        Text nameText = nameObj.AddComponent<Text>();
        nameText.font = pressStart;
        nameText.fontSize = 8;
        nameText.alignment = collected ? TextAnchor.MiddleLeft : TextAnchor.MiddleCenter;
        nameText.text = collected ? ins.name.ToUpper() : "???";
        nameText.color = collected ? Color.black : new Color(0.3f, 0.3f, 0.3f);
        if (collected)
        {
            Outline nameOutline = nameObj.AddComponent<Outline>();
            nameOutline.effectColor = new Color(1f, 1f, 1f, 0.5f);
            nameOutline.effectDistance = new Vector2(1, -1);
        }
        RectTransform nRt = nameObj.GetComponent<RectTransform>();
        if (collected)
        {
            nRt.anchorMin = new Vector2(0.34f, 0.38f);
            nRt.anchorMax = new Vector2(0.95f, 0.78f);
        }
        else
        {
            nRt.anchorMin = new Vector2(0f, 0.3f);
            nRt.anchorMax = new Vector2(1f, 0.7f);
        }
        nRt.sizeDelta = Vector2.zero;

        if (collected)
        {
            GameObject descObj = new GameObject("Desc");
            descObj.transform.SetParent(cardObj.transform, false);
            Text descText = descObj.AddComponent<Text>();
            descText.font = pressStart;
            descText.fontSize = 6;
            descText.alignment = TextAnchor.MiddleLeft;
            descText.text = ins.description;
            descText.color = Color.black;
            Outline descOutline = descObj.AddComponent<Outline>();
            descOutline.effectColor = new Color(1f, 1f, 1f, 0.4f);
            descOutline.effectDistance = new Vector2(1, -1);
            RectTransform dRt = descObj.GetComponent<RectTransform>();
            dRt.anchorMin = new Vector2(0.34f, 0.12f);
            dRt.anchorMax = new Vector2(0.95f, 0.42f);
            dRt.sizeDelta = Vector2.zero;
        }

        GameObject rarityObj = new GameObject("Rarity");
        rarityObj.transform.SetParent(cardObj.transform, false);
        Text rarityText = rarityObj.AddComponent<Text>();
        rarityText.font = pressStart;
        rarityText.fontSize = 6;
        rarityText.alignment = TextAnchor.MiddleCenter;
        rarityText.text = ins.rarity.ToUpper();
        rarityText.color = Color.black;
        if (collected)
        {
            Outline rarityOutline = rarityObj.AddComponent<Outline>();
            rarityOutline.effectColor = new Color(1f, 1f, 1f, 0.4f);
            rarityOutline.effectDistance = new Vector2(1, -1);
        }
        RectTransform rRt = rarityObj.GetComponent<RectTransform>();
        rRt.anchorMin = new Vector2(0f, 0f);
        rRt.anchorMax = new Vector2(1f, 0.22f);
        rRt.sizeDelta = Vector2.zero;

        if (collected)
        {
            GameObject checkObj = new GameObject("Check");
            checkObj.transform.SetParent(cardObj.transform, false);
            Text checkText = checkObj.AddComponent<Text>();
            checkText.font = pressStart;
            checkText.fontSize = 9;
            checkText.alignment = TextAnchor.MiddleCenter;
            checkText.text = "\u2713";
            checkText.color = new Color(0.3f, 0.9f, 0.3f);
            RectTransform checkRt = checkObj.GetComponent<RectTransform>();
            checkRt.anchorMin = new Vector2(0.82f, 0.72f);
            checkRt.anchorMax = new Vector2(1f, 1f);
            checkRt.sizeDelta = Vector2.zero;
        }
    }

    void BuildChestsView()
    {
        CreateViewTitle("CHESTS");
        CreateChestSlots();
        CreateTestChestButton();
    }

    void CreateChestSlots()
    {
        float[] slotX = { -240f, 240f };

        for (int i = 0; i < 2; i++)
        {
            ChestSlot slot = ChestManager.Slots[i];
            GameObject slotObj = new GameObject($"Slot_{i}");
            slotObj.transform.SetParent(contentRoot.transform, false);
            slotObjs[i] = slotObj;
            RectTransform slotRt = slotObj.AddComponent<RectTransform>();
            slotRt.anchorMin = new Vector2(0.5f, 0.5f);
            slotRt.anchorMax = new Vector2(0.5f, 0.5f);
            slotRt.pivot = new Vector2(0.5f, 0.5f);
            slotRt.sizeDelta = new Vector2(420, 430);
            slotRt.anchoredPosition = new Vector2(slotX[i], -10);

            Image slotBg = slotObj.AddComponent<Image>();
            slotBg.sprite = LoadFirstSprite("Sprites/Menu/panelCartaRed", "panelCartaRed");
            slotBg.preserveAspect = true;
            slotBg.color = Color.white;

            if (!slot.occupied) CreateEmptySlot(slotObj.transform);
            else if (slot.ready) CreateReadySlot(slotObj.transform, i);
            else CreateLockedSlot(slotObj.transform, i);
        }
    }

    void CreateEmptySlot(Transform parent)
    {
        GameObject emptyObj = new GameObject("Empty");
        emptyObj.transform.SetParent(parent, false);
        Text emptyText = emptyObj.AddComponent<Text>();
        emptyText.font = pressStart;
        emptyText.fontSize = 14;
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
        auraRt.anchoredPosition = new Vector2(0, 60);
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
        cRt.anchoredPosition = new Vector2(0, 60);

        bool hovering = false;
        EventTrigger trigger = parent.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry enter = new EventTrigger.Entry();
        enter.eventID = EventTriggerType.PointerEnter;
        enter.callback.AddListener((BaseEventData data) => { hovering = true; StartCoroutine(HoverGrow(chestObj.transform, 1.25f)); });
        trigger.triggers.Add(enter);
        EventTrigger.Entry exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;
        exit.callback.AddListener((BaseEventData data) => { hovering = false; StartCoroutine(HoverGrow(chestObj.transform, 1f)); });
        trigger.triggers.Add(exit);

        StartCoroutine(HeartbeatPulse(chestObj.transform, () => hovering));

        Button chestBtn = chestObj.AddComponent<Button>();
        chestBtn.targetGraphic = chestImg;
        int capturedIndex = index;
        chestBtn.onClick.AddListener(() => OnOpenClicked(capturedIndex));

        CreateOpenButton(parent, index);
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
        cRt.sizeDelta = new Vector2(130, 130);
        cRt.anchoredPosition = new Vector2(0, 55);

        GameObject timerObj = new GameObject("Timer");
        timerObj.transform.SetParent(parent, false);
        Text timerText = timerObj.AddComponent<Text>();
        timerText.font = pressStart;
        timerText.fontSize = 11;
        timerText.alignment = TextAnchor.MiddleCenter;
        timerText.color = new Color(0.8f, 0.7f, 0.4f);
        slotTimerTexts[index] = timerText;
        RectTransform tRt = timerObj.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0f, 0.33f);
        tRt.anchorMax = new Vector2(1f, 0.45f);
        tRt.sizeDelta = Vector2.zero;

        GameObject progressBg = new GameObject("ProgressBg");
        progressBg.transform.SetParent(parent, false);
        Image pBgImg = progressBg.AddComponent<Image>();
        pBgImg.color = new Color(0.2f, 0.15f, 0.1f);
        pBgImg.raycastTarget = false;
        RectTransform pBgRt = progressBg.GetComponent<RectTransform>();
        pBgRt.anchorMin = new Vector2(0.15f, 0.26f);
        pBgRt.anchorMax = new Vector2(0.85f, 0.3f);
        pBgRt.sizeDelta = Vector2.zero;

        GameObject progressFill = new GameObject("ProgressFill");
        progressFill.transform.SetParent(progressBg.transform, false);
        Image pFillImg = progressFill.AddComponent<Image>();
        pFillImg.color = new Color(0.8f, 0.6f, 0.1f);
        pFillImg.raycastTarget = false;
        slotProgressBars[index] = pFillImg;
        RectTransform pFillRt = progressFill.GetComponent<RectTransform>();
        pFillRt.anchorMin = Vector2.zero;
        pFillRt.anchorMax = new Vector2(ChestManager.GetSlotProgress(index), 1);
        pFillRt.sizeDelta = Vector2.zero;
    }

    void CreateOpenButton(Transform parent, int index)
    {
        GameObject btnObj = new GameObject("OpenBtn");
        btnObj.transform.SetParent(parent, false);
        Image btnImg = btnObj.AddComponent<Image>();
        Sprite openSprite = LoadFirstSprite("Sprites/Menu/botin ui/botonOpen", "botonOpen_0");
        if (openSprite != null)
        {
            btnImg.sprite = openSprite;
            btnImg.preserveAspect = true;
            btnImg.color = Color.white;
        }
        else
        {
            btnImg.color = new Color(0.3f, 0.6f, 0.3f, 0.9f);
        }
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0f);
        btnRt.anchorMax = new Vector2(0.5f, 0f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.sizeDelta = new Vector2(170, 60);
        btnRt.anchoredPosition = new Vector2(0, -45);

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        int capturedIndex = index;
        btn.onClick.AddListener(() => OnOpenClicked(capturedIndex));
        StartCoroutine(HeartbeatPulse(btnObj.transform, () => false));
    }

    void OnOpenClicked(int slotIndex)
    {
        SoundManager.Instance.PlaySelect();
        ChestReward reward = ChestManager.OpenChest(slotIndex);
        if (reward.insignias.Count > 0 || reward.gold > 0)
        {
            StartCoroutine(ShowRewardPopup(reward));
            RefreshChestSlots();
        }
    }

    void CreateTestChestButton()
    {
        GameObject btnObj = CreateButton("TEST CHEST", new Vector2(0, -255), new Vector2(190, 40), contentRoot.transform);
        btnObj.GetComponent<Image>().color = new Color(0.3f, 0.2f, 0.45f, 0.9f);
        Text label = btnObj.GetComponentInChildren<Text>();
        if (label != null) label.fontSize = 9;
        btnObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySelect();
            if (ChestManager.AddReadyChestForTest())
                RefreshChestSlots();
        });
    }

    void RefreshChestSlots()
    {
        if (currentView != "chests" || contentRoot == null) return;
        for (int i = 0; i < 2; i++)
        {
            if (slotObjs[i] != null) Destroy(slotObjs[i]);
            slotObjs[i] = null;
            slotTimerTexts[i] = null;
            slotProgressBars[i] = null;
        }
        CreateChestSlots();
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

    IEnumerator HeartbeatPulse(Transform t, System.Func<bool> hovering)
    {
        float t0 = 0;
        while (t != null)
        {
            t0 += Time.deltaTime;
            if (!hovering())
            {
                float s = 1f + Mathf.Sin(t0 * 3.5f) * 0.04f;
                t.localScale = new Vector3(s, s, 1);
            }
            yield return null;
        }
    }

    IEnumerator UpdateChestTimers()
    {
        while (updatingChests)
        {
            if (currentView == "chests")
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
                            RefreshChestSlots();
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
                        slotProgressBars[i].rectTransform.anchorMax = new Vector2(ChestManager.GetSlotProgress(i), 1);
                    }
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator ShowRewardPopup(ChestReward reward)
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
        Sprite contentPanelSprite = LoadFirstSprite("Sprites/Menu/panelCartaBlue", "panelCartaBlue");
        if (contentPanelSprite != null)
        {
            contentBg.sprite = contentPanelSprite;
            contentBg.color = Color.white;
        }
        else
        {
            contentBg.color = new Color(0.12f, 0.08f, 0.04f, 0.95f);
        }
        RectTransform contentRt = contentObj.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0.5f, 0.5f);
        contentRt.anchorMax = new Vector2(0.5f, 0.5f);
        contentRt.sizeDelta = new Vector2(600, 580);

        GameObject chestAnimObj = new GameObject("ChestAnimation", typeof(RectTransform));
        chestAnimObj.transform.SetParent(contentObj.transform, false);
        Image chestAnimImg = chestAnimObj.AddComponent<Image>();
        chestAnimImg.preserveAspect = true;
        RectTransform caRt = chestAnimObj.GetComponent<RectTransform>();
        caRt.anchorMin = new Vector2(0.5f, 0.5f);
        caRt.anchorMax = new Vector2(0.5f, 0.5f);
        caRt.sizeDelta = new Vector2(150, 150);
        caRt.anchoredPosition = new Vector2(0, 120);

        for (int f = 0; f <= 4; f++)
        {
            chestAnimImg.sprite = GetChestSprite(f);
            if (f == 4) SoundManager.Instance.PlayCoin();
            yield return new WaitForSeconds(0.12f);
        }
        Destroy(chestAnimObj);

        GameObject titleObj = new GameObject("Title", typeof(RectTransform));
        titleObj.transform.SetParent(contentObj.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.font = pressStart;
        titleText.fontSize = 20;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.text = "CHEST OPENED!";
        titleText.color = Color.black;
        RectTransform tRt = titleObj.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0f, 0.85f);
        tRt.anchorMax = new Vector2(1f, 0.95f);
        tRt.sizeDelta = Vector2.zero;

        yield return new WaitForSeconds(0.2f);

        if (reward.gold > 0)
        {
            GameObject goldObj = new GameObject("GoldReward", typeof(RectTransform));
            goldObj.transform.SetParent(contentObj.transform, false);
            Text goldText = goldObj.AddComponent<Text>();
            goldText.font = pressStart;
            goldText.fontSize = 22;
            goldText.alignment = TextAnchor.MiddleCenter;
            goldText.text = $"+{reward.gold} GOLD!";
            goldText.color = Color.black;
            RectTransform gRt = goldObj.GetComponent<RectTransform>();
            gRt.anchorMin = new Vector2(0f, 0.72f);
            gRt.anchorMax = new Vector2(1f, 0.82f);
            gRt.sizeDelta = Vector2.zero;
            SoundManager.Instance.PlayCoin();
            yield return new WaitForSeconds(0.5f);
        }

        float yStart = 110f;
        for (int i = 0; i < reward.insignias.Count; i++)
        {
            var insignia = InsigniaData.GetInsignia(reward.insignias[i]);
            if (insignia == null) continue;

            Color rarityColor = GetRarityColor(insignia.rarity);
            bool isDup = reward.isDuplicate[i];

            GameObject itemObj = new GameObject($"Item_{i}", typeof(RectTransform));
            itemObj.transform.SetParent(contentObj.transform, false);
            Image itemBg = itemObj.AddComponent<Image>();
            itemBg.sprite = LoadFirstSprite("Sprites/Menu/panelCartaBlue", "panelCartaBlue");
            if (insignia.rarity.ToLower() == "common")
                itemBg.color = new Color(0.45f, 0.45f, 0.45f, 0.9f);
            else
                itemBg.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, 0.85f);
            RectTransform itemRt = itemObj.GetComponent<RectTransform>();
            itemRt.anchorMin = new Vector2(0.5f, 0.5f);
            itemRt.anchorMax = new Vector2(0.5f, 0.5f);
            itemRt.pivot = new Vector2(0.5f, 1f);
            itemRt.sizeDelta = new Vector2(460, 60);
            itemRt.anchoredPosition = new Vector2(0, yStart - i * 75);

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
            iconRt.sizeDelta = new Vector2(48, 48);
            iconRt.anchoredPosition = new Vector2(40, 0);

            GameObject nameObj = new GameObject("Name", typeof(RectTransform));
            nameObj.transform.SetParent(itemObj.transform, false);
            Text nameText = nameObj.AddComponent<Text>();
            nameText.font = pressStart;
            nameText.fontSize = 11;
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
            rarityText.font = pressStart;
            rarityText.fontSize = 7;
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
                dupText.font = pressStart;
                dupText.fontSize = 14;
                dupText.alignment = TextAnchor.MiddleCenter;
                dupText.text = "DUPLICADA!";
                dupText.color = new Color(1f, 0.2f, 0.2f);
                RectTransform dRt = dupObj.GetComponent<RectTransform>();
                dRt.anchorMin = new Vector2(0f, 0f);
                dRt.anchorMax = new Vector2(1f, 1f);
                dRt.sizeDelta = Vector2.zero;
            }

            yield return new WaitForSeconds(0.4f);
        }

        GameObject closeBtnObj = new GameObject("OK", typeof(RectTransform));
        closeBtnObj.transform.SetParent(contentObj.transform, false);
        Image closeBtnImg = closeBtnObj.AddComponent<Image>();
        Sprite okSprite = LoadFirstSprite("Sprites/Menu/botin ui/botonOK", "botonOK_0");
        if (okSprite != null)
        {
            closeBtnImg.sprite = okSprite;
            closeBtnImg.preserveAspect = true;
            closeBtnImg.color = Color.white;
        }
        else
        {
            closeBtnImg.color = new Color(0.4f, 0.2f, 0.1f, 0.85f);
        }
        RectTransform closeBtnRt = closeBtnObj.GetComponent<RectTransform>();
        closeBtnRt.anchorMin = new Vector2(0.5f, 0.5f);
        closeBtnRt.anchorMax = new Vector2(0.5f, 0.5f);
        closeBtnRt.pivot = new Vector2(0.5f, 0.5f);
        closeBtnRt.anchoredPosition = new Vector2(0, -235);
        closeBtnRt.sizeDelta = new Vector2(200, 65);
        Button closeBtn = closeBtnObj.AddComponent<Button>();
        closeBtn.targetGraphic = closeBtnImg;
        closeBtn.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlayButton();
            Destroy(popupObj);
        });
    }

    void BuildTrophiesView()
    {
        CreateViewTitle("TROPHIES");

        CampaignDataWrapper data = CampaignData.Load();
        if (data == null) return;

        int totalLevels = data.levels != null ? data.levels.Length : 0;
        int completedLevels = CampaignManager.Instance != null ? CampaignManager.Instance.GetCompletedCount() : 0;
        string winsText = $"CAMPAIGN WINS: {completedLevels}/{totalLevels}";
        Color winsColor = completedLevels >= totalLevels ? new Color(1f, 0.84f, 0f) : new Color(0.8f, 0.8f, 0.8f);
        CreateText(winsText, new Vector2(0, 210), 10, winsColor);

        Vector2 tutPos = new Vector2(-161, 76);
        Vector2 tutSize = new Vector2(68, 87);
        bool tutEarned = TutorialCollectibles.HasEarned();
        Sprite tutSprite = TutorialCollectibles.GetCupSprite();
        CreateCupVisual("TUTORIAL", tutSprite, tutEarned, tutPos, tutSize, contentRoot.transform);

        string[] cupNames = { "IRON CROWN", "BLOOD FANG", "WILD HEART", "VOID SEAL" };
        Vector2[] cupPositions = {
            new Vector2(-323, 82),
            new Vector2(5, 77),
            new Vector2(165, 72),
            new Vector2(330, 71)
        };
        Vector2[] cupSizes = {
            new Vector2(85, 108),
            new Vector2(85, 108),
            new Vector2(85, 108),
            new Vector2(96, 125)
        };

        for (int i = 0; i < data.cups.Length; i++)
        {
            CampaignCup cup = data.cups[i];
            bool completed = CampaignManager.Instance != null && CampaignManager.Instance.IsCupCompleted(cup.id);
            CreateCupVisual(cupNames[i], GetCupSprite(i), completed, cupPositions[i], cupSizes[i], contentRoot.transform);
        }
    }

    void BuildRibbonsView()
    {
        ClearContent();
        CreateViewTitle("RIBBONS");

        CreateRibbonScroll();
    }

    void CreateRibbonScroll()
    {
        GameObject scrollObj = new GameObject("RibbonScroll");
        scrollObj.transform.SetParent(contentRoot.transform, false);
        RectTransform scrollRt = scrollObj.AddComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0f, 0f);
        scrollRt.anchorMax = new Vector2(1f, 1f);
        scrollRt.offsetMin = new Vector2(20, 90);
        scrollRt.offsetMax = new Vector2(-20, -160);

        ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.scrollSensitivity = 25f;

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObj.transform, false);
        RectTransform vpRt = viewport.AddComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.sizeDelta = Vector2.zero;
        vpRt.offsetMin = Vector2.zero;
        vpRt.offsetMax = Vector2.zero;
        Image vpImg = viewport.AddComponent<Image>();
        vpImg.color = new Color(0, 0, 0, 0.01f);
        vpImg.raycastTarget = false;
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        scroll.viewport = vpRt;

        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewport.transform, false);
        RectTransform crt = contentObj.AddComponent<RectTransform>();
        crt.anchorMin = new Vector2(0f, 1f);
        crt.anchorMax = new Vector2(1f, 1f);
        crt.pivot = new Vector2(0.5f, 1f);
        crt.sizeDelta = new Vector2(0, 0);
        scroll.content = crt;

        PopulateRibbonGrid(crt);
    }

    void PopulateRibbonGrid(RectTransform content)
    {
        CampaignDataWrapper data = CampaignData.Load();
        if (data == null) return;

        float cardW = 110f;
        float cardH = 130f;
        float gapX = 30f;
        float gapY = 25f;
        int cols = 5;

        float totalWidth = cols * cardW + (cols - 1) * gapX;
        float startX = -totalWidth / 2f + cardW / 2f;

        bool tutorialEarned = TutorialCollectibles.HasEarned();
        CreateRibbonCard(content, null, tutorialEarned,
            new Color(0.62f, 0.42f, 0.24f), startX, -10f, cardW, cardH, true);

        for (int i = 0; i < data.levels.Length; i++)
        {
            CampaignLevel level = data.levels[i];
            bool has = RibbonManager.HasRibbon(level.id);
            Color color = has ? RibbonManager.GetRibbonColor(level.id) : new Color(0.3f, 0.3f, 0.3f, 0.5f);

            int flatIndex = i + 1;
            int row = flatIndex / cols;
            int col = flatIndex % cols;

            float x = startX + col * (cardW + gapX);
            float y = -row * (cardH + gapY) - 10f;

            CreateRibbonCard(content, level, has, color, x, y, cardW, cardH, false);
        }

        int totalRows = Mathf.CeilToInt((float)(data.levels.Length + 1) / cols);
        float contentHeight = totalRows * (cardH + gapY) + 20f;
        content.sizeDelta = new Vector2(0, contentHeight);
    }

    void CreateRibbonCard(Transform parent, CampaignLevel level, bool has, Color color, float x, float y, float w, float h, bool isTutorial)
    {
        GameObject cardObj = new GameObject($"Ribbon_{(isTutorial ? "tutorial" : level.id)}");
        cardObj.transform.SetParent(parent, false);
        Image cardBg = cardObj.AddComponent<Image>();
        cardBg.sprite = LoadFirstSprite("Sprites/Menu/panelCartaBlue", "panelCartaBlue");
        if (has)
            cardBg.color = isTutorial
                ? new Color(0.62f, 0.42f, 0.24f, 0.85f)
                : new Color(color.r, color.g, color.b, 0.85f);
        else
            cardBg.color = new Color(0.35f, 0.35f, 0.35f, 0.7f);
        RectTransform cardRt = cardObj.GetComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 1f);
        cardRt.anchorMax = new Vector2(0.5f, 1f);
        cardRt.pivot = new Vector2(0.5f, 1f);
        cardRt.sizeDelta = new Vector2(w, h);
        cardRt.anchoredPosition = new Vector2(x, y);

        if (has)
        {
            Button cardBtn = cardObj.AddComponent<Button>();
            cardBtn.targetGraphic = cardBg;
            int capturedId = isTutorial ? 0 : level.id;
            cardBtn.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySelect();
                ShowRibbonPopup(capturedId, isTutorial);
            });
        }

        GameObject ribbonObj = new GameObject("Ribbon");
        ribbonObj.transform.SetParent(cardObj.transform, false);
        Image ribbonImg = ribbonObj.AddComponent<Image>();
        ribbonImg.sprite = isTutorial ? TutorialCollectibles.GetRibbonSprite() : RibbonManager.GetRibbonSprite(level.id);
        ribbonImg.preserveAspect = true;
        ribbonImg.raycastTarget = false;
        ribbonImg.color = has ? color : new Color(color.r, color.g, color.b, 0.3f);
        RectTransform rRt = ribbonObj.GetComponent<RectTransform>();
        rRt.anchorMin = new Vector2(0.5f, 0.5f);
        rRt.anchorMax = new Vector2(0.5f, 0.5f);
        rRt.pivot = new Vector2(0.5f, 0.5f);
        rRt.sizeDelta = new Vector2(50, 75);
        rRt.anchoredPosition = new Vector2(0, 10);

        GameObject nameObj = new GameObject("Level");
        nameObj.transform.SetParent(cardObj.transform, false);
        Text nameText = nameObj.AddComponent<Text>();
        nameText.font = pressStart;
        nameText.fontSize = 7;
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.text = has ? (isTutorial ? "TUT" : $"L{level.id}") : "???";
        nameText.color = has ? new Color(1f, 0.85f, 0.3f) : new Color(0.3f, 0.3f, 0.3f);
        RectTransform nRt = nameObj.GetComponent<RectTransform>();
        nRt.anchorMin = new Vector2(0f, 0f);
        nRt.anchorMax = new Vector2(1f, 0.22f);
        nRt.sizeDelta = Vector2.zero;
    }

    void ShowRibbonPopup(int levelId, bool isTutorial)
    {
        GameObject popupObj = new GameObject("RibbonPopup", typeof(RectTransform));
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
        Sprite panelSprite = LoadFirstSprite("Sprites/Menu/panelCartaBlue", "panelCartaBlue");
        if (panelSprite != null) { contentBg.sprite = panelSprite; contentBg.color = Color.white; }
        else { contentBg.color = new Color(0.12f, 0.08f, 0.04f, 0.95f); }
        RectTransform contentRt = contentObj.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0.5f, 0.5f);
        contentRt.anchorMax = new Vector2(0.5f, 0.5f);
        contentRt.sizeDelta = new Vector2(500, 350);

        string title = isTutorial ? "TUTORIAL" : $"LEVEL {levelId}";
        string levelName = "";
        string cupName = "";
        if (!isTutorial)
        {
            CampaignLevel level = CampaignData.GetLevel(levelId);
            if (level != null)
            {
                levelName = level.name;
                CampaignCup cup = CampaignData.GetCup(level.cup);
                if (cup != null) cupName = cup.name.ToUpper();
            }
        }

        Sprite ribbonSprite = isTutorial
            ? TutorialCollectibles.GetRibbonSprite()
            : (levelId > 0 ? RibbonManager.GetRibbonSprite(levelId) : null);
        Color ribbonColor = isTutorial
            ? new Color(0.62f, 0.42f, 0.24f)
            : (levelId > 0 ? RibbonManager.GetRibbonColor(levelId) : Color.white);
        if (ribbonSprite != null)
        {
            GameObject ribbonImgObj = new GameObject("RibbonImage", typeof(RectTransform));
            ribbonImgObj.transform.SetParent(contentObj.transform, false);
            Image ribbonImg = ribbonImgObj.AddComponent<Image>();
            ribbonImg.sprite = ribbonSprite;
            ribbonImg.preserveAspect = true;
            ribbonImg.color = ribbonColor;
            ribbonImg.raycastTarget = false;
            RectTransform rRt = ribbonImgObj.GetComponent<RectTransform>();
            rRt.anchorMin = new Vector2(0.5f, 0.5f);
            rRt.anchorMax = new Vector2(0.5f, 0.5f);
            rRt.sizeDelta = new Vector2(80, 120);
            rRt.anchoredPosition = new Vector2(0, 40);
        }

        GameObject titleObj = new GameObject("Title", typeof(RectTransform));
        titleObj.transform.SetParent(contentObj.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.font = pressStart;
        titleText.fontSize = 16;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.text = title;
        titleText.color = new Color(1f, 0.85f, 0.3f);
        RectTransform tRt = titleObj.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0f, 0.8f);
        tRt.anchorMax = new Vector2(1f, 0.95f);
        tRt.sizeDelta = Vector2.zero;

        if (!string.IsNullOrEmpty(levelName))
        {
            GameObject nameObj = new GameObject("LevelName", typeof(RectTransform));
            nameObj.transform.SetParent(contentObj.transform, false);
            Text nameText = nameObj.AddComponent<Text>();
            nameText.font = pressStart;
            nameText.fontSize = 10;
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.text = levelName;
            nameText.color = new Color(0.8f, 0.8f, 0.8f);
            RectTransform nRt = nameObj.GetComponent<RectTransform>();
            nRt.anchorMin = new Vector2(0.1f, 0.62f);
            nRt.anchorMax = new Vector2(0.9f, 0.78f);
            nRt.sizeDelta = Vector2.zero;
        }

        if (!string.IsNullOrEmpty(cupName))
        {
            GameObject cupObj = new GameObject("CupName", typeof(RectTransform));
            cupObj.transform.SetParent(contentObj.transform, false);
            Text cupText = cupObj.AddComponent<Text>();
            cupText.font = pressStart;
            cupText.fontSize = 9;
            cupText.alignment = TextAnchor.MiddleCenter;
            cupText.text = cupName;
            cupText.color = new Color(0.6f, 0.5f, 0.3f);
            RectTransform cRt = cupObj.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.1f, 0.45f);
            cRt.anchorMax = new Vector2(0.9f, 0.6f);
            cRt.sizeDelta = Vector2.zero;
        }

        string dateKey = isTutorial ? "Ribbon_Tutorial_Date" : $"Ribbon_Level_{levelId}_Date";
        string dateStr = PlayerPrefs.GetString(dateKey, "");
        if (!string.IsNullOrEmpty(dateStr))
        {
            GameObject dateObj = new GameObject("Date", typeof(RectTransform));
            dateObj.transform.SetParent(contentObj.transform, false);
            Text dateText = dateObj.AddComponent<Text>();
            dateText.font = pressStart;
            dateText.fontSize = 8;
            dateText.alignment = TextAnchor.MiddleCenter;
            dateText.text = $"Earned: {dateStr}";
            dateText.color = new Color(0.5f, 0.5f, 0.5f);
            RectTransform dRt = dateObj.GetComponent<RectTransform>();
            dRt.anchorMin = new Vector2(0.1f, 0.28f);
            dRt.anchorMax = new Vector2(0.9f, 0.42f);
            dRt.sizeDelta = Vector2.zero;
        }

        GameObject closeBtnObj = new GameObject("OK", typeof(RectTransform));
        closeBtnObj.transform.SetParent(contentObj.transform, false);
        Image closeBtnImg = closeBtnObj.AddComponent<Image>();
        Sprite okSprite = LoadFirstSprite("Sprites/Menu/botin ui/botonOK", "botonOK_0");
        if (okSprite != null) { closeBtnImg.sprite = okSprite; closeBtnImg.preserveAspect = true; closeBtnImg.color = Color.white; }
        else { closeBtnImg.color = new Color(0.4f, 0.2f, 0.1f, 0.85f); }
        RectTransform closeBtnRt = closeBtnObj.GetComponent<RectTransform>();
        closeBtnRt.anchorMin = new Vector2(0.5f, 0.5f);
        closeBtnRt.anchorMax = new Vector2(0.5f, 0.5f);
        closeBtnRt.pivot = new Vector2(0.5f, 0.5f);
        closeBtnRt.anchoredPosition = new Vector2(0, -150);
        closeBtnRt.sizeDelta = new Vector2(180, 55);
        Button closeBtn = closeBtnObj.AddComponent<Button>();
        closeBtn.targetGraphic = closeBtnImg;
        closeBtn.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlayButton();
            Destroy(popupObj);
        });
    }

    void BuildSettingsView()
    {
        CreateViewTitle("SETTINGS");
        CreateText("COMING SOON", new Vector2(0, 60), 16, new Color(0.6f, 0.55f, 0.45f));

        GameObject testChestBtn = CreateButton("TEST CHEST", new Vector2(0, -20), new Vector2(200, 45), contentRoot.transform);
        testChestBtn.GetComponent<Image>().color = new Color(0.3f, 0.2f, 0.45f, 0.9f);
        Text tcLabel = testChestBtn.GetComponentInChildren<Text>();
        if (tcLabel != null) { tcLabel.fontSize = 10; tcLabel.color = Color.white; }
        testChestBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySelect();
            if (ChestManager.AddReadyChestForTest())
            {
                ClearContent();
                ShowView("chests");
            }
        });

        GameObject testDailyBtn = CreateButton("TEST DAILY", new Vector2(0, -80), new Vector2(200, 45), contentRoot.transform);
        testDailyBtn.GetComponent<Image>().color = new Color(0.3f, 0.45f, 0.2f, 0.9f);
        Text tdLabel = testDailyBtn.GetComponentInChildren<Text>();
        if (tdLabel != null) { tdLabel.fontSize = 10; tdLabel.color = Color.white; }
        testDailyBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySelect();
            PlayerPrefs.DeleteKey("LastDailyBonus");
            PlayerPrefs.Save();
            if (DailyBonusUI.Instance != null)
                DailyBonusUI.Instance.ShowIfAvailable();
        });
    }

    void CreateViewTitle(string title)
    {
        CreateText(title, new Vector2(0, 280), 20, new Color(0.9f, 0.75f, 0.3f));
    }

    Sprite GetCupSprite(int cupIndex)
    {
        string[] paths = {
            "Sprites/Menu/copaHuman",
            "Sprites/Menu/copaOrc",
            "Sprites/Menu/copaBeast",
            "Sprites/Menu/copaMenu"
        };
        if (cupIndex < 0 || cupIndex >= paths.Length) return null;
        Sprite[] sprites = Resources.LoadAll<Sprite>(paths[cupIndex]);
        if (sprites == null || sprites.Length == 0) return null;
        foreach (var s in sprites)
        {
            if (s.name.EndsWith("_0")) return s;
        }
        return sprites[0];
    }

    GameObject CreateCupVisual(string cupName, Sprite sprite, bool completed, Vector2 position, Vector2 size, Transform parent)
    {
        GameObject cupObj = new GameObject($"Cup_{cupName}");
        cupObj.transform.SetParent(parent, false);

        RectTransform rt = cupObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image img = cupObj.AddComponent<Image>();
        if (sprite != null)
        {
            img.sprite = sprite;
            img.preserveAspect = true;
        }
        img.color = completed ? Color.white : new Color(0.35f, 0.35f, 0.35f);

        GameObject textObj = new GameObject("Name");
        textObj.transform.SetParent(cupObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.font = pressStart;
        text.text = cupName;
        text.fontSize = 6;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = completed ? new Color(1f, 0.9f, 0.5f) : Color.gray;
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0.5f, 0f);
        textRt.anchorMax = new Vector2(0.5f, 0f);
        textRt.pivot = new Vector2(0.5f, 1f);
        textRt.sizeDelta = new Vector2(160, 18);
        textRt.anchoredPosition = new Vector2(cupName == "TUTORIAL" ? -10 : 0, -12);

        return cupObj;
    }

    GameObject CreateText(string content, Vector2 position, int fontSize, Color color, Transform parent = null)
    {
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(parent != null ? parent : contentRoot.transform, false);

        Text text = textObj.AddComponent<Text>();
        text.font = pressStart;
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;

        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);

        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(300, fontSize * 2);

        return textObj;
    }

    GameObject CreateButton(string label, Vector2 position, Vector2 size, Transform parent)
    {
        GameObject btnObj = new GameObject(label);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.3f);

        btnObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(btnObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.font = pressStart;
        text.text = label;
        text.fontSize = 10;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        return btnObj;
    }

    Color GetRarityColor(string rarity)
    {
        switch (rarity?.ToLower())
        {
            case "common": return new Color(0.6f, 0.6f, 0.6f);
            case "rare": return new Color(0.2f, 0.5f, 1f);
            case "epic": return new Color(0.6f, 0.2f, 0.8f);
            case "legendary": return new Color(1f, 0.84f, 0f);
            default: return Color.white;
        }
    }

    Sprite GetChestSprite(int frame)
    {
        if (chestSprites == null)
        {
            chestSprites = Resources.LoadAll<Sprite>("Sprites/Cofre");
            if (chestSprites == null || chestSprites.Length == 0)
            {
                Texture2D tex = new Texture2D(64, 64);
                chestSprites = new Sprite[5];
                for (int i = 0; i < 5; i++) chestSprites[i] = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
            }
        }
        int idx = Mathf.Clamp(frame, 0, 4);
        foreach (var s in chestSprites)
        {
            if (s.name == $"cofre{idx}" || s.name.StartsWith($"cofre{idx}_")) return s;
        }
        return chestSprites.Length > 0 ? chestSprites[0] : null;
    }
}
