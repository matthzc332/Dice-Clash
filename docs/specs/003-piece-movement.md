# SPEC-003: Movimiento de Fichas

- ID: `003-piece-movement`
- Rama: `spec/003-piece-movement`
- Estado: `next`
- Autor: IA
- Fecha: 2026-06-05

## 1. Contexto

El tablero tiene fichas posicionadas (SPEC-001) y el jugador puede seleccionarlas viendo sus destinos válidos (SPEC-002). Falta la acción principal del juego: clickear un destino válido y que la ficha se mueva a esa casilla, respetando las reglas de captura y colisiones.

## 2. Objetivo

Implementar el movimiento físico de fichas: al clickear un destino válido, la pieza seleccionada se desplaza a la nueva casilla. Si el destino está ocupado por una ficha enemiga, se marca el conflicto para la futura resolución de combate.

## 3. Alcance

### Incluido

- Click en destino válido → la pieza se mueve a esa casilla
- Actualización de la matriz `grid[,]` al mover (Tile origen queda vacío, Tile destino se actualiza)
- La pieza visual se reposiciona suavemente en el nuevo tile
- Si el destino tiene ficha enemiga: la ficha atacante ocupa la casilla y la defensora queda marcada como "bajo ataque" (pendiente de resolución de combate)
- El turno termina automáticamente después de cada movimiento
- No se puede mover a una casilla ocupada por ficha aliada
- Después del movimiento, la selección se limpia

### Excluido

- Animación interpolada de movimiento (se hará en iteración de polish)
- Lógica de combate / dados (spec futuro)
- Efectos de sonido
- Reglas especiales de captura (solo ocupación directa)

## 4. Criterios de Aceptación

- [ ] Click en destino vacío válido → la pieza se mueve ahí y el tile origen queda vacío
- [ ] Click en destino con ficha enemiga → la pieza atacante ocupa el destino
- [ ] Click en destino con ficha aliada → no pasa nada (no se mueve)
- [ ] Después de mover, el turno cambia automáticamente al otro equipo
- [ ] La matriz `grid[,]` refleja correctamente el estado después del movimiento
- [ ] No se puede seleccionar ni mover después de mover (turno cambiado)
- [ ] El proyecto compila sin errores

## 5. UX/UI (si aplica)

- La ficha se reposiciona instantáneamente (por ahora sin animación)
- Si captura una ficha enemiga, la ficha defensora se marca (se puede ocultar o dejar para el spec de combate)

## 6. Diseño Técnico

### Archivos a modificar

- `Assets/Scripts/Game/InputManager.cs` — Agregar lógica de movimiento en `HandleClick`
- `Assets/Scripts/Game/BoardManager.cs` — Agregar método `MovePiece(from, to)`
- `Assets/Scripts/Game/Tile.cs` — Agregar método para liberar una ficha

### Estrategia

1. En `BoardManager`, crear `MovePiece(int fromRow, int fromCol, int toRow, int toCol)`:
   - Validar que el destino no sea aliado
   - Si hay enemigo: remover la ficha defensora (se marca como destruida)
   - Actualizar `tile.SetPiece()` / `tile.ClearPiece()`
   - Reposicionar el `pieceVisual`
2. En `InputManager.HandleClick()`, si el tile clickeado está en `validMoveTiles`:
   - Llamar a `board.MovePiece()`
   - Llamar a `turnManager.EndTurn()`
   - Limpiar selección

### Riesgos técnicos

- Asegurar que la matriz y los GameObjects se mantengan sincronizados
- La ficha defensora debe limpiarse correctamente sin dejar referencias huérfanas

## 7. Dependencias

### Con otros specs

- Depende de SPEC-001 (001-board-setup) ✅ done
- Depende de SPEC-002 (002-input-turns) ✅ done
- El spec de combate (004) depende de este

### Impacto en documentación

- [ ] `docs/data-model/ERD.md`
- [ ] `docs/architecture/`

## 8. Plan de Implementación

1. Agregar `MovePiece()` en `BoardManager.cs`
2. Modificar `InputManager.HandleClick()` para ejecutar movimiento al clickear destino válido
3. Agregar fin de turno automático post-movimiento
4. Probar flujo completo: seleccionar → mover → cambio de turno

## 9. Plan de Validación

- Test manual:
  - Seleccionar ficha → ver destinos → clickear destino vacío → ficha se mueve
  - Capturar ficha enemiga → atacante ocupa casilla
  - Verificar que el turno cambia después de cada movimiento
  - Verificar que después de mover no se puede mover otra ficha hasta el próximo turno

## 10. Rollback

Revertir cambios en `InputManager.cs` y `BoardManager.cs`. Volver al estado de SPEC-002.

## 11. Notas

- El movimiento instantáneo es intencional (spec de animaciones vendrá después)
- La ficha capturada se destruye (Destroy) por ahora; en el spec de combate se reemplazará con la resolución de dados
