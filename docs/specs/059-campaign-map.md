# SPEC-059: Mapa de Campaña Paginado

- ID: `059-campaign-map`
- Rama: `spec/059-campaign-map`
- Estado: `in-progress`
- Autor: matth + ox-alpha
- Fecha: 2026-08-21

## 1. Contexto

El flujo post-tutorial entra directo a niveles sin un mapa que muestre progreso.
La copa del HUD ("W x/2" de `CoinManager`) quedó obsoleta: nadie la usa para
desbloquear nada y su juice (monedas volando) alimenta una métrica muerta.
Se necesita una pantalla de campaña con identidad: 4 mapas (uno por trofeo),
tarjetas de nivel como nodos, y recompensas visibles que motiven a rejugar.

## 2. Objetivo

Reemplazar `CampaignUI` y el prototipo de mapa por una pantalla paginada
horizontal de 4 mapas, con tarjetas enemigas (`panelCartaRed`), cabezas que
indican al enemigo, cabeza del jugador que viaja entre tarjetas, estrellas por
performance, y eliminación completa de la copa del HUD.

## 3. Alcance

### Incluido

- `CampaignMapUI` v2: paginación horizontal (swipe/flechas/dots), 1 página por copa.
- Tarjetas de nivel reusando diseño de `CampaignUI.CreateLevelCard` pero con `panelCartaRed`.
  Estados: bloqueada gris / disponible roja / conquistada dorada (sello insignia + check).
- Cabeza/icono de raza enemiga sobre cada tarjeta.
- Cabeza del jugador viaja a la siguiente tarjeta tras ganar; auto-flip de página.
- Fallback de fondos: `Sprites/CampaignMap/mapa{Race}` → fondo existente por raza → panel teñido.
- Estrellas por performance (piezas azules sobrevivientes): ≥4 = 3★, 2–3 = 2★, 1 = 1★.
  Persistencia por nivel (máximo histórico). Revelado en popup de victoria y visibles en mapa.
- Eliminar copa: `CoinManager.CreateCupUI`, `RecordMatchWin`, cupVictories, ramas legacy en `GameOverUI`.
- Monedas por kill vuelan al bolso de oro del HUD y otorgan oro real:
  Peon/Ninja +1g, Caballero +2g, Paladin +3g.

### Excluido

- Arte final de fondos (el usuario los está creando; se integra por convenio de nombres).
- Rebalanceo de economía de entrada de modos.
- Ranked mode (fuera de este spec).

## 4. Criterios de Aceptación

- [ ] Menú → CAMPAIGN abre el mapa paginado; flechas/swipe cambian de página; dots indican página.
- [ ] Cada página muestra su copa en header y sus 5 tarjetas sobre un camino.
- [ ] Tarjeta disponible abre tickets con NEXT ENEMY (nombre + icono de raza).
- [ ] Tras ganar: sello cae sobre la tarjeta conquistada, cabeza enemiga se desvanece, cabeza del jugador camina a la siguiente.
- [ ] Estrellas reveladas una por una en popup de victoria y persistidas; visibles bajo tarjetas conquistadas.
- [ ] No existe UI de copa "W x/2"; monedas por kill llegan al bolso y suman oro real.
- [ ] El proyecto compila en Unity sin errores.

## 5. UX/UI

- Pantallas: overlay fullscreen sobre canvas de menú o GameOverUI (sortingOrder alto propio).
- Estados por tarjeta: locked/unlocked/conquered. Páginas: 4, navegación envolvente no (tope en extremos).
- Sonidos: select en click, hammer en sello, coin al volar monedas, victory al completar copa.

## 6. Diseño Técnico

- Archivos: `CampaignMapUI.cs` (reescritura), `CoinManager.cs`, `ScoreboardUI.cs`,
  `GameOverUI.cs`, `MainMenuManager.cs`, `ModeSelectionUI.cs` (ya soporta levelId),
  `CampaignManager.cs` (stars), `EconomyManager.cs` (target del bolso para FX).
- Posiciones de tarjetas: array editable por página (`static Vector2[]`) — se ajustan junto con el usuario.
- Estrategia: eliminar código muerto primero, luego reescribir mapa, luego estrellas.

## 7. Dependencias

### Con otros specs

- Ninguna bloqueante (046 done).

### Impacto en documentación

- [x] `AGENTS.md`

## 8. Plan de Implementación

1. Limpieza copa + redirección de monedas al bolso.
2. CampaignMapUI v2 (paginación, tarjetas, cabezas, camino).
3. Estrellas (persistencia, cálculo, revelado, render).
4. Cableado de flujos (menú, Next post-victoria, tickets).

## 9. Plan de Validación

- Compilación en Unity 6000.0.71f1 sin errores.
- Manual: tutorial→sombras→goblin→tickets→L3→ganar→mapa→sello+viaje→tickets L4.
- Manual: menú CAMPAIGN → swipe 4 páginas → click tarjeta bloqueada (nada) vs disponible (tickets).

## 10. Rollback

Revertir commit del spec; `CoinManager` conserva AwardKill (única API usada por el resto del juego).

## 11. Notas

- Niveles 1–2 del JSON no pertenecen a ninguna copa: quedan fuera del mapa (solo copas, niveles 3–22).
- Umbrales de estrellas y oro por kill son ajustables tras playtest.

## 12. Tarea pendiente — Revisión (próxima sesión)

Implementación escrita, **sin verificar en Unity**. Checklist:

1. Compilar en Unity 6000.0.71f1 → corregir errores si los hay.
   - `CampaignMapUI.cs` reescrito completo (~1040 líneas): revisar APIs usadas vs reales
     (`CampaignDataWrapper`, `LoadFirstSprite` de fondos con `Resources.LoadAll`, switch expressions).
2. Flujo manual: tutorial→sombras→goblin→tickets L3→ganar→mapa:
   - Conquista: flash×2 + martillo sobre L3, cabeza enemiga sube/desvanece con humo,
     cabeza del jugador camina de L3 a L4 (con flip de página si corresponde).
3. Menú → CAMPAIGN: 4 páginas, swipe/flechas/dots, header muestra copa + progreso.
4. Tarjeta disponible → tickets con NEXT ENEMY → jugar → ganar → estrellas en popup.
5. Monedas por kill llegan al bolso (+Ng flotante); NO existe copa "W x/2".
6. Ajustar `cardPositions` (CampaignMapUI.cs:48) junto con el usuario.
7. Integrar fondos del usuario: `Assets/Resources/Sprites/CampaignMap/mapaHuman|Orc|Beastfolk|Nigromantes.png`.

Estado de archivos modificados (todos sin commit):
- `CampaignMapUI.cs` (reescrito v2) · `CoinManager.cs` (reescrito, sin copa)
- `ScoreboardUI.cs` (estrellas) · `CampaignRewardUI.cs` (revelado ★)
- `GameOverUI.cs` / `MainMenuManager.cs` / `ModeSelectionUI.cs` (cableado flujo)
- `EconomyManager.cs` (`AddGoldFromWorld`) · `CampaignManager.cs` (`SaveStars/GetStars/GetNextUncompletedCupLevel`)
