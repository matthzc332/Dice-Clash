using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ModeSelectionUI : MonoBehaviour
{
    private GameObject panel;
    private Canvas canvas;
    private Font pressStart;

    public void Show(Canvas parentCanvas)
    {
        canvas = parentCanvas;
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

        CreateTitle();
        CreateModeButtons();
        CreateGoldDisplay();
        CreateBackButton();
    }

    void CreateTitle()
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.font = pressStart;
        titleText.fontSize = 20;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.text = "SELECT MODE";
        titleText.color = new Color(0.9f, 0.75f, 0.2f);
        RectTransform tRt = titleObj.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0f, 0.85f);
        tRt.anchorMax = new Vector2(1f, 1f);
        tRt.sizeDelta = Vector2.zero;
    }

    void CreateModeButtons()
    {
        float[] xPos = { -180f, 180f };
        float[] yPos = { 80f, -80f };
        string[] labels = {
            "CAMPAIGN\nWITH ITEMS",
            "RANKED\nWITH ITEMS",
            "CAMPAIGN\nNO ITEMS",
            "RANKED\nNO ITEMS"
        };
        int[] costs = { 20, 30, 10, 15 };
        bool[] isRanked = { false, true, false, true };
        bool[] hasPowerups = { true, true, false, false };

        for (int i = 0; i < 4; i++)
        {
            int idx = i;
            CreateModeButton(
                labels[i],
                new Vector2(xPos[i % 2], yPos[i / 2]),
                costs[i],
                isRanked[i],
                hasPowerups[i],
                () => OnModeSelected(isRanked[idx], hasPowerups[idx], costs[idx])
            );
        }
    }

    void CreateModeButton(string label, Vector2 position, int cost, bool isRankedMode, bool hasPowerups, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject("ModeBtn");
        btnObj.transform.SetParent(panel.transform, false);

        RectTransform btnRt = btnObj.AddComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0.5f);
        btnRt.anchorMax = new Vector2(0.5f, 0.5f);
        btnRt.sizeDelta = new Vector2(300, 120);
        btnRt.anchoredPosition = position;

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = isRankedMode ? new Color(0.3f, 0.15f, 0.4f, 0.9f) : new Color(0.15f, 0.25f, 0.4f, 0.9f);

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySelect();
            onClick();
        });

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
        textRt.sizeDelta = Vector2.zero;

        GameObject costObj = new GameObject("Cost");
        costObj.transform.SetParent(btnObj.transform, false);
        Text costText = costObj.AddComponent<Text>();
        costText.font = pressStart;
        costText.text = $"{cost}G";
        costText.fontSize = 12;
        costText.alignment = TextAnchor.MiddleCenter;
        costText.color = new Color(1f, 0.84f, 0f);
        RectTransform costRt = costObj.GetComponent<RectTransform>();
        costRt.anchorMin = new Vector2(0f, 0f);
        costRt.anchorMax = new Vector2(1f, 0.3f);
        costRt.sizeDelta = Vector2.zero;
    }

    void OnModeSelected(bool isRanked, bool hasPowerups, int cost)
    {
        EconomyManager econ = EconomyManager.Instance;
        int gold = econ != null ? econ.TotalGold : 0;

        if (gold < cost)
        {
            ShowMessage("NOT ENOUGH GOLD!");
            return;
        }

        if (isRanked && !RankedManager.IsUnlocked())
        {
            ShowMessage("COMPLETE CAMPAIGN FIRST!");
            return;
        }

        if (econ != null)
            econ.SpendGold(cost);

        PowerupMode powerupMode = hasPowerups ? PowerupMode.WithPowerups : PowerupMode.WithoutPowerups;

        if (isRanked)
            GameConfig.PlayRanked(powerupMode);
        else
        {
            int nextLevel = CampaignManager.Instance != null ? CampaignManager.Instance.GetNextUncompletedLevel() : 1;
            if (nextLevel <= 0) nextLevel = 1;
            GameConfig.PlayCampaign(nextLevel, powerupMode);
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
        Text goldText = goldObj.AddComponent<Text>();
        goldText.font = pressStart;
        goldText.text = $"GOLD: {gold}";
        goldText.fontSize = 12;
        goldText.alignment = TextAnchor.MiddleCenter;
        goldText.color = new Color(1f, 0.84f, 0f);
        RectTransform gRt = goldObj.GetComponent<RectTransform>();
        gRt.anchorMin = new Vector2(0f, 0f);
        gRt.anchorMax = new Vector2(1f, 0.1f);
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
}