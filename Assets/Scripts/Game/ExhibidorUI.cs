using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
    private int badgesPerPage = 20;
    private int currentPage = 0;

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
        CreateNavButton("Sprites/Menu/botin ui/panel total back", "panel total back_0", new Vector2(-700, -340), Close);
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
        CreatePaginationControls();
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

    void CreatePaginationControls()
    {
        int totalCount = 0;
        if (currentBadgeFilter == "all")
        {
            var data = InsigniaData.Load();
            totalCount = data?.insignias?.Length ?? 0;
        }
        else
        {
            totalCount = InsigniaData.GetBySource(currentBadgeFilter).Length;
        }
        int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)totalCount / badgesPerPage));
        currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);

        GameObject prevBtn = CreateButton("PREV", new Vector2(-70, 45), new Vector2(110, 40), contentRoot.transform);
        prevBtn.GetComponent<Image>().color = currentPage > 0
            ? new Color(0.35f, 0.25f, 0.15f, 0.9f)
            : new Color(0.2f, 0.2f, 0.2f, 0.6f);
        Text prevLabel = prevBtn.GetComponentInChildren<Text>();
        if (prevLabel != null) prevLabel.fontSize = 9;
        Button prevBtnComp = prevBtn.GetComponent<Button>();
        prevBtnComp.interactable = currentPage > 0;
        prevBtnComp.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySelect();
            currentPage = Mathf.Max(0, currentPage - 1);
            BuildBadgesView();
        });

        CreateText($"{currentPage + 1}/{totalPages}", new Vector2(0, 45), 10, new Color(0.8f, 0.7f, 0.4f));

        GameObject nextBtn = CreateButton("NEXT", new Vector2(70, 45), new Vector2(110, 40), contentRoot.transform);
        nextBtn.GetComponent<Image>().color = currentPage < totalPages - 1
            ? new Color(0.35f, 0.25f, 0.15f, 0.9f)
            : new Color(0.2f, 0.2f, 0.2f, 0.6f);
        Text nextLabel = nextBtn.GetComponentInChildren<Text>();
        if (nextLabel != null) nextLabel.fontSize = 9;
        Button nextBtnComp = nextBtn.GetComponent<Button>();
        nextBtnComp.interactable = currentPage < totalPages - 1;
        nextBtnComp.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySelect();
            currentPage = Mathf.Min(totalPages - 1, currentPage + 1);
            BuildBadgesView();
        });
    }

    void PopulateBadgeGrid(RectTransform content)
    {
        Insignia[] allInsignias;
        if (currentBadgeFilter == "all")
        {
            var data = InsigniaData.Load();
            allInsignias = data?.insignias ?? new Insignia[0];
        }
        else
        {
            allInsignias = InsigniaData.GetBySource(currentBadgeFilter);
        }

        if (allInsignias == null || allInsignias.Length == 0)
        {
            CreateText("No badges found.", new Vector2(0, 0), 12, Color.gray, content);
            return;
        }

        int totalPages = Mathf.CeilToInt((float)allInsignias.Length / badgesPerPage);
        currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);

        float cardW = 150f;
        float cardH = 90f;
        float gapX = 10f;
        float gapY = 10f;
        int cols = 5;

        float totalWidth = cols * cardW + (cols - 1) * gapX;
        float startX = -totalWidth / 2f + cardW / 2f;

        int startIndex = currentPage * badgesPerPage;
        int endIndex = Mathf.Min(startIndex + badgesPerPage, allInsignias.Length);

        for (int i = startIndex; i < endIndex; i++)
        {
            Insignia ins = allInsignias[i];
            bool collected = InsigniaManager.HasInsignia(ins.id);
            int localIndex = i - startIndex;
            int row = localIndex / cols;
            int col = localIndex % cols;

            float x = startX + col * (cardW + gapX);
            float y = -row * (cardH + gapY) - 10f;

            CreateBadgeCard(content, ins, collected, x, y, cardW, cardH);
        }

        int cardsOnPage = endIndex - startIndex;
        int totalRows = Mathf.CeilToInt((float)cardsOnPage / cols);
        float contentHeight = totalRows * (cardH + gapY) + 20f;
        if (contentHeight < 10) contentHeight = 10; // Avoid tiny content heights
        content.sizeDelta = new Vector2(0, contentHeight);
    }

    void CreateBadgeCard(Transform parent, Insignia ins, bool collected, float x, float y, float w, float h)
    {
        Color rarityColor = GetRarityColor(ins.rarity);

        GameObject cardObj = new GameObject($"Card_{ins.id}");
        cardObj.transform.SetParent(parent, false);
        Image cardBg = cardObj.AddComponent<Image>();
        if (collected)
            cardBg.color = new Color(rarityColor.r * 0.25f, rarityColor.g * 0.25f, rarityColor.b * 0.25f, 0.85f);
        else
            cardBg.color = new Color(0.1f, 0.08f, 0.06f, 0.7f);
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
        nameText.fontSize = 7;
        nameText.alignment = collected ? TextAnchor.MiddleLeft : TextAnchor.MiddleCenter;
        nameText.text = collected ? ins.name.ToUpper() : "???";
        nameText.color = collected ? rarityColor : new Color(0.3f, 0.3f, 0.3f);
        RectTransform nRt = nameObj.GetComponent<RectTransform>();
        if (collected)
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

        if (collected)
        {
            GameObject descObj = new GameObject("Desc");
            descObj.transform.SetParent(cardObj.transform, false);
            Text descText = descObj.AddComponent<Text>();
            descText.font = pressStart;
            descText.fontSize = 5;
            descText.alignment = TextAnchor.MiddleLeft;
            descText.text = ins.description;
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
        rarityText.text = ins.rarity.ToUpper();
        rarityText.color = collected
            ? new Color(rarityColor.r * 0.7f, rarityColor.g * 0.7f, rarityColor.b * 0.7f)
            : new Color(0.25f, 0.25f, 0.25f);
        RectTransform rRt = rarityObj.GetComponent<RectTransform>();
        rRt.anchorMin = new Vector2(0f, 0f);
        rRt.anchorMax = new Vector2(1f, 0.18f);
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
            slotBg.color = new Color(0.15f, 0.1f, 0.06f, 0.9f);

            CreateSlotBorder(slotObj.transform);

            if (!slot.occupied) CreateEmptySlot(slotObj.transform);
            else if (slot.ready) CreateReadySlot(slotObj.transform, i);
            else CreateLockedSlot(slotObj.transform, i);
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

        GameObject labelObj = new GameObject("ReadyLabel");
        labelObj.transform.SetParent(parent, false);
        Text labelText = labelObj.AddComponent<Text>();
        labelText.font = pressStart;
        labelText.fontSize = 16;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.text = "READY!";
        labelText.color = new Color(0.3f, 1f, 0.3f);
        RectTransform lRt = labelObj.GetComponent<RectTransform>();
        lRt.anchorMin = new Vector2(0f, 0.42f);
        lRt.anchorMax = new Vector2(1f, 0.55f);
        lRt.sizeDelta = Vector2.zero;

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
        GameObject btnObj = CreateButton("OPEN", new Vector2(0, -75), new Vector2(190, 45), parent);
        btnObj.GetComponent<Image>().color = new Color(0.3f, 0.6f, 0.3f, 0.9f);
        Text label = btnObj.GetComponentInChildren<Text>();
        if (label != null) label.fontSize = 13;
        int capturedIndex = index;
        btnObj.GetComponent<Button>().onClick.AddListener(() => OnOpenClicked(capturedIndex));
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
        contentBg.color = new Color(0.12f, 0.08f, 0.04f, 0.95f);
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
        titleText.fontSize = 16;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.text = "CHEST OPENED!";
        titleText.color = new Color(0.9f, 0.75f, 0.2f);
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
            goldText.fontSize = 24;
            goldText.alignment = TextAnchor.MiddleCenter;
            goldText.text = $"+{reward.gold} GOLD!";
            goldText.color = new Color(1f, 0.84f, 0f);
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
            itemBg.color = new Color(rarityColor.r * 0.2f, rarityColor.g * 0.2f, rarityColor.b * 0.2f, 0.55f);
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

        GameObject closeBtnObj = CreateButton("OK", new Vector2(0, -235), new Vector2(150, 40), contentObj.transform);
        closeBtnObj.GetComponent<Image>().color = new Color(0.4f, 0.2f, 0.1f, 0.85f);
        Text closeLabel = closeBtnObj.GetComponentInChildren<Text>();
        if (closeLabel != null) closeLabel.fontSize = 12;
        closeBtnObj.GetComponent<Button>().onClick.AddListener(() =>
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

        CreateText("CUPS", new Vector2(0, 190), 12, Color.white);
        for (int i = 0; i < data.cups.Length; i++)
        {
            CampaignCup cup = data.cups[i];
            bool completed = CampaignManager.Instance != null && CampaignManager.Instance.IsCupCompleted(cup.id);
            float x = -330 + i * 220;
            CreateCupVisual(cup.name, GetCupSprite(i), completed, new Vector2(x, 100), contentRoot.transform);
        }

        CreateText("TUTORIAL", new Vector2(0, -10), 12, Color.white);
        bool earned = TutorialCollectibles.HasEarned();
        CreateTutorialItem(new Vector2(-150, -40), TutorialCollectibles.GetCupSprite(), new Color(1f, 0.84f, 0f), new Vector2(45, 45), "CUP", earned);
        CreateTutorialItem(new Vector2(0, -40), TutorialCollectibles.GetRibbonSprite(), new Color(0.62f, 0.42f, 0.24f), new Vector2(75, 25), "RIBBON", earned);
        CreateTutorialItem(new Vector2(150, -40), TutorialCollectibles.GetInsigniaSprite(), Color.white, new Vector2(40, 40), "BADGE", earned);
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
        float gapX = 15f;
        float gapY = 15f;
        int cols = 5;

        float totalWidth = cols * cardW + (cols - 1) * gapX;
        float startX = -totalWidth / 2f + cardW / 2f;

        for (int i = 0; i < data.levels.Length; i++)
        {
            CampaignLevel level = data.levels[i];
            bool has = RibbonManager.HasRibbon(level.id);
            Color color = has ? RibbonManager.GetRibbonColor(level.id) : new Color(0.3f, 0.3f, 0.3f, 0.5f);

            int row = i / cols;
            int col = i % cols;

            float x = startX + col * (cardW + gapX);
            float y = -row * (cardH + gapY) - 10f;

            CreateRibbonCard(content, level, has, color, x, y, cardW, cardH);
        }

        int totalRows = Mathf.CeilToInt((float)data.levels.Length / cols);
        float contentHeight = totalRows * (cardH + gapY) + 20f;
        content.sizeDelta = new Vector2(0, contentHeight);
    }

    void CreateRibbonCard(Transform parent, CampaignLevel level, bool has, Color color, float x, float y, float w, float h)
    {
        GameObject cardObj = new GameObject($"Ribbon_{level.id}");
        cardObj.transform.SetParent(parent, false);
        Image cardBg = cardObj.AddComponent<Image>();
        if (has)
            cardBg.color = new Color(0.35f, 0.25f, 0.12f, 0.85f);
        else
            cardBg.color = new Color(0.1f, 0.08f, 0.06f, 0.7f);
        RectTransform cardRt = cardObj.GetComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 1f);
        cardRt.anchorMax = new Vector2(0.5f, 1f);
        cardRt.pivot = new Vector2(0.5f, 1f);
        cardRt.sizeDelta = new Vector2(w, h);
        cardRt.anchoredPosition = new Vector2(x, y);

        GameObject ribbonObj = new GameObject("Ribbon");
        ribbonObj.transform.SetParent(cardObj.transform, false);
        Image ribbonImg = ribbonObj.AddComponent<Image>();
        ribbonImg.sprite = RibbonManager.GetRibbonSprite(level.id);
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
        nameText.text = has ? $"L{level.id}" : "???";
        nameText.color = has ? new Color(1f, 0.85f, 0.3f) : new Color(0.3f, 0.3f, 0.3f);
        RectTransform nRt = nameObj.GetComponent<RectTransform>();
        nRt.anchorMin = new Vector2(0f, 0f);
        nRt.anchorMax = new Vector2(1f, 0.22f);
        nRt.sizeDelta = Vector2.zero;
    }

    void CreateTutorialItem(Vector2 pos, Sprite sprite, Color tint, Vector2 size, string label, bool earned)
    {
        GameObject obj = new GameObject($"Tut_{label}");
        obj.transform.SetParent(contentRoot.transform, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.color = earned ? tint : new Color(tint.r, tint.g, tint.b, 0.3f);
    }

    void BuildSettingsView()
    {
        CreateViewTitle("SETTINGS");
        CreateText("COMING SOON", new Vector2(0, 0), 16, new Color(0.6f, 0.55f, 0.45f));
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

    GameObject CreateCupVisual(string cupName, Sprite sprite, bool completed, Vector2 position, Transform parent)
    {
        GameObject cupObj = new GameObject($"Cup_{cupName}");
        cupObj.transform.SetParent(parent, false);

        RectTransform rt = cupObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(85, 108);

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
        textRt.anchoredPosition = new Vector2(0, -12);

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
