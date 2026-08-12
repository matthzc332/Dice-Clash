using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

        try
        {
            CreateScrollContent();
        }
        catch (System.Exception e)
        {
            Debug.LogError("CampaignUI: error building campaign content: " + e);
        }

        try
        {
            CreateTitle();
        }
        catch (System.Exception e)
        {
            Debug.LogError("CampaignUI: error building campaign title: " + e);
        }

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
        scroll.content = CreateContent(viewport.transform);
    }

    RectTransform CreateContent(Transform parent)
    {
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(parent, false);
        RectTransform crt = contentObj.AddComponent<RectTransform>();
        crt.anchorMin = new Vector2(0f, 1f);
        crt.anchorMax = new Vector2(1f, 1f);
        crt.pivot = new Vector2(0.5f, 1f);
        crt.sizeDelta = new Vector2(0, 0);

        var data = CampaignData.Load();
        if (data == null || data.cups == null) return crt;

        float yPos = -20f;
        float[] cupColors = { 0.45f, 0.35f, 0.25f, 0.15f };
        int cupIdx = 0;

        foreach (var cup in data.cups)
        {
            if (cup == null) continue;

            GameObject cupObj = new GameObject($"Cup_{cup.id}");
            cupObj.transform.SetParent(contentObj.transform, false);
            RectTransform cupRt = cupObj.AddComponent<RectTransform>();
            cupRt.anchorMin = new Vector2(0f, 1f);
            cupRt.anchorMax = new Vector2(1f, 1f);
            cupRt.pivot = new Vector2(0.5f, 1f);
            cupRt.anchoredPosition = new Vector2(0, yPos);

            float col = cupColors[Mathf.Min(cupIdx, cupColors.Length - 1)];

            Image cupBg = cupObj.AddComponent<Image>();
            cupBg.color = new Color(col, col * 0.7f, col * 0.5f, 0.3f);

            GameObject cupTitle = new GameObject("CupTitle");
            cupTitle.transform.SetParent(cupObj.transform, false);
            Text cupText = cupTitle.AddComponent<Text>();
            cupText.font = font;
            cupText.fontSize = 16;
            cupText.alignment = TextAnchor.MiddleLeft;
            cupText.text = cup.name.ToUpper();
            cupText.color = new Color(0.9f, 0.75f, 0.3f);
            RectTransform ctRt = cupTitle.GetComponent<RectTransform>();
            ctRt.anchorMin = new Vector2(0f, 1f);
            ctRt.anchorMax = new Vector2(1f, 1f);
            ctRt.pivot = new Vector2(0f, 1f);
            ctRt.sizeDelta = new Vector2(0, 40);
            ctRt.anchoredPosition = new Vector2(20, 0);

            GameObject raceLabel = new GameObject("RaceLabel");
            raceLabel.transform.SetParent(cupObj.transform, false);
            Text raceText = raceLabel.AddComponent<Text>();
            raceText.font = font;
            raceText.fontSize = 10;
            raceText.alignment = TextAnchor.MiddleRight;
            raceText.text = cup.race.ToUpper();
            raceText.color = new Color(0.6f, 0.5f, 0.4f);
            RectTransform rlRt = raceLabel.GetComponent<RectTransform>();
            rlRt.anchorMin = new Vector2(0f, 1f);
            rlRt.anchorMax = new Vector2(1f, 1f);
            rlRt.pivot = new Vector2(1f, 1f);
            rlRt.sizeDelta = new Vector2(0, 40);
            rlRt.anchoredPosition = new Vector2(-20, 0);

            float levelY = -50f;
            if (cup.levels != null)
            {
                for (int i = 0; i < cup.levels.Length; i++)
                {
                    int levelId = cup.levels[i];
                    var level = CampaignData.GetLevel(levelId);
                    if (level == null) continue;

                    bool completed = CampaignManager.Instance != null && CampaignManager.Instance.IsLevelCompleted(levelId);
                    bool unlocked = CampaignManager.Instance != null && CampaignManager.Instance.IsLevelUnlocked(levelId);

                    CreateLevelButton(cupObj.transform, level, completed, unlocked, i, levelY);
                    levelY -= 70f;
                }
            }

            float cupHeight = 50f + (cup.levels?.Length ?? 0) * 70f + 20f;
            cupRt.sizeDelta = new Vector2(0, cupHeight);

            yPos -= cupHeight + 15f;
            cupIdx++;
        }

        crt.sizeDelta = new Vector2(0, -yPos + 20f);
        return crt;
    }

    void CreateLevelButton(Transform parent, CampaignLevel level, bool completed, bool unlocked, int index, float yOffset)
    {
        float[] rowX = { -320f, 0f, 320f };
        float xPos = rowX[index % rowX.Length];

        GameObject btnObj = new GameObject($"Level_{level.id}");
        btnObj.transform.SetParent(parent, false);
        RectTransform btnRt = btnObj.AddComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 1f);
        btnRt.anchorMax = new Vector2(0.5f, 1f);
        btnRt.pivot = new Vector2(0.5f, 1f);
        btnRt.sizeDelta = new Vector2(280, 60);
        btnRt.anchoredPosition = new Vector2(xPos, yOffset);

        Image btnBg = btnObj.AddComponent<Image>();
        if (completed)
            btnBg.color = new Color(0.2f, 0.5f, 0.2f, 0.8f);
        else if (unlocked)
            btnBg.color = new Color(0.3f, 0.35f, 0.5f, 0.85f);
        else
            btnBg.color = new Color(0.2f, 0.2f, 0.2f, 0.6f);

        GameObject numObj = new GameObject("Number");
        numObj.transform.SetParent(btnObj.transform, false);
        Text numText = numObj.AddComponent<Text>();
        numText.font = font;
        numText.fontSize = 22;
        numText.alignment = TextAnchor.MiddleCenter;
        numText.text = level.id.ToString();
        numText.color = completed ? new Color(0.4f, 1f, 0.4f) : unlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
        RectTransform numRt = numObj.GetComponent<RectTransform>();
        numRt.anchorMin = new Vector2(0f, 0f);
        numRt.anchorMax = new Vector2(0.3f, 1f);
        numRt.sizeDelta = Vector2.zero;
        numRt.offsetMin = new Vector2(5, 0);
        numRt.offsetMax = new Vector2(0, 0);

        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(btnObj.transform, false);
        Text nameText = nameObj.AddComponent<Text>();
        nameText.font = font;
        nameText.fontSize = 10;
        nameText.alignment = TextAnchor.MiddleLeft;
        nameText.text = level.name;
        nameText.color = completed ? new Color(0.7f, 1f, 0.7f) : unlocked ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.4f, 0.4f, 0.4f);
        RectTransform nameRt = nameObj.GetComponent<RectTransform>();
        nameRt.anchorMin = new Vector2(0.3f, 0f);
        nameRt.anchorMax = new Vector2(0.75f, 1f);
        nameRt.sizeDelta = Vector2.zero;
        nameRt.offsetMin = new Vector2(5, 0);
        nameRt.offsetMax = new Vector2(-5, 0);

        if (completed)
        {
            GameObject checkObj = new GameObject("Check");
            checkObj.transform.SetParent(btnObj.transform, false);
            Text checkText = checkObj.AddComponent<Text>();
            checkText.font = font;
            checkText.fontSize = 18;
            checkText.alignment = TextAnchor.MiddleCenter;
            checkText.text = "W";
            checkText.color = new Color(0.2f, 0.9f, 0.2f);
            RectTransform checkRt = checkObj.GetComponent<RectTransform>();
            checkRt.anchorMin = new Vector2(0.75f, 0f);
            checkRt.anchorMax = new Vector2(1f, 1f);
            checkRt.sizeDelta = Vector2.zero;
            checkRt.offsetMin = new Vector2(0, 0);
            checkRt.offsetMax = new Vector2(-10, 0);
        }

        if (unlocked)
        {
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnBg;
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
        RectTransform tRt = titleObj.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0f, 1f);
        tRt.anchorMax = new Vector2(1f, 1f);
        tRt.pivot = new Vector2(0.5f, 1f);
        tRt.sizeDelta = new Vector2(0, 45);
        tRt.anchoredPosition = new Vector2(0, -10);
    }
}
