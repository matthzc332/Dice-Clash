using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    public static ObstacleManager Instance { get; private set; }

    private BoardManager board;
    private TurnManager turnManager;
    private List<Cell> destroyedCellCandidates = new();
    private List<Cell> glueCandidates = new();
    private List<Cell> mineCandidates = new();
    private Dictionary<int, int> gluedPieceTurns = new();
    private Dictionary<Cell, GameObject> floorOverlays = new();

    void Awake()
    {
        Instance = this;
    }

    public void Initialize(BoardManager boardRef, TurnManager turnRef)
    {
        board = boardRef;
        turnManager = turnRef;
        if (turnManager != null)
            turnManager.OnTurnChanged += OnTurnChanged;
    }

    public void SubscribeTurnManager(TurnManager tm)
    {
        if (tm == null) return;
        if (turnManager != null)
            turnManager.OnTurnChanged -= OnTurnChanged;
        turnManager = tm;
        turnManager.OnTurnChanged += OnTurnChanged;
    }

    void OnDestroy()
    {
        if (turnManager != null)
            turnManager.OnTurnChanged -= OnTurnChanged;
    }

    public void SpawnCampaignObstacles(string[] obstacleTypes)
    {
        ClearAll();
        if (obstacleTypes == null || obstacleTypes.Length == 0) return;

        List<(int r, int c)> emptyCells = new();
        for (int r = 0; r < board.rows; r++)
            for (int c = 0; c < board.cols; c++)
            {
                Cell cell = board.GetCell(r, c);
                if (cell != null && !cell.IsOccupied && !cell.isObstacle)
                    emptyCells.Add((r, c));
            }

        if (emptyCells.Count == 0) return;

        Shuffle(emptyCells);

        int idx = 0;
        int destroyedCount = 0;
        int glueCount = 0;
        int mineCount = 0;
        int rocaCount = 0;

        int maxDestroyed = 2;
        int maxGlue = 2;
        int maxMine = 2;
        int maxRoca = 3;

        foreach (string type in obstacleTypes)
        {
            if (idx >= emptyCells.Count) break;

            switch (type)
            {
                case "DestroyedCell":
                    if (destroyedCount >= maxDestroyed) continue;
                    SpawnDestroyedCell(emptyCells[idx].r, emptyCells[idx].c);
                    destroyedCount++;
                    idx++;
                    break;
                case "Glue":
                    if (glueCount >= maxGlue) continue;
                    SpawnGlue(emptyCells[idx].r, emptyCells[idx].c);
                    glueCount++;
                    idx++;
                    break;
                case "Mine":
                    if (mineCount >= maxMine) continue;
                    SpawnMine(emptyCells[idx].r, emptyCells[idx].c);
                    mineCount++;
                    idx++;
                    break;
                case "Roca":
                    if (rocaCount >= maxRoca) continue;
                    SpawnRoca(emptyCells[idx].r, emptyCells[idx].c);
                    rocaCount++;
                    idx++;
                    break;
            }
        }
    }

    public void SpawnAutoPlayObstacles()
    {
        SpawnCampaignObstacles(new[] { "DestroyedCell", "Glue", "Mine", "Glue", "Mine" });
    }

    void SpawnDestroyedCell(int row, int col)
    {
        Cell cell = board.GetCell(row, col);
        if (cell == null) return;

        cell.isObstacle = true;
        cell.obstacleType = ObstacleType.DestroyedCell;
        cell.maxDestroyTimer = 4;
        cell.destroyTimer = cell.maxDestroyTimer;
        cell.isDestroyed = false;

        cell.obstacleVisual = null;

        Vector3 pos = board.CellToWorld(row, col);
        Sprite tileSprite = board.GetTileSprite(row, col);
        GameObject floorTile = new GameObject("FloorShake");
        floorTile.transform.SetParent(board.transform);
        floorTile.transform.position = pos;
        SpriteRenderer oSr = floorTile.AddComponent<SpriteRenderer>();
        oSr.sprite = tileSprite;
        oSr.sortingOrder = -2;
        if (tileSprite != null)
        {
            floorTile.transform.localScale = Vector3.one * board.tileScale * 0.9f;
        }
        floorOverlays[cell] = floorTile;
        StartCoroutine(ShakeFloorTile(floorTile, 0.004f));

        GameObject bar = CreateTimerBar(row, col, cell.maxDestroyTimer);
        cell.timerBar = bar;

        destroyedCellCandidates.Add(cell);
    }

    IEnumerator ShakeFloorTile(GameObject go, float intensity)
    {
        Vector3 basePos = go.transform.position;
        while (go != null)
        {
            go.transform.position = basePos + (Vector3)(Random.insideUnitCircle * intensity);
            yield return new WaitForSeconds(0.05f);
        }
    }

    void SpawnGlue(int row, int col)
    {
        Cell cell = board.GetCell(row, col);
        if (cell == null) return;

        cell.isObstacle = true;
        cell.obstacleType = ObstacleType.Glue;
        cell.hasGlue = true;

        GameObject visual = CreateGlueVisual(row, col);
        cell.obstacleVisual = visual;

        glueCandidates.Add(cell);
    }

    void SpawnMine(int row, int col)
    {
        Cell cell = board.GetCell(row, col);
        if (cell == null) return;

        cell.isObstacle = true;
        cell.obstacleType = ObstacleType.Mine;
        cell.hasMine = true;

        GameObject visual = CreateMineVisual(row, col);
        cell.obstacleVisual = visual;

        mineCandidates.Add(cell);
    }

    void SpawnRoca(int row, int col)
    {
        Cell cell = board.GetCell(row, col);
        if (cell == null) return;

        cell.isObstacle = true;
        cell.obstacleType = ObstacleType.Tree;

        Sprite obstacleSprite = Resources.Load<Sprite>("Sprites/Beastfolk/Decor/Arbol");
        if (obstacleSprite == null)
            obstacleSprite = CreateSquareSprite(0.5f, new Color(0.4f, 0.3f, 0.2f));

        Vector3 pos = board.CellToWorld(row, col);
        GameObject visual = new GameObject("Roca");
        visual.transform.SetParent(board.transform);
        visual.transform.position = pos;
        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = obstacleSprite;
        sr.sortingOrder = -1;
        visual.transform.localScale = new Vector3(0.10f, 0.14f, 1f);
        cell.obstacleVisual = visual;
    }

    Sprite CreateSquareSprite(float size, Color color)
    {
        Texture2D tex = new Texture2D(4, 4);
        Color[] pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 4);
    }

    void OnTurnChanged(TurnState newState)
    {
        if (board == null) return;

        for (int i = destroyedCellCandidates.Count - 1; i >= 0; i--)
        {
            Cell cell = destroyedCellCandidates[i];
            if (cell == null || cell.obstacleType == ObstacleType.Hole)
            {
                destroyedCellCandidates.RemoveAt(i);
                continue;
            }

            cell.destroyTimer--;
            UpdateTimerBar(cell);

            if (cell.destroyTimer <= 0)
                StartCoroutine(CollapseDestroyedCell(cell));
        }

        List<int> toRemove = new();
        foreach (var kvp in new Dictionary<int, int>(gluedPieceTurns))
        {
            gluedPieceTurns[kvp.Key] = kvp.Value - 1;
            if (gluedPieceTurns[kvp.Key] <= 0)
                toRemove.Add(kvp.Key);
        }
        foreach (int id in toRemove)
            gluedPieceTurns.Remove(id);
    }

    IEnumerator CollapseDestroyedCell(Cell cell)
    {
        if (cell == null || cell.obstacleType != ObstacleType.DestroyedCell) yield break;

        if (AutoPlayStats.Instance != null)
        {
            string team = cell.pieceData?.team.ToString() ?? "";
            string type = cell.pieceData?.type.ToString() ?? "";
            AutoPlayStats.Instance.LogObstacle("DestroyedCell", cell.row, cell.col, team, type, "collapse");
        }

        Vector3 pos = board.CellToWorld(cell.row, cell.col);

        SoundManager.Instance.PlayRockBreak();
        StartCoroutine(CameraShake(0.15f, 0.3f));

        for (int i = 0; i < 8; i++)
        {
            GameObject chunk = new GameObject("Chunk");
            chunk.transform.position = pos + (Vector3)Random.insideUnitCircle * 0.1f;
            SpriteRenderer sr = chunk.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite(0.5f, new Color(0.5f, 0.4f, 0.3f, 0.9f));
            sr.sortingOrder = 8;
            Vector3 dir = new Vector3(Random.Range(-1f, 1f), Random.Range(0.5f, 2f), 0);
            StartCoroutine(FlyFade(chunk, dir, Random.Range(0.3f, 0.6f)));
        }

        StartCoroutine(SmokeBurst(pos, 5));

        if (cell.pieceVisual != null)
        {
            Cell from = cell;
            PieceData pieceData = from.pieceData.Value;
            GameObject pieceVisual = from.pieceVisual;

            StartCoroutine(ExplosionDestroy(pieceVisual));
            board.OnKillEffect(pos, pieceData.team, pieceData.type, true);
            if (CoinManager.Instance != null)
                CoinManager.Instance.AwardKill(pieceData.team, pos, pieceData.type, true);
            from.ClearPiece();
        }

        if (cell.obstacleVisual != null)
        {
            Destroy(cell.obstacleVisual);
            cell.obstacleVisual = null;
        }
        if (floorOverlays.TryGetValue(cell, out GameObject floorGo))
        {
            if (floorGo != null) Destroy(floorGo);
            floorOverlays.Remove(cell);
        }
        if (cell.timerBar != null)
        {
            Destroy(cell.timerBar);
            cell.timerBar = null;
        }

        Sprite pisoRoto = null;
        Sprite[] pisoRotoSprites = Resources.LoadAll<Sprite>("Sprites/Menu/pisoRoto");
        if (pisoRotoSprites.Length > 0) pisoRoto = pisoRotoSprites[0];
        if (pisoRoto != null && board != null)
            board.ReplaceTileSprite(cell.row, cell.col, pisoRoto);

        cell.obstacleType = ObstacleType.Hole;

        yield return new WaitForSeconds(0.1f);
        if (board != null)
            yield return board.CheckVictoryAndEndTurn();
    }

    public bool IsPieceGlued(int pieceId)
    {
        return gluedPieceTurns.ContainsKey(pieceId);
    }

    public void TryTriggerGlue(int pieceId, Cell cell)
    {
        if (cell == null || cell.obstacleType != ObstacleType.Glue || !cell.hasGlue) return;
        gluedPieceTurns[pieceId] = 2;
        SoundManager.Instance.PlaySlime();

        if (AutoPlayStats.Instance != null)
        {
            Cell pieceCell = FindCellByPieceId(pieceId);
            string team = pieceCell?.pieceData?.team.ToString() ?? "";
            string type = pieceCell?.pieceData?.type.ToString() ?? "";
            AutoPlayStats.Instance.LogObstacle("Glue", cell.row, cell.col, team, type, "trapped");
        }

        StartCoroutine(GlueVisualFeedback(cell));
    }

    IEnumerator GlueVisualFeedback(Cell cell)
    {
        if (cell.obstacleVisual != null)
        {
            SpriteRenderer sr = cell.obstacleVisual.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                sr.color = new Color(c.r, c.g, c.b, 0.3f);
                yield return new WaitForSeconds(0.3f);
                if (sr != null)
                    sr.color = new Color(c.r, c.g, c.b, 0.6f);
            }
        }
    }

    public void TryTriggerMine(int pieceRow, int pieceCol, PieceData pieceData, GameObject pieceVisual)
    {
        Cell cell = board.GetCell(pieceRow, pieceCol);
        if (cell == null || cell.obstacleType != ObstacleType.Mine || !cell.hasMine) return;

        if (AutoPlayStats.Instance != null)
        {
            AutoPlayStats.Instance.LogObstacle("Mine", pieceRow, pieceCol,
                pieceData.team.ToString(), pieceData.type.ToString(), "explosion");
        }

        StartCoroutine(ExecuteMineExplosion(pieceRow, pieceCol, pieceData, pieceVisual));
    }

    IEnumerator ExecuteMineExplosion(int centerRow, int centerCol, PieceData triggerPiece, GameObject triggerVisual)
    {
        Cell centerCell = board.GetCell(centerRow, centerCol);
        if (centerCell == null) yield break;

        centerCell.hasMine = false;
        Vector3 centerPos = board.CellToWorld(centerRow, centerCol);

        SoundManager.Instance.PlayExplosion();
        SoundManager.Instance.PlayHit();
        StartCoroutine(CameraShake(0.25f, 0.5f));

        StartCoroutine(LightingFlash(centerPos));
        StartCoroutine(SmokeBurst(centerPos, 10));
        StartCoroutine(DirtChunks(centerPos, 12));

        GameObject aoe = new GameObject("MineAOE");
        aoe.transform.position = centerPos;
        SpriteRenderer aoeSr = aoe.AddComponent<SpriteRenderer>();
        aoeSr.sprite = CreateCircleSprite();
        aoeSr.color = new Color(1f, 0.3f, 0f, 0.3f);
        aoeSr.sortingOrder = -1;

        float anim = 0;
        while (anim < 0.3f)
        {
            if (aoe == null) yield break;
            aoe.transform.localScale = Vector3.one * (anim / 0.3f * 3f);
            anim += Time.deltaTime;
            yield return null;
        }
        if (aoe != null) Destroy(aoe);

        yield return new WaitForSeconds(0.1f);

        List<(Cell cell, PieceData data, GameObject visual, Vector3 pos)> toKill = new();

        for (int r = -1; r <= 1; r++)
        {
            for (int c = -1; c <= 1; c++)
            {
                int nr = centerRow + r;
                int nc = centerCol + c;
                Cell cell = board.GetCell(nr, nc);
                if (cell == null || !cell.IsOccupied || !cell.pieceData.HasValue) continue;

                Vector3 killPos = board.CellToWorld(nr, nc);
                toKill.Add((cell, cell.pieceData.Value, cell.pieceVisual, killPos));
            }
        }

        foreach (var entry in toKill)
        {
            if (entry.visual == null) continue;
            board.OnKillEffect(entry.pos, entry.data.team, entry.data.type, true);
            StartCoroutine(ExplosionDestroy(entry.visual));
            if (CoinManager.Instance != null)
                CoinManager.Instance.AwardKill(triggerPiece.team, entry.pos, entry.data.type, true);
            entry.cell.ClearPiece();
        }

        if (centerCell.obstacleVisual != null)
        {
            Destroy(centerCell.obstacleVisual);
            centerCell.obstacleVisual = null;
        }
        centerCell.isObstacle = false;
        centerCell.obstacleType = ObstacleType.None;
        mineCandidates.Remove(centerCell);

        yield return new WaitForSeconds(0.2f);
        if (board != null)
            yield return board.CheckVictoryAndEndTurn();
    }

    public void ClearAll()
    {
        if (turnManager != null)
            turnManager.OnTurnChanged -= OnTurnChanged;

        destroyedCellCandidates.Clear();
        glueCandidates.Clear();
        mineCandidates.Clear();
        gluedPieceTurns.Clear();

        foreach (var kvp in floorOverlays)
            if (kvp.Value != null) Destroy(kvp.Value);
        floorOverlays.Clear();

        for (int r = 0; r < board.rows; r++)
            for (int c = 0; c < board.cols; c++)
            {
                Cell cell = board.GetCell(r, c);
                if (cell != null && cell.obstacleType != ObstacleType.None)
                    cell.ClearObstacle();
            }

        if (turnManager != null)
            turnManager.OnTurnChanged += OnTurnChanged;
    }

    GameObject CreateDestroyedCellVisual(int row, int col)
    {
        return null;
    }

    GameObject CreateTimerBar(int row, int col, int maxTurns)
    {
        Vector3 pos = board.CellToWorld(row, col);
        GameObject barBg = new GameObject("TimerBarBg");
        barBg.transform.SetParent(board.transform);
        barBg.transform.position = pos + Vector3.down * 0.45f;
        SpriteRenderer bgSr = barBg.AddComponent<SpriteRenderer>();
        bgSr.sprite = CreateSquareSprite(1f, new Color(0.2f, 0.2f, 0.2f, 0.8f));
        bgSr.sortingOrder = 5;
        barBg.transform.localScale = new Vector3(0.8f, 0.12f, 1f);

        GameObject barFill = new GameObject("TimerBarFill");
        barFill.transform.SetParent(barBg.transform);
        barFill.transform.localPosition = Vector3.zero;
        SpriteRenderer fillSr = barFill.AddComponent<SpriteRenderer>();
        fillSr.sprite = CreateSquareSprite(1f, Color.green);
        fillSr.sortingOrder = 6;
        barFill.transform.localScale = Vector3.one;

        return barBg;
    }

    void UpdateTimerBar(Cell cell)
    {
        if (cell.timerBar == null) return;
        Transform fill = cell.timerBar.transform.Find("TimerBarFill");
        if (fill == null) return;

        float ratio = (float)cell.destroyTimer / cell.maxDestroyTimer;
        fill.localScale = new Vector3(ratio, 1f, 1f);

        SpriteRenderer sr = fill.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (ratio > 0.5f)
                sr.color = Color.green;
            else if (ratio > 0.25f)
                sr.color = Color.yellow;
            else
                sr.color = Color.red;
        }
    }

    GameObject CreateGlueVisual(int row, int col)
    {
        Vector3 pos = board.CellToWorld(row, col);
        GameObject go = new GameObject("Glue");
        go.transform.SetParent(board.transform);
        go.transform.position = pos;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        Sprite slimeSprite = null;
        Sprite[] slimeSprites = Resources.LoadAll<Sprite>("Sprites/Menu/slime");
        if (slimeSprites.Length > 0) slimeSprite = slimeSprites[0];
        sr.sprite = slimeSprite != null ? slimeSprite : CreateGlueSprite();
        sr.sortingOrder = -1;
        go.transform.localScale = new Vector3(2.5f, 2.1f, 1f);

        StartCoroutine(PulseObstacle(go, 0.85f, 1.0f, 2f));
        return go;
    }

    GameObject CreateMineVisual(int row, int col)
    {
        Vector3 pos = board.CellToWorld(row, col);
        GameObject go = new GameObject("Mine");
        go.transform.SetParent(board.transform);
        go.transform.position = pos;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateMineSprite();
        sr.sortingOrder = -1;
        go.transform.localScale = Vector3.one * 0.7f;

        StartCoroutine(PulseObstacle(go, 0.9f, 1.05f, 1f));
        return go;
    }

    GameObject CreateHoleVisual(int row, int col)
    {
        Vector3 pos = board.CellToWorld(row, col);
        GameObject go = new GameObject("Hole");
        go.transform.SetParent(board.transform);
        go.transform.position = pos;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateHoleSprite();
        sr.sortingOrder = -1;
        go.transform.localScale = Vector3.one * 0.85f;
        return go;
    }

    Sprite CreateCrackedStoneSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        Color stone = new Color(0.55f, 0.5f, 0.45f);
        Color crack = new Color(0.3f, 0.25f, 0.2f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int i = y * size + x;
                float nx = (float)x / size;
                float ny = (float)y / size;
                float edge = Mathf.Min(nx, 1f - nx, ny, 1f - ny);
                if (edge < 0.08f)
                {
                    pixels[i] = Color.clear;
                    continue;
                }

                bool isCrack = false;
                float cx = nx - 0.5f;
                float cy = ny - 0.5f;
                if (Mathf.Abs(cy - cx * 0.7f) < 0.04f && Mathf.Abs(cx) < 0.35f) isCrack = true;
                if (Mathf.Abs(cy + cx * 0.5f) < 0.035f && Mathf.Abs(cx) < 0.3f) isCrack = true;
                if (Mathf.Abs(cx) < 0.025f && Mathf.Abs(cy) < 0.35f) isCrack = true;

                pixels[i] = isCrack ? crack : stone;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    Sprite CreateHoleSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        Color hole = new Color(0.12f, 0.1f, 0.08f);
        Color rim = new Color(0.25f, 0.2f, 0.15f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int i = y * size + x;
                float nx = (float)x / size;
                float ny = (float)y / size;
                float dx = nx - 0.5f;
                float dy = ny - 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist > 0.45f)
                {
                    pixels[i] = Color.clear;
                }
                else if (dist > 0.35f)
                {
                    pixels[i] = rim;
                }
                else
                {
                    pixels[i] = hole;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    Sprite CreateGlueSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        Color glue = new Color(0.85f, 0.75f, 0.2f, 0.7f);
        Color glueDark = new Color(0.7f, 0.6f, 0.15f, 0.8f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int i = y * size + x;
                float nx = (float)x / size - 0.5f;
                float ny = (float)y / size - 0.5f;
                float dist = Mathf.Sqrt(nx * nx + ny * ny);

                if (dist > 0.42f)
                {
                    pixels[i] = Color.clear;
                    continue;
                }

                float noise = Mathf.Sin(nx * 12f) * Mathf.Cos(ny * 10f) * 0.15f;
                bool isBlob = dist + noise < 0.38f;
                pixels[i] = isBlob ? (dist < 0.2f ? glueDark : glue) : Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    Sprite CreateMineSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        Color body = new Color(0.3f, 0.3f, 0.3f);
        Color spike = new Color(0.5f, 0.5f, 0.5f);
        Color dot = new Color(1f, 0.2f, 0.1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int i = y * size + x;
                float nx = (float)x / size - 0.5f;
                float ny = (float)y / size - 0.5f;
                float dist = Mathf.Sqrt(nx * nx + ny * ny);

                if (dist > 0.45f)
                {
                    pixels[i] = Color.clear;
                    continue;
                }

                float angle = Mathf.Atan2(ny, nx);
                float spikePattern = Mathf.Abs(Mathf.Sin(angle * 6f)) * 0.08f;

                if (dist < 0.12f)
                    pixels[i] = dot;
                else if (dist - spikePattern < 0.28f)
                    pixels[i] = body;
                else if (dist - spikePattern < 0.35f)
                    pixels[i] = spike;
                else
                    pixels[i] = Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    Sprite CreateCircleSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
        {
            int x = i % size;
            int y = i / size;
            float dx = (x + 0.5f) / size - 0.5f;
            float dy = (y + 0.5f) / size - 0.5f;
            pixels[i] = (dx * dx + dy * dy < 0.25f) ? Color.white : Color.clear;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    IEnumerator PulseObstacle(GameObject go, float minScale, float maxScale, float speed)
    {
        Vector3 baseScale = go.transform.localScale;
        while (go != null)
        {
            float t = (Mathf.Sin(Time.time * speed) + 1f) / 2f;
            float s = Mathf.Lerp(minScale, maxScale, t);
            if (go != null)
                go.transform.localScale = new Vector3(baseScale.x * s, baseScale.y * s, baseScale.z);
            yield return null;
        }
    }

    IEnumerator FlyFade(GameObject obj, Vector3 velocity, float life)
    {
        float t = 0;
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        while (t < life)
        {
            if (obj == null) yield break;
            obj.transform.position += velocity * Time.deltaTime;
            if (sr != null) { Color c = sr.color; c.a = 1f - t / life; sr.color = c; }
            t += Time.deltaTime;
            yield return null;
        }
        if (obj != null) Destroy(obj);
    }

    IEnumerator ExplosionDestroy(GameObject visual)
    {
        if (visual == null) yield break;
        SpriteRenderer sr = visual.GetComponent<SpriteRenderer>();
        if (sr == null) { Destroy(visual); yield break; }

        float duration = 0.25f;
        float t = 0;
        Color startColor = sr.color;
        while (t < duration)
        {
            if (visual == null) yield break;
            sr.color = Color.Lerp(startColor, new Color(1f, 0.4f, 0f, 0f), t / duration);
            visual.transform.localScale = Vector3.Lerp(visual.transform.localScale, visual.transform.localScale * 1.3f, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        if (visual != null) Destroy(visual);
    }

    IEnumerator LightingFlash(Vector3 pos)
    {
        GameObject flash = new GameObject("Flash");
        flash.transform.position = pos;
        SpriteRenderer sr = flash.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = new Color(1f, 1f, 1f, 0.9f);
        sr.sortingOrder = 20;
        flash.transform.localScale = Vector3.one * 2f;
        float t = 0;
        while (t < 0.1f)
        {
            if (flash == null) yield break;
            Color c = sr.color;
            c.a = Mathf.Lerp(0.9f, 0f, t / 0.1f);
            sr.color = c;
            t += Time.deltaTime;
            yield return null;
        }
        if (flash != null) Destroy(flash);
    }

    IEnumerator SmokeBurst(Vector3 pos, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject smoke = new GameObject("Smoke");
            smoke.transform.position = pos + (Vector3)Random.insideUnitCircle * 0.15f;
            SpriteRenderer sr = smoke.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            float gray = Random.Range(0.3f, 0.6f);
            sr.color = new Color(gray, gray, gray, 0.5f);
            sr.sortingOrder = 8;
            Vector3 dir = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.5f, 1.5f), 0);
            float life = Random.Range(0.4f, 0.8f);
            StartCoroutine(FlyFade(smoke, dir, life));
        }
        yield break;
    }

    IEnumerator DirtChunks(Vector3 pos, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject dirt = new GameObject("Dirt");
            dirt.transform.position = pos + (Vector3)Random.insideUnitCircle * 0.1f;
            SpriteRenderer sr = dirt.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite(0.6f, new Color(0.6f, 0.4f, 0.2f, 0.9f));
            sr.sortingOrder = 9;
            Vector3 dir = Random.insideUnitCircle.normalized * Random.Range(1.5f, 4f);
            float life = Random.Range(0.2f, 0.5f);
            StartCoroutine(FlyFade(dirt, dir, life));
        }
        yield break;
    }

    IEnumerator CameraShake(float intensity, float duration)
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;
        Vector3 orig = cam.transform.position;
        float t = 0;
        while (t < duration)
        {
            Vector3 offset = Random.insideUnitCircle * intensity;
            cam.transform.position = orig + new Vector3(offset.x, offset.y, 0);
            t += Time.deltaTime;
            yield return null;
        }
        cam.transform.position = orig;
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    Cell FindCellByPieceId(int pieceId)
    {
        for (int r = 0; r < board.rows; r++)
            for (int c = 0; c < board.cols; c++)
            {
                Cell cell = board.GetCell(r, c);
                if (cell != null && cell.IsOccupied && cell.pieceData.HasValue && cell.pieceData.Value.id == pieceId)
                    return cell;
            }
        return null;
    }
}

public class ShakeTimer : MonoBehaviour
{
    Vector3 basePos;
    void Awake() { basePos = transform.localPosition; }
    void Update()
    {
        transform.localPosition = basePos + (Vector3)(Random.insideUnitCircle * 0.02f);
    }
}
