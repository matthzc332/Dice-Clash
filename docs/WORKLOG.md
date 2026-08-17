# Worklog

Registro de trabajo y backlog del proyecto.

---

## Convenciones

- ID de tarea: `<NNN>-<slug>` (número secuencial de 3 dígitos)
- Cada tarea se convierte en un spec en `docs/specs/<id>.md`
- Cada spec vive en su rama `spec/<id>`
- Estados: `backlog` · `next` · `in-progress` · `blocked` · `done`
- Si una tarea está `blocked`, agregar una nota debajo de la tabla indicando el motivo del bloqueo
- Solo un item puede estar en `next` a la vez
- Solo un item puede estar en `in-progress` a la vez

---

## Feature: Menu Refinements & Buttons

> 2026-06-15 — Refinimientos visuales del menú principal.

| #   | ID               | Tarea                                                     | Estado |
| --- | ---------------- | --------------------------------------------------------- | ------ |
| 16  | 016-menu-polish  | Posiciones título/copa, efectos, botones, click anim      | done   |

### Detalle 016

- **Título**: posición (6, -83), terremoto cada 4-7s (20px intensidad, 0.35s duración)
- **Copa**: posición (-9, -382), escala 14.5x
- **Faction sprite**: agregado como botón, luego quitado
- **botonPlay**: reemplazó el texto PLAY, sprite `botonPlay_0`, escala 4.74, posición (6, 16)
- **botonQuit**: sprite `botonQuit_0`, escala 2.38, posición (820, 6), llama `Application.Quit()`
- **Click effect**: animación escala 0.88 → 1.05 → 1.0 en todos los botones
- **Cursor personalizado** (manoMenu_0): intentado con `LoadAll`, luego quitado
- **Background lightning**: probado (bolts blancos/azules con doble parpadeo), luego quitado
- **DOTween**: intento de instalación vía git URL falló (no tiene UPM package oficial), revertido a corrutinas
- **Music fix**: `StopMusic()` ahora cancela la corrutina `QueueNextMenuTrack` para que no siga sonando música del menú en el gameplay

## Feature: Menu Overhaul

> 2026-06-15 — Rediseño del menú con fondo, estante y trofeos bloqueados.

| #   | ID               | Tarea                                                     | Estado |
| --- | ---------------- | --------------------------------------------------------- | ------ |
| 17  | 017-menu-overhaul | Fondo Menu.png, estante, trofeos, ajustes de posición     | done   |

### Detalle 017

- **Fondo**: `Menu.png` cargado como background fullscreen
- **Título/Copa/Quit**: eliminados
- **Estante**: sprite `estante`, posición (811, 187), size 280×160, scale (3, 4)
- **TrophyHeader**: texto "TROPHIES" negro, posición (814, 315)
- **Trophies**: 3 copas bloqueadas (tinte gris 0.5, 0.5, 0.5, 0.8) con label "LOCKED"
  - `copaHuman`: (812, 223) — 60×80, scale 1.7
  - `copaOrc`: (812, 94) — 60×80, scale 1.7
  - `copaBeast`: (818, -46) — 60×80, scale 1.7
- **PlayButton**: sprite `botonplay2_0`, posición (-13, -365), size 85×30, scale (5.5, 4.5)
- **Carousel**: slots movidos a y=-211, center en x=-8
- **Flechas**: reposicionadas a y=-211
- **Todos los elementos UI**: migrados a anchor (0.5, 0.5) con posiciones absolutas
- **Botón Quit**: eliminado

## Feature: Red AI

> 2026-06-15 — IA para el bando rojo (single-player).

| #   | ID               | Tarea                                  | Spec                                         | Estado |
| --- | ---------------- | -------------------------------------- | -------------------------------------------- | ------ |
| 18  | 018-red-ai       | IA para fichas rojas                   | [spec](docs/specs/018-red-ai.md)            | done   |

### Detalle 018
- **Implementación inicial**: AIController escanea fichas rojas, evalúa movimientos (kill +100 > attack +50+diff*10 > advance +20 > lateral +10 > retreat +5), ejecuta el mejor en BoardManager.MovePieceAI().
- **Fix aleatoriedad**: Cambiado de selección ordenada por Tier a recolectar todas las piezas movibles y elegir una al azar. ±5 de ruido en evaluación de movimientos.
- **Turno único**: IA mueve solo una ficha por turno (no todas), alternando azul→rojo→azul→rojo.
- **Bandera aiPlaying/aiInProgress**: InputManager y BoardManager bloquean input del jugador durante turno de IA.
- **Música win-lose**: SoundManager.PlayWinLoseMusic() carga `Sounds/Fondo/win-lose.mp3` y se llama desde GameOverUI.Show().
- **Test buttons reposicionados**: GANAR en (-840, -415), PERDER en (-673, -424).

## Feature: Build & Polish

> 2026-06-15 — Build de Windows y ajustes finales.

| #   | ID               | Tarea                                  | Estado |
| --- | ---------------- | -------------------------------------- | ------ |
| 19  | 019-build-exe    | Build Windows EXE                      | done   |

### Detalle 019
- BuildScript actualizado con `Build/Build Standalone Windows` y `Build/Build WebGL`.
- EXE generado en `Builds/DiceClashTactics_Win/DiceClashTactics.exe`.

## Feature: Tester Feedback & Polish

> 2026-06-17 — Ajustes basados en feedback del tester.

| #   | ID                 | Tarea                                  | Spec                                                     | Estado   |
| --- | ------------------ | -------------------------------------- | -------------------------------------------------------- | -------- |
| 24  | 024-tester-feedback | Feedback del tester y ajustes         | [spec](docs/specs/024-tester-feedback.md)               | done |

### Detalle 024

- **Muro/farolas/estatuas eliminados** — `BoardManager.cs` `CreateBoardDecorations()`: se quitaron Pared, Farol, Estatua. Solo quedan Gárgolas y Barril.
- **Ojo de pez corregido** — `MainMenuManager.cs`: cámara cambiada de perspective a orthographic.
- **Bordes negros corregidos** — `BoardManager.cs`: fondo cambiado a opaco (alpha 1). `GameManager.cs`: color de fondo más claro.
- **Banderas reposicionadas** — `BoardManager.cs`: BlueFlag a y=-0.8, RedFlag a y=-5.6 (niveles diferentes).
- **Personajes flotantes eliminados** — `MainMenuManager.cs`: se eliminó `SpawnFloatingPieces` y toda la lógica de Update.
- **CharacterCardUI simplificada** — Reemplazado texto "ATK: X | DEF: Y" por iconos de espada/escudo con valores numéricos. Cards más compactas.
- **Efectos hover** — `InputManager.cs`: UpdateHover implementado — piezas propias se iluminan en amarillo, enemigas en rojo al pasar el mouse.
- **Botón de sonido permanente** — `TurnUI.cs`: toggle de audio con icono verde/rojo en esquina superior derecha.
- **Botón SALIR** — `TurnUI.cs`: visible en PC (oculto en WebGL). Llama `Application.Quit()`.
- **Reset por teclas** — `TurnUI.cs`: Ctrl+R recarga la escena activa.
- **Fanfarria de victoria** — `SoundManager.cs`: `PlayVictory()` ahora reproduce acorde C mayor seguido de fanfarria ascendente.
- **Sonido de derrota mejorado** — `PlayDefeat()` añade tono descendente adicional.
- **Partículas de movimiento** — `BoardManager.cs`: `MoveTrail()` añade estela de color del equipo al moverse cualquier pieza.
- **Partículas GameOver en bucle** — `GameOverUI.cs`: corrutinas de confeti, chispas y puntos cambiadas a bucles infinitos con `while(true)`.
- **Tilemap offset corregido** — `BoardManager.cs`: tilemap offset cambiado de (0, -1, 0) a (0, 0, 0).
- **Hover refinado** — `InputManager.cs`: hover solo resalta piezas propias en amarillo; enemigas solo si son targets válidos con pieza seleccionada (rojo).
- **Movimiento más lento** — `BoardManager.cs`: `AnimateSlide` aumentado de 0.6s a 0.9s.
- **Background más grande** — `BoardManager.cs`: escala del background multiplicada por 1.15.
- **Pose de ataque** — `BoardManager.cs`: nuevos métodos públicos `SetAttackPose()` (cambia sprite a Attack) y `ResetToIdle()`. Se llaman desde `InputManager.cs` al seleccionar/deseleccionar.
- **Labels ATK/DEF en cartas** — `CharacterCardUI.cs`: texto pequeño "ATK"/"DEF" debajo de los valores numéricos.
- **Carpetas emoji por especie** — Creadas `Resources/Sprites/{Human,Orc,Beastfolk}/Emoji/`.
- **Copetín/humito** — `BoardManager.cs`: `SpawnConfetti()` (12 dots de colores) al ganar combate, `SpawnSmoke()` (6 dots grises) al perder.

