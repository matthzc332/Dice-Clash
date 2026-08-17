using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerUpType
{
    Shake,
    Explosion,
    Fireball,
    Lightning,
    MAGIC
}

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }
    public bool IsExecuting { get; private set; }
    private BoardManager board;
    private Sprite lightningEffectSprite;

    private class ActivePowerUp
    {
        public int row;
        public int col;
        public PowerUpType type;
        public GameObject container;
        public GameObject icon;
        public GameObject glow;
    }

    private List<ActivePowerUp> spawnedPowerUps = new();
    private Coroutine autoSpawnCoroutine;
    private Sprite[] allIcons;
    private Sprite circleSprite;
    private Sprite glowSprite;
    private Sprite magoIdleSprite;
    private Sprite magoAttackSprite;
    private Sprite magoBackSprite;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        board = FindFirstObjectByType<BoardManager>();
        Sprite[] electricidadSprites = Resources.LoadAll<Sprite>("Sprites/PowerUps/Efect/electricidad");
        lightningEffectSprite = electricidadSprites != null && electricidadSprites.Length > 0 ? electricidadSprites[0] : null;
        allIcons = Resources.LoadAll<Sprite>("Sprites/PowerUps/Icon");
        if (allIcons == null || allIcons.Length == 0)
        {
            Texture2D[] texs = Resources.LoadAll<Texture2D>("Sprites/PowerUps/Icon");
            if (texs != null && texs.Length > 0)
            {
                allIcons = new Sprite[texs.Length];
                for (int i = 0; i < texs.Length; i++)
                    allIcons[i] = Sprite.Create(texs[i], new Rect(0, 0, texs[i].width, texs[i].height), new Vector2(0.5f, 0.5f));
            }
        }

        magoIdleSprite = Resources.Load<Sprite>("Sprites/PowerUps/Efect/magoIdle");
        magoAttackSprite = Resources.Load<Sprite>("Sprites/PowerUps/Efect/magoAttack");
        magoBackSprite = Resources.Load<Sprite>("Sprites/PowerUps/Efect/magoback");

        glowSprite = Resources.Load<Sprite>("Sprites/PowerUps/Icon/cambio");
        if (glowSprite == null)
        {
            Sprite[] loaded = Resources.LoadAll<Sprite>("Sprites/PowerUps/Icon");
            if (loaded != null)
                foreach (var s in loaded)
                    if (s.name == "cambio") { glowSprite = s; break; }
        }
    }

    void Update()
    {
        if (board == null) return;
        if (GameConfig.isTutorial && !board.isShadowPhase) return;
        CheckCollectionForTeam(Team.Blue);
        CheckCollectionForTeam(Team.Red);
    }

    Sprite GetGlowSprite()
    {
        if (glowSprite != null) return glowSprite;
        return GetCircleSprite();
    }

    Sprite GetCircleSprite()
    {
        if (circleSprite == null)
            circleSprite = CreateCircleSprite();
        return circleSprite;
    }

    public static Color GetColor(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.Shake: return new Color(0.2f, 0.8f, 0.2f);
            case PowerUpType.Explosion: return new Color(1f, 0.5f, 0f);
            case PowerUpType.Fireball: return new Color(1f, 0.15f, 0.05f);
            case PowerUpType.Lightning: return new Color(1f, 0.9f, 0.05f);
            case PowerUpType.MAGIC: return new Color(0.6f, 0.2f, 1f);
            default: return Color.white;
        }
    }

    public static string GetName(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.Shake: return "SHAKE!";
            case PowerUpType.Explosion: return "BOOM!";
            case PowerUpType.Fireball: return "FIREBALL!";
            case PowerUpType.Lightning: return "LIGHTNING!";
            case PowerUpType.MAGIC: return "MAGIC!";
            default: return "";
        }
    }

    public void Execute(PowerUpType type, int sourceRow, int sourceCol)
    {
        Execute(type, sourceRow, sourceCol, Team.Blue);
    }

    public void Execute(PowerUpType type, int sourceRow, int sourceCol, Team collectingTeam)
    {
        IsExecuting = true;
        switch (type)
        {
            case PowerUpType.Shake: StartCoroutine(ShakeEffect(collectingTeam)); break;
            case PowerUpType.Explosion: StartCoroutine(ExplosionEffect(sourceRow, sourceCol, collectingTeam)); break;
            case PowerUpType.Fireball: StartCoroutine(FireballEffect(collectingTeam)); break;
            case PowerUpType.Lightning: StartCoroutine(LightningEffect(sourceRow, sourceCol, collectingTeam)); break;
            case PowerUpType.MAGIC: StartCoroutine(MagicEffect(collectingTeam)); break;
        }
        StartCoroutine(ReleaseExecutingAfterDelay(2f));
    }

    IEnumerator ReleaseExecutingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        IsExecuting = false;
    }

    bool IsTypeSpawned(PowerUpType type)
    {
        foreach (var pu in spawnedPowerUps)
            if (pu.type == type && pu.container != null)
                return true;
        return false;
    }

    public void SpawnSpecificAt(int row, int col, PowerUpType type)
    {
        if (board == null) return;
        if (IsPowerUpAt(row, col)) return;

        Color color = GetColor(type);
        ActivePowerUp pu = new ActivePowerUp();
        pu.row = row;
        pu.col = col;
        pu.type = type;

        Vector3 wPos = board.CellToWorld(row, col);
        wPos.y -= 0.05f;

        pu.container = new GameObject($"SpawnedPowerUp_{type}");
        pu.container.transform.position = wPos;

        Sprite iconSprite = null;
        if (allIcons != null)
            foreach (var s in allIcons)
                if (s.name == type.ToString() || s.name == type.ToString() + "_0") { iconSprite = s; break; }
        if (iconSprite == null)
            iconSprite = CreatePowerUpIcon(type, color);

        pu.icon = new GameObject("Icon");
        pu.icon.transform.SetParent(pu.container.transform, false);
        SpriteRenderer sr = pu.icon.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 14;
        sr.sprite = iconSprite;
        sr.color = Color.white;
        pu.icon.transform.localScale = Vector3.one * 0.25f;

        pu.glow = new GameObject("Glow");
        pu.glow.transform.SetParent(pu.container.transform, false);
        SpriteRenderer gsr = pu.glow.AddComponent<SpriteRenderer>();
        gsr.sprite = GetGlowSprite();
        gsr.color = new Color(color.r, color.g, color.b, 0.25f);
        gsr.sortingOrder = 13;
        pu.glow.transform.localScale = Vector3.one * 0.9f;

        spawnedPowerUps.Add(pu);
        StartCoroutine(AnimateSpawned(pu, color));
    }

    public void SpawnOnBoard()
    {
        if (board == null) return;

        PowerUpType[] allTypes = new[] { PowerUpType.Shake, PowerUpType.Explosion, PowerUpType.Fireball, PowerUpType.Lightning, PowerUpType.MAGIC };

        PowerUpType[] types;
        if (GameConfig.isCampaign && GameConfig.selectedLevel > 0)
        {
            CampaignLevel level = CampaignData.GetLevel(GameConfig.selectedLevel);
            if (level != null && level.powerups != null && level.powerups.Length > 0)
            {
                List<PowerUpType> campaignTypes = new List<PowerUpType>();
                foreach (string puName in level.powerups)
                {
                    if (System.Enum.TryParse<PowerUpType>(puName, true, out PowerUpType puType))
                        campaignTypes.Add(puType);
                }
                types = campaignTypes.Count > 0 ? campaignTypes.ToArray() : allTypes;
            }
            else
                types = allTypes;
        }
        else
            types = allTypes;

        System.Collections.Generic.List<PowerUpType> available = new System.Collections.Generic.List<PowerUpType>();
        foreach (var t in types)
            if (!IsTypeSpawned(t)) available.Add(t);
        if (available.Count == 0) return;

        int maxAttempts = 30;
        for (int a = 0; a < maxAttempts; a++)
        {
            int r = Random.Range(0, board.rows);
            int c = Random.Range(0, board.cols);
            Cell cell = board.GetCell(r, c);
            if (cell == null || cell.IsOccupied || cell.isObstacle) continue;
            if (IsPowerUpAt(r, c)) continue;

            PowerUpType type = available[Random.Range(0, available.Count)];
            Color color = GetColor(type);

            ActivePowerUp pu = new ActivePowerUp();
            pu.row = r;
            pu.col = c;
            pu.type = type;

            Vector3 wPos = board.CellToWorld(r, c);
            wPos.y -= 0.05f;

            pu.container = new GameObject($"SpawnedPowerUp_{type}");
            pu.container.transform.position = wPos;

            Sprite iconSprite = null;
            if (allIcons != null)
                foreach (var s in allIcons)
                    if (s.name == type.ToString() || s.name == type.ToString() + "_0") { iconSprite = s; break; }
            if (iconSprite == null)
                iconSprite = CreatePowerUpIcon(type, color);

            pu.icon = new GameObject("Icon");
            pu.icon.transform.SetParent(pu.container.transform, false);
            SpriteRenderer sr = pu.icon.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 14;
            sr.sprite = iconSprite;
            sr.color = Color.white;
            pu.icon.transform.localScale = Vector3.one * 0.25f;

            pu.glow = new GameObject("Glow");
            pu.glow.transform.SetParent(pu.container.transform, false);
            SpriteRenderer gsr = pu.glow.AddComponent<SpriteRenderer>();
            gsr.sprite = GetGlowSprite();
            gsr.color = new Color(color.r, color.g, color.b, 0.25f);
            gsr.sortingOrder = 13;
            pu.glow.transform.localScale = Vector3.one * 0.9f;

            spawnedPowerUps.Add(pu);
            StartCoroutine(AnimateSpawned(pu, color));

            if (autoSpawnCoroutine != null)
                StopCoroutine(autoSpawnCoroutine);
            autoSpawnCoroutine = StartCoroutine(AutoSpawnTimer());
            break;
        }
    }

    IEnumerator AnimateSpawned(ActivePowerUp pu, Color color)
    {
        Vector3 basePos = pu.container.transform.position;
        float t = 0;
        while (pu.container != null)
        {
            float floatOff = Mathf.Sin(t * 2f) * 0.06f;
            pu.container.transform.position = new Vector3(basePos.x, basePos.y + floatOff, basePos.z);

            if (pu.icon != null)
            {
                float pulse = 1f + Mathf.Sin(t * 3f) * 0.1f;
                pu.icon.transform.localScale = Vector3.one * 0.25f * pulse;
            }

            if (pu.glow != null)
            {
                SpriteRenderer gsr = pu.glow.GetComponent<SpriteRenderer>();
                if (gsr != null)
                {
                    float glowAlpha = 0.15f + Mathf.Sin(t * 2.5f) * 0.1f;
                    gsr.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(glowAlpha));
                    float glowScale = 0.55f + Mathf.Sin(t * 1.8f) * 0.1f;
                    pu.glow.transform.localScale = Vector3.one * glowScale;
                }
            }

            t += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator AutoSpawnTimer()
    {
        int alive = 0;
        for (int r = 0; r < board.rows; r++)
            for (int c = 0; c < board.cols; c++)
                if (board.grid[r, c].IsOccupied)
                    alive++;

        float timeLeft = TimerManager.Instance != null ? TimerManager.Instance.timeRemaining : 300f;
        float delay;
        if (timeLeft > 180f)
            delay = Random.Range(10f, 18f);
        else if (timeLeft > 60f)
            delay = Random.Range(8f, 15f);
        else
            delay = Random.Range(6f, 12f);
        delay = Mathf.Max(delay, 4f);
        yield return new WaitForSeconds(delay);
        SpawnOnBoard();
    }

    public bool IsPowerUpAt(int r, int c)
    {
        foreach (var pu in spawnedPowerUps)
            if (pu.row == r && pu.col == c && pu.container != null)
                return true;
        return false;
    }

    public void CheckCollectionForTeam(Team team)
    {
        for (int i = spawnedPowerUps.Count - 1; i >= 0; i--)
        {
            var pu = spawnedPowerUps[i];
            if (pu.container == null) { spawnedPowerUps.RemoveAt(i); continue; }

            Cell cell = board.GetCell(pu.row, pu.col);
            if (cell != null && cell.IsOccupied && cell.pieceData?.team == team)
            {
                PieceData collector = cell.pieceData.Value;
                if (pu.container != null) Destroy(pu.container);
                string name = GetName(pu.type);
                spawnedPowerUps.RemoveAt(i);

                if (AutoPlayStats.Instance != null)
                {
                    AutoPlayStats.Instance.LogPowerUp(
                        pu.type.ToString(), pu.row, pu.col,
                        team.ToString(), collector.type.ToString(),
                        (team == Team.Blue ? "Red" : "Blue"), "", "collected");
                }

                if (team == Team.Blue && GameConfig.currentPowerupMode == PowerupMode.WithoutPowerups && !board.isShadowPhase)
                    continue;

                Execute(pu.type, pu.row, pu.col, team);
            }
        }
    }

    public void ClearAllSpawned()
    {
        foreach (var pu in spawnedPowerUps)
            if (pu.container != null) Destroy(pu.container);
        spawnedPowerUps.Clear();
        if (autoSpawnCoroutine != null)
        {
            StopCoroutine(autoSpawnCoroutine);
            autoSpawnCoroutine = null;
        }
    }

    IEnumerator ShakeEffect(Team collectingTeam)
    {
        SoundManager.Instance.PlayFireRayo();
        SoundManager.Instance.PlayTemblor();
        Team enemyTeam = collectingTeam == Team.Blue ? Team.Red : Team.Blue;
        List<(int r, int c)> enemyCells = new();
        for (int r = 0; r < board.rows; r++)
            for (int c = 0; c < board.cols; c++)
            {
                Cell cell = board.GetCell(r, c);
                if (cell != null && cell.IsOccupied && cell.pieceData?.team == enemyTeam)
                    enemyCells.Add((r, c));
            }

        Vector3 boardCenter = board.CellToWorld(board.rows / 2, board.cols / 2);
        for (int i = 0; i < 30; i++)
            StartCoroutine(SparkFade(boardCenter + (Vector3)Random.insideUnitCircle * 4f));

        CameraShake(0.2f, 0.45f);

        foreach (var (r, c) in enemyCells)
        {
            Cell cell = board.GetCell(r, c);
            if (cell == null || !cell.IsOccupied) continue;

            List<(int dr, int dc)> dirs = new()
            {
                (0, 1), (0, -1), (1, 0), (-1, 0),
                (1, 1), (1, -1), (-1, 1), (-1, -1)
            };

            for (int i = dirs.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                var tmp = dirs[i]; dirs[i] = dirs[j]; dirs[j] = tmp;
            }

            foreach (var dir in dirs)
            {
                int nr = r + dir.dr;
                int nc = c + dir.dc;
                Cell target = board.GetCell(nr, nc);
                if (target != null && !target.IsOccupied && !target.BlocksMovement)
                {
                    cell = board.GetCell(r, c);
                    if (cell == null || !cell.IsOccupied) continue;
                    Vector3 fromPos = board.CellToWorld(r, c);
                    Vector3 toPos = board.CellToWorld(nr, nc);
                    GameObject visual = cell.pieceVisual;
                    PieceData data = cell.pieceData.Value;
                    cell.ClearPiece();
                    target.SetPiece(data, visual);

                    StartCoroutine(PushSlide(visual, fromPos, toPos, 0.25f));
                    StartCoroutine(ShakePuff(fromPos));
                    StartCoroutine(DirtChunks(fromPos, 5));
                    StartCoroutine(SmokeBurst(fromPos, 3));
                    for (int s = 0; s < 6; s++)
                        StartCoroutine(SparkFade(fromPos + (Vector3)Random.insideUnitCircle * 0.4f));
                    break;
                }
            }
        }

        yield return new WaitForSeconds(0.6f);
        SoundManager.Instance.PlayMove();
    }

    IEnumerator SparkFade(Vector3 pos)
    {
        GameObject spark = new GameObject("TemblorSpark");
        spark.transform.position = pos;
        SpriteRenderer sr = spark.AddComponent<SpriteRenderer>();
        sr.sprite = GetCircleSprite();
        sr.color = new Color(1f, 0.8f + Random.value * 0.2f, 0f, 1f);
        sr.sortingOrder = 12;
        spark.transform.localScale = Vector3.one * Random.Range(0.08f, 0.15f);
        Vector3 vel = new Vector3(Random.Range(-2f, 2f), Random.Range(1f, 3f), 0);
        float life = Random.Range(0.3f, 0.6f);
        float t = 0;
        while (t < life)
        {
            if (spark == null) yield break;
            spark.transform.position += vel * Time.deltaTime;
            vel.y -= 4f * Time.deltaTime;
            if (sr != null) { Color c = sr.color; c.a = Mathf.Lerp(1f, 0f, t / life); sr.color = c; }
            t += Time.deltaTime;
            yield return null;
        }
        if (spark != null) Destroy(spark);
    }

    IEnumerator PushSlide(GameObject visual, Vector3 from, Vector3 to, float duration)
    {
        float t = 0;
        while (t < duration)
        {
            if (visual == null) yield break;
            float raw = t / duration;
            visual.transform.position = Vector3.Lerp(from, to, raw * raw * (3f - 2f * raw));
            t += Time.deltaTime;
            yield return null;
        }
        if (visual != null) visual.transform.position = to;
    }

    IEnumerator ShakePuff(Vector3 pos)
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject p = new GameObject("ShakePuff");
            p.transform.position = pos + (Vector3)Random.insideUnitCircle * 0.15f;
            SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
            sr.sprite = GetCircleSprite();
            sr.color = new Color(0.25f, 0.75f, 0.25f, 0.6f);
            sr.sortingOrder = 8;
            Vector3 dir = Random.insideUnitCircle.normalized * Random.Range(1f, 2.5f);
            StartCoroutine(FlyFade(p, dir, 0.3f));
        }
        yield break;
    }

    IEnumerator ExplosionEffect(int centerRow, int centerCol, Team collectingTeam)
    {
        Team enemyTeam = collectingTeam == Team.Blue ? Team.Red : Team.Blue;
        Vector3 centerPos = board.CellToWorld(centerRow, centerCol);

        GameObject aoe = new GameObject("ExplosionAOE");
        aoe.transform.position = centerPos;
        SpriteRenderer aoeSr = aoe.AddComponent<SpriteRenderer>();
        aoeSr.sprite = GetCircleSprite();
        aoeSr.color = new Color(1f, 0.5f, 0f, 0.2f);
        aoeSr.sortingOrder = -1;

        GameObject ring = new GameObject("ExplosionRing");
        ring.transform.position = centerPos;
        SpriteRenderer ringSr = ring.AddComponent<SpriteRenderer>();
        ringSr.sprite = CreateRingSprite();
        ringSr.color = new Color(1f, 0.7f, 0f, 0.6f);
        ringSr.sortingOrder = -1;

        float anim = 0;
        while (anim < 0.3f)
        {
            float s = anim / 0.3f * 3f;
            aoe.transform.localScale = Vector3.one * s;
            ring.transform.localScale = Vector3.one * s;
            anim += Time.deltaTime;
            yield return null;
        }
        aoe.transform.localScale = Vector3.one * 3f;
        ring.transform.localScale = Vector3.one * 3f;

        yield return new WaitForSeconds(0.15f);

        Destroy(aoe);
        Destroy(ring);

        StartCoroutine(LightingFlash(centerPos));
        StartCoroutine(ExplosionBurst(centerPos));
        StartCoroutine(SmokeBurst(centerPos, 8));
        StartCoroutine(DirtChunks(centerPos, 10));
        SoundManager.Instance.PlayExplosion();
        SoundManager.Instance.PlayHit();

        yield return new WaitForSeconds(0.15f);

        for (int r = -1; r <= 1; r++)
        {
            for (int c = -1; c <= 1; c++)
            {
                int nr = centerRow + r;
                int nc = centerCol + c;
                Cell cell = board.GetCell(nr, nc);
                if (cell == null) continue;
                if (cell.IsOccupied && cell.pieceData?.team == enemyTeam)
                {
                    PieceType killedType = cell.pieceData.Value.type;
                    GameObject visual = cell.pieceVisual;
                    Vector3 killPos = board.CellToWorld(nr, nc);
                    cell.ClearPiece();
                    StartCoroutine(ExplosionDestroy(visual));
                    if (CoinManager.Instance != null)
                        CoinManager.Instance.AwardKill(collectingTeam, killPos, killedType, true);
                    if (board != null)
                    {
                        board.OnKillEffect(killPos, enemyTeam, killedType, true);
                        board.SpawnPowerKillSparks(killPos, "fire");
                    }
                }
            }
        }

        CameraShake(0.15f, 0.3f);

        BurnTiles(centerRow, centerCol);
    }

    void BurnTiles(int centerRow, int centerCol)
    {
        for (int r = -1; r <= 1; r++)
        {
            for (int c = -1; c <= 1; c++)
            {
                int nr = centerRow + r;
                int nc = centerCol + c;
                Cell cell = board.GetCell(nr, nc);
                if (cell == null) continue;

                Vector3 pos = board.CellToWorld(nr, nc);
                GameObject burn = new GameObject("BurnTile");
                burn.transform.SetParent(board.transform);
                burn.transform.position = pos;
                burn.transform.localScale = Vector3.one * board.cellSize * 0.95f;

                SpriteRenderer sr = burn.AddComponent<SpriteRenderer>();
                sr.sprite = GetCircleSprite();
                sr.color = new Color(0.15f, 0.08f, 0.02f, 0.65f);
                sr.sortingOrder = -1;

                StartCoroutine(FadeBurn(burn, sr, 3f));
            }
        }
    }

    IEnumerator FadeBurn(GameObject burn, SpriteRenderer sr, float duration)
    {
        yield return new WaitForSeconds(duration);

        float fadeTime = 0.6f;
        float t = 0;
        while (t < fadeTime)
        {
            if (sr == null) yield break;
            Color c = sr.color;
            c.a = Mathf.Lerp(0.65f, 0f, t / fadeTime);
            sr.color = c;
            t += Time.deltaTime;
            yield return null;
        }
        if (burn != null) Destroy(burn);
    }

    IEnumerator ExplosionBurst(Vector3 pos)
    {
        for (int i = 0; i < 20; i++)
        {
            GameObject p = new GameObject("ExplodeParticle");
            p.transform.position = pos + (Vector3)Random.insideUnitCircle * 0.2f;
            SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
            sr.sprite = GetCircleSprite();
            sr.color = new Color(1f, 0.5f + Random.value * 0.3f, 0f, 1f);
            sr.sortingOrder = 0;
            Vector3 dir = Random.insideUnitCircle.normalized * Random.Range(2f, 5f);
            float life = Random.Range(0.15f, 0.35f);
            StartCoroutine(FlyFade(p, dir, life));
        }
        yield break;
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
            visual.transform.localScale = Vector3.Lerp(Vector3.one * 0.5f, Vector3.one * 1.3f, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        if (visual != null) Destroy(visual);
    }

    IEnumerator FireballEffect(Team collectingTeam)
    {
        Team enemyTeam = collectingTeam == Team.Blue ? Team.Red : Team.Blue;
        int targetRow = -1, targetCol = -1;
        float minDist = float.MaxValue;

        for (int r = 0; r < board.rows; r++)
        {
            for (int c = 0; c < board.cols; c++)
            {
                Cell cell = board.GetCell(r, c);
                if (cell == null || !cell.IsOccupied) continue;
                if (cell.pieceData?.team != enemyTeam) continue;

                for (int fr = 0; fr < board.rows; fr++)
                {
                    for (int fc = 0; fc < board.cols; fc++)
                    {
                        Cell friendly = board.GetCell(fr, fc);
                        if (friendly == null || !friendly.IsOccupied) continue;
                        if (friendly.pieceData?.team != collectingTeam) continue;

                        float d = Vector3.Distance(board.CellToWorld(r, c), board.CellToWorld(fr, fc));
                        if (d < minDist) { minDist = d; targetRow = r; targetCol = c; }
                    }
                }
            }
        }

        if (targetRow < 0) yield break;

        Vector3 targetPos = board.CellToWorld(targetRow, targetCol);
        Vector3 originPos = targetPos;
        float closestFriendly = float.MaxValue;
        for (int fr = 0; fr < board.rows; fr++)
        {
            for (int fc = 0; fc < board.cols; fc++)
            {
                Cell fc2 = board.GetCell(fr, fc);
                if (fc2 == null || !fc2.IsOccupied) continue;
                if (fc2.pieceData?.team != collectingTeam) continue;
                float d = Vector3.Distance(board.CellToWorld(fr, fc), targetPos);
                if (d < closestFriendly) { closestFriendly = d; originPos = board.CellToWorld(fr, fc); }
            }
        }

        yield return StartCoroutine(MageAndProjectile(originPos, targetPos));

        SoundManager.Instance.PlayFireRayo();
        SoundManager.Instance.PlayHit();
        CameraShake(0.1f, 0.2f);

        Cell tc = board.GetCell(targetRow, targetCol);
        if (tc != null && tc.IsOccupied && tc.pieceVisual != null)
        {
            PieceType killedType = tc.pieceData.Value.type;
            GameObject vis = tc.pieceVisual;
            Vector3 killPos = board.CellToWorld(targetRow, targetCol);
            tc.ClearPiece();
            StartCoroutine(FireDestroy(vis));
            if (CoinManager.Instance != null)
                CoinManager.Instance.AwardKill(collectingTeam, killPos, killedType, true);
            if (board != null)
            {
                board.OnKillEffect(killPos, enemyTeam, killedType, true);
                board.SpawnPowerKillSparks(killPos, "fire");
            }
        }
    }

    IEnumerator MageAndProjectile(Vector3 originPos, Vector3 targetPos)
    {
        Sprite mageSprite = magoIdleSprite ?? magoAttackSprite ?? magoBackSprite;
        if (mageSprite == null)
        {
            yield return StartCoroutine(FireballProjectile(originPos, targetPos));
            yield break;
        }

        Vector3 magePos = GetRandomEmptyCellWorldPos();
        if (magePos == Vector3.zero) magePos = originPos;

        GameObject mage = new GameObject("Mage");
        mage.transform.position = magePos;
        SpriteRenderer mageSr = mage.AddComponent<SpriteRenderer>();
        mageSr.sortingOrder = 15;

        mageSr.sprite = magoIdleSprite ?? mageSprite;
        if (magoIdleSprite == null) mage.transform.localScale = Vector3.one * 0.2f;
        else mage.transform.localScale = Vector3.one * 0.35f;

        if (magoAttackSprite != null)
        {
            mageSr.sprite = magoAttackSprite;
            mageSr.sortingOrder = 15;
        }

        SoundManager.Instance.PlayFireball();
        yield return new WaitForSeconds(0.4f);

        yield return StartCoroutine(FireballProjectile(magePos, targetPos));

        if (mage != null && mageSr != null)
        {
            if (magoBackSprite != null)
            {
                mageSr.sprite = magoBackSprite;
                mageSr.sortingOrder = 14;
                float fade = 0.25f;
                float ft = 0;
                while (ft < fade)
                {
                    if (mage == null) yield break;
                    Color c = mageSr.color;
                    c.a = 1f - (ft / fade);
                    mageSr.color = c;
                    ft += Time.deltaTime;
                    yield return null;
                }
            }
            if (mage != null) Destroy(mage);
        }
    }

    Vector3 GetRandomEmptyCellWorldPos()
    {
        if (board == null) return Vector3.zero;
        for (int attempt = 0; attempt < 20; attempt++)
        {
            int r = Random.Range(0, board.rows);
            int c = Random.Range(0, board.cols);
            Cell cell = board.GetCell(r, c);
            if (cell != null && !cell.IsOccupied && !cell.BlocksMovement)
                return board.CellToWorld(r, c);
        }
        return Vector3.zero;
    }

    IEnumerator FireballProjectile(Vector3 from, Vector3 to)
    {
        GameObject fb = new GameObject("Fireball");
        fb.transform.position = from;
        SpriteRenderer fbSr = fb.AddComponent<SpriteRenderer>();
        fbSr.sprite = GetCircleSprite();
        fbSr.color = new Color(1f, 0.3f, 0.05f, 1f);
        fbSr.sortingOrder = 15;
        fb.transform.localScale = Vector3.one * 0.45f;

        float duration = 0.35f;
        float t = 0;
        while (t < duration)
        {
            if (fb == null) yield break;
            Vector3 pos = Vector3.Lerp(from, to, t / duration);
            fb.transform.position = pos;

            GameObject tr = new GameObject("FireTrail");
            tr.transform.position = pos;
            SpriteRenderer trSr = tr.AddComponent<SpriteRenderer>();
            trSr.sprite = GetCircleSprite();
            float trailBright = 0.5f + Mathf.Sin(t * 20f) * 0.3f;
            trSr.color = new Color(1f, 0.6f * trailBright, 0.1f, 0.5f);
            trSr.sortingOrder = 14;
            tr.transform.localScale = Vector3.one * 0.25f;
            StartCoroutine(FadeAndDie(tr, 0.3f));

            if (Random.value < 0.4f)
            {
                GameObject smoke = new GameObject("FireSmoke");
                smoke.transform.position = pos + (Vector3)Random.insideUnitCircle * 0.1f;
                SpriteRenderer smSr = smoke.AddComponent<SpriteRenderer>();
                smSr.sprite = GetCircleSprite();
                smSr.color = new Color(0.3f, 0.3f, 0.3f, 0.4f);
                smSr.sortingOrder = 14;
                smoke.transform.localScale = Vector3.one * 0.12f;
                Vector3 smokeDir = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0.3f, 0.8f), 0);
                StartCoroutine(FlyFade(smoke, smokeDir, 0.4f));
            }

            t += Time.deltaTime;
            yield return null;
        }

        if (fb != null) Destroy(fb);

        StartCoroutine(LightingFlash(to));
        StartCoroutine(SmokeBurst(to, 4));
        StartCoroutine(DirtChunks(to, 6));

        for (int i = 0; i < 15; i++)
        {
            GameObject p = new GameObject("FireImpact");
            p.transform.position = to + (Vector3)Random.insideUnitCircle * 0.25f;
            SpriteRenderer psr = p.AddComponent<SpriteRenderer>();
            psr.sprite = GetCircleSprite();
            psr.color = new Color(1f, 0.3f + Random.value * 0.3f, 0f, 1f);
            psr.sortingOrder = 14;
            Vector3 dir = Random.insideUnitCircle.normalized * Random.Range(1.5f, 4f);
            StartCoroutine(FlyFade(p, dir, 0.3f));
        }
    }

    IEnumerator LightningEffect(int sourceRow, int sourceCol, Team collectingTeam)
    {
        Team enemyTeam = collectingTeam == Team.Blue ? Team.Red : Team.Blue;
        Vector3 origin = board.CellToWorld(sourceRow, sourceCol);

        int targetRow = -1, targetCol = -1;
        float minDist = float.MaxValue;

        for (int r = 0; r < board.rows; r++)
        {
            for (int c = 0; c < board.cols; c++)
            {
                Cell cell = board.GetCell(r, c);
                if (cell == null || !cell.IsOccupied) continue;
                if (cell.pieceData?.team != enemyTeam) continue;

                float d = Vector3.Distance(board.CellToWorld(r, c), origin);
                if (d < minDist) { minDist = d; targetRow = r; targetCol = c; }
            }
        }

        if (targetRow < 0) yield break;

        Vector3 targetPos = board.CellToWorld(targetRow, targetCol);

        yield return StartCoroutine(MageLightning(origin, targetPos));

        SoundManager.Instance.PlayFireRayo();
        StartCoroutine(LightingFlash(targetPos));
        StartCoroutine(SmokeBurst(targetPos, 5));
        StartCoroutine(DirtChunks(targetPos, 8));
        SoundManager.Instance.PlayHit();
        CameraShake(0.12f, 0.25f);

        Cell tc = board.GetCell(targetRow, targetCol);
        if (tc != null && tc.IsOccupied && tc.pieceVisual != null)
        {
            PieceType killedType = tc.pieceData.Value.type;
            GameObject vis = tc.pieceVisual;
            Vector3 killPos = board.CellToWorld(targetRow, targetCol);
            tc.ClearPiece();
            StartCoroutine(LightningDestroy(vis));
            if (CoinManager.Instance != null)
                CoinManager.Instance.AwardKill(collectingTeam, killPos, killedType, true);
            if (board != null)
            {
                board.OnKillEffect(killPos, enemyTeam, killedType, true);
                board.SpawnPowerKillSparks(killPos, "lightning");
            }
        }
    }

    IEnumerator MageLightning(Vector3 originPos, Vector3 targetPos)
    {
        Sprite mageSprite = magoIdleSprite ?? magoAttackSprite ?? magoBackSprite;

        Vector3 magePos = GetRandomEmptyCellWorldPos();
        if (magePos == Vector3.zero) magePos = originPos;

        GameObject mage = new GameObject("LightningMage");
        mage.transform.position = magePos;
        SpriteRenderer mageSr = mage.AddComponent<SpriteRenderer>();
        mageSr.sortingOrder = 15;
        mageSr.sprite = magoIdleSprite ?? mageSprite;
        if (magoIdleSprite == null) mage.transform.localScale = Vector3.one * 0.2f;
        else mage.transform.localScale = Vector3.one * 0.35f;

        if (magoAttackSprite != null)
        {
            mageSr.sprite = magoAttackSprite;
            mageSr.sortingOrder = 15;
        }

        SoundManager.Instance.PlayLightning();
        yield return new WaitForSeconds(0.4f);

        yield return StartCoroutine(ProceduralLightning(magePos, targetPos));

        if (mage != null && mageSr != null)
        {
            if (magoBackSprite != null)
            {
                mageSr.sprite = magoBackSprite;
                mageSr.sortingOrder = 14;
                float fade = 0.25f;
                float ft = 0;
                while (ft < fade)
                {
                    if (mage == null) yield break;
                    Color c = mageSr.color;
                    c.a = 1f - (ft / fade);
                    mageSr.color = c;
                    ft += Time.deltaTime;
                    yield return null;
                }
            }
            if (mage != null) Destroy(mage);
        }
    }

    IEnumerator ProceduralLightning(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        float dist = dir.magnitude;
        Vector3 norm = dir / dist;
        Vector3 perp = Vector3.Cross(norm, Vector3.forward).normalized;

        int bolts = 3;
        for (int b = 0; b < bolts; b++)
        {
            Vector3 prev = from;
            int segments = Mathf.Max(4, Mathf.RoundToInt(dist * 6));
            for (int i = 1; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector3 basePos = from + norm * dist * t;
                float jitter = (i < segments) ? Random.Range(-0.12f, 0.12f) : 0f;
                Vector3 point = basePos + perp * jitter;

                GameObject seg = new GameObject("LightningSeg");
                seg.transform.position = (prev + point) / 2f;
                SpriteRenderer sr = seg.AddComponent<SpriteRenderer>();
                sr.sprite = GetCircleSprite();
                sr.color = new Color(1f, 0.95f, 0.1f, 1f);
                sr.sortingOrder = 15;
                Vector3 delta = point - prev;
                float segDist = delta.magnitude;
                seg.transform.localScale = new Vector3(segDist * 3f, 0.04f, 1f);
                float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
                seg.transform.rotation = Quaternion.Euler(0, 0, angle);
                Destroy(seg, 0.2f);

                prev = point;
            }
            yield return new WaitForSeconds(0.04f);
        }
    }

    IEnumerator LightningArc(Vector3 from, Vector3 to)
    {
        yield return StartCoroutine(ProceduralLightning(from, to));
    }

    IEnumerator LightningStrike(Vector3 pos)
    {
        float scale = 0.25f;
        if (lightningEffectSprite != null)
        {
            GameObject bolt = new GameObject("LightningBolt");
            bolt.transform.position = pos;
            SpriteRenderer sr = bolt.AddComponent<SpriteRenderer>();
            sr.sprite = lightningEffectSprite;
            sr.color = new Color(1f, 0.95f, 0.1f, 1f);
            sr.sortingOrder = 15;
            bolt.transform.localScale = Vector3.one * scale;
            yield return new WaitForSeconds(0.15f);
            if (bolt != null)
            {
                sr.color = new Color(1f, 0.95f, 0.1f, 0.3f);
                Destroy(bolt, 0.1f);
            }
        }
        else
        {
            for (int i = 0; i < 6; i++)
            {
                GameObject bolt = new GameObject("LightningBolt");
                bolt.transform.position = pos + new Vector3(Random.Range(-0.2f, 0.2f), 0, 0);
                SpriteRenderer sr = bolt.AddComponent<SpriteRenderer>();
                sr.sprite = GetCircleSprite();
                sr.color = new Color(1f, 0.95f, 0.1f, 1f);
                sr.sortingOrder = 15;
                bolt.transform.localScale = new Vector3(Random.Range(0.05f, 0.12f), Random.Range(0.6f, 1.2f), 1f);
                Destroy(bolt, 0.12f);

                GameObject spark = new GameObject("LightningSpark");
                spark.transform.position = pos + new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(-0.3f, 0.3f), 0);
                SpriteRenderer skr = spark.AddComponent<SpriteRenderer>();
                skr.sprite = GetCircleSprite();
                skr.color = Color.white;
                skr.sortingOrder = 16;
                spark.transform.localScale = Vector3.one * Random.Range(0.04f, 0.1f);
                Destroy(spark, 0.1f);
            }
        }
        yield return new WaitForSeconds(0.05f);
    }

    IEnumerator LightningDestroy(GameObject visual)
    {
        if (visual == null) yield break;
        SpriteRenderer sr = visual.GetComponent<SpriteRenderer>();
        if (sr == null) { Destroy(visual); yield break; }

        float duration = 0.2f;
        float t = 0;
        Color startColor = sr.color;
        while (t < duration)
        {
            if (visual == null) yield break;
            float p = t / duration;
            sr.color = Color.Lerp(startColor, new Color(1f, 0.9f, 0.1f, 0f), p);
            visual.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.1f, p);
            t += Time.deltaTime;
            yield return null;
        }
        if (visual != null) Destroy(visual);
    }

    IEnumerator FireDestroy(GameObject visual)
    {
        if (visual == null) yield break;
        SpriteRenderer sr = visual.GetComponent<SpriteRenderer>();
        if (sr == null) { Destroy(visual); yield break; }

        float duration = 0.35f;
        float t = 0;
        Color startColor = sr.color;
        while (t < duration)
        {
            if (visual == null) yield break;
            float p = t / duration;
            sr.color = Color.Lerp(startColor, new Color(1f, 0.2f, 0f, 0f), p);
            visual.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.3f, p);
            t += Time.deltaTime;
            yield return null;
        }
        if (visual != null) Destroy(visual);
    }

    IEnumerator MagicEffect(Team collectingTeam)
    {
        Team enemyTeam = collectingTeam == Team.Blue ? Team.Red : Team.Blue;

        List<(int r, int c, PieceType type)> candidates = new();
        for (int r = 0; r < board.rows; r++)
            for (int c = 0; c < board.cols; c++)
            {
                Cell cell = board.GetCell(r, c);
                if (cell != null && cell.IsOccupied && cell.pieceData?.team == enemyTeam && cell.pieceData.Value.type != PieceType.Pawn)
                    candidates.Add((r, c, cell.pieceData.Value.type));
            }

        if (candidates.Count == 0) yield break;

        var target = candidates[Random.Range(0, candidates.Count)];
        Vector3 targetPos = board.CellToWorld(target.r, target.c);

        SoundManager.Instance.PlayLightning();
        SoundManager.Instance.PlayFireRayo();

        Color magicColor = GetColor(PowerUpType.MAGIC);
        for (int i = 0; i < 20; i++)
            StartCoroutine(SparkFadeColor(targetPos + (Vector3)Random.insideUnitCircle * 1.2f, magicColor));

        CameraShake(0.1f, 0.3f);

        yield return new WaitForSeconds(0.3f);

        Cell tc = board.GetCell(target.r, target.c);
        if (tc != null && tc.IsOccupied && tc.pieceVisual != null)
        {
            SpriteRenderer sr = tc.pieceVisual.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                float t = 0;
                while (t < 0.3f)
                {
                    if (tc.pieceVisual == null) yield break;
                    float p = t / 0.3f;
                    sr.color = Color.Lerp(Color.white, magicColor, p);
                    tc.pieceVisual.transform.localScale = Vector3.Lerp(tc.pieceVisual.transform.localScale, Vector3.one * 0.3f, Time.deltaTime * 5f);
                    t += Time.deltaTime;
                    yield return null;
                }
            }

            board.ConvertPieceType(tc, PieceType.Pawn);

            if (tc.pieceVisual != null)
            {
                SpriteRenderer sr2 = tc.pieceVisual.GetComponent<SpriteRenderer>();
                if (sr2 != null)
                {
                    Color targetColor = tc.pieceData.HasValue ? board.GetPieceColor(tc.pieceData.Value.team) : Color.white;
                    float t2 = 0;
                    while (t2 < 0.3f)
                    {
                        if (tc.pieceVisual == null) yield break;
                        sr2.color = Color.Lerp(magicColor, targetColor, t2 / 0.3f);
                        t2 += Time.deltaTime;
                        yield return null;
                    }
                    if (sr2 != null) sr2.color = targetColor;
                }
            }

            for (int i = 0; i < 12; i++)
                StartCoroutine(SparkFadeColor(targetPos + (Vector3)Random.insideUnitCircle * 1.5f, Color.white));
        }
    }

    IEnumerator SparkFadeColor(Vector3 pos, Color color)
    {
        GameObject spark = new GameObject("MagicSpark");
        spark.transform.position = pos;
        SpriteRenderer sr = spark.AddComponent<SpriteRenderer>();
        sr.sprite = GetCircleSprite();
        sr.color = color;
        sr.sortingOrder = 16;
        spark.transform.localScale = Vector3.one * Random.Range(0.06f, 0.15f);

        Vector3 velocity = new Vector3(Random.Range(-1f, 1f), Random.Range(1f, 3f), 0);
        float life = 0.6f;
        float t = 0;
        while (t < life)
        {
            if (spark == null) yield break;
            spark.transform.position += velocity * Time.deltaTime;
            velocity.y -= 3f * Time.deltaTime;
            if (sr != null) { Color c = sr.color; c.a = 1f - t / life; sr.color = c; }
            t += Time.deltaTime;
            yield return null;
        }
        if (spark != null) Destroy(spark);
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

    IEnumerator FadeAndDie(GameObject obj, float duration)
    {
        float t = 0;
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        while (t < duration)
        {
            if (obj == null) yield break;
            if (sr != null) { Color c = sr.color; c.a = Mathf.Lerp(0.5f, 0, t / duration); sr.color = c; }
            t += Time.deltaTime;
            yield return null;
        }
        if (obj != null) Destroy(obj);
    }

    IEnumerator LightingFlash(Vector3 pos)
    {
        GameObject flash = new GameObject("Flash");
        flash.transform.position = pos;
        SpriteRenderer sr = flash.AddComponent<SpriteRenderer>();
        sr.sprite = GetCircleSprite();
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
            sr.sprite = GetCircleSprite();
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
            sr.sprite = GetCircleSprite();
            Color brown = new Color(
                Random.Range(0.3f, 0.55f),
                Random.Range(0.15f, 0.3f),
                Random.Range(0.05f, 0.1f),
                1f);
            sr.color = brown;
            sr.sortingOrder = 9;
            Vector3 dir = Random.insideUnitCircle.normalized * Random.Range(1.5f, 4f);
            float life = Random.Range(0.2f, 0.5f);
            StartCoroutine(FlyFade(dirt, dir, life));
        }
        yield break;
    }

    void CameraShake(float intensity)
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        StartCoroutine(DoCameraShake(cam, intensity, 0.2f));
    }

    void CameraShake(float intensity, float duration)
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        StartCoroutine(DoCameraShake(cam, intensity, duration));
    }

    IEnumerator DoCameraShake(Camera cam, float intensity, float duration)
    {
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

    Sprite CreateCircleSprite()
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

    Sprite CreateRingSprite()
    {
        Texture2D tex = new Texture2D(16, 16);
        Color[] pixels = new Color[256];
        for (int i = 0; i < pixels.Length; i++)
        {
            int x = i % 16;
            int y = i / 16;
            float dx = (x + 0.5f) / 16f - 0.5f;
            float dy = (y + 0.5f) / 16f - 0.5f;
            float d = dx * dx + dy * dy;
            pixels[i] = (d < 0.25f && d > 0.15f) ? Color.white : Color.clear;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
    }

    Sprite CreatePowerUpIcon(PowerUpType type, Color color)
    {
        Texture2D tex = new Texture2D(64, 64);
        Color clear = Color.clear;
        for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
                tex.SetPixel(x, y, clear);

        switch (type)
        {
            case PowerUpType.Shake: DrawShakeIcon(tex, color); break;
            case PowerUpType.Explosion: DrawExplosionIcon(tex, color); break;
            case PowerUpType.Fireball: DrawFireballIcon(tex, color); break;
            case PowerUpType.Lightning: DrawLightningIcon(tex, color); break;
            case PowerUpType.MAGIC: DrawMagicIcon(tex, color); break;
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
    }

    void DrawThickLine(Texture2D tex, int x1, int y1, int x2, int y2, int thick, Color color)
    {
        int steps = Mathf.Max(Mathf.Abs(x2 - x1), Mathf.Abs(y2 - y1));
        for (int s = 0; s <= steps; s++)
        {
            float t = steps > 0 ? (float)s / steps : 0;
            int px = Mathf.RoundToInt(Mathf.Lerp(x1, x2, t));
            int py = Mathf.RoundToInt(Mathf.Lerp(y1, y2, t));
            for (int dy = -thick / 2; dy <= thick / 2; dy++)
                for (int dx = -thick / 2; dx <= thick / 2; dx++)
                {
                    int sx = px + dx, sy = py + dy;
                    if (sx >= 0 && sx < 64 && sy >= 0 && sy < 64)
                        tex.SetPixel(sx, sy, color);
                }
        }
    }

    void DrawDot(Texture2D tex, int cx, int cy, int radius, Color color)
    {
        for (int dy = -radius; dy <= radius; dy++)
            for (int dx = -radius; dx <= radius; dx++)
            {
                if (dx * dx + dy * dy <= radius * radius)
                {
                    int px = cx + dx, py = cy + dy;
                    if (px >= 0 && px < 64 && py >= 0 && py < 64)
                        tex.SetPixel(px, py, color);
                }
            }
    }

    void DrawShakeIcon(Texture2D tex, Color color)
    {
        int cx = 32, cy = 32;
        for (int a = -1; a <= 1; a++)
            DrawThickLine(tex, cx - 10, cy + a * 12, cx + 10, cy + a * 12, 3, color);
        for (int i = 0; i < 5; i++)
        {
            tex.SetPixel(cx - 10 - i, cy - 12 + i, color);
            tex.SetPixel(cx + 10 + i, cy - 12 + i, color);
            tex.SetPixel(cx - 10 - i, cy + 12 - i, color);
            tex.SetPixel(cx + 10 + i, cy + 12 - i, color);
        }
    }

    void DrawExplosionIcon(Texture2D tex, Color color)
    {
        int cx = 32, cy = 32;
        for (int a = 0; a < 8; a++)
        {
            float angle = a * 3.14159f / 4;
            int ex = cx + (int)(Mathf.Cos(angle) * 14);
            int ey = cy + (int)(Mathf.Sin(angle) * 14);
            DrawThickLine(tex, cx, cy, ex, ey, 3, color);
        }
        DrawDot(tex, cx, cy, 6, color);
    }

    void DrawFireballIcon(Texture2D tex, Color color)
    {
        int cx = 32, cy = 32;
        for (int dy = -14; dy <= 14; dy++)
        {
            int halfW = Mathf.Max(0, 14 - Mathf.Abs(dy) / 2 - 2);
            DrawThickLine(tex, cx - halfW, cy + dy, cx + halfW, cy + dy, 3, color);
        }
        for (int i = 0; i < 4; i++)
        {
            tex.SetPixel(cx, cy - 18 - i, color);
            tex.SetPixel(cx - 1, cy - 18 - i, color);
            tex.SetPixel(cx + 1, cy - 18 - i, color);
        }
    }

    void DrawLightningIcon(Texture2D tex, Color color)
    {
        int cx = 32, cy = 32;
        int[] xs = { 0, -8, 8, -4, 12 };
        int[] ys = { -16, -6, 0, 8, 16 };
        for (int s = 0; s < xs.Length - 1; s++)
            DrawThickLine(tex, cx + xs[s], cy + ys[s], cx + xs[s + 1], cy + ys[s + 1], 3, color);
        DrawDot(tex, cx + xs[2], cy + ys[2], 3, color);
    }

    void DrawMagicIcon(Texture2D tex, Color color)
    {
        int cx = 32, cy = 32;
        for (int i = 0; i < 5; i++)
        {
            float angle = (i * 72f - 90f) * Mathf.Deg2Rad;
            int px = cx + (int)(Mathf.Cos(angle) * 14);
            int py = cy + (int)(Mathf.Sin(angle) * 14);
            DrawDot(tex, px, py, 4, color);
        }
        for (int i = 0; i < 5; i++)
        {
            float a1 = (i * 72f - 90f) * Mathf.Deg2Rad;
            float a2 = ((i + 2) * 72f - 90f) * Mathf.Deg2Rad;
            DrawThickLine(tex,
                cx + (int)(Mathf.Cos(a1) * 14), cy + (int)(Mathf.Sin(a1) * 14),
                cx + (int)(Mathf.Cos(a2) * 14), cy + (int)(Mathf.Sin(a2) * 14), 3, color);
        }
        DrawDot(tex, cx, cy, 3, color);
    }
}
