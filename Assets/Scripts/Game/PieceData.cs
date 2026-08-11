using UnityEngine;

[System.Serializable]
public struct PieceData
{
    public int id;
    public Team team;
    public PieceType type;
    public string species;

    public int Tier => type switch
    {
        PieceType.Pawn => 1,
        PieceType.Ninja => 2,
        PieceType.Knight => 3,
        PieceType.Paladin => 4,
        _ => 1
    };

    public int defBonus => type switch
    {
        PieceType.Pawn => 0,
        PieceType.Ninja => 1,
        PieceType.Knight => 2,
        PieceType.Paladin => 3,
        _ => 0
    };

    public PieceData(int id, Team team, PieceType type, string species = "Human")
    {
        this.id = id;
        this.team = team;
        this.type = type;
        this.species = species;
    }

    public int PointValue => type switch
    {
        PieceType.Pawn => 0,
        PieceType.Ninja => 1,
        PieceType.Knight => 2,
        PieceType.Paladin => 3,
        _ => 0
    };
}