## Feature: Tester Feedback 2 — UI & Polish

> 2026-06-17 — Segunda ronda de feedback: ajustes visuales y de UI.

| #   | ID                 | Tarea                                  | Spec                                                     | Estado   |
| --- | ------------------ | -------------------------------------- | -------------------------------------------------------- | -------- |
| 25  | 025-tester-feedback-2 | Feedback tester 2: UI y polish       | —                                                        | done |

### Detalle 025

- **Aura botón Play** — `MainMenuManager.cs`: glow pulsante azul detrás del botón, botón respira suavemente (1.5% oscilación).
- **Retry quieto / Next pulso** — `GameOverUI.cs`: en victoria, Next tiene pulso suave, Retry estático.
- **Humo derrota mejorado** — `GameOverUI.cs`: más partículas (5), más grandes (40-65), color más blanquecino (0.65,0.15,0.25), origen y=-320.
- **Tablero bajado** — `BoardManager.cs`: tilemap offset -0.3, cámara -0.3.
- **BattleResultUI ganador** — `BattleResultUI.cs`: texto blanco si gana azul, negro si gana rojo. `highlightedColor` rojo/azul según equipo.
- **Emojis por raza** — `BoardManager.cs`: `SpawnEmojis()` tras combate. Cada pieza guarda su `species` en `PieceData`. Azul usa `GameConfig.selectedSpecies`, rojo usa `scenarioTheme`. Sprites cargados desde `Sprites/{species}/Emoji/` con naming `emote{key}{emotion}_0`.
- **Pose de ataque** — sprites cargados por especie/clase via `Pose{name}`. Usa `speciesTheme`/`scenarioTheme` en vez de `data.species`. Orc/Beast 90% del tamaño Human. Paladín Orco escala fija 0.267.
- **Efectos dados** — `SoundManager.cs`: `PlayDiceRoll()` carga mp3 de dado, volumen 1f por 0.8s, arranca en 1s.
- **Sonido yes por raza** — `SoundManager.cs`: `PlaySelectSound(species)` carga yes.mp3 de `Sprites/{species}/Sound/`. Human arranca en 1s (salta silencio).
- **Confeti/humo en panel** — `BattleResultUI.cs`: confeti (30 partículas) al ganar, humo blanco (12 partículas) al perder, sobre el panel de dados.
- **Iconos sonido** — `TurnUI.cs`: carga `soundOn`/`soundOff` de `Sprites/Human/Decor/`.
- **Dice sound reemplazado** — `SoundManager.cs`: `PlayDiceRoll()` ahora reproduce `dice1.mp3` → `dice2.mp3` secuencial sin pausa.
- **Orc yes truncado** — `PlaySelectSound()`: Orco arranca en 0s y se corta a 1.25s vía `StopAfterDelay()`.
- **CharacterCardUI rediseñada** — Nueva carta blanca con: nombre + HP, imagen de criatura por especie (`{Type}carta{Species}`), texto de sabor, stats (ATK/DEF/Mov/Score) y tags (WEAK/RES/RET). Reemplaza las cartas oscuras anteriores.

## Feature: Content Expansion

> Tareas pendientes para expandir el juego más allá del mundo humano.

| #   | ID               | Tarea                                  | Spec | Estado   |
| --- | ---------------- | -------------------------------------- | ---- | -------- |
| 20  | 020-trophy-persist | Persistencia de trofeos desbloqueados | —    | done     |
| 21  | 021-orc-world    | Mundo Orco (data-driven, misma escena) | —    | done     |
| 22  | 022-beast-world  | Mundo Bestia (data-driven, misma escena) | —   | done     |
| 23  | 023-next-button  | Wire botón Next para avanzar entre mundos | — | done     |

## Feature: World Progression Fixes

> 2026-06-23 — Correcciones al flujo de progresión entre mundos.

| #   | ID               | Tarea                                  | Estado |
| --- | ---------------- | -------------------------------------- | ------ |
| 27  | 027-world-fixes  | Fixes de escenario, enemigos, Next, botones | done |

### Detalle 027

- **BoardManager.Awake()** — Nueva lectura de `GameConfig.selectedScenario`/`selectedSpecies` desde `PlayerPrefs` en `Awake()`, corre antes que `Start()` y evita que el escenario siempre sea "Human" por valores default de campos.
- **GameManager.cs** — Eliminado `UpdateBoardManager()` (ya no necesario).
- **LoadSprites** — Red team usa `scenarioTheme` como tema primario en vez de `speciesTheme`, cargando sprites correctos para enemigos Orco/Bestia.
- **Next button** — Avanza Human→Orc→Beastfolk→MainMenu según escenario actual, en vez de hardcodear "Orc".
- **Quit button** — Sprite carga desde `botonQuit_0.png` (archivo individual) porque `botonQuit .PNG` (con espacio en nombre) no se cargaba con `Resources.LoadAll`.
- **Beastfolk tree** — Escala de árbol cambiada de (1.1, 2.1) a (0.10, 0.14) para ajustarse al sprite de Bestias.

## Feature: Battle UI & Board Sorting Fixes

> 2026-06-23 — Correcciones de sprite loading, layout del combate y orden de renderizado.

| #   | ID               | Tarea                                  | Estado |
| --- | ---------------- | -------------------------------------- | ------ |
| 28  | 028-battle-ui-fixes | Fixes BattleResultUI, CharacterCardUI, BoardManager sorting | done |

### Detalle 028

- **Sprite loading** — Cambiado de búsqueda por nombre a `LoadFirstSprite` (índice 0) en BattleResultUI y CharacterCardUI. Solo `panelVictoria_1` usa búsqueda por nombre.
- **Piece icons duplicados** — atkSprite/defSprite ahora 200×200 en y=227. atkIcon/defIcon ya cargan sprite de carta (no más cuadrados blancos).
- **Result texts** — Eliminado `resultText`. Nuevos `blueResultText` en (-251, -199) con "ATK {n}" y `redResultText` en (237, -187) con "DEF {n}", ambos blancos.
- **Victoria panel** — Posición y=-343. Agregado `StopAllCoroutines()` + `victoriaObj.transform.localScale = Vector3.one` al inicio de `ShowResult()` para que reaparezca en peleas subsecuentes. Agregado `PulseVictoria()` (pulso ±5% cada 0.6s).
- **CharacterCardUI** — Cartas 340×370 (antes 300×330). OwnCard y=-79. Elementos OwnCard/EnemyCard reposicionados según coordenadas especificadas. Métodos `AdjustOwnCard()`/`AdjustEnemyCard()`.
- **BoardManager sorting** — Gárgolas, barriles y árboles cambiados a `sortingOrder = -1` (detrás de piezas en sortingOrder 0).

## Feature: Scoreboard UI Redesign

> 2026-06-23 — Rediseño del scoreboard final con paneles medievales, animación pieza por pieza, y transición a GameOverUI.

