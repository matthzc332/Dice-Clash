using System.Collections.Generic;

public static class TutorialInitialPosition
{
    public static IReadOnlyList<InitialPositionEntry> Entries => entries;

    private static readonly List<InitialPositionEntry> entries = new()
    {
        new() { row = 0, col = 2, team = Team.Blue, type = PieceType.Knight },
        new() { row = 0, col = 4, team = Team.Blue, type = PieceType.Knight },
        new() { row = 1, col = 2, team = Team.Blue, type = PieceType.Pawn },
        new() { row = 1, col = 4, team = Team.Blue, type = PieceType.Pawn },
        new() { row = 2, col = 2, team = Team.Blue, type = PieceType.Ninja },
        new() { row = 2, col = 4, team = Team.Blue, type = PieceType.Paladin },

        new() { row = 5, col = 1, team = Team.Red, type = PieceType.Pawn },
        new() { row = 5, col = 3, team = Team.Red, type = PieceType.Pawn },
        new() { row = 5, col = 5, team = Team.Red, type = PieceType.Pawn },
        new() { row = 6, col = 3, team = Team.Red, type = PieceType.Pawn },
    };
}
