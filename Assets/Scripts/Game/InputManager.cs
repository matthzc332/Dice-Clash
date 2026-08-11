using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [Header("References")]
    public BoardManager board;
    public TurnManager turnManager;

    [Header("Visuals")]
    public GameObject validMoveMarkerPrefab;

    [Header("Colors")]
    public Color validMoveColor = new Color(0.2f, 1f, 0.2f, 0.5f);
    public bool aiPlaying;

    public Cell selectedCell;
    private List<Cell> validMoveCells = new();
    private List<GameObject> moveMarkers = new();
    private GameObject moveMarkerContainer;
    private Cell lastHoveredCell;

    void Awake()
    {
        if (board == null)
            board = FindFirstObjectByType<BoardManager>();
        if (turnManager == null)
            turnManager = FindFirstObjectByType<TurnManager>();
        moveMarkerContainer = new GameObject("MoveMarkers");
    }

    void OnEnable()
    {
        if (turnManager != null)
            turnManager.OnTurnChanged += OnTurnChanged;
    }

    void OnDisable()
    {
        if (turnManager != null)
            turnManager.OnTurnChanged -= OnTurnChanged;
    }

    void OnTurnChanged(TurnState state)
    {
        DeselectCurrent();
    }

    void Update()
    {
        if (aiPlaying) return;
        if (PowerUpManager.Instance != null && PowerUpManager.Instance.IsExecuting) return;

        Camera cam = FindFirstObjectByType<Camera>();
        if (cam == null) return;

        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            var cellPos = board != null ? board.WorldToCell(mousePos) : null;
            Cell clickedCell = cellPos.HasValue ? board.GetCell(cellPos.Value.row, cellPos.Value.col) : null;
            HandleClick(clickedCell);
        }

        UpdateHover(mousePos);
    }

    void HandleClick(Cell clickedCell)
    {
        if (clickedCell == null)
        {
            DeselectCurrent();
            return;
        }

        bool isOwnPiece = clickedCell.IsOccupied &&
                          clickedCell.pieceData?.team == turnManager.GetCurrentTeam();

        if (isOwnPiece)
        {
            SelectPiece(clickedCell);
            return;
        }

        if (selectedCell != null && validMoveCells.Contains(clickedCell))
        {
            ExecuteMove(clickedCell);
            return;
        }

        DeselectCurrent();
    }

    void ExecuteMove(Cell destination)
    {
        Cell source = selectedCell;
        PieceData? ownPiece = source.pieceData;
        PieceData? enemyPiece = destination.IsOccupied ? destination.pieceData : null;
        DeselectCurrent();

        if (ownPiece.HasValue && enemyPiece.HasValue)
            CharacterCardUI.Instance.ShowWithTarget(ownPiece.Value, enemyPiece.Value);

        board.MovePiece(source.row, source.col, destination.row, destination.col);
    }

    void SelectPiece(Cell cell)
    {
        if (selectedCell != null)
        {
            board.ResetToIdle(selectedCell);
            board.ClearSelectedCell();
        }

        selectedCell = cell;
        board.SetSelectedCell(cell.row, cell.col);
        board.SetAttackPose(cell);
        string species = board.speciesTheme;
        SoundManager.Instance.PlaySelectSound(species);

        if (cell.pieceData.HasValue)
        {
            CharacterCardUI.Instance.Show(cell.pieceData.Value);
        }

        ObstacleManager om = FindFirstObjectByType<ObstacleManager>();
        bool isGlued = om != null && cell.pieceData.HasValue && om.IsPieceGlued(cell.pieceData.Value.id);

        var validCells = board.GetValidMoves(cell.row, cell.col, cell.pieceData?.type ?? PieceType.Pawn);

        if (isGlued)
        {
            List<Cell> attackOnly = new();
            foreach (var c in validCells)
            {
                if (c.IsOccupied && c.pieceData.HasValue && c.pieceData.Value.team != cell.pieceData.Value.team)
                    attackOnly.Add(c);
            }
            validCells = attackOnly;
        }

        ShowValidMoves(validCells);
    }

    void DeselectCurrent()
    {
        if (selectedCell != null)
        {
            board.ResetToIdle(selectedCell);
            board.ClearSelectedCell();
            selectedCell = null;
        }
        ClearMoveMarkers();
        CharacterCardUI.Instance.ClearUI();
    }

    void UpdateHover(Vector2 mousePos)
    {
        var cellPos = board != null ? board.WorldToCell(mousePos) : null;
        Cell hoveredCell = cellPos.HasValue ? board.GetCell(cellPos.Value.row, cellPos.Value.col) : null;

        if (hoveredCell == lastHoveredCell) return;
        ResetPieceColor(lastHoveredCell);
        lastHoveredCell = hoveredCell;

        if (hoveredCell != null && hoveredCell.IsOccupied && hoveredCell.pieceVisual != null)
        {
            SpriteRenderer sr = hoveredCell.pieceVisual.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                PieceData data = hoveredCell.pieceData.Value;
                bool isOwn = data.team == turnManager.GetCurrentTeam();
                if (isOwn)
                {
                    sr.color = new Color(1f, 1f, 0.8f, 1f);
                }
                else if (board.isShadowPhase && data.team == Team.Red)
                {
                    sr.color = new Color(0.17f, 0.17f, 0.22f, 1f);
                }
                else if (selectedCell != null && validMoveCells.Contains(hoveredCell))
                {
                    sr.color = new Color(1f, 0.5f, 0.5f, 1f);
                }
            }
        }
    }

    void ResetPieceColor(Cell cell)
    {
        if (cell == null || !cell.IsOccupied || cell.pieceVisual == null) return;
        SpriteRenderer sr = cell.pieceVisual.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (board.isShadowPhase && cell.pieceData.Value.team == Team.Red)
                sr.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            else if (cell.pieceData.Value.team == Team.Red)
                sr.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            else
                sr.color = Color.white;
        }
    }

    void ShowValidMoves(List<Cell> cells)
    {
        ClearMoveMarkers();
        validMoveCells = cells;

        foreach (var cell in cells)
        {
            Vector3 pos = board.CellToWorld(cell.row, cell.col);
            GameObject marker;
            if (validMoveMarkerPrefab != null)
                marker = Instantiate(validMoveMarkerPrefab, pos, Quaternion.identity);
            else
                marker = CreateMarkerPlaceholder(pos);
            marker.transform.SetParent(moveMarkerContainer.transform);
            moveMarkers.Add(marker);
        }
    }

    void ClearMoveMarkers()
    {
        foreach (var marker in moveMarkers)
            Destroy(marker);
        moveMarkers.Clear();
        validMoveCells.Clear();
    }

    GameObject CreateMarkerPlaceholder(Vector3 position)
    {
        GameObject obj = new GameObject("MoveMarker");
        obj.transform.position = position;
        obj.transform.localScale = Vector3.one * 1.05f;

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        Texture2D tex = new Texture2D(16, 16);
        Color border = new Color(0.1f, 0.9f, 0.1f, 0.7f);
        Color clear = Color.clear;
        Color[] pixels = new Color[256];
        for (int i = 0; i < pixels.Length; i++)
        {
            int x = i % 16;
            int y = i / 16;
            if (x < 1 || x >= 15 || y < 1 || y >= 15)
                pixels[i] = border;
            else if (x < 2 || x >= 14 || y < 2 || y >= 14)
                pixels[i] = new Color(0.1f, 0.9f, 0.1f, 0.3f);
            else
                pixels[i] = clear;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
        sr.sortingOrder = 3;

        return obj;
    }
}