| #   | ID               | Tarea                                  | Estado |
| --- | ---------------- | -------------------------------------- | ------ |
| 29  | 029-scoreboard-redesign | ScoreboardUI con nuevos paneles y animaciones | done |

### Detalle 029

- **Panel sprites** — Usa nuevos sprites en `Resources/Sprites/Menu/Score/`: `FinBatallaPanel` (1492×250, banner superior), `FinConteoBluePanel` (657×256, puntaje azul), `FinConteoRedPanel` (640×256, puntaje rojo), `FinPiezasBluePanel` (1461×1433, detalle azul), `FinPiezasRedPanel` (1442×1432, detalle rojo), `FinVictoriaPanel` (2267×256, banner ganador). Todos cargados con `LoadFirstSprite`.
- **Layout** — Posiciones absolutas con anchor (0.5,0.5): FinBatallaPanel en (0, 400), FinConteoBlue en (-320, 190), FinConteoRed en (320, 190), FinPiezasBlue en (-370, -250), FinPiezasRed en (370, -250), FinVictoriaPanel en (0, -440).
- **Animación** — Revela piezas una por una con sonido de martillo (`PlayHammer()`) y efecto de vibración (`HitEffect`). Muestra total de puntos después de cada pieza.
- **Piece rows** — Cada fila tiene icono de carta 28×28, nombre y puntos. Ancladas al top-left del panel (0, 1).
- **Victory banner** — Pulso suave (2.5Hz, ±3%). Texto "VICTORIA AZUL!" en azul claro.
- **Defeat banner** — Texto "DERROTA!" en rojo. Partículas de humo (`smoke puff`) ascendentes que se expanden y desvanecen.
- **Transición** — Después de 2s de mostrar el banner, llama `GameOverUI.Instance.Show(winner)` y se oculta.
- **Flow change** — `BoardManager.CheckVictoryAndEndTurn()` y `AIController` ahora llaman `ScoreboardUI.Instance.Show()` en vez de `GameOverUI.Instance.Show()` directamente.

## Feature: Scoreboard Layout & Anchor Fixes

> 2026-06-23 — Corrección de pivots/anchors en las filas del scoreboard para que los textos queden dentro de los paneles.

| #   | ID               | Tarea                                  | Estado |
| --- | ---------------- | -------------------------------------- | ------ |
| 30  | 030-scoreboard-layout | Posiciones y anchors correctos en filas del scoreboard | done |

### Detalle 030

- **Icono eliminado** — Se quitó el sprite de carta de cada row porque ya está en el fondo del panel.
- **Anclajes corregidos** — NameLabel ahora usa anchor `(0, 0.5)` pivot `(0, 0.5)` con Pos X=10 (anclado a la izquierda del contenedor row). ScoreLabel usa anchor `(1, 0.5)` pivot `(1, 0.5)` con Pos X=-10 (anclado a la derecha del contenedor row).
- **Posiciones finales** — Calculadas desde referencias del usuario:
  - Azul: Peon (-347, -77), Ninja (-346, -139), Caballero (-346, -201), Paladin (-346, -259), Total (-346, -334)
  - Rojo: Peon (401, -85), Ninja (401, -147), Caballero (401, -204), Paladin (401, -270), Total (401, -339)
- **Título** "FIN DE LA BATALLA" → font size 33.
- **BlueTotal/RedTotal** → font size 26.
- **ScoreboardRow.cs** y **CreateScoreboardRowPrefab.cs** eliminados (no se usaron).


| #   | ID               | Tarea                                  | Spec                                          | Estado      |
| --- | ---------------- | -------------------------------------- | --------------------------------------------- | ----------- |
| 1   | 001-board-setup  | Definición técnica y matriz [7][7]     | [spec](docs/specs/001-board-setup.md)         | done        |
| 2   | 002-input-turns  | Input del jugador y máquina de turnos  | [spec](docs/specs/002-input-turns.md)         | done        |
| 3   | 003-piece-movement | Movimiento de fichas en el tablero   | [spec](docs/specs/003-piece-movement.md)      | done        |
| 4   | 004-combat         | Combate con dados                    | [spec](docs/specs/004-combat.md)             | done        |
| 5   | 005-game-over      | Condición de victoria y reinicio     | [spec](docs/specs/005-game-over.md)          | done        |
| 6   | 006-abilities      | Habilidades especiales por ficha     | [spec](docs/specs/006-abilities.md)          | done        |
| 7   | 007-sfx            | Efectos de sonido procedurales       | [spec](docs/specs/007-sfx.md)                | done        |
| 8   | 008-animations     | Animaciones visuales procedurales    | [spec](docs/specs/008-animations.md)         | done        |
| 9   | 009-sprites        | Sprites de personajes                | [spec](docs/specs/009-sprites.md)           | done        |
| 10  | 010-tilemap        | Migración a Tilemap + refactor Cell  | [spec](docs/specs/010-tilemap.md)           | done        |
| 11  | 011-bgm            | Música de fondo (gameplay loop + menú) | [spec](docs/specs/011-bgm.md)            | done        |
| 12  | 012-orcs-and-decor | Reorganización de sprites por tema     | [spec](docs/specs/012-orcs-and-decor.md) | done |
| 13  | 013-combat-balance-and-feedback | Balance de combate y feedback visual | [spec](docs/specs/013-combat-balance-and-feedback.md) | done |
| 14  | 014-beastfolk-obstacles        | Mapa BeastFolk y obstáculos         | [spec](docs/specs/014-beastfolk-obstacles.md)        | done |
| 15  | 015-main-menu                  | Pantalla de inicio                  | [spec](docs/specs/015-main-menu.md)                 | done |
| 33  | 033-tutorial                   | Tutorial para nuevos usuarios       | [spec](docs/specs/033-tutorial.md)                 | done |
| 34  | 034-game-hook                  | Game Hook — primeros 3 min          | [spec](docs/specs/034-game-hook.md)                | done |

## Feature: Testing Feedback & Fixes

> 2026-06-25 — Correcciones post-tester: sorting, paneles de dados, trofeos.

| #   | ID               | Tarea                                                     | Estado |
| --- | ---------------- | --------------------------------------------------------- | ------ |
| 32  | 032-tester-fixes | Sorting capas, paneles dados, trofeos, Ctrl+R full reset   | done   |

### Detalle 032

- **BattleResultUI**: Azul siempre izquierda, Rojo siempre derecha en paneles de dados (swap stats/dados/habilidades según quién ataque).
- **Gargoyle sorting**: `sortingOrder` cambiado de 1 a -1 (estaban sobre piezas).
- **Piece sorting por fila**: `CreatePieceVisual` recibe `row`, asigna `sortingOrder = row` para que piezas delanteras se rendericen encima.
- **Caballero idle scale**: Reducido de 1.3 a 1.15.
- **Ctrl+R full reset**: Ahora llama `PlayerPrefs.DeleteAll()` antes de recargar escena.
- **Trofeos separados**: `IsBeaten` (copa dorada/gris) vs `IsUnlocked` (disponible para jugar). `UnlockNextTrophy` usa claves separadas (`Trophy_X` para ganado, `Unlocked_X` para desbloqueado).

## Feature: Null Safety & PowerUp Visibility Fixes

> 2026-06-29 — Correcciones de null reference en AnimatedMove y visibilidad de power-ups.

| #   | ID               | Tarea                                                     | Estado |
| --- | ---------------- | --------------------------------------------------------- | ------ |
| 35  | 035-null-fixes   | Null safety en AnimatedMove + PowerUp sorting order       | done   |

### Detalle 035

