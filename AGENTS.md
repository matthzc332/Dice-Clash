# Project Instructions

> Instrucciones para agentes AI y desarrolladores. Este es el documento principal del proyecto.

## Project Context

### Goal
Implement single-player mode with AI-controlled red team, combat system overhaul (2d6+mod), match timer, scoreboard, and campaign progression.

### Constraints
- ScoreboardUI: Anchor `(0.5, 0.5)` with absolute positions, piece rows anchored to parent's top-left `(0, 1)`.
- Scoreboard panels loaded with `LoadFirstSprite` (index 0 via `Resources.LoadAll<Sprite>`).
- Victory/defeat check triggers `ScoreboardUI.Instance.Show()` which then calls `GameOverUI.Instance.Show()` after animation.
- All UI elements use anchor `(0.5, 0.5)` with absolute positions.
- AI uses simple heuristic (kill > attack > advance > lateral > retreat); selects one random piece per turn from all movable pieces.
- Sprites loaded with `LoadAll` and matched by name (e.g. `Next_0`, `Retry_0`, `botonQuit_0`). Win sprites use `winSprites[0]`.
- GameOverUI uses particle canvas at sortingOrder 201 (no raycaster) for all particles.
- Particles are UI Image–based coroutines (not ParticleSystem) for ScreenSpaceOverlay.
- SoundManager: short SFX are procedural tones; background music from `Resources/Sounds/Fondo/`. Slime SFX from `Sounds/Efectos/slime.mp3`.
- Test buttons (GANAR/PERDER) positioned at (-840, -415) and (-673, -424).

### Progress
#### Done
- **Gamanbit SDK métricas (098)** *(implementado, verificación pendiente en Unity — game id provisional `dice-clash-tactics`)*: SDK de telemetría de Gamanbit integrado. Paquete `GamanbitSDK.unitypackage` → `Assets/Scripts/Analytics/GamanbitAnalytics.cs` (singleton `DontDestroyOnLoad`, namespace `Gamanbit`, sesión + heartbeat 30s + flush 10s + caché de eventos en `PlayerPrefs` ante caídas) y nuevo `Assets/Scripts/Analytics/GamanbitBootstrap.cs` (`Boot()` crea+configura el GO al arrancar el menú, `StartSessionOnce()` diferida 1 frame — solo una vez por sesión, `OnLogMessage` con throttle = crash_log). Config por código: API `https://api.gamanbit.com/sdk/games`, gameId `dice-clash-tactics`, flush 10, **`isEventMode` forzado a `false` vía `Configure()`** (si fuera `true`, `PlayerPrefs.DeleteAll()` en Start borraría oro/campaña/cofres CADA arranque). Hooks: `MainMenuManager.Start` → Boot + sesión; `EconomyManager.AddGold` → `TrackCoreLoopHook` (una vez por sesión, el SDK lo guarda); `TutorialManager` → `TrackFtueStep("tutorial_start")` y `("tutorial_shadows")`; `GameOverUI` → `("tutorial_complete")` + `TrackRetry()` con contador en los 4 botones retry. Cambiar el gameId real es una línea (`GamanbitBootstrap.GameId`). Documento de implementación copiado a `docs/Gamanbit_Implementation_For_Agents.md`. Validación: dotnet build Assembly-CSharp.csproj = 0 errores (1 warning pre-existente).

- **Power-ups caían en los últimos minutos (097)** *(implementado, verificación pendiente en Unity)*: reporte de Dani en APK v0032 — tras un rato no aparecían más power-ups. Causa raíz: deadlock en `PowerUpManager.AutoSpawnTimer` — el loop espera `while (CountSpawned() >= 2)` (cap de 2 en tablero) pero los power-ups NO tenían vida útil: persistían hasta que una pieza pisaba la casilla. Al final de la partida quedan pocas piezas en movimiento → se acumulan 2 sin recoger → el spawner bloquea para siempre. Fix: nuevo `ExpirePowerUp(pu)` coroutine — vida útil 30s, fade 0.35s del icono y destroy + `spawnedPowerUps.Remove(pu)` → el slot se libera solo y el ciclo de spawn continúa toda la partida (solo aplica a `SpawnOnBoard`/auto-spawn; `SpawnSpecificAt`/placeholders del tutorial intactos). Validación: dotnet build Assembly-CSharp.csproj = 0 errores.

- **UI celular + builds v0031/v0032 (096)** *(implementado, verificación pendiente en Unity — v0032 enviado a Dani/grupo)*: reporte en APK: botón de sonido y SkipTurn se corrían en pantallas no-16:9, trompeta lejos de la orilla. Causa: el canvas del gold HUD (`EconomyManager`) no seteaba `matchWidthOrHeight` (default 0) mientras el resto usa 0.5 + elementos con anchor centro/izquierda y posiciones absolutas. Fix: `EconomyManager.cs` → `cs.matchWidthOrHeight = 0.5f`; `TurnUI.cs` SkipTurnButton re-anclado al borde derecho (anchor/pivot `(1,0.5)`, `(-10,-217)`); `BattleResultUI.cs` nueva helper `MakeImageRight` (anchor borde derecho) y trompeta re-anclada — tras "la trompeta no aparece" el pivot pasó de `(1,0.5)` a `(0.5,0.5)` con `(-220,-380)` (`Image.preserveAspect` con pivot no-centrado puede desplazar la imagen fuera del rect). Rebuild: EXE `Builds/v0031` + APK `Builds/v0032_Android` (juego completo; `ExpoBuild.txt` borrado durante build y restaurado). Gotcha CLI: `Start-Process` con `-ArgumentList` rompe la ruta con espacios → pasar el path con comillas embebidas `'"path"'` o falla `Couldn't set project path` (exit 1). Validación: dotnet build = 0 errores.

- **Ninja move más grande + emojis (095)** *(implementado, verificación pendiente en Unity)*: (1) Ninja Human/Orc/Beast moviéndose un poco más grande: en `AnimatedMove` el bloque `movingResolved != "Nigromantes"` de ninja multiplica `sizeRatio` por **1.35** (clamp a 1.35 en vez de 1.0) — el move queda ~35% más grande que la huella idle. Nigromantes no se toca (mantiene escala fija). (2) Emojis de Human/Orc caían al fallback cuadrado: los sprites se llaman `happy_0`/`sad_0`/`angry_0` (Human) y `happy_0`/`sad_0`/`angry_0`+frames (Orc), pero `CreateEmojiSprite` solo buscaba `emote{emojiKey}{emotion}_0` (formato Beastfolk `emotebeasthappy_0`). `CreateEmojiSprite` ahora prueba candidatos `emote{key}{emo}_0` → `{emo}_0` → `{emo}` por carpeta (species primero, fallback Human — cubre Human/Orc/Beastfolk y Nigromantes vía fallback Human). Validación: dotnet build Assembly-CSharp.csproj = 0 errores.

- **Ninja move sizes Human/Orc (094-corregido)** *(implementado, verificación pendiente en Unity)*: el ninja Human/Orc se veía gigante al moverse. Causa real: los PNG son láminas grandes (Human `NinjaMoveFront` 1087×657 con slice full-canvas (0,4) 1066×653 @PPU100 → huella ~10.7 u) y `AnimatedMove` solo compensaba la escala por área cuando el mundo era Nigromantes (gate `movingResolved == "Nigromantes"`) — el ninja no-nigromante movía el sprite a escala idle → gigante. Fix en C#: bloque nuevo en `AnimatedMove` para `Ninja` no-nigromante — `sizeRatio = sqrt(idleArea/moveArea)` (solo encoge) donde las áreas se calculan con **`sprite.bounds.size` (unidades de mundo, PPU-aware)** — NUNCA con `rect` en píxeles (el ninja Beast quedaba diminuto porque su move tiene PPU 450). Fix de metas (`.meta` NO versionados, el experimento previo a `spriteMode:1`/PPU 137-245 hizo "todos los slices visibles" y se revirtió): los 6 metas vuelven a `spriteMode: 2` + `spritePixelsToUnits: 100` con slices de banda derivados de análisis de píxeles que calzan las texturas grandes (0 fuera de límites): Human `NinjaIdleFront` 2×3 grid (ej. (78,161,74,96)), `Back/NinjaIdleBack` 2×3 grid, Human `NinjaMoveFront` (0,4,1066,653), `Back/NinjaMoveBack` (27,12,1103,715), Orc `NinjaIdleFront` 4 frames (80×~95-100), Orc `Back/NinjaIdleBack` 6 reales + 1 trash. Pivots `{x:0,y:0}`/formal `nameFileIdTable` de hoja (plantilla `PeonMoveFront`/`Orc NinjaMoveFront` intactos). Orc Move (1091×773 full-canvas) quedan intactos; Nigromantes conserva su escala fija 0.16/0.36. Moves de Beast conservan su meta legacy (ppu 450, slice (36,0,1109,801) con nombre `NubesBeast_0`) — el ratio por bounds lo deja en ~1.2 u. Guids preservados. Validación: dotnet build Assembly-CSharp.csproj = 0 errores.

