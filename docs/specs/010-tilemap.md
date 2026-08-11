# SPEC-010: Migración a Tilemap

- ID: `010-tilemap`
- Rama: `spec/010-tilemap`
- Estado: `in-progress`
- Autor: —
- Fecha: 2026-06-08

## 1. Contexto

El tablero se renderiza actualmente con 49 GameObjects `Tile` individuales, cada uno con un `SpriteRenderer` y `BoxCollider2D`. Esto es ineficiente y ensucia la jerarquía. Además, `Sprite.Create(texture, ...)` requiere Read/Write Enabled en las texturas, causando que los tiles queden invisibles (el sprite se crea como null).

Durante la revisión senior se identificó esto como un riesgo de performance y mantenibilidad.

## 2. Objetivo

Reemplazar los 49 GameObjects de tile por un único `Tilemap` de Unity, manteniendo la lógica de juego (grid, piezas, colisiones) intacta.

## 3. Alcance

### Incluido

- Creación procedural de `Grid` + `Tilemap` + `TilemapRenderer` en `BoardManager.Start()`
- Pintado del patrón de ajedrez con sprites `piso1_0` (claro) y `piso2_0` (oscuro)
- `Tile.cs` → `Cell.cs` como clase plana (no MonoBehaviour)
- Conversión de coordenadas: `CellToWorld()` / `WorldToCell()` en BoardManager
- Highlight de selección como único GameObject móvil
- Detección de clicks por coordenadas (sin `Physics2D.Raycast` sobre tiles)
- Eliminación de `tilePrefab`, `CreateTilePlaceholder`, `selectionHighlight` por tile

### Excluido

- Migración a Tilemap Collider (se mantiene detección por coordenadas)
- Animación de tiles (cambio de color, flash, etc.)
- Migración del piso en escenas existentes (se crea proceduralmente siempre)

## 4. Criterios de Aceptación

- [ ] El tablero muestra el patrón de ajedrez con `piso1`/`piso2` en la ventana Game
- [ ] Las piezas se posicionan correctamente sobre sus celdas
- [ ] El click selecciona la celda correcta
- [ ] Los marcadores de movimiento aparecen en las celdas válidas
- [ ] El highlight de selección se muestra en la celda seleccionada
- [ ] No hay GameObjects `Tile_*` en la jerarquía (solo `TilemapGrid/FloorTilemap`)
- [ ] El combate y habilidades (Flank, Charge, Aura) funcionan correctamente
- [ ] El build compila sin errores

## 5. UX/UI

- Pantallas impactadas: Game (tablero)
- Estados: el tablero se ve como antes pero con sprites de piso en vez de grises semitransparentes
- Sin cambios en accesibilidad

## 6. Diseño Técnico

### Archivos modificados

| Archivo                         | Cambio                                                              |
| ------------------------------- | ------------------------------------------------------------------- |
| `Assets/Scripts/Game/Cell.cs`   | Creación: clase plana (no MonoBehaviour) reemplazando Tile.cs       |
| `Assets/Scripts/Game/Tile.cs`   | Eliminación                                                         |
| `Assets/Scripts/Game/BoardManager.cs` | Tilemap en Start, CellToWorld/WorldToCell, selección unificada |
| `Assets/Scripts/Game/InputManager.cs` | Click/hover por coordenadas, Cell en vez de Tile              |
| `Assets/Scripts/Game/CombatManager.cs` | Cell en vez de Tile, GetCell en vez de GetTileWorld           |

### Estrategia

1. `BoardManager.Start()` crea Grid + Tilemap + TilemapRenderer
2. Carga `piso1_0` y `piso2_0` desde Resources y los asigna a `UnityEngine.Tilemaps.Tile` ScriptableObjects
3. Pinta 49 celdas con `tilemap.SetTile()` y escala vía `SetTransformMatrix`
4. `InitializeGrid()` crea solo objetos `Cell` ligeros (sin GameObjects)
5. Clicks: `InputManager` llama a `board.WorldToCell(mousePos)` → coordenadas (row, col)
6. `CellToWorld()` se usa para posicionar piezas y marcadores

### Riesgos técnicos

- `Resources.LoadAll<Sprite>()` puede fallar si los archivos no existen → fallback a cuadrado gris procedural
- `SetTransformMatrix` por celda agrega 49 matrices → overhead mínimo
- La físicas previas (BoxCollider2D en tiles) se eliminan; el input depende exclusivamente de `WorldToCell`

## 7. Dependencias

### Con otros specs

- Depende de SPEC-009 (sprites) porque usa los sprites de piso de Resources.
- Ningún spec futuro depende de este.

### Impacto en documentación

- [ ] `docs/data-model/ERD.md` o `CLASS-DIAGRAM.md`
- [ ] `docs/architecture/BACKEND-ARCHITECTURE.md`
- [ ] `docs/architecture/FRONTEND-ARCHITECTURE.md`
- [x] `docs/WORKLOG.md` — agregar entrada 010

## 8. Plan de Implementación

1. ⬜ Crear `Cell.cs` como clase plana (COMPLETADO)
2. ⬜ Refactorizar `BoardManager.cs` con Tilemap y CellToWorld (COMPLETADO)
3. ⬜ Refactorizar `InputManager.cs` con WorldToCell (COMPLETADO)
4. ⬜ Refactorizar `CombatManager.cs` con Cell (COMPLETADO)
5. ⬜ Verificar build en Unity

## 9. Plan de Validación

- Comandos: `Ctrl+B` en Unity (Build Player)
- Tests manuales: abrir escena, verificar piso visible, seleccionar pieza, mover, combatir, confirmar victoria
- Evidencia: captura de Game view mostrando el piso con sprites de ajedrez

## 10. Rollback

Revertir cambios en los 5 archivos involucrados (Cell.cs, BoardManager.cs, InputManager.cs, CombatManager.cs, Tile.cs eliminado). Restaurar Tile.cs desde git.

## 11. Notas

- La implementación ya está completa en el momento de escribir este spec, a modo de documentación.
- El cambio se ha hecho sin rama porque el proyecto no tiene git.
