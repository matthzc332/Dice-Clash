using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AutoPlayManager : MonoBehaviour
{
    public static AutoPlayManager Instance { get; private set; }

    private static readonly string[] worlds = { "Human", "Orc", "Beastfolk" };

    private int currentMatch;
    private int totalMatches;
    private bool matchInProgress;
    private bool matchEnded;
    private float matchTimeout = 300f;
    private float matchStartTime;
    private bool waitingForLoad;
    private UnityEngine.UI.Text statusText;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (matchInProgress && !matchEnded)
        {
            float elapsed = Time.time - matchStartTime;
            if (elapsed >= matchTimeout)
            {
                HandleTimeout();
            }
        }
    }

    void HandleTimeout()
    {
        if (matchEnded) return;
        matchEnded = true;
        matchInProgress = false;

        AutoPlayStats.Instance?.errors.Add($"[Timeout] Match {currentMatch} timed out after {matchTimeout}s");

        int blueAlive = 0;
        int redAlive = 0;
        BoardManager bm = FindAnyObjectByType<BoardManager>();
        if (bm != null)
        {
            blueAlive = bm.CountAlive(Team.Blue);
            redAlive = bm.CountAlive(Team.Red);
        }
        string winner = blueAlive >= redAlive ? "Blue" : "Red";
        int turnNum = 0;
        TurnManager tm = FindAnyObjectByType<TurnManager>();
        if (tm != null) turnNum = tm.turnNumber;

        AutoPlayStats.Instance?.EndMatch(winner, blueAlive, redAlive, turnNum, 0, 0);

        ScoreboardUI sb = FindAnyObjectByType<ScoreboardUI>();
        if (sb != null && !sb.gameObject.activeSelf)
        {
            sb.Show();
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!GameConfig.isAutoPlay) return;
        if (scene.name != "SampleScene") return;
        if (waitingForLoad)
        {
            waitingForLoad = false;
            StartCoroutine(RunSingleMatch());
        }
    }

    public void StartAutoPlay(int matches)
    {
        totalMatches = matches;
        currentMatch = 0;

        AutoPlayStats.Instance?.Deinit();
        var stats = new AutoPlayStats();
        stats.Init();

        CreateStatusUI();
        StartCoroutine(StartFirstMatch());
    }

    void CreateStatusUI()
    {
        if (statusText != null) return;

        GameObject canvasObj = new GameObject("AutoPlayCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasObj);

        GameObject bgObj = new GameObject("StatusBG", typeof(RectTransform));
        bgObj.transform.SetParent(canvasObj.transform, false);
        UnityEngine.UI.Image bgImg = bgObj.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0, 0, 0, 0.85f);
        bgImg.raycastTarget = false;
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0.5f, 1f);
        bgRt.anchorMax = new Vector2(0.5f, 1f);
        bgRt.pivot = new Vector2(0.5f, 1f);
        bgRt.sizeDelta = new Vector2(600, 80);
        bgRt.anchoredPosition = new Vector2(0, 0);

        GameObject textObj = new GameObject("StatusText", typeof(RectTransform));
        textObj.transform.SetParent(bgObj.transform, false);
        statusText = textObj.AddComponent<UnityEngine.UI.Text>();
        Font font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statusText.font = font;
        statusText.fontSize = 14;
        statusText.alignment = TextAnchor.MiddleCenter;
        statusText.color = Color.white;
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        UpdateStatus("Starting Auto-Play...");
    }

    void UpdateStatus(string msg)
    {
        if (statusText != null)
            statusText.text = msg;
    }

    IEnumerator StartFirstMatch()
    {
        string world = worlds[0];
        GameConfig.selectedSpecies = world;
        GameConfig.selectedScenario = world;
        yield return new WaitForSeconds(0.5f);
        currentMatch = 1;
        yield return StartCoroutine(RunSingleMatch());
    }

    IEnumerator RunSingleMatch()
    {
        string world = worlds[(currentMatch - 1) % worlds.Length];
        UpdateStatus($"Auto-Play: Match {currentMatch}/{totalMatches} [{world}]");
        matchEnded = false;
        matchInProgress = true;
        matchStartTime = Time.time;

        AutoPlayStats.Instance?.StartMatch(currentMatch);

        yield return null;
        yield return null;

        while (!matchEnded)
        {
            AutoPlayStats.Instance?.RecordFrame();
            yield return null;

            ScoreboardUI sb = FindAnyObjectByType<ScoreboardUI>();
            if (sb != null && sb.gameObject.activeSelf && !matchEnded)
            {
                matchEnded = true;
            }
        }

        if (currentMatch >= totalMatches)
        {
            string reportPath = ReportPath();
            AutoPlayStats.Instance?.WriteReport(reportPath);
            UpdateStatus($"Auto-Play COMPLETE! {totalMatches} matches. Report: {reportPath}");
            Debug.Log($"[AutoPlay] All {totalMatches} matches complete. Report: {reportPath}");

            yield return new WaitForSeconds(5f);
            GameConfig.isAutoPlay = false;
            SceneManager.LoadScene("MainMenuScene");
            yield break;
        }

        UpdateStatus($"Auto-Play: Match {currentMatch}/{totalMatches} done. Loading next...");
        yield return new WaitForSeconds(0.3f);

        currentMatch++;
        waitingForLoad = true;
        GameConfig.isAutoPlay = true;
        GameConfig.autoPlayMatches = totalMatches;

        string nextWorld = worlds[(currentMatch - 1) % worlds.Length];
        GameConfig.selectedSpecies = nextWorld;
        GameConfig.selectedScenario = nextWorld;

        SceneManager.LoadScene("SampleScene");
    }

    public void OnMatchOver(string winner, int blueAlive, int redAlive)
    {
        if (!matchInProgress || matchEnded) return;
        matchEnded = true;
        matchInProgress = false;

        int turnNum = 0;
        TurnManager tm = FindAnyObjectByType<TurnManager>();
        if (tm != null) turnNum = tm.turnNumber;

        AutoPlayStats.Instance?.EndMatch(winner, blueAlive, redAlive, turnNum, 0, 0);
    }

    static string ReportPath()
    {
        string dir = Application.persistentDataPath;
        return System.IO.Path.Combine(dir, "auto_play_report.txt");
    }
}
