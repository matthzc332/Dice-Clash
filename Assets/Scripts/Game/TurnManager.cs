using UnityEngine;

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
