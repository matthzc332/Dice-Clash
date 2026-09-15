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

        Sprite bannerSprite = null;
        Sprite[] allDecor = Resources.LoadAll<Sprite>("Sprites/Decor");
        if (allDecor != null)
            foreach (var s in allDecor)
                if (s.name.StartsWith("enemyBanner")) { bannerSprite = s; break; }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        GameObject rootObj;
        if (canvas != null)
        {
            rootObj = canvas.gameObject;
        }
        else
        {
            rootObj = new GameObject("EnemyBannerCanvas");
            Canvas c = rootObj.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 219;
            CanvasScaler cs = rootObj.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920, 1080);
            cs.matchWidthOrHeight = 0.5f;
        }

        GameObject bannerObj = new GameObject("EnemyBanner");
        bannerObj.transform.SetParent(rootObj.transform, false);

        RectTransform bannerRt = bannerObj.AddComponent<RectTransform>();
        bannerRt.anchorMin = new Vector2(0.5f, 0.5f);
        bannerRt.anchorMax = new Vector2(0.5f, 0.5f);
        bannerRt.sizeDelta = new Vector2(600, 120);

        Image bg = bannerObj.AddComponent<Image>();
        if (bannerSprite != null)
        {
            bg.sprite = bannerSprite;
            bg.color = Color.white;
            bg.type = Image.Type.Simple;
            bg.preserveAspect = true;
        }
        else
        {
            bg.color = new Color(color.r * 0.3f, color.g * 0.3f, color.b * 0.3f, 0.9f);
        }
        bg.raycastTarget = false;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(bannerObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.font = pressStart;
        text.text = name.ToUpper();
        text.fontSize = 16;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
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