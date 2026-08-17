using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CampaignRewardUI : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private GameObject canvasObj;
    private RectTransform panelRt;

    public void Show(int levelId, int goldReward, bool chestGranted, string cupCompletedRace = null)
    {
        StartCoroutine(Sequence(levelId, goldReward, chestGranted, cupCompletedRace));
    }

    IEnumerator Sequence(int levelId, int goldReward, bool chestGranted, string cupCompletedRace)
    {
        CampaignLevel level = CampaignData.GetLevel(levelId);
        if (level == null) { Destroy(gameObject); yield break; }

        Font font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        canvasObj = new GameObject("CampaignRewardCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 210;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject overlay = new GameObject("Overlay");
        overlay.transform.SetParent(canvasObj.transform, false);
        Image overlayBg = overlay.AddComponent<Image>();
        overlayBg.color = new Color(0, 0, 0, 0.75f);
        overlayBg.raycastTarget = true;
        RectTransform overlayRt = overlay.GetComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero;
        overlayRt.anchorMax = Vector2.one;
        overlayRt.sizeDelta = Vector2.zero;

        canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        Sprite panelSprite = LoadFirstSprite("Sprites/Menu/panelCartaBlue");

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(canvasObj.transform, false);
        Image panelBg = panel.AddComponent<Image>();
        if (panelSprite != null) { panelBg.sprite = panelSprite; panelBg.color = Color.white; }
        else panelBg.color = new Color(0.12f, 0.15f, 0.22f, 0.95f);
        panelBg.raycastTarget = true;
        panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(560, 580);

        Text titleText = MakeText(panel.transform, "REWARD!", font, 22, Color.white, new Vector2(0, 175));
        Text subtitleText = MakeText(panel.transform, level.name.ToUpper(), font, 14, new Color(0.8f, 0.8f, 0.5f), new Vector2(0, 135));

        yield return new WaitForSecondsRealtime(0.3f);

        float t = 0f;
        while (t < 0.25f) { t += Time.unscaledDeltaTime; canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.25f); yield return null; }
        canvasGroup.alpha = 1f;

        titleText.transform.localScale = Vector3.one * 2.5f;
        StartCoroutine(ScalePop(titleText.transform, 0.25f));
        SoundManager.Instance.PlayHammer();
        StartCoroutine(ShakePanel(0.08f, 0.3f));
        yield return new WaitForSecondsRealtime(0.35f);

        subtitleText.transform.localScale = Vector3.one * 2f;
        StartCoroutine(ScalePop(subtitleText.transform, 0.2f));
        SoundManager.Instance.PlaySelect();
        yield return new WaitForSecondsRealtime(0.3f);

        float y = 80f;

        if (goldReward > 0)
        {
            GameObject goldRow = CreateGoldRowHidden(panel.transform, font, goldReward, y);
            yield return new WaitForSecondsRealtime(0.15f);
            yield return RevealRow(goldRow, font);
            SoundManager.Instance.PlayCoin();
            StartCoroutine(AnimateCoinBounce(goldRow));
            StartCoroutine(SpawnCoinParticles(panel.transform, goldRow.GetComponent<RectTransform>().anchoredPosition));
            StartCoroutine(ShakePanel(0.06f, 0.2f));
            y -= 65f;
            yield return new WaitForSecondsRealtime(0.4f);
        }

        string insigniaId = $"camp_{levelId:D2}";
        Insignia insignia = InsigniaData.GetInsignia(insigniaId);
        if (insignia != null)
        {
            GameObject insigniaRow = CreateInsigniaRowHidden(panel.transform, font, insignia, y);
            yield return new WaitForSecondsRealtime(0.15f);
            yield return RevealRow(insigniaRow, font);
            SoundManager.Instance.PlayVictory();
            StartCoroutine(ShakePanel(0.05f, 0.2f));
            y -= 75f;
            yield return new WaitForSecondsRealtime(0.4f);
        }

        Color ribbonColor = RibbonManager.GetRibbonColor(levelId);
        Sprite ribbonSprite = RibbonManager.GetRibbonSprite(levelId);
        GameObject ribbonRow = CreateRibbonRowHidden(panel.transform, font, ribbonColor, ribbonSprite, y);
        yield return new WaitForSecondsRealtime(0.15f);
        yield return RevealRow(ribbonRow, font);
        SoundManager.Instance.PlaySelect();
        StartCoroutine(ShakePanel(0.05f, 0.2f));
        y -= 60f;
        yield return new WaitForSecondsRealtime(0.4f);

        if (chestGranted)
        {
            GameObject chestRow = CreateChestRowHidden(panel.transform, font, y);
            yield return new WaitForSecondsRealtime(0.15f);
            yield return RevealRow(chestRow, font);
            SoundManager.Instance.PlayHammer();
            StartCoroutine(ShakePanel(0.1f, 0.35f));
            StartCoroutine(AnimateChestBounce(chestRow));
            yield return new WaitForSecondsRealtime(0.4f);
        }

        if (!string.IsNullOrEmpty(cupCompletedRace))
        {
            GameObject cupRow = CreateCupRowHidden(panel.transform, font, cupCompletedRace, y);
            yield return new WaitForSecondsRealtime(0.15f);
            yield return RevealRow(cupRow, font);
            SoundManager.Instance.PlayVictory();
            StartCoroutine(ShakePanel(0.12f, 0.4f));
            Transform cupIcon = cupRow.transform.Find("Cup");
            if (cupIcon != null) StartCoroutine(CupBounceAnimation(cupIcon));
            yield return new WaitForSecondsRealtime(0.6f);
        }

        Sprite okSprite = Resources.Load<Sprite>("Sprites/Menu/botonOK_0");
        GameObject okBtn = new GameObject("OKButton");
        okBtn.transform.SetParent(panel.transform, false);
        Image okBg = okBtn.AddComponent<Image>();
        if (okSprite != null) { okBg.sprite = okSprite; okBg.color = Color.white; }
        else okBg.color = new Color(0.25f, 0.45f, 0.75f);
        RectTransform okRt = okBtn.GetComponent<RectTransform>();
        okRt.anchorMin = new Vector2(0.5f, 0.5f);
        okRt.anchorMax = new Vector2(0.5f, 0.5f);
        okRt.sizeDelta = new Vector2(220, 70);
        okRt.anchoredPosition = new Vector2(0, -210);

        GameObject okLabel = new GameObject("Label");
        okLabel.transform.SetParent(okBtn.transform, false);
        Text okText = okLabel.AddComponent<Text>();
        okText.font = font;
        okText.text = "OK";
        okText.fontSize = 14;
        okText.alignment = TextAnchor.MiddleCenter;
        okText.color = Color.white;
        RectTransform okLblRt = okLabel.GetComponent<RectTransform>();
        okLblRt.anchorMin = Vector2.zero;
        okLblRt.anchorMax = Vector2.one;
        okLblRt.sizeDelta = Vector2.zero;

        Button okButton = okBtn.AddComponent<Button>();
        okButton.targetGraphic = okBg;
        okButton.onClick.AddListener(OnOK);

        okBtn.transform.localScale = Vector3.zero;
        StartCoroutine(ScalePop(okBtn.transform, 0.3f, 1f));

        StartCoroutine(AnimateSparkles(panel.transform, new Vector2(-200, 180), new Vector2(200, 200)));
    }

    IEnumerator RevealRow(GameObject row, Font font)
    {
        if (row == null) yield break;

        CanvasGroup cg = row.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        row.transform.localScale = Vector3.one * 0.3f;

        Image[] images = row.GetComponentsInChildren<Image>();
        List<Image> flashTargets = new List<Image>();
        foreach (Image img in images)
        {
            if (img != null && img.gameObject != row)
                flashTargets.Add(img);
        }

        float flashDur = 0.15f;
        float t = 0f;
        while (t < flashDur)
        {
            if (row == null) yield break;
            t += Time.unscaledDeltaTime;
            float p = t / flashDur;
            cg.alpha = p;
            row.transform.localScale = Vector3.Lerp(Vector3.one * 0.3f, Vector3.one * 1.2f, p);
            foreach (Image img in flashTargets)
                if (img != null) img.color = Color.Lerp(new Color(2f, 2f, 2f, 1f), Color.white, p);
            yield return null;
        }

        t = 0f;
        float settleDur = 0.12f;
        while (t < settleDur)
        {
            if (row == null) yield break;
            t += Time.unscaledDeltaTime;
            float p = t / settleDur;
            row.transform.localScale = Vector3.Lerp(Vector3.one * 1.2f, Vector3.one, p);
            yield return null;
        }
        if (row != null) row.transform.localScale = Vector3.one;
    }

    IEnumerator ScalePop(Transform target, float dur, float from = 2.5f)
    {
        if (target == null) yield break;
        float t = 0f;
        while (t < dur)
        {
            if (target == null) yield break;
            t += Time.unscaledDeltaTime;
            float p = t / dur;
            float ease = 1f - Mathf.Pow(1f - p, 3f);
            float scale = Mathf.Lerp(from, 1f, ease);
            target.localScale = Vector3.one * scale;
            yield return null;
        }
        if (target != null) target.localScale = Vector3.one;
    }

    IEnumerator ShakePanel(float intensity, float dur)
    {
        if (panelRt == null) yield break;
        Vector2 original = panelRt.anchoredPosition;
        float t = 0f;
        while (t < dur)
        {
            if (panelRt == null) yield break;
            t += Time.unscaledDeltaTime;
            float decay = 1f - (t / dur);
            float x = Random.Range(-1f, 1f) * intensity * decay;
            float y = Random.Range(-1f, 1f) * intensity * decay;
            panelRt.anchoredPosition = original + new Vector2(x, y);
            yield return null;
        }
        if (panelRt != null) panelRt.anchoredPosition = original;
    }

    IEnumerator AnimateCoinBounce(GameObject row)
    {
        if (row == null) yield break;
        Transform coinT = row.transform.Find("Coin");
        if (coinT == null) yield break;

        for (int i = 0; i < 3; i++)
        {
            float bounceH = 12f - i * 3f;
            float dur = 0.12f;
            float t = 0f;
            Vector2 origPos = coinT.localPosition;
            while (t < dur)
            {
                if (coinT == null) yield break;
                t += Time.unscaledDeltaTime;
                float p = t / dur;
                float h = Mathf.Sin(p * Mathf.PI) * bounceH;
                coinT.localPosition = origPos + new Vector2(0, h);
                yield return null;
            }
            if (coinT != null) coinT.localPosition = origPos;
        }
    }

    IEnumerator AnimateChestBounce(GameObject row)
    {
        if (row == null) yield break;
        Transform chestT = row.transform.Find("Chest");
        if (chestT == null) yield break;

        for (int i = 0; i < 2; i++)
        {
            float dur = 0.15f;
            float t = 0f;
            Vector3 orig = chestT.localScale;
            while (t < dur)
            {
                if (chestT == null) yield break;
                t += Time.unscaledDeltaTime;
                float p = t / dur;
                float bump = 1f + 0.25f * Mathf.Sin(p * Mathf.PI);
                chestT.localScale = orig * bump;
                yield return null;
            }
            if (chestT != null) chestT.localScale = orig;
        }
    }

    IEnumerator SpawnCoinParticles(Transform panelParent, Vector2 rowPos)
    {
        Sprite coinSprite = Resources.Load<Sprite>("Sprites/Menu/moneda");
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(Random.Range(0.02f, 0.08f));
            if (canvasObj == null) yield break;

            GameObject coin = new GameObject("CoinParticle");
            coin.transform.SetParent(panelParent, false);
            Image coinImg = coin.AddComponent<Image>();
            if (coinSprite != null) coinImg.sprite = coinSprite;
            coinImg.color = new Color(1f, 0.85f, 0.2f, 0.9f);
            coinImg.raycastTarget = false;
            RectTransform coinRt = coin.GetComponent<RectTransform>();
            coinRt.anchorMin = new Vector2(0.5f, 0.5f);
            coinRt.anchorMax = new Vector2(0.5f, 0.5f);
            float sz = Random.Range(12f, 22f);
            coinRt.sizeDelta = new Vector2(sz, sz);
            coinRt.anchoredPosition = rowPos + new Vector2(Random.Range(-80f, 80f), 30f);

            float speed = Random.Range(80f, 180f);
            float drift = Random.Range(-60f, 60f);
            float rotSpeed = Random.Range(-300f, 300f);
            float life = Random.Range(0.4f, 0.7f);
            StartCoroutine(AnimateCoinFly(coin, coinRt, coinImg, speed, drift, rotSpeed, life));
        }
    }

    IEnumerator AnimateCoinFly(GameObject coin, RectTransform rt, Image img, float speed, float drift, float rot, float life)
    {
        float t = 0f;
        Vector2 start = rt.anchoredPosition;
        while (t < life)
        {
            if (coin == null) yield break;
            t += Time.unscaledDeltaTime;
            float p = t / life;
            rt.anchoredPosition = start + new Vector2(drift * p, speed * p);
            rt.localRotation = Quaternion.Euler(0, 0, rot * t);
            if (img != null) img.color = new Color(1f, 0.85f, 0.2f, 0.9f * (1f - p));
            yield return null;
        }
        if (coin != null) Destroy(coin);
    }

    GameObject CreateGoldRowHidden(Transform parent, Font font, int gold, float y)
    {
        GameObject row = new GameObject("GoldRow");
        row.transform.SetParent(parent, false);
        RectTransform rt = row.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(400, 50);
        rt.anchoredPosition = new Vector2(0, y);

        Sprite coinSprite = Resources.Load<Sprite>("Sprites/Menu/moneda");
        if (coinSprite != null)
        {
            GameObject coinObj = new GameObject("Coin");
            coinObj.transform.SetParent(row.transform, false);
            Image coinImg = coinObj.AddComponent<Image>();
            coinImg.sprite = coinSprite;
            coinImg.color = Color.white;
            RectTransform coinRt = coinObj.GetComponent<RectTransform>();
            coinRt.anchorMin = new Vector2(0.3f, 0.5f);
            coinRt.anchorMax = new Vector2(0.3f, 0.5f);
            coinRt.sizeDelta = new Vector2(40, 40);
        }

        MakeText(row.transform, $"+{gold} GOLD", font, 16, new Color(1f, 0.85f, 0.2f), new Vector2(50, 0));
        return row;
    }

    GameObject CreateInsigniaRowHidden(Transform parent, Font font, Insignia insignia, float y)
    {
        GameObject row = new GameObject("InsigniaRow");
        row.transform.SetParent(parent, false);
        RectTransform rt = row.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(400, 70);
        rt.anchoredPosition = new Vector2(0, y);

        Sprite icon = InsigniaSprites.Get(insignia);
        if (icon != null)
        {
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(row.transform, false);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = icon;
            iconImg.color = Color.white;
            RectTransform iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.25f, 0.5f);
            iconRt.anchorMax = new Vector2(0.25f, 0.5f);
            iconRt.sizeDelta = new Vector2(50, 50);
        }

        Color rarityColor = InsigniaSprites.GetRarityColor(insignia.rarity);
        MakeText(row.transform, insignia.name.ToUpper(), font, 12, rarityColor, new Vector2(60, 10));
        MakeText(row.transform, "INSIGNIA", font, 9, new Color(0.6f, 0.6f, 0.6f), new Vector2(60, -15));
        return row;
    }

    GameObject CreateRibbonRowHidden(Transform parent, Font font, Color ribbonColor, Sprite ribbonSprite, float y)
    {
        GameObject row = new GameObject("RibbonRow");
        row.transform.SetParent(parent, false);
        RectTransform rt = row.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(400, 60);
        rt.anchoredPosition = new Vector2(0, y);

        if (ribbonSprite != null)
        {
            GameObject ribbonObj = new GameObject("Ribbon");
            ribbonObj.transform.SetParent(row.transform, false);
            Image ribbonImg = ribbonObj.AddComponent<Image>();
            ribbonImg.sprite = ribbonSprite;
            ribbonImg.color = ribbonColor;
            ribbonImg.type = Image.Type.Simple;
            ribbonImg.preserveAspect = true;
            RectTransform ribbonRt = ribbonObj.GetComponent<RectTransform>();
            ribbonRt.anchorMin = new Vector2(0.28f, 0.5f);
            ribbonRt.anchorMax = new Vector2(0.28f, 0.5f);
            ribbonRt.sizeDelta = new Vector2(40, 55);
        }
        else
        {
            GameObject ribbonObj = new GameObject("Ribbon");
            ribbonObj.transform.SetParent(row.transform, false);
            Image ribbonImg = ribbonObj.AddComponent<Image>();
            ribbonImg.color = ribbonColor;
            RectTransform ribbonRt = ribbonObj.GetComponent<RectTransform>();
            ribbonRt.anchorMin = new Vector2(0.28f, 0.5f);
            ribbonRt.anchorMax = new Vector2(0.28f, 0.5f);
            ribbonRt.sizeDelta = new Vector2(30, 45);
        }

        MakeText(row.transform, "RIBBON", font, 12, ribbonColor, new Vector2(60, 5));
        MakeText(row.transform, "Earned!", font, 10, new Color(0.6f, 0.6f, 0.6f), new Vector2(60, -15));
        return row;
    }

    GameObject CreateChestRowHidden(Transform parent, Font font, float y)
    {
        GameObject row = new GameObject("ChestRow");
        row.transform.SetParent(parent, false);
        RectTransform rt = row.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(400, 50);
        rt.anchoredPosition = new Vector2(0, y);

        Sprite chestSprite = Resources.Load<Sprite>("Sprites/Cofre");
        if (chestSprite != null)
        {
            GameObject chestObj = new GameObject("Chest");
            chestObj.transform.SetParent(row.transform, false);
            Image chestImg = chestObj.AddComponent<Image>();
            chestImg.sprite = chestSprite;
            chestImg.color = Color.white;
            RectTransform chestRt = chestObj.GetComponent<RectTransform>();
            chestRt.anchorMin = new Vector2(0.3f, 0.5f);
            chestRt.anchorMax = new Vector2(0.3f, 0.5f);
            chestRt.sizeDelta = new Vector2(35, 35);
        }

        MakeText(row.transform, "CHEST GRANTED!", font, 11, new Color(0.8f, 0.65f, 0.2f), new Vector2(50, 0));
        return row;
    }

    GameObject CreateCupRowHidden(Transform parent, Font font, string race, float y)
    {
        GameObject row = new GameObject("CupRow");
        row.transform.SetParent(parent, false);
        RectTransform rt = row.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(400, 80);
        rt.anchoredPosition = new Vector2(0, y);

        string cupPath = race switch
        {
            "Human" => "Sprites/Menu/copaHuman",
            "Orc" => "Sprites/Menu/copaOrc",
            "Beastfolk" => "Sprites/Menu/copaBeast",
            "Nigromantes" => "Sprites/Menu/copaMenu",
            _ => "Sprites/Menu/copaMenu"
        };
        Sprite cupSprite = Resources.Load<Sprite>(cupPath);
        if (cupSprite == null) cupSprite = Resources.Load<Sprite>("Sprites/Menu/copaMenu");

        GameObject cupObj = new GameObject("Cup");
        cupObj.transform.SetParent(row.transform, false);
        Image cupImg = cupObj.AddComponent<Image>();
        cupImg.sprite = cupSprite;
        cupImg.color = Color.white;
        cupImg.preserveAspect = true;
        RectTransform cupRt = cupObj.GetComponent<RectTransform>();
        cupRt.anchorMin = new Vector2(0.28f, 0.5f);
        cupRt.anchorMax = new Vector2(0.28f, 0.5f);
        cupRt.sizeDelta = new Vector2(60, 60);

        string cupName = CampaignData.GetCup(
            CampaignData.GetLevel(GameConfig.selectedLevel)?.cup ?? 0)?.name ?? "CUP";
        MakeText(row.transform, cupName.ToUpper() + " COMPLETED!", font, 13, new Color(1f, 0.85f, 0.2f), new Vector2(65, 10));
        MakeText(row.transform, "Trophy Unlocked!", font, 10, new Color(0.8f, 0.8f, 0.8f), new Vector2(65, -15));
        return row;
    }

    IEnumerator CupBounceAnimation(Transform cupTransform)
    {
        Vector3 targetScale = Vector3.one;
        float growDur = 0.25f;
        float shrinkDur = 0.2f;

        float t = 0f;
        while (t < growDur)
        {
            if (cupTransform == null) yield break;
            t += Time.unscaledDeltaTime;
            float p = t / growDur;
            float ease = 1f - Mathf.Pow(1f - p, 3f);
            cupTransform.localScale = Vector3.Lerp(Vector3.one * 0.3f, Vector3.one * 1.6f, ease);
            yield return null;
        }

        t = 0f;
        while (t < shrinkDur)
        {
            if (cupTransform == null) yield break;
            t += Time.unscaledDeltaTime;
            float p = t / shrinkDur;
            float ease = p * p;
            cupTransform.localScale = Vector3.Lerp(Vector3.one * 1.6f, targetScale, ease);
            yield return null;
        }

        if (cupTransform != null) cupTransform.localScale = targetScale;
    }

    void OnOK()
    {
        SoundManager.Instance.PlaySelect();
        StartCoroutine(FadeAndClose());
    }

    IEnumerator FadeAndClose()
    {
        if (canvasGroup != null)
        {
            float t = 0f;
            while (t < 0.2f) { t += Time.unscaledDeltaTime; canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / 0.2f); yield return null; }
        }

        if (ScoreboardUI.Instance != null)
            ScoreboardUI.Instance.ProceedToGameOver();
        else
            GameOverUI.Instance.Show(Team.Blue);

        if (canvasObj != null) Destroy(canvasObj);
        Destroy(gameObject);
    }

    IEnumerator AnimateSparkles(Transform parent, Vector2 minCorner, Vector2 maxCorner)
    {
        for (int i = 0; i < 15; i++)
        {
            yield return new WaitForSeconds(Random.Range(0.2f, 0.6f));
            if (canvasObj == null) yield break;

            GameObject sparkle = new GameObject("Sparkle");
            sparkle.transform.SetParent(parent, false);
            Image sparkImg = sparkle.AddComponent<Image>();
            sparkImg.color = new Color(1f, 0.95f, 0.5f, 0.9f);
            sparkImg.raycastTarget = false;
            RectTransform sparkRt = sparkle.GetComponent<RectTransform>();
            sparkRt.anchorMin = new Vector2(0.5f, 0.5f);
            sparkRt.anchorMax = new Vector2(0.5f, 0.5f);
            float sz = Random.Range(4f, 12f);
            sparkRt.sizeDelta = new Vector2(sz, sz);
            sparkRt.anchoredPosition = new Vector2(Random.Range(minCorner.x, maxCorner.x), Random.Range(minCorner.y, maxCorner.y));

            StartCoroutine(AnimateSingleSparkle(sparkle, 0.5f));
        }
    }

    IEnumerator AnimateSingleSparkle(GameObject obj, float dur)
    {
        if (obj == null) yield break;
        Image img = obj.GetComponent<Image>();
        float t = 0f;
        while (t < dur)
        {
            if (obj == null) yield break;
            t += Time.unscaledDeltaTime;
            float p = t / dur;
            if (img != null) img.color = new Color(1f, 0.95f, 0.5f, 0.9f * (1f - p));
            float s = 1f + 0.4f * Mathf.Sin(p * Mathf.PI);
            obj.transform.localScale = Vector3.one * s;
            yield return null;
        }
        if (obj != null) Destroy(obj);
    }

    Text MakeText(Transform parent, string content, Font font, int size, Color color, Vector2 pos)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);
        Text text = obj.AddComponent<Text>();
        text.font = font;
        text.text = content;
        text.fontSize = size;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.raycastTarget = false;
        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(380, size * 2);
        rt.anchoredPosition = pos;
        return text;
    }

    Sprite LoadFirstSprite(string path)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);
        return sprites != null && sprites.Length > 0 ? sprites[0] : null;
    }
}
