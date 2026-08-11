using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        SetupEventSystem();
        SetupCamera();
        if (CampaignManager.Instance == null)
        {
            GameObject cmObj = new GameObject("CampaignManager");
            cmObj.AddComponent<CampaignManager>();
        }
        TurnManager turn = FindAnyObjectByType<TurnManager>();
        if (turn == null) turn = CreateTurnManager();
        BoardManager board = FindAnyObjectByType<BoardManager>();
        if (board == null) board = CreateBoardManager();

        ObstacleManager om = FindAnyObjectByType<ObstacleManager>();
        if (om != null) om.SubscribeTurnManager(turn);
        InputManager input = FindAnyObjectByType<InputManager>();
        if (input == null) CreateInputManager(board, turn);
        CreateTurnUI(turn);
        if (GameConfig.isTutorial)
        {
            gameObject.AddComponent<TutorialManager>();
        }
        else if (GameConfig.isAutoPlay)
        {
            CreateAIController();
            if (AutoPlayManager.Instance == null)
            {
                GameObject apObj = new GameObject("AutoPlayManager");
                AutoPlayManager apm = apObj.AddComponent<AutoPlayManager>();
                apm.StartAutoPlay(GameConfig.autoPlayMatches);
            }
            ObstacleManager omAuto = FindAnyObjectByType<ObstacleManager>();
            if (omAuto != null) omAuto.SpawnAutoPlayObstacles();
            StartCoroutine(InitPowerUpsDelayed());
        }
        else
        {
            CreateAIController();
            gameObject.AddComponent<TestButtons>();
            TimerManager.Instance.StartTimer(300f);
            TimerManager.Instance.OnTimerExpired += OnMatchTimerExpired;
            StartCoroutine(InitPowerUpsDelayed());

            if (GameConfig.isCampaign && GameConfig.selectedLevel > 0)
                StartCoroutine(ShowEnemyBannerDelayed());
        }

        if (!GameConfig.isAutoPlay)
        {
            gameObject.AddComponent<CoinManager>();
            gameObject.AddComponent<EconomyManager>();
        }
        SoundManager.Instance.StopMusic();
        if (!GameConfig.isAutoPlay)
            SoundManager.Instance.PlayGameplayMusic();
        else
            AudioListener.volume = 0f;
    }

    void SetupEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }
    }

    void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = FindFirstObjectByType<Camera>();
            if (cam != null)
                cam.gameObject.tag = "MainCamera";
            else
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }
        }

        cam.orthographic = true;
        cam.transform.rotation = Quaternion.identity;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(198f / 255f, 198f / 255f, 198f / 255f);

        if (cam.GetComponent<AudioListener>() == null)
            cam.gameObject.AddComponent<AudioListener>();

        CameraScaler scaler = cam.GetComponent<CameraScaler>();
        if (scaler != null) scaler.enabled = false;
    }

    TurnManager CreateTurnManager()
    {
        GameObject obj = new GameObject("TurnManager");
        return obj.AddComponent<TurnManager>();
    }

    BoardManager CreateBoardManager()
    {
        GameObject obj = new GameObject("BoardManager");
        BoardManager board = obj.AddComponent<BoardManager>();
        board.speciesTheme = GameConfig.selectedSpecies;
        board.scenarioTheme = GameConfig.selectedScenario;
        return board;
    }

    InputManager CreateInputManager(BoardManager board, TurnManager turn)
    {
        GameObject obj = new GameObject("InputManager");
        InputManager input = obj.AddComponent<InputManager>();
        input.board = board;
        input.turnManager = turn;
        return input;
    }

    TurnUI CreateTurnUI(TurnManager turn)
    {
        GameObject obj = new GameObject("TurnUI");
        TurnUI ui = obj.AddComponent<TurnUI>();
        ui.turnManager = turn;
        return ui;
    }

    void CreateAIController()
    {
        GameObject obj = new GameObject("AIController");
        obj.AddComponent<AIController>();
    }

    IEnumerator InitPowerUpsDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        if (PowerUpManager.Instance == null)
        {
            GameObject puObj = new GameObject("PowerUpManager");
            puObj.AddComponent<PowerUpManager>();
        }
        yield return null;
        PowerUpManager.Instance?.SpawnOnBoard();
        yield return new WaitForSeconds(0.3f);
        PowerUpManager.Instance?.SpawnOnBoard();
    }

    public int GetCurrentTurn()
    {
        TurnManager t = FindAnyObjectByType<TurnManager>();
        return t != null ? t.turnNumber : 0;
    }

    void OnMatchTimerExpired()
    {
        if (GameConfig.isAutoPlay) return;
        ScoreboardUI.Instance.Show();
    }

    IEnumerator ShowEnemyBannerDelayed()
    {
        yield return new WaitForSeconds(1f);

        CampaignLevel level = CampaignData.GetLevel(GameConfig.selectedLevel);
        if (level == null) yield break;

        string armyName = level.name;
        Color raceColor = GetRaceColor(level.enemyRace);

        GameObject bannerObj = new GameObject("EnemyBanner");
        EnemyBanner banner = bannerObj.AddComponent<EnemyBanner>();
        banner.Show(armyName, raceColor);
    }

    Color GetRaceColor(string race)
    {
        switch (race)
        {
            case "Human": return new Color(0.3f, 0.5f, 1f);
            case "Orc": return new Color(0.3f, 0.8f, 0.3f);
            case "Wolf": return new Color(0.8f, 0.5f, 0.2f);
            case "Beastfolk": return new Color(0.6f, 0.8f, 0.3f);
            case "NewRace": return new Color(0.6f, 0.3f, 0.8f);
            default: return Color.white;
        }
    }
}
