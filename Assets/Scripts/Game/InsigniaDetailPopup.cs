using UnityEngine;
using UnityEngine.UI;

public static class InsigniaDetailPopup
{
    public static void Show(Canvas canvas, Font font, Insignia ins, bool collected)
    {
        if (canvas == null || ins == null) return;

        GameObject overlay = new GameObject("InsigniaDetail");
        overlay.transform.SetParent(canvas.transform, false);

        RectTransform oRt = overlay.AddComponent<RectTransform>();
        oRt.anchorMin = Vector2.zero;
        oRt.anchorMax = Vector2.one;
        oRt.offsetMin = Vector2.zero;
        oRt.offsetMax = Vector2.zero;

        Image oBg = overlay.AddComponent<Image>();
        oBg.color = new Color(0, 0, 0, 0.65f);

        Button closeBtn = overlay.AddComponent<Button>();
        closeBtn.targetGraphic = oBg;
        closeBtn.onClick.AddListener(() => Object.Destroy(overlay));

        Color rarity = InsigniaSprites.GetRarityColor(ins.rarity);

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(overlay.transform, false);
        RectTransform pRt = panel.AddComponent<RectTransform>();
        pRt.anchorMin = new Vector2(0.5f, 0.5f);
        pRt.anchorMax = new Vector2(0.5f, 0.5f);
        pRt.pivot = new Vector2(0.5f, 0.5f);
        pRt.sizeDelta = new Vector2(360, 440);
        pRt.anchoredPosition = Vector2.zero;

        Image pBg = panel.AddComponent<Image>();
        pBg.color = new Color(rarity.r * 0.15f, rarity.g * 0.15f, rarity.b * 0.15f, 0.95f);
        pBg.raycastTarget = false;
        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = rarity;
        outline.effectDistance = new Vector2(3, -3);

        if (collected)
        {
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(panel.transform, false);
            Image icon = iconObj.AddComponent<Image>();
            icon.sprite = InsigniaSprites.Get(ins);
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            RectTransform iRt = iconObj.GetComponent<RectTransform>();
            iRt.anchorMin = new Vector2(0.5f, 0.5f);
            iRt.anchorMax = new Vector2(0.5f, 0.5f);
            iRt.pivot = new Vector2(0.5f, 0.5f);
            iRt.sizeDelta = new Vector2(180, 180);
            iRt.anchoredPosition = new Vector2(0, 40);
        }

        CreateText(panel.transform, font, collected ? ins.name.ToUpper() : "???", 16,
            collected ? rarity : new Color(0.5f, 0.5f, 0.5f), new Vector2(0, 185), new Vector2(330, 40));

        if (collected)
        {
            CreateText(panel.transform, font, ins.description, 10,
                new Color(0.75f, 0.7f, 0.6f), new Vector2(0, -140), new Vector2(300, 120));
        }

        CreateText(panel.transform, font, ins.rarity.ToUpper(), 12,
            collected ? rarity : new Color(0.4f, 0.4f, 0.4f), new Vector2(0, -190), new Vector2(300, 30));

        PopIn pop = overlay.AddComponent<PopIn>();
        pop.target = pRt;
    }

    static Text CreateText(Transform parent, Font font, string content, int fontSize, Color color, Vector2 pos, Vector2 size)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);
        Text text = obj.AddComponent<Text>();
        text.font = font;
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        return text;
    }

    class PopIn : MonoBehaviour
    {
        public RectTransform target;
        float t = 0f;

        void Start()
        {
            if (target != null) target.localScale = Vector3.zero;
        }

        void Update()
        {
            if (target == null)
            {
                Destroy(this);
                return;
            }
            t = Mathf.Min(1f, t + Time.unscaledDeltaTime * 6f);
            float s = 1f - Mathf.Pow(1f - t, 3f);
            target.localScale = new Vector3(s, s, 1f);
            if (t >= 1f) Destroy(this);
        }
    }
}
