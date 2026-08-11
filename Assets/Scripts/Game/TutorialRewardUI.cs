using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialRewardUI : MonoBehaviour
{
    private const string KEY_CLAIMED = "TutorialRewardClaimed";
    private const int GOLD_AMOUNT = 150;

    public static bool HasClaimed()
    {
        return PlayerPrefs.GetInt(KEY_CLAIMED, 0) == 1;
    }

    public static void MarkClaimed()
    {
        PlayerPrefs.SetInt(KEY_CLAIMED, 1);
        PlayerPrefs.Save();
    }

    private Font font;
    private Sprite circleSprite;
    private Sprite[] chestSprites;
    private Canvas canvas;

    public void Show()
    {
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        circleSprite = CreateCircleSprite();
        LoadChestSprites();

        GameObject canvasObj = new GameObject("TutorialRewardCanvas");
        canvasObj.transform.SetParent(transform, false);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 210;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject overlayObj = new GameObject("Overlay");
        overlayObj.transform.SetParent(canvasObj.transform, false);
        RectTransform overlayRt = overlayObj.AddComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero;
        overlayRt.anchorMax = Vector2.one;
        overlayRt.sizeDelta = Vector2.zero;
        Image overlayImg = overlayObj.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0.7f);

        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(canvasObj.transform, false);
        Image contentBg = contentObj.AddComponent<Image>();
        contentBg.color = new Color(0.12f, 0.08f, 0.04f, 0.95f);
        RectTransform contentRt = contentObj.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0.5f, 0.5f);
        contentRt.anchorMax = new Vector2(0.5f, 0.5f);
        contentRt.sizeDelta = new Vector2(720, 660);

        yield return FadeIn(overlayImg, 0.2f);

        Text title = CreateText(contentObj.transform, "TREASURE!", 26, new Color(0.95f, 0.8f, 0.2f), new Vector2(0, 278));
        yield return PopIn(title.transform, 0.3f);
        yield return new WaitForSeconds(0.2f);

        GameObject glowObj = new GameObject("ChestGlow");
        glowObj.transform.SetParent(contentObj.transform, false);
        Image glowImg = glowObj.AddComponent<Image>();
        glowImg.sprite = circleSprite;
        glowImg.color = new Color(1f, 0.85f, 0.2f, 0.35f);
        glowImg.raycastTarget = false;
        RectTransform glowRt = glowObj.GetComponent<RectTransform>();
        glowRt.anchorMin = new Vector2(0.5f, 0.5f);
        glowRt.anchorMax = new Vector2(0.5f, 0.5f);
        glowRt.sizeDelta = new Vector2(440, 440);
        glowRt.anchoredPosition = new Vector2(0, 100);
        StartCoroutine(PulseGlow(glowRt, glowImg));

        GameObject chestObj = new GameObject("Chest");
        chestObj.transform.SetParent(contentObj.transform, false);
        Image chestImg = chestObj.AddComponent<Image>();
        chestImg.preserveAspect = true;
        chestImg.raycastTarget = false;
        chestImg.sprite = GetChestSprite(0);
        RectTransform chestRt = chestObj.GetComponent<RectTransform>();
        chestRt.anchorMin = new Vector2(0.5f, 0.5f);
        chestRt.anchorMax = new Vector2(0.5f, 0.5f);
        chestRt.sizeDelta = new Vector2(360, 324);
        chestRt.anchoredPosition = new Vector2(0, 100);
        chestRt.localScale = Vector3.zero;
        yield return PopIn(chestRt.transform, 0.35f);
        yield return new WaitForSeconds(0.15f);

        Vector2 chestBasePos = chestRt.anchoredPosition;
        for (int f = 0; f <= 4; f++)
        {
            chestImg.sprite = GetChestSprite(f);
            if (f < 4)
            {
                float amp = 3.5f + f * 2f;
                chestRt.anchoredPosition = chestBasePos + new Vector2(Random.Range(-amp, amp), Random.Range(-amp, amp));
            }
            else
            {
                chestRt.anchoredPosition = chestBasePos;
                SoundManager.Instance.PlayCoin();
                StartCoroutine(FlashBurst(contentObj.transform, chestBasePos));
                SpawnSparkles(contentObj.transform, chestBasePos + new Vector2(0, 60), 26);
                StartCoroutine(Hop(chestRt));
            }
            yield return new WaitForSeconds(0.14f);
        }
        yield return new WaitForSeconds(0.2f);

        if (EconomyManager.Instance != null)
            EconomyManager.Instance.AddGold(GOLD_AMOUNT);

        Text goldText = CreateText(contentObj.transform, $"+{GOLD_AMOUNT} GOLD!", 30, new Color(1f, 0.84f, 0f), new Vector2(0, -25));
        goldText.gameObject.AddComponent<Outline>().effectColor = new Color(0.5f, 0.3f, 0f);
        yield return BounceIn(goldText.transform, 0.5f);
        SpawnFallingCoins(contentObj.transform, new Vector2(0, -15), 18);
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(ShowCollectible(contentObj.transform, new Vector2(-200, -135), TutorialCollectibles.GetCupSprite(), new Color(1f, 0.84f, 0f), new Vector2(104, 104), "TUTORIAL CUP"));
        yield return StartCoroutine(ShowCollectible(contentObj.transform, new Vector2(0, -135), TutorialCollectibles.GetRibbonSprite(), new Color(0.62f, 0.42f, 0.24f), new Vector2(210, 60), "TUTORIAL RIBBON"));
        yield return StartCoroutine(ShowCollectible(contentObj.transform, new Vector2(200, -135), TutorialCollectibles.GetInsigniaSprite(), Color.white, new Vector2(96, 96), "TUTORIAL BADGE"));
        yield return new WaitForSeconds(0.2f);

        bool clicked = false;
        GameObject btnObj = new GameObject("ClaimBtn");
        btnObj.transform.SetParent(contentObj.transform, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.55f, 0.4f, 0.15f, 0.95f);
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0.5f);
        btnRt.anchorMax = new Vector2(0.5f, 0.5f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.sizeDelta = new Vector2(260, 56);
        btnRt.anchoredPosition = new Vector2(0, -290);

        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.font = font;
        btnText.text = "OK!";
        btnText.fontSize = 14;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        RectTransform btnTextRt = btnTextObj.GetComponent<RectTransform>();
        btnTextRt.anchorMin = Vector2.zero;
        btnTextRt.anchorMax = Vector2.one;
        btnTextRt.sizeDelta = Vector2.zero;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlayButton();
            clicked = true;
        });
        StartCoroutine(PulseButton(btnRt, btnImg));

        yield return new WaitUntil(() => clicked);

        MarkClaimed();
        Destroy(gameObject);
    }

    IEnumerator ShowCollectible(Transform parent, Vector2 pos, Sprite sprite, Color tint, Vector2 size, string label)
    {
        GameObject obj = new GameObject(label);
        obj.transform.SetParent(parent, false);
        Image img = obj.AddComponent<Image>();
        if (sprite != null)
        {
            img.sprite = sprite;
            img.preserveAspect = true;
        }
        img.color = tint;
        img.raycastTarget = false;
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        rt.localScale = Vector3.zero;
        yield return PopIn(rt.transform, 0.25f);

        Text labelText = CreateText(parent, label, 9, new Color(0.8f, 0.65f, 0.45f), pos + new Vector2(0, -size.y * 0.5f - 18));
        yield return PopIn(labelText.transform, 0.25f);
    }

    void LoadChestSprites()
    {
        Sprite[] all = Resources.LoadAll<Sprite>("Sprites/Cofre");
        chestSprites = new Sprite[5];
        for (int i = 0; i < 5; i++)
        {
            if (all != null)
            {
                foreach (var s in all)
                {
                    if (s.name == $"cofre{i}" || s.name == $"cofre{i}_0")
                    {
                        chestSprites[i] = s;
                        break;
                    }
                }
            }
            if (chestSprites[i] == null)
            {
                Texture2D tex = new Texture2D(64, 64);
                Color brown = new Color(0.55f, 0.38f, 0.18f);
                Color gold = new Color(1f, 0.8f, 0.2f);
                for (int y = 0; y < 64; y++)
                    for (int x = 0; x < 64; x++)
                    {
                        bool lid = y > 36;
                        bool body = y <= 36 && y > 12;
                        bool lockPx = x > 28 && x < 36 && y > 20 && y < 32;
                        bool band = body && (y > 20 && y < 24);
                        tex.SetPixel(x, y, lockPx || band ? gold : (lid || body ? brown : Color.clear));
                    }
                tex.Apply();
                chestSprites[i] = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
            }
        }
    }

    Sprite GetChestSprite(int frame)
    {
        int idx = Mathf.Clamp(frame, 0, 4);
        return chestSprites != null && chestSprites[idx] != null ? chestSprites[idx] : null;
    }

    Sprite CreateCircleSprite()
    {
        Texture2D tex = new Texture2D(16, 16);
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
            {
                float dx = (x + 0.5f) / 16f - 0.5f;
                float dy = (y + 0.5f) / 16f - 0.5f;
                tex.SetPixel(x, y, (dx * dx + dy * dy < 0.25f) ? Color.white : Color.clear);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
    }

    Text CreateText(Transform parent, string text, int size, Color color, Vector2 pos)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);
        Text t = obj.AddComponent<Text>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(400, 40);
        rt.anchoredPosition = pos;
        return t;
    }

    IEnumerator FadeIn(Graphic g, float dur)
    {
        float t = 0;
        Color c = g.color;
        while (t < dur)
        {
            t += Time.deltaTime;
            g.color = new Color(c.r, c.g, c.b, Mathf.Lerp(0f, c.a, t / dur));
            yield return null;
        }
        g.color = c;
    }

    IEnumerator PopIn(Transform target, float dur)
    {
        float t = 0;
        while (t < dur)
        {
            t += Time.deltaTime;
            float s = Mathf.Sin(t / dur * Mathf.PI * 0.5f);
            target.localScale = new Vector3(s, s, 1);
            yield return null;
        }
        target.localScale = Vector3.one;
    }

    IEnumerator BounceIn(Transform target, float dur)
    {
        Vector3 orig = target.localScale;
        float t = 0;
        while (t < dur)
        {
            t += Time.deltaTime;
            float s = 1f + Mathf.Sin(t / dur * Mathf.PI * 2f) * 0.25f * (1f - t / dur);
            target.localScale = new Vector3(s, s, 1);
            yield return null;
        }
        target.localScale = orig;
    }

    IEnumerator PulseGlow(RectTransform rt, Image img)
    {
        Vector2 baseSize = rt.sizeDelta;
        float t = 0;
        while (rt != null)
        {
            t += Time.deltaTime;
            float s = 1f + Mathf.Sin(t * 3f) * 0.08f;
            rt.sizeDelta = baseSize * s;
            Color c = img.color;
            img.color = new Color(c.r, c.g, c.b, 0.25f + Mathf.Sin(t * 2.5f) * 0.1f);
            yield return null;
        }
    }

    IEnumerator PulseButton(RectTransform rt, Image img)
    {
        Vector3 orig = rt.localScale;
        while (rt != null)
        {
            float pulse = 1f + Mathf.Sin(Time.time * 3.5f) * 0.04f;
            rt.localScale = orig * pulse;
            yield return null;
        }
    }

    IEnumerator FlashBurst(Transform parent, Vector2 pos)
    {
        GameObject flashObj = new GameObject("Flash");
        flashObj.transform.SetParent(parent, false);
        Image flashImg = flashObj.AddComponent<Image>();
        flashImg.sprite = circleSprite;
        flashImg.color = new Color(1f, 1f, 0.85f, 0.9f);
        flashImg.raycastTarget = false;
        RectTransform flashRt = flashObj.GetComponent<RectTransform>();
        flashRt.anchorMin = new Vector2(0.5f, 0.5f);
        flashRt.anchorMax = new Vector2(0.5f, 0.5f);
        flashRt.sizeDelta = new Vector2(80, 80);
        flashRt.anchoredPosition = pos;

        float t = 0;
        while (t < 0.35f)
        {
            t += Time.deltaTime;
            float p = t / 0.35f;
            flashRt.sizeDelta = Vector2.Lerp(new Vector2(80, 80), new Vector2(460, 460), p);
            flashImg.color = new Color(1f, 1f, 0.85f, Mathf.Lerp(0.9f, 0f, p));
            yield return null;
        }
        Destroy(flashObj);
    }

    IEnumerator Hop(RectTransform rt)
    {
        Vector2 basePos = rt.anchoredPosition;
        float t = 0;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            float p = t / 0.4f;
            rt.anchoredPosition = basePos + new Vector2(0, Mathf.Sin(p * Mathf.PI) * 34f);
            float s = 1f + Mathf.Sin(p * Mathf.PI) * 0.12f;
            rt.localScale = new Vector3(s, s, 1);
            yield return null;
        }
        rt.anchoredPosition = basePos;
        rt.localScale = Vector3.one;
    }

    void SpawnSparkles(Transform parent, Vector2 origin, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject spark = new GameObject("Spark");
            spark.transform.SetParent(parent, false);
            Image img = spark.AddComponent<Image>();
            img.sprite = circleSprite;
            img.color = new Color(1f, 0.84f, 0f);
            img.raycastTarget = false;
            RectTransform rt = spark.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(Random.Range(10f, 18f), Random.Range(10f, 18f));
            rt.anchoredPosition = origin;
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(70f, 170f);
            Vector2 target = origin + new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);
            StartCoroutine(AnimateSparkBurst(rt, target));
        }
    }

    void SpawnFallingCoins(Transform parent, Vector2 origin, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject coin = new GameObject("Coin");
            coin.transform.SetParent(parent, false);
            Image img = coin.AddComponent<Image>();
            img.sprite = circleSprite;
            img.color = new Color(1f, 0.84f, 0f, 0.9f);
            img.raycastTarget = false;
            RectTransform rt = coin.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(Random.Range(12f, 20f), Random.Range(12f, 20f));
            rt.anchoredPosition = origin + new Vector2(Random.Range(-60f, 60f), Random.Range(0f, 40f));
            Vector2 dir = new Vector2(Random.Range(-40f, 40f), Random.Range(-140f, -70f));
            StartCoroutine(AnimateFloatingParticle(rt, dir, Random.Range(0.6f, 1f)));
        }
    }

    IEnumerator AnimateSparkBurst(RectTransform rt, Vector2 target)
    {
        if (rt == null) yield break;
        Vector2 start = rt.anchoredPosition;
        float t = 0;
        float dur = Random.Range(0.25f, 0.45f);
        Color c = rt.GetComponent<Image>().color;
        while (t < dur)
        {
            t += Time.deltaTime;
            if (rt == null) yield break;
            rt.anchoredPosition = Vector2.Lerp(start, target, t / dur);
            rt.GetComponent<Image>().color = new Color(c.r, c.g, c.b, Mathf.Lerp(1f, 0f, t / dur));
            yield return null;
        }
        if (rt != null) Destroy(rt.gameObject);
    }

    IEnumerator AnimateFloatingParticle(RectTransform rt, Vector2 direction, float life)
    {
        if (rt == null) yield break;
        float t = 0;
        Vector2 start = rt.anchoredPosition;
        Color c = rt.GetComponent<Image>().color;
        while (t < life)
        {
            t += Time.deltaTime;
            if (rt == null) yield break;
            float p = t / life;
            rt.anchoredPosition = start + direction * p + new Vector2(0, Mathf.Sin(p * Mathf.PI * 3f) * 15f);
            rt.GetComponent<Image>().color = new Color(c.r, c.g, c.b, Mathf.Lerp(1f, 0f, p));
            yield return null;
        }
        if (rt != null) Destroy(rt.gameObject);
    }
}
