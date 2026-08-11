using UnityEngine;

public enum ObstacleType
{
    None,
    Tree,
    DestroyedCell,
    Glue,
    Mine,
    Hole
}

public class Cell
{
    public int row;
    public int col;
    public PieceData? pieceData;
    public GameObject pieceVisual;
    public bool isObstacle;
    public ObstacleType obstacleType;
    public int destroyTimer;
    public int maxDestroyTimer;
    public bool isDestroyed;
    public bool hasGlue;
    public int gluedPieceId = -1;
    public bool hasMine;
    public GameObject obstacleVisual;
    public GameObject timerBar;

    public bool IsOccupied => pieceVisual != null || (isObstacle && !isDestroyed);

    public bool BlocksMovement => isObstacle && !isDestroyed && obstacleType != ObstacleType.Glue && obstacleType != ObstacleType.Mine && obstacleType != ObstacleType.DestroyedCell;

    public void SetPiece(PieceData data, GameObject visual)
    {
        pieceData = data;
        pieceVisual = visual;
    }

    public void ClearPiece()
    {
        pieceData = null;
        pieceVisual = null;
    }

    public void ClearObstacle()
    {
        isObstacle = false;
        obstacleType = ObstacleType.None;
        destroyTimer = 0;
        maxDestroyTimer = 0;
        isDestroyed = false;
        hasGlue = false;
        gluedPieceId = -1;
        hasMine = false;
        if (obstacleVisual != null)
        {
            Object.Destroy(obstacleVisual);
            obstacleVisual = null;
        }
        if (timerBar != null)
        {
            Object.Destroy(timerBar);
            timerBar = null;
        }
    }
}
