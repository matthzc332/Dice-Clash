using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBanner : MonoBehaviour
{
    public static EnemyBanner Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public void Show(string armyName, Color raceColor)
    {
        if (string.IsNullOrEmpty(armyName)) return;
        StartCoroutine(BannerSequence(armyName, raceColor));
    }

    IEnumerator BannerSequence(string name, Color color)
    {
        Font pressStart = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        if (pressStart == null) pressStart = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) yield break;

        GameObject bannerObj = new GameObject("EnemyBanner");
        bannerObj.transform.SetParent(canvas.transform, false);

        RectTransform bannerRt = bannerObj.AddComponent<RectTransform>();
        bannerRt.anchorMin = new Vector2(0.5f, 0.5f);
        bannerRt.anchorMax = new Vector2(0.5f, 0.5f);
        bannerRt.sizeDelta = new Vector2(600, 80);

        Image bg = bannerObj.AddComponent<Image>();
        bg.color = new Color(color.r * 0.3f, color.g * 0.3f, color.b * 0.3f, 0.9f);
        bg.raycastTarget = false;

        Outline border = bannerObj.AddComponent<Outline>();
        border.effectColor = new Color(color.r, color.g, color.b, 0.8f);
        border.effectDistance = new Vector2(4, -4);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(bannerObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.font = pressStart;
        text.text = name.ToUpper();
        text.fontSize = 14;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);

        CanvasGroup cg = bannerObj.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        float duration = 0.4f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }
        cg.alpha = 1f;

        yield return new WaitForSeconds(2f);

        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, t / duration);
            yield return null;
        }

        Destroy(bannerObj);
    }
}