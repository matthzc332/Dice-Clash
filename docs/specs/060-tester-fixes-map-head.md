# SPEC-060: Tester Feedback Round 3 + Flujo de Cabeza del Mapa

- ID: `060-tester-fixes-map-head`
- Rama: `spec/060-tester-fixes-map-head`
- Estado: `in-progress`
- Autor: ox-alpha
- Fecha: 2026-08-24

## 1. Contexto

Feedback del tester sobre el build EXE anterior (los audios son de un exe viejo; filtrar qué sigue vigente). Además, al ganar una partida de campaña y abrir el mapa con "Next", la pantalla de victoria queda de fondo con su música y la cabeza del jugador salta entre páginas del mapa.

## 2. Objetivo

Pulir los detalles de UI reportados y que la transición victoria → mapa de campaña sea limpia (sin restos del GameOverUI ni música) y con un recorrido de cabeza simple y legible.

## 3. Alcance

### Incluido

- Quitar el texto "OK" duplicado sobre el sprite `botonOK_0` en `CampaignRewardUI` y `GoblinDialogue`.
- Bajar el título "BATTLE OVER" dentro de su panel en `ScoreboardUI`.
- Número de resultado grande semitransparente detrás de cada par de dados en `BattleResultUI` durante el clash.
- Estado GRANDE del diálogo del tutorial más chico y más abajo (`TutorialManager`). El estado chico (abajo a la izquierda) queda como está.
- `GameOverUI.Dismiss()`: al abrir mapa/modo selección desde GameOverUI se limpian partículas/botones/overlay y se corta la música win/lose.
- Cabeza del mapa: abrir directo en la página del último nivel jugado, FX de conquista ahí, luego un único avance hacia el siguiente nivel (máx. 1 cambio de página, sin saltos atrás).
- Botón en tarjetas completadas del mapa para rejugar niveles viejos.

### Excluido

- Rey Cabezón (SPEC-061).
- Re-verificación completa del flujo post-victoria trabado del tester (probablemente ya resuelto por Dismiss(); verificar jugando).

## 4. Criterios de Aceptación

- [ ] En CampaignRewardUI y GoblinDialogue el botón muestra solo el sprite `botonOK_0` sin texto superpuesto.
- [ ] "BATTLE OVER" está ~12px más abajo dentro del panel FinBatallaPanel.
- [ ] Durante el clash de dados se ve un número grande detrás de cada par de dados; los textos chicos ATK/DEF siguen existiendo.
- [ ] El diálogo grande del tutorial no tapa el centro del tablero (más chico y bajado); el chico no cambió.
- [ ] Tras ganar en campaña: Next abre el mapa sin overlay de victoria detrás y sin música win/lose sonando.
- [ ] La cabeza aparece sobre el último nivel jugado, hace FX de conquista y camina una sola vez al siguiente.
- [ ] Las tarjetas de niveles completados abren el selector de modo para rejugar.

## 5. UX/UI (si aplica)

- Pantallas impactadas: recompensa de campaña, diálogo goblin, scoreboard, resultado de batalla, tutorial, mapa de campaña.
- Estados: victoria/derrota, conquista animada, replay de nivel.

## 6. Diseño Técnico

- Archivos: `CampaignRewardUI.cs`, `GoblinDialogue.cs`, `ScoreboardUI.cs`, `BattleResultUI.cs`, `TutorialManager.cs`, `GameOverUI.cs`, `CampaignMapUI.cs`.
- TutorialManager: extraer campos `bigPos`/`bigScale` (paralelo a `smallPos`/`smallScale`) y usarlos en `MovePanelBig()` y `WelcomeCoroutine()` (3 puntos hardcodeados con `(0,-120)` / escala `2.4`).
- BattleResultUI: Text grande (fontSize ~70, alpha ~0.45) agregado antes de los dados en jerarquía para quedar detrás.
- CampaignMapUI: `ShowInternal` usa página de `GameConfig.selectedLevel`; `ConquestAndTravel` arranca en ese nivel (no en `GetPreviousCompletedBefore`).

## 7. Dependencias

### Con otros specs

- Ninguna.

### Impacto en documentación

- [x] `AGENTS.md` (al cerrar)

## 8. Plan de Implementación

1. Fixes UI puntuales (OK, BATTLE OVER, dados, diálogo).
2. `Dismiss()` + llamadas en `OpenCampaignMap` / `ShowCampaignModeSelection`.
3. Rework `ConquestAndTravel` + botón en tarjetas completadas.

## 9. Plan de Validación

- Compilar en Unity (Ctrl+R / auto).
- Manual: ganar nivel de campaña → verificar mapa limpio + cabeza; rejugar nivel completado; tutorial completo; combate con dados grandes.

## 10. Rollback

Revertir commits de la rama `spec/060-tester-fixes-map-head`.

## 11. Notas

- Los audios del tester son del exe anterior: ítem "quedó trabado tras ganar" se marca para re-verificación, no para fix.
