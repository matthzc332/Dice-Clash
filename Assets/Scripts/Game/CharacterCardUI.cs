using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CharacterCardUI : MonoBehaviour
{
    private static CharacterCardUI _instance;
    public static CharacterCardUI Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("CharacterCardUI");
                _instance = obj.AddComponent<CharacterCardUI>();
            }
            return _instance;
        }
    }

    private GameObject ownCard, enemyCard;
    private Image ownCreatureImage, enemyCreatureImage;
    private Text ownNameText, ownScoreText;
    private Text enemyNameText, enemyScoreText;
    private Text ownAtkPrefix, ownAtkSuffix, ownDefPrefix, ownDefSuffix;
    private Text enemyAtkPrefix, enemyAtkSuffix, enemyDefPrefix, enemyDefSuffix;
    private Image ownAtkDice, ownDefDice, enemyAtkDice, enemyDefDice;
    private Text ownRangeText, enemyRangeText;
    private List<Image> ownGridCells = new();
    private List<Image> enemyGridCells = new();
    private Sprite swordSprite, shieldSprite, starSprite, diceSprite;
    private Sprite panelBlueSprite, panelRedSprite;

    private const float CARD_W = 370;
    private const float CARD_H = 400;
    private Font labelFont;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        CreateUI();
    }

    void CreateUI()
    {
        GameObject canvasObj = new GameObject("CardCanvas");
        canvasObj.transform.SetParent(transform);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 95;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        labelFont = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        if (labelFont == null)
            labelFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        swordSprite = LoadBestSprite("Sprites/Common/Decor/Espada");
        if (swordSprite == null) swordSprite = LoadBestSprite("Sprites/Human/Decor/Espada");
        if (swordSprite == null) swordSprite = LoadBestSprite("Sprites/Decor/Espada");
        if (swordSprite == null) swordSprite = CreateIconSprite(36, new Color(0.85f, 0.55f, 0.15f));
        shieldSprite = LoadBestSprite("Sprites/Common/Decor/Escudo");
        if (shieldSprite == null) shieldSprite = LoadBestSprite("Sprites/Human/Decor/Escudo");
        if (shieldSprite == null) shieldSprite = LoadBestSprite("Sprites/Decor/Escudo");
        if (shieldSprite == null) shieldSprite = CreateIconSprite(36, new Color(0.15f, 0.45f, 0.85f));
        starSprite = CreateIconSprite(16, new Color(1f, 0.8f, 0.1f));

        diceSprite = LoadFirstSprite("Sprites/Dice/Dado6");
        if (diceSprite == null) diceSprite = CreateIconSprite(16, Color.white);

        panelBlueSprite = LoadFirstSprite("Sprites/Menu/panelCartaBlue");
        panelRedSprite = LoadFirstSprite("Sprites/Menu/panelCartaRed");

        ownCard = CreateCard(canvasObj, "OwnCard", new Vector2(20, -79), panelBlueSprite,
            out ownCreatureImage, out ownNameText, out ownScoreText, out ownAtkPrefix, out ownAtkDice, out ownAtkSuffix, out ownDefPrefix, out ownDefDice, out ownDefSuffix, out ownRangeText, out ownGridCells, 115, -61, 176, 200);
        AdjustOwnCard();
        enemyCard = CreateCard(canvasObj, "EnemyCard", new Vector2(20, -450), panelRedSprite,
            out enemyCreatureImage, out enemyNameText, out enemyScoreText, out enemyAtkPrefix, out enemyAtkDice, out enemyAtkSuffix, out enemyDefPrefix, out enemyDefDice, out enemyDefSuffix, out enemyRangeText, out enemyGridCells, 115, -61, 176, 200);
        AdjustEnemyCard();

        gameObject.SetActive(false);
    }

    void AdjustOwnCard()
    {
        AdjustCard(ownCard, new Vector2(152, -49), new Vector2(297, -49), new Vector2(239, -40), new Vector2(28, -259), new Vector2(154, -256));
    }

    void AdjustEnemyCard()
    {
        AdjustCard(enemyCard, new Vector2(143, -48), new Vector2(292, -50), new Vector2(233, -40), new Vector2(26, -257), new Vector2(154, -254));
    }

    void AdjustCard(GameObject card, Vector2 namePos, Vector2 scorePos, Vector2 scoreIconPos, Vector2 atkBlockPos, Vector2 defBlockPos)
    {
        card.transform.Find("NameText").GetComponent<Text>().rectTransform.anchoredPosition = namePos;
        card.transform.Find("ScoreText").GetComponent<Text>().rectTransform.anchoredPosition = scorePos;
        card.transform.Find("ScoreIcon").GetComponent<RectTransform>().anchoredPosition = scoreIconPos;
        card.transform.Find("AtkBlock").GetComponent<RectTransform>().anchoredPosition = atkBlockPos;
        card.transform.Find("DefBlock").GetComponent<RectTransform>().anchoredPosition = defBlockPos;
    }

    GameObject CreateCard(GameObject parent, string name, Vector2 anchoredPos, Sprite panelSprite,
        out Image creatureImage, out Text nameText, out Text scoreText,
        out Text atkPrefix, out Image atkDice, out Text atkSuffix,
        out Text defPrefix, out Image defDice, out Text defSuffix,
        out Text rangeText, out List<Image> gridCells,
        float spriteCX, float spriteTop, float spriteW, float spriteH)
    {
        GameObject card = new GameObject(name);
        card.transform.SetParent(parent.transform);

        RectTransform cardRt = card.AddComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0f, 1f);
        cardRt.anchorMax = new Vector2(0f, 1f);
        cardRt.pivot = new Vector2(0f, 1f);
        cardRt.sizeDelta = new Vector2(CARD_W, CARD_H);
        cardRt.anchoredPosition = anchoredPos;

        Image cardBg = card.AddComponent<Image>();
        if (panelSprite != null)
        {
            cardBg.sprite = panelSprite;
            cardBg.preserveAspect = true;
        }
        else
        {
            cardBg.color = new Color(0.78f, 0.72f, 0.62f, 0.85f);
        }

        nameText = MakeLabel(card, "NameText", "", 18, Color.black, TextAnchor.MiddleLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(104, -18), new Vector2(200, 28));

        GameObject scoreObj = new GameObject("ScoreIcon");
        scoreObj.transform.SetParent(card.transform);
        Image scoreIcon = scoreObj.AddComponent<Image>();
        scoreIcon.sprite = starSprite;
        scoreIcon.preserveAspect = true;
        RectTransform scoreIconRt = scoreObj.GetComponent<RectTransform>();
        scoreIconRt.anchorMin = new Vector2(0f, 1f);
        scoreIconRt.anchorMax = new Vector2(0f, 1f);
        scoreIconRt.pivot = new Vector2(0f, 1f);
        scoreIconRt.sizeDelta = new Vector2(18, 18);
        scoreIconRt.anchoredPosition = new Vector2(220, -18);

        scoreText = MakeLabel(card, "ScoreText", "0", 13, Color.black, TextAnchor.MiddleLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(265, -18), new Vector2(50, 22));

        creatureImage = MakeSpriteArea(card, "CreatureImage", spriteCX, spriteTop, spriteW, spriteH);

        float gCell = 26f;
        float gGap = 2f;
        float gStep = gCell + gGap;
        float gTotal = 3 * gCell + 2 * gGap;
        float gridCX = 247f;
        float gridLeftX = gridCX - gTotal / 2f;
        float gridOffsetY = (gTotal - spriteH) / 2f;
        float gridTopY = spriteTop + gridOffsetY;

        gridCells = new List<Image>();
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                float cx = gridLeftX + c * gStep + gCell / 2f;
                float cy = gridTopY - r * gStep - gCell / 2f;

                GameObject cellObj = new GameObject($"GridCell_{r}_{c}");
                cellObj.transform.SetParent(card.transform);
                Image cellImg = cellObj.AddComponent<Image>();
                cellImg.color = new Color(0.25f, 0.25f, 0.25f, 0.1f);
                RectTransform cellRt = cellObj.GetComponent<RectTransform>();
                cellRt.anchorMin = new Vector2(0f, 1f);
                cellRt.anchorMax = new Vector2(0f, 1f);
                cellRt.pivot = new Vector2(0.5f, 0.5f);
                cellRt.sizeDelta = new Vector2(gCell, gCell);
                cellRt.anchoredPosition = new Vector2(cx, cy);

                GameObject borderObj = new GameObject($"Border_{r}_{c}");
                borderObj.transform.SetParent(cellObj.transform);
                Image borderImg = borderObj.AddComponent<Image>();
                borderImg.color = new Color(0.25f, 0.25f, 0.25f, 0.55f);
                RectTransform borderRt = borderObj.GetComponent<RectTransform>();
                borderRt.anchorMin = new Vector2(0f, 0f);
                borderRt.anchorMax = new Vector2(1f, 1f);
                borderRt.pivot = new Vector2(0.5f, 0.5f);
                borderRt.sizeDelta = new Vector2(gCell + 1, gCell + 1);
                borderRt.anchoredPosition = new Vector2(0, 0);

                gridCells.Add(cellImg);
            }
        }

        float rangeY = gridTopY - gTotal - 4;
        rangeText = MakeLabel(card, "RangeText", "Range: 3", 8, new Color(0.35f, 0.35f, 0.35f), TextAnchor.MiddleCenter,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(gridCX, rangeY), new Vector2(80, 14));

        float botY = -CARD_H + 54;
        float botH = 54;
        float blockW = (CARD_W - 24) / 2f;

        GameObject atkBlock = new GameObject("AtkBlock");
        atkBlock.transform.SetParent(card.transform);
        RectTransform atkBlockRt = atkBlock.AddComponent<RectTransform>();
        atkBlockRt.anchorMin = new Vector2(0f, 1f);
        atkBlockRt.anchorMax = new Vector2(0f, 1f);
        atkBlockRt.pivot = new Vector2(0f, 1f);
        atkBlockRt.sizeDelta = new Vector2(blockW, botH);
        atkBlockRt.anchoredPosition = new Vector2(10, botY);
        Image atkBlockBg = atkBlock.AddComponent<Image>();
        atkBlockBg.color = new Color(0.85f, 0.78f, 0.68f, 0.3f);

        MakeIcon(atkBlock, "AtkIcon", new Vector2(8, 0), swordSprite, new Vector2(36, 36));
        atkPrefix = MakeLabel(atkBlock, "AtkPrefix", "2", 14, Color.black, TextAnchor.MiddleLeft,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(50, 0), new Vector2(16, 18));
        atkDice = MakeIcon(atkBlock, "AtkDice", new Vector2(70, 0), diceSprite, new Vector2(22, 22));
        atkSuffix = MakeLabel(atkBlock, "AtkSuffix", "6+1", 12, Color.black, TextAnchor.MiddleLeft,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(98, 0), new Vector2(60, 18));

        GameObject defBlock = new GameObject("DefBlock");
        defBlock.transform.SetParent(card.transform);
        RectTransform defBlockRt = defBlock.AddComponent<RectTransform>();
        defBlockRt.anchorMin = new Vector2(0f, 1f);
        defBlockRt.anchorMax = new Vector2(0f, 1f);
        defBlockRt.pivot = new Vector2(0f, 1f);
        defBlockRt.sizeDelta = new Vector2(blockW, botH);
        defBlockRt.anchoredPosition = new Vector2(10 + blockW + 4, botY);
        Image defBlockBg = defBlock.AddComponent<Image>();
        defBlockBg.color = new Color(0.85f, 0.78f, 0.68f, 0.3f);

        MakeIcon(defBlock, "DefIcon", new Vector2(8, 0), shieldSprite, new Vector2(36, 36));
        defPrefix = MakeLabel(defBlock, "DefPrefix", "2", 14, Color.black, TextAnchor.MiddleLeft,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(50, 0), new Vector2(16, 18));
        defDice = MakeIcon(defBlock, "DefDice", new Vector2(70, 0), diceSprite, new Vector2(22, 22));
        defSuffix = MakeLabel(defBlock, "DefSuffix", "6+0", 12, Color.black, TextAnchor.MiddleLeft,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(98, 0), new Vector2(60, 18));

        return card;
    }

    Image MakeSpriteArea(GameObject parent, string name, float cx, float top, float w, float h)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform);
        Image img = obj.AddComponent<Image>();
        img.preserveAspect = true;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(cx, top - h / 2f);

        return img;
    }

    Image MakeIcon(GameObject parent, string name, Vector2 pos, Sprite sprite, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform);
        Image img = obj.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        return img;
    }

    Text MakeLabel(GameObject parent, string name, string text, int size, Color color, TextAnchor anchor, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 sizeDelta)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform);

        Text label = obj.AddComponent<Text>();
        label.font = labelFont;
        label.fontSize = size;
        label.alignment = anchor;
        label.color = color;
        label.text = text;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = pos;

        return label;
    }

    Sprite LoadFirstSprite(string path)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);
        return sprites.Length > 0 ? sprites[0] : null;
    }

    Sprite LoadBestSprite(string path)
    {
        Sprite[] all = Resources.LoadAll<Sprite>(path);
        if (all.Length == 0) return null;
        Sprite best = null;
        float bestArea = 0;
        foreach (Sprite s in all)
        {
            float area = s.rect.width * s.rect.height;
            if (area > bestArea)
            {
                bestArea = area;
                best = s;
            }
        }
        return best;
    }

    Sprite CreateIconSprite(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
        {
            int x = i % size;
            int y = i / size;
            float dx = (x + 0.5f) / size - 0.5f;
            float dy = (y + 0.5f) / size - 0.5f;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            pixels[i] = dist < 0.38f ? color : Color.Lerp(color, Color.clear, (dist - 0.38f) * 8f);
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
    }

    public void Show(PieceData data)
    {
        gameObject.SetActive(true);
        FillCard(ownCreatureImage, ownNameText, ownScoreText, ownAtkPrefix, ownAtkDice, ownAtkSuffix, ownDefPrefix, ownDefDice, ownDefSuffix, ownRangeText, ownGridCells, data);
        enemyCard.SetActive(false);
    }

    public void ShowWithTarget(PieceData ownPiece, PieceData enemyPiece)
    {
        gameObject.SetActive(true);
        FillCard(ownCreatureImage, ownNameText, ownScoreText, ownAtkPrefix, ownAtkDice, ownAtkSuffix, ownDefPrefix, ownDefDice, ownDefSuffix, ownRangeText, ownGridCells, ownPiece);
        FillCard(enemyCreatureImage, enemyNameText, enemyScoreText, enemyAtkPrefix, enemyAtkDice, enemyAtkSuffix, enemyDefPrefix, enemyDefDice, enemyDefSuffix, enemyRangeText, enemyGridCells, enemyPiece);
        enemyCard.SetActive(true);
    }

    void FillCard(Image creatureImage, Text nameText, Text scoreText,
        Text atkPrefix, Image atkDice, Text atkSuffix,
        Text defPrefix, Image defDice, Text defSuffix,
        Text rangeText, List<Image> gridCells, PieceData data)
    {
        string typeName = GetTypeName(data.type);
        string spriteName = GetSpriteName(data.type);

        nameText.text = typeName;
        nameText.color = Color.black;
        scoreText.text = data.PointValue.ToString();

        atkPrefix.text = "2";
        atkDice.sprite = diceSprite;
        atkSuffix.text = "6+1";
        defPrefix.text = "2";
        defDice.sprite = diceSprite;
        defSuffix.text = $"6+{data.defBonus}";

        int range = data.type == PieceType.Pawn ? 1 : 3;
        rangeText.text = $"Range: {range}";

        string cardName = spriteName + "Carta";
        Sprite[] loaded = Resources.LoadAll<Sprite>($"Sprites/Card/{cardName}");
        if (loaded.Length > 0)
        {
            creatureImage.sprite = loaded[0];
            creatureImage.preserveAspect = true;
            creatureImage.color = Color.white;
        }
        else
        {
            creatureImage.sprite = null;
            creatureImage.color = new Color(0.7f, 0.7f, 0.7f);
        }

        UpdateGrid(gridCells, data.type);
    }

    void UpdateGrid(List<Image> cells, PieceType type)
    {
        bool[] lit = GetGridPattern(type);
        Color active = new Color(0.15f, 0.75f, 0.15f, 0.75f);
        Color center = new Color(0.65f, 0.50f, 0.05f, 0.65f);
        Color dim = new Color(0.15f, 0.15f, 0.15f, 0.12f);

        for (int i = 0; i < cells.Count && i < 9; i++)
        {
            if (i == 4)
                cells[i].color = center;
            else if (lit[i])
                cells[i].color = active;
            else
                cells[i].color = dim;
        }
    }

    bool[] GetGridPattern(PieceType type)
    {
        bool[] cells = new bool[9];
        for (int i = 0; i < 9; i++) cells[i] = false;

        switch (type)
        {
            case PieceType.Pawn:
                cells[0] = true; cells[1] = true; cells[2] = true;
                cells[3] = true;                  cells[5] = true;
                cells[6] = true; cells[7] = true; cells[8] = true;
                break;
            case PieceType.Ninja:
                cells[0] = true;                  cells[2] = true;
                                                          ;
                cells[6] = true;                  cells[8] = true;
                break;
            case PieceType.Knight:
                                 cells[1] = true;
                cells[3] = true;                  cells[5] = true;
                                 cells[7] = true;
                break;
            case PieceType.Paladin:
                cells[0] = true; cells[1] = true; cells[2] = true;
                cells[3] = true;                  cells[5] = true;
                cells[6] = true; cells[7] = true; cells[8] = true;
                break;
        }
        return cells;
    }

    string GetTypeName(PieceType type)
    {
        return type switch
        {
            PieceType.Pawn => "Pawn",
            PieceType.Ninja => "Ninja",
            PieceType.Knight => "Knight",
            PieceType.Paladin => "Paladin",
            _ => "Pawn"
        };
    }

    string GetSpriteName(PieceType type)
    {
        return type switch
        {
            PieceType.Pawn => "Peon",
            PieceType.Ninja => "Ninja",
            PieceType.Knight => "Caballero",
            PieceType.Paladin => "Paladin",
            _ => "Peon"
        };
    }

    public void ClearUI()
    {
        gameObject.SetActive(false);
    }
}
