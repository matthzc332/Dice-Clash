using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DebugShortcuts : MonoBehaviour
{
    private static DebugShortcuts _instance;

    private static readonly string[] TestButtonNames =
    {
        "TutorialButton",
        "CampaignButton",
        "ChestButton",
        "InsigniaButton",
        "SpeciesButton",
        "ScenarioButton",
        "RankedButton",
    };

    public static bool DevBuild
    {
        get
        {
            if (Application.isEditor) return true;
            RuntimePlatform p = Application.platform;
            return p == RuntimePlatform.WindowsPlayer || p == RuntimePlatform.LinuxPlayer || p == RuntimePlatform.OSXPlayer;
        }
    }

    public static DebugShortcuts Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("DebugShortcuts");
                _instance = go.AddComponent<DebugShortcuts>();
            }
            return _instance;
        }
    }

    private bool testButtonsHidden;
    private List<GameObject> testButtons = new List<GameObject>();
    private bool initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        _ = Instance;
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ReapplyAfterFrame());
    }

    IEnumerator ReapplyAfterFrame()
    {
        yield return null;
        initialized = false;
        SetTestButtons(testButtonsHidden);
    }

    void Update()
    {
        if (!DevBuild) return;

        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        if (ctrl && Input.GetKeyDown(KeyCode.F))
            ToggleTestButtons();
        if (ctrl && Input.GetKeyDown(KeyCode.R))
            RestartToPostTutorial();
        if (ctrl && Input.GetKeyDown(KeyCode.D))
            ShowDailyBonus();
    }

    void ShowDailyBonus()
    {
        if (DailyBonusUI.Instance != null)
            DailyBonusUI.Instance.ShowForDebug();
        else if (GameObject.FindFirstObjectByType<DailyBonusUI>() != null)
            GameObject.FindFirstObjectByType<DailyBonusUI>().ShowForDebug();
    }

    void ToggleTestButtons()
    {
        SetTestButtons(!testButtonsHidden);
    }

    void SetTestButtons(bool hidden)
    {
        if (!initialized)
        {
            CacheTestButtons();
            initialized = true;
        }
        testButtonsHidden = hidden;
        foreach (GameObject go in testButtons)
        {
            if (go != null)
                go.SetActive(!testButtonsHidden);
        }
    }

    void CacheTestButtons()
    {
        testButtons.Clear();
        foreach (Button b in FindObjectsByType<Button>(FindObjectsSortMode.None))
        {
            if (b == null) continue;
            if (IsTestButton(b.transform))
                testButtons.Add(b.gameObject);
        }
    }

    bool IsTestButton(Transform t)
    {
        Transform n = t;
        while (n != null)
        {
            if (n.name == "TestCanvas") return true;
            for (int i = 0; i < TestButtonNames.Length; i++)
            {
                if (n.name == TestButtonNames[i])
                    return true;
            }
            if (n.name == "QuitButton")
                return IsUnderCanvas(t, "TurnCanvas");
            n = n.parent;
        }
        return false;
    }

    bool IsUnderCanvas(Transform t, string canvasName)
    {
        Transform n = t;
        while (n != null)
        {
            if (n.name == canvasName) return true;
            n = n.parent;
        }
        return false;
    }

    void RestartToPostTutorial()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        if (ExpoConfig.Enabled)
            ExpoConfig.ApplyBootState();
        else
            ApplyPostTutorialState();

        int levelId = PostTutorialLevel();
        GameConfig.PlayCampaign(levelId, PowerupMode.WithPowerups);
    }

    static void ApplyPostTutorialState()
    {
        TutorialProgress.MarkPlayed();
        PlayerPrefs.SetInt("Campaign_Level_1", 1);
        PlayerPrefs.SetInt("Campaign_Level_2", 1);
        PlayerPrefs.Save();
    }

    static int PostTutorialLevel()
    {
        var data = CampaignData.Load();
        if (data != null && data.cups != null && data.cups.Length > 1)
        {
            foreach (int lid in data.cups[1].levels)
            {
                bool completed = PlayerPrefs.GetInt("Campaign_Level_" + lid, 0) == 1;
                bool unlocked = lid <= 2 || PlayerPrefs.GetInt("Campaign_Level_" + (lid - 1), 0) == 1;
                if (!completed && unlocked)
                    return lid;
            }
        }
        return 3;
    }
}