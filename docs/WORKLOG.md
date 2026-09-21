## Fix: power-ups caían en los últimos minutos (097)

> 2026-09-16 - Reporte de Dani en APK v0032: tras un rato no aparecían más power-ups. Causa raíz: deadlock en `AutoSpawnTimer` (espera a que haya ≤2 en tablero) pero los power-ups no tenían vida útil y se acumulaban 2 sin recoger hacia el final, bloqueando el spawn para siempre. Fix: vida útil de 30s con fade + destroy.

| #   | ID                    | Tarea                                                      | Estado      |
| --- | --------------------- | ---------------------------------------------------------- | ----------- |
| 97  | 097-powerup-expiry    | ExpirePowerUp: vida útil 30s para liberar slots del spawn  | done        |

### Detalle 097

- **Causa raiz** - `PowerUpManager.AutoSpawnTimer` hace `while (CountSpawned() >= 2) yield return new WaitForSeconds(1f)` (cap de 2 power-ups simultáneos). Los power-ups persistían hasta que una pieza pisaba la casilla. Hacia el final quedan pocas piezas → se acumulan 2 sin recoger → el loop bloquea y ya no spawnea ninguno.
- **Fix** - nuevo `ExpirePowerUp(pu)`: espera 30s de vida útil, fade del icono 0.35s, `spawnedPowerUps.Remove(pu)` + `Destroy(container)`. El slot se libera solo y el ciclo sigue toda la partida. Solo se lanza desde `SpawnOnBoard` (auto-spawn); `SpawnSpecificAt`/placeholders del tutorial intactos.
- **Validacion** - `dotnet build Assembly-CSharp.csproj` = 0 errores (1 warning pre-existente).
## Fix: UI celular + builds v0031/v0032 (096)

> 2026-09-16 - Reporte de pruebas en APK: boton de sonido y skip turn movidos de lugar, trompeta alejada de la orilla. Se normaliza el escalado de canvas (matchWidthOrHeight=0.5) y se re-anclan elementos al borde derecho para pantallas no-16:9. Builds v0031 (EXE) y v0032 (APK) enviados a Dani y al grupo para testeo.

| #   | ID                    | Tarea                                                      | Estado      |
| --- | --------------------- | ---------------------------------------------------------- | ----------- |
| 96  | 096-ui-celular-builds | matchWidthOrHeight=0.5 en gold HUD + re-anclaje SkipTurn/trompeta + rebuild | done        |

### Detalle 096

- **Causa raiz** - el canvas del HUD de oro (`EconomyManager`) no seteaba `matchWidthOrHeight` (default 0) mientras todos los demas usan 0.5, y SkipTurn/trompeta usaban anchor centro/izquierda con posiciones absolutas que se corren en pantallas que no son 16:9.
- **Fix** - `EconomyManager.cs`: `cs.matchWidthOrHeight = 0.5f;`. `TurnUI.cs`: SkipTurnButton re-anclado a borde derecho (anchor/pivot (1,0.5), pos (-10,-217)). `BattleResultUI.cs`: nueva helper `MakeImageRight` (anchor borde derecho) y trompeta re-anclada. Tras reporte de "no aparece la trompeta", pivot pasado de (1,0.5) a (0.5,0.5) con pos (-220,-380): `Image.preserveAspect` con pivot no-centrado puede desplazar la imagen fuera del rect en Unity.
- **Rebuild** - EXE `Builds/v0031/DiceClashTactics_v0031.exe` (369.57 MB) y APK `Builds/v0032_Android/DiceClashTactics_v0032.apk` (96.75 MB). Marcador `ExpoBuild.txt` borrado durante el build (juego completo) y restaurado al final.
- **Gotcha CLI** - `Start-Process` con `-ArgumentList` rompe la ruta del proyecto por el espacio en "Dice Clash Tactics" -> hay que pasar el path con comillas embebidas (`'"C:\....\Dice Clash Tactics"'`), si no falla con `Couldn't set project path to: ...Tactics/C:/Users/...` y exit code 1.
- **Pendiente** - peso WebGL (`.data.br`) no alcanza el target <= 50 MB para CrazyGames. Plan a definir: recompresion de audio (ffmpeg), streaming de Assets, etc.
- **Validacion** - `dotnet build Assembly-CSharp.csproj` = 0 errores (1 warning pre-existente).
## Fix: Nigromantes front/back move + powerups ticket free (086)

> 2026-09-14 - El fix 085 removio el cap de escala de Nigromantes tambien para los movimientos BACK (no eran los sprites a ajustar) -> paladin/caballero move back se agrandaron. Ademas, entrar con ticket FREE (WithoutPowerups) seguia dando power-ups al azul por el override de modo expo.

| #   | ID                    | Tarea                                                     | Estado      |
| --- | --------------------- | --------------------------------------------------------- | ----------- |
| 86  | 086-nigro-move-dir + ticket-free | Back move = formula original; WithoutPowerups bloquea recoleccion azul | done        |

### Detalle 086

- **Escala** - `BoardManager.cs` (AnimatedMove): la normalizacion sin cap ahora aplica SOLO al movimiento FRONT (`movingForward`), que es el que pidio el usuario (los nuevos sprites `PeonNecroMoveFront`, `caballerofrontnigro`, `PaladinMoveFrontNigro2`). El movimiento BACK vuelve a la formula original: no-pawn con `Mathf.Min(sizeRatio, 1.0f)` y pawn con `*0.9f` (restaura el tamano previo de PeonMoveBack/CaballeroMoveBack/PaladinMoveBack). Ninja conserva su escala fija (0.14,0.14,1).
- **Ticket FREE** - `PowerUpManager.cs` (CheckCollectionForTeam): eliminado el override de modo expo `&& !ExpoConfig.Enabled` del guard `if (Team.Blue && WithoutPowerups && !isShadowPhase) continue;`. Ahora SIEMPRE (con o sin marcador ExpoBuild) en modo WithoutPowerups el jugador azul no recolecta/ejecuta power-ups; los enemigos si. Restaura comportamiento del spec.
- **Validacion** - `dotnet build Assembly-CSharp.csproj` = 0 errores (1 warning pre-existente).
## Fix: Nigromantes front Move scale (085)

> 2026-09-14 - Piezas Nigromantes se vean chicas y pisaban a otros al moverse hacia delante. La compensaci�n por �rea de `AnimatedMove` ten�a un l�mite `Mathf.Min(sizeRatio,1.0f)` que imped�a agrandar los nuevos sprites front Move (frames pequenios vs idle). Se elimina el cap y el factor 0.9 del peon.

| #   | ID                 | Tarea                                                     | Estado      |
| --- | ------------------ | --------------------------------------------------------- | ----------- |
| 85  | 085-nigro-move-scale | Escala de sprites front Move de Nigromantes = footprint idle | done        |

### Detalle 085