- **Ninja idle scale (083)**: Bug cerrado. `originalScales` era un diccionario muerto — se almacenaba en `SetAttackPose` (key `SpriteRenderer.GetInstanceID()`) pero `ResetPieceSprite` nunca lo leía (solo `Remove`), restauraba siempre vía `GetIdleScale`. La causa original (store con `GameObject.GetInstanceID()` en `AnimatedMove`) ya había sido eliminada por el refactor de la nube de pelea (057/058). Fix: eliminado `originalScales`, `SetAttackPose` ahora usa `GetIdleScale(type, poseSpecies)` (fuente de verdad única) como base de la pose; `originalPositions` intacto (restauración de posición). Comportamiento idéntico en idle; cierra el patrón frágil para siempre.
- **Menu overhaul**: Background Menu.png, shelf, trophy header, three locked trophies, PlayButton with `botonplay2_0`, carousel with arrows.
- **GameOverUI**: Sprite buttons (Next/Retry/Quit), BlueWin/RedWin backgrounds with `winSprites[0]`, victory/defeat particles.
- **AI (018-red-ai)**: `AIController` scans red pieces, evaluates moves (kill +100, attack +50+diff\*10, advance +20, lateral +10, retreat +5), selects one random movable piece per turn, auto-ends turn.
- **Random AI fix**: Changed from tier-sorted-first-move to collecting all movable pieces and picking randomly. Added ±5 noise to move evaluation.
- **Turn alternation**: AI moves one piece per turn (not all), enabling blue→red→blue→red flow.
- **Win/lose background music**: `PlayWinLoseMusic()` loads `Sounds/Fondo/win-lose.mp3`, called from GameOverUI.
- **BoardManager**: `aiInProgress`, `MovePieceAI()` coroutine.
- **InputManager**: `aiPlaying` flag blocks input during AI.
- **TurnUI**: End Turn hidden during RedTurn.
- **Tester feedback (024/025)**: All UI/feedback polish applied.
- **Combat overhaul (026)**: New formula `2d6 + mod` replacing baseAttack/baseDefense. ATK always `2d6+1`, DEF varies by piece. Tier penalty removed. Charge/Caballero: +1 at dist=3. Aura/Paladin: +2 DEF to adjacent allies. Flank/Ninja: unchanged. Shadow Step for Ninja (move through occupied diagonal).
- **Card sprites**: Changed from `{Type}carta{Species}` (per-species) to `{Type}Carta` (universal): `PeonCarta`, `NinjaCarta`, `CaballeroCarta`, `PaladinCarta`.
- **CharacterCardUI**: Redesigned — no HP display, ATK shows `"2d6 + N"`, DEF shows `"2d6 + N"`, MOV shows type description, ability box added.
- **BattleResultUI**: Second dice per side for 2d6 animation.
- **TimerManager**: Match timer (default 5 min) with visual countdown.
- **ScoreboardUI**: End-of-match scoreboard showing Blue pieces → Red pieces with hammer SFX + hit effect. Victory/defeat banner with light/smoke. Transitions to existing GameOverUI.
- **SoundManager**: `PlayHammer()` procedural hammer-blow SFX for scoreboard.
- **World progression**: Trophy persistence via PlayerPrefs (`Trophy_Orc`, `Trophy_Beastfolk`). Next button advances Human→Orc→Beastfolk→MainMenu. Carousel shows all species (locked/unlocked), blocks locked on Play.
- **BoardManager.Awake()**: Reads `GameConfig.selectedScenario`/`selectedSpecies` from PlayerPrefs before `Start()`, fixing scenario not loading on scene reload.
- **LoadSprites red team**: Uses `scenarioTheme` as primary theme for red pieces, so enemies display correct sprites per world.
- **Quit button on defeat**: Loads from `botonQuit_0.png` individual sprite file.
- **Beastfolk tree**: Obstacle scale set to `(0.10, 0.14)`.
- **Battle UI fixes**: Piece icons 200x200 at y=227, "ATK n"/"DEF n" result texts, victoria panel pulsing (stop/coroutines/scale reset).
- **CharacterCardUI**: Cards 340x370, OwnCard y=-79, per-card element positions via AdjustCard.
- **BoardManager sorting**: Gargoyles, barrels, trees at sortingOrder -1 (behind pieces).
- **ScoreboardUI redesign**: New medieval-themed panel sprites (FinBatallaPanel, FinConteoBlue/RedPanel, FinPiezasBlue/RedPanel, FinVictoriaPanel). Animated piece-by-piece reveal with hammer SFX + hit effect. Victory banner pulses; defeat banner shows smoke particles. Transitions to GameOverUI after 2s delay.
- **Scoreboard layout fixes**: Removed card icons from rows (already in panel bg). Row NameLabel anchored `(0, 0.5)` pivot `(0, 0.5)` with Pos X=10; ScoreLabel anchored `(1, 0.5)` pivot `(1, 0.5)` with Pos X=-10. Rows stay at parent top-left `(0, 1)`. Title font 33; total texts font 26. "TOTAL" labels in each panel. Positions: Blue x=-346, Red x=401, per-type Y offsets.
- **Scoreboard piece sprites** (031-loser-animation): Removed FinPiezasBluePanel / RedPanel. One large Paladin sprite (230×230) per team in FinConteo panels at (0, -300). Both front-facing loaded by race (`speciesTheme`/`scenarioTheme`); red team at full color (no tint). Idle during scoring; winner switches to pose + gold sparkles, loser gets darkened (0.4 alpha) + pulsing red X. Total text at y=-45. Incremental scoring restored.
- **CharacterCardUI redesign**: CARD_W=370, CARD_H=400. ATK/DEF shows dice icon (Dado6 22×22) instead of letter "d". Prefix x=50, dice x=70, suffix x=98, font 12.
- **Back-facing sprite fix**: `LoadSprites` now takes `Team team` param — theme is `team == Blue ? speciesTheme : scenarioTheme` instead of `front ? speciesTheme : scenarioTheme`. Fixes blue pieces displaying Orc sprites when moving backward in Orc world.
- **Nigromantes front Move sprites (084)**: `LoadSprites` añade alias candidatos para Nigromantes en `suffix=="Move" && front` (`Peon→PeonNecroMoveFront`, `Knight→caballerofrontnigro`, `Paladin→PaladinMoveFrontNigro2`, y el existente `NinjaMoveFront`) cuando `SpriteFolder(t)=="Nigromantes"`. Antes las piezas rojas Nigromantes usaban el fallback `Sprites/Human/Pieces/{Nombre}MoveFront` al moverse hacia delante. Peon/Caballero/Ninja = 1 frame estático; Paladin = 4 frames animables. Fallback Human intacto.
- **Nigromantes front Move scale (085)**: piezas Nigromantes se veían chicas/pisaban al moverse front. Los PNG `MoveFront` son tiras de 4 frames; el frame 0 real es pequeño (Peon 77×92, Caballero 94×130, Paladin 90×123) vs idle back (247×351, 140×221, 130×208). La compensación por área exigía AGRANDAR (~1.6-3.5×) pero el cap `Mathf.Min(sizeRatio,1.0f)` (no-pawn) lo dejaba en 1.0 → ~0.6×0.8u vs idle 0.9×1.4u. Fix en `AnimatedMove`: eliminado el cap y el `×0.9` del peón SOLO para `movingForward` (movimiento front) → footprint idle (Peon ~1.02×1.26u, Knight ~0.97×1.32u, Paladin ~1.08×1.37u). Ninja conserva su escala fija (0.14,0.14,1) de 081.
- **Back move restaurado + ticket FREE (086)**: el cap/×0.9 originales vuelven a aplicarse en el movimiento BACK de Nigromantes (`!movingForward`) — no eran los sprites a ajustar y paladin/caballero move back se habían agrandado. `PowerUpManager.CheckCollectionForTeam` eliminó el override de expo `&& !ExpoConfig.Enabled` del guard `WithoutPowerups` → el azul ya NO recibe power-ups con ticket FREE (los enemigos sí), en cualquier build.
- **Orc Peon scale fix**: `CreatePieceVisual` sets Pawn scale to 0.85 to prevent visual overlap with adjacent Paladin.
- **Midground (fondo2)**: Additional background layer at sortingOrder -8 loaded from `{scenarioTheme}/Background/fondo2` with fallback to `Human/Background/fondo2`.
- **Null safety in AnimatedMove**: Added null checks after all yield points in `AnimatedMove`, `AnimateDestroy`, and related coroutines to prevent `MissingReferenceException` when pieces are destroyed mid-coroutine.
- **PowerUp icon visibility**: Increased icon sortingOrder from 1→14 and glow from 0→13 (above all pieces 0-13). Scale increased to 0.50 (icon) and 0.60 (glow). Procedural fallback now uses 64x64 textures with thick 3px lines (was 32x32 with 1px). Applied in both TutorialManager and PowerUpManager.
- **Scoreboard in tutorial**: Removed `if (GameConfig.isTutorial) return;` from ScoreboardUI.Show(). Added `BoardManager.suppressVictoryCheck` flag. TutorialManager sets true at start, false at shadow phase. VictoryTransition removed — ScoreboardUI→GameOverUI handles shadow victory. GameOverUI shows single "Next" button in tutorial mode.
- **Input blocked during power-up**: Added `PowerUpManager.IsExecuting` flag, set in `Execute()`, auto-clears after 2s. InputManager skips input while true. Prevents moving during power-up effects.
- **Power-up sounds**: Fireball/rayo suenan al lanzar (mage attack sprite), fire-rayo al impactar (vol 0.4, pitch 0.75). `PlayLightning()`, `PlayFireball()`, `PlayFireRayo()`, `PlayTemblor()`, `PlayExplosion()` methods.
- **Coin effect enhanced**: Texto fontSize 28 con bounce, 10 partículas (10-20px), arco alto (80-150), sparkle, screen shake, `PlayCoin()` sound (1800Hz+2400Hz).
- **Temblor mejorado**: 30 chispas doradas, CameraShake 0.2f/0.45s, piezas deslizadas con PushSlide, `PlayFireRayo()` al inicio.
- **PaladinAttackFront scale**: Hardcodeado (0.46, 0.52) para atacante y defensor, condición `sprite != defAtk[0]` eliminada. PaladinAttackBack sin ratio dinámico.
- **DailyBonusUI fix**: Crea su propio Canvas (sortingOrder 220) con GraphicRaycaster en vez de `FindFirstObjectByType<Canvas>` — arregla popup en esquina inferior-izquierda (canvas ajeno con ancla/raíz distinta) y OK no cliqueable (canvas sin raycaster). `Close()` destruye popup + canvas propio; guard `if (popupObj != null) return;` evita duplicados.
- **Coins fly to bag**: `EconomyManager.AddGold` → `AddGoldFX` → `SpawnCoinsToBag` anima monedas desde arriba hacia el bolso (conversión WorldToScreen → `ScreenPointToLocalPointInRectangle` para ubicar el target real del bolso) y al terminar `BagShake()` vibra el bolso. Reemplaza el viejo `SpawnCoinParticles` que dispersaba monedas lejos del bolso. Aplica a bonus diario, campaña, cofres y tutorial (+150).
- **Shadow tint on MAGIC conversion**: `BoardManager.GetPieceColor(team)` (shadow Red=0.2/0.2/0.25, Red=0.7/0.7/0.7, Blue=white) usado por `CreatePieceVisual`, `ResetPieceSprite` y el fade-back de `PowerUpManager` (antes `Color.white` → enemigo convertido quedaba blanco en fase de sombras).
- **Cup position fix**: Reset a (0,45) cada animación del vaso, white squares de dados ocultos durante cup.
- **Cup dialog**: Todos los mapas (no solo Human). Copa no llena → solo Retry centrado grande. Llena → Next + Retry. Botón Retry con `DelayedButtonSound(0.33s)`.
- **Power-up spawn rates**: Time-based delays según `TimerManager.timeRemaining` (10-18s primer tramo, 6-12s final).
- **EXE build**: Compilado a `Build/DiceClashTactics.exe`.
- **Combo fix (041)**: `blueKillCombo` now only increments when `turnManager.GetCurrentTeam() == Team.Blue`. Prevents combo x2 showing during AI turn or with single enemy.
- **Trumpet fix (042)**: Removed 0.5s initial delay, switched from `uiSource` to `source` (main AudioSource), volumes increased to 0.8/0.9. Plays immediately on dice win.
- **Paladin scale fix (043)**: `InputManager` subscribes to `TurnManager.OnTurnChanged` and calls `DeselectCurrent()`. Prevents pieces stuck in attack pose (0.65 scale) when turn timer expires. `ResetPieceSprite` now always restores to correct idle scale.
- **Cup victory system (044)**: `RecordMatchWin()` increments `cupVictories` on Blue win. Cup fill based on `cupVictories / VICTORIES_NEEDED` (2 wins). Removed `CUP_MAX = 1100` points threshold. `IsWorldUnlocked = cupVictories >= 2`. Cup text shows "W 0/2".
- **Gameplay music**: Two tracks random per match — `medieval_horizons` and `deuslower-fantasy-medieval-ambient-237371`. Selected randomly in `PlayGameplayMusic()`.
- **ObstacleManager (Fase 4)**: Created `ObstacleManager` with `ObstacleType` enum (`DestroyedCell`, `Glue`, `Mine`, `Hole`). Spawn logic finds empty cells, shuffles, places up to 2 per type. Procedural sprites for Glue (slime from `Sprites/Menu/slime`), Mine, and floor overlay.
- **DestroyedCell**: Timer-based (4 turns). Visual: real tilemap sprite (`GetTileSprite`) at `tileScale * 0.9f` that shakes via `ShakeFloorTile` coroutine. Timer bar changes green→yellow→red. At 0: `CollapseDestroyedCell` — sound + chunks + smoke + camera shake. Replaces tilemap with `pisoRoto` (`Sprites/Menu/pisoRoto`) at `tileScale * 1.1f`. Cell becomes `ObstacleType.Hole` blocking movement.
- **Glue (Pegote)**: Piece that passes over gets trapped for 2 turns (`gluedPieceTurns`). Glued piece can only attack adjacent enemies, not move. Visual: slime sprite at `(2.5, 2.1)` scale, `sortingOrder -1`. Sound: `Sounds/Efectos/slime.mp3`. Re-triggers if piece steps on another glue cell.
- **Mine (Mina)**: 3×3 explosion on contact. Kills piece on cell + damages adjacent pieces. Visual: procedural mine sprite with pulsing. `PlayMineExplosion()` sound.
- **BoardManager init order fix**: `ObstacleManager` created and initialized BEFORE `SetupInitialBoard()` so obstacles spawn correctly. `GameManager.Start()` calls `ObstacleManager.SubscribeTurnManager(turn)` after TurnManager creation.
- **GetValidMoves updated**: New obstacle exclusions — `Glue`, `Mine`, `DestroyedCell` don't block movement (walkable). `Hole` blocks movement. Destroyed cell tiles are walkable until collapse.
- **AnimatedMove obstacle triggers**: Mine triggers `TryTriggerMine` (3×3 explosion), Glue triggers `TryTriggerGlue` (2-turn trap) when piece lands on them.
- **TestButtons campaign**: LVL 9 button + burger menu with all 22 campaign levels. `GameConfig.PlayCampaign(levelId)` sets `isCampaign = true`, `selectedLevel` via PlayerPrefs.
- **ObstacleManager.OnTurnChanged**: Iterates destroyed cell candidates, decrements timers, triggers collapse. Iterates glue turns, decrements counters. Uses `new Dictionary<int,int>(gluedPieceTurns)` copy to avoid `InvalidOperationException` during enumeration.
- **Beastfolk piece scales**: Paladin `0.95f`, Knight `1.0f` (other species Knight `1.15f`). `GetIdleScale` and `CreatePieceVisual` unified — `GetIdleScale` is single source of truth for all piece scales.
- **TileScale**: `board.tileScale` made public. Used for floor overlay sizing and `ReplaceTileSprite` calculations.
- **CampaignManager + CampaignUI (Fase 5)**: `CampaignManager` singleton DontDestroyOnLoad, tracks completions via PlayerPrefs (`Campaign_Level_{id}`). `CompleteLevel()` awards gold via `EconomyManager`. `IsLevelUnlocked()` chains from previous level. `CampaignUI` scrollable panel on MainMenuManager: 4 copas, 3 columns, levels with number+name+checkmark. Back button hides panel. GameManager also creates CampaignManager if missing. ScoreboardUI calls `CompleteLevel()` on blue win in campaign. GameOverUI shows Next+Quit in campaign mode (Next loads next uncompleted level or menu). MainMenuManager has "CAMPAIGN" button with X/22 progress counter.

