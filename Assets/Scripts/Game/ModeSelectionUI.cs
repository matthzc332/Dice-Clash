using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ModeSelectionUI : MonoBehaviour
{
    private GameObject panel;
    private Canvas canvas;
    private Font pressStart;
    private Text goldText;
    private bool isCampaignMode;

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
        canvas = parentCanvas;
        isCampaignMode = true;
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
            CreateCampaignTickets();
        }
        else
        {
            CreateTitle();
            CreateTickets();
        }
        CreateGoldDisplay();
        if (!isCampaignMode) CreateBackButton();
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

    void CreateCampaignTickets()
    {
        CreateTicketCard("FREE TICKET", "NO POWER-UPS", 0, new Vector2(-190, 30),
            new Color(0.2f, 0.45f, 0.2f), new Color(0.4f, 0.8f, 0.4f),
            PowerupMode.WithoutPowerups, true);

        CreateTicketCard("PREMIUM TICKET", "WITH POWER-UPS", 20, new Vector2(190, 30),
            new Color(0.45f, 0.2f, 0.5f), new Color(0.8f, 0.4f, 1f),
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
            new Color(0.2f, 0.45f, 0.2f), new Color(0.4f, 0.8f, 0.4f),
            PowerupMode.WithoutPowerups, false);

        CreateTicketCard("PREMIUM TICKET", "WITH POWER-UPS", 30, new Vector2(190, 30),
            new Color(0.45f, 0.2f, 0.5f), new Color(0.8f, 0.4f, 1f),
            PowerupMode.WithPowerups, false);
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
        cardRt.sizeDelta = new Vector2(320, 320);
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
        titleText.fontSize = 14;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = accentColor;
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.05f, 0.78f);
        titleRt.anchorMax = new Vector2(0.95f, 0.95f);
        titleRt.sizeDelta = Vector2.zero;

        GameObject subtitleObj = new GameObject("Subtitle");
        subtitleObj.transform.SetParent(cardObj.transform, false);
        Text subtitleText = subtitleObj.AddComponent<Text>();
        subtitleText.font = pressStart;
        subtitleText.text = subtitle;
        subtitleText.fontSize = 8;
        subtitleText.alignment = TextAnchor.MiddleCenter;
        subtitleText.color = new Color(0.7f, 0.7f, 0.7f);
        RectTransform subtitleRt = subtitleObj.GetComponent<RectTransform>();
        subtitleRt.anchorMin = new Vector2(0.05f, 0.65f);
        subtitleRt.anchorMax = new Vector2(0.95f, 0.78f);
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
        priceText.fontSize = 16;
        priceText.alignment = TextAnchor.MiddleCenter;
        priceText.color = priceColor;
        RectTransform priceRt = priceObj.GetComponent<RectTransform>();
        priceRt.anchorMin = new Vector2(0.05f, 0.45f);
        priceRt.anchorMax = new Vector2(0.95f, 0.6f);
        priceRt.sizeDelta = Vector2.zero;

        string descText = cost == 0
            ? "No gold cost.\nEnemies may use items."
            : "Pay gold to enter.\nFull experience.";
        GameObject descObj = new GameObject("Desc");
        descObj.transform.SetParent(cardObj.transform, false);
        Text desc = descObj.AddComponent<Text>();
        desc.font = pressStart;
        desc.text = descText;
        desc.fontSize = 7;
        desc.alignment = TextAnchor.MiddleCenter;
        desc.color = new Color(0.6f, 0.6f, 0.6f);
        RectTransform descRt = descObj.GetComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0.08f, 0.18f);
        descRt.anchorMax = new Vector2(0.92f, 0.42f);
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
        btnRt.sizeDelta = new Vector2(170, 55);

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
                OnTicketSelected(capturedCost, capturedMode);
        });

        if (cost > 0 && campaign)
        {
            StartCoroutine(HeartbeatPulse(btnObj));
            HoverGrow hover = btnObj.AddComponent<HoverGrow>();
        }
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
            goldText.text = $"GOLD: {newGold}";
        }

        StartCoroutine(FlashAndStartCampaign(cardObj, powerupMode));
    }

    IEnumerator FlashAndStartCampaign(GameObject cardObj, PowerupMode powerupMode)
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

    class HoverGrow : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler
    {
        private bool hovering;
        private Coroutine heartbeat;

        public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
        {
            hovering = true;
            transform.localScale = Vector3.one * 1.08f;
        }

        public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
        {
            hovering = false;
            transform.localScale = Vector3.one;
        }
    }

    void OnTicketSelected(int cost, PowerupMode powerupMode)
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
            goldText.text = $"GOLD: {newGold}";
        }

        GameConfig.PlayRanked(powerupMode);
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
        goldText.text = $"GOLD: {gold}";
        goldText.fontSize = 12;
        goldText.alignment = TextAnchor.MiddleCenter;
        goldText.color = new Color(1f, 0.84f, 0f);
        RectTransform gRt = goldObj.GetComponent<RectTransform>();
        gRt.anchorMin = new Vector2(0f, 0f);
        gRt.anchorMax = new Vector2(1f, 0.08f);
        gRt.sizeDelta = Vector2.zero;
    }

    void CreateBackButton()
    {
        Sprite[] backSprites = Resources.LoadAll<Sprite>("Sprites/Menu/botin ui/panel total back");
        Sprite backSprite = backSprites != null && backSprites.Length > 0
            ? (System.Array.Find(backSprites, s => s.name == "panel total back_0") ?? backSprites[0])
            : null;

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
}