- **Contexto** - tras el wiring (084), Peon/Caballero/Paladin de Nigromantes usaban sus nuevos sprites front Move. El usuario reporto: se ven CHICOS durante el movimiento front y pisan los sprite de otras piezas.
- **Causa raiz** - los 4 PNG `MoveFront` son tiras verticales de 4 frames; el frame 0 real es pequenio (PeonNecroMoveFront_0 77x92, caballerofrontnigro_0 94x130, PaladinMoveFrontNigro2_0 90x123) frente al idle back (247x351, 140x221, 130x208). La compensacion `sizeRatio = sqrt(idleArea/moveArea)` daba ~1.6x-3.5x (tenia que AGRANDAR), pero el cap `Mathf.Min(sizeRatio, 1.0f)` para no-pawn lo dejaba en 1.0 -> piezas renderizadas a ~0.6x0.8u vs idle 0.9x1.4u.
- **Fix** - `BoardManager.cs` (AnimatedMove): eliminado el cap `sizeRatio = Mathf.Min(sizeRatio, 1.0f)` y el factor `*0.9f` del pawn. Ahora todo Nigromantes en front Move normaliza a footprint idle: Peon ~1.02x1.26u (idle 0.94x1.37), Knight ~0.97x1.32 (idle 0.91x1.41), Paladin ~1.08x1.37 (idle 1.00x1.48). Ninja conserva su escala fija (0.14,0.14,1) solicitada en 081.
- **Validacion** - `dotnet build Assembly-CSharp.csproj` = 0 errores (1 warning pre-existente).
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
- **Knight jump animation** — `KnightJump` coroutine with parabolic arc, Salto1 in air + Salto2 landing, squash, `PlayHammer()` + `CombatShake`, dust particles. Triggered when Knight moves >1 cell. Jump scale per race via `GetJumpScale`: Human 0.60/0.5 front · 0.09/0.09 back, Orc 0.55/0.6 front · 0.33/0.34 back, Beastfolk 0.66/0.52 front · 0.41/0.34 back, Nigromantes 0.15/0.12. `AnimatedMove` skips the Move animation when `useKnightJump` so no coroutine overwrites the Salto sprites.
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

## Feature: Sound Effects, Visual Polish & Campaign UI

> 2026-08-19 — Knight/Ninja sounds, Paladin holy trail, cloud-beam fix, campaign panels bigger, tutorial trophy repositioned.

| #   | ID                 | Tarea                                                     | Spec | Estado |
| --- | ------------------ | --------------------------------------------------------- | ---- | ------ |
| 58  | 058-sound-visual-polish | Sonidos caballero/ninja, pasos sagrados, nube, campaña UI | —  | done   |

### Detalle 058

- **Paladin light beam cloud fix** — `PaladinLightBeam()` in `BoardManager.cs`: cloud (`Nube` sprite, sortingOrder 4, scale 0.35) positioned at `pos + Vector3.up * 4.5f`, beam (height 4f, sortingOrder 2) at `pos + Vector3.up * 2.5f` — beam top meets cloud bottom, cloud covers beam's top edge. Both follow visual and fade together. `SoundManager.PlayChoir()` loads `Sounds/Efectos/choir.mp3`, called at beam start.
- **Knight salto2 sound** — `SoundManager.PlaySalto2()` loads `Sounds/Efectos/salto2.mp3` (volumen 0.8). Llamado en `BoardManager.KnightJump()` al mostrar sprite salto2 (aterrizaje). Eliminado `PlayHammer()` del KnightJump (duplicado).
- **Ninja move sound** — `SoundManager.PlayNinja()` loads `Sounds/Efectos/ninja.mp3` (volumen 0.7). Llamado en `AnimatedMove()` cuando `movingData.type == PieceType.Ninja`.
- **Paladin pasos sagrados** — `PaladinPasos(visual, fromPos, toPos, duration)` en `BoardManager.cs`: spawna sprites `Sprites/PowerUps/Efect/pasos` cada 0.18s en la posición actual del Paladín durante el slide. Scatter ±0.25 horizontal, ±0.2 vertical. Rotación random ±15°. Escala 0.28–0.38. Color dorado `rgba(1, 0.95, 0.7, 0.5)`, sortingOrder -1. `FadePasos()` espera 0.3s visible luego desvanece en 0.9s. Se ejecuta durante movimiento de Paladín (1.3s slide).
- **Campaign UI más grande** — `CampaignUI.cs`: tarjetas de nivel 330×135 (antes 280×90), sprite de copa 245×259 (antes 110×140). Default `cardHeight = 135f`.
- **Tutorial trophy repositioned** — `ExhibidorUI.cs`: copa tutorial movida a `(-161, 76)` (antes `(-303, 82)`), ya no tapada por Iron Crown. Nombre de corona tutorial en `(-10, -12)` (solo aplica a TUTORIAL, el resto queda en `(0, -12)`).
- **Build v0003** — EXE compilado a `Builds/v0003/DiceClashTactics_v0003.exe`.

## Feature: Campaign Map Paginado + Estrellas + Fuera Copa (059)

> 2026-08-21 — Mapa de campaña horizontal paginado (4 trofeos), tarjetas panelCartaRed, estrellas por performance, copa del HUD eliminada.

| #   | ID                 | Tarea                                                     | Spec | Estado |
| --- | ------------------ | --------------------------------------------------------- | ---- | ------ |
| 59  | 059-campaign-map   | Mapa campaña paginado, estrellas, fuera copa, monedas→bolso | [spec](docs/specs/059-campaign-map.md) | in-progress |


## Feature: Tester Feedback Round 3 + Rey Cabezon

> 2026-08-24 - Fixes del tester (OK duplicado sobre sprite, BATTLE OVER abajo, numeros grandes en dados, dialogo tutorial grande), flujo de cabeza del mapa de campania y Dismiss() del GameOverUI al abrir el mapa.

| #   | ID                        | Tarea                                                        | Estado      |
| --- | ------------------------- | ------------------------------------------------------------ | ----------- |
| 60  | 060-tester-fixes-map-head | Tester round 3 + cabeza del mapa + Dismiss GameOverUI        | in-progress |
| 61  | 061-rey-cabezon           | Jefe Rey Cabezon: niveles 23-26, 2x2, 5 HP, d20 fase 2       | cancelled   |
| 62  | 062-reset-cheat           | Cheat reset total desde menu principal (para eventos)         | in-progress |
| 63  | 063-tester-round4-ui      | Totales por equipo en dados, OK sprite, cards mapa, fondo Nigromantes, fonts tickets | in-progress |
| 64  | 064-magic-icon-ritual     | Icono cambio para MAGIC + secuencia ritual del mago (circulo→mago→ataque→humo) | in-progress |
| 65  | 065-shake-fist-sprite-fixes | Puño parabólico para Shake + LoadFullSprite (fix import Multiple de cambio/ritual) | in-progress |
| 66  | 066-magic-icon-adjustments | Glow neutral (cambio solo en MAGIC), íconos 70% casilla, MAGIC sin mago sobre objetivo | in-progress |
| 67  | 067-art-sound-fixes | Puño desde derecha, lock hand-drawn, estrellas por rendimiento+tiempo, música par, fondo Nigromantes, reward más grande | in-progress |
| 68  | 068-candado-fondo-back-musica | Candado 80×80, fondo Nigromantes fix (meta Single), Back→menú, música campaña | in-progress |
| 68b | 068-candado-fondo-back-musica | Fix iteración: lock transparente, música en menú campaña, CAMPAIGN button Y, REWARD font | done |
| 68c | 068-candado-fondo-back-musica | Fix iteración 2: candado sin candado (solo panel oscuro), Knight jump backward scale dinámica | done |
| 69  | 069-emoji-floor-fix | Emoji fallback Wolf/NewRace→Human, CollapseDestroyedCell no come turno | done |

## Detalle 068b

> 2026-08-25 — Iteración sobre 068.