- **BoardManager AnimatedMove**: Added null checks after every `yield` point (`movingVisual`, `defenderVisual`) to prevent `MissingReferenceException` at line 1126 (`SetParent`). Fixed `defenderVisual.transform.position` being read after `AnimateDestroy`. Added null guard to `AnimateDestroy` loop.
- **PowerUpManager sorting**: Icon sortingOrder 1→14, glow 0→13 (above all pieces 0-13). Icon scale 0.22→0.50, glow 0.35→0.60. Pulse animation updated for new scale.
- **PowerUp procedural icons**: Replaced 32x32 1px drawings with 64x64 3px thick lines. Added `DrawThickLine`/`DrawDot` helpers. Applied in both PowerUpManager and TutorialManager.
- **Scoreboard en tutorial**: Removido `if (GameConfig.isTutorial) return;` de ScoreboardUI.Show(). Agregado `BoardManager.suppressVictoryCheck`. TutorialManager lo activa al inicio, desactiva al empezar ShadowPhase. `CheckShadowEnemyKilled` ya no inicia `VictoryTransition` — el flujo ScoreboardUI→GameOverUI maneja la victoria.
- **GameOverUI tutorial**: Victoria en tutorial muestra solo botón "Next" que llama `GameConfig.Play()`, sin cup/trophy unlock.
- **Input bloqueado durante power-up**: `PowerUpManager.IsExecuting` flag. Se activa en `Execute()`, se desactiva tras 2s. InputManager salta input mientras esté activo.

## Feature: Tutorial & Game Hook

> 2026-06-26 — Tutorial para nuevos usuarios y game hook de primeros 3 minutos.

| #   | ID               | Tarea                                  | Spec                                                     | Estado      |
| --- | ---------------- | -------------------------------------- | -------------------------------------------------------- | ----------- |
| 33  | 033-tutorial     | Tutorial para nuevos usuarios          | [spec](docs/specs/033-tutorial.md)                       | done |
| 34  | 034-game-hook    | Game Hook — primeros 3 min post-tutorial | [spec](docs/specs/034-game-hook.md)                    | done |
| 36  | 036-powerup-sound | Sonidos, efectos y fixes post-tester  | —                                                        | done |

### Detalle 033

- **Tutorial**: Maniquíes de madera como enemigos sin IA, secuencia scriptada paso a paso (selección → movimiento → combate → fin de turno)
- **Power-ups**: 4 tipos con iconos visibles (64x64, 3px grosor, sortingOrder 14/13), introducidos durante el tutorial
- **Instructivo**: Texto en parte superior con instrucciones claras, overlay de resalte en elementos UI relevantes
- **Saltar**: Botón "SALTAR" para jugadores experimentados
- **Shadow phase**: Nuevos enemigos sombra con IA, power-ups respawnean con iconos visibles, ScoreboardUI→GameOverUI al vencer
- **Input bloqueado**: No se puede mover ni actuar durante efectos de power-up (IsExecuting flag, 2s)
- **Victory flow**: ScoreboardUI muestra puntajes, GameOverUI en modo tutorial tiene botón "Next" → MainMenu
- **Transición**: Al completar → MainMenu (GameConfig.Play("Human"))

### Detalle 034

- **Duración**: 3-5 minutos, equipos de 4 piezas por bando
- **Enemigos especiales**: Esbirros (1 HP, fáciles de eliminar), Campeón Enemigo (mini-boss con más HP)
- **Power-ups**: Mismos 4 tipos del tutorial, aparecen como paquetes en casillas del tablero
- **Eventos scriptados**: turno 1 power-up cerca del jugador, turno 2-3 esbirro débil, turno 4-5 IA mal posicionada, turno 6+ campeón + power-up fuerte
- **IA fácil**: 50% probabilidad de no atacar, movimientos subóptimos
- **Feedback**: Celebración en primer combate ganado, scoreboard muestra próximo trofeo
- El hook es la shadow phase del tutorial (no un modo separado). TutorialManager maneja ambas fases: maniquíes → sombras con power-ups → ScoreboardUI → GameOverUI.

## Feature: Sound, Effects & Post-Tester Fixes

> 2026-07-09 — Power-up sounds, coin effects, temblor, Paladin scale, cup dialog, power-up spawn rates.

| #   | ID               | Tarea                                                     | Estado |
| --- | ---------------- | --------------------------------------------------------- | ------ |
| 36  | 036-sound-fx     | Sonidos power-up, monedas, botón, temblor                 | done   |

### Detalle 036

- **Power-up sounds**: Fireball/rayo suenan al lanzar (mage attack sprite), fire-rayo al impactar (ambos tipos). Volumen 0.4, pitch 0.75 para alargar.
- **Coin effect**: Texto "+10" fontSize 28 con bounce scale, partículas 10-20px (antes 6-10), arco más alto (80-150), 10 partículas, sparkle brillo, screen shake al llegar a copa, sonido `PlayCoin()` (1800Hz+2400Hz).
- **Temblor**: `PlayFireRayo()` + `PlayTemblor()` al inicio, 30 chispas doradas con gravedad, CameraShake 0.2f/0.45s (antes 0.15f/0.3s), piezas se deslizan con PushSlide en vez de teletransporte.
- **PaladinAttackFront scale**: Hardcodeado (0.50, 0.55) para defensor Paladín, eliminada condición `sprite != defAtk[0]` para que siempre aplique.
- **PaladinAttackBack**: Eliminado dynamic ratio del atacante — ya no se achica.
- **Cup position drift**: Reset `anchoredPosition` a (0,45) al inicio de `AnimateCupShake()`.
- **White square fix**: Dice images ocultas durante animación del vaso, se activan en `AnimateDiceClash()`.
- **Cup dialog**: Ahora en todos los mapas, no solo Human. Si copa no llena → solo Retry centrado grande. Si llena → Next + Retry.
- **Power-up spawn rates**: Basado en `TimerManager.timeRemaining`: >180s → 10-18s delay, 60-180s → 8-15s, <60s → 6-12s. Siguen apareciendo hasta el final.
- **Button sound**: `PlayButton()` (660Hz+880Hz) con 0.33s delay en Retry del diálogo.
- **EXE build**: Compilación exitosa a `Build/DiceClashTactics.exe`.

## Feature: Post-Release Polish

> 2026-07-13 — Efectos visuales, sonidos y fixes de balance.

| #   | ID               | Tarea                                                     | Estado  |
| --- | ---------------- | --------------------------------------------------------- | ------- |
| 37  | 037-explosion-burn | Power-up explosion deja tablero quemado unos segundos    | done    |
| 38  | 038-coin-improve   | Mejorar efectos de monedas                              | done    |
| 39  | 039-trumpet-win    | Sonido de trompetas al ganar combate con dados           | done    |
| 40  | 040-dice-reset     | Resetear valor de dados a cero en tablero al chocar      | done    |
| 41  | 041-combo-fix      | Fix combo x2 mostrándose con un solo enemigo             | done    |
| 42  | 042-trumpet-fix    | Fix trompeta no sonaba al ganar tirada de dados          | done    |
| 43  | 043-paladin-scale  | Fix Paladín agrandándose al ser atacado horizontalmente   | done    |
| 44  | 044-cup-victories  | Copa se llena con 2 victorias (no con 1100 puntos)       | done    |

### Detalle 037
- Power-up de explosión: al impactar, las casillas afectadas muestran textura de quemado/tierra oscura durante ~3 segundos antes de volver a la normal.

### Detalle 038
- Efectos de monedas: mejorar partículas, animación de bounce, screen shake, sonido al recolectar.

### Detalle 039
- Sonido de trompetas/fanfarria cuando el jugador gana un combate de dados.

### Detalle 040
- Cuando chocan los dados en el tablero, el valor mostrado en las casillas debe volver a cero/campo.

### Detalle 041
- **Combo x2** se mostraba al luchar contra un solo enemigo. Causa: `blueKillCombo` se incrementaba al morir cualquier ficha Roja, incluso durante el turno de la IA. Fix: solo incrementar cuando `turnManager.GetCurrentTeam() == Team.Blue`. (`BoardManager.cs:1900`)

