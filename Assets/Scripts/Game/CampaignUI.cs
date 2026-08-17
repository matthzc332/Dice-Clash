using UnityEngine;
using UnityEngine.UI;

public class CampaignUI : MonoBehaviour
{
    public System.Action OnClose;

    private GameObject panelObj;
    private Canvas canvas;
    private Font font;

    public void Show(Canvas parentCanvas)
    {
        font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        canvas = parentCanvas;
        BuildPanel();
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
        Hide();
        OnClose?.Invoke();
        Destroy(this);
    }

    void BuildPanel()
    {
        panelObj = new GameObject("CampaignPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        Image bg = panelObj.AddComponent<Image>();
        bg.color = new Color(0.06f, 0.04f, 0.03f, 0.95f);
        RectTransform bgRt = panelObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;

        CreateScrollContent();
        CreateTitle();
        CreateBackButton();
    }

    void CreateScrollContent()
    {
        GameObject scrollObj = new GameObject("Scroll");
        scrollObj.transform.SetParent(panelObj.transform, false);
        RectTransform scrollRt = scrollObj.AddComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0f, 0f);
        scrollRt.anchorMax = new Vector2(1f, 1f);
        scrollRt.offsetMin = new Vector2(40, 80);
        scrollRt.offsetMax = new Vector2(-40, -60);

        ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.scrollSensitivity = 30f;

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObj.transform, false);
        RectTransform vpRt = viewport.AddComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.sizeDelta = Vector2.zero;
        vpRt.offsetMin = Vector2.zero;
        vpRt.offsetMax = Vector2.zero;
        viewport.AddComponent<Image>().color = new Color(0, 0, 0, 0.01f);
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

        var data = CampaignData.Load();
        if (data == null || data.cups == null) return;

        float yPos = -20f;

        for (int cupIdx = 0; cupIdx < data.cups.Length; cupIdx++)
        {
            var cup = data.cups[cupIdx];
            if (cup == null) continue;

            GameObject cupObj = new GameObject($"Cup_{cup.id}");
            cupObj.transform.SetParent(contentObj.transform, false);
            RectTransform cupRt = cupObj.AddComponent<RectTransform>();
            cupRt.anchorMin = new Vector2(0f, 1f);
            cupRt.anchorMax = new Vector2(1f, 1f);
            cupRt.pivot = new Vector2(0.5f, 1f);
            cupRt.anchoredPosition = new Vector2(0, yPos);

            bool cupCompleted = CampaignManager.Instance != null && IsCupCompleted(cup);

            Image cupBg = cupObj.AddComponent<Image>();
            cupBg.sprite = GetCupSprite(cup.race);
            if (cupBg.sprite != null)
            {
                cupBg.preserveAspect = true;
                cupBg.color = cupCompleted ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.6f);
            }
            else
            {
                float shade = cupCompleted ? 0.35f : 0.2f;
                cupBg.color = new Color(shade, shade * 0.8f, shade * 0.6f, 0.3f);
            }

            GameObject cupTitle = new GameObject("CupTitle");
            cupTitle.transform.SetParent(cupObj.transform, false);
            Text cupText = cupTitle.AddComponent<Text>();
            cupText.font = font;
            cupText.fontSize = 16;
            cupText.alignment = TextAnchor.MiddleCenter;
            cupText.text = cup.name.ToUpper();
            cupText.color = new Color(0.9f, 0.75f, 0.3f);
            Outline cupOutline = cupTitle.AddComponent<Outline>();
            cupOutline.effectColor = Color.black;
            cupOutline.effectDistance = new Vector2(1, -1);
            RectTransform ctRt = cupTitle.GetComponent<RectTransform>();
            ctRt.anchorMin = new Vector2(0.15f, 1f);
            ctRt.anchorMax = new Vector2(0.85f, 1f);
            ctRt.pivot = new Vector2(0.5f, 1f);
            ctRt.sizeDelta = new Vector2(0, 35);
            ctRt.anchoredPosition = new Vector2(0, 0);