#### SPEC-046: Rediseño de Recompensas
- **CampaignData.json actualizado**: 22 niveles con power-ups/obstáculos específicos del doc del diseñador. Nombres de power-ups: Shake, Explosion, Fireball, Lightning, MAGIC. Obstáculos: Roca, DestroyedCell, Glue, Mine. Enemigos varían por nivel (Human, Orc, Wolf, NewRace).
- **GameConfig modos de juego**: Enums `GameMode` (Campaign/Ranked) y `PowerupMode` (WithPowerups/WithoutPowerups). Métodos `PlayCampaign(levelId, powerupMode)` y `PlayRanked(powerupMode)`. Flag `isRanked` en PlayerPrefs.
- **Power-ups siempre spawn**: `PowerUpManager.SpawnOnBoard()` respeta power-ups del nivel en campaña. En modo `WithoutPowerups`, el jugador no puede recolectar power-ups pero los enemigos sí.
- **RibbonManager**: Sistema de listones por victoria de campaña. Colores por mundo (Human=azul, Orc=verde, Beastfolk=marrón, Nigromantes=violeta). 5 tonalidades de claro a oscuro por copa. Integrado en `CampaignManager.CompleteLevel()`.
- **ExhibidorUI**: Pantalla completa reemplaza estante del menú. Muestra 4 copas (dorado si completada), grid de listones e insignias, contadores. Botón TROPHIES en menú principal.
- **ChestManager con oro + progresión**: `OpenChest()` retorna `ChestReward` con oro (50 base) + insignias + flags de duplicada. Algoritmo progresivo: 80% nueva si <50% coleccionado, 50% si 50-80%, 20% si >80%. `ChestUI` muestra secuencia: oro primero, luego insignias con flash blanco, sello "DUPLICADA!" en rojo.
- **EnemyBanner**: Banner con nombre del ejército enemigo al inicio de cada nivel de campaña. Animación fade in → stay → fade out. Color por raza.
- **RankedManager**: Modo libre con enemigos aleatorios. Power-ups por raza (Human=Shake+Explosion, Orc=Fireball+Lightning, Wolf=Lightning+MAGIC, NewRace=Fireball+MAGIC). 1 obstáculo aleatorio. Desbloqueado después del tutorial.
- **ModeSelectionUI**: Menú con 4 opciones (Campaign With/Without, Ranked With/Without). Costos: Campaign=20g, Ranked With=30g, Ranked Without=15g. Verificación de oro suficiente.
- **GoblinDialogue**: Onboarding post-tutorial. Explica power-ups gratuitos. Una vez mostrado, no se repite. Persiste en PlayerPrefs.
- **Desbloqueo de personaje por copa**: `CampaignManager.CheckCupCompletion()` otorga `Unlocked_{race}` al completar todos los niveles de una copa.
- **Tutorial arreglado**: `TurnUI.UpdateUI` fuerza BlueTurn en tutorial y oculta End Turn (botón solo activo si `state == BlueTurn && !isTutorial`). `TutorialManager.IdleBlinkLoop` reemplaza el blink único por corrutina continua (4s de inactividad → pulso azul 3s → reintenta), se pausa al seleccionar. `GoblinDialogue` reescrito con fondo `Tutorial/GoblinDialogue` (`LoadAll[0]`, null-guard a rect), máquina de texto (0.02s/char), botón OK! → `MarkShown()`. `TutorialRewardUI` nuevo: popup al terminar tutorial (antes del goblin) con cofre real `Sprites/Cofre` (shake + flash + chispas + hop), +150 oro (`EconomyManager.AddGold`), listón de madera (`liston.png` teñido 0.62,0.42,0.24) e insignia (`insigniaMadera.png`), PlayerPrefs `TutorialRewardClaimed` (una sola vez). `GameOverUI` Next del tutorial → `ShowRewardThenProceed()` (recompensa → goblin → `GameConfig.Play`).
- **Power-ups gratis**: Quitado por completo el costo de oro en `PowerUpManager.Execute()` (ya no usa `EconomyConfig.GetPowerupCost`/`SpendGold`). Solo se paga la entrada del modo.
- **Exhibidor listones corregido**: `RibbonManager.GetColorsForCup()` con mapeo correcto (cup0/cup1=Human, cup2=Orc, cup3=Beastfolk, cup4=Nigromantes). Listones por raza en `Sprites/Liston/ListonHuman|Orc|Beast|Nigromante` (blancos, spriteMode 2): `RibbonManager.GetRibbonSprite(levelId)` selecciona sprite por raza de la copa y `ExhibidorUI` lo tiñe con `GetRibbonColor` (claro→oscuro por progreso). Fallback a rectángulo plano si falta.
- **Coleccionables tutorial**: `TutorialCollectibles` (static, PlayerPrefs `TutorialCollectiblesEarned`) otorga copa/insignia/listón al ganar a las sombras (`TutorialManager.CheckShadowEnemyKilled` → `Grant()`). Sprites `Tutorial/copaTuto`, `Tutorial/InsigniaTutorial`, `Tutorial/ListonTutorial` (spriteMode 2, `LoadAll[0]`). `TutorialRewardUI` muestra los 3 items + 150 oro (copa dorada 1,0.84,0 · listón madera 0.62,0.42,0.24 · insignia blanco). `ExhibidorUI.CreateTutorialSection` (banda central superior, gris 0.3 alpha si no ganado).
- **Goblin bloqueado arreglado**: `GoblinDialogue` crea su propio Canvas (sortingOrder 215, ScreenSpaceOverlay, CanvasScaler, GraphicRaycaster) en vez de `FindFirstObjectByType<Canvas>` — el OK podía caer en un canvas sin raycaster (ej. ParticleCanvas 201) y quedarse sin clic. Texto de diálogo con rect fijo: anchor centro, offsetMin (-79,-84), offsetMax (79,84).
- **Clics repetidos del Next (tutorial)**: guard `rewardFlowRunning` en `GameOverUI` evita que clics múltiples apilen varios popups de recompensa/goblin (y oro +150 duplicado).
- **Cofre más grande + efectos**: `TutorialRewardUI` content 720×660, cofre 360×324 (glow 440), título 26, oro 30, items 104/210×60/96, chispas 26 (10-18px, dist 70-170), monedas 18 (12-20px), flash 80→460, hop ±34px.
- **Tutorial fixes (054)** *(implementado, verificación pendiente en Unity)*: Botón "OK!" de `TutorialRewardUI` re-anclado a `(0.5, 0.5)` en `(0, -290)` (antes ancla inferior en `(0, -295)` → fuera de pantalla y flujo colgado). `BoardManager` helpers `IsTutorialDummy`/`GetTutorialDummySprite`/`GetTutorialDummyScale` + guards en `ResetPieceSprite` y `AnimatedMove` (los maniquíes conservan `muneco1` y escala 0.19 en combate). Daily bonus movido al menú: nuevo `TutorialProgress` (PlayerPrefs `TutorialPlayed`, marcado al completar O saltear el tutorial en `TutorialManager`); `GameManager` ya no crea `DailyBonusUI`/`ShowDailyBonusDelayed`; `MainMenuManager.Start` lo muestra solo si `TutorialProgress.HasPlayed()` (también crea `EconomyManager`/`DailyBonusUI` → HUD de oro y costos del menú funcionan). `ScoreboardUI.Show(Team? forcedWinner)`; `TestButtons` WIN→`Show(Team.Blue)`, LOSE→`Show(Team.Red)` para que las recompensas reales (campaña: oro+listón+insignia; copa: `RecordMatchWin`) se otorguen vía pipeline.

#### Done
- **Tutorial (033)**: Tutorial for new users with wooden dummies, simplified flow (no turns — player just attacks all dummies). 4 dummies at scale 0.19. 4 power-up placeholders at board edges. `TutorialManager` with `OnTurnChanged` auto-revert to keep always BlueTurn. Tutorial hides cup, End Turn, Scenario/Species buttons, timers. BoardManager loads `Tutorial/fondoTuto`, `Tutorial/piso5/piso6`, `Tutorial/muneco1`, `Tutorial/barril2`. GameManager skips AI+Timer in tutorial. Menu has "TUTORIAL" button calling `GameConfig.PlayTutorial()`. Shadow phase victory shows ScoreboardUI → GameOverUI. Input blocked during power-up effects. Skip/Next buttons start normal game via `GameConfig.Play()`.
- **Game feel (057)**: Hit-stop (0.05s dice clash, 0.06s kills) via `Time.timeScale` freeze. Pitch random ±10% on procedural SFX, ±5% per chord voice. Dramatic silence 0.15s after dice spin. Knight jump animation (parabolic arc, Salto1/2, squash, hammer+shake, dust particles). Jump scale per race via `GetJumpScale` (`KnightJump`): Human 0.60/0.5 front · 0.09/0.09 back, Orc 0.55/0.6 front · 0.33/0.34 back, Beastfolk 0.66/0.52 front · 0.41/0.34 back, Nigromantes 0.15/0.12 (front+back). `SpritePrefix("Beastfolk")="beast"`, `LoadLargestSprite()` for spriteMode 2. Paladin light beam (procedural rect, fadeIn/flicker/fadeOut, 6 holy sparks, `PlayHolyBeam()`). Slide 1.3s for Paladins.
- **CampaignRewardUI**: Full-screen popup (sortingOrder 210) with sequential reveal: gold/insignia/ribbon/chest with white flash, scale pop, panel shake, sounds, coin bounce + particles, sparkles. OK button with `botonOK_0`.
- **Cup completion animation**: `CampaignManager.CompleteLevel()` returns cup race on first-time completion. `CampaignRewardUI` shows cup sprite with bounce (0.3→1.6→1.0) + `PlayVictory()` at end of reward sequence.
- **Scoreboard skip + faster counting**: "SKIP >>" button instantly completes. Normal delays 0.4→0.25s, inter-team 0.3→0.2s, post-count 0.5→0.4s.
- **Post-tutorial flow**: ScoreboardUI → GameOverUI → TutorialRewardUI → GoblinDialogue → ModeSelectionUI (campaign tickets) → FlashAndStartCampaign → level 3 (scans `cups[1]`).
- **ExhibidorUI badges cleanup**: Removed PREV/NEXT pagination, all badges in one scrollable list. BlueFlag covers broken 1/2+arrow at (X=-1, Y=-204, 326×269).
- **EnemyBanner**: Loads `enemyBanner` sprite from `Sprites/Decor` (spriteMode 2, `StartsWith`), white text with black outline, `preserveAspect`.
- **Power-up glow**: Uses `cambio` sprite from `Sprites/PowerUps/Icon/cambio` via `GetGlowSprite()`.
- **CampaignUI lighter panels**: Completed `(0.25,0.55,0.25)`, unlocked `(0.3,0.38,0.55)`, locked `(0.35,0.35,0.38)`. Insignias shifted left `(0.74-0.90)`.
- **ModeSelectionUI campaign mode**: `ShowCampaign(Canvas)`, FREE/PREMIUM 20g tickets, HeartbeatPulse, HoverGrow, flash+sound.
- **Common reward panels**: Darker `(0.45,0.45,0.45,0.9)` so they don't blend with panelCartaBlue.
- **Test buttons moved**: TEST CHEST + TEST DAILY now in ExhibidorUI settings.
- **TutorialRewardUI**: panelCartaBlue background, `botonOK_0` claim button. **GoblinDialogue**: lower text, `botonOK_0` continue button.
- **Fight cloud (nube de pelea)**: Reemplaza sprites de ataque durante combate. 6 combinaciones de especies (HumanHuman, HumanOrc, HumanBeast, OrcOrc, OrcBeast, BeastBeast), 3 frames de animación por nube (0.35s/frame), rotación 40°/s, pulse ±4% escala ±7% alpha. Sprites en `Sprites/PowerUps/Efect/FightCloud_{sp1}{sp2}{1|2|3}`. Escala normalizada proporcional (referencia 251px). Sort por tier: Human=0 < Orc=1 < Beast=2. Fallback procedural (5 círculos + estrella central, 256×256). Ambas piezas ocultas con `SetActive(false)` durante la nube. Perdedor destruido silenciosamente (`Destroy`). Ganador muestra con `ResetPieceSprite` + `SetActive(true)`. `DeathPoof` + `OnKillEffect` en posición. `PlaySwordClash()` al inicio. Tutorial y sombras usan siempre `FightCloud_HumanHuman`.
- **Tester round 3 + mapa (060)** *(implementado, verificación pendiente en Unity)*: Texto OK/OK! duplicado sobre sprite `botonOK_0` eliminado en `CampaignRewardUI`, `GoblinDialogue` y `TutorialRewardUI` (el Text queda solo como fallback si falta el sprite; ChestUI/ExhibidorUI ya lo hacían bien). "BATTLE OVER" bajado 12px dentro del panel (`ScoreboardUI`). Números grandes semitransparentes (font 64, alpha 0.45) detrás de los dados durante clash en `BattleResultUI` (`AtkBigResult`/`DefBigResult`, se revelan con las caras finales). Diálogo grande del tutorial más chico y bajado: campos `bigPos=(0,-200)`/`bigScale=1.85` reemplazan `(0,-120)`/2.4 hardcodeados en `MovePanelBig` y `WelcomeCoroutine`; estado chico sin cambios. `GameOverUI.Dismiss()` limpia partículas/botones, oculta OverlayBg+ParticleCanvas y llama `StopMusic()`; llamado desde `OpenCampaignMap` y `ShowCampaignModeSelection` para que la pantalla de victoria no quede detrás del mapa. OverlayBg/ParticleCanvas se reactivan solos en `Show()`. Mapa de campaña: abre directo en la página del último nivel jugado (`GameConfig.selectedLevel`) con cabeza en `HeadAnchor` de ese nivel → FX conquista ahí → un solo avance al siguiente (máx 1 flip adelante); `ConquestAndTravel` usa `lastPlayed` como origen (fallback `GetPreviousCompletedBefore`). Tarjetas completadas (state 2) ahora tienen botón para rejugar vía `ModeSelectionUI.ShowCampaign`. Fix `HeadBob`: usa `headBasePos` fija (antes cuantificaba su propia salida cada frame → cabeza saltaba errática); `WalkHead` reinicia bob con base correcta al terminar. Validación: compilación Roslyn de todo `Assets/Scripts/Game` contra DLLs de Unity = 0 errores.