### Detalle 042
- **Trompeta no sonaba** al ganar tirada de dados. Causa: `PlayTrumpetSequence` tenía 0.5s de delay inicial y usaba `uiSource` (AudioSource separado). Fix: eliminado delay, cambiado a `source` (AudioSource principal), subido volumen a 0.8/0.9. (`SoundManager.cs:222-234`)

### Detalle 043
- **Paladín se agrandaba** horizontalmente al ser atacado. Causa: `InputManager` no deseleccionaba la pieza al cambiar de turno. Si el jugador tenía una pieza seleccionada (ataque pose, escala 0.65) y expiraba el temporizador, la pieza quedaba en pose de ataque. Al ser atacada por la IA, `ResetPieceSprite` restauraba a escala idle (1.0), pareciendo que crecía. Fix: `InputManager` se suscribe a `OnTurnChanged` y llama `DeselectCurrent()`. (`InputManager.cs:33-48`)

### Detalle 044
- **Copa llenaba por puntos** (1100 puntos = 1 victoria). Fix: sistema basado en victorias. `RecordMatchWin()` se llama desde `ScoreboardUI` cuando Blue gana. Copa se llena con `cupVictories / VICTORIES_NEEDED` (2 victorias). Texto muestra "W 0/2". Eliminado `CUP_MAX = 1100`. (`CoinManager.cs`, `ScoreboardUI.cs:296-297`)

## Feature: Campaña, Economía y Progresión

> 2026-07-14 — Campaña 22 niveles, 4 copas, economía de oro, cofres, insignias, powerup MAGIC!, obstáculos nuevos.

| #   | ID               | Tarea                                  | Spec                                          | Estado   |
| --- | ---------------- | -------------------------------------- | --------------------------------------------- | -------- |
| 45  | 045-campaign-progression | Campaña completa, economía, insignias | [spec](docs/specs/045-campaign-progression.md) | in-progress |

### Detalle 045 — Fase 1: Data Models
- **CampaignData.json** — 22 niveles en 4 copas (Iron Crown/Human, Blood Fang/Orc, Wild Heart/Beastfolk, Void Seal/Nigromantes). Niveles 1-2 intro, 3-22 campañas. Cada nivel define: enemyRace, powerups[], obstacles[], enemyCount, goldReward, insigniaId.
- **InsigniaData.json** — 35 insignias (22 campaña + 13 cofres). 4 raridades: common, rare, epic, legendary.
- **EconomyData.json** — 100 oro inicial, daily bonus (25-75 oro por streak 1-7), costos powerups (Shake 15, Explosion 20, Fireball 25, Lightning 30, MAGIC 40), entrada por copa (0-30), cofres 8hs/2 slots.
- **CampaignData.cs** — Load(), GetLevel(), GetCup(), GetTotalLevels(), GetTotalCups().
- **InsigniaData.cs** — Load(), GetInsignia(), GetBySource(), GetByRarity(), GetTotalCount().
- **EconomyConfig.cs** — Load(), GetPowerupCost(), GetLevelEntryCost(), GetDailyBonusAmount(). Usa `StringIntPair[]` (no Dictionary) para compatibilidad con JsonUtility.

### Detalle 045 — Fase 2: Economía
- **EconomyManager.cs** — Singleton (Instance pattern como CoinManager). Gold persistido en PlayerPrefs ("TotalGold"). HUD: bolsa sprite `Sprites/Menu/bolsa` (48x48) + texto dorado esquina superior derecha. Efectos: pulso idle (±4% escala), pulso al ganar oro (+25%), shake al gastar, mini monedas que saltan con arco al ganar.
- **DailyBonusUI.cs** — Popup modal overlay: título "DAILY BONUS", moneda grande, monto + oro, streak, botón OK. Se muestra al iniciar partida si no se reclamó hoy.
- **GameManager.cs** — Agregado `EconomyManager`, `DailyBonusUI`, corrutina `ShowDailyBonusDelayed()`.
- **PowerUpManager.cs** — `Execute()` verifica gold antes de ejecutar (Blue team only). Si no tiene suficiente, return sin ejecutar.
- Fix: `DailyBonusUI` — eliminado `btn.targetImage` (no existe en esta versión de Unity).

### Detalle 045 — Fase 3: MAGIC! Powerup
- **PowerUpType.MAGIC** — Nuevo tipo, color púrpura (0.6, 0.2, 1.0).
- **MagicEffect()** — Corrutina: busca piezas enemigas que no sean Peón, elige una al azar, efecto visual púrpura (chispas + flash + encogimiento), convierte a Peón via `BoardManager.ConvertPieceType()`, revelación con chispas blancas.
- **DrawMagicIcon()** — Estrella de 5 puntas procedural (64x64).
- **BoardManager.ConvertPieceType()** — Nuevo método público: cambia `pieceData.type` y resetea sprite.
- MAGIC! registrado en spawn system, getColor, getName, CreatePowerUpIcon.

### Detalle 045 — Fixes
- **Beastfolk Paladin escala** — Reducido de 1.3f a 1.1f en `GetIdleScale()`, `CreatePieceVisual()` y pose. Evita que Paladines Beastfolk se vean gigantes en combate.
- **Card sprites** — `CharacterCardUI` ahora usa `GetSpriteName()` (español: Peon, Caballero) para cargar sprites y `GetTypeName()` (inglés: Pawn, Knight) para mostrar texto.
- **PowerUpManager tutorial** — Guard cambiado de `GameConfig.isTutorial` a `GameConfig.isTutorial && !board.isShadowPhase`. Permite auto-spawn y recogida de powerups durante fase de sombras.
- **TutorialManager shadowPhase** — Spawnea 3 powerups (no 1) con delays. Instrucción "Move onto glowing squares for power-ups!". Eliminadas llamadas manuales redundantes a `CheckCollectionForTeam`.
- **EconomyManager HUD** — Eliminado panel Image de fondo (causaba overflow). Ahora es solo icono bolsa + texto flotando.
- **EconomyManager Random** — Agregado `using Random = UnityEngine.Random;` para resolver ambigüedad con System.Random.

## Feature: Rediseño de Recompensas, Modos y Exhibidor

> 2026-07-23 — SPEC-046: 4 modos de juego, exhibidor de trofeos, listones, insignias de ranked, goblin onboarding, cofre con progresión inteligente.

| #   | ID               | Tarea                                  | Spec                                          | Estado   |
| --- | ---------------- | -------------------------------------- | --------------------------------------------- | -------- |
| 46  | 046-reward-redesign | Rediseño recompensas, modos, exhibidor | [spec](docs/specs/046-reward-redesign.md) | done |

### Detalle 046

- **CampaignData.json actualizado** — 22 niveles con power-ups/obstáculos específicos del doc del diseñador. Nombres: Shake, Explosion, Fireball, Lightning, MAGIC. Obstáculos: Roca, DestroyedCell, Glue, Mine. Enemigos varían por nivel (Human, Orc, Wolf, NewRace).
- **GameConfig modos de juego** — Enums `GameMode` (Campaign/Ranked) y `PowerupMode` (WithPowerups/WithoutPowerups). Métodos `PlayCampaign(levelId, powerupMode)` y `PlayRanked(powerupMode)`. Flag `isRanked` en PlayerPrefs.
- **Power-ups siempre spawn** — `PowerUpManager.SpawnOnBoard()` respeta power-ups del nivel en campaña. En modo `WithoutPowerups`, el jugador no puede recolectar power-ups pero los enemigos sí.
- **RibbonManager** — Sistema de listones por victoria de campaña. Colores por mundo (Human=azul, Orc=verde, Beastfolk=marrón, Nigromantes=violeta). 5 tonalidades de claro a oscuro por copa. Integrado en `CampaignManager.CompleteLevel()`.
- **ExhibidorUI** — Pantalla completa reemplaza estante del menú. Muestra 4 copas (dorado si completada), grid de listones e insignias, contadores. Botón TROPHIES en menú principal.
- **ChestManager con oro + progresión** — `OpenChest()` retorna `ChestReward` con oro (50 base) + insignias + flags de duplicada. Algoritmo progresivo: 80% nueva si <50% coleccionado, 50% si 50-80%, 20% si >80%. `ChestUI` muestra secuencia: oro primero, luego insignias con flash blanco, sello "DUPLICADA!" en rojo.
- **EnemyBanner** — Banner con nombre del ejército enemigo al inicio de cada nivel de campaña. Animación fade in → stay → fade out. Color por raza.
- **RankedManager** — Modo libre con enemigos aleatorios. Power-ups por raza (Human=Shake+Explosion, Orc=Fireball+Lightning, Wolf=Lightning+MAGIC, NewRace=Fireball+MAGIC). 1 obstáculo aleatorio. Desbloqueado después del tutorial.
- **ModeSelectionUI** — Menú con 4 opciones (Campaign With/Without, Ranked With/Without). Costos: Campaign=20g, Ranked With=30g, Ranked Without=15g. Verificación de oro suficiente.
- **GoblinDialogue** — Onboarding post-tutorial. Explica power-ups gratuitos. Una vez mostrado, no se repite. Persiste en PlayerPrefs.
- **Desbloqueo de personaje por copa** — `CampaignManager.CheckCupCompletion()` otorga `Unlocked_{race}` al completar todos los niveles de una copa.