- **Música campaña** — `SoundManager.cs`: `PlayCampaignMenuMusic()` llama desde `CampaignMapUI.ShowInternal()`. `Close()` restaura `PlayMenuMusic()`. Campo dedicado `campaignMenuTrack`.
- **CAMPAIGN button** — `MainMenuManager.cs`: Y de -510 a -470 (junto con chest e insignia).
- **REWARD font** — `CampaignRewardUI.cs`: font 24→28, subtitle 16→18.
- **Lock transparente** — `CampaignMapUI.cs`: card tint `Color.clear`, outline `Color.clear`. Solo candado dorado 80×80 visible.

## Detalle 068c

> 2026-08-25 — Feedback del tester: quitar candado, fix Knight jump backward.

- **Candado eliminado** — `CampaignMapUI.cs`: Se eliminó `CreateLockIcon` call. Panel oscuro restaurado `(0.33, 0.33, 0.36, 0.82)`, outline `(0, 0, 0, 0.15)`.
- **Knight jump backward** — `BoardManager.cs` `KnightJump()`: escalas hardcoded (Human 0.10, Orc 0.33/0.24, Beastfolk 0.38) reemplazadas por cálculo dinámico `idleScale * 0.55 * sqrt(frontArea/backArea)` que compensa sprites back más grandes manteniendo tamaño visual consistente.

## Detalle 069

> 2026-08-25 — Fixes: emoji para Wolf/NewRace, broken floor comiendo turno, fondo Nigromantes en CampaignMapPanel.

- **Emoji fallback** — `BoardManager.cs` `CreateEmojiSprite()`: reestructurado para iterar por carpetas (species primero, luego Human). `emojiKey` se deriva de la carpeta actual, no de la especie original. Wolf→Beastfolk, NewRace→Human.
- **Broken floor fix** — `ObstacleManager.cs` `CollapseDestroyedCell()`: eliminada llamada a `board.CheckVictoryAndEndTurn()` que ejecutaba `EndTurn()` y hacía que el enemigo jugara dos veces. Ahora solo verifica si alguien ganó para mostrar Scoreboard, sin cambiar turno.
- **Nigromantes background CampaignMapPanel** — `CampaignMapUI.cs` `GetBackgroundSprite()`: agregado caso "Nigromantes" → `Sprites/Nigromantes/Background/Nigromantes`. También `GetCupSprite()` tiene caso explícito.
- **Race list** — Niveles 1-4,6,9,12,14,16,19 = Human; 5,7,10,13,17,18 = Wolf; 8,11,15,21 = Orc; 20,22 = NewRace.

## Detalle 070

> 2026-08-27 — Fix tamaño salto caballero (por raza) + carrera de corrutinas + ninja en escenario Nigromantes.

- **Salto caballero por raza** — `BoardManager.cs` `GetJumpScale(species, movingForward)` reemplaza el 0.9 fijo y los overrides viejos. Valores confirmados en Editor: Human 0.60/0.5 front · 0.09/0.09 back, Orc 0.55/0.6 front · 0.33/0.34 back, Beastfolk 0.66/0.52 front · 0.41/0.34 back, Nigromantes 0.15/0.12. Misma escala en salto1 (inicio) y salto2 (aterrizaje).
- **Carrera de corrutinas (causa raíz del orco)** — `AnimatedMove` arrancaba `PlayAnimation` (Move) SIEMPRE antes de decidir salto/deslizar. Esa corrutina `AnimateSprites` vivía en paralelo pisando `sr.sprite` durante el salto del caballero. Ahora `useKnightJump` se calcula antes y el bloque Move queda dentro de `if (!useKnightJump)`.
- **Ninja en escenario Nigromantes** — El ratio de escala de Nigromantes (compensaba sprites de movimiento más grandes) se aplicaba según `SpriteFolder(scenarioTheme)` (el escenario del nivel), por lo que estiraba al ninja humano (azul) cuando jugaba en nivel Nigromantes (5/22). Ahora condiciona con `SpriteFolder(ThemeForTeam(movingData.team)) == "Nigromantes"` para aplicar solo a piezas realmente nigromantes.

## Feature: Tweaks de Escala + Hit Sprite Fuego/Rayo

> 2026-08-28 — Ajustes de escala (salto back humano, ninja nigromante) y sprite quemado que reemplaza al PJ al impactar Fireball/Lightning.

| #   | ID                 | Tarea                                                     | Estado      |
| --- | ------------------ | --------------------------------------------------------- | ----------- |
| 70  | 070-jump-scale     | Fix tamaño salto caballero por raza                       | done        |
| 70b | 070-scale-tweaks   | Salto back humano 0.13 + ninja nigromante move 0.15/0.16  | done        |
| 71  | 071-fire-lightning-hit | Sprite quemado reemplaza al PJ al impactar fuego/rayo | done        |
| 71b | 071b-hit-tweaks    | Sonido antes, sprites +15%, intercalado 0.1s, suelo quemado | done   |
| 72  | 072-goldpanel-sprite | GoldPanel HUD usa sprite `panelVictoria_0`            | done        |
| 73  | 073-expo-build       | Marcador ExpoBuild.txt, ExpoConfig, DebugShortcuts (Ctrl+F/R), pacing powerups | done |
| 74  | 074-expo-visual-fixes | Devoluciones visuales de Dani sobre EXE v0004 (trofeos, insignias, tickets, dados, oro, timer, banner) | done |
| 74b | 074b-expo-visual-fixes-2 | Segunda ronda de ajustes visuales (negros en bordes, copas alineadas, ribbons popup legible, dado en tarjeta, sonido a la izquierda del oro) | done |
| 74c | 074c-expo-visual-fixes-3 | Tercera ronda (tarjeta campaña fuera, GOLD/tickets más grandes, quitar DefIcon, sound toggle, dado alineado, atajo daily bonus Ctrl+D) | done |
| 74d | 074d-expo-fixes-round4 | Powerup-kill victoria, DefDice, numeración campaña, badges nav, ribbons LevelName, peones animados, fix doble reward post-victoria | done |

## Detalle 074c

> 2026-08-31 — Tercera ronda de devoluciones visuales sobre EXE v0004 + atajo de daily bonus. Solo visual + un atajo de debug. Compilación Roslyn = 0 errores.

- **CampaignMapUI cards (dentro de la tarjeta)** — El card real es 340×150 (horizontal); la 074b puso el número y el nombre en y=90 (FUERA, el card va de -75 a 75). Rediseñado dentro de los límites: número id `(98,30)` size (52,50), nombre de enemigo agrandado font 10→13 en `(-40,30)` size (220,50) MiddleCenter, datos `(HUMAN | E:N | +Pg)` en `(-50,-35)` size (200,24) (ya no choca con la insignia), insignia reducida (89,73)→(70,58) en `(125,-35)`, check movido `(152,30)` (antes (0,145) fuera).
- **ModeSelectionUI GOLD + tickets** — `CreateGoldDisplay`: "GOLD: n" font 20→40 (doble) con outline negro (2,-2), rect de altura 0.1→0.2. Tickets `CreateTicketCard`: card 320×320→416×416 (×1.3) y contenido ×1.3 — title 14→18, subtitle 10→13, price 16→21, desc 9→12, botón Play (170,55)→(221,71). Los tickets en ±190 siguen sin solaparse (mitad 208 → -398..-18 y 18..398).
- **BattleResultUI — iconos chicos eliminados** — AtkIcon y DefIcon retirados por completo (creación `MakeImage`, `LoadCardSprite` y campos `atkIconImage`/`defIconImage`). Antes solo se ocultaban con alpha 0. Cuando el atacante era rojo los iconos quedaban cruzados; ahora no se crean.
- **TurnUI SoundToggle** — Reposicionado `(-276,-46)`→`(-288,-18)` (pivot 1,1), a la izquierda del panel de oro.
- **CharacterCardUI dados alineados** — Bloques ATK/DEF con los dados en la misma línea horizontal: AtkDice `(80,0)`→`(66,2)`, DefDice `(80,0)`→`(80,-1)` (valores de ejemplo del ninja del usuario). Suffix en 124 con separación clara del dado. Ambos bloques (icon/prefix/dice/suffix) alineados horizontalmente.
- **Atajo daily bonus (Ctrl+D)** — `DebugShortcuts` agrega `Ctrl+D` (gate `ExpoConfig.Enabled || Application.isEditor`, igual que Ctrl+F/Ctrl+R): llama `DailyBonusUI.ShowForDebug()`. `DailyBonusUI.ShowForDebug()` fuerza el popup — si `ClaimDailyBonus()` devuelve >0 muestra el monto real, si ya se reclamó hoy muestra 100 de prueba para probar el layout.