- **Reset cheat (062)** *(implementado, verificación pendiente en Unity)*: Cheat oculto para eventos en menú principal. Zona invisible 90×90 en esquina inferior derecha del canvas del menú (`(895, -495)`): 5 taps dentro de 3s → popup de confirmación (RESET rojo / CANCEL gris, dim 0.75). `ProgressReset.WipeAll()` hace `PlayerPrefs.DeleteAll()` + `Save()` + recarga `MainMenuScene` (inventario verificado: no hay otras claves; solo `CampaignManager` es DontDestroyOnLoad y lee prefs por llamada, así que la recarga basta). Resetea campaña/estrellas/listones/unlocks/ranked/oro/daily/cofres/tutorial flags/goblin/trofeos. Nuevo archivo `Assets/Scripts/Game/ProgressReset.cs`; trigger + popup en `MainMenuManager` (`BuildResetCheat`, `OnResetCheatTap`, `ShowResetConfirm`). Validación: compilación Roslyn = 0 errores.

- **Tester round 4 (063)** *(implementado, verificación pendiente en Unity)*: `BattleResultUI` números grandes ahora por EQUIPO (azul izquierda `-395`, roja derecha `+395`, junto a los dados; antes se asignaban por rol atk/def y aparecían cruzados cuando atacaba rojo). `CampaignRewardUI` botón OK: ruta corregida a `Sprites/Menu/botin ui/botonOK` con `LoadAll` + find `botonOK_0` (ruta vieja inexistente → caía en fallback panel+texto). `CampaignMapUI` tarjetas: nombre x -20→8, info "WOLF | E:5 | +30g" font 7→9 y y 4→-30, insignia 46×46→89×73. Fondo Nigromantes: usuario subió `Sprites/Nigromantes/Background/Nigromantes.jpeg` (import Multiple); `BoardManager.CreateBackground` resuelve carpeta `NewRace`→`Nigromantes` y carga vía `LoadAll` con `Find(name.StartsWith(theme)) ?? [0]`. `ModeSelectionUI` fonts: ticket subtitle 8→10 (WITH/NO POWER-UPS), ticket desc 7→9 ("Pay gold to enter. Full experience."), EnemyPreview label 9→11 y army name 15→18. Validación: compilación Roslyn = 0 errores.