## Feature: Asset Integration & Fixes

> 2026-07-30 — Integración de sprites de cofre/insignias, efectos visuales y fixes post-build.

| #   | ID               | Tarea                                                     | Estado |
| --- | ---------------- | --------------------------------------------------------- | ------ |
| 47  | 047-asset-fixes  | Sprites reales, efectos, fixes de compilación             | done   |

### Detalle 047

- **ObstacleManager fix**: Eliminado duplicado de `CreateSquareSprite` (línea 744) que causaba error CS0111.
- **ChestUI sprites**: Reemplazado sprite procedural por `cofre0`–`cofre4` desde `Resources/Sprites/Cofre/`. Slot bloqueado muestra `cofre0` (cerrado), listo muestra `cofre4` (abierto). Animación de apertura secuencial al abrir cofre.
- **ChestUI efectos**: 12 chispas doradas al llegar a `cofre4`, 15 partículas doradas flotantes, título "CHEST OPENED!" con bounce-in, texto de oro rebota + 8 monedas cayendo con onda senoidal, power-ups con icono coloreado + 4 chispas por tipo, insignias con scale-in + 6 chispas de rareza, botón OK con fade-in.
- **ChestUI recompensas mixtas**: 1–3 power-ups gratis aparecen en el popup de recompensa además del oro e insignias.
- **InsigniaUI sprites**: Ahora carga `insignia1`–`insignia22` reales desde `Resources/Sprites/Insignias/`. Fallback procedural para chest insignias sin sprite.
- **GoblinDialogue hook**: Integrado en GameOverUI — al hacer clic en Next tras el tutorial, si no se ha mostrado antes, aparece el diálogo del goblin explicando power-ups y oro. Una vez visto, no se repite.
- **Economy canvas rediseñado**: Nuevo panel semitransparente con borde dorado en top-right (`(-20, -20)`). Bolsa 32×32 + texto gold en el panel. Reemplaza el HUD anterior.
- **Shadow phase power-ups fix**: `PowerUpManager.Execute()` ahora permite a Blue usar power-ups durante la shadow phase incluso en modo `WithoutPowerups`.
- **ExhibidorUI fix**: `InsigniaBadge` → `Insignia` (tipo correcto), `GetGold()` → `TotalGold` (propiedad correcta de EconomyManager).
- **ModeSelectionUI fix**: `GetGold()` → `TotalGold` en ambas referencias.

## Feature: UI & Reward Fixes

> 2026-08-04 — Fixes de UI/recompensas: popup bonus, monedas al bolso, escala Paladín, tinte de sombras en conversión MAGIC.

| #   | ID               | Tarea                                                     | Spec | Estado |
| --- | ---------------- | --------------------------------------------------------- | ---- | ------ |
| 48  | 048-ui-reward-fixes | Fixes popup diario, monedas al bolso, Paladín, tinte MAGIC | —  | done   |

### Detalle 048

- **DailyBonusUI fix** — Crea su propio Canvas (sortingOrder 220) con `GraphicRaycaster` en vez de `FindFirstObjectByType<Canvas>`. Arregla el popup en la esquina inferior-izquierda (canvas ajeno con ancla/raíz distinta) y el OK no cliqueable (canvas sin raycaster). `Close()` destruye popup + canvas propio; guard `if (popupObj != null) return;` evita duplicados.
- **Coins fly to bag** — `EconomyManager.AddGold` → `AddGoldFX` → `SpawnCoinsToBag` anima monedas desde arriba hacia el bolso (conversión `WorldToScreenPoint` → `ScreenPointToLocalPointInRectangle` para ubicar el target real del bolso) y al terminar `BagShake()` vibra el bolso. Reemplaza el viejo `SpawnCoinParticles` que dispersaba monedas lejos del bolso. Aplica a bonus diario, campaña, cofres y tutorial (+150).
- **PaladinAttackFront scale** — Hardcodeado `(0.46, 0.52)` para atacante y defensor (antes `(0.40, 0.46)`), condición `sprite != defAtk[0]` eliminada. (`BoardManager.cs:1238,1248`)
- **Shadow tint on MAGIC conversion** — `BoardManager.GetPieceColor(team)` (sombra Red=0.2/0.2/0.25, Red=0.7/0.7/0.7, Blue=white) usado por `CreatePieceVisual`, `ResetPieceSprite` y el fade-back de `PowerUpManager` (antes `Color.white` → enemigo convertido quedaba blanco en fase de sombras). (`BoardManager.cs:671`, `PowerUpManager.cs:1144`)
- **Goblin text verificado** — Coordenadas del diálogo del goblin ya correctas: `offsetMin (-79,-84)`, `offsetMax (79,84)` anclado al centro del panel.
- **Recompensa campaña visible** — El oro de `CampaignManager.CompleteLevel()` sí se otorgaba; ahora la animación de monedas al bolso + vibración lo hace visible al jugador.


## Feature: Campaign Menu Fix

> 2026-08-05 — Fix menú campaign atascado: BACK siempre disponible, cierre completo con limpieza de referencia.

| #   | ID               | Tarea                                                     | Spec | Estado |
| --- | ---------------- | --------------------------------------------------------- | ---- | ------ |
| 49  | 049-campaign-menu-fix | Fix menú campaign (trabado, sin salida)               | —  | done   |

### Detalle 049

- **Back button garantizado** — `CreateBackButton()` se crea al final (encima del scroll y título para seguir siendo cliqueable) y fuera del try/catch: aunque fallara la construcción del contenido, el panel siempre tiene botón para salir.
- **Contenido aislado** — `CreateScrollContent()` y `CreateTitle()` envueltos en try/catch con `Debug.LogError`. Un fallo de datos (JSON null, cup sin levels, etc.) ya no impide el cierre del panel.
- **Cierre completo** — Nuevo método `Close()` (destruye panel + invoca `OnClose` + destruye el componente). El BACK ahora llama a `Close()` en vez de `Hide()`.
- **Referencia saneada** — `MainMenuManager` suscribe `OnClose` para anular `campaignUI` al cerrar con BACK; el toggle del botón CAMPAIGN queda consistente (antes la referencia quedaba colgando y rompía el abrir/cerrar).
- **Null-guard sprites** — `backSprites` puede ser null si falta la carpeta de Resources; ahora se protege el `Array.Find`.

## Feature: Estantes Exhibidor + Botón Collect All/Reset

> 2026-08-05 — Estantes para insignias/listones en el Exhibidor y botón toggle para otorgar/borrar toda la colección (incluye cofres).

| #   | ID               | Tarea                                                     | Spec | Estado |
| --- | ---------------- | --------------------------------------------------------- | ---- | ------ |
| 50  | 050-exhibidor-shelves-collectall | Estantes en Exhibidor + botón COLLECT ALL/RESET ALL | —  | done   |