## Detalle 074b

> 2026-08-31 — Segunda ronda de devoluciones visuales sobre EXE v0004. Solo visual, 0 fallos funcionales. Compilación Roslyn = 0 errores.

- **DailyBonusUI** — Outline de título/monto/streak de blanco → negro `Color.black` (borde en negro). Se revierten `panelBg.color` y `btnBg.color` (que seguían en `Color.white` tras el reemplazo global).
- **ExhibidorUI nav** — Nav buttons re-espaciados con separación uniforme mayor (spacing 150): back 360, chests 210, trofeos 60, ribbons -90, badges -240. Separación chests/trofeos 150 (antes 120, "pegados").
- **Badges (ExhibidorUI BuildBadgesView)** — Insignia (icon) más a la derecha: `anchoredPosition x 40→72`. Título (nombre de insignia) más abajo: anchors y `(0.35,0.85)`→`(0.3,0.78)`.
- **Cups alineadas** — `BuildTrophiesView`: todas las copas a Y=72 con tamaño uniforme (85,108), espaciado 165: TUTORIAL(-330), IRON(-165), BLOOD(0), WILD(165), VOID(330). Antes IRON(-323)/BLOOD(5) desajustadas y VOID en y=74 size (96,125).
- **Ribbon popup legible** — `ShowRibbonPopup`: fondo `contentBg` claro (fallback `(0.95,0.92,0.82)` en vez de oscuro), overlay 0.75→0.55 (no tapa tanto el título RIBBONS), content bajado a y=-80 (se genera debajo del ribbon, no encima). Textos legibles sobre fondo claro: LevelName `(0.2,0.16,0.08)`, CupName `(0.35,0.25,0.1)`, Date `(0.25,0.2,0.12)` (antes grises/marrones ilegibles). Título gold con outline negro.
- **CampaignMapUI cards** — Nombre de enemigo al centro y número más a la derecha: id grande `(-122,28)`→`(118,90)`, level.name MiddleCenter `(-30,90)`, info centrada `(0,-30)`. Check de completado movido a `(0,145)` para no chocar con el número.
- **TurnUI turn timer** — Outline del TurnTimerText (segundos de tu jugada) de blanco → negro, effectDistance (2,-2).
- **EconomyManager HUD** — Bolsa 40px con Outline negro; goldText font 20→22 con outline negro (2,-2); panel de oro movido a x=-66 (size 200×52) para dejar espacio al sonido a su izquierda; textRt reposicionado.
- **TurnUI SoundToggle** — Movido a la IZQUIERDA del panel de oro, alineado verticalmente con su centro `(-276,-46)` (pivot 1,1).
- **CharacterCardUI layout dado** — Orden nuevo en bloques ATK/DEF: `[icono espada/escudo] "2" [dado 26px] "6+n"` con separación clara (icon 10, "2" 54, dado 80, suffix 116). Antes el dado (x=70) se superponía con el "2" (50-66).



## Detalle 074

> 2026-08-31 — Devoluciones visuales de Dani sobre el EXE expo v0004. Solo visual, 0 fallos funcionales. Compilación Roslyn = 0 errores.

- **ExhibidorUI / InsigniaUI (trofeos e insignias)** — Descripción/subtítulo de insignias eliminado (por ahora sin tooltip). Nombre de insignia font 8→10/11, anchors compactados hacia arriba. Checkmark font 9→16/18 con outline verde oscuro `(0.05,0.45,0.05)`. Nav buttons re-centrados sin settings (back 315, chests 195, trofeos 75, ribbons -45, badges -165; spacing 120). Quitado botón Settings + `BuildSettingsView()` (TEST CHEST/TEST DAILY eliminados). Cups unificadas a Y=72, labels font 8 centrados, size (170,22) pos (0,-14). DailyBonusUI: outline blanco en título, monto y streak.
- **CampaignMapUI (cards mapa)** — Textos de card reposicionados más adentro sin reducir fuente: level name `(8,40)`→`(-40,40)` size `(210,40)`→`(150,40)`; info `(-20,-30)`→`(-40,-30)` size `(250,30)`→`(170,24)`. Evita desborde por la derecha y solape con insignia.
- **ModeSelectionUI (tickets)** — Textos del ticket re-distribuidos con espaciado parejo y título un poco más abajo (title 0.7-0.88, subtitle 0.56-0.7, price 0.42-0.56, desc 0.16-0.38). SIN cambio de colores de botón (no pedido). Oro del panel de selección: font 12→20 con outline blanco (igual estilo que partida).
- **TurnUI (timer)** — TimerText font 34→24, color naranja `(1,0.65,0.15)` con outline blanco. TurnTimerText font 25→18 con outline blanco. SoundToggle (on/off) reposicionado junto al panel de oro (esquina sup-der, `(-14,-46)`); QuitButton bajado a `(-14,-120)`. Fix de indentación en Update (color timer fuera del else).
- **BattleResultUI (dados)** — Nombres font 16→22 y en MAYÚSCULAS (`ToUpper`). Iconos chicos AtkIcon/DefIcon ocultos (color alpha 0 + raycast off; LoadCardSprite intacto). Números grandes font 64→48 (no se salen). BlueResult/RedResult centrados y en la misma línea Y `(-240,-190)`/`(240,-190)` (antes asimétricos -199/-187). AtkStat/DefStat y AtkAbility/DefAbility rects a 280 de ancho para que DEF/AURA no se salgan de la línea horizontal.
- **CharacterCardUI (cartas)** — Dado icon 22→26px en bloques ATK y DEF (own/enemy), manteniendo centrado vertical con "2" y "6+n".
- **EconomyManager (oro HUD)** — Panel 190×48→220×52. Bolsa 32→38px. goldText font 16→20 con outline blanco, textRt más ancho (140×30) y reposicionado. Sonido on/off queda a la derecha del oro (ver TurnUI).
- **EnemyBanner** — font 14→16, texto full-stretch (ya centrado verticalmente).



### Detalle 070

> 2026-08-27 — (ya registrado arriba). Fila agregada para tracking.

### Detalle 070b

- **Salto back caballero humano** — `BoardManager.cs` `GetJumpScale`: Human back `(0.09, 0.09)` → `(0.19, 0.19)` → `(0.13, 0.13)` final (0.19 quedaba muy grande, confirmado en Editor). Ambos ejes, aplica a salto1 y salto2.
- **Ninja nigromante en movimiento** — `BoardManager.cs` `AnimatedMove`: el ninja nigromante usa escala fija `(0.15, 0.16)` al moverse, reemplazando el ratio por área (el resto de piezas nigromantes conserva el ratio).