- **MAGIC icon + ritual del mago (064)** *(implementado, verificación pendiente en Unity)*: Pickup MAGIC del tablero usa sprite `cambio` (`Sprites/PowerUps/Icon/cambio`, ya era el glow) — antes buscaba "MAGIC"/"MAGIC_0" inexistente y caía al ícono procedural; special-case en ambos spawns (`SpawnOnBoard`/`SpawnSpecificAt`). `MagicEffect` reescrito con secuencia ritual: círculo `ritual.png` (`Sprites/PowerUps/Efect/ritual`, import Multiple → triple fallback Load<Sprite>/LoadAll/Texture2D+Sprite.Create) sobre la casilla objetivo a 1.15× su tamaño (`GetCellWorldSize()` mide distancia entre centros de casillas), fade-in 0.3s violeta con rotación 60°/s → mago aparece (magoIdle, escala 0.35, orden 16) → magoAttack + `PlayFireRayo` + shake → conversión existente (tint/shrink/ConvertPieceType(Pawn)/restore) → mago fade-out via magoback 0.25s → círculo fade-out rotando → destroy → `SmokeBurst(targetPos, 6)`. Null-guards tras cada yield. Helpers compartidos `SpawnRitualCircle`/`FadeRitualCircle` reutilizados en `MageAndProjectile` (Fireball, tinte naranja) y `MageLightning` (Lightning, amarillo): círculo ritual bajo el mago, fade-out cuando el mago desaparece. Quien pisa el pickup ejecuta el efecto para su equipo (MAGIC convierte pieza del contrario). Validación: compilación Roslyn = 0 errores.
- **Puño de Shake + fix sprites Multiple (065)** *(implementado, verificación pendiente en Unity)*: Causa raíz de "machita violeta" en ícono MAGIC y círculo invisible: `cambio.png` y `ritual.png` importan como spritesheet **Multiple** → slices `cambio_0`/`ritual_0` no matchean búsquedas por nombre exacto. Nuevo helper `LoadFullSprite(path)`: `Load<Texture2D>` + `Sprite.Create` rect completo primero (inmune al import mode), luego Load<Sprite>, luego LoadAll[0]. Aplicado a `cambio` (glow + ícono MAGIC) y `ritual`. Shake ahora ejecuta `ShakeWithFist(sourceRow, sourceCol, team)`: puño (`punio.png`, escala 2.2× casilla, orden 17) cae desde arriba-izquierda en parábola bezier cuadrática (0.55s, rotación -25°→0°) hasta la casilla pisada → DirtChunks → `ShakeEffect` intacta con el puño plantado → SmokeBurst ×10 + fade 0.25s → destroy. Fallback sin sprite: espera + ShakeEffect normal. Validación: compilación Roslyn = 0 errores.
- **Ajustes MAGIC (066)** *(implementado, verificación pendiente en Unity)*: `cambio` ya NO es glow universal (hacía que todos los pickups mostraran el arte transparente detrás) — `GetGlowSprite()` vuelve al círculo procedural, campo `glowSprite` eliminado. MAGIC usa campo dedicado `magicIconSprite` = `cambio` via LoadFullSprite. Íconos de pickups escalados por bounds a 70% del ancho de casilla (`ActivePowerUp.baseScale`, pulso en `AnimateSpawned` lo respeta; fallback 0.25). `MagicEffect` sin mago sobre la pieza objetivo: solo círculo ritual (fade-in → PlayFireRayo+shake → conversión → fade-out → SmokeBurst). Validación: compilación Roslyn = 0 errores.
- **Fixes arte/sonido (067)** *(implementado, verificación pendiente en Unity)*: Puño cae desde la derecha (bezier invertido, rotación Lerp(25°→0°)). Lock de tarjetas redibujado como candado (16×16, FilterMode Point, sizeDelta 34). Estrellas por rendimiento: `alive/7≥0.5`=+1★ + `elapsed≤180s`=+1★; `CompleteLevel(levelId, stars)` — insignia solo si `stars≥3`. Volúmenes de música por pista (0.45 horizons / 0.85 deuslower); arrays extensibles `campaignTracks`/`campaignTrackVolumes` listos. Fondo con fallback Texture2D+Sprite.Create (cubre cualquier import mode); midground intenta carpeta temática primero (Nigromantes/fondo3) fallback Human. Reward: filas 470px, iconos fuente ~×1.25, height×1.2, spacing +20%. Validación: compilación Roslyn = 0 errores.
- **Fixes candado/fondo/back/música (068)** *(implementado, verificación pendiente en Unity)*: Candado de tarjetas ampliado a sizeDelta 80×80 (antes 34×34 casi invisible). Fondo Nigromantes: meta JPEG cambiado de Multiple(2) a Single(1) + fallback `Resources.Load<Sprite>` antes de Texture2D+SpriteCreate. ModeSelectionUI Back button ahora carga `MainMenuScene` en vez de solo `Destroy(panel)` (evitaba loop a partida vacía tras sombras). Música campaña: `PlayCampaignMenuMusic()` en `CampaignMapUI.ShowInternal()`, `Close()` restaura menú; campo dedicado `campaignMenuTrack`. CAMPAIGN button subido a Y=-470 (antes -510 fuera de pantalla). REWARD font 24→28. **Lock transparente**: card tint `Color.clear` + outline `Color.clear` — solo se ve candado dorado, sin panel gris. **Knight jump backward**: escalas hardcoded reemplazadas por cálculo dinámico `idleScale * 0.55 * sqrt(frontArea/backArea)` — compensa sprites back más grandes. Validación: compilación Roslyn = 0 errores.
- **Salto caballero + ninja (069)** *(implementado, verificación pendiente en Unity)*: `KnightJump` escala FIJADA por raza vía `GetJumpScale(species, movingForward)` (reemplaza el 0.9 y los overrides hardcoded viejos): Human 0.60/0.5 front · 0.09/0.09 back, Orc 0.55/0.6 front · 0.33/0.34 back, Beastfolk 0.66/0.52 front · 0.41/0.34 back, Nigromantes 0.15/0.12. Se usa la misma escala al salto1 (inicio) y salto2 (aterrizaje). Fix corrutina: `AnimatedMove` ahora calcula `useKnightJump` ANTES y salta el bloque Move/`PlayAnimation` cuando el caballero va a saltar (evita que la corrutina `AnimateSprites` pise `sr.sprite = salto1` — causaba que el orco mostrara la caminata en vez del salto). Fix ninja: el ratio de escala de Nigromantes en `AnimatedMove` (compensaba sprites de movimiento más grandes) ahora condiciona a `SpriteFolder(ThemeForTeam(movingData.team)) == "Nigromantes"` en vez de `SpriteFolder(scenarioTheme)` — antes estiraba al ninja humano (azul) cuando el ESCENARIO era Nigromantes (niveles 5/22) aunque la pieza fuera humana. Validación: compilación Roslyn = 0 errores.
- **Emoji fallback + floor fix (069)** *(implementado, verificación pendiente en Unity)*: `CreateEmojiSprite` reestructurado — itera por carpetas (species primero, fallback Human), `emojiKey` deriva de la carpeta actual (Wolf→Beastfolk, NewRace→Human). `CollapseDestroyedCell` ya NO llama `EndTurn()` — solo verifica victoria para mostrar Scoreboard. Nigromantes background agregado a `GetBackgroundSprite`/`GetCupSprite` en CampaignMapUI. Panel oscuro restaurado `(0.33,0.33,0.36,0.82)` sin candado. Validación: compilación Roslyn = 0 errores.
- **Escala salto back caballero humano (070b)**: `GetJumpScale` Human back `(0.09,0.09)` → `(0.13,0.13)` (probado 0.19 quedaba muy grande).
- **Escala ninja nigromante moviéndose (070b)**: en `AnimatedMove`, ninja nigromante usa escala fija `(0.15, 0.16)` al moverse (reemplaza el ratio por área; resto de piezas nigromantes conservan el ratio).
- **Hit sprite fuego/rayo (071)** *(implementado, verificación pendiente en Unity)*: al impactar Fireball/Lightning, el sprite del PJ alcanzado es REEMPLAZADO por el quemado (`Sprites/PowerUps/Efect/fuego1`/`fuego2` o `rayo1`/`rayo2`), animando 1-2-1-2 con parpadeo/alpha + pulso de escala, tamaño normalizado por área para que "ocupe su lugar", sortingOrder 16, sonido `PlayFlame()` (fire) o `PlayElectricity()` (lightning), y luego desaparece con el fade+shrink actual (`ElementHitDestroy`). Ciclo mago/círculo/proyectil intacto. `FireDestroy`/`LightningDestroy` quedan como fallback. Nuevos `PlayFlame()`/`PlayElectricity()` en SoundManager (`flame.mp3`/`electricity.mp3`).
- **Ajustes hit fuego/rayo (071b)** *(implementado, verificación pendiente en Unity)*: sonido flame/electricity ahora arranca al inicio del impacto (antes del swap de sprite) y dura justo lo de la animación + un poco (fire 1.2s/electricity 1.1s) cortándose vía AudioSource temporal (`PlayTempClip` + `DestroyAfterDelay` en SoundManager, `Mathf.Min(duration, clip.length)`). Sprite quemado +15% más grande (`origScale *= 1.15f`). Intercalado 1-2-1-2 cada 0.1s. El suelo queda QUEMADO unos segundos en la casilla del impacto (`BurnGroundAt(killPos, color)` — círculo procedural `GetCircleSprite`, sortingOrder -1, hold 2.8s + fade 0.6s vía `FadeBurn`; fuego `(0.12,0.06,0.02,0.7)`, electricidad `(0.05,0.05,0.1,0.7)`).
- **GoldPanel con sprite (072)**: HUD de oro en `EconomyManager.CreateGoldHUD` usa como fondo `Sprites/Menu/panelVictoria` (`panelVictoria_0`, 1280×220, LoadAll — banner que estira bien al rect actual 190×48) en vez de rectángulo plano + borde 3px; fallback `panelCartaBlue` (`panelCartaBlue_0`) y luego color sólido. Helper `LoadPanelSprite()`. Borrado el hijo `Border`.
- **Build de expo (073)**: Marcador `Assets/Resources/ExpoBuild.txt` (cualquier contenido) → `ExpoConfig.Enabled` = `Resources.Load<TextAsset>("ExpoBuild") != null`. `ExpoConfig.ApplyBootState()` (llamado en `MainMenuManager.Start`): marca `TutorialPlayed`, completa lvls 1-2 (`Campaign_Level_1/2`), y sube oro a mínimo 500 (`StartingGold`). **DebugShortcuts** (persistente, DontDestroyOnLoad, activo con `ExpoConfig.Enabled || Application.isEditor`, Input legacy): **Ctrl+F** oculta/muestra SOLO los botones de testeo — `TutorialButton`, `CampaignButton`, `ChestButton`, `InsigniaButton`, `SpeciesButton`, `ScenarioButton`, `QuitButton` de la partida (bajo `TurnCanvas`) y todo lo que esté bajo `TestCanvas` (WIN/LOSE/SCORE/LVL9/LVLS). El `QuitButton` de pantalla final (`GameOverCanvas`) NO es test. Los botones de juego (PlayButton, SoundToggle, SkipTurnButton, BackButton, flechas `Arrow_*`, TrophiesButton, RankedButton) nunca se tocan. El estado se reaplica tras cargar escena; **Ctrl+R** = `PlayerPrefs.DeleteAll()` + estado post-sombras (TutorialPlayed + lvls 1-2) + `PlayCampaign(firstLevelDeCup1, WithPowerups)` directo (fallback lvl 3). Power-ups (ambos builds): sin spawn inicial doble — `InitPowerUpsDelayed` solo llama `StartAutoSpawn()`; `AutoSpawnTimer` ahora es loop (espera hasta que haya ≤2 esparcidos) con delays 28-34s (t>240) / 20-26s (t>120) / 15-20s (final), ×0.75 en expo; `SpawnOnBoard` elige tipo por cola rotatoria (`rotationQueue`, shuffle al vaciarse, usa disponible no-esparcido). Expo overrides: restricción por nivel de campaña ignorada (usa `allTypes`) y modo `WithoutPowerups` ya NO bloquea la recolección del azul (línea ~449). El marcador `Assets/Resources/ExpoBuild.txt` debe existir SOLO en el build de expo (creado — presente en repo); para el build completo (tutorial) hay que borrarlo antes de compilar. Si no está, el juego va completo con tutorial.
- **Devoluciones visuales (074)**: Feedback visual de Dani sobre EXE v0004 — solo visual, 0 fallos funcionales. `ExhibidorUI`/`InsigniaUI`: descripción de insignias eliminada (sin tooltip por ahora), nombre 10-11, checkmark 16-18 con outline verde oscuro, nav sin settings. `CampaignMapUI`: textos de card más adentro sin reducir fuente. `ModeSelectionUI`: tickets con espaciado parejo y título más abajo (sin cambio de colores de botón), oro del panel font 20 con outline. `TurnUI`: TimerText 24 naranja `(1,0.65,0.15)` + outline blanco, TurnTimerText 18, SoundToggle junto al oro, QuitButton `(-14,-120)`. `BattleResultUI`: nombres 22 MAYÚSCULAS, iconos chicos ocultos, números grandes 48, BlueResult/RedResult centrados en misma línea, stats/abilities rects 280. `CharacterCardUI`: dado 26px. `EconomyManager`: panel 220×52, bolsa 38px, goldText 20 con outline. `EnemyBanner`: font 16. Roslyn = 0 errores.
- **Devoluciones visuales 2 (074b)**: Segunda ronda de Dani. `DailyBonusUI`: outlines blanco→negro. `ExhibidorUI` nav: spacing 150 (back 360, chests 210, trofeos 60, ribbons -90, badges -240); badges icon x 40→72, título más abajo `(0.3,0.78)`; copas alineadas Y=72 uniformes espaciadas 165 (TUTORIAL -330, IRON -165, BLOOD 0, WILD 165, VOID 330); ribbon popup: fondo claro, overlay 0.55, content y=-80 (debajo del ribbons), textos legibles oscuros, título gold con outline negro. `CampaignMapUI`: número id a `(118,90)`, nombre a `(0,-30)`/`(-30,90)` centrado, check `(0,145)`. `TurnUI`: TurnTimerText outline negro `(2,-2)`, SoundToggle a la izquierda del oro `(-276,-46)`. `EconomyManager`: bolsa 40px y goldText 22 con outline negro, panel x=-66. `CharacterCardUI`: layout `[icono] "2" [dado 26px] "6+n"` con separación (10/54/80/116). Roslyn = 0 errores.
- **Devoluciones visuales 3 (074c)**: Tercera ronda de Dani + atajo. `CampaignMapUI`: el card es 340×150 (horizontal) — 074b dejó número/nombre en y=90 (FUERA). Rediseñado dentro: número `(98,30)` size(52,50), nombre agrandado font 10→13 `(-40,30)` size(220,50), datos `(-50,-35)` size(200,24), insignia (70,58) `(125,-35)`, check `(152,30)`. `ModeSelectionUI`: GOLD font 20→40 outline negro, rect 0.1→0.2; tickets card 320→416 (×1.3) con contenido ×1.3 (title 18, subtitle 13, price 21, desc 12, botón 221×71). `BattleResultUI`: AtkIcon/DefIcon ELIMINADOS por completo (antes ocultos con alpha 0). `TurnUI`: SoundToggle `(-288,-18)`. `CharacterCardUI`: dados alineados — AtkDice `(66,2)`, DefDice `(80,-1)`. **Atajo Ctrl+D**: `DebugShortcuts` → `DailyBonusUI.ShowForDebug()` (fuerza popup; monto real si hay, 100 si ya reclamado). Roslyn = 0 errores.
- **Fix powerup-mata-último-enemigo**: los powerups (Shake/Explosion/Fireball/Lightning/MAGIC) nunca verificaban la victoria → el juego quedaba atascado tras matar al último enemigo con ellos. `BoardManager.cs`: nuevos `CheckVictoryOnly()` y `DelayedShowScoreboard(Team)` (0.3s). `PowerUpManager.cs`: `board.CheckVictoryOnly()` al final de los 5 efectos. Roslyn = 0 errores.
- **DefDice corregido (074d)**: usuario midió y confirmó `(69,0)` 26×26 para OwnCard (antes `(88,-1)`); `AdjustEnemyCard()` re-posiciona `enemyCard/DefBlock/DefDice` a `(68,1)`. No se pisa ni queda bajo los números.
- **Numeración campaña global (074d)**: `CampaignMapUI.cs:260` muestra `level.id.ToString()` sin padding (era `(slot+1).ToString("D2")`, confundía 03-09 con 03/09 etc.) y el rect del número se amplió 52→70px: la fuente Press Start 2P (~30px/dígito a font 30) truncaba "10"-"22" mostrando solo el primer dígito ("1"/"2"). Ahora se ven "3"..."22" completos.
- **Botón badges alineado (074d)**: `ExhibidorUI.cs:102` quitado size custom `(332.1,132.8)` y scaleX 1.09 → usa default `(280,135)` igual que el resto de nav buttons; y=-240 (espaciado 150 uniforme).
- **Ribbons LevelName debajo del listón (074d)**: `ShowRibbonPopup`: LevelName `(0,-60)`→`(0,-35)`, CupName→`(0,-80)`, Date→`(0,-110)`. Roslyn = 0 errores.
- **Peones animados al moverse (074d)**: `BoardManager.cs` (~1470) el flag `useMoveAnim` solo incluía Paladin/Knight → peones (y Beastfolk back) se movían rígidos. Añadido `PieceType.Pawn` (`PeonMoveBack` tiene 4 frames y ahora anima). Ninja se deja fuera a propósito (`NinjaMoveCloud`).
- **Fix doble reward tras ganar nivel (074d)**: tras la victoria el pipeline corría DOS veces (contador + reward + GameOverUI de nuevo sobre el mapa de campaña, y el nivel quedaba con doble check). Causa: el timer de turno de `TurnManager` (20s) seguía corriendo y `ScoreboardUI.Show()` solo frenaba el timer de partida (`TimerManager.Stop()`) → a los ~20s `EndTurn()` re-disparaba `CheckVictoryAndEndTurn()` → segundo `Show()` (el guard `isShowing` ya se había reseteado en la línea 346 antes del reward) → `CompleteLevel` duplicado. Fix: `ScoreboardUI.Show()` ahora llama `TurnManager.PauseTimer()` (punto único por el que pasan todos los caminos de victoria: BoardManager, AI, ObstacleManager). Roslyn = 0 errores.
- **Fix tablero visible 1s al volver al menú (074d)**: tras ganar, BACK en el mapa de campaña mostraba el tablero con las piezas ~1s antes del menú. Causa real: durante `LoadScene("MainMenuScene")` no hay cámara activa (~1s de carga) → la pantalla conserva el último frame renderizado (el tablero). Fix: nuevo `SceneCover.cs` (helper estático): `SceneCover.Show()` crea overlay negro fullscreen `ScreenSpaceOverlay` (sortingOrder 999) con `DontDestroyOnLoad`; `CampaignMapUI.Close()` lo llama antes del load (sobrevive al swap de escena y tapa el frame viejo); `MainMenuManager.Start` hace `SceneCover.Clear()` para mostrarlo limpio. Guard `CampaignMapUI.loadsSceneOnClose` (true solo en `GameOverUI.OpenCampaignMap`) — el cover solo se muestra al cerrar con LoadScene, no cuando el mapa se abre desde el menú (ahí `OnClose` no carga escena y el overlay negro quedaría pegado). `CampaignMapUI.Close()` reordenado igualmente (mapa no se oculta antes del load). Roslyn = 0 errores.
- **Nav buttons de trofeos uniformes (074d)**: `ExhibidorUI.CreateNavButton`: eliminado `img.preserveAspect = true` — los sprites `panel total X` tienen aspectos distintos (badges 2.0, chests 2.24, ribbons 2.36, trofeos/back 2.15) y con preserveAspect cada botón dibujaba tamaño distinto (badges se veía chico/alejado). Ahora todos estiran a 280×135 idénticos. Roslyn = 0 errores.
- **Trofeos: ribbons restaurado + badges stretch (074e)**: quitar `preserveAspect` global distorsionaba ribbons (aspect 2.356 → estiraba +34% vertical, "se ve raro"). `CreateNavButton` ahora recibe `bool preserve = true`: chests/trofeos/ribbons/back vuelven a preserveAspect (como estaban); SOLO badges pasa `preserve: false` → estira full 280×135 e iguala visualmente a los demás (su aspect 2.008 casi no se distorsiona). Roslyn = 0 errores.
- **StartupVideo (074e)**: video de presentación Gamanbit tras el logo de Unity, antes del menú. (guard estático `shownThisSession`, 1 vez por sesión) crea canvas hijo del root (canvas debe ser CHILD del GO para que `Destroy(gameObject)` lo cascadee), `ScreenSpaceOverlay` (sortingOrder 250) DontDestroyOnLoad con `VideoPlayer` (`VideoRenderMode.RenderTexture` → RawImage fullscreen), fondo negro, skip por click (Button invisible + `GraphicRaycaster` obligatorio en el canvas o el click no registra) y fade-out 0.4s con flags `fading` (evita FadeOut doble por errorReceived+timeout) y null-guard al CanvasGroup. URL = `Application.streamingAssetsPath + "/Video/gamanbit.mp4"`. Short-circuit `File.Exists` en no-WebGL (RuntimePlatform.WebGLPlayer) → fade inmediato si no existe; si el archivo es inválido/vacío (0 bytes → WMF "empty file") `errorReceived` → FadeOut limpio (sin NRE) tras timeout 6s. Carpeta `Assets/StreamingAssets/Video/` creada. Callback desde el final de `MainMenuManager.Start`. **Fix WMF (video)**: el mp4 original tenía `color_primaries=0` (unknown) → warning de Windows Media Foundation "Color primaries 0 is unknown..."; las rutas intermedias (re-encode completo libx264, `-c copy -bsf:v h264_metadata=...`) no lograron reproducirse en Unity/WMF. Fix final: re-encode **máxima compatibilidad** `libx264 -profile:v baseline -level 3.0 -pix_fmt yuv420p -r 8 -movflags +faststart` + `-vf setparams=colorspace=bt709:color_primaries=bt709:color_trc=bt709` (H.264 Constrained Baseline con moov al frente y SEI BT.709, 48 KB — **probado, ya se ve**). Además `StartupVideo` blindado: RenderTexture asignada en `Start()` (antes de `Prepare`, orden robusto en Unity) en vez de re-crearla en `prepareCompleted`; `OnVideoError` ahora loggea `Debug.LogError("[StartupVideo] "+message)` para diagnosticar. Roslyn = 0 errores.