### Detalle 050

- **Estantes en ExhibidorUI** — `CreateShelf(parent, pos, width)` dibuja una barra de estante horizontal bajo cada fila de insignias y listones (insignias/listones se renderizan encima). Sprite `Estantes_0` cargado desde `Sprites/Estantes/Estantes` (antes usaba `Sprites/Menu/estante`, que era incorrecto). Borde superior claro para efecto de madera. `GetShelfSprite()` cachea el sprite con fallback a color plano.
- **Botón COLLECT ALL / RESET ALL** — `MainMenuManager.CreateCollectAllButton()` en `(-280, -510)` junto a CAMPAIGN/CHESTS/BADGES. Es un toggle:
  - Si no está todo coleccionado → otorga todas las copas (22 niveles `Campaign_Level_*`), todos los listones (`Ribbon_Level_*`), las 35 insignias (`Insignia_*`) y desbloquea mundos (`Unlocked_*`/`Trophy_*`). El label cambia a "RESET ALL".
  - Si ya está todo → borra niveles, listones, insignias (`InsigniaManager.ResetAll()`), cofres (`ChestManager.ResetAll()`) y claves de desbloqueo. El label vuelve a "COLLECT ALL".
- **Helpers**: `IsEverythingCollected()` (todas las insignias + todos los listones), `GrantAllCollections()`, `ResetAllCollections()`.

## Feature: Sprites Insignias + Popup Detalle

> 2026-08-05 — Insignias de cofre visibles, sprites reales en el Exhibidor y popup de detalle al hacer clic (click fuera lo cierra).

| #   | ID               | Tarea                                                     | Spec | Estado |
| --- | ---------------- | --------------------------------------------------------- | ---- | ------ |
| 51  | 051-insignia-sprites-detail | Sprites insignias (incl. cofre) + popup detalle al clic | —  | done   |

### Detalle 051

- **`InsigniaSprites.cs`** (nuevo) — Helper compartido `Get(Insignia)`: carga `Sprites/Insignias/insignia{N}` para `camp_{N}`, y para el resto (cofre, camp_01/02 sin sprite) genera icono procedural de estrella coloreado por rareza (64px). `GetRarityColor()` centralizado.
- **`InsigniaDetailPopup.cs`** (nuevo) — Overlay fullscreen (fondo oscuro 0.65) con botón que cierra al hacer clic en cualquier lado. Panel central 360×440 con borde de rareza, sprite grande 180×180, nombre, descripción y rareza. Animación pop-in (escala 0→1, easing cúbico).
- **`InsigniaUI.cs`** — Las cartas ahora son clicables (`Button`): abren `InsigniaDetailPopup`. `LoadInsigniaSprite` delega en `InsigniaSprites.Get`; eliminado `CreateInsigniaIcon` duplicado.
- **`ExhibidorUI.cs`** — Las insignias del grid ahora muestran el sprite real (`InsigniaSprites.Get`, antes eran cuadrados de color sin sprite — por eso no se veían los creados ni las de cofre). Las coleccionadas son clicables → popup de detalle.

## Fix: Exhibidor/Cofre (052)

> 2026-08-05 � Sprites reales de copa en el exhibidor, Trofeos reabrible tras Badges, cofre probable desde el menu.

| #   | ID               | Tarea                                                     | Spec | Estado |
| --- | ---------------- | --------------------------------------------------------- | ---- | ------ |
| 52  | 052-exhibidor-cofre-fixes | Fix copas planas, reapertura Trofeos, TEST CHEST en menu | -  | done   |

### Detalle 052

- **ExhibidorUI.cs** � Las copas ahora usan sprites reales (Sprites/Menu/copaHuman|Orc|Beast|Menu, sub-sprite _0 via GetCupSprite) en vez del rectangulo plano dorado que parecia un boton. Tint blanco si completada, gris si no. Nombre debajo de la copa.
- **ExhibidorUI.cs** � Nuevo Close() + OnClose para que MainMenuManager anule la referencia al cerrar.
- **MainMenuManager.cs** � Boton TROPHIES ahora es toggle (abrir/cerrar) y registra OnClose que anula exhibidorUI � corrige que no se pudiera reabrir despues de Badges.
- **ChestManager.cs** � AddReadyChestForTest(): agrega cofre ya listo (timestamp en el pasado) para probar la apertura sin esperar 8h.
- **ChestUI.cs** � GetChestSprite matchea prefijo cofre{idx}_ (los PNG se importan como cofre0_0). Boton TEST CHEST (abajo-izquierda) que agrega cofre listo y refresca los slots.

## Fix: Cofre grande y deseable (053)

> 2026-08-05 � NRE en ShowRewardSequence resuelto; cofre mas grande, crece al hover, aura dorada, recompensas con iconos de insignia reales.

| #   | ID               | Tarea                                                     | Spec | Estado |
| --- | ---------------- | --------------------------------------------------------- | ---- | ------ |
| 53  | 053-cofre-grande | Fix NRE recompensa + cofre deseable (hover, aura, iconos) | -  | done   |

### Detalle 053

- **Fix NRE**: todos los GameObject del popup ahora se crean con 	ypeof(RectTransform) y el flash de recompensa es un hijo dedicado (antes un segundo Image sobre el mismo objeto). Null-guards en AnimateSparkBurst/AnimateFloatingParticle y tras yields.
- **Cofre mas grande**: slots 400x430, cofre listo 170x170 (antes 120), bloqueado 140x140 (antes 100).
- **Deseable**: aura dorada pulsante tras el cofre listo, el cofre crece 1.25x al pasar el raton (EventTrigger), animacion de apertura 170px (antes 80).
- **Recompensas atractivas**: filas de insignia 520x68 con el sprite real de la insignia (52px) + nombre + rareza; oro mas grande (fontSize 26).

## Fix: Tutorial y Pipeline de Victoria (054)

> 2026-08-10 — Botón OK fuera de pantalla, maniquíes con sprite real en combate, daily bonus al menú, botón WIN con recompensas reales.

| #   | ID                 | Tarea                                                     | Spec | Estado      |
| --- | ------------------ | --------------------------------------------------------- | ---- | ----------- |
| 54  | 054-tutorial-fixes | Fixes tutorial + daily bonus al menú + WIN con recompensas | [spec](docs/specs/054-tutorial-fixes.md) | in-progress |

### Detalle 054

- **TutorialRewardUI** — botón "OK!" re-anclado a `(0.5, 0.5)` con `anchoredPosition (0, -290)` (antes ancla inferior `(0.5, 0)` en `(0, -295)`, quedaba bajo pantalla y colgaba el flujo).
- **BoardManager** — helpers `IsTutorialDummy(team)`, `GetTutorialDummySprite()`, `GetTutorialDummyScale()`; guards en `ResetPieceSprite` y `AnimatedMove` para que los maniquíes conserven sprite `muneco1` y escala 0.19 durante el combate.
- **Daily bonus al menú** — nuevo `TutorialProgress` (PlayerPrefs `TutorialPlayed`, marcado al completar o saltear el tutorial); eliminado `DailyBonusUI` + `ShowDailyBonusDelayed` de `GameManager`; hook en `MainMenuManager.Start()` con gate `TutorialProgress.HasPlayed()`.
- **ScoreboardUI ganador forzado** — `Show(Team? forcedWinner = null)`; `TestButtons` WIN → `ScoreboardUI.Instance.Show(Team.Blue)`, LOSE → `ScoreboardUI.Instance.Show(Team.Red)`. WIN ahora completa campaña (oro + listón + insignia) y registra victoria de copa.

## Fix: EnemyBanner NRE + Flujo post-tutorial (055)

> 2026-08-11 — NRE en EnemyBanner, volumen 2do tema gameplay, cofre→goblin→modos tras ganar tutorial.

