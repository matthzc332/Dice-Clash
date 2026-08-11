using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    private BoardManager board;
    private TurnManager turnManager;
    private InputManager input;

    void Start()
    {
        board = FindFirstObjectByType<BoardManager>();
        turnManager = FindFirstObjectByType<TurnManager>();
        input = FindFirstObjectByType<InputManager>();
        turnManager.OnTurnChanged += OnTurnChanged;
    }

    void OnDestroy()
    {
        if (turnManager != null)
            turnManager.OnTurnChanged -= OnTurnChanged;
    }

    void OnTurnChanged(TurnState state)
    {
        if (state == TurnState.RedTurn)
            StartCoroutine(PlayTurn(Team.Red));
        else if (GameConfig.isAutoPlay && state == TurnState.BlueTurn)
            StartCoroutine(PlayTurn(Team.Blue));
    }

    IEnumerator PlayTurn(Team team)
    {
        if (PowerUpManager.Instance != null)
            yield return new WaitUntil(() => !PowerUpManager.Instance.IsExecuting);

        float delay = GameConfig.isAutoPlay ? 0.15f : 2.5f;
        yield return new WaitForSeconds(delay);

        board.aiInProgress = true;
        if (input != null) input.aiPlaying = true;

        Team enemy = team == Team.Red ? Team.Blue : Team.Red;

        List<(Cell cell, PieceData data)> myPieces = new();

        for (int r = 0; r < board.rows; r++)
        {
            for (int c = 0; c < board.cols; c++)
            {
                Cell cell = board.grid[r, c];
                if (cell.IsOccupied && cell.pieceData?.team == team)
                    myPieces.Add((cell, cell.pieceData.Value));
            }
        }

        List<(Cell cell, PieceData data, Cell move)> candidates = new();

        foreach (var entry in myPieces)
        {
            Cell cell = entry.cell;
            PieceData data = entry.data;

            ObstacleManager om = FindFirstObjectByType<ObstacleManager>();
            if (om != null && om.IsPieceGlued(data.id))
            {
                List<Cell> adjMoves = board.GetValidMoves(cell.row, cell.col, data.type);
                List<Cell> attackOnly = new();
                foreach (var c in adjMoves)
                {
                    if (c.IsOccupied && c.pieceData.HasValue && c.pieceData.Value.team == enemy)
                        attackOnly.Add(c);
                }
                if (attackOnly.Count == 0) continue;
                Cell bestAttack = EvaluateMoves(cell, data, attackOnly, team);
                if (bestAttack == null) continue;
                candidates.Add((cell, data, bestAttack));
                continue;
            }

            List<Cell> moves = board.GetValidMoves(cell.row, cell.col, data.type);
            if (moves.Count == 0) continue;

            Cell best = EvaluateMoves(cell, data, moves, team);
            if (best == null) continue;

            candidates.Add((cell, data, best));
        }

        if (candidates.Count > 0)
        {
            var pick = candidates[Random.Range(0, candidates.Count)];

            if (team == Team.Red)
            {
                board.SetAIRedHighlight(pick.cell);
                board.SetAIRedMarker(board.CellToWorld(pick.move.row, pick.move.col));
                if (pick.move.IsOccupied && pick.move.pieceData?.team == Team.Blue)
                    board.SetAIGreenHighlight(pick.move);
            }
            else if (GameConfig.isAutoPlay)
            {
                board.SetAIRedHighlight(pick.cell);
                board.SetAIRedMarker(board.CellToWorld(pick.move.row, pick.move.col));
                if (pick.move.IsOccupied && pick.move.pieceData?.team == Team.Red)
                    board.SetAIGreenHighlight(pick.move);
            }

            float highlightDelay = GameConfig.isAutoPlay ? 0.1f : 1.2f;
            yield return new WaitForSeconds(highlightDelay);

            board.ClearAIHighlights();

            float turnStartTime = Time.realtimeSinceStartup;
            bool didAttack = pick.move.IsOccupied && pick.move.pieceData.HasValue && pick.move.pieceData.Value.team == enemy;

            yield return board.MovePieceAI(pick.cell.row, pick.cell.col, pick.move.row, pick.move.col);

            float turnDuration = Time.realtimeSinceStartup - turnStartTime;

            if (AutoPlayStats.Instance != null)
            {
                AutoPlayStats.Instance.LogTurn(
                    team.ToString(), pick.data.type.ToString(),
                    pick.cell.row, pick.cell.col,
                    pick.move.row, pick.move.col,
                    didAttack, turnDuration);
            }

            if (PowerUpManager.Instance != null)
                yield return new WaitUntil(() => !PowerUpManager.Instance.IsExecuting);

            Team? winner = board.CheckVictory();
            if (winner.HasValue)
            {
                if (GameConfig.isAutoPlay)
                {
                    int blueAlive = board.CountAlive(Team.Blue);
                    int redAlive = board.CountAlive(Team.Red);
                    AutoPlayManager.Instance?.OnMatchOver(winner.Value == Team.Blue ? "Blue" : "Red", blueAlive, redAlive);
                    yield return new WaitForSeconds(0.3f);
                    ScoreboardUI.Instance.Show();
                }
                else
                {
                    yield return new WaitForSeconds(0.3f);
                    ScoreboardUI.Instance.Show();
                }
            }
        }

        board.aiInProgress = false;
        if (input != null) input.aiPlaying = false;
        board.ClearAIHighlights();

        Team? finalWinner = board.CheckVictory();
        if (!finalWinner.HasValue)
        {
            if (PowerUpManager.Instance != null)
                yield return new WaitUntil(() => !PowerUpManager.Instance.IsExecuting);
            turnManager.EndTurn();
        }
    }

    Cell EvaluateMoves(Cell from, PieceData data, List<Cell> moves, Team myTeam)
    {
        Team enemy = myTeam == Team.Red ? Team.Blue : Team.Red;
        Cell best = null;
        float bestScore = float.MinValue;
        float noiseRange = 5f;

        int nearestEnemyDist = FindNearestEnemyDist(from, enemy);

        foreach (Cell move in moves)
        {
            float score = 0;

            if (move.IsOccupied && move.pieceData?.team == enemy)
            {
                PieceData enemyPiece = move.pieceData.Value;
                if (data.Tier >= enemyPiece.Tier)
                    score += 100;
                else
                    score += 50 + (enemyPiece.Tier - data.Tier) * 10;
            }
            else
            {
                if (PowerUpManager.Instance != null && PowerUpManager.Instance.IsPowerUpAt(move.row, move.col))
                    score += GameConfig.isAutoPlay ? 60 : 30;

                int advanceDir = myTeam == Team.Red ? 1 : -1;
                int advanceScore = GameConfig.isAutoPlay ? 35 : 20;
                if (move.row * advanceDir < from.row * advanceDir)
                    score += advanceScore;
                else if (move.row == from.row)
                    score += GameConfig.isAutoPlay ? 15 : 10;
                else
                    score += 5;

                int newDist = Mathf.Abs(move.row - nearestEnemyDist);
                int curDist = Mathf.Abs(from.row - nearestEnemyDist);
                if (newDist < curDist)
                    score += GameConfig.isAutoPlay ? 25 : 0;
            }

            score += Random.Range(-noiseRange, noiseRange);

            if (score > bestScore)
            {
                bestScore = score;
                best = move;
            }
        }

        return best;
    }

    int FindNearestEnemyDist(Cell from, Team enemy)
    {
        BoardManager board = FindAnyObjectByType<BoardManager>();
        int best = 999;
        for (int r = 0; r < board.rows; r++)
            for (int c = 0; c < board.cols; c++)
            {
                Cell cell = board.GetCell(r, c);
                if (cell != null && cell.IsOccupied && cell.pieceData?.team == enemy)
                {
                    int d = Mathf.Abs(r - from.row) + Mathf.Abs(c - from.col);
                    if (d < best) best = d;
                }
            }
        return best;
    }
}