**Blink inmediato + texto susto (092)** *(implementado, verificación pendiente en Unity)*: `IdleBlinkLoop` ya NO espera 4s de inactividad — apenas `WelcomeCoroutine` termina `MovePanelSmall` (diálogo se achica y va a la izquierda) las piezas azules empiezan a parpadear el pulso azul (2.5s) con pausa de 1s entre pulsos, hasta que el jugador selecciona. En `ShadowPhaseTransition`, el texto "Shadows? Defeat them!!" (diálogo sorprendido, panel grande) se desplaza 8px a la derecha (`offetMin/offetMax.x += 8` tras `MovePanelBig`). Roslyn = 0 errores.

**Aura detrás del botón + GoblinDialogue (093)** *(implementado, verificación pendiente en Unity — PRIORIDAD de testeo)*: (1) `SpawnPremiumAura` (`ModeSelectionUI.cs`) ahora usa `auraRt.SetSiblingIndex(0)` en vez de `SetAsLastSibling()` — el anillo dorado del ticket PREMIUM ya NO se dibuja encima del botón (PlayBtn/títulos quedan por encima; el aura solo resalta el borde del card, `raycastTarget=false` intacto). (2) `GoblinDialogue` es de UNA sola vez: key `GoblinDialogueShown` (PlayerPrefs) — si ya se mostró en una sesión anterior, `ShowRewardThenProceed` lo salta y el goblin "no aparece". Para volver a verlo: borrar esa key (o `PlayerPrefs.DeleteAll()`) y ganar el tutorial de nuevo. El diálogo crea su propio Canvas (sortingOrder 215, GraphicRaycaster, ScreenSpaceOverlay, CanvasScaler 1920×1080) con fondo `Tutorial/GoblinDialogue` (LoadAll[0]), máquina de texto (0.02s/char) y botón OK (`Sprites/Menu/botin ui/botonOK` → `botonOK_0`); al OK → `MarkShown()` y continúa a `ShowCampaignModeSelection`. Roslyn = 0 errores.
- **Ícono del peón generado (074e)**: el usuario pidió usar `Sprites/Menu/menuHuman.PNG` (retrato del peón humano, 341×366 spriteMode 2, slice `_0` rect (6,5) 294×357, caja opaca 242×283 @ (35,49)). Exportado → `Assets/Icons/app_icon.png` (512×512, recorte caja opaca + padding 7%) y `Assets/Icons/app_icon.ico` (256×256 entrada PNG). Para usar: Project Settings → Player → Icon (Windows = .ico; Android = PNG; WebGL = favicon). Roslyn = 0 errores.
- **Botones de testeo por build (074e)**: `DebugShortcuts.DevBuild` = editor || desktop (Windows/Linux/OSX). En WebGL/APK (builds de subida) NO se crean los botones que oculta Ctrl+F: TestCanvas (WIN/LOSE/SCORE/LVL9/LVLS), TutorialButton, CampaignButton, ChestButton, InsigniaButton, RankedButton (menú), SpeciesButton/ScenarioButton/QuitButton (partida) y ResetCheat — creación gateada con `if (!DebugShortcuts.DevBuild) return;` en cada método. Los atajos (Update) también se apagan (`if (!DevBuild) return;` reemplaza `ExpoConfig.Enabled || Application.isEditor`). RankedUnlockSequence primero comprueba null (botón ausente). Fix además: `isWeb` huérfano en TurnUI → `SetActive(Application.platform != WebGLPlayer)`. En el build de PC (expo) todo igual + Ctrl+F/Ctrl+R/Ctrl+D. Roslyn = 0 errores.
- **Optimización de texturas (075)** *(implementado, verificación pendiente en Unity)*: reducción drástica del tamaño WebGL/APK. Origen: WebGL `.data.br` = 140 MB (descarga lenta al cargar web), APK Android = 179 MB. Causa raíz: 391 PNG en `Resources/` con `maxTextureSize: 2048`, compresión normal y **`crunchedCompression: 0`**; fondos de 2816×1536 (~8 MB c/u). Nuevo `Assets/Editor/TextureOptimizer.cs` con `[MenuItem("Optimize/Apply Texture Optimization")]` que recorre todos los `TextureImporter` de `Assets/Resources/**`, aplica `textureCompression=1` + **`crunchedCompression=1`** (compresión LZMA, se desempaqueta en runtime), fija `maxTextureSize` por clase (fondos/UI 1024, piezas 512, decor 512, Win/Tutorial 1024) conservando `spriteMode`/`alphaIsTransparency`/spritesheets, y reimporta con `ForceUpdate`. Redimensión física in-place a 1920×1080 de los fondos gigantes (`Human/Background/Human.png`, `Beastfolk/Background/BeastFolk.png`, `Orc/Background/BackgroundOrco.png`, `Nigromantes/Background/Nigromantes.png`, `Human/Background/fondo3`/`fondo2`, `Tutorial/fondoTuto`, `Win/RedWin`/`BlueWin`, diálogos `Tutorial/dialogue*`/`GoblinDialogue`) — de ~8 MB c/u a ~1-2 MB. Eliminados los 12 PNG legacy de `Assets/Prefabs/Pieces/` (`PeonFront/Back`, `CaballeroFront/Back`, `NinjaBack`, `PaladinFront/Back` — no usados, fuera de Resources así que no entraban al build, limpieza). Los decor por especie (`Escudo`, `Espada`, `DiceTable`, `Farol`, `Pared`, etc.) NO se deduplican — aunque comparten nombre, cada tema (Human/Orc/Beastfolk/etc.) tiene arte distinto. Los sprites de sombra (`Tutorial/*Sombra*.PNG`) y paredes/decor sin uso los borró el usuario manualmente (el código no referencia ningún `*Sombra*`; las sombras se hacen con sprites Human vía `GetPieceColor`). Audio (`Sounds/Fondo/*.mp3`, ~21 MB) queda fuera esta iteración (requiere ffmpeg para recomprimir). Meta: `.data.br` ≤ 50 MB (estimado 25-40 MB).
- **Fixes visuales power-ups/ranked (076)** *(implementado, verificación pendiente en Unity)*: Iconos de power-ups rotos (recortados) — causa raíz: import Multiple con slice rect mayor que el PNG → los 5 íconos (`Shake.PNG`, `Explosion.PNG`, `Fireball.PNG`, `Lightning.PNG`, `cambio.png`) se pasaron a `spriteMode: 1` (Single) para que `Resources.Load` use la textura completa. Efectos degradados por 075 (512px + crunch LZMA → arte borroso/artefactos): `Efect/punio`, `ritual`, `fuego1/2`, `rayo1/2`, `magoIdle`, `magoAttack`, `magoback`, `trumpet` → `maxTextureSize: 2048` + `crunchedCompression: 0`. `punio.png` además tenía slice rect (1049×731) > PNG (1024×678) → a Single; `ShakeWithFist` ajusta el pivot: si el sprite tiene pivot centro, `fistRoot = impactPos` (sin restar half). Escalas Nigromantes: jump front (0.30,0.36)→(0.26,0.31), back (0.23,0.27)→(0.20,0.24); ninja move (0.38,0.32)→(0.44,0.37); pawn move *0.9 via sizeRatio en `AnimatedMove`. Botón RANKED: `MainMenuManager.BuildRankedButton` usa sprite `Sprites/Menu/ranked` (`ranked_0`, slice; fallback rect plano) y `GameOverUI.RankedUnlockSequence` corrige case `Sprites/Menu/ranked`+`ranked_0` (antes `Ranked`+`Ranked_0` → fallback botonOpen). Alineación ranked: en win/lose el Retry va centrado (0,-374) y Menu/Next a la derecha (167,-374). Trompeta en `BattleResultUI` reposo `(760,-450)`→`(880,-380)` y slide-in `(1500,-450)`→`(1600,-380)`. Validación: compilación Roslyn = 0 errores.
- **Iconos power-ups robusto (077)** *(implementado, verificación pendiente en Unity)*: pickup seguía cayendo al fallback procedural ("no llama a los sprites de los iconos"). `LoadIcon()` ahora añade fallback `Resources.Load<Texture2D>` + `Sprite.Create` (rect completo, pivot centro) cuando ninguno de Load/LoadAll devuelve sprite; los 5 metas `Icon/*` (`Shake`, `Explosion`, `Fireball`, `Lightning`, `cambio`) pasan `isReadable: 1` para que ese fallback siempre funcione (independiente del modo de import Single/Multiple). Validación: compilación Roslyn = 0 errores.
- **Iconos centrados + trompeta + NinjaMoveBack (078)** *(implementado, verificación pendiente en Unity)*: Icono del pickup descentrado porque el offset asumía pivot bottom-left; ahora usa el pivot real del sprite `((pivot - 0.5) * size * scale)` en `SpawnSpecificAt` → centrado con el Glow. Trompeta reposo `(880,-380)`→`(802,-380)` (420×330). `NinjaMoveBack.PNG` (Nigromantes) tenía slice 1163×750 > PNG 1024×661 (sprite roto) → meta a Single + isReadable 1 + maxTextureSize 512/crunch 0. Escala ninja Nigromantes moviéndose `(0.44,0.37)`→`(0.7,0.7)` en `AnimatedMove`. Validación: compilación Roslyn = 0 errores.
- **Fix pivots (079)** *(implementado, verificación pendiente en Unity)*: Revisión de Claudio — `Sprite.pivot` en Unity viene en **píxeles**, no como fracción 0–1. `PowerUpManager.cs` nuevo helper `NormalizedPivot(s)` (`pivot.x/rect.width`, igual Y) + `IconLocalPos(s,sizeX,sizeY,scale)`. Aplicado a todos los offsets que asumían pivot esquina: `SpawnSpecificAt`, `SpawnOnBoard` y `AnimateSpawned` (iconos ahora centrados con el Glow; antes restaban `-half` → descentrado), `ShakeWithFist` (puño; antes comparaba `pivot.x` en píxeles contra 0.5 → nunca centraba), `MageAndProjectile` y `MageLightning` (mago: quitada resta `magePos - half`), `SpawnRitualCircle` (círculo ritual: quitada resta de half). `TutorialManager` iconos idem. Con pivot centrado el offset normalizado da (0,0). Validación: compilación Roslyn = 0 errores.
- **Fixes reporte usuario (080)** *(implementado, verificación pendiente en Unity)*: (1) Trompeta reposo `(802,-380)`→`(812,-380)` — unos px a la derecha para que al pulsar a 1.06× (bounce) no se corte el borde derecho. (2) Cartel "ENEMY TURN" (lvl 22 contra Nigromantes no salía): `TurnManager.ShowEnemyTurnBanner` ya NO usa `FindFirstObjectByType<Canvas>` (parent param nunca se usaba; el banner ya era root) — se elimina esa dependencia frágil, sortingOrder 200→220 y delay inicial 2.5s→1.5s. (3) `EnemyBanner` (nombre del ejército al inicio): misma fragilidad `FindFirstObjectByType<Canvas>` + SetParent → si no hay canvas crea uno propio root (ScreenSpaceOverlay sortingOrder 219 + CanvasScaler 1920×1080, patrón BattleResultUI/AutoPlayManager). (4) Puño (punio): el arte re-subido quedó como `punio 1.png` (slice 1049×731) y el código cargaba `punio` (1024×678 viejo) → nuevo helper `LoadBestSprite(basePath)` en `PowerUpManager` que prueba `path` y `path + " 1"` eligiendo el de mayor área (usa el nuevo). (5) Ninja nigromante: igual caso — `NinjaMoveBack 1.PNG` (slice 1163×750) ignorado porque `BoardManager.LoadSprites` solo probaba `NinjaMoveBack` (1024×661) → `LoadSprites` ahora itera candidatos `{name+suffix+dir}`, `{name+suffix+dir} 1` (y sin sufijo idem) tomando el de mayor área total por carpeta. El front (`NinjaMoveFront` 1163×751) ya se cargaba bien. Validación: compilación Roslyn = 0 errores.
- **Escala ninja nigromante (081)** *(implementado, verificación pendiente en Unity)*: `BoardManager.cs:1466` escala de movimiento del ninja nigromante `(0.7,0.7)` → `(0.14,0.14)` (solicitud del usuario).
- **GameOverUI ranked botones (081)** *(implementado, verificación pendiente en Unity)*: en RANKED, victoria y derrota tenían botones encimados (~275px de solape) y sprites cruzados. Ahora: Retry (Retry_0) a `x=-170` → `PlayRanked`, Menu a `x=170` → `MainMenuScene`. Victoria: Menu usa `quitSprite` (antes flecha `nextSprite` engañosa) con fallback a nextSprite si es null. Derrota: Menu usa `quitSprite` (antes `retrySprite` con acción de menú). Sin solape (gap 102px). Validación: dotnet build Assembly-CSharp.csproj = 0 errores.
- **punio**: verificado OK en Unity (cierra el punto (4) de 080).
- **Feedback usuario (082)** *(implementado, verificación pendiente en Unity)*: (1) Doble-tap durante el conteo del scoreboard → `skipRequested` automático (nueva ventana `DOUBLE_TAP_WINDOW` 0.45s en `ScoreboardUI.Update()`, detecta touch+mouse). (2) Botones de DERROTA más separados (tocaban en x=54): Retry `-167`→`-280`, Quit `156`→`260` (no-ranked); ranked Retry `-170`→`-280`, Menu `170`→`280`. (3) Cartel "RANKED UNLOCKED!" en campaña completa ahora SOLO la 1ª vez (`PlayerPrefs.RankedUnlockPopupShown`, se marca y guarda antes de mostrarlo) — antes aparecía tras ganar cualquier partida con la campaña al 100%. (4) En campaña completa, el botón flecha (Next) ya NO sale al menú principal: ahora `OpenCampaignMap(-1)` (mapa para rejugar niveles), Quit sigue saliendo al menú. Validación: dotnet build Assembly-CSharp.csproj = 0 errores.
- **Economía sin boot (088)** *(implementado, verificación pendiente en Unity)*: balance aprobado con el usuario (no x10). (1) `EconomyData.json`: `levelEntryCosts` cup1 0→5 (quedan 5/10/20/30; Cup0/niveles 1-2 gratis). PREMIUM de campaña cableado: nuevo `GetCampaignEntryCost()` en `ModeSelectionUI` resuelve la copa del nivel destino (`CampaignData.GetLevel(targetLevelId).cup`, fallback cup1) y usa `EconomyConfig.GetLevelEntryCost` → costo 5/10/20/30 por copa en display y cobro (reemplaza el plano 20); el precio del ticket se muestra en el `CreateTicketCard` sin cambios de formato. (2) `ScoreboardUI`: reward ranked por victoria azul (`GameConfig.isRanked && !isAutoPlay`) = `currentPowerupMode == WithPowerups ? 15 : 10` vía `AddGold`; migaja de rejugada: `wasCompleted` capturado ANTES de `CompleteLevel` → si jugás un nivel ya completo con ticket FREE (`WithoutPowerups`) → `AddGold(5)`. (3) Peón Nigromantes idle: `GetIdleScale` `(0.38,0.39)`→`(0.36,0.36)` y en `PlacePiece` + `AnimatedMove` lift dinámico `NigroPawnLift(sr)` = `max(0, halfSpriteH - cellSize/2)` para `SpriteFolder(species)=="Nigromantes" && type==Pawn` (pies al borde inferior de la casilla, evita pisarse y no flota; corregido el `0.63f` fijo que la dejaba muy arriba). (4) `EconomyManager` feedback visual: `FormatGold` estático con separador de miles por punto (usado también en `ModeSelectionUI` GOLD), count-up animado 0.5s con guard (`countUpRoutine`/`displayedGold`, se corta en `UpdateGoldHUD`), popup flotante "+N" sobre el panel (fade+subida 0.9s). (5) `docs/economia.md` actualizado: entradas campaña 325g, ranked +15/+10, migaja +5. (6) **Aura PREMIUM** (2 partes): al comprar un ticket PREMIUM `ModeSelectionUI` spawna `PremiumAura`, que en 091 quedó como aureola dorada en el BORDE del panel (Image hijo de la card con anclas stretch sobre el rect del card, sprite `BuildGoldBorderSprite` = anillo rectangular procedural 256×256 (banda 8%) con glow suave, `SetAsLastSibling` raycastTarget=false, pulso alpha 0.24-0.40 + escala 1.00-1.03; extraído de `FlashAndStartCampaign` → nuevo `FlashCard` compartido) y ranked PREMIUM ahora pasa por `FlashAndStartRanked` (flash + 0.3s antes de `PlayRanked`; ticket FREE queda instantáneo como antes); nuevo `PremiumHalo.cs` — halo dorado pulsante sobre las piezas AZULES durante toda la partida PREMIUM (`PremiumHalo.Attach` desde `CreatePieceVisual`, condición `PremiumHalo.Active()` = con-powerups && !tutorial && !autoplay): sprite radial procedural, sortingOrder pieza−1, sigue a la pieza cada frame y escala con su footprint ×1.25, alpha 0 (oculto) si la pieza está inactivada; se autodestruye si la pieza muere. `Assembly-CSharp.csproj`: include de `PremiumHalo.cs`. Validación: dotnet build Assembly-CSharp.csproj = 0 errores.