            float levelY = -40f;
            if (cup.levels != null)
            {
                for (int i = 0; i < cup.levels.Length; i++)
                {
                    int levelId = cup.levels[i];
                    var level = CampaignData.GetLevel(levelId);
                    if (level == null) continue;

                    bool completed = CampaignManager.Instance != null && CampaignManager.Instance.IsLevelCompleted(levelId);
                    bool unlocked = CampaignManager.Instance != null && CampaignManager.Instance.IsLevelUnlocked(levelId);

                    CreateLevelCard(cupObj.transform, level, completed, unlocked, i, levelY);
                    levelY -= 80f;
                }
            }

            float cupHeight = 40f + (cup.levels?.Length ?? 0) * 80f + 20f;
            cupRt.sizeDelta = new Vector2(0, cupHeight);
            yPos -= cupHeight + 12f;
        }

        crt.sizeDelta = new Vector2(0, -yPos + 20f);
    }

    bool IsCupCompleted(CampaignCup cup)
    {
        if (CampaignManager.Instance == null) return false;
        foreach (int id in cup.levels)
        {
            if (!CampaignManager.Instance.IsLevelCompleted(id))
                return false;
        }
        return true;
    }

    void CreateLevelCard(Transform parent, CampaignLevel level, bool completed, bool unlocked, int index, float yOffset)
    {
        float[] rowX = { -300f, 0f, 300f };
        float xPos = rowX[index % rowX.Length];

        GameObject cardObj = new GameObject($"Level_{level.id}");
        cardObj.transform.SetParent(parent, false);
        RectTransform cardRt = cardObj.AddComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 1f);
        cardRt.anchorMax = new Vector2(0.5f, 1f);
        cardRt.pivot = new Vector2(0.5f, 1f);
        cardRt.sizeDelta = new Vector2(260, 72);
        cardRt.anchoredPosition = new Vector2(xPos, yOffset);

        Image cardBg = cardObj.AddComponent<Image>();
        Sprite panelSprite = LoadFirstSprite("Sprites/Menu/panelCartaBlue", "panelCartaBlue");
        if (panelSprite != null)
        {
            cardBg.sprite = panelSprite;
            cardBg.preserveAspect = false;
        }

        Color panelColor;
        if (completed)
            panelColor = new Color(0.25f, 0.55f, 0.25f, 0.9f);
        else if (unlocked)
            panelColor = new Color(0.3f, 0.38f, 0.55f, 0.95f);
        else
            panelColor = new Color(0.35f, 0.35f, 0.38f, 0.7f);
        cardBg.color = panelColor;

        Outline cardOutline = cardObj.AddComponent<Outline>();
        cardOutline.effectColor = completed ? new Color(0.2f, 0.8f, 0.2f, 0.5f) : new Color(0, 0, 0, 0.3f);
        cardOutline.effectDistance = new Vector2(2, -2);

        GameObject numObj = new GameObject("Number");
        numObj.transform.SetParent(cardObj.transform, false);
        Text numText = numObj.AddComponent<Text>();
        numText.font = font;
        numText.fontSize = 24;
        numText.alignment = TextAnchor.MiddleCenter;
        numText.text = level.id.ToString("D2");
        numText.color = completed ? new Color(0.4f, 1f, 0.4f) : unlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
        Outline numOutline = numObj.AddComponent<Outline>();
        numOutline.effectColor = Color.black;
        numOutline.effectDistance = new Vector2(1, -1);
        RectTransform numRt = numObj.GetComponent<RectTransform>();
        numRt.anchorMin = new Vector2(0f, 0.3f);
        numRt.anchorMax = new Vector2(0.22f, 0.95f);
        numRt.sizeDelta = Vector2.zero;

        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(cardObj.transform, false);
        Text nameText = nameObj.AddComponent<Text>();
        nameText.font = font;
        nameText.fontSize = 9;
        nameText.alignment = TextAnchor.MiddleLeft;
        nameText.text = level.name;
        nameText.color = completed ? new Color(0.7f, 1f, 0.7f) : unlocked ? new Color(0.85f, 0.85f, 0.85f) : new Color(0.45f, 0.45f, 0.45f);
        Outline nameOutline = nameObj.AddComponent<Outline>();
        nameOutline.effectColor = Color.black;
        nameOutline.effectDistance = new Vector2(1, -1);
        RectTransform nameRt = nameObj.GetComponent<RectTransform>();
        nameRt.anchorMin = new Vector2(0.22f, 0.52f);
        nameRt.anchorMax = new Vector2(0.6f, 0.95f);
        nameRt.sizeDelta = Vector2.zero;

        GameObject infoObj = new GameObject("Info");
        infoObj.transform.SetParent(cardObj.transform, false);
        Text infoText = infoObj.AddComponent<Text>();
        infoText.font = font;
        infoText.fontSize = 7;
        infoText.alignment = TextAnchor.MiddleLeft;
        string enemyLabel = level.enemyRace.ToUpper();
        int pLen = level.powerups != null ? level.powerups.Length : 0;
        int oLen = level.obstacles != null ? level.obstacles.Length : 0;
        infoText.text = $"{enemyLabel} | E:{level.enemyCount} | +{level.goldReward}g";
        infoText.color = completed ? new Color(0.5f, 0.8f, 0.5f, 0.8f) : unlocked ? new Color(0.7f, 0.7f, 0.7f) : new Color(0.4f, 0.4f, 0.4f, 0.6f);
        RectTransform infoRt = infoObj.GetComponent<RectTransform>();
        infoRt.anchorMin = new Vector2(0.22f, 0.05f);
        infoRt.anchorMax = new Vector2(0.6f, 0.48f);
        infoRt.sizeDelta = Vector2.zero;

        Sprite enemyIcon = GetRaceIcon(level.enemyRace);
        if (enemyIcon != null)
        {
            GameObject enemyObj = new GameObject("EnemyIcon");
            enemyObj.transform.SetParent(cardObj.transform, false);
            Image enemyImg = enemyObj.AddComponent<Image>();
            enemyImg.sprite = enemyIcon;
            enemyImg.preserveAspect = true;
            enemyImg.raycastTarget = false;
            enemyImg.color = unlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);
            RectTransform eRt = enemyObj.GetComponent<RectTransform>();
            eRt.anchorMin = new Vector2(0.62f, 0.15f);
            eRt.anchorMax = new Vector2(0.78f, 0.85f);
            eRt.sizeDelta = Vector2.zero;
        }

        Sprite insigniaSprite = GetInsigniaSprite(level.insigniaId);
        if (insigniaSprite != null)
        {
            GameObject insObj = new GameObject("RewardInsignia");
            insObj.transform.SetParent(cardObj.transform, false);
            Image insImg = insObj.AddComponent<Image>();
            insImg.sprite = insigniaSprite;
            insImg.preserveAspect = true;
            insImg.raycastTarget = false;
            insImg.color = completed ? Color.white : new Color(0.4f, 0.4f, 0.4f, 0.5f);
            RectTransform iRt = insObj.GetComponent<RectTransform>();
            iRt.anchorMin = new Vector2(0.74f, 0.15f);
            iRt.anchorMax = new Vector2(0.90f, 0.85f);
            iRt.sizeDelta = Vector2.zero;
        }

        if (completed)
        {
            GameObject checkObj = new GameObject("Check");
            checkObj.transform.SetParent(cardObj.transform, false);
            Text checkText = checkObj.AddComponent<Text>();
            checkText.font = font;
            checkText.fontSize = 14;
            checkText.alignment = TextAnchor.MiddleCenter;
            checkText.text = "\u2713";
            checkText.color = new Color(0.3f, 1f, 0.3f);
            Outline checkOutline = checkObj.AddComponent<Outline>();
            checkOutline.effectColor = Color.black;
            checkOutline.effectDistance = new Vector2(1, -1);
            RectTransform checkRt = checkObj.GetComponent<RectTransform>();
            checkRt.anchorMin = new Vector2(0.75f, 0.5f);
            checkRt.anchorMax = new Vector2(0.9f, 0.95f);
            checkRt.sizeDelta = Vector2.zero;
        }

        if (unlocked)
        {
            Button btn = cardObj.AddComponent<Button>();
            btn.targetGraphic = cardBg;
            int capturedId = level.id;
            btn.onClick.AddListener(() => OnLevelClicked(capturedId));
        }
    }

    void OnLevelClicked(int levelId)
    {
        SoundManager.Instance.PlaySelect();
        var level = CampaignData.GetLevel(levelId);
        if (level != null)
        {
            GameConfig.selectedScenario = level.enemyRace;
            PlayerPrefs.Save();
        }
        GameConfig.PlayCampaign(levelId);
    }

    Sprite GetCupSprite(string race)
    {
        string cupPath = race switch
        {
            "Human" => "Sprites/Menu/copaHuman",
            "Orc" => "Sprites/Menu/copaOrc",
            "Beastfolk" => "Sprites/Menu/copaBeast",
            "Nigromantes" => "Sprites/Menu/copaMenu",
            _ => "Sprites/Menu/copaMenu"
        };
        Sprite[] sprites = Resources.LoadAll<Sprite>(cupPath);
        if (sprites == null || sprites.Length == 0) return null;
        foreach (var s in sprites)
        {
            if (s.name.EndsWith("_0") || s.name == cupPath.Split('/')[cupPath.Split('/').Length - 1]) return s;
        }
        return sprites[0];
    }

    Sprite GetRaceIcon(string race)
    {
        string scenarioTheme = race switch
        {
            "Human" => "Human",
            "Orc" => "Orc",
            "Wolf" => "Wolf",
            "Beastfolk" => "Wolf",
            "NewRace" => "Nigromantes",
            _ => "Human"
        };
        string iconPath = $"Sprites/{scenarioTheme}/Icono";
        Sprite[] sprites = Resources.LoadAll<Sprite>(iconPath);
        if (sprites != null && sprites.Length > 0)
        {
            foreach (var s in sprites)
            {
                if (s.name.Contains("Peon") || s.name.Contains("peon") || s.name.Contains("_0")) return s;
            }
            return sprites[0];
        }
        return null;
    }

    Sprite GetInsigniaSprite(string insigniaId)
    {
        if (string.IsNullOrEmpty(insigniaId)) return null;
        Insignia ins = InsigniaData.GetInsignia(insigniaId);
        return InsigniaSprites.Get(ins);
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

    void CreateBackButton()
    {
        Sprite[] backSprites = Resources.LoadAll<Sprite>("Sprites/Menu/botin ui/panel total back");
        Sprite backSprite = backSprites != null && backSprites.Length > 0
            ? (System.Array.Find(backSprites, s => s.name == "panel total back_0") ?? backSprites[0])
            : null;

        GameObject btnObj = new GameObject("BackButton");
        btnObj.transform.SetParent(panelObj.transform, false);
        RectTransform btnRt = btnObj.AddComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0f, 1f);
        btnRt.anchorMax = new Vector2(0f, 1f);
        btnRt.pivot = new Vector2(0f, 1f);
        btnRt.sizeDelta = new Vector2(150, 70);
        btnRt.anchoredPosition = new Vector2(15, -10);

        Image btnImg = btnObj.AddComponent<Image>();
        if (backSprite != null)
            btnImg.sprite = backSprite;
        else
            btnImg.color = new Color(0.4f, 0.2f, 0.1f, 0.85f);
        btnImg.preserveAspect = true;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(() => { SoundManager.Instance.PlayButton(); Close(); });
    }

    void CreateTitle()
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelObj.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.font = font;
        titleText.fontSize = 20;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.text = "CAMPAIGN";
        titleText.color = new Color(0.9f, 0.75f, 0.3f);
        Outline titleOutline = titleObj.AddComponent<Outline>();
        titleOutline.effectColor = Color.black;
        titleOutline.effectDistance = new Vector2(1, -1);
        RectTransform tRt = titleObj.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0f, 1f);
        tRt.anchorMax = new Vector2(1f, 1f);
        tRt.pivot = new Vector2(0.5f, 1f);
        tRt.sizeDelta = new Vector2(0, 45);
        tRt.anchoredPosition = new Vector2(0, -10);
    }
}
