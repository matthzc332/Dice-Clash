using UnityEngine;

public enum CombatResult
{
    AttackerWins,
    DefenderWins
}

public struct CombatOutcome
{
    public CombatResult result;
    public int atkTotal;
    public int defTotal;
    public int atkBonus;
    public int defBonus;
    public string atkAbilityName;
    public string defAbilityName;
    public int atkDie1;
    public int atkDie2;
    public int defDie1;
    public int defDie2;
}

public static class CombatManager
{
    static readonly (int dr, int dc)[] AllDirs = {
        (0, 1), (0, -1), (1, 0), (-1, 0),
        (1, 1), (1, -1), (-1, 1), (-1, -1)
    };

    public static CombatOutcome Resolve(
        PieceData attacker, PieceData defender,
        int atkFromRow, int atkFromCol,
        Cell defCell, BoardManager board)
    {
        int atkD1 = Random.Range(1, 7);
        int atkD2 = Random.Range(1, 7);
        int defD1 = Random.Range(1, 7);
        int defD2 = Random.Range(1, 7);

        int atkRoll = atkD1 + atkD2;
        int defRoll = defD1 + defD2;

        GetAbilityModifiers(attacker, defender, atkFromRow, atkFromCol, defCell.row, defCell.col, defCell, board,
            out int atkBonus, out int defBonus,
            out string atkAbility, out string defAbility);

        int atkTotal = atkRoll + 1 + atkBonus;
        int defTotal = defRoll + defender.defBonus + defBonus;

        bool attackerWins = atkTotal > defTotal;
        if (GameConfig.isTutorial && defender.team == Team.Red && (board == null || !board.isShadowPhase))
            attackerWins = true;
        CombatResult combatResult = attackerWins ? CombatResult.AttackerWins : CombatResult.DefenderWins;

        Debug.Log($"[Combat] Attacker={attacker.team} {attacker.type} dice={atkD1}+{atkD2}={atkRoll} +1 +{atkBonus} total={atkTotal}");
        Debug.Log($"[Combat] Defender={defender.team} {defender.type} dice={defD1}+{defD2}={defRoll} +{defender.defBonus} +{defBonus} total={defTotal}");

        return new CombatOutcome
        {
            result = combatResult,
            atkTotal = atkTotal,
            defTotal = defTotal,
            atkBonus = atkBonus,
            defBonus = defBonus,
            atkAbilityName = atkAbility,
            defAbilityName = defAbility,
            atkDie1 = atkD1,
            atkDie2 = atkD2,
            defDie1 = defD1,
            defDie2 = defD2
        };
    }

    public static void GetAbilityModifiers(
        PieceData attacker, PieceData defender,
        int atkFromRow, int atkFromCol,
        int defRow, int defCol, Cell defCell, BoardManager board,
        out int atkBonus, out int defBonus,
        out string atkAbility, out string defAbility)
    {
        atkBonus = 0;
        defBonus = 0;
        atkAbility = null;
        defAbility = null;

        if (attacker.type == PieceType.Ninja && IsEnemyIsolated(defender, defCell, board))
        {
            atkBonus += 6;
            atkAbility = "Flank";
        }

        if (attacker.type == PieceType.Knight && IsCharging(atkFromRow, atkFromCol, defRow, defCol))
        {
            atkBonus += 1;
            atkAbility = atkAbility == null ? "Charge" : atkAbility + " + Charge";
        }

        if (HasPaladinAura(defender, defCell, board))
        {
            defBonus += 1;
            defAbility = "Aura";
        }
    }

    static bool IsEnemyIsolated(PieceData defender, Cell defCell, BoardManager board)
    {
        foreach (var dir in AllDirs)
        {
            Cell adj = board.GetCell(defCell.row + dir.dr, defCell.col + dir.dc);
            if (adj != null && adj.IsOccupied && adj.pieceData?.team == defender.team)
                return false;
        }
        return true;
    }

    static bool IsCharging(int fromRow, int fromCol, int toRow, int toCol)
    {
        return Mathf.Abs(fromRow - toRow) + Mathf.Abs(fromCol - toCol) >= 2;
    }

    static bool HasPaladinAura(PieceData defender, Cell defCell, BoardManager board)
    {
        foreach (var dir in AllDirs)
        {
            Cell adj = board.GetCell(defCell.row + dir.dr, defCell.col + dir.dc);
            if (adj != null && adj.IsOccupied &&
                adj.pieceData?.type == PieceType.Paladin &&
                adj.pieceData?.team == defender.team)
                return true;
        }
        return false;
    }
}