### Key Decisions

#### Blocked
- Rey Cabezón (spec 061) dado de baja por el cliente — queda como draft para futuro. Prioridad post-060: pulir arte y sonido.

- **Escalas nigromantes + aura borde (091)** *(implementado, verificación pendiente en Unity)*: nigromantes un poquito más chicos en idle y move back (×0.94): `GetIdleScale` Peón `(0.36,0.36)`→`(0.34,0.34)`, Knight `(0.65,0.64)`→`(0.61,0.60)`, Paladin `(0.77,0.71)`→`(0.72,0.67)`; Ninja idle `(0.38,0.32)`→`(0.36,0.30)` (solo idle; move queda `(0.16,0.16)`). En `AnimatedMove` el move-back de nigromantes (no ninja) ancla ×0.94 extra sobre la compensación por área; ninja move `(0.14,0.14)`→`(0.16,0.16)`. `NigroPawnLift` recalcula solo con el idle 0.34. Aura del ticket PREMIUM rediseñada: `SpawnPremiumAura` dibuja un anillo dorado en el BORDE del panel (Image stretch al rect del card, sprite `BuildGoldBorderSprite` = anillo cuadrado procedural 256×256 adherido EXACTAMENTE al borde (alpha 0 en el centro — antes el centro valía 1 y teñía todo el ticket de amarillo; ahora solo brilla en las orillas con falloff hacia adentro `thicknessN=0.05`), `SetAsLastSibling`, raycastTarget=false, pulso alpha 0.04-0.09 + escala 1.00-1.02) en vez del glow radial central 430×430. El aura se muestra ahora en los tickets PREMIUM (cost>0) desde su creación (`CreateTicketCard` → `SpawnPremiumAura`), NO tras la compra — se eliminó el spawn post-purchase en `OnCampaignTicketSelected`/`OnTicketSelected` (dejaba un "manchado de mostaza" tras elegir). Validación: dotnet build Assembly-CSharp.csproj = 0 errores.


### Key Decisions
- Win images accessed via `winSprites[0]` (AI-generated internal names).
- Particles on separate canvas (sortingOrder 201) to render above ScreenSpaceOverlay.
- Next button advances Human→Orc→Beastfolk→MainMenu via `GameConfig.selectedScenario`.
- AI subscribes to `TurnManager.OnTurnChanged`.
- Combat formula: `2d6 + mod` — ATK always `+1`, DEF varies by piece (Peon=0, Ninja=1, Knight=2, Paladin=3).
- Card sprites: universal `{Type}Carta` without species suffix (e.g. `PeonCarta`), loaded via `LoadAll`.
- Timer: 5 minutes default per match, displayed in TurnUI.
- ScoreboardUI triggered before GameOverUI: victory check calls `ScoreboardUI.Instance.Show()` which animates then delegates to `GameOverUI.Instance.Show()`.
- Scoreboard panels use `LoadFirstSprite` (index 0) pattern from `Resources.LoadAll<Sprite>`. Piece rows anchored at parent top-left `(0, 1)` with absolute positions.
- Cup tracks victories (match wins): 2 wins needed to unlock next world. `RecordMatchWin()` called from ScoreboardUI on Blue win. Cup text shows "W {v}/2".
- Obstacles: `DestroyedCell` walkable until collapse (4 turns) → becomes `Hole` (blocks movement). `Glue` traps piece 2 turns (can only attack). `Mine` 3×3 explosion on contact. All obstacles load with `Resources.LoadAll<Sprite>` (spriteMode 2).
- `ObstacleManager.OnTurnChanged` uses `new Dictionary<int,int>(gluedPieceTurns)` copy to avoid `InvalidOperationException` during enumeration.
- `GetIdleScale` is single source of truth for all piece scales — `CreatePieceVisual` and `ResetPieceSprite` both call it.
- `CampaignManager` uses PlayerPrefs (`Campaign_Level_{id}`) for persistence. Level 1 always unlocked, others require previous completed. `CompleteLevel()` awards gold. GameOverUI shows Next button in campaign mode (loads next uncompleted level or MainMenuScene).
- `CampaignUI` is a component added to MainMenuManager, not a separate scene. Back button destroys the panel. CampaignManager is created in both MainMenuManager and GameManager to ensure availability.
- `ChestManager` uses PlayerPrefs (`ChestSlot_0`, `ChestSlot_1`) with timestamp format `1|unixTimestamp`. 2 max slots, 8h open time. Drop chance scales with campaign level (40% lvl1-9, 55% lvl10-15, 70% lvl16+). Chest insignias weighted: common 50%, rare 30%, epic 15%, legendary 5%.
- `InsigniaManager` uses PlayerPrefs (`Insignia_{id}`) for collection tracking. 35 total: 22 campaign (auto-granted on level complete) + 13 chest (random from pool). 4 rarities: common, rare, epic, legendary.
- `ChestUI` shows 2 slots with timer, progress bar, OPEN button when ready, reward popup with rarity colors.
- `InsigniaUI` shows scrollable grid with 3 filters (ALL/CAMPAIGN/CHESTS), 5 columns, colors by rarity. Collected badges show name+description, uncollected show "???"
- Power-ups ALWAYS spawn on board — the mode difference is whether the PLAYER can use them (enemies always can)
- Ribbon colors by world: Human=blue, Orc=green, Beastfolk=brown/green, Nigromantes=violet; shades from light→dark within each world
- Chest reward system: prioritize uncollected insignias (80% new if <50% collected, scaling to 20% new if >80%), always gives gold + 3 insignias, duplicates get "DUPLICADA!" red seal
- Ranked mode: random opponents, power-ups per enemy race, 1 random obstacle per match
- Goblin dialogue appears post-tutorial/shadows (after level 2), explains free power-ups, one-time only
- Trophy display replaces the current trophy shelf in main menu — full-screen with cups (left), ribbons+insignias (right)
- Enemy name banner shows at start of each campaign level with the army name
- Character unlock by completing a full cup (dialogue announcement)
- ModeSelectionUI costs: Campaign=levelEntryCosts, Ranked WITH=30g, Ranked WITHOUT=15g
- Cup completion: `CompleteLevel()` returns race name (or null) on first-time cup completion. CampaignRewardUI shows cup sprite with bounce animation (0.3→1.6→1.0) at end of reward sequence.
- Knight jump: triggered when Knight moves >1 cell. Jump scale per race via `GetJumpScale`: Human 0.60/0.5 front · 0.09/0.09 back, Orc 0.55/0.6 front · 0.33/0.34 back, Beastfolk 0.66/0.52 front · 0.41/0.34 back, Nigromantes 0.15/0.12. `SpritePrefix` maps "Beastfolk"→"beast". KnightJump runs its own Salto1/Salto2 sprites; `AnimatedMove` skips launching the Move animation (`PlayAnimation`) when `useKnightJump` to avoid a parallel `AnimateSprites` coroutine overwriting `sr.sprite` mid-flight.
- Paladin light beam: procedural rect follows visual during movement, 0.15s fadeIn, 0.6s hold with flicker, 0.3s fadeOut, 6 holy sparks. Slide 1.3s for Paladins.
- Fight cloud replaces attack sprite animations during combat. Sprites at `Sprites/PowerUps/Efect/FightCloud_{species1}{species2}{1|2|3}`. Species sort order: Human=0, Orc=1, Beast=2 (not alphabetical). Scale normalized to 251px reference. Sprites < 50px filtered out. Cloud at defender cell (`toPos`), sortingOrder 15. Both pieces hidden during cloud; loser silently destroyed, winner reset and shown.

### Bugs (no arreglados)
- **MissingReferenceException en power-ups**: `AnimateAttackLunge` sin null checks. Sombra destruida durante power-up → coroutine accede a `visual.transform` → crash. Causa: faltaban `if (visual == null) yield break;` tras cada yield.
- **Scoreboard win condition**: Si un equipo tiene 0 piezas y el otro solo Peones (PointValue=0), el scoreboard declaraba ganador al equipo vacío por `blueTotal >= redTotal` (0 >= 0 = true). Causa: no verificaba si realmente quedaban piezas vivas.
- **Paladin attack sprites**: `FilterSprites` eliminaba el frame 2 (169×169) por umbral 1.5×, dejando solo 2 frames. `PlayAnimation` requiere ≥3 frames, así que nunca se animaba. Causa: umbral 1.5× muy bajo para sprites con armas extendidas.

