using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CampaignMapUI : MonoBehaviour
{
    public System.Action OnClose;
    public bool loadsSceneOnClose;

    private GameObject panelObj;
    private Canvas canvas;
    private Font font;
    private Sprite circleSprite;
    private Sprite starSprite;
    private string selectedSpecies;

    private int currentPage;
    private GameObject[] pages;
    private Image[] dotImages;
    private Text cupLabel;
    private Image cupImage;

    private GameObject playerHeadObj;
    private RectTransform headRt;
    private Vector2 headBasePos;
    private Coroutine headBobCoroutine;
    private Coroutine pulseCoroutine;
    private RectTransform backButtonRt;

    class CardRefs
    {
        public GameObject root;
        public int levelId;
        public int state;
        public Transform enemyHead;
        public Vector2 pos;
    }

    private readonly Dictionary<int, CardRefs> cardsByLevel = new Dictionary<int, CardRefs>();
    private readonly Dictionary<int, List<CardRefs>> cardsByPage = new Dictionary<int, List<CardRefs>>();

    static readonly Vector2[] cardPositions = new Vector2[]
    {
        new Vector2(-700f, -240f),
        new Vector2(-350f, 20f),
        new Vector2(0f, -230f),
        new Vector2(350f, 20f),
        new Vector2(700f, -250f),
    };

    const float CARD_W = 340f;
    const float CARD_H = 150f;

    public void Show(Canvas parentCanvas)
    {
        ShowInternal(parentCanvas, -1);
    }

    public void ShowForNextLevel(Canvas parentCanvas, int targetLevel)
    {
        ShowInternal(parentCanvas, targetLevel);
    }

    void ShowInternal(Canvas parentCanvas, int targetLevel)
    {
        canvas = parentCanvas;
        font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        selectedSpecies = GameConfig.selectedSpecies;
        CreateCircleSprite();
        CreateStarSprite();

        SoundManager.Instance.PlayCampaignMenuMusic();

        BuildPanel();

        var data = CampaignData.Load();
        int target = targetLevel > 0 ? targetLevel : (CampaignManager.Instance != null ? CampaignManager.Instance.GetNextUncompletedCupLevel() : -1);
        int lastPlayed = GameConfig.selectedLevel;

        int anchorLevel = lastPlayed > 0 ? lastPlayed : target;
        int page = 0;
        if (anchorLevel > 0 && data != null && data.cups != null)
            page = Mathf.Max(0, FindPageIndex(data, anchorLevel));
        ShowPage(page);

        if (anchorLevel > 0 && data != null)
            PlacePlayerHead(HeadAnchor(GetLevelPos(data, anchorLevel)));

        if (targetLevel > 0 && targetLevel == (CampaignManager.Instance != null ? CampaignManager.Instance.GetNextUncompletedCupLevel() : -1))
            StartCoroutine(ConquestAndTravel(targetLevel));
    }

    void BuildPanel()
    {
        panelObj = new GameObject("CampaignMapPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        RectTransform rt = panelObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        Image bgImg = panelObj.AddComponent<Image>();
        bgImg.color = new Color(0.05f, 0.04f, 0.03f, 0.97f);

        SwipeDetector swipe = panelObj.AddComponent<SwipeDetector>();
        swipe.OnSwipeLeft = () => SwitchPage(-1);
        swipe.OnSwipeRight = () => SwitchPage(1);

        CreateHeader();
        BuildPages();
        CreateArrows();
        CreateDots();
        if (backButtonRt != null)
            backButtonRt.SetAsLastSibling();
    }

    void CreateHeader()
    {
        Sprite backSprite = LoadBackSprite("Sprites/Menu/botin ui/panel total back");

        GameObject backObj = new GameObject("BackButton");
        backObj.transform.SetParent(panelObj.transform, false);
        RectTransform backRt = backObj.AddComponent<RectTransform>();
        backButtonRt = backRt;
        backRt.anchorMin = new Vector2(0f, 1f);
        backRt.anchorMax = new Vector2(0f, 1f);
        backRt.pivot = new Vector2(0f, 1f);
        backRt.sizeDelta = new Vector2(150, 70);
        backRt.anchoredPosition = new Vector2(15, -10);

        Image backImg = backObj.AddComponent<Image>();
        if (backSprite != null) backImg.sprite = backSprite;
        else backImg.color = new Color(0.4f, 0.25f, 0.12f, 0.9f);
        backImg.preserveAspect = true;

        Button backBtn = backObj.AddComponent<Button>();
        backBtn.targetGraphic = backImg;
        backBtn.onClick.AddListener(() => { SoundManager.Instance.PlayButton(); Close(); });

        MakeText(panelObj.transform, "CAMPAIGN", 24, new Color(0.95f, 0.8f, 0.25f), new Vector2(0, 620), new Vector2(500, 44));

        cupLabel = MakeText(panelObj.transform, "", 13, new Color(0.85f, 0.85f, 0.85f), new Vector2(30, 578), new Vector2(700, 34));

        GameObject cupImgObj = new GameObject("HeaderCup");
        cupImgObj.transform.SetParent(panelObj.transform, false);
        cupImage = cupImgObj.AddComponent<Image>();
        cupImage.preserveAspect = true;
        cupImage.raycastTarget = false;
        RectTransform ciRt = cupImgObj.GetComponent<RectTransform>();
        ciRt.anchorMin = new Vector2(0.5f, 1f);
        ciRt.anchorMax = new Vector2(0.5f, 1f);
        ciRt.pivot = new Vector2(0.5f, 0.5f);
        ciRt.sizeDelta = new Vector2(40, 50);
        ciRt.anchoredPosition = new Vector2(-330, -80);
    }

    void BuildPages()
    {
        var data = CampaignData.Load();
        int cupCount = data != null && data.cups != null ? data.cups.Length : 0;
        pages = new GameObject[cupCount];
        dotImages = new Image[cupCount];

        for (int i = 0; i < cupCount; i++)
            pages[i] = CreatePage(data.cups[i], i);
    }

    GameObject CreatePage(CampaignCup cup, int pageIdx)
    {
        GameObject pageObj = new GameObject($"Page_{cup.id}");
        pageObj.transform.SetParent(panelObj.transform, false);
        RectTransform prt = pageObj.AddComponent<RectTransform>();
        prt.anchorMin = Vector2.zero;
        prt.anchorMax = Vector2.one;
        prt.sizeDelta = Vector2.zero;
        pageObj.SetActive(false);

        Image pageBg = pageObj.AddComponent<Image>();
        pageBg.raycastTarget = true;
        pageBg.color = new Color(0.07f, 0.06f, 0.05f, 1f);

        Sprite bgSprite = GetBackgroundSprite(cup.race);
        if (bgSprite != null)
        {
            GameObject bgObj = new GameObject("MapBg");
            bgObj.transform.SetParent(pageObj.transform, false);
            Image bgi = bgObj.AddComponent<Image>();
            bgi.sprite = bgSprite;
            bgi.preserveAspect = false;
            bgi.raycastTarget = false;
            RectTransform brt = bgObj.GetComponent<RectTransform>();
            brt.anchorMin = Vector2.zero;
            brt.anchorMax = Vector2.one;
            brt.sizeDelta = Vector2.zero;

            GameObject darkObj = new GameObject("Dim");
            darkObj.transform.SetParent(pageObj.transform, false);
            Image dim = darkObj.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.28f);
            dim.raycastTarget = false;
            RectTransform drt = darkObj.GetComponent<RectTransform>();
            drt.anchorMin = Vector2.zero;
            drt.anchorMax = Vector2.one;
            drt.sizeDelta = Vector2.zero;
        }

        for (int i = 1; i < cardPositions.Length; i++)
            CreateConnector(pageObj.transform, cardPositions[i - 1], cardPositions[i]);

        var refsList = new List<CardRefs>();
        cardsByPage[pageIdx] = refsList;

        for (int slot = 0; slot < cup.levels.Length && slot < cardPositions.Length; slot++)
        {
            var level = CampaignData.GetLevel(cup.levels[slot]);
            if (level == null) continue;

            bool completed = CampaignManager.Instance != null && CampaignManager.Instance.IsLevelCompleted(level.id);
            bool unlocked = CampaignManager.Instance != null && CampaignManager.Instance.IsLevelUnlocked(level.id);

            CardRefs refs = CreateMapCard(pageObj, level, slot, completed, unlocked, cardPositions[slot]);
            refsList.Add(refs);
            cardsByLevel[level.id] = refs;
        }

        return pageObj;
    }

    CardRefs CreateMapCard(GameObject page, CampaignLevel level, int slot, bool completed, bool unlocked, Vector2 pos)
    {
        CardRefs refs = new CardRefs { levelId = level.id, pos = pos, state = completed ? 2 : (unlocked ? 1 : 0) };

        GameObject card = new GameObject($"Card_{level.id}");
        card.transform.SetParent(page.transform, false);
        RectTransform crt = card.AddComponent<RectTransform>();
        crt.anchorMin = new Vector2(0.5f, 0.5f);
        crt.anchorMax = new Vector2(0.5f, 0.5f);
        crt.pivot = new Vector2(0.5f, 0.5f);
        crt.sizeDelta = new Vector2(CARD_W, CARD_H);
        crt.anchoredPosition = pos;

        Image bg = card.AddComponent<Image>();
        Sprite panelSprite = LoadFirstSprite("Sprites/Menu/panelCartaRed", "panelCartaRed");
        if (panelSprite != null) bg.sprite = panelSprite;

        Color tint = refs.state == 2 ? new Color(0.85f, 0.68f, 0.32f, 0.96f)
            : refs.state == 1 ? new Color(0.72f, 0.25f, 0.22f, 0.95f)
            : new Color(0.33f, 0.33f, 0.36f, 0.82f);
        bg.color = tint;
        refs.root = card;

        Outline outline = card.AddComponent<Outline>();
        outline.effectColor = refs.state == 2 ? new Color(1f, 0.84f, 0.3f, 0.7f)
            : refs.state == 1 ? new Color(0f, 0f, 0f, 0.4f)
            : new Color(0f, 0f, 0f, 0.15f);
        outline.effectDistance = new Vector2(2, -2);

        MakeText(card.transform, level.id.ToString(), 30,
            refs.state == 0 ? new Color(0.55f, 0.55f, 0.55f) : Color.white,
            new Vector2(98, 30), new Vector2(70, 50));

        MakeText(card.transform, level.name.ToUpper(), 13,
            refs.state == 0 ? new Color(0.55f, 0.55f, 0.55f) : new Color(0.95f, 0.92f, 0.8f),
            new Vector2(-40, 30), new Vector2(220, 50), TextAnchor.MiddleCenter);

        MakeText(card.transform, $"{level.enemyRace.ToUpper()} | E:{level.enemyCount} | +{level.goldReward}g", 9,
            refs.state == 0 ? new Color(0.45f, 0.45f, 0.45f) : new Color(0.85f, 0.75f, 0.6f),
            new Vector2(-50, -35), new Vector2(200, 24), TextAnchor.MiddleCenter);

        if (!string.IsNullOrEmpty(level.insigniaId))
        {
            Insignia ins = InsigniaData.GetInsignia(level.insigniaId);
            Sprite insSprite = ins != null ? InsigniaSprites.Get(ins) : null;
            if (insSprite != null)
            {
                GameObject insObj = new GameObject("Insignia");
                insObj.transform.SetParent(card.transform, false);
                Image insImg = insObj.AddComponent<Image>();
                insImg.sprite = insSprite;
                insImg.preserveAspect = true;
                insImg.raycastTarget = false;
                insImg.color = refs.state == 2 ? Color.white : new Color(0.55f, 0.5f, 0.5f, 0.7f);
                RectTransform irt = insObj.GetComponent<RectTransform>();
                irt.anchorMin = new Vector2(0.5f, 0.5f);
                irt.anchorMax = new Vector2(0.5f, 0.5f);
                irt.pivot = new Vector2(0.5f, 0.5f);
                irt.sizeDelta = new Vector2(70, 58);
                irt.anchoredPosition = new Vector2(110, -35);
            }
        }

        if (refs.state == 2)
        {
            MakeText(card.transform, "\u2713", 14, new Color(0.3f, 1f, 0.3f), new Vector2(140, 30), new Vector2(30, 30));
            CreateStarsRow(card.transform, CampaignManager.Instance != null ? CampaignManager.Instance.GetStars(level.id) : 0);
        }

        if (refs.state >= 1)
        {
            Button btn = card.AddComponent<Button>();
            btn.targetGraphic = bg;
            int capturedId = level.id;
            btn.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySelect();
                ModeSelectionUI modeUI = gameObject.AddComponent<ModeSelectionUI>();
                modeUI.onBack = () =>
                {
                    if (modeUI != null) Destroy(modeUI);
                };
                modeUI.ShowCampaign(canvas, capturedId);
            });
        }

        if (refs.state != 2)
        {
            Sprite icon = GetRaceIcon(level.enemyRace);
            if (icon != null)
            {
                GameObject headObj = new GameObject("EnemyHead");
                headObj.transform.SetParent(card.transform, false);
                Image headImg = headObj.AddComponent<Image>();
                headImg.sprite = icon;
                headImg.preserveAspect = true;
                headImg.raycastTarget = false;
                headImg.color = refs.state == 1 ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.6f);
                RectTransform hrt = headObj.GetComponent<RectTransform>();
                hrt.anchorMin = new Vector2(0.5f, 0.5f);
                hrt.anchorMax = new Vector2(0.5f, 0.5f);
                hrt.pivot = new Vector2(0.5f, 0.5f);
                hrt.sizeDelta = new Vector2(100, 86);
                hrt.anchoredPosition = new Vector2(0, CARD_H / 2f + 52f);
                refs.enemyHead = headObj.transform;
            }
        }

        return refs;
    }

    void CreateStarsRow(Transform parent, int count)
    {
        GameObject row = new GameObject("Stars");
        row.transform.SetParent(parent, false);
        RectTransform rr = row.AddComponent<RectTransform>();
        rr.anchorMin = new Vector2(0.5f, 0.5f);
        rr.anchorMax = new Vector2(0.5f, 0.5f);
        rr.pivot = new Vector2(0.5f, 0.5f);
        rr.sizeDelta = new Vector2(120, 34);
        rr.anchoredPosition = new Vector2(-60, -CARD_H / 2f + 16f);

        for (int i = 0; i < 3; i++)
        {
            GameObject sObj = new GameObject($"Star{i}");
            sObj.transform.SetParent(row.transform, false);
            Image sImg = sObj.AddComponent<Image>();
            sImg.sprite = starSprite;
            sImg.preserveAspect = true;
            sImg.raycastTarget = false;
            sImg.color = i < count ? new Color(1f, 0.82f, 0.1f) : new Color(0.25f, 0.25f, 0.28f, 0.9f);
            RectTransform srt = sObj.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 0.5f);
            srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.sizeDelta = new Vector2(32, 32);
            srt.anchoredPosition = new Vector2(i * 38f - 38f, 0);
        }
    }

    void CreateLockIcon(Transform parent)
    {
        GameObject lockObj = new GameObject("Lock");
        lockObj.transform.SetParent(parent, false);
        Image lockImg = lockObj.AddComponent<Image>();

        int size = 16;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color gold = new Color(0.85f, 0.72f, 0.35f);
        Color dark = new Color(0.25f, 0.18f, 0.08f);
        string[] art =
        {
            "....GGGG....",
            "...G....G...",
            "..G......G..",
            "..G......G..",
            "..G......G..",
            ".GGGGGGGGGG.",
            ".GGGGGGGGGG.",
            ".GGGGDDGGGG.",
            ".GGGDDDDGGG.",
            ".GGGGDDGGGG.",
            ".GGGGDDGGGG.",
            ".GGGDDDDGGG.",
            ".GGGGGGGGGG.",
            ".GGGGGGGGGG."
        };
        for (int y = 0; y < art.Length; y++)
        {
            for (int x = 0; x < art[y].Length; x++)
            {
                char c = art[y][x];
                if (c == 'G') tex.SetPixel(x + 2, size - 2 - y, gold);
                else if (c == 'D') tex.SetPixel(x + 2, size - 2 - y, dark);
            }
        }
        tex.Apply();
        lockImg.sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        lockImg.raycastTarget = false;

        RectTransform lrt = lockObj.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0.5f, 0.5f);
        lrt.anchorMax = new Vector2(0.5f, 0.5f);
        lrt.sizeDelta = new Vector2(80, 80);
        lrt.anchoredPosition = new Vector2(0, -10);
    }

    void CreateConnector(Transform page, Vector2 from, Vector2 to)
    {
        Vector2 mid = (from + to) * 0.5f;
        float dist = Vector2.Distance(from, to);

        GameObject lineObj = new GameObject("Path");
        lineObj.transform.SetParent(page.transform, false);
        Image line = lineObj.AddComponent<Image>();
        line.color = new Color(0.5f, 0.38f, 0.22f, 0.55f);
        line.raycastTarget = false;

        Texture2D tex = new Texture2D(2, 2);
        tex.SetPixel(0, 0, Color.white);
        tex.SetPixel(1, 0, Color.white);
        tex.SetPixel(0, 1, Color.white);
        tex.SetPixel(1, 1, Color.white);
        tex.Apply();
        line.sprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 100);

        RectTransform lrt = lineObj.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0.5f, 0.5f);
        lrt.anchorMax = new Vector2(0.5f, 0.5f);
        lrt.anchoredPosition = mid;
        lrt.sizeDelta = new Vector2(dist, 5f);
        lrt.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg);
    }

    void CreateArrows()
    {
        CreateArrowBtn("<", new Vector2(-880, -20), () => SwitchPage(-1));
        CreateArrowBtn(">", new Vector2(880, -20), () => SwitchPage(1));
    }

    void CreateArrowBtn(string label, Vector2 pos, System.Action onClick)
    {
        GameObject btnObj = new GameObject("Arrow");
        btnObj.transform.SetParent(panelObj.transform, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(70, 90);
        rt.anchoredPosition = pos;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.25f, 0.18f, 0.1f, 0.85f);

        Text txt = MakeText(btnObj.transform, label, 30, new Color(0.95f, 0.8f, 0.25f), Vector2.zero, new Vector2(70, 90));

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => { SoundManager.Instance.PlaySelect(); onClick(); });
    }

    void CreateDots()
    {
        GameObject dotsObj = new GameObject("Dots");
        dotsObj.transform.SetParent(panelObj.transform, false);
        RectTransform drt = dotsObj.AddComponent<RectTransform>();
        drt.anchorMin = new Vector2(0.5f, 0.5f);
        drt.anchorMax = new Vector2(0.5f, 0.5f);
        drt.sizeDelta = new Vector2(200, 24);
        drt.anchoredPosition = new Vector2(0, -505);

        for (int i = 0; i < pages.Length; i++)
        {
            GameObject dot = new GameObject($"Dot{i}");
            dot.transform.SetParent(dotsObj.transform, false);
            Image dimg = dot.AddComponent<Image>();
            dimg.sprite = circleSprite;
            dimg.raycastTarget = true;
            RectTransform dr = dot.GetComponent<RectTransform>();
            dr.anchorMin = new Vector2(0.5f, 0.5f);
            dr.anchorMax = new Vector2(0.5f, 0.5f);
            dr.sizeDelta = new Vector2(16, 16);
            dr.anchoredPosition = new Vector2((i - (pages.Length - 1) / 2f) * 36f, 0);
            dotImages[i] = dimg;

            int captured = i;
            Button dbtn = dot.AddComponent<Button>();
            dbtn.targetGraphic = dimg;
            dbtn.onClick.AddListener(() => { SoundManager.Instance.PlaySelect(); ShowPage(captured); });
        }
    }

    void SwitchPage(int dir)
    {
        ShowPage(currentPage + dir);
    }

    void ShowPage(int idx)
    {
        if (pages == null || pages.Length == 0) return;
        idx = Mathf.Clamp(idx, 0, pages.Length - 1);
        currentPage = idx;

        for (int i = 0; i < pages.Length; i++)
            if (pages[i] != null)
                pages[i].SetActive(i == idx);

        UpdateDots();
        UpdateCupLabel(idx);
        RestartPulseLoop();
    }

    void UpdateDots()
    {
        if (dotImages == null) return;
        for (int i = 0; i < dotImages.Length; i++)
        {
            if (dotImages[i] == null) continue;
            dotImages[i].color = i == currentPage ? Color.white : new Color(1f, 1f, 1f, 0.3f);
        }
    }

    void UpdateCupLabel(int idx)
    {
        var data = CampaignData.Load();
        if (data == null || data.cups == null || idx < 0 || idx >= data.cups.Length) return;
        var cup = data.cups[idx];
        if (cup == null || cup.levels == null) return;

        int done = 0;
        foreach (int lid in cup.levels)
            if (CampaignManager.Instance != null && CampaignManager.Instance.IsLevelCompleted(lid))
                done++;

        cupLabel.text = $"{cup.name.ToUpper()}   {done}/{cup.levels.Length}";
        if (cupImage != null)
            cupImage.sprite = GetCupSprite(cup.race);
    }

    void RestartPulseLoop()
    {
        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PulseLoop());
    }

    IEnumerator PulseLoop()
    {
        while (true)
        {
            if (cardsByPage.TryGetValue(currentPage, out List<CardRefs> list))
            {
                foreach (CardRefs c in list)
                {
                    if (c == null || c.root == null || c.state != 1) continue;
                    float p = 1f + Mathf.Sin(Time.time * 2.6f) * 0.04f;
                    c.root.transform.localScale = Vector3.one * p;
                }
            }
            yield return null;
        }
    }

    void PlacePlayerHead(Vector2 anchorPos)
    {
        if (playerHeadObj == null)
        {
            playerHeadObj = new GameObject("PlayerHead");
            playerHeadObj.transform.SetParent(panelObj.transform, false);
            Image img = playerHeadObj.AddComponent<Image>();
            string menuName = selectedSpecies switch
            {
                "Orc" => "menuOrc",
                "Beastfolk" => "menuBeast",
                _ => "menuHuman"
            };
            Sprite sp = LoadFirstSprite($"Sprites/Menu/{menuName}", menuName);
            if (sp != null)
            {
                img.sprite = sp;
                img.preserveAspect = true;
            }
            else
            {
                img.sprite = circleSprite;
                img.color = new Color(0.3f, 0.6f, 1f);
            }
            img.raycastTarget = false;
            headRt = playerHeadObj.GetComponent<RectTransform>();
            headRt.anchorMin = new Vector2(0.5f, 0.5f);
            headRt.anchorMax = new Vector2(0.5f, 0.5f);
            headRt.pivot = new Vector2(0.5f, 0.5f);
            headRt.sizeDelta = new Vector2(110, 140);
        }

        headBasePos = anchorPos;
        headRt.anchoredPosition = anchorPos;
        RestartBob();
    }

    void RestartBob()
    {
        if (headBobCoroutine != null)
            StopCoroutine(headBobCoroutine);
        headBobCoroutine = StartCoroutine(HeadBob());
    }

    IEnumerator HeadBob()
    {
        while (headRt != null)
        {
            float bob = Mathf.Sin(Time.time * 1.6f) * 6f;
            headRt.anchoredPosition = headBasePos + new Vector2(0f, bob);
            yield return null;
        }
    }

    IEnumerator ConquestAndTravel(int targetLevel)
    {
        yield return new WaitForSeconds(0.45f);

        var data = CampaignData.Load();
        if (data == null || data.cups == null) yield break;

        int lastPlayed = GameConfig.selectedLevel;
        int origin = lastPlayed > 0 && cardsByLevel.ContainsKey(lastPlayed)
            ? lastPlayed
            : GetPreviousCompletedBefore(data, targetLevel);
        int targetPage = FindPageIndex(data, targetLevel);

        if (origin <= 0 || origin == targetLevel)
        {
            yield return WalkHead(HeadAnchor(GetLevelPos(data, targetLevel)));
            SoundManager.Instance.PlaySelect();
            yield break;
        }

        PlacePlayerHead(HeadAnchor(GetLevelPos(data, origin)));

        if (cardsByLevel.TryGetValue(origin, out CardRefs oc))
            yield return StartCoroutine(ConquestFX(oc));

        Vector2 endPos = HeadAnchor(GetLevelPos(data, targetLevel));
        Vector2 startPos = headRt != null ? headRt.anchoredPosition : endPos;
        int originPage = FindPageIndex(data, origin);

        if (originPage != targetPage)
        {
            bool dirRight = endPos.x >= startPos.x;
            yield return WalkHead(new Vector2(dirRight ? 900f : -900f, endPos.y));
            yield return FadeHead(1f, 0f, 0.12f);
            ShowPage(targetPage);
            headRt.anchoredPosition = new Vector2(dirRight ? -900f : 900f, endPos.y);
            yield return FadeHead(0f, 1f, 0.12f);
        }

        yield return WalkHead(endPos);
        SoundManager.Instance.PlaySelect();
    }

    IEnumerator ConquestFX(CardRefs card)
    {
        if (card == null || card.root == null) yield break;

        RectTransform rt = card.root.GetComponent<RectTransform>();
        Image bg = card.root.GetComponent<Image>();
        Color baseCol = bg != null ? bg.color : Color.white;

        for (int i = 0; i < 2; i++)
        {
            if (bg == null) yield break;
            bg.color = new Color(2f, 2f, 2f, baseCol.a);
            yield return new WaitForSeconds(0.09f);
            if (bg == null) yield break;
            bg.color = baseCol;
            yield return new WaitForSeconds(0.07f);
        }

        SoundManager.Instance.PlayHammer();
        if (rt != null)
            StartCoroutine(ShakeRect(rt, 0.25f));

        card.state = 2;
        ApplyConqueredLook(card);

        if (card.enemyHead != null)
            StartCoroutine(RiseAndFadeEnemy(card.enemyHead));

        SpawnSmoke(card.pos + new Vector2(0, CARD_H / 2f + 52f));

        yield return new WaitForSeconds(0.55f);
    }

    void ApplyConqueredLook(CardRefs card)
    {
        if (card.root == null) return;
        card.root.transform.localScale = Vector3.one;

        Image bg = card.root.GetComponent<Image>();
        if (bg != null) bg.color = new Color(0.85f, 0.68f, 0.32f, 0.96f);

        Outline outline = card.root.GetComponent<Outline>();
        if (outline != null) outline.effectColor = new Color(1f, 0.84f, 0.3f, 0.7f);

        if (card.root.transform.Find("CheckMark") == null)
            MakeText(card.root.transform, "\u2713", 18, new Color(0.3f, 1f, 0.3f), new Vector2(140, 48), new Vector2(40, 40)).name = "CheckMark";

        if (card.root.transform.Find("Stars") == null && CampaignManager.Instance != null)
            CreateStarsRow(card.root.transform, CampaignManager.Instance.GetStars(card.levelId));

        if (card.enemyHead != null)
        {
            Image h = card.enemyHead.GetComponent<Image>();
            if (h != null) h.color = new Color(0.45f, 0.42f, 0.42f, 0.6f);
        }
    }

    IEnumerator RiseAndFadeEnemy(Transform headT)
    {
        if (headT == null) yield break;
        RectTransform rt = headT as RectTransform;
        if (rt == null) yield break;
        Image img = rt.GetComponent<Image>();
        Vector2 start = rt.anchoredPosition;
        Color startCol = img != null ? img.color : Color.white;
        float t = 0f;

        while (t < 0.8f)
        {
            if (rt == null) yield break;
            t += Time.deltaTime;
            float p = t / 0.8f;
            rt.anchoredPosition = start + Vector2.up * (p * 55f);
            if (img != null)
                img.color = new Color(startCol.r, startCol.g, startCol.b, startCol.a * (1f - p));
            yield return null;
        }
        if (rt != null) Destroy(rt.gameObject);
    }

    void SpawnSmoke(Vector2 localPos)
    {
        for (int i = 0; i < 6; i++)
        {
            GameObject puff = new GameObject($"Smoke{i}");
            puff.transform.SetParent(panelObj.transform, false);
            Image img = puff.AddComponent<Image>();
            img.sprite = circleSprite;
            img.color = new Color(0.55f, 0.55f, 0.58f, 0.7f);
            img.raycastTarget = false;
            RectTransform rt = puff.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            float sz = Random.Range(26f, 44f);
            rt.sizeDelta = new Vector2(sz, sz);
            rt.anchoredPosition = localPos + new Vector2(Random.Range(-35f, 35f), Random.Range(-15f, 15f));
            StartCoroutine(AnimateSmoke(rt, Random.Range(0.05f, 0.3f)));
        }
    }

    IEnumerator AnimateSmoke(RectTransform rt, float delay)
    {
        yield return new WaitForSeconds(delay);
        Vector2 start = rt.anchoredPosition;
        float t = 0f;
        while (t < 0.9f)
        {
            if (rt == null) yield break;
            t += Time.deltaTime;
            float p = t / 0.9f;
            rt.anchoredPosition = start + Vector2.up * (p * 70f);
            rt.localScale = Vector3.one * (1f + p * 0.8f);
            Image img = rt.GetComponent<Image>();
            if (img != null)
                img.color = new Color(0.55f, 0.55f, 0.58f, 0.7f * (1f - p));
            yield return null;
        }
        if (rt != null) Destroy(rt.gameObject);
    }

    IEnumerator ShakeRect(RectTransform rt, float dur)
    {
        Vector2 orig = rt.anchoredPosition;
        float t = 0f;
        while (t < dur)
        {
            if (rt == null) yield break;
            t += Time.deltaTime;
            float decay = 1f - t / dur;
            rt.anchoredPosition = orig + new Vector2(Random.Range(-3f, 3f), Random.Range(-3f, 3f)) * decay;
            yield return null;
        }
        if (rt != null) rt.anchoredPosition = orig;
    }

    IEnumerator WalkHead(Vector2 end)
    {
        if (headRt == null) yield break;
        if (headBobCoroutine != null)
        {
            StopCoroutine(headBobCoroutine);
            headBobCoroutine = null;
        }

        Vector2 start = headRt.anchoredPosition;
        float dur = 0.65f;
        float t = 0f;
        while (t < dur)
        {
            if (headRt == null) yield break;
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / dur);
            headRt.anchoredPosition = Vector2.Lerp(start, end, p);
            headRt.localScale = Vector3.one * (1f + Mathf.Sin(p * Mathf.PI) * 0.08f);
            yield return null;
        }
        if (headRt != null)
        {
            headRt.anchoredPosition = end;
            headRt.localScale = Vector3.one;
            headBasePos = end;
            RestartBob();
        }
    }

    IEnumerator FadeHead(float from, float to, float dur)
    {
        if (playerHeadObj == null) yield break;
        Image img = playerHeadObj.GetComponent<Image>();
        if (img == null) yield break;
        Color baseCol = new Color(img.color.r, img.color.g, img.color.b, 1f);
        float t = 0f;
        while (t < dur)
        {
            if (img == null) yield break;
            t += Time.deltaTime;
            img.color = new Color(baseCol.r, baseCol.g, baseCol.b, Mathf.Lerp(from, to, t / dur));
            yield return null;
        }
        if (img != null) img.color = new Color(baseCol.r, baseCol.g, baseCol.b, to);
    }

    Vector2 HeadAnchor(Vector2 cardPos)
    {
        return cardPos + new Vector2(0, -160f);
    }

    Vector2 GetLevelPos(CampaignDataWrapper data, int levelId)
    {
        if (data.cups == null) return Vector2.zero;
        foreach (var cup in data.cups)
        {
            if (cup == null || cup.levels == null) continue;
            for (int i = 0; i < cup.levels.Length; i++)
            {
                if (cup.levels[i] == levelId && i < cardPositions.Length)
                    return cardPositions[i];
            }
        }
        return Vector2.zero;
    }

    int FindPageIndex(CampaignDataWrapper data, int levelId)
    {
        if (data.cups == null) return 0;
        for (int c = 0; c < data.cups.Length; c++)
        {
            var cup = data.cups[c];
            if (cup == null || cup.levels == null) continue;
            foreach (int lid in cup.levels)
                if (lid == levelId) return c;
        }
        return 0;
    }

    int GetPreviousCompletedBefore(CampaignDataWrapper data, int targetLevel)
    {
        if (CampaignManager.Instance == null) return -1;
        int lastCompleted = -1;
        foreach (var cup in data.cups)
        {
            if (cup == null || cup.levels == null) continue;
            foreach (int lid in cup.levels)
            {
                if (lid == targetLevel) return lastCompleted;
                if (CampaignManager.Instance.IsLevelCompleted(lid))
                    lastCompleted = lid;
            }
        }
        return lastCompleted;
    }

    Sprite GetBackgroundSprite(string race)
    {
        string[] candidates =
        {
            $"Sprites/CampaignMap/mapa{race}",
            BoardManager.SpriteFolder(race) switch
            {
                "Human" => "Sprites/Human/Background/Human",
                "Orc" => "Sprites/Orc/Background/BackgroundOrco",
                "Beastfolk" => "Sprites/Beastfolk/Background/BeastFolk",
                "Nigromantes" => "Sprites/Nigromantes/Background/Nigromantes",
                _ => null
            }
        };

        foreach (string path in candidates)
        {
            if (path == null) continue;
            Sprite[] sprites = Resources.LoadAll<Sprite>(path);
            if (sprites != null && sprites.Length > 0) return sprites[0];
        }
        return null;
    }

    Sprite GetCupSprite(string race)
    {
        string resolved = BoardManager.SpriteFolder(race);
        string path = resolved switch
        {
            "Human" => "Sprites/Menu/copaHuman",
            "Orc" => "Sprites/Menu/copaOrc",
            "Beastfolk" => "Sprites/Menu/copaBeast",
            "Nigromantes" => "Sprites/Menu/copaMenu-copy-0",
            _ => "Sprites/Menu/copaMenu-copy-0"
        };
        Sprite s = Resources.Load<Sprite>(path);
        if (s == null)
        {
            Sprite[] arr = Resources.LoadAll<Sprite>(path);
            if (arr != null && arr.Length > 0)
            {
                foreach (var sp in arr)
                    if (sp.name.Contains("_0")) { s = sp; break; }
                if (s == null) s = arr[0];
            }
        }
        return s;
    }

    Sprite GetRaceIcon(string race)
    {
        string theme = BoardManager.SpriteFolder(race);
        Sprite[] sprites = Resources.LoadAll<Sprite>($"Sprites/{theme}/Icono");
        if (sprites == null || sprites.Length == 0) return null;
        foreach (var s in sprites)
        {
            if (s.name.Contains("Peon") || s.name.Contains("peon") || s.name.Contains("_0")) return s;
        }
        return sprites[0];
    }

    void CreateCircleSprite()
    {
        Texture2D tex = new Texture2D(64, 64);
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float dx = x - 31.5f;
                float dy = y - 31.5f;
                tex.SetPixel(x, y, Mathf.Sqrt(dx * dx + dy * dy) <= 31.5f ? Color.white : Color.clear);
            }
        }
        tex.Apply();
        circleSprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 100);
    }

    void CreateStarSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        float cx = (size - 1) / 2f, cy = (size - 1) / 2f;
        float outer = size / 2f - 1f;
        float inner = outer * 0.42f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx, dy = y - cy;
                float ang = Mathf.Atan2(dy, dx);
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                float norm = Mathf.Repeat(ang + Mathf.PI / 2f, Mathf.PI * 2f / 5f);
                float tri = norm / (Mathf.PI / 5f);
                if (tri > 1f) tri = 2f - tri;
                tex.SetPixel(x, y, r <= Mathf.Lerp(inner, outer, tri) ? Color.white : Color.clear);
            }
        }
        tex.Apply();
        starSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
    }

    Text MakeText(Transform parent, string content, int fontSize, Color color, Vector2 pos, Vector2 size, TextAnchor alignment = TextAnchor.MiddleCenter)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);
        Text text = obj.AddComponent<Text>();
        text.font = font;
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        return text;
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

    Sprite LoadBackSprite(string path)
    {
        Texture2D tex = Resources.Load<Texture2D>(path);
        if (tex != null)
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        Sprite s = Resources.Load<Sprite>(path);
        if (s != null) return s;
        Sprite[] arr = Resources.LoadAll<Sprite>(path);
        return arr != null && arr.Length > 0 ? arr[0] : null;
    }

    public void Hide()
    {
        if (panelObj != null)
        {
            Destroy(panelObj);
            panelObj = null;
        }
    }

    public void Close()
    {
        if (loadsSceneOnClose)
            SceneCover.Show();
        SoundManager.Instance.PlayMenuMusic();
        OnClose?.Invoke();
        Hide();
        Destroy(this);
    }

    class SwipeDetector : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        public System.Action OnSwipeLeft;
        public System.Action OnSwipeRight;
        private Vector2 startPos;

        public void OnBeginDrag(PointerEventData eventData)
        {
            startPos = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            float dx = eventData.position.x - startPos.x;
            if (Mathf.Abs(dx) > 80f)
            {
                if (dx < 0 && OnSwipeLeft != null) OnSwipeLeft();
                else if (dx > 0 && OnSwipeRight != null) OnSwipeRight();
            }
        }
    }
}