### Detalle 071 / 071b

- **Hit sprite fuego/rayo** — `PowerUpManager.cs` `ElementHitDestroy`: al impactar Fireball/Lightning el sprite del PJ se reemplaza por el quemado (`fuego1`+`fuego2` o `rayo1`+`rayo2`, frame 0 de cada archivo = 2 frames), tamaño normalizado por área para que ocupe su lugar, sortingOrder 16, color blanco.
- **Ajustes (071b)** — sonido flame/electricity arranca al inicio del impacto (antes del swap); sprite +15% más grande (`origScale *= 1.15f`); intercalado 1-2-1-2 cada 0.1s; bombardero (burn) 0.65s con pulso de escala y alpha, luego fade+shrink y `Destroy`.
- **Suelo quemado (071b)** — `BurnGroundAt(killPos, color)` deja una marca quemada en la casilla del impacto: círculo procedural `GetCircleSprite`, escala `cellSize * 0.98`, sortingOrder -1, hold 2.8s + fade 0.6s (reusa `FadeBurn`). Fuego `(0.12,0.06,0.02,0.7)`, electricidad `(0.05,0.05,0.1,0.7)`.
- **Sonido truncado** — `SoundManager.cs`: `PlayFlame(duration=1.2)` / `PlayElectricity(duration=1.1)` usan AudioSource temporal (`PlayTempClip`) que se destruye a los `Mathf.Min(duration, clip.length)` vía `DestroyAfterDelay` — ya no se escucha el clip completo.

### Detalle 072

- **GoldPanel con sprite** — `EconomyManager.cs` `CreateGoldHUD`: el HUD de oro usa `Sprites/Menu/panelVictoria` (`panelVictoria_0`, 1280×220, LoadAll) como fondo (banner que estira bien al rect actual 190×48); fallback `panelCartaBlue` (`panelCartaBlue_0`) o color sólido. Eliminado el hijo `Border`. Helper `LoadPanelSprite()`.

### Detalle 073

- **ExpoConfig.cs** (nuevo) — `Resources.Load<TextAsset>("ExpoBuild") != null` detecta el build de expo (marcador opcional `Assets/Resources/ExpoBuild.txt`; si no está, el juego va completo con tutorial). `ApplyBootState()`: marca `TutorialPlayed`, completa `Campaign_Level_1/2` y sube el oro a mínimo `StartingGold=500`. Llamado en `MainMenuManager.Start`.
- **DebugShortcuts.cs** (nuevo) — componente persistente (DontDestroyOnLoad, creado por `RuntimeInitializeOnLoadMethod`), activo solo con `ExpoConfig.Enabled || Application.isEditor`. **Ctrl+F**: oculta/muestra SOLO los botones de testeo — `TutorialButton`, `CampaignButton`, `ChestButton`, `InsigniaButton`, `SpeciesButton`, `ScenarioButton`, `QuitButton` de la partida (bajo `TurnCanvas`) y todo lo que esté bajo `TestCanvas` (WIN/LOSE/SCORE/LVL9/LVLS). El `QuitButton` de pantalla final (`GameOverCanvas`) NO es test. Los botones de juego (PlayButton, SoundToggle, SkipTurnButton, BackButton, flechas `Arrow_*`, TrophiesButton, RankedButton) nunca se tocan. El estado se reaplica tras cargar escena. **Ctrl+R**: `PlayerPrefs.DeleteAll()` → estado post-sombras (TutorialPlayed + niveles 1-2) → `PlayCampaign(primer nivel de cup 1, WithPowerups)` directo (fallback lvl 3), replicando la lógica de `ModeSelectionUI`.
- **BackButton campaña arreglado** — `CampaignMapUI.cs`: el botón Back se creaba en `CreateHeader()` ANTES de `BuildPages()`, así que los pages (fondo opaco full-screen, `raycastTarget=true`) lo tapaban y quedaba invisible/inservible. Fix: referencia `backButtonRt` y `SetAsLastSibling()` al final de `BuildPanel()` → queda primero en el orden de render (arriba-izquierda, anchor `(0,1)` pos `(15,-10)`).
- **Pacing powerups (ambos builds)** — `PowerUpManager` `StartAutoSpawn()` arranca `AutoSpawnTimer` como loop: espera hasta que haya ≤2 powerups esparcidos, delays 28-34s (t>240) / 20-26s (t>120) / 15-20s (final), `Mathf.Max(6f)`, ×0.75 en expo. Ya no se reinicia el timer desde `SpawnOnBoard`; `GameManager.InitPowerUpsDelayed` solo llama `StartAutoSpawn()` (sin los 2 spawns iniciales de 0.5s/0.8s).
- **Rotación de tipos** — `SpawnOnBoard` elige tipo por cola rotatoria (`rotationQueue`): shuffle al vaciarse, descarta los ya esparcidos (`IsTypeSpawned`), fallback random → más variedad sin repetir de golpe.
- **Overrides expo** — en campaña, expo ignora la restricción de powerups del nivel (usa `allTypes`); el bloqueo de recolección del azul en `WithoutPowerups` se salta en expo (línea ~449). En el build completo el comportamiento es el previo.

## Detalle 074d

> 2026-09-01 — Fix powerup-mata-último-enemigo, DefDice corregido, numeración campaña global, botón badges, LevelName del listón, peones animados y fix del doble reward tras ganar nivel. Compilación Roslyn = 0 errores.

