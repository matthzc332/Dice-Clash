using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LightAuraBanner : MonoBehaviour
{
    public static LightAuraBanner Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public void Show(string message)
    {
        StartCoroutine(BannerSequence(message));
    }

    IEnumerator BannerSequence(string message)
    {
        Font font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject bannerObj = new GameObject("LightAuraBanner");
        Canvas canvas = bannerObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 220;
        CanvasScaler scaler = bannerObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        bannerObj.AddComponent<GraphicRaycaster>();

        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(bannerObj.transform, false);
        Image panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(1f, 0.85f, 0.3f, 0.92f);
        panelImg.raycastTarget = false;
        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(900, 110);

        Sprite panelSprite = Resources.Load<Sprite>("Sprites/Menu/panelVictoria");
        if (panelSprite != null)
        {
            panelImg.sprite = panelSprite;
            panelImg.preserveAspect = false;
        }

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(panelObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.font = font;
        text.text = message;
        text.fontSize = 40;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(1f, 0.98f, 0.85f, 1f);
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.4f, 0.25f, 0f, 0.9f);
        outline.effectDistance = new Vector2(3, -3);
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        float duration = 0.4f;
        float holdTime = 0.8f;
        float t = 0f;

        while (t < duration)
        {
            if (bannerObj == null) yield break;
            t += Time.unscaledDeltaTime;
            float eased = 1f - Mathf.Pow(1f - (t / duration), 3f);
            panelRt.anchoredPosition = new Vector2(Mathf.Lerp(-1920f, 0f, eased), 0f);
            yield return null;
        }
        panelRt.anchoredPosition = Vector2.zero;

        yield return new WaitForSecondsRealtime(holdTime);

        t = 0f;
        while (t < duration)
        {
            if (bannerObj == null) yield break;
            t += Time.unscaledDeltaTime;
            float eased = 1f - Mathf.Pow(1f - (t / duration), 3f);
            panelRt.anchoredPosition = new Vector2(Mathf.Lerp(0f, 1920f, eased), 0f);
            yield return null;
        }

        Destroy(bannerObj);
    }
}