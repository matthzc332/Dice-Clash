using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class AutoPlayStats
{
    private static AutoPlayStats _instance;
    public static AutoPlayStats Instance => _instance;

    public struct CombatEvent
    {
        public int matchNumber;
        public int turnNumber;
        public string attackerTeam;
        public string attackerType;
        public Vector2Int attackerPos;
        public string defenderTeam;
        public string defenderType;
        public Vector2Int defenderPos;
        public int atkDie1;
        public int atkDie2;
        public int atkMod;
        public int atkTotal;
        public int defDie1;
        public int defDie2;
        public int defMod;
        public int defTotal;
        public string attackerAbility;
        public string defenderAbility;
        public bool attackerWon;
    }

    public struct TurnEvent
    {
        public int matchNumber;
        public int turnNumber;
        public string team;
        public string pieceType;
        public Vector2Int from;
        public Vector2Int to;
        public bool didAttack;
        public float duration;
    }

    public struct PowerUpEvent
    {
        public int matchNumber;
        public int turnNumber;
        public string type;
        public Vector2Int position;
        public string collectedByTeam;
        public string collectedByType;
        public string usedAgainstTeam;
        public string usedAgainstType;
        public string result;
    }

    public struct ObstacleEvent
    {
        public int matchNumber;
        public int turnNumber;
        public string type;
        public Vector2Int position;
        public string triggeredByTeam;
        public string triggeredByType;
        public string result;
    }

    public struct MatchResult
    {
        public int matchNumber;
        public string winner;
        public int totalTurns;
        public float durationSeconds;
        public int bluePiecesAlive;
        public int redPiecesAlive;
        public int totalCombats;
        public int powerUpsUsed;
        public int obstaclesTriggered;
        public int blueGoldEarned;
        public int redGoldEarned;
        public float avgFps;
        public float peakMemoryMB;
        public int errorCount;
    }

    public struct IAEvent
    {
        public int matchNumber;
        public int turnNumber;
        public string team;
        public int possibleMoves;
        public int candidatesFound;
        public bool couldNotMove;
    }

    public struct DiceDistribution
    {
        public int[] rollCounts;
        public int totalRolls;
    }

    public List<CombatEvent> combats = new();
    public List<TurnEvent> turns = new();
    public List<PowerUpEvent> powerUps = new();
    public List<ObstacleEvent> obstacles = new();
    public List<MatchResult> matches = new();
    public List<IAEvent> iaEvents = new();
    public List<string> errors = new();

    private int currentMatch;
    private float matchStartTime;
    private int matchCombatCount;
    private int matchPowerUpCount;
    private int matchObstacleCount;
    private float matchFpsAccum;
    private int matchFpsSamples;
    private float peakMemoryMB;
    private int matchErrorCount;

    private DiceDistribution blueDice = new() { rollCounts = new int[13] };
    private DiceDistribution redDice = new() { rollCounts = new int[13] };

    public int TotalMatches => matches.Count;
    public int BlueWins => matches.FindAll(m => m.winner == "Blue").Count;
    public int RedWins => matches.FindAll(m => m.winner == "Red").Count;
    public int TotalErrors => errors.Count;

    public void Init()
    {
        _instance = this;
        combats.Clear();
        turns.Clear();
        powerUps.Clear();
        obstacles.Clear();
        matches.Clear();
        iaEvents.Clear();
        errors.Clear();
        blueDice.rollCounts = new int[13];
        redDice.rollCounts = new int[13];
        blueDice.totalRolls = 0;
        redDice.totalRolls = 0;
        Application.logMessageReceived += OnLogMessage;
    }

    public void Deinit()
    {
        Application.logMessageReceived -= OnLogMessage;
    }

    void OnLogMessage(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            string msg = $"[{type}] {condition}";
            if (!string.IsNullOrEmpty(stackTrace))
                msg += $"\n  StackTrace: {stackTrace.Split('\n')[0]}";
            errors.Add(msg);
            matchErrorCount++;
        }
    }

    static int CurrentTurn()
    {
        TurnManager t = Object.FindAnyObjectByType<TurnManager>();
        return t != null ? t.turnNumber : 0;
    }

    public void StartMatch(int matchNum)
    {
        currentMatch = matchNum;
        matchStartTime = Time.realtimeSinceStartup;
        matchCombatCount = 0;
        matchPowerUpCount = 0;
        matchObstacleCount = 0;
        matchFpsAccum = 0;
        matchFpsSamples = 0;
        peakMemoryMB = 0;
        matchErrorCount = 0;
    }

    public void RecordFrame()
    {
        matchFpsAccum += 1f / Time.unscaledDeltaTime;
        matchFpsSamples++;
        float mem = System.GC.GetTotalMemory(false) / (1024f * 1024f);
        if (mem > peakMemoryMB) peakMemoryMB = mem;
    }

    public void EndMatch(string winner, int blueAlive, int redAlive, int totalTurns, int blueGold, int redGold)
    {
        matches.Add(new MatchResult
        {
            matchNumber = currentMatch,
            winner = winner,
            totalTurns = totalTurns,
            durationSeconds = Time.realtimeSinceStartup - matchStartTime,
            bluePiecesAlive = blueAlive,
            redPiecesAlive = redAlive,
            totalCombats = matchCombatCount,
            powerUpsUsed = matchPowerUpCount,
            obstaclesTriggered = matchObstacleCount,
            blueGoldEarned = blueGold,
            redGoldEarned = redGold,
            avgFps = matchFpsSamples > 0 ? matchFpsAccum / matchFpsSamples : 0,
            peakMemoryMB = peakMemoryMB,
            errorCount = matchErrorCount
        });
    }

    public void LogCombat(string attackerTeam, string attackerType, int atkFromRow, int atkFromCol,
        string defenderTeam, string defenderType, int defRow, int defCol,
        int atkD1, int atkD2, int atkMod, int atkTotal,
        int defD1, int defD2, int defMod, int defTotal,
        string atkAbility, string defAbility, bool attackerWon)
    {
        matchCombatCount++;

        if (attackerTeam == "Blue")
        {
            blueDice.rollCounts[atkD1 + atkD2]++;
            blueDice.totalRolls++;
        }
        else
        {
            redDice.rollCounts[atkD1 + atkD2]++;
            redDice.totalRolls++;
        }

        if (defenderTeam == "Blue")
        {
            blueDice.rollCounts[defD1 + defD2]++;
            blueDice.totalRolls++;
        }
        else
        {
            redDice.rollCounts[defD1 + defD2]++;
            redDice.totalRolls++;
        }

        combats.Add(new CombatEvent
        {
            matchNumber = currentMatch,
            turnNumber = CurrentTurn(),
            attackerTeam = attackerTeam,
            attackerType = attackerType,
            attackerPos = new Vector2Int(atkFromCol, atkFromRow),
            defenderTeam = defenderTeam,
            defenderType = defenderType,
            defenderPos = new Vector2Int(defCol, defRow),
            atkDie1 = atkD1,
            atkDie2 = atkD2,
            atkMod = atkMod,
            atkTotal = atkTotal,
            defDie1 = defD1,
            defDie2 = defD2,
            defMod = defMod,
            defTotal = defTotal,
            attackerAbility = atkAbility,
            defenderAbility = defAbility,
            attackerWon = attackerWon
        });
    }

    public void LogTurn(string team, string pieceType, int fromRow, int fromCol, int toRow, int toCol, bool didAttack, float duration)
    {
        turns.Add(new TurnEvent
        {
            matchNumber = currentMatch,
            turnNumber = CurrentTurn(),
            team = team,
            pieceType = pieceType,
            from = new Vector2Int(fromCol, fromRow),
            to = new Vector2Int(toCol, toRow),
            didAttack = didAttack,
            duration = duration
        });
    }

    public void LogPowerUp(string type, int row, int col, string collectedByTeam, string collectedByType, string usedAgainstTeam, string usedAgainstType, string result)
    {
        matchPowerUpCount++;
        powerUps.Add(new PowerUpEvent
        {
            matchNumber = currentMatch,
            turnNumber = CurrentTurn(),
            type = type,
            position = new Vector2Int(col, row),
            collectedByTeam = collectedByTeam,
            collectedByType = collectedByType,
            usedAgainstTeam = usedAgainstTeam,
            usedAgainstType = usedAgainstType,
            result = result
        });
    }

    public void LogObstacle(string type, int row, int col, string triggeredByTeam, string triggeredByType, string result)
    {
        matchObstacleCount++;
        obstacles.Add(new ObstacleEvent
        {
            matchNumber = currentMatch,
            turnNumber = CurrentTurn(),
            type = type,
            position = new Vector2Int(col, row),
            triggeredByTeam = triggeredByTeam,
            triggeredByType = triggeredByType,
            result = result
        });
    }

    public void LogIA(string team, int possibleMoves, int candidatesFound, bool couldNotMove)
    {
        iaEvents.Add(new IAEvent
        {
            matchNumber = currentMatch,
            turnNumber = CurrentTurn(),
            team = team,
            possibleMoves = possibleMoves,
            candidatesFound = candidatesFound,
            couldNotMove = couldNotMove
        });
    }

    public string GenerateReport()
    {
        StringBuilder sb = new();

        sb.AppendLine("========================================");
        sb.AppendLine("   AUTO-PLAY REPORT — Dice Clash Tactics");
        sb.AppendLine("========================================");
        sb.AppendLine($"Date: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Total matches: {TotalMatches}");
        sb.AppendLine();

        if (TotalMatches > 0)
        {
            float avgDuration = 0;
            int totalTurns = 0;
            int totalCombatsAll = 0;
            float avgFpsAll = 0;
            float peakMem = 0;
            foreach (var m in matches)
            {
                avgDuration += m.durationSeconds;
                totalTurns += m.totalTurns;
                totalCombatsAll += m.totalCombats;
                avgFpsAll += m.avgFps;
                if (m.peakMemoryMB > peakMem) peakMem = m.peakMemoryMB;
            }
            avgDuration /= TotalMatches;
            avgFpsAll /= TotalMatches;

            sb.AppendLine("--- SUMMARY ---");
            sb.AppendLine($"Blue wins: {BlueWins} ({(100f * BlueWins / TotalMatches):F1}%)");
            sb.AppendLine($"Red wins:  {RedWins} ({(100f * RedWins / TotalMatches):F1}%)");
            sb.AppendLine($"Avg duration: {avgDuration:F1}s");
            sb.AppendLine($"Avg turns per match: {(float)totalTurns / TotalMatches:F1}");
            sb.AppendLine($"Total combats: {totalCombatsAll}");
            sb.AppendLine($"Total power-ups used: {powerUps.Count}");
            sb.AppendLine($"Total obstacles triggered: {obstacles.Count}");
            sb.AppendLine($"Avg FPS: {avgFpsAll:F1}");
            sb.AppendLine($"Peak memory: {peakMem:F1} MB");
            sb.AppendLine($"Errors captured: {errors.Count}");
            sb.AppendLine();
        }

        sb.AppendLine("--- DICE DISTRIBUTION ---");
        sb.AppendLine("Roll | Blue | Red");
        for (int i = 2; i <= 12; i++)
        {
            sb.AppendLine($"  {i,2} | {blueDice.rollCounts[i],4} | {redDice.rollCounts[i],4}");
        }
        sb.AppendLine($"Total rolls: Blue={blueDice.totalRolls} Red={redDice.totalRolls}");
        sb.AppendLine();

        if (combats.Count > 0)
        {
            sb.AppendLine("--- COMBAT STATS ---");
            Dictionary<string, int> winnerByType = new();
            foreach (var c in combats)
            {
                string key = c.attackerWon ? $"{c.attackerTeam} {c.attackerType}" : $"{c.defenderTeam} {c.defenderType}";
                if (!winnerByType.ContainsKey(key)) winnerByType[key] = 0;
                winnerByType[key]++;
            }
            foreach (var kv in winnerByType)
                sb.AppendLine($"  {kv.Key}: {kv.Value} wins ({(100f * kv.Value / combats.Count):F1}%)");

            int highestRoll = 0;
            int lowestRoll = 13;
            foreach (var c in combats)
            {
                if (c.atkTotal > highestRoll) highestRoll = c.atkTotal;
                if (c.defTotal > highestRoll) highestRoll = c.defTotal;
                if (c.atkTotal < lowestRoll) lowestRoll = c.atkTotal;
                if (c.defTotal < lowestRoll) lowestRoll = c.defTotal;
            }
            sb.AppendLine($"Highest total: {highestRoll}");
            sb.AppendLine($"Lowest total: {lowestRoll}");

            int abilityCount = 0;
            foreach (var c in combats)
                if (!string.IsNullOrEmpty(c.attackerAbility) || !string.IsNullOrEmpty(c.defenderAbility))
                    abilityCount++;
            sb.AppendLine($"Combats with abilities: {abilityCount} ({(100f * abilityCount / combats.Count):F1}%)");
            sb.AppendLine();
        }

        if (iaEvents.Count > 0)
        {
            sb.AppendLine("--- IA STATS ---");
            int noMoveCount = 0;
            float avgCandidates = 0;
            foreach (var e in iaEvents)
            {
                if (e.couldNotMove) noMoveCount++;
                avgCandidates += e.candidatesFound;
            }
            sb.AppendLine($"IA turns with no moves: {noMoveCount} ({(100f * noMoveCount / iaEvents.Count):F1}%)");
            sb.AppendLine($"Avg candidates per turn: {(float)avgCandidates / iaEvents.Count:F1}");
            sb.AppendLine();
        }

        if (turns.Count > 0)
        {
            sb.AppendLine("--- TURN STATS ---");
            float avgTurnDuration = 0;
            foreach (var t in turns)
                avgTurnDuration += t.duration;
            sb.AppendLine($"Avg turn duration: {(float)avgTurnDuration / turns.Count:F3}s");
            int attackTurns = 0;
            foreach (var t in turns)
                if (t.didAttack) attackTurns++;
            sb.AppendLine($"Attack turns: {attackTurns} ({(100f * attackTurns / turns.Count):F1}%)");
            sb.AppendLine();
        }

        if (errors.Count > 0)
        {
            sb.AppendLine("--- ERRORS ---");
            HashSet<string> uniqueErrors = new();
            foreach (string e in errors)
            {
                string shortErr = e.Length > 120 ? e.Substring(0, 120) + "..." : e;
                uniqueErrors.Add(shortErr);
            }
            foreach (string e in uniqueErrors)
                sb.AppendLine($"  [{errors.FindAll(x => x.StartsWith(e.Substring(0, Mathf.Min(40, e.Length)))).Count}x] {e}");
            sb.AppendLine();
        }

        sb.AppendLine("--- MATCH DETAILS ---");
        foreach (var m in matches)
        {
            sb.AppendLine($"Match {m.matchNumber}: {m.winner} | {m.totalTurns} turns | {m.durationSeconds:F1}s | " +
                $"Blue:{m.bluePiecesAlive} Red:{m.redPiecesAlive} | Combats:{m.totalCombats} | " +
                $"PowerUps:{m.powerUpsUsed} | Obstacles:{m.obstaclesTriggered} | " +
                $"FPS:{m.avgFps:F0} | Mem:{m.peakMemoryMB:F0}MB | Errors:{m.errorCount}");
        }

        return sb.ToString();
    }

    public void WriteReport(string path)
    {
        string report = GenerateReport();
        File.WriteAllText(path, report);
        Debug.Log($"[AutoPlay] Report written to {path}");
    }
}
