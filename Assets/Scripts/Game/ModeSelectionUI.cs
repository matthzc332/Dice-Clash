using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ModeSelectionUI : MonoBehaviour
{
    private GameObject panel;
    private Canvas canvas;
    private Font pressStart;
    private Text goldText;
    private bool isCampaignMode;
    private int targetLevelId = -1;
    public System.Action<PowerupMode> OnCampaignLevelSelected;
    public System.Action onBack;

    public void Show(Canvas parentCanvas)
    {
        canvas = parentCanvas;
        isCampaignMode = false;
        pressStart = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        if (pressStart == null) pressStart = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        CreatePanel();
    }

    public void ShowCampaign(Canvas parentCanvas)
    {
        ShowCampaign(parentCanvas, -1);
    }

    public void ShowCampaign(Canvas parentCanvas, int levelId)
    {
        canvas = parentCanvas;
        isCampaignMode = true;
        targetLevelId = levelId;
        pressStart = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        if (pressStart == null) pressStart = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        CreatePanel();
    }

    void CreatePanel()
    {
        panel = new GameObject("ModeSelectionPanel");
        panel.transform.SetParent(canvas.transform, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.06f, 0.04f, 0.1f, 0.95f);

        if (isCampaignMode)
        {
            CreateCampaignTitle();
            if (targetLevelId > 0) CreateEnemyPreview();
            CreateCampaignTickets();
        }
        else
        {
            CreateTitle();
            CreateTickets();
        }
        CreateGoldDisplay();
        CreateBackButton();
    }

    void CreateCampaignTitle()
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.font = pressStart;
        titleText.fontSize = 22;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.text = "SELECT CAMPAIGN MODE";
        titleText.color = new Color(0.9f, 0.75f, 0.2f);
        RectTransform tRt = titleObj.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0f, 0.85f);
        tRt.anchorMax = new Vector2(1f, 1f);
        tRt.sizeDelta = Vector2.zero;
    }

    void CreateEnemyPreview()
    {
        var level = CampaignData.GetLevel(targetLevelId);
        if (level == null) return;

        Color raceColor = GetRaceColor(level.enemyRace);

        GameObject holder = new GameObject("EnemyPreview", typeof(RectTransform));
        holder.transform.SetParent(panel.transform, false);
        RectTransform hRt = holder.GetComponent<RectTransform>();
        hRt.anchorMin = new Vector2(0.5f, 0.5f);
        hRt.anchorMax = new Vector2(0.5f, 0.5f);
        hRt.pivot = new Vector2(0.5f, 0.5f);
        hRt.sizeDelta = new Vector2(720, 120);
        hRt.anchoredPosition = new Vector2(0, 322);

        Image bgImg = holder.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.07f, 0.05f, 0.88f);
        Outline bgOutline = holder.AddComponent<Outline>();
        bgOutline.effectColor = raceColor;
        bgOutline.effectDistance = new Vector2(3, -3);

        Sprite icon = GetRaceIcon(level.enemyRace);
        if (icon != null)
        {
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(holder.transform, false);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = icon;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            RectTransform iRt = iconObj.GetComponent<RectTransform>();
            iRt.anchorMin = new Vector2(0.03f, 0.5f);
            iRt.anchorMax = new Vector2(0.03f, 0.5f);
            iRt.pivot = new Vector2(0.5f, 0.5f);
            iRt.sizeDelta = new Vector2(90, 90);
            iRt.anchoredPosition = Vector2.zero;
        }

        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(holder.transform, false);
        Text labelText = labelObj.AddComponent<Text>();
        labelText.font = pressStart;
        labelText.text = "NEXT ENEMY";
        labelText.fontSize = 11;
        labelText.alignment = TextAnchor.UpperLeft;
        labelText.color = raceColor;
        labelText.raycastTarget = false;
        RectTransform lRt = labelObj.GetComponent<RectTransform>();
        lRt.anchorMin = new Vector2(icon != null ? 0.2f : 0.05f, 0.52f);
        lRt.anchorMax = new Vector2(0.95f, 0.85f);
        lRt.sizeDelta = Vector2.zero;

        GameObject nameObj = new GameObject("ArmyName");
        nameObj.transform.SetParent(holder.transform, false);
        Text nameText = nameObj.AddComponent<Text>();
        nameText.font = pressStart;
        nameText.text = level.name.ToUpper();
        nameText.fontSize = 18;
        nameText.alignment = TextAnchor.MiddleLeft;
        nameText.color = Color.white;
        Outline nameOutline = nameObj.AddComponent<Outline>();
        nameOutline.effectColor = Color.black;
        nameOutline.effectDistance = new Vector2(1.5f, -1.5f);
        nameText.raycastTarget = false;
        RectTransform nRt = nameObj.GetComponent<RectTransform>();
        nRt.anchorMin = new Vector2(icon != null ? 0.2f : 0.05f, 0.12f);
        nRt.anchorMax = new Vector2(0.95f, 0.55f);
        nRt.sizeDelta = Vector2.zero;

        StartCoroutine(EnemyPreviewPulse(hRt));
    }

    IEnumerator EnemyPreviewPulse(RectTransform rt)
    {
        while (rt != null)
        {
            float pulse = 1f + Mathf.Sin(Time.time * 2f) * 0.015f;
            rt.localScale = Vector3.one * pulse;
            yield return null;
        }
    }

    void CreateCampaignTickets()
    {
        CreateTicketCard("FREE TICKET", "NO POWER-UPS", 0, new Vector2(-190, 30),
            new Color(0.15f, 0.25f, 0.55f), new Color(0.3f, 0.5f, 0.95f),
            PowerupMode.WithoutPowerups, true);

        CreateTicketCard("PREMIUM TICKET", "WITH POWER-UPS", GetCampaignEntryCost(), new Vector2(190, 30),
            new Color(0.2f, 0.45f, 0.2f), new Color(0.4f, 0.8f, 0.4f),
            PowerupMode.WithPowerups, true);
    }

    void CreateTitle()
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.font = pressStart;
        titleText.fontSize = 22;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.text = "SELECT RANKED MODE";
        titleText.color = new Color(0.9f, 0.75f, 0.2f);
        RectTransform tRt = titleObj.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0f, 0.85f);
        tRt.anchorMax = new Vector2(1f, 1f);
        tRt.sizeDelta = Vector2.zero;
    }

    void CreateTickets()
    {
        CreateTicketCard("FREE TICKET", "NO POWER-UPS", 0, new Vector2(-190, 30),
            new Color(0.15f, 0.25f, 0.55f), new Color(0.3f, 0.5f, 0.95f),
            PowerupMode.WithoutPowerups, false);

        CreateTicketCard("PREMIUM TICKET", "WITH POWER-UPS", 30, new Vector2(190, 30),
            new Color(0.2f, 0.45f, 0.2f), new Color(0.4f, 0.8f, 0.4f),
            PowerupMode.WithPowerups, false);
    }

    int GetCampaignEntryCost()
    {
        int cup = 1;
        if (targetLevelId > 0)
        {
            CampaignLevel level = CampaignData.GetLevel(targetLevelId);
            cup = level != null ? level.cup : 0;
        }
        return EconomyConfig.GetLevelEntryCost(cup);
    }

    void CreateTicketCard(string title, string subtitle, int cost, Vector2 pos,
        Color bgColor, Color accentColor, PowerupMode powerupMode, bool campaign = false)
    {
        GameObject cardObj = new GameObject("TicketCard");
        cardObj.transform.SetParent(panel.transform, false);

        RectTransform cardRt = cardObj.AddComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 0.5f);
        cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.pivot = new Vector2(0.5f, 0.5f);
        cardRt.sizeDelta = new Vector2(416, 416);
        cardRt.anchoredPosition = pos;

        Image cardBg = cardObj.AddComponent<Image>();
        cardBg.sprite = LoadFirstSprite("Sprites/Menu/panelCartaBlue", "panelCartaBlue");
        if (cardBg.sprite != null)
            cardBg.color = bgColor;
        else
            cardBg.color = bgColor;

        Outline cardOutline = cardObj.AddComponent<Outline>();
        cardOutline.effectColor = accentColor;
        cardOutline.effectDistance = new Vector2(3, -3);

        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(cardObj.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.font = pressStart;
        titleText.text = title;
        titleText.fontSize = 18;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = accentColor;
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.05f, 0.7f);
        titleRt.anchorMax = new Vector2(0.95f, 0.88f);
        titleRt.sizeDelta = Vector2.zero;

        GameObject subtitleObj = new GameObject("Subtitle");
        subtitleObj.transform.SetParent(cardObj.transform, false);
        Text subtitleText = subtitleObj.AddComponent<Text>();
        subtitleText.font = pressStart;
        subtitleText.text = subtitle;
        subtitleText.fontSize = 13;
        subtitleText.alignment = TextAnchor.MiddleCenter;
        subtitleText.color = new Color(0.7f, 0.7f, 0.7f);
        RectTransform subtitleRt = subtitleObj.GetComponent<RectTransform>();
        subtitleRt.anchorMin = new Vector2(0.05f, 0.56f);
        subtitleRt.anchorMax = new Vector2(0.95f, 0.7f);
        subtitleRt.sizeDelta = Vector2.zero;

        string priceLabel = cost == 0 ? "FREE!" : $"COST: {cost}G";
        Color priceColor = cost == 0
            ? new Color(0.4f, 1f, 0.4f)
            : new Color(1f, 0.84f, 0f);

        GameObject priceObj = new GameObject("Price");
        priceObj.transform.SetParent(cardObj.transform, false);
        Text priceText = priceObj.AddComponent<Text>();
        priceText.font = pressStart;
        priceText.text = priceLabel;
        priceText.fontSize = 21;
        priceText.alignment = TextAnchor.MiddleCenter;
        priceText.color = priceColor;
        RectTransform priceRt = priceObj.GetComponent<RectTransform>();
        priceRt.anchorMin = new Vector2(0.05f, 0.42f);
        priceRt.anchorMax = new Vector2(0.95f, 0.56f);
        priceRt.sizeDelta = Vector2.zero;

        string descText = cost == 0
            ? "No gold cost.\nEnemies may use items."
            : "Pay gold to enter.\nFull experience.";
        GameObject descObj = new GameObject("Desc");
        descObj.transform.SetParent(cardObj.transform, false);
        Text desc = descObj.AddComponent<Text>();
        desc.font = pressStart;
        desc.text = descText;
        desc.fontSize = 12;
        desc.alignment = TextAnchor.MiddleCenter;
        desc.color = new Color(0.6f, 0.6f, 0.6f);
        RectTransform descRt = descObj.GetComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0.08f, 0.16f);
        descRt.anchorMax = new Vector2(0.92f, 0.38f);
        descRt.sizeDelta = Vector2.zero;

        GameObject btnObj = new GameObject("PlayBtn");
        btnObj.transform.SetParent(cardObj.transform, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.sprite = LoadFirstSprite("Sprites/Menu/botin ui/botonOpen", "botonOpen_0");
        if (btnImg.sprite != null)
        {
            btnImg.preserveAspect = true;
            btnImg.color = Color.white;
        }
        else
        {
            btnImg.color = accentColor;
        }
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0.02f);
        btnRt.anchorMax = new Vector2(0.5f, 0.02f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.sizeDelta = new Vector2(221, 71);

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        int capturedCost = cost;
        PowerupMode capturedMode = powerupMode;
        btn.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySelect();
            if (isCampaignMode)
                OnCampaignTicketSelected(capturedCost, capturedMode, cardObj);
            else
                OnTicketSelected(capturedCost, capturedMode, cardObj);
        });
        Button cardBtn = cardObj.AddComponent<Button>();
        Image cardBtnImg = cardObj.GetComponent<Image>();
        cardBtn.targetGraphic = cardBtnImg;
        cardBtn.onClick.AddListener(() =>
        {
            if (cost > 0)
                SoundManager.Instance.PlayVictory();
            else
                SoundManager.Instance.PlaySelect();
            if (isCampaignMode)
                OnCampaignTicketSelected(capturedCost, capturedMode, cardObj);
            else
                OnTicketSelected(capturedCost, capturedMode, cardObj);
        });

        if (cost > 0 && campaign)
        {
            StartCoroutine(HeartbeatPulse(btnObj));
            HoverGrow hover = btnObj.AddComponent<HoverGrow>();
        }

        if (cost > 0)
            SpawnPremiumAura(cardObj);
    }

    void OnCampaignTicketSelected(int cost, PowerupMode powerupMode, GameObject cardObj)
    {
        EconomyManager econ = EconomyManager.Instance;
        int gold = econ != null ? econ.TotalGold : 0;

        if (gold < cost)
        {
            ShowMessage("NOT ENOUGH GOLD!");
            return;
        }

        if (econ != null && cost > 0)
            econ.SpendGold(cost);

        if (goldText != null)
        {
            int newGold = econ != null ? econ.TotalGold : 0;
            goldText.text = $"GOLD: {EconomyManager.FormatGold(newGold)}";
        }

        StartCoroutine(FlashAndStartCampaign(cardObj, powerupMode));
    }

    IEnumerator FlashAndStartCampaign(GameObject cardObj, PowerupMode powerupMode)
    {
        yield return StartCoroutine(FlashCard(cardObj));

        if (targetLevelId > 0)
        {
            GameConfig.PlayCampaign(targetLevelId, powerupMode);
            yield break;
        }

        if (OnCampaignLevelSelected != null)
        {
            OnCampaignLevelSelected.Invoke(powerupMode);
            yield break;
        }

        int firstLevel = 3;
        if (CampaignManager.Instance != null)
        {
            var data = CampaignData.Load();
            if (data != null && data.cups != null && data.cups.Length > 1)
            {
                foreach (int lid in data.cups[1].levels)
                {
                    if (!CampaignManager.Instance.IsLevelCompleted(lid) && CampaignManager.Instance.IsLevelUnlocked(lid))
                    {
                        firstLevel = lid;
                        break;
                    }
                }
            }
        }
        GameConfig.PlayCampaign(firstLevel, powerupMode);
    }

    IEnumerator HeartbeatPulse(GameObject btnObj)
    {
        while (btnObj != null)
        {
            yield return new WaitForSeconds(0.5f);
            if (btnObj == null) yield break;
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            if (rt == null) yield break;
            Vector3 orig = rt.localScale;
            float t = 0f;
            while (t < 0.15f)
            {
                if (btnObj == null) yield break;
                t += Time.unscaledDeltaTime;
                float scale = 1f + Mathf.Sin(t / 0.15f * Mathf.PI) * 0.12f;
                rt.localScale = orig * scale;
                yield return null;
            }
            rt.localScale = orig;
            yield return new WaitForSeconds(0.35f);
            if (btnObj == null) yield break;
            rt = btnObj.GetComponent<RectTransform>();
            if (rt == null) yield break;
            orig = rt.localScale;
            t = 0f;
            while (t < 0.12f)
            {
                if (btnObj == null) yield break;
                t += Time.unscaledDeltaTime;
                float scale = 1f + Mathf.Sin(t / 0.12f * Mathf.PI) * 0.08f;
                rt.localScale = orig * scale;
                yield return null;
            }
            rt.localScale = orig;
        }
    }

    IEnumerator FlashCard(GameObject cardObj)
    {
        if (cardObj != null)
        {
            Image cardBg = cardObj.GetComponent<Image>();
            if (cardBg != null)
            {
                Color orig = cardBg.color;
                cardBg.color = Color.white;
                yield return new WaitForSecondsRealtime(0.12f);
                cardBg.color = orig;
                yield return new WaitForSecondsRealtime(0.08f);
                cardBg.color = new Color(orig.r + 0.3f, orig.g + 0.3f, orig.b + 0.3f, orig.a);
                yield return new WaitForSecondsRealtime(0.1f);
                cardBg.color = orig;
            }
        }
        SoundManager.Instance.PlayVictory();
        yield return new WaitForSecondsRealtime(0.15f);
    }

    IEnumerator FlashAndStartRanked(GameObject cardObj, PowerupMode powerupMode)
    {
        yield return StartCoroutine(FlashCard(cardObj));
        GameConfig.PlayRanked(powerupMode);
    }

    void SpawnPremiumAura(GameObject cardObj)
    {
        if (cardObj == null) return;
        GameObject auraObj = new GameObject("PremiumAura", typeof(RectTransform));
        auraObj.transform.SetParent(cardObj.transform, false);
        Image auraImg = auraObj.AddComponent<Image>();
        auraImg.sprite = BuildGoldBorderSprite();
        auraImg.color = new Color(1f, 0.9f, 0.45f, 0.07f);
        auraImg.raycastTarget = false;
        RectTransform auraRt = auraObj.GetComponent<RectTransform>();
        auraRt.anchorMin = Vector2.zero;
        auraRt.anchorMax = Vector2.one;
        auraRt.offsetMin = Vector2.zero;
        auraRt.offsetMax = Vector2.zero;
        auraRt.SetSiblingIndex(0);
        StartCoroutine(PulsePremiumAura(auraImg, auraRt));
    }

    IEnumerator PulsePremiumAura(Image aura, RectTransform rt)
    {
        float t = 0f;
        while (aura != null && rt != null)
        {
            float p = (Mathf.Sin(t * 3f) + 1f) * 0.5f;
            aura.color = new Color(1f, 0.9f, 0.45f, 0.04f + p * 0.05f);
            float s = 1f + p * 0.02f;
            rt.localScale = new Vector3(s, s, 1f);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    static Sprite BuildGoldBorderSprite()
    {
        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color gold = new Color(1f, 0.9f, 0.45f);
        float half = size / 2f;
        float thicknessN = 0.05f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - half) / half;
                float dy = Mathf.Abs(y - half) / half;
                float dmax = Mathf.Max(dx, dy);
                float fromEdge = 1f - dmax;
                float alpha = 0f;
                if (fromEdge <= thicknessN)
                {
                    float glow = 1f - fromEdge / thicknessN;
                    alpha = glow * glow;
                }
                tex.SetPixel(x, y, new Color(gold.r, gold.g, gold.b, alpha));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    void OnTicketSelected(int cost, PowerupMode powerupMode, GameObject cardObj)
    {
        EconomyManager econ = EconomyManager.Instance;
        int gold = econ != null ? econ.TotalGold : 0;

        if (gold < cost)
        {
            ShowMessage("NOT ENOUGH GOLD!");
            return;
        }

        if (!RankedManager.IsUnlocked())
        {
            ShowMessage("COMPLETE CAMPAIGN FIRST!");
            return;
        }

        if (econ != null && cost > 0)
            econ.SpendGold(cost);

        if (goldText != null)
        {
            int newGold = econ != null ? econ.TotalGold : 0;
            goldText.text = $"GOLD: {EconomyManager.FormatGold(newGold)}";
        }

        if (cost > 0)
        {
            StartCoroutine(FlashAndStartRanked(cardObj, powerupMode));
        }
        else
        {
            GameConfig.PlayRanked(powerupMode);
        }
    }

    void ShowMessage(string msg)
    {
        SoundManager.Instance.PlayButton();
        StartCoroutine(MessageSequence(msg));
    }

    IEnumerator MessageSequence(string msg)
    {
        GameObject msgObj = new GameObject("Message");
        msgObj.transform.SetParent(panel.transform, false);

        RectTransform msgRt = msgObj.AddComponent<RectTransform>();
        msgRt.anchorMin = new Vector2(0.5f, 0.5f);
        msgRt.anchorMax = new Vector2(0.5f, 0.5f);
        msgRt.sizeDelta = new Vector2(400, 50);

        Image msgBg = msgObj.AddComponent<Image>();
        msgBg.color = new Color(0.5f, 0.1f, 0.1f, 0.95f);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(msgObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.font = pressStart;
        text.text = msg;
        text.fontSize = 10;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        yield return new WaitForSeconds(1.5f);
        Destroy(msgObj);
    }

    void CreateGoldDisplay()
    {
        EconomyManager econ = EconomyManager.Instance;
        int gold = econ != null ? econ.TotalGold : 0;

        GameObject goldObj = new GameObject("GoldDisplay");
        goldObj.transform.SetParent(panel.transform, false);
        goldText = goldObj.AddComponent<Text>();
        goldText.font = pressStart;
        goldText.text = $"GOLD: {EconomyManager.FormatGold(gold)}";
        goldText.fontSize = 40;
        goldText.alignment = TextAnchor.MiddleCenter;
        goldText.color = new Color(1f, 0.84f, 0f);
        Outline goldOutline = goldObj.AddComponent<Outline>();
        goldOutline.effectColor = Color.black;
        goldOutline.effectDistance = new Vector2(2, -2);
        RectTransform gRt = goldObj.GetComponent<RectTransform>();
        gRt.anchorMin = new Vector2(0f, 0f);
        gRt.anchorMax = new Vector2(1f, 0.2f);
        gRt.sizeDelta = Vector2.zero;
    }

    void CreateBackButton()
    {
        Sprite backSprite = LoadBackSpriteRobust("Sprites/Menu/botin ui/panel total back");

        GameObject btnObj = new GameObject("BackBtn");
        btnObj.transform.SetParent(panel.transform, false);

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
            btnImg.color = new Color(0.3f, 0.2f, 0.15f, 0.9f);
        btnImg.preserveAspect = true;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlayButton();
            Destroy(panel);
            if (onBack != null)
            {
                onBack.Invoke();
            }
            else
            {
                SceneManager.LoadScene("MainMenuScene");
            }
        });
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

    Sprite LoadBackSpriteRobust(string path)
    {
        Texture2D tex = Resources.Load<Texture2D>(path);
        if (tex != null)
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        Sprite s = Resources.Load<Sprite>(path);
        if (s != null) return s;
        Sprite[] arr = Resources.LoadAll<Sprite>(path);
        return arr != null && arr.Length > 0 ? arr[0] : null;
    }

    Color GetRaceColor(string race)
    {
        switch (race)
        {
            case "Human": return new Color(0.3f, 0.5f, 1f);
            case "Orc": return new Color(0.3f, 0.8f, 0.3f);
            case "Wolf": return new Color(0.8f, 0.5f, 0.2f);
            case "Beastfolk": return new Color(0.6f, 0.8f, 0.3f);
            case "NewRace": return new Color(0.6f, 0.3f, 0.8f);
            default: return Color.white;
        }
    }

    Sprite GetRaceIcon(string race)
    {
        string resolved = BoardManager.SpriteFolder(race);
        Sprite[] sprites = Resources.LoadAll<Sprite>($"Sprites/{resolved}/Icono");
        if (sprites == null || sprites.Length == 0) return null;
        foreach (var s in sprites)
        {
            if (s.name.Contains("Peon") || s.name.Contains("peon") || s.name.Contains("_0")) return s;
        }
        return sprites[0];
    }
}
