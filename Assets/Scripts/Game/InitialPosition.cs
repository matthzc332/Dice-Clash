using System.Collections.Generic;

[System.Serializable]
public struct InitialPositionEntry
{
    public int row;
    public int col;
    public Team team;
    public PieceType type;
}

public static class InitialPosition
{
    public static IReadOnlyList<InitialPositionEntry> Entries => entries;

    private static readonly List<InitialPositionEntry> entries = new()
    {
        // Blue back rank (row 0)
        new() { row = 0, col = 0, team = Team.Blue, type = PieceType.Knight },
        new() { row = 0, col = 1, team = Team.Blue, type = PieceType.Ninja },
        new() { row = 0, col = 2, team = Team.Blue, type = PieceType.Paladin },
        new() { row = 0, col = 3, team = Team.Blue, type = PieceType.Paladin },
        new() { row = 0, col = 4, team = Team.Blue, type = PieceType.Paladin },
        new() { row = 0, col = 5, team = Team.Blue, type = PieceType.Ninja },
        new() { row = 0, col = 6, team = Team.Blue, type = PieceType.Knight },

        // Blue pawn line (row 1, cols 1-5; cols 0 and 6 empty)
        new() { row = 1, col = 1, team = Team.Blue, type = PieceType.Pawn },
        new() { row = 1, col = 2, team = Team.Blue, type = PieceType.Pawn },
        new() { row = 1, col = 3, team = Team.Blue, type = PieceType.Pawn },
        new() { row = 1, col = 4, team = Team.Blue, type = PieceType.Pawn },
        new() { row = 1, col = 5, team = Team.Blue, type = PieceType.Pawn },

        // Red pawn line (row 5, cols 1-5; cols 0 and 6 empty)
        new() { row = 5, col = 1, team = Team.Red, type = PieceType.Pawn },
        new() { row = 5, col = 2, team = Team.Red, type = PieceType.Pawn },
        new() { row = 5, col = 3, team = Team.Red, type = PieceType.Pawn },
        new() { row = 5, col = 4, team = Team.Red, type = PieceType.Pawn },
        new() { row = 5, col = 5, team = Team.Red, type = PieceType.Pawn },

        // Red back rank (row 6)
        new() { row = 6, col = 0, team = Team.Red, type = PieceType.Knight },
        new() { row = 6, col = 1, team = Team.Red, type = PieceType.Ninja },
        new() { row = 6, col = 2, team = Team.Red, type = PieceType.Paladin },
        new() { row = 6, col = 3, team = Team.Red, type = PieceType.Paladin },
        new() { row = 6, col = 4, team = Team.Red, type = PieceType.Paladin },
        new() { row = 6, col = 5, team = Team.Red, type = PieceType.Ninja },
        new() { row = 6, col = 6, team = Team.Red, type = PieceType.Knight },
    };
}