| #   | ID                 | Tarea                                                     | Spec | Estado |
| --- | ------------------ | --------------------------------------------------------- | ---- | ------ |
| 55  | 055-enemybanner-posttutorial | Fix EnemyBanner + flujo cofre/goblin/modos + volumen | —  | done   |

### Detalle 055

- **EnemyBanner NRE** — `EnemyBanner.cs:40` — segundo `AddComponent<Image>()` sobre el mismo GameObject devuelve `null` en Unity nuevo → corrutina rota en `border.color` → banner oscuro pegado en medio de la pantalla sin texto ni fade. Fix: reemplazado el segundo Image por `Outline` (borde con color de la raza, `effectDistance (4, -4)`).
- **Volumen 2do tema** — `SoundManager.PlayGameplayMusic()` — `deuslower-fantasy-medieval-ambient-237371` ahora suena a volumen 0.5 (el 1er track queda en 0.3).
- **Flujo post-tutorial** — `GameOverUI` — al ganar el tutorial con sombras: cofre (TutorialRewardUI +150 oro) → goblin → **ModeSelectionUI** (Campaign/Ranked With/Without) en vez de entrar directo al juego con `GameConfig.Play()`. Nuevo `ShowModeSelection()` que crea ModeSelectionUI sobre el canvas de GameOverUI; el +150 del cofre alcanza para pagar la entrada (20/30/10/15g). SKIP del tutorial sin cambios.

## ExhibidorUI: vistas Ribbons + back unificado (056)

> 2026-08-12 — Rediseño del ExhibidorUI (badges con paginación, vista dedicada de listones, botones BACK con sprite `panel total back`).

| #   | ID                 | Tarea                                                     | Spec | Estado |
| --- | ------------------ | --------------------------------------------------------- | ---- | ------ |
| 56  | 056-exhibidor-ribbons-back | ExhibidorUI badges paginados + vista Ribbons + back unificado | —  | done   |

### Detalle 056

- **Fix compilación** — `ExhibidorUI.cs:163` llamaba a `CreatePaginationControls()` sin implementar (código cortado por límite de uso). Implementado: botones PREV/NEXT + indicador `{page}/{total}` en y=45, deshabilitados en extremos, redibujan con `BuildBadgesView()`. Además `BuildBadgesView()` ahora llama `ClearContent()` para evitar apilar UI al cambiar filtro/página.
- **Botón RIBBONS** — nuevo nav button `panel total ribbons_0` en `(-700, 20)`; los 6 nav buttons re-espaciados a 140px (chests 260, trofeos 140, ribbons 20, badges -100, settings -220, back -340).
- **Vista Ribbons** — `BuildRibbonsView()` dedicada: grid scrolleable idéntico a badges (`offsetMin (20,90)`, `offsetMax (-20,-160)`, 5 columnas, cards 110×130, listón 50×75 + etiqueta `L{id}`/`???`, color por raza / gris 30% si no ganado). Sin contador ni paginación (22 listones caben en el scroll). Listones removidos de `BuildTrophiesView` (queda copas + tutorial en y=-40).
- **Back unificado** — `ChestUI`, `InsigniaUI`, `CampaignUI` y `ModeSelectionUI` usan ahora `Sprites/Menu/botin ui/panel total back` (`panel total back_0`, fallback color) en vez de `Retry_0`/color plano. Tamaño 150×70, sin texto superpuesto (el sprite trae la etiqueta, como los nav buttons). `ExhibidorUI` ya lo usaba.

## Feature: Game Feel, Jump Animation & Reward Polish

> 2026-08-17 — Hit-stop, pitch variation, dramatic silence, knight jump, paladin beam, campaign rewards, badges cleanup.

| #   | ID                 | Tarea                                                     | Spec | Estado |
| --- | ------------------ | --------------------------------------------------------- | ---- | ------ |
| 57  | 057-game-feel-polish | Game feel (hit-stop, pitch, silence) + knight jump + rewards | —  | done   |

### Detalle 057

- **Hit-stop** — `SoundManager.HitStop(0.05s)` at dice clash, `HitStop(0.06s)` at attacker/defender kills. Freezes `Time.timeScale` via `WaitForSecondsRealtime`. Prevents stacking with `isHitStopRunning` flag.
- **Pitch random ±10%** — All procedural SFX in `GenerateTone`, `GenerateDescendingTone` randomize frequency. Chord voices ±5% per voice. Noise ±15% volume.
- **Dramatic silence** — 0.15s `WaitForSecondsRealtime` pause after dice spin before result reveal.
- **Knight jump animation** — `KnightJump` coroutine with parabolic arc, Salto1 in air + Salto2 landing, squash, `PlayHammer()` + `CombatShake`, dust particles. Triggered when Knight moves >1 cell. Scale 0.55x idle. Back sprite overrides: Human 0.10, Orc 0.33/0.24, Beastfolk 0.38.
- **Beastfolk back jump fix** — `SpritePrefix("Beastfolk")` maps to `"beast"`. `LoadLargestSprite()` picks largest sub-sprite from spriteMode 2 textures.
- **Paladin light beam** — `PaladinLightBeam(visual)` procedural rect beam follows visual, fadeIn→hold with flicker→fadeOut, 6 holy sparks, `PlayHolyBeam()` sound. Triggered on every Paladin movement. Slide 1.3s.
- **CampaignRewardUI** — Full-screen popup (sortingOrder 210) with sequential reveal of gold/insignia/ribbon/chest with white flash, scale pop, panel shake, sounds, coin bounce + particles, sparkle particles. OK button with `botonOK_0`.
- **Cup completion animation** — `CampaignManager.CompleteLevel()` returns cup race on first-time completion. `CampaignRewardUI` shows cup sprite with bounce animation (scale 0.3→1.6→1.0) + `PlayVictory()` at end of reward sequence.
- **Scoreboard skip button** — "SKIP >>" at bottom-right, instantly completes counting with no hammer sounds, 0.2s delay before win/defeat reveal.
- **Scoreboard faster counting** — Normal delays 0.4s→0.25s, inter-team 0.3s→0.2s, post-count 0.5s→0.4s.
- **Post-tutorial flow** — ScoreboardUI → GameOverUI → TutorialRewardUI → GoblinDialogue → ModeSelectionUI (campaign tickets) → FlashAndStartCampaign → level 3 "The Garrison" (scans `cups[1]`).
- **ExhibidorUI badges cleanup** — Removed PREV/NEXT pagination, all badges in one scrollable list. BlueFlag sprite covers broken 1/2 + arrow at bottom (X=-1, Y=-204, 326×269).
- **EnemyBanner** — Loads `enemyBanner` sprite from `Sprites/Decor` (spriteMode 2), white text with black outline, `preserveAspect`.
- **Power-up glow** — Uses `cambio` sprite from `Sprites/PowerUps/Icon/cambio` via `GetGlowSprite()`.
- **CampaignUI lighter panels** — Completed `(0.25, 0.55, 0.25)`, unlocked `(0.3, 0.38, 0.55)`, locked `(0.35, 0.35, 0.38)`. Insignias shifted left to `(0.74-0.90)`.
- **ModeSelectionUI campaign mode** — `ShowCampaign(Canvas)`, campaign tickets (FREE/PREMIUM 20g), HeartbeatPulse on premium, HoverGrow, flash+sound on selection.
- **Common reward panels** — Both ChestUI and ExhibidorUI use darker `(0.45, 0.45, 0.45, 0.9)` for common badges so they don't blend with panelCartaBlue behind.
- **Test buttons moved** — Removed from ChestUI, added to ExhibidorUI settings (TEST CHEST + TEST DAILY).
- **TutorialRewardUI** — panelCartaBlue background, `botonOK_0` claim button. **GoblinDialogue** — lower text, `botonOK_0` continue button.
- **MAGIC powerup icon** — Procedural star fallback via `CreateProceduralMagicIcon()` (no PNG exists).


