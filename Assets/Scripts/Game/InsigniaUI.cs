using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InsigniaUI : MonoBehaviour
{
    private GameObject panelObj;
    private Canvas canvas;
    private Font font;
    private string currentFilter = "all";

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

    void BuildPanel()
    {
        panelObj = new GameObject("InsigniaPanel");
        panelObj.transform.SetParent(canvas.transform, false);

        Image bg = panelObj.AddComponent<Image>();
        bg.color = new Color(0.06f, 0.04f, 0.03f, 0.95f);
        RectTransform bgRt = panelObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;

        CreateTitle();
        CreateFilterButtons();
        CreateScrollGrid();
        CreateBackButton();
    }

    void CreateTitle()
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelObj.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.font = font;
        titleText.fontSize = 22;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.text = "COLLECTION";
        titleText.color = new Color(0.9f, 0.75f, 0.3f);
        RectTransform tRt = titleObj.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0f, 1f);
        tRt.anchorMax = new Vector2(1f, 1f);
        tRt.pivot = new Vector2(0.5f, 1f);
        tRt.sizeDelta = new Vector2(0, 50);
        tRt.anchoredPosition = new Vector2(0, -10);

        int collected = InsigniaManager.GetCollectedCount();
        int total = InsigniaManager.GetTotalCount();

        GameObject countObj = new GameObject("Count");
        countObj.transform.SetParent(panelObj.transform, false);
        Text countText = countObj.AddComponent<Text>();
        countText.font = font;
        countText.fontSize = 12;
        countText.alignment = TextAnchor.MiddleCenter;
        countText.text = $"{collected}/{total}";
        countText.color = new Color(0.8f, 0.7f, 0.4f);
        RectTransform cRt = countObj.GetComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0f, 1f);
        cRt.anchorMax = new Vector2(1f, 1f);
        cRt.pivot = new Vector2(0.5f, 1f);
        cRt.sizeDelta = new Vector2(0, 25);
        cRt.anchoredPosition = new Vector2(0, -55);
    }

    void CreateFilterButtons()
    {
        string[] filters = { "all", "campaign", "chest" };
        string[] labels = { "ALL", "CAMPAIGN", "CHESTS" };
        float startX = -280f;
        float spacing = 190f;

        for (int i = 0; i < filters.Length; i++)
        {
            string filter = filters[i];
            GameObject btnObj = new GameObject($"Filter_{filter}");
            btnObj.transform.SetParent(panelObj.transform, false);
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = currentFilter == filter
                ? new Color(0.5f, 0.4f, 0.2f, 0.9f)
                : new Color(0.2f, 0.15f, 0.1f, 0.7f);
            RectTransform btnRt = btnObj.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.5f, 1f);
            btnRt.anchorMax = new Vector2(0.5f, 1f);
            btnRt.pivot = new Vector2(0.5f, 1f);
            btnRt.sizeDelta = new Vector2(160, 35);
            btnRt.anchoredPosition = new Vector2(startX + i * spacing, -85);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            Text text = textObj.AddComponent<Text>();
            text.font = font;
            text.fontSize = 10;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = labels[i];
            text.color = currentFilter == filter ? new Color(1f, 0.85f, 0.3f) : new Color(0.6f, 0.55f, 0.45f);
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySelect();
                currentFilter = filter;
                RefreshGrid();
            });
        }
    }

    void CreateScrollGrid()
    {
        GameObject scrollObj = new GameObject("Scroll");
        scrollObj.transform.SetParent(panelObj.transform, false);
        RectTransform scrollRt = scrollObj.AddComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0f, 0f);
        scrollRt.anchorMax = new Vector2(1f, 1f);
        scrollRt.offsetMin = new Vector2(60, 70);
        scrollRt.offsetMax = new Vector2(-60, -125);

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

        PopulateGrid(crt);
    }

    void PopulateGrid(RectTransform content)
    {
        Insignia[] allInsignias;
        if (currentFilter == "all")
        {
            var data = InsigniaData.Load();
            allInsignias = data?.insignias ?? new Insignia[0];
        }
        else
        {
            allInsignias = InsigniaData.GetBySource(currentFilter);
        }

        if (allInsignias == null || allInsignias.Length == 0) return;

        float cardW = 155f;
        float cardH = 80f;
        float gapX = 10f;
        float gapY = 10f;
        int cols = 5;

        float totalWidth = cols * cardW + (cols - 1) * gapX;
        float startX = -totalWidth / 2f + cardW / 2f;

        for (int i = 0; i < allInsignias.Length; i++)
        {
            Insignia ins = allInsignias[i];
            bool collected = InsigniaManager.HasInsignia(ins.id);
            int row = i / cols;
            int col = i % cols;

            float x = startX + col * (cardW + gapX);
            float y = -row * (cardH + gapY) - 10f;

            CreateInsigniaCard(content, ins, collected, x, y, cardW, cardH);
        }

        int totalRows = Mathf.CeilToInt((float)allInsignias.Length / cols);
        float contentHeight = totalRows * (cardH + gapY) + 20f;
        content.sizeDelta = new Vector2(0, contentHeight);
    }

    void CreateInsigniaCard(Transform parent, Insignia ins, bool collected, float x, float y, float w, float h)
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
        cardBtn.onClick.AddListener(() => InsigniaDetailPopup.Show(canvas, font, ins, collected));

        if (collected)
        {
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(cardObj.transform, false);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = LoadInsigniaSprite(ins);
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            RectTransform iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.08f, 0.2f);
            iconRt.anchorMax = new Vector2(0.35f, 0.9f);
            iconRt.sizeDelta = Vector2.zero;
        }

        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(cardObj.transform, false);
        Text nameText = nameObj.AddComponent<Text>();
        nameText.font = font;
        nameText.fontSize = 8;
        nameText.alignment = collected ? TextAnchor.MiddleLeft : TextAnchor.MiddleCenter;
        nameText.text = collected ? ins.name.ToUpper() : "???";
        nameText.color = collected ? rarityColor : new Color(0.3f, 0.3f, 0.3f);
        RectTransform nRt = nameObj.GetComponent<RectTransform>();
        if (collected)
        {
            nRt.anchorMin = new Vector2(0.38f, 0.5f);
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
            descText.font = font;
            descText.fontSize = 6;
            descText.alignment = TextAnchor.MiddleLeft;
            descText.text = ins.description;
            descText.color = new Color(0.6f, 0.55f, 0.45f);
            RectTransform dRt = descObj.GetComponent<RectTransform>();
            dRt.anchorMin = new Vector2(0.38f, 0.05f);
            dRt.anchorMax = new Vector2(0.95f, 0.5f);
            dRt.sizeDelta = Vector2.zero;
        }

        GameObject rarityObj = new GameObject("Rarity");
        rarityObj.transform.SetParent(cardObj.transform, false);
        Text rarityText = rarityObj.AddComponent<Text>();
        rarityText.font = font;
        rarityText.fontSize = 6;
        rarityText.alignment = TextAnchor.MiddleCenter;
        rarityText.text = ins.rarity.ToUpper();
        rarityText.color = collected
            ? new Color(rarityColor.r * 0.7f, rarityColor.g * 0.7f, rarityColor.b * 0.7f)
            : new Color(0.25f, 0.25f, 0.25f);
        RectTransform rRt = rarityObj.GetComponent<RectTransform>();
        rRt.anchorMin = new Vector2(0f, 0f);
        rRt.anchorMax = new Vector2(1f, 0.2f);
        rRt.sizeDelta = Vector2.zero;

        if (collected)
        {
            GameObject checkObj = new GameObject("Check");
            checkObj.transform.SetParent(cardObj.transform, false);
            Text checkText = checkObj.AddComponent<Text>();
            checkText.font = font;
            checkText.fontSize = 10;
            checkText.alignment = TextAnchor.MiddleCenter;
            checkText.text = "\u2713";
            checkText.color = new Color(0.3f, 0.9f, 0.3f);
            RectTransform checkRt = checkObj.GetComponent<RectTransform>();
            checkRt.anchorMin = new Vector2(0.8f, 0.7f);
            checkRt.anchorMax = new Vector2(1f, 1f);
            checkRt.sizeDelta = Vector2.zero;
        }
    }

    Sprite LoadInsigniaSprite(Insignia ins)
    {
        return InsigniaSprites.Get(ins);
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

    void RefreshGrid()
    {
        Transform scrollTransform = panelObj.transform.Find("Scroll/Viewport/Content");
        if (scrollTransform != null)
        {
            foreach (Transform child in scrollTransform)
                Destroy(child.gameObject);

            RectTransform crt = scrollTransform.GetComponent<RectTransform>();
            PopulateGrid(crt);
        }

        Transform filtersParent = panelObj.transform;
        foreach (Transform child in filtersParent)
        {
            if (child.name.StartsWith("Filter_"))
            {
                string filterName = child.name.Replace("Filter_", "");
                Image img = child.GetComponent<Image>();
                if (img != null)
                {
                    img.color = currentFilter == filterName
                        ? new Color(0.5f, 0.4f, 0.2f, 0.9f)
                        : new Color(0.2f, 0.15f, 0.1f, 0.7f);
                }
                Text text = child.GetComponentInChildren<Text>();
                if (text != null)
                {
                    text.color = currentFilter == filterName
                        ? new Color(1f, 0.85f, 0.3f)
                        : new Color(0.6f, 0.55f, 0.45f);
                }
            }
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
