using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum TurnState
{
    BlueTurn,
    RedTurn
}

public class TurnManager : MonoBehaviour
{
    [Header("Turn State")]
    public TurnState currentTurn = TurnState.BlueTurn;
    public int turnNumber = 1;

    public float turnTimeLimit = 20f;
    public float turnTimeRemaining;
    public bool timerRunning;
    public bool warningShown;
    public bool matchOver;

    public System.Action<TurnState> OnTurnChanged;
    public System.Action OnTurnTimerWarning;

    void Start()
    {
        ResetTurnTimer();
    }

    public Team GetCurrentTeam()
    {
        return currentTurn == TurnState.BlueTurn ? Team.Blue : Team.Red;
    }

    void Update()
    {
        if (GameConfig.isAutoPlay) return;
        if (timerRunning && currentTurn == TurnState.BlueTurn)
        {
            turnTimeRemaining -= Time.deltaTime;

            if (!warningShown && turnTimeRemaining <= 5f)
            {
                warningShown = true;
                OnTurnTimerWarning?.Invoke();
            }

            if (turnTimeRemaining <= 0f)
            {
                turnTimeRemaining = 0f;
                EndTurn();
            }
        }
    }

    private bool endingTurn;

    public void EndTurn()
    {
        if (endingTurn) return;
        endingTurn = true;
        timerRunning = false;
        warningShown = false;

        BoardManager bm = FindFirstObjectByType<BoardManager>();
        if (bm != null && !bm.suppressVictoryCheck)
        {
            int blueAlive = bm.CountAlive(Team.Blue);
            int redAlive = bm.CountAlive(Team.Red);
            if (blueAlive == 0 || redAlive == 0)
            {
                bm.StartCoroutine(bm.CheckVictoryAndEndTurn());
                endingTurn = false;
                return;
            }
        }

        currentTurn = currentTurn == TurnState.BlueTurn ? TurnState.RedTurn : TurnState.BlueTurn;

        if (currentTurn == TurnState.BlueTurn)
            turnNumber++;

        OnTurnChanged?.Invoke(currentTurn);
        endingTurn = false;

        if (currentTurn == TurnState.RedTurn)
            ShowEnemyTurnBanner();
    }

    void ShowEnemyTurnBanner()
    {
        if (GameConfig.isTutorial || GameConfig.isAutoPlay) return;
        if (matchOver) return;
        StartCoroutine(EnemyTurnBannerCoroutine());
    }

    IEnumerator EnemyTurnBannerCoroutine()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        if (GameConfig.isTutorial || GameConfig.isAutoPlay) yield break;
        if (matchOver) yield break;
        SoundManager.Instance?.PlaySelect();

        Font font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject bannerObj = new GameObject("EnemyTurnBanner");
        Canvas bannerCanvas = bannerObj.AddComponent<Canvas>();
        bannerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        bannerCanvas.sortingOrder = 220;
        CanvasScaler scaler = bannerObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        bannerObj.AddComponent<GraphicRaycaster>();

        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(bannerObj.transform, false);
        Image panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.75f, 0.05f, 0.05f, 0.88f);
        panelImg.raycastTarget = false;
        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(900, 110);

        Sprite panelSprite = Resources.Load<Sprite>("Sprites/Menu/panelVictoria");
        if (panelSprite == null) panelSprite = Resources.Load<Sprite>("Sprites/Menu/panelCartaBlue");
        if (panelSprite != null)
        {
            panelImg.sprite = panelSprite;
            panelImg.preserveAspect = false;
        }

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(panelObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.font = font;
        text.text = "ENEMY TURN";
        text.fontSize = 48;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(1f, 0.9f, 0.3f, 1f);
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.1f, 0f, 0f, 0.9f);
        outline.effectDistance = new Vector2(3, -3);
        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = new Vector2(4, -4);
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        float screenWidth = 1920f;
        float startX = -screenWidth;
        float endX = screenWidth;
        float y = 0f;
        float duration = 0.4f;
        float holdTime = 0.6f;
        float t = 0f;

        while (t < duration)
        {
            if (bannerObj == null) yield break;
            t += Time.unscaledDeltaTime;
            float p = t / duration;
            float eased = 1f - Mathf.Pow(1f - p, 3f);
            float xPos = Mathf.Lerp(startX, 0, eased);
            panelRt.anchoredPosition = new Vector2(xPos, y);
            float alpha = Mathf.Lerp(0f, 1f, p);
            panelImg.color = new Color(0.75f, 0.05f, 0.05f, 0.88f * alpha);
            text.color = new Color(1f, 0.9f, 0.3f, alpha);
            yield return null;
        }
        panelRt.anchoredPosition = new Vector2(0, y);

        float shakeT = 0f;
        while (shakeT < 0.15f)
        {
            if (bannerObj == null) yield break;
            shakeT += Time.unscaledDeltaTime;
            float sx = Mathf.Sin(shakeT * 60f) * 3f * (1f - shakeT / 0.15f);
            panelRt.anchoredPosition = new Vector2(sx, y);
            yield return null;
        }
        panelRt.anchoredPosition = new Vector2(0, y);

        yield return new WaitForSecondsRealtime(holdTime);

        t = 0f;
        while (t < duration)
        {
            if (bannerObj == null) yield break;
            t += Time.unscaledDeltaTime;
            float p = t / duration;
            float xPos = Mathf.Lerp(0, endX, p * p);
            panelRt.anchoredPosition = new Vector2(xPos, y);
            float alpha = Mathf.Lerp(1f, 0f, p);
            panelImg.color = new Color(0.75f, 0.05f, 0.05f, 0.88f * alpha);
            text.color = new Color(1f, 0.9f, 0.3f, alpha);
            yield return null;
        }

        if (bannerObj != null) Destroy(bannerObj);
    }

    public void ResetTurnTimer()
    {
        turnTimeRemaining = turnTimeLimit;
        timerRunning = currentTurn == TurnState.BlueTurn;
        warningShown = false;
    }

    public void PauseTimer()
    {
        timerRunning = false;
        matchOver = true;
    }

    public void ResetMatchOver()
    {
        matchOver = false;
    }

    public void ResumeTimer()
    {
        if (currentTurn == TurnState.BlueTurn)
        {
            timerRunning = true;
        }
    }

    public float GetTurnTimeNormalized()
    {
        return Mathf.Clamp01(turnTimeRemaining / turnTimeLimit);
    }
}
