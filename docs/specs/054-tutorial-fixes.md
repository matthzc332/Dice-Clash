# SPEC-054: Fixes de tutorial y pipeline de victoria

- ID: `054-tutorial-fixes`
- Estado: `in-progress`
- Fecha: `2026-08-10`

## 1. Contexto

El tester reportó que al terminar el tutorial se quedaba sin botones con un panel negro ("cartelito bonus") y que el caballero peleaba con un peón diminuto. Además, al probar el botón WIN de testeo no se otorgaba ninguna recompensa (ni cofre ni oro) a pesar de "ganar".

Análisis en código:

1. **TutorialRewardUI.cs** — el botón "OK!" tiene ancla `(0.5, 0)` con `anchoredPosition (0, -295)`: queda ~85px bajo la pantalla. El flujo se cuelga en `WaitUntil(() => clicked)` y el jugador no puede continuar.
2. **BoardManager.cs** — el guard de maniquí solo existe en `CreatePieceVisual`. Durante el combate en `AnimatedMove` el maniquí defensor se cambia a sprites reales (`PawnAttack` etc.) y `ResetPieceSprite` lo restaura como Peón real a escala 0.85 en vez del maniquí a 0.19.
3. **Daily bonus** — `GameManager` lo muestra en toda partida no-autoplay, incluido el tutorial. Debe vivir solo en el menú principal y solo después de que el jugador haya jugado el tutorial (completado O salteado).
4. **Botón WIN de testeo** — `TestButtons` llama `GameOverUI.Instance.Show(Team.Blue)` directamente, saltándose `ScoreboardUI.Show()`, que es donde se otorgan las recompensas reales (`CoinManager.RecordMatchWin()` + `CampaignManager.CompleteLevel()`). Por eso "ganar" no daba oro ni progreso.

## 2. Objetivo

Corregir los 4 problemas para que el flujo de tutorial termine correctamente, los maniquíes mantengan su sprite durante el combate, el daily bonus aparezca solo en el menú tras jugar el tutorial, y el botón WIN simule una victoria real con recompensas.

## 3. Alcance

### Incluido

1. **TutorialRewardUI.cs** — reposicionar el botón "OK!" a ancla centro `(0.5, 0.5)` con `anchoredPosition (0, -290)`.
2. **BoardManager.cs** — helpers `IsTutorialDummy(team)`, `GetTutorialDummySprite()`, `GetTutorialDummyScale()`; guards en `ResetPieceSprite` y `AnimatedMove` para no intercambiar sprites reales sobre maniquíes.
3. **Daily bonus al menú** — nuevo `TutorialProgress` (PlayerPrefs `TutorialPlayed`) marcado al completar o saltear el tutorial; quitar `DailyBonusUI` + `ShowDailyBonusDelayed` de `GameManager`; hook en `MainMenuManager.Start()` con gate `TutorialProgress.HasPlayed()`.
4. **ScoreboardUI con ganador forzado** — `Show(Team? forcedWinner = null)`; `TestButtons` WIN → `ScoreboardUI.Instance.Show(Team.Blue)`, LOSE → `ScoreboardUI.Instance.Show(Team.Red)`.

### Excluido

- Cambios al diseño de economía (oro solo en campaña, sin cambios).
- Cambios al contenido del tutorial (pasos, textos, sprites).
- Nuevo arte para cofres/recompensas.
- Build/entrega de EXE (se verifica compilación en Unity).

## 4. Criterios de aceptación

1. Al ganar el tutorial, el popup de recompensa (cofre +150 oro + coleccionables) se completa y el botón "OK!" es visible y clickeable; el flujo continúa a goblin → juego.
2. En la fase de maniquíes, los maniquíes mantienen sprite `muneco1` y escala 0.19 durante todo el combate (no se muestran como peones reales).
3. El daily bonus no aparece en ninguna partida (incluido el tutorial), solo en el menú principal y únicamente si el tutorial fue jugado (completado o salteado).
4. Presionar WIN en una partida de campaña completa el nivel (oro + listón + insignia) y registra la victoria de copa; LOSE no otorga recompensas.