- **Fix powerup-mata-último-enemigo** — los powerups (Shake/Explosion/Fireball/Lightning/MAGIC) nunca verificaban la victoria, así que el juego quedaba atascado al matar al último enemigo con ellos (`CheckVictoryAndEndTurn` solo corría desde `EndTurn`). `BoardManager.cs`: nuevos `CheckVictoryOnly()` (misma lógica de winner, sin `EndTurn`) y `DelayedShowScoreboard(Team)` (0.3s) con guard `suppressVictoryCheck`. `PowerUpManager.cs`: `board.CheckVictoryOnly()` al final de `ShakeWithFist`, `ExplosionEffect`, `FireballEffect`, `LightningEffect` y `MagicEffect`.
- **DefDice** — `CharacterCardUI.cs` `CreateCard`: `defDice` movido `(88,-1)`→`(69,0)` (26×26, medido por el usuario para que no se pise ni quede bajo los números). `AdjustEnemyCard()` re-posiciona `enemyCard/DefBlock/DefDice` a `(68,1)`.
- **Numeración campaña (truncamiento "10"-"22")** — la fuente Press Start 2P ~30px/dígito a font 30 cabe ~52px de rect, así que dos dígitos se cortaban y solo se veía "1"/"2" (los niveles 1-9 ya salían). `CampaignMapUI.cs:260`: `(slot+1).ToString("D2")` → `level.id.ToString()` (número global sin padding; antes `D2` confundía 03-09 con "03/09") y rect del número `(52,50)`→`(70,50)`. Ahora se ven "3"..."22" completos.
- **Botón badges alineado** — `ExhibidorUI.cs:102` quitado el size custom `(332.1,132.8)` y el scaleX 1.09 (quedaba más abajo que el resto); usa el default `(280,135)` igual que los otros nav buttons, y=-240 (espaciado 150 uniforme).
- **LevelName debajo del listón** — `ShowRibbonPopup`: LevelName `(0,-60)`→`(0,-35)` (queda justo debajo del listón), CupName→`(0,-80)`, Date→`(0,-110)`.
- **Peones animados al moverse** — `BoardManager.cs` (~1470) el flag `useMoveAnim` solo contemplaba Paladin/Knight, así que peones (y Beastfolk de espaldas) se movían rígidos. Añadido `PieceType.Pawn` (`PeonMoveBack` tiene 4 frames y ya anima). Ninja se deja fuera a propósito (usa `NinjaMoveCloud`).
- **Fix doble reward tras ganar nivel (bug reportado por el usuario en nivel 20)** — tras el scoreboard/reward/GameOver/Next al mapa, sonaba de nuevo el contador, aparecía otro reward con los mismos premios y botones Next/Quit sobre el mapa, y el nivel quedaba con doble check. Causa raíz: el timer de turno de `TurnManager` (20s, `turnTimeRemaining`) seguía corriendo tras la victoria; `ScoreboardUI.Show()` solo paraba el timer de partida (`TimerManager.Stop()`). A los ~20s `TurnManager.Update`→`EndTurn()` (redAlive==0) → `StartCoroutine(CheckVictoryAndEndTurn())` → segundo `ScoreboardUI.Show()` (el guard `isShowing` se resetea en la línea 346 ANTES de que termine el reward, así que no bloqueaba) → pipeline duplicado + `CampaignManager.CompleteLevel` duplicado (doble check). Fix: en `ScoreboardUI.Show()`, punto único por el que pasan los 3 caminos de victoria (BoardManager, AIController, ObstacleManager), se llama `TurnManager.PauseTimer()` (`timerRunning = false`) → el timer ya no expira nunca después de la partida. Roslyn = 0 errores.
- **Fix tablero visible 1s al volver al menú (074d)** — ganado el nivel, al pulsar BACK en el mapa de campaña se veía el tablero con las piezas restantes ~1s antes del menú. Primera hipótesis (`Hide()` destruía el mapa opaco antes del `LoadScene`) se descartó al reordenar `Close()` y seguir viéndose. Causa real: durante `LoadScene("MainMenuScene")` (~1s de carga) no queda NINGÚN camera activa → Unity conserva el último frame renderizado (el tablero con piezas) en pantalla hasta que entra la escena del menú. Fix: nuevo `SceneCover.cs` (helper estático) — `Show()` crea un overlay negro fullscreen `ScreenSpaceOverlay` (sortingOrder 999, `DontDestroyOnLoad`) que sigue renderizando durante la carga (los overlays no necesitan cámara) y tapa el frame viejo; `CampaignMapUI.Close()` llama `SceneCover.Show()` justo antes del `OnClose`/load; `MainMenuManager.Start` hace `SceneCover.Clear()` (empieza con el primer frame del menú ya cargado, sin negro pegado). `Close()` quedó reordenado (música → `OnClose` → `Hide()` → `Destroy(this)`). **Guard**: `CampaignMapUI.loadsSceneOnClose = true` solo en `GameOverUI.OpenCampaignMap` — `Close()` solo llama `SceneCover.Show()` con ese flag; si el mapa se abre desde el menú principal (`OnClose` solo anula la referencia, sin load) NO se crea el overlay para no dejar pantalla negra pegada (validado: 2 puntos de apertura, menú líneas 703/745 y post-victoria 619). Botón badges NAV confirmado por el usuario como el problemático.
- **Nav buttons de trofeos uniformes (074d)** — las medidas que pasó el usuario (-697, -224, 286×145) no coinciden con código uniforme (-700, -240, 280×135); la causa es el SPRITE: los `panel total X` tienen aspectos distintos (badges 249×124=2.008, chests 244×109=2.239, ribbons 238×101=2.356, trofeos 234×109=2.147, back 221×103=2.146) y `CreateNavButton` usaba `img.preserveAspect = true`, así que cada botón dibujaba el sprite DENTRO del rect a tamaño distinto (badges quedaba más chico/alejado). Fix: eliminado `preserveAspect` en `ExhibidorUI.CreateNavButton` → todos los sprites estiran a 280×135 idénticos (distorsión ≤~13% en ribbons, imperceptible en paneles). Roslyn = 0 errores.
- **Ribbons restaurado + badges stretch (074e)** — el fix global (quitar preserveAspect) hizo que el usuario viera RIBBONS "raro": su sprite aspecto 2.356 estirado a 135px de alto = +34% vertical. Análisis por código (System.Drawing, caja opaca por sprite): badges 2.008 casi llenaba el rect aun con preserveAspect (por eso "ni lo tocaste"), ribbons era el único con distorsión notable. Fix: `CreateNavButton(..., bool preserve = true)`; chests/trofeos/ribbons/back con preserveAspect (como antes), SOLO badges `preserve: false` → 280×135 full, igualando a los demás (su arte estira solo ~12%/9%, imperceptible). Roslyn = 0 errores.
- **StartupVideo Gamanbit (074e)** — video de presentación entre el logo de Unity y el menú. Nuevo `StartupVideo.cs`: `Show()` (guard estático por sesión) crea GameObject DontDestroyOnLoad con canvas ScreenSpaceOverlay (sortingOrder 250), RawImage fullscreen + VideoPlayer (VideoRenderMode.RenderTexture, url `Application.streamingAssetsPath + "/Video/gamanbit.mp4"`), fondo negro, Button invisible fullscreen para fejar (skip), `loopPointReached`/`errorReceived` → fade-out 0.4s → destroy. Si el archivo no existe o la preparación no termina en 6s → fade-out sin video (el juego no se rompe). Conectado en `MainMenuManager.Start` (después de la música). Carpeta `Assets/StreamingAssets/Video/` creada — el usuario debe poner ahí `gamanbit.mp4`. Roslyn = 0 errores.
- **StartupVideo fixes NRE + canvas (074e)** — el usuario probó con un `gamanbit.mp4` VACÍO (0 bytes) y saltó el NRE de WMF ("empty file") + `NullReferenceException` en `StartupVideo+<FadeOut>d__13.MoveNext()` línea 129. 3 bugs reales arreglados: (1) el canvas se creaba como objeto RAÍZ (no hijo del GO), así que `Destroy(gameObject)` NO lo destruía → la pantalla negra quedaba pegada para siempre sobre el menú; ahora el canvas es CHILD del root y el `Destroy` cascadea; (2) `errorReceived` + `Timeout` disparaban CO-RUTINAS `FadeOut()` dobles → `AddComponent<CanvasGroup>` podía devolver null en la segunda y `group.blocksRaycasts` NREaba; nuevo flag `fading` + null-guards (GetComponent+AddComponent) + corrutina ejecutable solo una vez; (3) el skip NO funcionaba porque el canvas del video no tenía `GraphicRaycaster` (los raycasts de UI requieren uno por canvas) → agregado. Añadido short-circuit `File.Exists` (no WebGL) para saltar directo si no hay archivo, y timeout de preparación 6s. Roslyn = 0 errores.
- **Ícono del peón humano (074e)** — responder al usuario: SÍ, setea el icono en Project Settings → Player → Icon (Windows .ico, Android PNG, WebGL favicon). El usuario aclaró que se refería a `Sprites/Menu/menuHuman.PNG` (retrato del peón humano, 341×366 spriteMode 2, slice `_0` rect (6,5) 294×357, caja opaca 242×283 @ (35,49)). Generados vía System.Drawing (recorte caja opaca 242×283 + padding 7%): `Assets/Icons/app_icon.png` (512×512) y `Assets/Icons/app_icon.ico` (256×256, entrada PNG 95KB). Roslyn = 0 errores.
- **Botones de testeo por build (074e)** — el usuario pidió: para subir WebGL/APK se sacan los botones que oculta Ctrl+F; para el build de PC se mantienen los atajos. Decisión: quitar TODOS los que oculta Ctrl+F (incluye TutorialButton y RankedButton — flujos reales pero el usuario eligió sacarlos; en WebGL/APK el jugador igual llega a campaña/ranked vía Play→ModeSelectionUI). Implementado con `DebugShortcuts.DevBuild` (editor || WindowsPlayer || LinuxPlayer || OSXPlayer): en WebGL/Android es false → `if (!DevBuild) return;` al inicio de cada creación (TestButtons en GameManager, TutorialButton/CampaignButton/ChestButton/InsigniaButton/RankedButton/BuildResetCheat en MainMenuManager, SpeciesButton/ScenarioButton/QuitButton en TurnUI) y los atajos de `DebugShortcuts.Update()` pasan de `ExpoConfig.Enabled || Application.isEditor` a `if (!DevBuild) return;`. RankedUnlockSequence ya tiene null-guard para botón ausente. Aprovechado para arreglar `isWeb` huérfano de TurnUI (`bool isWeb` eliminado por una edición previa dejó `SetActive(!isWeb)` roto) → `SetActive(Application.platform != RuntimePlatform.WebGLPlayer)`. Roslyn = 0 errores.
- **Builds v0005/v0006/v0007 (074e)** — compilados por CLI en batchmode (Unity 6000.0.71f1, `-executeMethod BuildScript.BuildWindowsCLI`/`BuildAndroidCLI`/`BuildWebGLCLI`, exit 0): **Windows** → `Builds/v0005/DiceClashTactics_v0005.exe`; **Android** → `Builds/v0006_Android/DiceClashTactics_v0006.apk` (179 MB, debug keystore — sirve para sideload/itch, no Play Store; si hace falta firma propia, configurar keystore en Player Settings); **WebGL** → `Builds/v0007_WebGL/` (index.html + Build/*.br brotli, primera build IL2CPP lenta ~30 min, `gamanbit.mp4` empaquetado 45 KB). Estado: los 3 builds van SIN tutorial ahora (expo) porque `Assets/Resources/ExpoBuild.txt` sigue presente — tras la expo borrarlo para volver al tutorial. `Builds/.buildnumber` quedó en 8. Roslyn = 0 errores.

## Feature: Optimización de Texturas + Fixes Art/UI (075–081)

> 2026-09-04 → 2026-09-10 — Reducción del tamaño WebGL/APK, fixes de arte (pivots, sprites rotos, iconos), escala ninja, botones ranked.

| #   | ID                        | Tarea                                                     | Estado |
| --- | ------------------------- | --------------------------------------------------------- | ------ |
| 75  | 075-optimizacion-texturas | Reducir .data.br WebGL (140 MB) y APK (179 MB)           | in-progress |
| 76  | 076-powerup-ranked-fixes  | Iconos power-ups Single, efectos 2048 sin crunch, ranked button sprite | in-progress |
| 77  | 077-iconos-robusto        | Fallback Load<Texture2D>+Sprite.Create en LoadIcon()      | in-progress |
| 78  | 078-iconos-trompeta-ninja | Pivot real de sprites, trompeta reposo, NinjaMoveBack Single | in-progress |
| 79  | 079-fix-pivots            | NormalizedPivot() en todos los offsets                     | in-progress |
| 80  | 080-reporte-usuario       | Trompeta, ENEMY TURN, EnemyBanner NRE, punio 1, NinjaMoveBack 1 | in-progress |
| 81  | 081-ninja-ranked-scale    | Escala ninja nigromante 0.14 + botones ranked separados    | in-progress |

### Detalle 075

- **Optimización de texturas** — 391 PNG en `Resources/` con `maxTextureSize: 2048` y `crunchedCompression: 0`; fondos 2816×1536 (~8 MB c/u). Nuevo `Assets/Editor/TextureOptimizer.cs` (`Optimize/Apply Texture Optimization`): `textureCompression=1` + `crunchedCompression=1` (LZMA), `maxTextureSize` por clase (fondos/UI 1024, piezas 512, decor 512, Win/Tutorial 1024), redimensión física a 1920×1080 de fondos gigantes, eliminados 12 PNG legacy de `Assets/Prefabs/Pieces/`. Audio (~21 MB) queda fuera (requiere ffmpeg). Meta `.data.br` ≤ 50 MB.

### Detalle 076

- **Iconos power-ups rotos** — los 5 íconos (`Shake`, `Explosion`, `Fireball`, `Lightning`, `cambio`) pasados a `spriteMode: 1` (Single). Efectos (`punio`, `ritual`, `fuego1/2`, `rayo1/2`, `mago*`, `trumpet`) a `maxTextureSize: 2048` + `crunchedCompression: 0` (075 los había degradado). `punio.png` a Single (slice > PNG). Escalas Nigromantes ajustadas. Botón RANKED usa `Sprites/Menu/ranked` (+`ranked_0`). Alineación ranked en GameOverUI.

### Detalle 077

- **LoadIcon() robusto** — fallback `Resources.Load<Texture2D>` + `Sprite.Create` (rect completo, pivot centro) cuando Load/LoadAll no devuelven sprite; los 5 metas `Icon/*` con `isReadable: 1`.

### Detalle 078

- **Pivots** — `Sprite.pivot` viene en píxeles; offset corregido con pivot real en `SpawnSpecificAt`. Trompeta reposo `(802,-380)`. `NinjaMoveBack.PNG` (slice 1163×750 > PNG) → Single + isReadable. Escala ninja nigromante moviéndose `(0.44,0.37)`→`(0.7,0.7)`.

### Detalle 079

- **NormalizedPivot(s)** (`pivot.x/rect.width`) + `IconLocalPos(s, sizeX, sizeY, scale)` en `PowerUpManager`; aplicado a `SpawnSpecificAt`, `SpawnOnBoard`, `AnimateSpawned`, `ShakeWithFist`, `MageAndProjectile`, `MageLightning`, `SpawnRitualCircle`, `TutorialManager`. Con pivot centrado el offset normalizado da (0,0).

### Detalle 080

- **Reporte usuario** — (1) Trompeta reposo `(802,-380)`→`(812,-380)`. (2) "ENEMY TURN" (lvl 22): `TurnManager.ShowEnemyTurnBanner` sin `FindFirstObjectByType<Canvas>`, sortingOrder 200→220, delay 2.5s→1.5s. (3) `EnemyBanner`: canvas propio si no hay (ScreenSpaceOverlay 219 + CanvasScaler 1920×1080). (4) Puño: `LoadBestSprite(basePath)` prueba `path` y `path + " 1"` (mayor área) → usa `punio 1` subido por el usuario. (5) Ninja nigromante: `LoadSprites` itera candidatos `{name}`, `{name} 1` por carpeta con mayor área → usa `NinjaMoveBack 1`.

### Detalle 081

- **Escala ninja nigromante** — `BoardManager.cs:1466` escala de movimiento `(0.7,0.7)`→`(0.14,0.14)` (solicitud del usuario).
- **GameOverUI ranked botones** — victoria y derrota tenían botones encimados (~275px de solape) y sprites cruzados (derrota: sprite retry→acción menú, sprite quit→acción retry). Ahora: Retry (Retry_0) `x=-170`→`PlayRanked`, Menu `x=170`→`MainMenuScene`. Victoria: Menu usa `quitSprite` (antes flecha `nextSprite` engañosa). Derrota: Menu usa `quitSprite`. Gap 102px entre centros.

## Feature: Feedback Usuario (082)

> 2026-09-11 — Ronda de feedback del usuario tras probar la build: separación de botones de derrota, doble-tap para saltar el conteo, cartel RANKED una sola vez, Next en campaña completa abre el mapa. Verificación pendiente en Unity (el usuario testea mañana).

| #   | ID                 | Tarea                                                     | Estado      |
| --- | ------------------ | --------------------------------------------------------- | ----------- |
| 82  | 082-feedback-user  | Botones derrota, doble-tap scoreboard, RANKED una vez, Next→mapa | in-progress |

### Detalle 082

- **Botones de derrota más separados (tocaban en los extremos)** — en la derrota no-ranked el Retry (260×90 a escala 1.7 → 442px visuales) y el Quit (120×42 a 1.7 → 204px) se tocaban EXACTO en x=54 (right edge del retry -167+221=54, left edge del quit 156-102=54). Movidos: Retry `x=-167`→`-280`, Quit `x=156`→`260`. Ranked: Retry `-170`→`-280`, Menu `170`→`280`. (`GameOverUI.cs:367-395`)
- **Doble-tap en el conteo del scoreboard** — `ScoreboardUI.Update()`: dos toques/clics dentro de `DOUBLE_TAP_WINDOW` (0.45s, `Time.unscaledTime`) → `skipRequested = true` (mismo mecanismo que el botón SKIP >>, verifica touch+mouse). Al salir de la ventana `lastTapTime` vuelve a -1 para evitar acumular taps.
- **Cartel "RANKED UNLOCKED!" una sola vez** — aparecía tras ganar CUALQUIER partida de campaña con la campaña al 100% (replays → `GetNextUncompletedCupLevel()` = -1 → rama CAMPAIGN COMPLETE → `RankedUnlockSequence`). Nuevo flag `PlayerPrefs.RankedUnlockPopupShown` (se marca y guarda antes de mostrarlo); el cartel solo se muestra la 1ª vez. (`GameOverUI.cs:299-308`)
- **Next en campaña completa ya no sale al menú** — el botón flecha (`nextSprite`) hacía `LoadScene(MainMenuScene)` → "si toco next me saca a menu principal". Ahora `OpenCampaignMap(-1)` (mapa para rejugar niveles con estrellas y botón BACK→menú). Quit (quitSprite pequeño) sigue saliendo al menú. (`GameOverUI.cs:294-296`)
- **Validación** — `dotnet build Assembly-CSharp.csproj` = 0 errores (1 warning pre-existente `ScoreboardUI.showCampaignRewards` sin usar).

## Fix: Nigromantes front Move sprites (084)

> 2026-09-14 — El usuario agregó sprites de movimiento FRONT para Nigromantes (`PeonNecroMoveFront`, `caballerofrontnigro`, `PaladinMoveFrontNigro2`). El código no los encontraba: `LoadSprites` sigue la convención `{Nombre}MoveFront` (ej. `CaballeroMoveFront`) y caía al fallback Human. Añadidos como candidatos por carpeta Nigromantes.

| #   | ID                 | Tarea                                                     | Estado      |
| --- | ------------------ | --------------------------------------------------------- | ----------- |
| 84  | 084-nigro-move-front | Wiring de sprites front Move de Nigromantes en LoadSprites | done        |

### Detalle 084

- **Contexto** — al mover "front" (`toRow > fromRow`), una pieza roja Nigromantes cargaba `Sprites/Nigromantes/Pieces/{Nombre}MoveFront`, que NO existía para Peon/Caballero/Paladin → caía a `Sprites/Human/Pieces/{Nombre}MoveFront` (raza equivocada). El `NinjaMoveFront.PNG` ya existía y se cargaba bien (misma convención).
- **Archivos nuevos** (`Sprites/Nigromantes/Pieces/`, import Multiple): `PeonNecroMoveFront.png` (1 slice 80×557), `caballerofrontnigro.png` (1 slice 222×731), `PaladinMoveFrontNigro2.png` (4 slices ~90×126 c/u → animable).
- **Fix** — `BoardManager.LoadSprites()`: mapeo `nigroAlias` para `suffix=="Move" && front` (`Peon→PeonNecroMoveFront`, `Knight→caballerofrontnigro`, `Paladin→PaladinMoveFrontNigro2`) añadido como candidatos extra (con variante ` 1`) cuando `SpriteFolder(t) == "Nigromantes"`. El ciclo de temas ya probaba `primaryFolder` (Nigromantes cuando `scenarioTheme="NewRace"`), así que los encuentra antes del fallback Human. Si faltaran, el fallback Human sigue intacto.
- **Comportamiento resultante** — Peon/Caballero/Ninja: 1 frame → sprite estático de movimiento (sin animación, `PlayAnimation` exige ≥3 frames); Paladin: 4 frames → animación de caminata. La compensación de escala por área de Nigromantes en `AnimatedMove` aplica igual (cálculo idle/move real de sus propios sprites).
- **Validación** — `dotnet build Assembly-CSharp.csproj` = 0 errores.

## Fix: Ninja Idle Scale (083)

> 2026-09-14 — Bug de escala de la ninja cerrado. El diccionario `originalScales` era código muerto que podía dejar la base de la pose mal capturada; se elimina y `SetAttackPose` usa `GetIdleScale` como fuente de verdad única.

| #   | ID                 | Tarea                                                     | Estado      |
| --- | ------------------ | --------------------------------------------------------- | ----------- |
| 83  | 083-ninja-idle-scale | Eliminar `originalScales` muerto, pose base vía `GetIdleScale` | done        |

### Detalle 083

- **Causa raíz ya eliminada** — el bug descrito en AGENTS.md (key `GameObject.GetInstanceID()` en `AnimatedMove` vs `SpriteRenderer.GetInstanceID()` en `ResetPieceSprite`) vivía en el código de sprites de ataque dentro de `AnimatedMove`, que fue REEMPLAZADO por el sistema de nube de pelea (057/058, commit 8efd1d0). Verificado por git history: el store se eliminó y quedó `SoundManager.Instance.PlaySwordClash()` en su lugar.
- **`originalScales` era código muerto** — en el código actual se poblaba en `SetAttackPose` (key `sr.GetInstanceID()`) pero `ResetPieceSprite` NUNCA lo leía (solo `Remove`); la restauración de escala iba siempre por `GetIdleScale(type, pieceSpecies)`. El diccionario solo podía inyectar una base de pose incorrecta si la pieza no estaba en idle al seleccionarse (p.ej. ninja nigromante tras movimiento con escala `(0.14,0.14,1)`).
- **Fix** — `BoardManager.cs`: eliminado el campo `originalScales`, el store/`Remove` y las lecturas. `SetAttackPose` ahora calcula `idleScale = GetIdleScale(data.type, poseSpecies)` como base de la pose (caso especial Orc Paladín usa `idleScale.z`). `originalPositions` queda intacto (restauración de la posición tras el yOffset de pose). Comportamiento idéntico cuando la pieza está en idle (caso normal); cierra el patrón frágil para siempre.
- **Validación** — `dotnet build Assembly-CSharp.csproj` = 0 errores (1 warning pre-existente `ScoreboardUI.showCampaignRewards` sin usar).


