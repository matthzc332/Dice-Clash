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

### Key Decisions

#### Blocked
- *(none)*

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

### Bugs (no arreglados)
- **Ninja idle scale**: Después de un ataque, la ninja se queda con escala 0.6 (sombra) en vez de restaurar su escala original (~0.37/0.45). Causa: `originalScales` keyeado con `GameObject.GetInstanceID()` en `AnimatedMove` pero buscado con `SpriteRenderer.GetInstanceID()` en `ResetPieceSprite`.
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
    - ~~RankedManager~~ (done — power-ups por raza, obstáculo random)
    - ~~ModeSelectionUI~~ (done — 4 opciones con costos)
    - ~~GoblinDialogue~~ (done — onboarding post-tutorial)
    - ~~Desbloqueo de personaje por copa~~ (done — CheckCupCompletion)

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
- `Assets/Scripts/Game/ObstacleManager.cs` — obstacles: DestroyedCell (timer+collapse), Glue (trap), Mine (3×3 explosion), Roca (static)
- `Assets/Scripts/Game/CampaignManager.cs` — campaign progression: level unlock/completion via PlayerPrefs, gold rewards, ribbons, cup completion
- `Assets/Scripts/Game/CampaignUI.cs` — campaign map: scrollable panel with cups, level buttons, back button
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