### Next Steps
1. ~~Tester feedback and adjustments.~~ (done)
2. ~~Card sprites: crear {Type}Carta universales.~~ (done)
3. ~~Combat overhaul 2d6+mod.~~ (done)
4. ~~Timer + Scoreboard.~~ (done)
5. ~~Attack pose → selection pose.~~ (done — no change needed, highlight sortingOrder fixed)
6. ~~Trophy unlock persistence (020).~~ (done)
7. ~~Orc/Beastfolk worlds (021/022).~~ (done — data-driven, same scene)
8. ~~Wire Next button to advance worlds (023).~~ (done)
9. ~~Tutorial (033)~~ (done)
10. ~~Game Hook (034)~~ (done)
11. ~~Power-up sounds & effects (036)~~ (done)
13. **Campaña, economía y progresión** (045): 22 niveles, 4 copas, oro, cofres, insignias, MAGIC!, obstáculos nuevos.
    - ~~Fase 1: Data Models~~ (done — CampaignData.json, InsigniaData.json, EconomyData.json + C# classes)
    - ~~Fase 2: Economía~~ (done — EconomyManager, DailyBonusUI, gold HUD con bolsa + pulso + monedas)
    - ~~Fase 3: MAGIC!~~ (done — powerup convierte enemigo a Peón, estrella púrpura, efectos visuales)
    - ~~Fase 4: Obstáculos~~ (done — DestroyedCell, Glue, Mine)
    - ~~Fase 5: CampaignManager + CampaignUI~~ (done — CampaignManager con PlayerPrefs, CampaignUI scrollable con copas/niveles, ScoreboardUI→CompleteLevel, GameOverUI Next en campaña)
    - ~~Fase 6: Cofres + Insignias~~ (done — ChestManager, ChestUI, InsigniaManager, InsigniaUI, MainMenu buttons)
    - Fase 6b: Sprites cofre/insignias + recompensas mixtas (oro+powerups) — pendiente
    - Fase 7: Ranking libre — pendiente
14. **SPEC-046: Rediseño de Recompensas** (done): 4 modos, exhibidor, listones, insignias ranked, goblin onboarding.
    - ~~CampaignData.json actualizado~~ (done — power-ups/obstáculos específicos del doc del diseñador)
    - ~~GameConfig GameMode/PowerupMode~~ (done — enums, PlayRanked, flag isRanked)
    - ~~Power-ups siempre spawn~~ (done — WithoutPowerups bloquea solo jugador)
    - ~~RibbonManager~~ (done — listones por victoria, colores por mundo)
    - ~~ExhibidorUI~~ (done — pantalla completa reemplaza estante)
    - ~~ChestManager con oro + progresión~~ (done — ChestReward, algoritmo 80/50/20%)
    - ~~EnemyBanner~~ (done — fade-in/out con nombre enemigo)
    - ~~RankedManager~~ (done — power-ups por raza, obstáculo random) ~~ModeSelectionUI~~ (done — 4 opciones con costos)
    - ~~GoblinDialogue~~ (done — onboarding post-tutorial)
    - ~~Desbloqueo de personaje por copa~~ (done — CheckCupCompletion)
15. **Peso WebGL no alcanzado** *(pendiente de decisión)*: target `.data.br` ≤ 50 MB para CrazyGames (optimización 075 bajó de ~140 MB, falta llegar a la meta). Candidatos a evaluar: recompresión de audio `Sounds/Fondo/*.mp3` (~21 MB) con ffmpeg (bitrate/bajar a mono/44.1k), ajuste de `maxTextureSize`/crunch en piezas restantes, streaming de Assets por tipo, o limpiar sprites huérfanos en `Resources/`.

### Relevant Files
- `Assets/Scripts/Game/CharacterCardUI.cs` — card UI (hover/selection)
- `Assets/Scripts/Game/AIController.cs` — AI logic
- `Assets/Scripts/Game/GameManager.cs` — bootstrap, enemy banner
- `Assets/Scripts/Game/BoardManager.cs` — board, movement, victory check, kill effects, combo
- `Assets/Scripts/Game/InputManager.cs` — player input, deselect on turn change
- `Assets/Scripts/Game/TurnManager.cs` — turn state machine
- `Assets/Scripts/Game/GameOverUI.cs` — win/loss screen, particles
- `Assets/Scripts/Game/MainMenuManager.cs` — menu, trophies button, mode selection
- `Assets/Scripts/Game/SoundManager.cs` — audio, trumpet
- `Assets/Scripts/Game/CombatManager.cs` — dice combat
- `Assets/Scripts/Game/BattleResultUI.cs` — combat result popup
- `Assets/Scripts/Game/TestButtons.cs` — temp GANAR/PERDER
- `Assets/Scripts/Game/TurnUI.cs` — turn HUD
- `Assets/Scripts/Game/PieceType.cs`, `Team.cs`, `PieceData.cs`, `Tile.cs` — data models
- `Assets/Scripts/Game/TimerManager.cs` — match timer
- `Assets/Scripts/Game/ScoreboardUI.cs` — end-of-match scoreboard, records match win
- `Assets/Scripts/Game/PowerUpManager.cs` — power-up system (shake, explosion, fireball, lightning)
- `Assets/Scripts/Game/TutorialManager.cs` — tutorial flow
- `Assets/Scripts/Game/TutorialCollectibles.cs` — tutorial collectibles (copa/insignia/listón) persistence
- `Assets/Scripts/Game/TutorialRewardUI.cs` — tutorial reward popup (cofre + 150 oro + coleccionables)
- `Assets/Scripts/Game/GameHookManager.cs` — game hook events
- `Assets/Scripts/Game/CoinManager.cs` — coin/cup system (victory-based: 2 wins to unlock)
- `Assets/Scripts/Game/EconomyManager.cs` — gold, daily bonus, HUD (bolsa + pulso + monedas)
- `Assets/Scripts/Game/DailyBonusUI.cs` — daily bonus popup
- `Assets/Scripts/Game/CampaignData.cs` — campaign level data loader
- `Assets/Scripts/Game/InsigniaData.cs` — insignia data loader
- `Assets/Scripts/Game/EconomyConfig.cs` — economy config loader
- `Assets/Scripts/Game/ObstacleManager.cs` — obstacles: DestroyedCell (timer+collapse), Glue (trap), Mine (3×3 explosion), Roca (static). CollapseDestroyedCell does NOT call EndTurn (prevents enemy double-turn).
- `Assets/Scripts/Game/CampaignManager.cs` — campaign progression: level unlock/completion via PlayerPrefs, gold rewards, ribbons, cup completion
- `Assets/Scripts/Game/CampaignUI.cs` — campaign map: scrollable panel with cups, level buttons, back button
- `Assets/Scripts/Game/CampaignMapUI.cs` — campaign map paginated: cards, lock, background per race, swipe pages, head animation, Nigromantes background
- `Assets/Scripts/Game/ChestManager.cs` — chest system: 2 slots, 8h timer, drop rates, gold+insignia rewards, progressive algorithm
- `Assets/Scripts/Game/ChestUI.cs` — chest panel: slot visuals, timer, progress bar, open button, sequential reveal with DUPLICADA! seal
- `Assets/Scripts/Game/InsigniaManager.cs` — badge collection: tracking, grant from campaign/chests, PlayerPrefs
- `Assets/Scripts/Game/InsigniaUI.cs` — collection screen: scrollable grid, rarity filters, colors by rarity
- `Assets/Scripts/Game/GameConfig.cs` — game config: GameMode/PowerupMode enums, PlayCampaign, PlayRanked, isRanked
- `Assets/Scripts/Game/RibbonManager.cs` — ribbons per campaign victory, colors by world, procedural sprites
- `Assets/Scripts/Game/ExhibidorUI.cs` — full-screen trophy display: cups, ribbons, insignias
- `Assets/Scripts/Game/EnemyBanner.cs` — army name banner at campaign level start
- `Assets/Scripts/Game/RankedManager.cs` — ranked mode: random enemies, power-ups by race, obstacle
- `Assets/Scripts/Game/ModeSelectionUI.cs` — 4-mode selection menu with gold costs
- `Assets/Scripts/Game/GoblinDialogue.cs` — onboarding dialogue post-tutorial
- `Assets/Scripts/Game/CampaignRewardUI.cs` — campaign reward popup with game juice, sequential reveals, cup completion animation
- `Assets/Resources/Data/CampaignData.json` — 22 levels, 4 cups, specific power-ups/obstacles per level
- `Assets/Resources/Data/InsigniaData.json` — 35 badges (22 campaign + 13 chest)
- `Assets/Resources/Data/EconomyData.json` — costs, daily bonus, chest config
- `docs/specs/018-red-ai.md` — AI spec
- `docs/specs/033-tutorial.md` — tutorial spec
- `docs/specs/034-game-hook.md` — game hook spec
- `docs/specs/046-reward-redesign.md` — reward redesign spec

### Code Style
- **C#**: PascalCase for methods/properties, camelCase for fields, `_camelCase` for private fields.
- Prefer `FindFirstObjectByType` over serialized references.
- UI created programmatically in `Start()`; use `Resources.Load` for sprites/fonts/sounds.
- No comments in code.

---

## 1. Proyecto

**Dice Clash Tactics** — Juego de táctica posicional en tablero 7x7 con duelos de dados.

### Stack

| Capa       | Tecnología                     |
| ---------- | ------------------------------ |
| Game Engine| Unity 6000.0.71f1 (URP 2D)    |
| Lenguaje   | C#                             |
| Input      | Unity Input System 1.19.0      |
| Plataforma | WebGL (CrazyGames) → Android   |

### Estructura

```
Assets/
├── Scripts/
│   └── Game/
│       ├── PieceType.cs
│       ├── Team.cs
│       ├── PieceData.cs
│       ├── Tile.cs
│       ├── BoardManager.cs
│       ├── InputManager.cs
│       └── TurnManager.cs
├── Prefabs/
│   └── Pieces/
├── Scenes/
└── Settings/
```

### Quick Start

```bash
Abrir proyecto en Unity 6000.0.71f1 → Abrir Scenes/SampleScene → Play
```

### Idioma

- Documentación: **es**
- Código y términos técnicos: inglés

---

## 2. Workflow SDD

Metodología: **Spec-Driven Development**. Cada unidad de trabajo es un spec. No se implementa nada sin spec previo.

Rama base: `main`

### Ciclo de vida de un spec

1. Tomar el item con estado `next` en `docs/WORKLOG.md`.
2. Crear `docs/specs/{{NNN}}-{{slug}}.md` usando `.templates/specs/spec-template.md` como base.
3. Cambiar el estado del item a `in-progress` en el worklog.
4. Crear la rama `spec/{{NNN}}-{{slug}}` e implementar solo el alcance definido.
5. Verificar localmente (tests / build).
6. Abrir PR referenciando el spec. **Esperar aprobación antes de mergear.**
7. Al hacer merge, cambiar estado a `done` y promover el siguiente item de `backlog` a `next`.

---

## 3. Convenciones

### Specs

- Archivo: `docs/specs/<NNN>-<slug>.md`
- Base: `.templates/specs/spec-template.md`
- Contenido mínimo: Contexto · Objetivo · Alcance (in/out) · Criterios de aceptación

### Ramas

- Formato: `spec/<NNN>-<slug>`
- Un spec por rama. No mezclar cambios de distintos specs en la misma rama.
- Si el alcance cambia, actualizar el spec antes de modificar el código.

### Commits

- **No commitear hasta que el usuario lo solicite explícitamente.**
- Formato: `<tipo>(<alcance>): <mensaje>`
- Tipos válidos: `feat` · `fix` · `refactor` · `docs` · `chore` · `test`
- Incluir referencia al spec en el cuerpo del commit cuando aplique.

### Pull Requests

- Título: `[SPEC <NNN>] <resumen>`
- Target: `{{rama base}}`
- Debe incluir: enlace al spec · checklist de criterios de aceptación · evidencia de verificación · screenshots si hay cambios visuales.

### Code Style

<!-- Completar con las convenciones del proyecto -->

**{{Lenguaje principal}}:**

- {{convención 1}}
- {{convención 2}}

---

## 4. Reglas

### Para implementar

- Trabajar solo dentro del alcance del spec activo. Sin refactors amplios fuera del spec.
- Preservar la estructura existente del proyecto.
- No agregar dependencias nuevas salvo que el spec lo indique explícitamente.
- Antes de crear o modificar modelos, revisar `docs/data-model/ERD.md`. No agregar campos que no estén definidos allí.
- Si el spec tiene definiciones ambiguas o incompletas, **pausar y consultar** antes de implementar.

### Prohibido

- Implementar sin spec.
- Commitear o hacer push sin autorización explícita del usuario.
- Commitear archivos de secretos (`.env`, credenciales, claves).
- Hacer push directo a `main` sin autorización.
- Modificar migraciones ya aplicadas.

---

## 5. Verificación

> Comandos que el agente debe ejecutar para validar que no rompió nada. Completar con los comandos reales del proyecto.

| Tipo      | Comando                                           |
| --------- | ------------------------------------------------- |
| Build     | `Ctrl+B en Unity (Build Player)`                  |
| Lint      | N/A                                               |
| Typecheck | N/A                                               |

> Eliminar las filas que no apliquen y completar con los comandos reales del proyecto.

---

## 6. Referencias

| Documento    | Ruta                     |
| ------------ | ------------------------ |
| Architecture | `docs/architecture/`     |
| Data Model   | `docs/data-model/ERD.md` |
| Specs        | `docs/specs/`            |
| Worklog      | `docs/WORKLOG.md`        |
