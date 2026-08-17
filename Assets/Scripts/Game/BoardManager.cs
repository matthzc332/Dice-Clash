using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    public int rows = 7;
    public int cols = 7;
    public float cellSize = 1.1f;
    public Vector2 boardCenter = Vector2.zero;

    [Header("Theme")]
    public string speciesTheme = "Human";
    public string scenarioTheme = "Human";
    public bool isTutorial;
    public bool isShadowPhase;

    private bool initialized = false;

    [Header("Prefabs")]
    public GameObject pawnPrefab;
    public GameObject ninjaPrefab;
    public GameObject knightPrefab;
    public GameObject paladinPrefab;

    [Header("Colors")]
    public Color blueTint = new Color(0.4f, 0.6f, 1f);
    public Color redTint = new Color(1f, 0.4f, 0.4f);
    public Color highlightColor = new Color(1f, 0.8f, 0f, 0.15f);

    public Cell[,] grid { get; private set; }

    private static int nextId = 1;
    private Grid tileGrid;
    private Tilemap tilemap;
    public float tileScale = 0.9f;
    private GameObject selectionHighlight;
    private Sprite particleSprite;
    private Sprite cloudSprite;
    private Dictionary<int, Coroutine> spriteAnims = new();
    private int lastScreenWidth;
    private int lastScreenHeight;
    private Dictionary<int, Vector3> originalScales = new();
    private Dictionary<int, Vector3> originalPositions = new();
    private Dictionary<string, Sprite> poseCache = new();
    private GameObject blueFlagObj;
    private GameObject redFlagObj;
    private ParticleSystem blueSparkles;
    private ParticleSystem redSparkles;
    private TurnManager turnManager;
    private GameObject scenarioContainer;
    private List<GameObject> obstacleObjects = new();
    public bool aiInProgress { get; set; }
    public bool suppressVictoryCheck;
    private List<GameObject> aiHighlights = new();
    private int blueKillCombo = 0;
    private ObstacleManager obstacleManager;

    public void SwitchSpecies(string newSpecies)
    {
        ClearSelectedCell();

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Cell cell = grid[r, c];
                if (cell.pieceVisual != null)
                {
                    StopAnimation(cell.pieceVisual);
                    Destroy(cell.pieceVisual);
                }
                cell.ClearPiece();
            }
        }
        spriteAnims.Clear();
        nextId = 1;
        speciesTheme = newSpecies;
        SetupInitialBoard();
    }

    public void SwitchScenario(string newScenario)
    {
        scenarioTheme = newScenario;
        ClearObstacles();
        if (scenarioContainer != null) Destroy(scenarioContainer);
        scenarioContainer = new GameObject("ScenarioContainer");
        scenarioContainer.transform.SetParent(transform);
        CreateBackground();
        CreateMidground();
        CreateTilemapFloor();
        CreateTeamFlags();
        CreateBoardDecorations();
    }

    void Awake()
    {
        speciesTheme = GameConfig.selectedSpecies;
        scenarioTheme = GameConfig.selectedScenario;
        isTutorial = GameConfig.isTutorial;
    }

    void Start()
    {
        if (initialized) return;
        InitializeBoard();
    }

    public void InitializeBoard()
    {
        if (initialized) return;
        initialized = true;

        particleSprite = CreateParticleSprite();
        cloudSprite = Resources.Load<Sprite>("Sprites/Efect/Nubes");

        scenarioContainer = new GameObject("ScenarioContainer");
        scenarioContainer.transform.SetParent(transform);

        FitCamera();
        CreateBackground();
        CreateMidground();
        CreateTilemapFloor();
        CreateSelectionHighlight();
        InitializeGrid();

        if ((GameConfig.isCampaign || GameConfig.isAutoPlay) && !isTutorial)
        {
            ObstacleManager existing = FindFirstObjectByType<ObstacleManager>();
            if (existing != null)
                obstacleManager = existing;
            else
            {
                GameObject omObj = new GameObject("ObstacleManager");
                obstacleManager = omObj.AddComponent<ObstacleManager>();
            }
            TurnManager tm = FindFirstObjectByType<TurnManager>();
            obstacleManager.Initialize(this, tm);
        }

        SetupInitialBoard();
        CreateTeamFlags();

        CreateBoardDecorations();
        SetupSparkles();
    }

    void FitCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float boardWidth = (cols - 1) * cellSize;
        float boardHeight = (rows - 1) * cellSize;
        float margin = 1.5f;
        float vertSize = (boardHeight / 2f) + margin;
        float horizSize = ((boardWidth / 2f) / cam.aspect) + margin;
        cam.orthographicSize = Mathf.Max(vertSize, horizSize);
        float centerX = boardWidth / 2f;
        float centerY = -(boardHeight / 2f);
        cam.transform.position = new Vector3(centerX, centerY, -10f);
    }

    void Update()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            FitCamera();
        }
    }

    void CreateTilemapFloor()
    {
        GameObject gridObj = new GameObject("TilemapGrid");
        gridObj.transform.SetParent(scenarioContainer.transform);

        tileGrid = gridObj.AddComponent<Grid>();
        tileGrid.cellSize = new Vector3(cellSize, cellSize, 1);
        tileGrid.cellLayout = GridLayout.CellLayout.Rectangle;

        GameObject tmObj = new GameObject("FloorTilemap");
        tmObj.transform.SetParent(gridObj.transform);
        tmObj.transform.localPosition = new Vector3(0f, -0.95f, 0f);

        tilemap = tmObj.AddComponent<Tilemap>();
        tilemap.color = new Color(0.92f, 0.92f, 0.90f, 1f);
        TilemapRenderer renderer = tmObj.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = -2;

        Sprite[] piso1, piso2;
        piso1 = Resources.LoadAll<Sprite>($"Sprites/{scenarioTheme}/Floor/{scenarioTheme}Piso1");
        if (piso1.Length == 0) piso1 = Resources.LoadAll<Sprite>($"Sprites/{scenarioTheme}/Floor/piso1");
        if (piso1.Length == 0) piso1 = Resources.LoadAll<Sprite>("Sprites/Floor/piso1");
        piso2 = Resources.LoadAll<Sprite>($"Sprites/{scenarioTheme}/Floor/{scenarioTheme}Piso2");
        if (piso2.Length == 0) piso2 = Resources.LoadAll<Sprite>($"Sprites/{scenarioTheme}/Floor/piso2");
        if (piso2.Length == 0) piso2 = Resources.LoadAll<Sprite>("Sprites/Floor/piso2");

        Sprite piso1Sprite = piso1.Length > 0 ? piso1[0] : CreateSquareSprite(1f, new Color(0.9f, 0.9f, 0.9f, 0.5f));
        Sprite piso2Sprite = piso2.Length > 0 ? piso2[0] : CreateSquareSprite(1f, new Color(0.5f, 0.5f, 0.5f, 0.5f));

        var tileLight = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
        tileLight.sprite = piso1Sprite;

        var tileDark = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
        tileDark.sprite = piso2Sprite;

        float sprite1Size = piso1Sprite.rect.width / piso1Sprite.pixelsPerUnit;
        float sprite2Size = piso2Sprite.rect.width / piso2Sprite.pixelsPerUnit;
        float maxSpriteSize = Mathf.Max(sprite1Size, sprite2Size);
        float floorScale = scenarioTheme == "Beastfolk" ? 1.3f : 0.9f;
        float scale = (cellSize * floorScale) / maxSpriteSize;
        tileScale = scale;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var pos = new Vector3Int(c, -r, 0);
                var tile = (r + c) % 2 == 0 ? tileLight : tileDark;
                tilemap.SetTile(pos, tile);
                tilemap.SetTransformMatrix(pos, Matrix4x4.Scale(Vector3.one * scale));
                if (scenarioTheme == "Beastfolk")
                {
                    tilemap.SetTileFlags(pos, UnityEngine.Tilemaps.TileFlags.None);
                    if ((r + c) % 2 == 0)
                        tilemap.SetColor(pos, new Color(0.92f, 0.91f, 0.88f));
                    else
                        tilemap.SetColor(pos, new Color(0.84f, 0.82f, 0.78f));
                }
            }
        }
    }

    void CreateSelectionHighlight()
    {
        selectionHighlight = new GameObject("SelectionHighlight");
        selectionHighlight.transform.SetParent(transform);
        selectionHighlight.transform.localScale = Vector3.one * cellSize * 0.8f;
        SpriteRenderer hlSr = selectionHighlight.AddComponent<SpriteRenderer>();
        hlSr.sprite = CreateSquareSprite(1f, highlightColor);
        hlSr.sortingOrder = -1;
        selectionHighlight.SetActive(false);
    }

    void InitializeGrid()
    {
        grid = new Cell[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Cell cell = new Cell();
                cell.row = r;
                cell.col = c;
                grid[r, c] = cell;
            }
        }
    }

    void SetupInitialBoard()
    {
        var entries = isTutorial ? TutorialInitialPosition.Entries : InitialPosition.Entries;
        foreach (var entry in entries)
            PlacePiece(entry.row, entry.col, entry.team, entry.type);
    }

    void CreateBackground()
    {
        Sprite bg = null;
        if (isTutorial)
            bg = Resources.Load<Sprite>("Tutorial/fondoTuto");
        if (bg == null)
            bg = Resources.Load<Sprite>($"Sprites/{scenarioTheme}/Background/{scenarioTheme}");
        if (bg == null && scenarioTheme == "Orc") bg = Resources.Load<Sprite>("Sprites/Orc/Background/BackgroundOrco");
        if (bg == null) bg = Resources.Load<Sprite>("Sprites/Human/Background/Human");
        if (bg == null) return;

        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(scenarioContainer.transform);
        bgObj.transform.position = Camera.main.transform.position + new Vector3(0, 0, 5);
        SpriteRenderer sr = bgObj.AddComponent<SpriteRenderer>();
        sr.sprite = bg;
        sr.sortingOrder = -10;
        sr.color = Color.white;

        float worldHeight = Camera.main.orthographicSize * 2f;
        float worldWidth = worldHeight * Camera.main.aspect;
        float spriteHeight = bg.bounds.size.y;
        float spriteWidth = bg.bounds.size.x;
        float scale = Mathf.Max(worldWidth / spriteWidth, worldHeight / spriteHeight) * 1.15f;
        bgObj.transform.localScale = Vector3.one * scale;
    }

    void CreateMidground()
    {
        if (isTutorial) return;

        Sprite[] mgSprites = Resources.LoadAll<Sprite>("Sprites/Human/Background/fondo3");
        Sprite mg = mgSprites != null && mgSprites.Length > 0 ? mgSprites[0] : null;
        if (mg == null) return;

        GameObject mgObj = new GameObject("Midground");
        mgObj.transform.SetParent(scenarioContainer.transform);
        mgObj.transform.position = new Vector3(3.78f, -3.40f, 4);
        SpriteRenderer sr = mgObj.AddComponent<SpriteRenderer>();
        sr.sprite = mg;
        sr.sortingOrder = -8;
        sr.color = new Color(1, 1, 1, 0.4f);

        mgObj.transform.localScale = new Vector3(0.98f, 0.67f, 1);

        CreateClouds();
    }

    void CreateClouds()
    {
        Sprite nube1 = Resources.Load<Sprite>("Sprites/Human/Background/nube1");
        Sprite nube2 = Resources.Load<Sprite>("Sprites/Human/Background/nube2");
        if (nube1 == null || nube2 == null) return;

        Camera cam = Camera.main;
        float worldW = cam.orthographicSize * cam.aspect * 2f;
        float worldH = cam.orthographicSize * 2f;

        System.Random rng = new System.Random();

        void PlaceCloud(Sprite sprite, float xMin, float xMax, float yMin, float yMax, float speed, int dir)
        {
            GameObject obj = new GameObject(sprite.name);
            obj.transform.SetParent(scenarioContainer.transform);
            float x = (float)(rng.NextDouble() * (xMax - xMin) + xMin);
            float y = (float)(rng.NextDouble() * (yMax - yMin) + yMin);
            obj.transform.position = cam.transform.position + new Vector3(x, y, 3.5f);
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -9;
            sr.color = Color.white;
            float sScale = (float)(0.3f + rng.NextDouble() * 0.4f);
            obj.transform.localScale = Vector3.one * sScale;
            StartCoroutine(AnimateCloud(obj, speed, dir, xMin, xMax));
        }

        float hw = worldW * 0.5f;
        float hh = worldH * 0.5f;

        int count = 3;
        for (int i = 0; i < count; i++)
        {
            PlaceCloud(nube1, -hw * 1.5f, hw * 1.5f, -hh * 0.6f, hh * 0.8f, 0.3f, -1);
            PlaceCloud(nube2, -hw * 1.5f, hw * 1.5f, -hh * 0.6f, hh * 0.8f, 0.25f, 1);
        }
    }

    IEnumerator AnimateCloud(GameObject obj, float speed, int dir, float xMin, float xMax)
    {
        while (true)
        {
            if (obj == null) yield break;
            Vector3 pos = obj.transform.position;
            pos.x += speed * dir * Time.deltaTime;
            if (dir > 0 && pos.x > xMax) pos.x = xMin;
            else if (dir < 0 && pos.x < xMin) pos.x = xMax;
            obj.transform.position = pos;
            yield return null;
        }
    }

    void SetupSparkles()
    {
        StartCoroutine(SetupSparklesDelayed());
    }

    IEnumerator SetupSparklesDelayed()
    {
        // Wait until TurnManager exists (created by GameManager)
        turnManager = FindAnyObjectByType<TurnManager>();
        while (turnManager == null)
        {
            yield return null;
            turnManager = FindAnyObjectByType<TurnManager>();
        }

        // Build a soft circle texture
        Texture2D circleTex = new Texture2D(16, 16);
        Color[] pixels = new Color[256];
        for (int i = 0; i < 256; i++)
        {
            int x = i % 16;
            int y = i / 16;
            float dx = (x + 0.5f) / 16f - 0.5f;
            float dy = (y + 0.5f) / 16f - 0.5f;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            pixels[i] = dist < 0.4f ? Color.white : Color.Lerp(Color.white, Color.clear, (dist - 0.4f) * 10f);
        }
        circleTex.SetPixels(pixels);
        circleTex.Apply();

        // Try to find URP particle shader, fallback to legacy
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");

        Material mat = new Material(shader);
        mat.mainTexture = circleTex;
        if (shader.name.Contains("Universal"))
        {
            mat.SetColor("_BaseColor", Color.white);
            mat.SetFloat("_BlendOp", 0f);
            mat.SetFloat("_DstBlend", 10f);
            mat.SetFloat("_SrcBlend", 5f);
            mat.SetFloat("_ZWrite", 0f);
            mat.renderQueue = 3000;
        }

        blueSparkles = CreateSparkleSystem(mat, blueFlagObj.transform.position, "BlueSparkles");
        redSparkles = CreateSparkleSystem(mat, redFlagObj.transform.position, "RedSparkles");

        // Stop the non-active turn's sparkles
        if (turnManager.currentTurn == TurnState.BlueTurn) redSparkles.Stop();
        else blueSparkles.Stop();

        turnManager.OnTurnChanged += OnTurnChanged;
    }

    void OnTurnChanged(TurnState state)
    {
        if (state == TurnState.BlueTurn) { blueSparkles.Play(); redSparkles.Stop(); blueKillCombo = 0; }
        else { redSparkles.Play(); blueSparkles.Stop(); blueKillCombo = 0; }
    }

    ParticleSystem CreateSparkleSystem(Material mat, Vector3 position, string name)
    {
        GameObject psObj = new GameObject(name);
        psObj.transform.position = position;
        ParticleSystem ps = psObj.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = ps.main;
        main.startColor = new Color(1, 0.84f, 0, 1);
        main.startSize = 0.08f;
        main.startLifetime = 1.0f;
        main.maxParticles = 12;
        main.gravityModifier = -0.06f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true;
        main.startSpeed = 0.04f;
        main.prewarm = true;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 6;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.12f;

        ParticleSystem.ColorOverLifetimeModule colorLifetime = ps.colorOverLifetime;
        colorLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(1, 0.84f, 0), 0f), new GradientColorKey(new Color(1, 0.8f, 0), 0.5f), new GradientColorKey(new Color(1, 0.9f, 0.3f), 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.3f), new GradientAlphaKey(0f, 1f) }
        );
        colorLifetime.color = new ParticleSystem.MinMaxGradient(grad);

        ParticleSystemRenderer psr = ps.GetComponent<ParticleSystemRenderer>();
        psr.material = mat;
        psr.sortingOrder = 10;

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.2f;
        noise.frequency = 0.4f;

        var rotation = ps.rotationOverLifetime;
        rotation.enabled = true;
        rotation.z = new ParticleSystem.MinMaxCurve(60f, 180f);

        ps.Play();
        return ps;
    }

    void CreateTeamFlags()
    {
        blueFlagObj = PlaceDeco("BlueFlag", new Vector3(-0.55f, -0.8f, 0), 0.55f, 0.55f, 1);
        redFlagObj = PlaceDeco("RedFlag", new Vector3(-0.56f, -5.6f, 0), 0.55f, 0.55f, 1);
    }

    void CreateBoardDecorations()
    {
        if (isTutorial)
        {
            Sprite barril2 = Resources.Load<Sprite>("Tutorial/barril2");
            if (barril2 != null)
            {
                GameObject obj = new GameObject("barril2");
                obj.transform.SetParent(scenarioContainer.transform);
                obj.transform.position = new Vector3(4.47f, 1.4f, 0);
                SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = barril2;
                sr.sortingOrder = -1;
                obj.transform.localScale = new Vector3(0.4f, 0.4f, 1);
            }
            return;
        }

        bool orc = scenarioTheme == "Orc";
        bool beast = scenarioTheme == "Beastfolk";

        float gs = orc || beast ? 1.5f : 0.4f;

        PlaceDeco("Gargola",  new Vector3(0.39f, 0.77f, 0),  gs, gs, -1, -1);
        if (orc || beast)
            PlaceDeco("Barril",   new Vector3(4.47f, 0.61f, 0), 0.8f, 1.2f, -1);
        else
            PlaceDeco("Barril",   new Vector3(4.47f, 0.37f, 0), 0.4f, 0.4f, -1);
        PlaceDeco("Gargola",  new Vector3(7.36f, 0.77f, 0),  gs, gs, -1, -1);

        if (scenarioTheme == "Beastfolk")
            SpawnObstacles();
        else if (scenarioTheme == "Orc")
            SoundManager.Instance.PlayOrcAppear();

        if (GameConfig.isCampaign && obstacleManager != null)
        {
            CampaignLevel level = CampaignData.GetLevel(GameConfig.selectedLevel);
            if (level != null && level.obstacles != null && level.obstacles.Length > 0)
                obstacleManager.SpawnCampaignObstacles(level.obstacles);
        }
    }

    void ClearObstacles()
    {
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                Cell cell = grid[r, c];
                cell.isObstacle = false;
                if (cell.obstacleType != ObstacleType.None)
                    cell.ClearObstacle();
            }

        foreach (GameObject obj in obstacleObjects)
            if (obj != null) Destroy(obj);

        obstacleObjects.Clear();

        if (obstacleManager != null)
            obstacleManager.ClearAll();
    }

    void SpawnObstacles()
    {
        ClearObstacles();

        Sprite obstacleSprite = Resources.Load<Sprite>("Sprites/Beastfolk/Decor/Arbol");
        if (obstacleSprite == null)
            obstacleSprite = CreateSquareSprite(0.5f, new Color(0.2f, 0.6f, 0.2f));

        int count = 0;
        int maxAttempts = 50;
        while (count < 3 && maxAttempts > 0)
        {
            maxAttempts--;
            int r = Random.Range(0, rows);
            int c = Random.Range(0, cols);
            Cell cell = grid[r, c];
            if (cell.IsOccupied || cell.isObstacle) continue;

            cell.isObstacle = true;
            count++;

            GameObject obstacle = new GameObject("Arbol");
            obstacle.transform.SetParent(scenarioContainer.transform);
            obstacle.transform.position = CellToWorld(r, c);
            SpriteRenderer sr = obstacle.AddComponent<SpriteRenderer>();
            sr.sprite = obstacleSprite;
            sr.sortingOrder = -1;
            obstacle.transform.localScale = new Vector3(0.05f, 0.07f, 1f);
            obstacleObjects.Add(obstacle);
            StartCoroutine(AnimateTreeGrow(obstacle));
            StartCoroutine(SpawnGlow(obstacle.transform.position));
        }

        SoundManager.Instance.PlayRockBreak();
    }

    IEnumerator AnimateTreeGrow(GameObject tree)
    {
        if (tree == null) yield break;
        float duration = 0.5f;
        float t = 0;
        Vector3 startScale = tree.transform.localScale;
        Vector3 endScale = new Vector3(0.10f, 0.14f, 1f);
        while (t < duration)
        {
            if (tree == null) yield break;
            float raw = t / duration;
            float eased = raw * raw * (3f - 2f * raw);
            tree.transform.localScale = Vector3.Lerp(startScale, endScale, eased);
            t += Time.deltaTime;
            yield return null;
        }
        if (tree != null)
            tree.transform.localScale = endScale;
    }

    GameObject PlaceDeco(string spriteName, Vector3 position, float scaleX, float scaleY, float xFlip, int order = 1)
    {
        Sprite sprite = null;

        string lookupName = spriteName;
        if (scenarioTheme != "Human" && spriteName == "Estatua")
            lookupName = $"{scenarioTheme}Craneo";

        string themePath = $"Sprites/{scenarioTheme}/Decor/{lookupName}";
        sprite = Resources.Load<Sprite>(themePath);

        if (sprite == null && scenarioTheme != "Human" && lookupName == spriteName)
        {
            string prefixedPath = $"Sprites/{scenarioTheme}/Decor/{scenarioTheme}{spriteName}";
            sprite = Resources.Load<Sprite>(prefixedPath);
        }

        if (sprite == null)
        {
            string commonPath = $"Sprites/Common/Decor/{spriteName}";
            sprite = Resources.Load<Sprite>(commonPath);
        }

        if (sprite == null)
        {
            string sharedPath = $"Sprites/Decor/{spriteName}";
            sprite = Resources.Load<Sprite>(sharedPath);
        }

        if (sprite == null) return null;

        GameObject obj = new GameObject(spriteName);
        obj.transform.SetParent(scenarioContainer.transform);
        obj.transform.position = position;
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        obj.transform.localScale = new Vector3(scaleX * xFlip, scaleY, 1);
        return obj;
    }

    void PlacePiece(int row, int col, Team team, PieceType type)
    {
        Cell cell = grid[row, col];
        int id = nextId++;

        string species = team == Team.Blue ? GameConfig.selectedSpecies : scenarioTheme;
        PieceData data = new PieceData(id, team, type, species);
        bool front = team == Team.Blue;
        GameObject visual = CreatePieceVisual(type, team, front, row);
        Vector3 pos = CellToWorld(row, col);
        if (row == rows - 1) pos.y += 0.25f;
        visual.transform.position = pos;
        visual.transform.SetParent(transform);
        visual.name = $"{team}_{type}_{id}";
        cell.SetPiece(data, visual);
        StartCoroutine(AnimateSpawn(visual));
        StartCoroutine(SpawnGlow(visual.transform.position));
    }

    public Color GetPieceColor(Team team)
    {
        if (isShadowPhase && team == Team.Red) return new Color(0.2f, 0.2f, 0.25f);
        if (team == Team.Red) return new Color(0.7f, 0.7f, 0.7f);
        return Color.white;
    }

    bool IsTutorialDummy(Team team)
    {
        return isTutorial && !isShadowPhase && team == Team.Red;
    }

    Sprite GetTutorialDummySprite()
    {
        return Resources.Load<Sprite>("Tutorial/muneco1");
    }

    Vector3 GetTutorialDummyScale()
    {
        return new Vector3(0.19f, 0.19f, 1f);
    }

    GameObject CreatePieceVisual(PieceType type, Team team, bool front, int row = 0)
    {
        GameObject obj = new GameObject($"{team}_{type}");
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = row;
        sr.color = GetPieceColor(team);

        if (IsTutorialDummy(team))
        {
            Sprite dummySprite = GetTutorialDummySprite();
            if (dummySprite != null) sr.sprite = dummySprite;
            else sr.sprite = CreateSquareSprite(0.7f, new Color(0.6f, 0.4f, 0.2f));
            obj.transform.localScale = GetTutorialDummyScale();
            return obj;
        }

        Sprite baseSprite = GetSprite(type, front, "Idle", team);
        if (baseSprite != null) sr.sprite = baseSprite;
        else sr.sprite = CreateSquareSprite(0.7f, Color.white);
        string pieceSpecies = team == Team.Blue ? speciesTheme : scenarioTheme;
        obj.transform.localScale = GetIdleScale(type, pieceSpecies);
        return obj;
    }

    string PieceName(PieceType type) => type switch
    {
        PieceType.Pawn => "Peon",
        PieceType.Ninja => "Ninja",
        PieceType.Knight => "Caballero",
        PieceType.Paladin => "Paladin",
        _ => "Peon"
    };

    string PieceDisplayName(PieceType type) => type switch
    {
        PieceType.Pawn => "Pawn",
        PieceType.Ninja => "Ninja",
        PieceType.Knight => "Knight",
        PieceType.Paladin => "Paladin",
        _ => "Pawn"
    };

    string ThemeForTeam(Team team) => team == Team.Blue ? speciesTheme : scenarioTheme;

    Sprite[] LoadSprites(PieceType type, bool front, string suffix, Team team)
    {
        string name = PieceName(type);
        string dir = front ? "Front" : "Back";
        string sub = front ? "" : "Back/";
        string primary = ThemeForTeam(team);
        foreach (string t in new[] { primary, speciesTheme, scenarioTheme, "Human" })
        {
            string path = $"Sprites/{t}/Pieces/{sub}{name}{suffix}{dir}";
            Sprite[] sprites = Resources.LoadAll<Sprite>(path);
            if (sprites.Length > 0) return FilterSprites(sprites);
            if (suffix != "")
            {
                path = $"Sprites/{t}/Pieces/{sub}{name}{dir}";
                sprites = Resources.LoadAll<Sprite>(path);
                if (sprites.Length > 0) return FilterSprites(sprites);
            }
        }
        return null;
    }

    Sprite[] FilterSprites(Sprite[] sprites)
    {
        System.Array.Sort(sprites, (a, b) =>
            (int)(b.rect.width * b.rect.height - a.rect.width * a.rect.height));
        List<Sprite> valid = new List<Sprite>(sprites.Length);
        foreach (var s in sprites)
            if (s.rect.width >= 40 && s.rect.height >= 40)
                valid.Add(s);
        if (valid.Count == 0) return sprites;
            if (valid.Count > 1)
            {
                float maxArea = valid[0].rect.width * valid[0].rect.height;
                float nextArea = valid[1].rect.width * valid[1].rect.height;
                if (maxArea > nextArea * 3f)
                    valid.RemoveAt(0);
            }
        return valid.ToArray();
    }

    Sprite GetSprite(PieceType type, bool front, string suffix, Team team)
    {
        Sprite[] all = LoadSprites(type, front, suffix, team);
        return all != null && all.Length > 0 ? all[0] : CreateSquareSprite(0.7f, Color.white);
    }

    void PlayAnimation(GameObject visual, Sprite[] sprites, float delay)
    {
        int id = visual.GetInstanceID();
        if (spriteAnims.TryGetValue(id, out Coroutine old) && old != null)
            StopCoroutine(old);
        if (sprites == null || sprites.Length == 0) return;
        SpriteRenderer sr = visual.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sprite = sprites[0];
        if (sprites.Length < 3) return;
        spriteAnims[id] = StartCoroutine(AnimateSprites(visual, sprites, delay));
    }

    void StopAnimation(GameObject visual)
    {
        int id = visual.GetInstanceID();
        if (spriteAnims.TryGetValue(id, out Coroutine old) && old != null)
        {
            StopCoroutine(old);
            spriteAnims.Remove(id);
        }
    }

    IEnumerator AnimateSprites(GameObject visual, Sprite[] sprites, float delay)
    {
        SpriteRenderer sr = visual.GetComponent<SpriteRenderer>();
        int i = 0;
        while (true)
        {
            if (visual == null || sr == null) yield break;
            sr.sprite = sprites[i];
            i = (i + 1) % sprites.Length;
            yield return new WaitForSeconds(delay);
        }
    }

    public List<Cell> GetValidMoves(int row, int col, PieceType type)
    {
        List<Cell> result = new();

        var directions = type switch
        {
            PieceType.Pawn => new[] {
                (dr:  0, dc:  1), (dr:  0, dc: -1), (dr:  1, dc:  0), (dr: -1, dc:  0),
                (dr:  1, dc:  1), (dr:  1, dc: -1), (dr: -1, dc:  1), (dr: -1, dc: -1)
            },
            PieceType.Ninja => new[] {
                (dr:  1, dc:  1), (dr:  1, dc: -1), (dr: -1, dc:  1), (dr: -1, dc: -1)
            },
            PieceType.Knight => new[] {
                (dr:  0, dc:  1), (dr:  0, dc: -1), (dr:  1, dc:  0), (dr: -1, dc:  0)
            },
            PieceType.Paladin => new[] {
                (dr:  0, dc:  1), (dr:  0, dc: -1), (dr:  1, dc:  0), (dr: -1, dc:  0),
                (dr:  1, dc:  1), (dr:  1, dc: -1), (dr: -1, dc:  1), (dr: -1, dc: -1)
            },
            _ => new[] { (dr: 0, dc: 0) }
        };

        int maxRange = type == PieceType.Pawn ? 1 : 3;
        bool isNinja = type == PieceType.Ninja;
        bool isDiagonal = false;

        foreach (var dir in directions)
        {
            if (isNinja)
                isDiagonal = dir.dr != 0 && dir.dc != 0;

            for (int d = 1; d <= maxRange; d++)
            {
                int nr = row + dir.dr * d;
                int nc = col + dir.dc * d;

                Cell cell = GetCell(nr, nc);
                if (cell == null) break;

                if (cell.isObstacle && !cell.isDestroyed && cell.BlocksMovement)
                {
                    break;
                }

                if (cell.pieceData.HasValue)
                {
                    if (cell.pieceData.Value.team != GetTeamForPiece(row, col))
                        result.Add(cell);

                    if (isNinja && isDiagonal)
                        continue;
                    else
                        break;
                }

                result.Add(cell);
            }
        }

        return result;
    }

    bool IsFrontByTeam(Team team) => team == Team.Blue;

    Team GetTeamForPiece(int row, int col)
    {
        Cell cell = grid[row, col];
        return cell.pieceData?.team ?? Team.Blue;
    }

    public Cell GetCell(int row, int col)
    {
        if (row < 0 || row >= rows || col < 0 || col >= cols)
            return null;
        return grid[row, col];
    }

    public Vector3 CellToWorld(int row, int col)
    {
        Vector3Int cellPos = new Vector3Int(col, -row, 0);
        return tilemap.GetCellCenterWorld(cellPos);
    }

    public void ReplaceTileSprite(int row, int col, Sprite newSprite)
    {
        Vector3Int pos = new Vector3Int(col, -row, 0);
        var tile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
        tile.sprite = newSprite;
        tilemap.SetTile(pos, tile);

        float spriteSize = newSprite.rect.width / newSprite.pixelsPerUnit;
        float s = spriteSize > 0 ? (cellSize * 0.9f) / spriteSize : tileScale;
        tilemap.SetTransformMatrix(pos, Matrix4x4.Scale(Vector3.one * s * 1.1f));
    }

    public Sprite GetTileSprite(int row, int col)
    {
        Vector3Int pos = new Vector3Int(col, -row, 0);
        var tile = tilemap.GetTile(pos) as UnityEngine.Tilemaps.Tile;
        return tile != null ? tile.sprite : null;
    }

    public (int row, int col)? WorldToCell(Vector3 worldPos)
    {
        Vector3Int cellPos = tilemap.WorldToCell(worldPos);
        int col = cellPos.x;
        int row = -cellPos.y;
        if (row < 0 || row >= rows || col < 0 || col >= cols)
            return null;
        return (row, col);
    }

    public Team? CheckVictory()
    {
        bool blueAlive = false;
        bool redAlive = false;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Cell cell = grid[r, c];
                if (cell.IsOccupied && cell.pieceData.HasValue)
                {
                    if (cell.pieceData.Value.team == Team.Blue)
                        blueAlive = true;
                    else
                        redAlive = true;
                }
            }
        }

        if (!blueAlive) return Team.Red;
        if (!redAlive) return Team.Blue;
        return null;
    }

    public void SetSelectedCell(int row, int col)
    {
        selectionHighlight.transform.position = CellToWorld(row, col);
        selectionHighlight.SetActive(true);
    }

    public void ClearSelectedCell()
    {
        selectionHighlight.SetActive(false);
    }

    public void SetAIRedHighlight(Cell cell)
    {
        if (cell == null) return;
        Vector3 pos = CellToWorld(cell.row, cell.col);
        GameObject hl = new GameObject("AIRedHighlight");
        hl.transform.SetParent(transform);
        hl.transform.position = pos;
        hl.transform.localScale = Vector3.one * cellSize * 0.85f;
        SpriteRenderer sr = hl.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite(1f, new Color(0.9f, 0.15f, 0.15f, 0.35f));
        sr.sortingOrder = -1;
        aiHighlights.Add(hl);
    }

    public void SetAIGreenHighlight(Cell cell)
    {
        if (cell == null) return;
        Vector3 pos = CellToWorld(cell.row, cell.col);
        GameObject hl = new GameObject("AIGreenHighlight");
        hl.transform.SetParent(transform);
        hl.transform.position = pos;
        hl.transform.localScale = Vector3.one * cellSize * 0.85f;
        SpriteRenderer sr = hl.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite(1f, new Color(0.15f, 0.9f, 0.15f, 0.35f));
        sr.sortingOrder = -1;
        aiHighlights.Add(hl);
    }

    public void SetAIRedMarker(Vector3 pos)
    {
        GameObject marker = new GameObject("AIRedMoveMarker");
        marker.transform.position = pos;
        marker.transform.localScale = Vector3.one * 1.05f;
        SpriteRenderer sr = marker.AddComponent<SpriteRenderer>();
        Texture2D tex = new Texture2D(16, 16);
        Color border = new Color(0.9f, 0.15f, 0.15f, 0.7f);
        Color clear = Color.clear;
        Color[] pixels = new Color[256];
        for (int i = 0; i < pixels.Length; i++)
        {
            int x = i % 16;
            int y = i / 16;
            if (x < 1 || x >= 15 || y < 1 || y >= 15)
                pixels[i] = border;
            else if (x < 2 || x >= 14 || y < 2 || y >= 14)
                pixels[i] = new Color(0.9f, 0.15f, 0.15f, 0.3f);
            else
                pixels[i] = clear;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
        sr.sortingOrder = 3;
        aiHighlights.Add(marker);
    }

    public void ClearAIHighlights()
    {
        foreach (var hl in aiHighlights)
            if (hl != null) Destroy(hl);
        aiHighlights.Clear();
    }

    public void MovePiece(int fromRow, int fromCol, int toRow, int toCol)
    {
        StartCoroutine(AnimatedMove(fromRow, fromCol, toRow, toCol));
    }

    public int CountAlive(Team team)
    {
        int count = 0;
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                Cell cell = grid[r, c];
                if (cell.IsOccupied && cell.pieceData.HasValue && cell.pieceData.Value.team == team)
                    count++;
            }
        return count;
    }

    public IEnumerator MovePieceAI(int fromRow, int fromCol, int toRow, int toCol)
    {
        yield return AnimatedMove(fromRow, fromCol, toRow, toCol);
    }

    void ResetPieceSprite(GameObject visual, PieceType type, Team team)
    {
        StopAnimation(visual);
        SpriteRenderer sr = visual.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        int id = sr.GetInstanceID();
        if (originalPositions.TryGetValue(id, out Vector3 origPos))
        {
            sr.transform.position = origPos;
            originalPositions.Remove(id);
        }
        originalScales.Remove(id);
        if (IsTutorialDummy(team))
        {
            Sprite dummySprite = GetTutorialDummySprite();
            if (dummySprite != null) sr.sprite = dummySprite;
            sr.transform.localScale = GetTutorialDummyScale();
            sr.color = GetPieceColor(team);
            return;
        }
        string pieceSpecies = team == Team.Blue ? speciesTheme : scenarioTheme;
        sr.transform.localScale = GetIdleScale(type, pieceSpecies);
        bool front = team == Team.Blue;
        Sprite baseSprite = GetSprite(type, front, "Idle", team);
        if (baseSprite != null) sr.sprite = baseSprite;
        sr.color = GetPieceColor(team);
    }

    float GetPoseScale(PieceType type, string species = "Human")
    {
        float baseScale = type switch
        {
            PieceType.Pawn => 0.62f,
            PieceType.Ninja => 0.57f,
            PieceType.Knight => 0.60f,
            PieceType.Paladin => 0.65f,
            _ => 0.58f
        };
        float multiplier = species == "Human" ? 1f : 0.9f;
        return baseScale * multiplier;
    }

    Vector3 GetIdleScale(PieceType type, string species)
    {
        if (type == PieceType.Pawn) return new Vector3(0.85f, 0.85f, 1f);
        if (type == PieceType.Knight && species == "Beastfolk") return new Vector3(1.0f, 1.0f, 1f);
        if (type == PieceType.Knight) return new Vector3(1.15f, 1.15f, 1f);
        if (type == PieceType.Paladin && species == "Beastfolk") return new Vector3(0.95f, 0.95f, 1f);
        return Vector3.one;
    }

    float GetPoseYOffset(PieceType type) => 0.2f;

    Sprite LoadPoseSprite(string species, string className)
    {
        string cacheKey = $"{species}_{className}";
        if (poseCache.TryGetValue(cacheKey, out Sprite cached))
            return cached;

        string basePath = $"Sprites/{species}/Pieces/Pose{className}";

        Sprite[] all = Resources.LoadAll<Sprite>(basePath);
        if (all != null && all.Length > 0)
        {
            poseCache[cacheKey] = all[0];
            return all[0];
        }

        Texture2D tex = Resources.Load<Texture2D>(basePath);
        if (tex != null)
        {
            Sprite created = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            poseCache[cacheKey] = created;
            return created;
        }

        return null;
    }

    public void SetAttackPose(Cell cell)
    {
        if (cell == null || !cell.IsOccupied || cell.pieceVisual == null) return;
        PieceData data = cell.pieceData.Value;
        StopAnimation(cell.pieceVisual);
        SpriteRenderer sr = cell.pieceVisual.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        string className = PieceName(data.type);
        string poseSpecies = data.team == Team.Blue ? speciesTheme : scenarioTheme;
        Sprite poseSprite = LoadPoseSprite(poseSpecies, className);
        if (poseSprite == null) return;
        int id = sr.GetInstanceID();
        if (!originalScales.ContainsKey(id))
        {
            originalScales[id] = sr.transform.localScale;
            originalPositions[id] = sr.transform.position;
        }
        float yOffset = GetPoseYOffset(data.type);
        if (poseSpecies == "Orc" && data.type == PieceType.Paladin)
            sr.transform.localScale = new Vector3(0.267f, 0.267f, originalScales[id].z);
        else
            sr.transform.localScale = originalScales[id] * GetPoseScale(data.type, poseSpecies);
        Vector3 basePos = originalPositions[id];
        sr.transform.position = new Vector3(basePos.x, basePos.y + yOffset, basePos.z);
        sr.sprite = poseSprite;
    }

    public void ResetToIdle(Cell cell)
    {
        if (cell == null || !cell.IsOccupied || cell.pieceVisual == null) return;
        PieceData data = cell.pieceData.Value;
        ResetPieceSprite(cell.pieceVisual, data.type, data.team);
    }

    public void ConvertPieceType(Cell cell, PieceType newType)
    {
        if (cell == null || !cell.IsOccupied || cell.pieceData == null) return;
        PieceData data = cell.pieceData.Value;
        PieceData newData = new PieceData
        {
            id = data.id,
            team = data.team,
            type = newType,
            species = data.species
        };
        cell.pieceData = newData;
        ResetPieceSprite(cell.pieceVisual, newType, data.team);
    }

    public Sprite GetPiecePoseSprite(PieceType type, Team team)
    {
        string className = PieceName(type);
        string poseSpecies = team == Team.Blue ? speciesTheme : scenarioTheme;
        return LoadPoseSprite(poseSpecies, className);
    }

    public Sprite GetPieceIdleSprite(PieceType type, Team team)
    {
        bool front = team == Team.Blue;
        return GetSprite(type, front, "Idle", team);
    }

    public Sprite GetSpeciesFrontIdleSprite(PieceType type, string species)
    {
        string name = PieceName(type);
        string path = $"Sprites/{species}/Pieces/{name}IdleFront";
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);
        return sprites.Length > 0 ? sprites[0] : null;
    }

    IEnumerator AnimatedMove(int fromRow, int fromCol, int toRow, int toCol)
    {
        Cell from = GetCell(fromRow, fromCol);
        Cell to = GetCell(toRow, toCol);

        if (from == null || to == null) yield break;
        if (!from.IsOccupied || !from.pieceData.HasValue) yield break;

        PieceData movingData = from.pieceData.Value;
        GameObject movingVisual = from.pieceVisual;
        if (movingVisual == null) yield break;

        bool movingForward = toRow > fromRow;
        SpriteRenderer sr = movingVisual != null ? movingVisual.GetComponent<SpriteRenderer>() : null;

        Vector3 fromPos = CellToWorld(fromRow, fromCol);
        Vector3 toPos = CellToWorld(toRow, toCol);

        Sprite moveSprite = GetSprite(movingData.type, movingForward, "Move", movingData.team);
        if (moveSprite != null && sr != null) sr.sprite = moveSprite;

        if (movingData.type == PieceType.Ninja)
            StartCoroutine(NinjaMoveCloud(fromPos));
        else if (movingData.type == PieceType.Paladin)
        {
            StartCoroutine(PaladinAura(movingVisual, fromPos, toPos, false));
            StartCoroutine(PaladinLightBeam(movingVisual));
        }

        if (to.IsOccupied && to.pieceData.HasValue && to.pieceData.Value.team != movingData.team)
            StartCoroutine(SwordClashDelayed(0.45f));

        float knightDist = Mathf.Abs(toRow - fromRow) + Mathf.Abs(toCol - fromCol);
        bool useKnightJump = movingData.type == PieceType.Knight && knightDist > 1 && !GameConfig.isAutoPlay;

        if (useKnightJump)
        {
            StartCoroutine(MoveTrail(movingVisual, fromPos, toPos, movingData.team));
            yield return KnightJump(movingVisual, fromPos, toPos, knightDist, movingData.team, movingForward);
        }
        else
        {
            float slideDur = GameConfig.isAutoPlay ? 0.08f : 0.9f;
            if (movingData.type == PieceType.Paladin && !GameConfig.isAutoPlay) slideDur = 1.3f;
            StartCoroutine(MoveTrail(movingVisual, fromPos, toPos, movingData.team));
            yield return AnimateSlide(movingVisual, fromPos, toPos, slideDur);
        }
        if (movingVisual == null) yield break;

        if (to.IsOccupied && to.pieceData.HasValue)
        {
            if (to.pieceData?.team == movingData.team)
            {
                ResetPieceSprite(movingVisual, movingData.type, movingData.team);
                yield break;
            }

            PieceData defenderData = to.pieceData.Value;
            GameObject defenderVisual = to.pieceVisual;
            if (defenderVisual == null) yield break;

            Vector3 attackDir = (toPos - fromPos).normalized;

            int atkId = sr != null ? sr.GetInstanceID() : movingVisual.GetInstanceID();
            if (!originalScales.ContainsKey(atkId))
                originalScales[atkId] = movingVisual.transform.localScale;

            Sprite[] atkSprites = IsTutorialDummy(movingData.team) ? null : LoadSprites(movingData.type, movingForward, "Attack", movingData.team);
            if (atkSprites != null && sr != null)
            {
                sr.sprite = atkSprites[0];
                PlayAnimation(movingVisual, atkSprites, 0.3f);
                if (movingData.type == PieceType.Paladin && movingForward)
                    sr.transform.localScale = new Vector3(0.46f, 0.52f, 1f);
            }
            SpriteRenderer defSr = defenderVisual.GetComponent<SpriteRenderer>();
            bool defForward = !movingForward;
            Sprite[] defAtk = IsTutorialDummy(defenderData.team) ? null : LoadSprites(defenderData.type, defForward, "Attack", defenderData.team);
            if (defAtk != null && defSr != null)
            {
                defSr.sprite = defAtk[0];
                PlayAnimation(defenderVisual, defAtk, 0.3f);
                if (defenderData.type == PieceType.Paladin && defForward)
                    defSr.transform.localScale = new Vector3(0.46f, 0.52f, 1f);
            }

            StartCoroutine(AnimateAttackLunge(movingVisual, attackDir));
            StartCoroutine(CombatShake(0.15f));

            if (movingData.type == PieceType.Paladin)
                StartCoroutine(PaladinAura(movingVisual, fromPos, toPos, true));

            float combatDelay = GameConfig.isAutoPlay ? 0.1f : 1f;
            yield return new WaitForSeconds(combatDelay);
            if (movingVisual == null) yield break;

            CombatOutcome outcome = CombatManager.Resolve(movingData, defenderData, fromRow, fromCol, to, this);

            if (AutoPlayStats.Instance != null)
            {
                AutoPlayStats.Instance.LogCombat(
                    movingData.team.ToString(), movingData.type.ToString(), fromRow, fromCol,
                    defenderData.team.ToString(), defenderData.type.ToString(), to.row, to.col,
                    outcome.atkDie1, outcome.atkDie2, outcome.atkBonus + 1, outcome.atkTotal,
                    outcome.defDie1, outcome.defDie2, outcome.defBonus + defenderData.defBonus, outcome.defTotal,
                    outcome.atkAbilityName, outcome.defAbilityName,
                    outcome.result == CombatResult.AttackerWins);
            }

            TimerManager.Instance.Pause();
            if (turnManager != null) turnManager.PauseTimer();

            BattleResultUI.Instance.ShowResult(outcome, movingData, defenderData);
            yield return new WaitUntil(() => BattleResultUI.Instance.IsClosed);

            if (turnManager != null) turnManager.ResumeTimer();
            TimerManager.Instance.Resume();

            if (outcome.result == CombatResult.AttackerWins)
            {
                SoundManager.HitStop(0.06f);
                StartCoroutine(AnimateHitImpact(defenderVisual));
                Vector3 deathPos = defenderVisual != null ? defenderVisual.transform.position : toPos;
                OnKillEffect(deathPos, defenderData.team, defenderData.type, false);
                yield return AnimateDestroy(defenderVisual, defenderData.team);
                to.ClearPiece();
                if (CoinManager.Instance != null)
                    CoinManager.Instance.AwardKill(movingData.team, movingVisual != null ? movingVisual.transform.position : fromPos, defenderData.type, false);

                if (movingVisual == null) yield break;
                movingVisual.transform.SetParent(transform);
                movingVisual.transform.position = toPos;
                from.ClearPiece();
                to.SetPiece(movingData, movingVisual);

                ResetPieceSprite(movingVisual, movingData.type, movingData.team);
            }
            else
            {
                SoundManager.HitStop(0.06f);
                SoundManager.Instance.PlayBlock();
                StartCoroutine(AnimateHitImpact(movingVisual));
                Vector3 deathPos2 = movingVisual != null ? movingVisual.transform.position : fromPos;
                OnKillEffect(deathPos2, movingData.team, movingData.type, false);
                yield return AnimateDestroy(movingVisual, movingData.team);
                from.ClearPiece();

                if (defenderVisual != null)
                    ResetPieceSprite(defenderVisual, defenderData.type, defenderData.team);
            }

            SpawnEmojis(outcome, movingData, defenderData);

            yield return CheckVictoryAndEndTurn();
            yield break;
        }

        if (movingVisual == null) yield break;
        SoundManager.Instance.PlayMove();
        movingVisual.transform.SetParent(transform);
        movingVisual.transform.position = toPos;
        from.ClearPiece();
        to.SetPiece(movingData, movingVisual);
        ResetPieceSprite(movingVisual, movingData.type, movingData.team);

        if (obstacleManager != null)
        {
            if (to.obstacleType == ObstacleType.Mine && to.hasMine)
            {
                obstacleManager.TryTriggerMine(toRow, toCol, movingData, movingVisual);
                yield break;
            }
            if (to.obstacleType == ObstacleType.Glue && to.hasGlue)
            {
                obstacleManager.TryTriggerGlue(movingData.id, to);
            }
        }

        yield return CheckVictoryAndEndTurn();
    }

    IEnumerator AnimateAttackLunge(GameObject visual, Vector3 dir)
    {
        if (visual == null) yield break;
        Vector3 origin = visual.transform.position;
        float reach = 0.15f;
        float lungeTime = 0.12f;
        float t = 0;
        while (t < lungeTime)
        {
            if (visual == null) yield break;
            visual.transform.position = Vector3.Lerp(origin, origin + dir * reach, t / lungeTime);
            t += Time.deltaTime;
            yield return null;
        }
        if (visual == null) yield break;
        visual.transform.position = origin + dir * reach;
        t = 0;
        while (t < lungeTime)
        {
            if (visual == null) yield break;
            visual.transform.position = Vector3.Lerp(origin + dir * reach, origin, t / lungeTime);
            t += Time.deltaTime;
            yield return null;
        }
        if (visual != null) visual.transform.position = origin;
    }

    IEnumerator AnimateSlide(GameObject visual, Vector3 fromPos, Vector3 toPos, float duration)
    {
        float t = 0;
        while (t < duration)
        {
            if (visual == null) yield break;
            float raw = t / duration;
            float eased = raw * raw * (3f - 2f * raw);
            visual.transform.position = Vector3.Lerp(fromPos, toPos, eased);
            t += Time.deltaTime;
            yield return null;
        }
        if (visual != null) visual.transform.position = toPos;
    }

    string SpritePrefix(string theme)
    {
        if (theme == "Beastfolk") return "beast";
        return theme.ToLower();
    }

    Sprite LoadJumpSprite(string species, int index, bool front)
    {
        string sub = front ? "" : "Back/";
        string prefix = SpritePrefix(species);
        string name = $"{prefix}{(front ? "" : "Back")}Salto{index}";
        string[] themes = new[] { species, speciesTheme, scenarioTheme, "Human" };
        foreach (string t in themes)
        {
            Sprite sp = LoadLargestSprite($"Sprites/{t}/Pieces/{sub}{name}");
            if (sp != null) return sp;
        }
        if (!front)
        {
            string frontName = $"{prefix}Salto{index}";
            foreach (string t in themes)
            {
                Sprite sp = LoadLargestSprite($"Sprites/{t}/Pieces/{frontName}");
                if (sp != null) return sp;
            }
        }
        return null;
    }

    Sprite LoadLargestSprite(string path)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);
        if (sprites == null || sprites.Length == 0) return null;
        if (sprites.Length == 1) return sprites[0];
        Sprite best = sprites[0];
        float bestArea = best.rect.width * best.rect.height;
        for (int i = 1; i < sprites.Length; i++)
        {
            float area = sprites[i].rect.width * sprites[i].rect.height;
            if (area > bestArea) { best = sprites[i]; bestArea = area; }
        }
        return best;
    }

    IEnumerator KnightJump(GameObject visual, Vector3 fromPos, Vector3 toPos, float distance, Team team, bool movingForward)
    {
        if (visual == null) yield break;
        SpriteRenderer sr = visual.GetComponent<SpriteRenderer>();

        string pieceSpecies = ThemeForTeam(team);
        Sprite salto1 = LoadJumpSprite(pieceSpecies, 1, movingForward);
        Sprite salto2 = LoadJumpSprite(pieceSpecies, 2, movingForward);

        Vector3 idleScale = GetIdleScale(PieceType.Knight, pieceSpecies);
        Vector3 baseScale;
        Vector3 salto1Scale = idleScale * 0.55f;
        Vector3 salto2Scale = idleScale * 0.55f;

        if (!movingForward)
        {
            if (pieceSpecies == "Human")
            {
                salto1Scale = new Vector3(0.10f, 0.10f, 1f);
                salto2Scale = new Vector3(0.10f, 0.10f, 1f);
            }
            else if (pieceSpecies == "Orc")
            {
                salto1Scale = new Vector3(0.33f, 0.30f, 1f);
                salto2Scale = new Vector3(0.24f, 0.28f, 1f);
            }
            else if (pieceSpecies == "Beastfolk")
            {
                salto1Scale = new Vector3(0.38f, 0.38f, 1f);
                salto2Scale = new Vector3(0.38f, 0.38f, 1f);
            }
        }

        baseScale = movingForward ? idleScale * 0.55f : salto1Scale;
        visual.transform.localScale = baseScale;
        if (salto1 != null && sr != null) sr.sprite = salto1;

        float arcHeight = 0.45f + distance * 0.22f;
        float jumpDuration = 0.35f + distance * 0.1f;

        float t = 0;
        while (t < jumpDuration)
        {
            if (visual == null) yield break;
            float progress = t / jumpDuration;
            float arc = 4f * progress * (1f - progress);
            float height = arc * arcHeight;
            Vector2 pos = Vector2.Lerp(fromPos, toPos, progress);
            pos.y += height;
            visual.transform.position = pos;
            float midScale = 1f + arc * 0.12f;
            visual.transform.localScale = baseScale * midScale;
            t += Time.deltaTime;
            yield return null;
        }

        if (visual == null) yield break;
        visual.transform.position = toPos;
        baseScale = movingForward ? idleScale * 0.55f : salto2Scale;
        visual.transform.localScale = baseScale;

        if (salto2 != null && sr != null) sr.sprite = salto2;

        yield return new WaitForSecondsRealtime(0.08f);

        float squashTime = 0.1f;
        float st = 0;
        while (st < squashTime)
        {
            if (visual == null) yield break;
            float p = st / squashTime;
            visual.transform.localScale = new Vector3(baseScale.x * (1f + 0.08f * (1f - p)), baseScale.y * (1f - 0.1f * (1f - p)), baseScale.z);
            st += Time.deltaTime;
            yield return null;
        }
        if (visual != null) visual.transform.localScale = baseScale;

        SoundManager.Instance.PlayHammer();
        StartCoroutine(CombatShake(0.12f + distance * 0.04f));

        if (visual != null)
            visual.transform.localScale = idleScale;

        int dustCount = 4 + Mathf.FloorToInt(distance);
        for (int i = 0; i < dustCount; i++)
        {
            GameObject dust = new GameObject("KnightDust");
            dust.transform.position = toPos;
            SpriteRenderer dustSr = dust.AddComponent<SpriteRenderer>();
            dustSr.sprite = particleSprite;
            dustSr.color = new Color(0.7f, 0.6f, 0.4f, 0.7f);
            dustSr.sortingOrder = -1;
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float speed = Random.Range(1.5f, 3.5f);
            Vector2 vel = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed * 0.4f + 1f);
            StartCoroutine(AnimateDustParticle(dust, dustSr, vel, 0.4f));
        }
    }

    IEnumerator AnimateDustParticle(GameObject obj, SpriteRenderer sr, Vector2 vel, float life)
    {
        float t = 0;
        while (t < life)
        {
            if (obj == null) yield break;
            float dt = Time.deltaTime;
            obj.transform.position += (Vector3)(vel * dt);
            vel.y -= 4f * dt;
            t += dt;
            Color c = sr.color;
            c.a = Mathf.Lerp(0.7f, 0f, t / life);
            sr.color = c;
            sr.transform.localScale = Vector3.one * (1f + t / life * 0.5f);
            yield return null;
        }
        if (obj != null) Destroy(obj);
    }

    IEnumerator AnimateDestroy(GameObject visual, Team team)
    {
        if (visual == null) yield break;
        Vector3 origin = visual.transform.position;

        StartCoroutine(DeathPoof(origin, team));

        float shakeDuration = 0.1f;
        float shakeTimer = 0;
        while (shakeTimer < shakeDuration)
        {
            if (visual == null) yield break;
            visual.transform.position = origin + (Vector3)Random.insideUnitCircle * 0.08f;
            shakeTimer += Time.deltaTime;
            yield return null;
        }
        if (visual == null) yield break;
        visual.transform.position = origin;

        SpriteRenderer sr = visual.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            float fadeDuration = 0.25f;
            float fadeTimer = 0;
            Color startColor = sr.color;
            Vector3 startScale = visual.transform.localScale;
            while (fadeTimer < fadeDuration)
            {
                if (visual == null) yield break;
                float t = fadeTimer / fadeDuration;
                sr.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
                visual.transform.localScale = startScale * (1f - t);
                fadeTimer += Time.deltaTime;
                yield return null;
            }
        }

        if (visual != null) Destroy(visual);
    }

    IEnumerator AnimateSpawn(GameObject visual)
    {
        if (visual == null) yield break;
        Vector3 targetScale = visual.transform.localScale;
        visual.transform.localScale = Vector3.zero;
        float duration = 0.35f;
        float t = 0;
        while (t < duration)
        {
            if (visual == null) yield break;
            float raw = t / duration;
            float scale = raw < 0.5f
                ? 2f * raw * raw
                : 1f - (-2f * raw + 2f) * (raw - 1f) * 0.5f;
            visual.transform.localScale = targetScale * Mathf.Clamp01(scale);
            t += Time.deltaTime;
            yield return null;
        }
        if (visual != null)
            visual.transform.localScale = targetScale;
    }

    IEnumerator AnimateHitImpact(GameObject visual)
    {
        if (visual == null) yield break;
        SpriteRenderer sr = visual.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;
        Color original = sr.color;
        sr.color = Color.white;
        Vector3 originalScale = visual.transform.localScale;
        visual.transform.localScale = originalScale * 1.25f;
        StartCoroutine(HitSparks(visual.transform.position, original));
        yield return new WaitForSeconds(0.06f);
        sr.color = original;
        visual.transform.localScale = originalScale;
    }

    Sprite CreateParticleSprite()
    {
        Texture2D tex = new Texture2D(8, 8);
        Color[] pixels = new Color[64];
        for (int i = 0; i < pixels.Length; i++)
        {
            int x = i % 8;
            int y = i / 8;
            float dx = (x + 0.5f) / 8f - 0.5f;
            float dy = (y + 0.5f) / 8f - 0.5f;
            pixels[i] = (dx * dx + dy * dy < 0.25f) ? Color.white : Color.clear;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8);
    }

    IEnumerator SpawnGlow(Vector3 pos)
    {
        GameObject glow = new GameObject("SpawnGlow");
        glow.transform.position = pos;
        SpriteRenderer sr = glow.AddComponent<SpriteRenderer>();
        sr.sprite = particleSprite;
        sr.color = new Color(1f, 1f, 1f, 0.5f);
        sr.sortingOrder = 2;

        float duration = 0.4f;
        float t = 0;
        while (t < duration)
        {
            float scale = 1f + t / duration * 3f;
            glow.transform.localScale = Vector3.one * scale;
            sr.color = new Color(1f, 1f, 1f, 0.5f * (1f - t / duration));
            t += Time.deltaTime;
            yield return null;
        }
        Destroy(glow);
    }

    IEnumerator HitSparks(Vector3 pos, Color color)
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject spark = new GameObject("HitSpark");
            spark.transform.position = pos;
            SpriteRenderer sr = spark.AddComponent<SpriteRenderer>();
            sr.sprite = particleSprite;
            sr.color = color;
            sr.sortingOrder = 3;

            Vector2 dir = Random.insideUnitCircle.normalized;
            float speed = Random.Range(1.5f, 3.5f);
            float duration = Random.Range(0.15f, 0.3f);
            float t = 0;
            while (t < duration)
            {
                spark.transform.position += (Vector3)dir * speed * Time.deltaTime;
                sr.color = new Color(color.r, color.g, color.b, 1f - t / duration);
                float s = 1f - t / duration;
                spark.transform.localScale = Vector3.one * s;
                t += Time.deltaTime;
                yield return null;
            }
            Destroy(spark);
        }
    }

    IEnumerator DeathPoof(Vector3 pos, Team team)
    {
        Color baseColor = team == Team.Blue ? new Color(0.4f, 0.6f, 1f) : new Color(1f, 0.4f, 0.4f);
        for (int i = 0; i < 5; i++)
        {
            GameObject puff = new GameObject("DeathPuff");
            puff.transform.position = pos + (Vector3)Random.insideUnitCircle * 0.1f;
            SpriteRenderer sr = puff.AddComponent<SpriteRenderer>();
            sr.sprite = particleSprite;
            sr.color = baseColor;
            sr.sortingOrder = 3;

            Vector2 dir = Random.insideUnitCircle.normalized;
            float speed = Random.Range(0.5f, 1.5f);
            float duration = Random.Range(0.2f, 0.4f);
            float t = 0;
            while (t < duration)
            {
                puff.transform.position += (Vector3)dir * speed * Time.deltaTime;
                sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f - t / duration);
                float s = 1f - t / duration;
                puff.transform.localScale = Vector3.one * s * 0.35f;
                t += Time.deltaTime;
                yield return null;
            }
            Destroy(puff);
        }
    }

    IEnumerator MoveTrail(GameObject visual, Vector3 fromPos, Vector3 toPos, Team team)
    {
        Color trailColor = team == Team.Blue ? new Color(0.4f, 0.6f, 1f, 0.3f) : new Color(1f, 0.4f, 0.4f, 0.3f);
        float duration = 0.6f;
        float t = 0;
        float nextSpawn = 0;
        while (t < duration)
        {
            if (visual == null) yield break;
            nextSpawn -= Time.deltaTime;
            if (nextSpawn <= 0)
            {
                nextSpawn = 0.1f;
                GameObject trail = new GameObject("MoveTrail");
                trail.transform.position = visual.transform.position;
                SpriteRenderer sr = trail.AddComponent<SpriteRenderer>();
                sr.sprite = particleSprite;
                sr.color = trailColor;
                sr.sortingOrder = 1;
                trail.transform.localScale = Vector3.one * 0.2f;
                StartCoroutine(FadeTrail(trail, 0.3f));
            }
            t += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator FadeTrail(GameObject obj, float duration)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        float t = 0;
        while (t < duration)
        {
            if (obj == null) yield break;
            Color c = sr.color;
            c.a = Mathf.Lerp(c.a, 0, t / duration);
            sr.color = c;
            t += Time.deltaTime;
            yield return null;
        }
        if (obj != null) Destroy(obj);
    }

    IEnumerator NinjaMoveCloud(Vector3 fromPos)
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject cloud = new GameObject("NinjaCloud");
            cloud.transform.position = fromPos + (Vector3)Random.insideUnitCircle * 0.2f;
            SpriteRenderer sr = cloud.AddComponent<SpriteRenderer>();
            sr.sprite = cloudSprite != null ? cloudSprite : particleSprite;
            sr.color = new Color(1f, 1f, 1f, 0.6f);
            sr.sortingOrder = 3;
            cloud.transform.localScale = Vector3.one * 0.2f;

            float dirX = Random.value > 0.5f ? 1f : -1f;
            float speed = Random.Range(0.6f, 1.2f);
            float duration = Random.Range(0.3f, 0.5f);
            float t = 0;
            while (t < duration)
            {
                cloud.transform.position += new Vector3(dirX, 0f, 0f) * speed * Time.deltaTime;
                sr.color = new Color(1f, 1f, 1f, 0.6f * (1f - t / duration));
                cloud.transform.localScale = Vector3.one * 0.2f * (1f + t / duration * 0.3f);
                t += Time.deltaTime;
                yield return null;
            }
            Destroy(cloud);
        }
    }

    IEnumerator PaladinAura(GameObject visual, Vector3 fromPos, Vector3 toPos, bool attacking)
    {
        int count = attacking ? 6 : 3;
        float life = attacking ? 0.5f : 0.3f;
        float size = attacking ? 1.2f : 0.8f;

        for (int i = 0; i < count; i++)
        {
            GameObject aura = new GameObject("PaladinAura");
            aura.transform.position = fromPos + (Vector3)Random.insideUnitCircle * 0.3f;
            SpriteRenderer sr = aura.AddComponent<SpriteRenderer>();
            sr.sprite = particleSprite;
            sr.color = new Color(1f, 0.9f, 0.5f, 0.7f);
            sr.sortingOrder = 3;

            Vector2 dir = Random.insideUnitCircle.normalized;
            float speed = Random.Range(0.3f, 0.8f);
            float t = 0;
            while (t < life)
            {
                aura.transform.position += (Vector3)dir * speed * Time.deltaTime;
                sr.color = new Color(1f, 0.9f, 0.5f, 0.7f * (1f - t / life));
                float s = 1f - t / life;
                aura.transform.localScale = Vector3.one * s * size;
                t += Time.deltaTime;
                yield return null;
            }
            Destroy(aura);
        }
    }

    IEnumerator PaladinLightBeam(GameObject visual)
    {
        if (visual == null) yield break;
        SoundManager.Instance.PlayHolyBeam();

        GameObject beam = new GameObject("LightBeam");
        SpriteRenderer beamSr = beam.AddComponent<SpriteRenderer>();
        beamSr.sprite = CreateRectSprite(0.45f, 6f, new Color(1f, 0.95f, 0.7f, 0.45f));
        beamSr.sortingOrder = 2;

        float fadeIn = 0.15f;
        float hold = 0.6f;
        float fadeOut = 0.3f;
        float t = 0;

        while (t < fadeIn)
        {
            if (beam == null || visual == null) yield break;
            beam.transform.position = visual.transform.position + Vector3.up * 3f;
            float p = t / fadeIn;
            beamSr.color = new Color(1f, 0.95f, 0.7f, 0.45f * p);
            beam.transform.localScale = Vector3.one * (0.5f + p * 0.5f);
            t += Time.deltaTime;
            yield return null;
        }

        t = 0;
        while (t < hold)
        {
            if (beam == null || visual == null) yield break;
            beam.transform.position = visual.transform.position + Vector3.up * 3f;
            float flicker = 1f + Mathf.Sin(t * 30f) * 0.08f;
            beamSr.color = new Color(1f, 0.95f, 0.7f, 0.45f * flicker);
            t += Time.deltaTime;
            yield return null;
        }

        t = 0;
        Vector3 lastPos = visual != null ? visual.transform.position : beam.transform.position - Vector3.up * 3f;
        while (t < fadeOut)
        {
            if (beam == null) yield break;
            if (visual != null) lastPos = visual.transform.position;
            beam.transform.position = lastPos + Vector3.up * 3f;
            float p = 1f - t / fadeOut;
            beamSr.color = new Color(1f, 0.95f, 0.7f, 0.45f * p);
            beam.transform.localScale = Vector3.one * (0.5f + p * 0.5f);
            t += Time.deltaTime;
            yield return null;
        }
        Vector3 finalPos = visual != null ? visual.transform.position : lastPos;
        if (beam != null) Destroy(beam);

        for (int i = 0; i < 6; i++)
        {
            GameObject spark = new GameObject("HolySpark");
            spark.transform.position = finalPos + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.2f, 0.5f), 0);
            SpriteRenderer sparkSr = spark.AddComponent<SpriteRenderer>();
            sparkSr.sprite = particleSprite;
            sparkSr.color = new Color(1f, 0.9f, 0.4f, 1f);
            sparkSr.sortingOrder = 3;
            Vector2 vel = new Vector2(Random.Range(-1f, 1f), Random.Range(1.5f, 3f));
            float life = Random.Range(0.3f, 0.6f);
            StartCoroutine(AnimateDustParticle(spark, sparkSr, vel, life));
        }
    }

    Sprite CreateRectSprite(float width, float height, Color color)
    {
        int w = Mathf.Max(1, Mathf.RoundToInt(width * 100f));
        int h = Mathf.Max(1, Mathf.RoundToInt(height * 100f));
        Texture2D tex = new Texture2D(w, h);
        Color[] pixels = new Color[w * h];
        for (int y = 0; y < h; y++)
        {
            float edgeFade = 1f;
            if (y < 8) edgeFade = (float)y / 8f;
            else if (y > h - 8) edgeFade = (float)(h - y) / 8f;
            for (int x = 0; x < w; x++)
            {
                float xf = 1f;
                float dx = ((float)x / w - 0.5f) * 2f;
                xf = 1f - dx * dx * 0.6f;
                pixels[y * w + x] = new Color(color.r, color.g, color.b, color.a * edgeFade * xf);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100);
    }

    IEnumerator SwordClashDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        SoundManager.Instance.PlaySwordClash();
    }

    IEnumerator CombatShake(float intensity)
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;
        Vector3 orig = cam.transform.position;
        float t = 0;
        while (t < 0.3f)
        {
            Vector3 offset = Random.insideUnitCircle * intensity;
            cam.transform.position = orig + new Vector3(offset.x, offset.y, 0);
            t += Time.deltaTime;
            yield return null;
        }
        cam.transform.position = orig;
    }

    public IEnumerator CheckVictoryAndEndTurn()
    {
        if (aiInProgress) yield break;

        Team? winner = CheckVictory();
        if (!winner.HasValue)
        {
            int blueAlive = CountAlive(Team.Blue);
            int redAlive = CountAlive(Team.Red);
            if (blueAlive == 0) winner = Team.Red;
            else if (redAlive == 0) winner = Team.Blue;
        }

        if (winner.HasValue)
        {
            if (suppressVictoryCheck) yield break;
            yield return new WaitForSeconds(0.3f);
            ScoreboardUI.Instance.Show();
        }
        else
        {
            FindFirstObjectByType<TurnManager>().EndTurn();
        }
    }

    Sprite CreateSquareSprite(float size, Color color)
    {
        Texture2D tex = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        for (int i = 0; i < pixels.Length; i++)
        {
            int x = i % 64;
            int y = i / 64;
            float margin = 64 * (1f - size) / 2f;
            if (x >= margin && x < 64 - margin && y >= margin && y < 64 - margin)
                pixels[i] = color;
            else
                pixels[i] = Color.clear;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
    }

    void SpawnConfetti(Vector3 position)
    {
        System.Random rng = new System.Random();
        for (int i = 0; i < 12; i++)
        {
            GameObject dot = new GameObject("Confetti");
            dot.transform.position = position;
            SpriteRenderer sr = dot.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite(0.5f, new Color(
                (float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble(), 1f));
            sr.sortingOrder = 200;
            Vector3 dir = new Vector3((float)(rng.NextDouble() - 0.5f), (float)(rng.NextDouble() - 0.5f), 0f).normalized;
            StartCoroutine(AnimateConfettiBurst(dot, dir));
        }
    }

    void SpawnSmoke(Vector3 position)
    {
        System.Random rng = new System.Random();
        for (int i = 0; i < 6; i++)
        {
            GameObject dot = new GameObject("Smoke");
            dot.transform.position = position;
            SpriteRenderer sr = dot.AddComponent<SpriteRenderer>();
            Color gray = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            sr.sprite = CreateSquareSprite(0.7f, gray);
            sr.sortingOrder = 200;
            Vector3 dir = new Vector3((float)(rng.NextDouble() - 0.5f), 1f + (float)rng.NextDouble(), 0f).normalized;
            StartCoroutine(AnimateSmokePuff(dot, dir));
        }
    }

    IEnumerator AnimateConfettiBurst(GameObject obj, Vector3 dir)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        float t = 0f;
        float duration = 0.6f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float dt = Time.deltaTime;
            obj.transform.position += dir * 4f * dt;
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f - (t / duration);
                sr.color = c;
            }
            yield return null;
        }
        Destroy(obj);
    }

    IEnumerator AnimateSmokePuff(GameObject obj, Vector3 dir)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        float t = 0f;
        float duration = 0.8f;
        Vector3 startScale = Vector3.one * 0.3f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float dt = Time.deltaTime;
            obj.transform.position += dir * 1.5f * dt;
            obj.transform.localScale = startScale * (1f + t * 3f);
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 0.8f * (1f - (t / duration));
                sr.color = c;
            }
            yield return null;
        }
        Destroy(obj);
    }

    void SpawnEmojis(CombatOutcome outcome, PieceData attacker, PieceData defender)
    {
        Team winnerTeam = outcome.result == CombatResult.AttackerWins ? attacker.team : defender.team;
        Team loserTeam = outcome.result == CombatResult.AttackerWins ? defender.team : attacker.team;

        Cell happyCell = GetRandomPiece(winnerTeam);
        Cell sadCell = GetRandomPiece(loserTeam);

        if (happyCell != null && happyCell.pieceVisual != null && happyCell.pieceData.HasValue)
        {
            Vector3 pos = happyCell.pieceVisual.transform.position + new Vector3(0, 0.8f, 0);
            GameObject emoji = CreateEmojiSprite(pos, true, happyCell.pieceData.Value.species);
            StartCoroutine(AnimateEmoji(emoji));
        }

        if (sadCell != null && sadCell.pieceVisual != null && sadCell.pieceData.HasValue)
        {
            Vector3 pos = sadCell.pieceVisual.transform.position + new Vector3(0, 0.8f, 0);
            GameObject emoji = CreateEmojiSprite(pos, false, sadCell.pieceData.Value.species);
            StartCoroutine(AnimateEmoji(emoji));
        }
    }

    Cell GetRandomPiece(Team team)
    {
        List<Cell> cells = new List<Cell>();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Cell cell = grid[r, c];
                if (cell.IsOccupied && cell.pieceData.HasValue && cell.pieceData.Value.team == team)
                    cells.Add(cell);
            }
        }
        if (cells.Count == 0) return null;
        return cells[Random.Range(0, cells.Count)];
    }

    GameObject CreateEmojiSprite(Vector3 position, bool happy, string species = "Human")
    {
        GameObject obj = new GameObject(happy ? "HappyEmoji" : "SadEmoji");
        obj.transform.position = position;
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 200;
        obj.transform.localScale = Vector3.one * 0.5f;

        string folderPath = $"Sprites/{species}/Emoji";
        Sprite[] loaded = Resources.LoadAll<Sprite>(folderPath);

        string emojiKey = species switch
        {
            "Human" => "human",
            "Orc" => "orc",
            "Beastfolk" => "beast",
            _ => species.ToLowerInvariant()
        };
        string[] emojiNames = happy
            ? new[] { $"emote{emojiKey}happy_0" }
            : new[] { $"emote{emojiKey}sad_0", $"emote{emojiKey}angry_0" };

        foreach (string en in emojiNames)
        {
            Sprite found = System.Array.Find(loaded, s => s.name == en);
            if (found != null)
            {
                sr.sprite = found;
                break;
            }
        }

        if (sr.sprite == null)
        {
            Color col = happy ? new Color(1f, 0.9f, 0.2f) : new Color(0.7f, 0.7f, 1f);
            sr.sprite = CreateSquareSprite(0.8f, col);
        }

        return obj;
    }

    IEnumerator AnimateEmoji(GameObject obj)
    {
        float duration = 2.5f;
        float t = 0f;
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        Vector3 baseScale = obj.transform.localScale;

        while (t < duration)
        {
            t += Time.deltaTime;
            float bounce = 1f + Mathf.Sin(t * 8f) * 0.08f;
            obj.transform.localScale = baseScale * bounce;
            obj.transform.position += new Vector3(0, 0.3f * Time.deltaTime, 0);

            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Clamp01(1f - (t / duration));
                sr.color = c;
            }
            yield return null;
        }

        Destroy(obj);
    }

    public void SpawnShadowPieceAt(int row, int col, PieceType type)
    {
        Cell cell = GetCell(row, col);
        if (cell == null || cell.IsOccupied) return;

        int id = nextId++;
        string species = scenarioTheme;
        PieceData data = new PieceData(id, Team.Red, type, species);
        bool oldShadow = isShadowPhase;
        isShadowPhase = true;
        GameObject visual = CreatePieceVisual(type, Team.Red, false, row);
        isShadowPhase = oldShadow;
        Vector3 pos = CellToWorld(row, col);
        visual.transform.position = pos;
        visual.transform.SetParent(transform);
        visual.name = $"Shadow_{type}_{id}";
        cell.SetPiece(data, visual);
        StartCoroutine(AnimateSpawn(visual));
    }

    public void ShowTutorialMidground()
    {
        Sprite[] mgSprites = Resources.LoadAll<Sprite>("Sprites/Human/Background/fondo3");
        Sprite mg = mgSprites != null && mgSprites.Length > 0 ? mgSprites[0] : null;
        if (mg == null)
        {
            mgSprites = Resources.LoadAll<Sprite>("Sprites/Human/Background/fondo2");
            mg = mgSprites != null && mgSprites.Length > 0 ? mgSprites[0] : null;
        }
        if (mg == null) return;

        GameObject mgObj = new GameObject("TutorialMidground");
        mgObj.transform.SetParent(scenarioContainer.transform);
        mgObj.transform.position = new Vector3(3.78f, -3.40f, 4);
        SpriteRenderer sr = mgObj.AddComponent<SpriteRenderer>();
        sr.sprite = mg;
        sr.sortingOrder = -8;
        sr.color = new Color(0.35f, 0.35f, 0.4f, 0.75f);

        mgObj.transform.localScale = new Vector3(0.98f, 0.67f, 1);

        CreateClouds();
    }

    public void DarkenScene()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        GameObject darkObj = new GameObject("SceneDarken");
        darkObj.transform.SetParent(scenarioContainer.transform);
        darkObj.transform.position = cam.transform.position + new Vector3(0, 0, 3);

        SpriteRenderer sr = darkObj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateFullScreenSprite();
        sr.color = new Color(0f, 0f, 0.05f, 0.35f);
        sr.sortingOrder = 0;

        float worldH = cam.orthographicSize * 2f;
        float worldW = worldH * cam.aspect;
        darkObj.transform.localScale = new Vector3(worldW, worldH, 1);
    }

    Sprite CreateFullScreenSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
    }

    public void ResetToNormalLayout()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Cell cell = grid[r, c];
                if (cell.IsOccupied && cell.pieceData?.team == Team.Blue)
                {
                    if (cell.pieceVisual != null) Destroy(cell.pieceVisual);
                    cell.ClearPiece();
                }
            }
        }

        foreach (var entry in InitialPosition.Entries)
        {
            if (entry.team != Team.Blue) continue;
            Cell cell = grid[entry.row, entry.col];
            if (cell.IsOccupied) continue;
            PlacePiece(entry.row, entry.col, entry.team, entry.type);
        }
    }

    public void OnKillEffect(Vector3 pos, Team deadTeam, PieceType deadType, bool isPowerUp)
    {
        try
        {
            int tier = deadType switch
            {
                PieceType.Pawn => 1,
                PieceType.Ninja => 2,
                PieceType.Knight => 3,
                PieceType.Paladin => 4,
                _ => 1
            };
            float intensity = 0.04f + tier * 0.02f;
            float duration = 0.1f + tier * 0.03f;
            StartCoroutine(KillCameraShake(intensity, duration));
            SpawnKillSparks(pos, deadTeam, tier);
            bool isBlueTurn = turnManager != null && turnManager.GetCurrentTeam() == Team.Blue;
            if (deadTeam == Team.Red && isBlueTurn) blueKillCombo++;
            if (blueKillCombo >= 2) ShowComboText(pos, blueKillCombo);
        }
        catch (System.Exception) { }
    }

    IEnumerator KillCameraShake(float intensity, float duration)
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;
        Vector3 orig = cam.transform.position;
        float t = 0;
        while (t < duration)
        {
            cam.transform.position = orig + (Vector3)Random.insideUnitCircle * intensity;
            t += Time.deltaTime;
            yield return null;
        }
        cam.transform.position = orig;
    }

    void SpawnKillSparks(Vector3 pos, Team team, int tier)
    {
        Color baseColor = team == Team.Blue ? new Color(0.5f, 0.7f, 1f) : new Color(1f, 0.5f, 0.5f);
        int count = 4 + tier * 3;
        for (int i = 0; i < count; i++)
        {
            GameObject spark = new GameObject("KillSpark");
            spark.transform.position = pos + (Vector3)Random.insideUnitCircle * 0.1f;
            SpriteRenderer sr = spark.AddComponent<SpriteRenderer>();
            sr.sprite = particleSprite;
            sr.color = Color.Lerp(baseColor, Color.white, Random.Range(0f, 0.4f));
            sr.sortingOrder = 200;
            Vector2 dir = Random.insideUnitCircle.normalized;
            float speed = Random.Range(2f, 5f) * (1f + tier * 0.2f);
            float life = Random.Range(0.15f, 0.35f);
            float size = Random.Range(0.08f, 0.18f) * (1f + tier * 0.1f);
            spark.transform.localScale = Vector3.one * size;
            StartCoroutine(AnimateKillSpark(spark, sr, dir, speed, life));
        }
    }

    IEnumerator AnimateKillSpark(GameObject obj, SpriteRenderer sr, Vector2 dir, float speed, float life)
    {
        float t = 0;
        Vector3 vel = (Vector3)dir * speed + Vector3.up * 2f;
        while (t < life)
        {
            if (obj == null) yield break;
            float dt = Time.deltaTime;
            obj.transform.position += vel * dt;
            vel.y -= 8f * dt;
            t += dt;
            float alpha = 1f - t / life;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
            sr.transform.localScale = Vector3.one * (1f - t / life * 0.5f);
            yield return null;
        }
        if (obj != null) Destroy(obj);
    }

    void ShowComboText(Vector3 pos, int combo)
    {
        Font font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        GameObject txtObj = new GameObject("ComboText");
        Vector3 worldPos = pos + Vector3.up * 0.7f;
        Canvas canvas = txtObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 201;
        RectTransform rt = txtObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 60);
        txtObj.transform.position = worldPos;
        txtObj.transform.localScale = Vector3.one * 0.005f;

        GameObject child = new GameObject("Text");
        child.transform.SetParent(txtObj.transform, false);
        UnityEngine.UI.Text txt = child.AddComponent<UnityEngine.UI.Text>();
        txt.font = font;
        txt.fontSize = 28;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = new Color(1f, 0.85f, 0f);
        txt.text = $"COMBO x{combo}!";
        RectTransform childRt = child.GetComponent<RectTransform>();
        childRt.anchorMin = Vector2.zero;
        childRt.anchorMax = Vector2.one;
        childRt.sizeDelta = Vector2.zero;

        StartCoroutine(AnimateComboText(txtObj, txt));
    }

    IEnumerator AnimateComboText(GameObject obj, UnityEngine.UI.Text txt)
    {
        float t = 0;
        float duration = 1.0f;
        Vector3 startPos = obj.transform.position;
        RectTransform rt = obj.GetComponent<RectTransform>();
        while (t < duration)
        {
            if (obj == null) yield break;
            float p = t / duration;
            obj.transform.position = startPos + Vector3.up * p * 1.2f;
            float s = 1f + Mathf.Sin(p * Mathf.PI) * 0.3f;
            obj.transform.localScale = Vector3.one * 0.005f * s;
            Color c = txt.color;
            c.a = 1f - p;
            txt.color = c;
            t += Time.deltaTime;
            yield return null;
        }
        if (obj != null) Destroy(obj);
    }

    public void SpawnPowerKillSparks(Vector3 pos, string element)
    {
        Color baseColor = element switch
        {
            "fire" => new Color(1f, 0.4f, 0f),
            "lightning" => new Color(1f, 0.9f, 0.2f),
            _ => new Color(1f, 0.5f, 0.1f)
        };
        int count = 14;
        for (int i = 0; i < count; i++)
        {
            GameObject spark = new GameObject("PowerSpark");
            spark.transform.position = pos + (Vector3)Random.insideUnitCircle * 0.15f;
            SpriteRenderer sr = spark.AddComponent<SpriteRenderer>();
            sr.sprite = particleSprite;
            sr.color = Color.Lerp(baseColor, Color.white, Random.Range(0f, 0.3f));
            sr.sortingOrder = 200;
            Vector2 dir = Random.insideUnitCircle.normalized;
            float speed = Random.Range(3f, 7f);
            float life = Random.Range(0.2f, 0.45f);
            float size = Random.Range(0.1f, 0.22f);
            spark.transform.localScale = Vector3.one * size;
            StartCoroutine(AnimateKillSpark(spark, sr, dir, speed, life));
        }
    }
}
