# SPEC-018: AI para fichas rojas

- ID: `018-red-ai`
- Rama: `spec/018-red-ai`
- Estado: `draft`
- Autor: —
- Fecha: 2026-06-15

## 1. Contexto

Actualmente el juego es hot-seat: el mismo jugador controla ambos bandos (Blue y Red) con clics. Para que sea un juego single-player, las fichas rojas deben moverse automáticamente con una IA básica.

## 2. Objetivo

Las fichas rojas juegan solas su turno: evaluar el tablero, elegir la mejor acción disponible (mover o atacar) para cada pieza, ejecutarla, y finalizar el turno.

## 3. Alcance

### Incluido

- Nuevo `AIController` que se activa al comenzar `RedTurn`
- Escaneo de todas las fichas rojas en la grilla
- Para cada ficha: consultar movimientos válidos (`BoardManager.GetValidMoves`)
- Heurística simple de evaluación (priorizar kills > atacar > avanzar > mover aleatorio)
- Ejecución del movimiento/ataque mediante `BoardManager.MovePiece`
- Desactivar input del jugador durante el turno de la IA
- Finalizar el turno rojo automáticamente cuando todas las fichas hayan actuado

### Excluido

- Árbol de búsqueda (minimax, MCTS) o planning a futuro
- Evaluación posicional avanzada (control de casillas, formación)
- Dificultad configurable
- Aprendizaje automático
- IA para el bando azul

## 4. Criterios de Aceptación

- [ ] Al comenzar `RedTurn`, la IA mueve automáticamente todas las fichas rojas sin intervención del jugador
- [ ] La IA prioriza ataques que eliminan fichas enemigas sobre movimientos inofensivos
- [ ] No se puede hacer clic ni seleccionar fichas durante el turno de la IA (input deshabilitado)
- [ ] Al terminar todas las acciones, el turno cambia a `BlueTurn` automáticamente
- [ ] La IA nunca mueve a casillas ocupadas por aliados u obstáculos

## 5. UX/UI

- Durante el turno rojo: las fichas azules no son clicables (input deshabilitado)
- Las animaciones de movimiento/combate se reproducen normalmente
- No se muestra el panel de "END TURN" ni se requiere clic para avanzar
- Texto "ENEMY TURN" mientras la IA juega

## 6. Diseño Técnico

### Archivos a modificar/crear

| Archivo | Acción |
|---------|--------|
| `Assets/Scripts/Game/AIController.cs` | Crear — lógica de IA |
| `Assets/Scripts/Game/TurnManager.cs` | Modificar — disparar IA al iniciar RedTurn |
| `Assets/Scripts/Game/InputManager.cs` | Modificar — bloquear input durante turno de IA |
| `Assets/Scripts/Game/TurnUI.cs` | Modificar — ocultar botón "END TURN" en turno rojo |

### Estrategia de implementación

1. Crear `AIController` con método `PlayTurn()`:
   - Escanea la grilla buscando `team == Team.Red`
   - Para cada pieza llama `GetValidMoves(row, col, type)`
   - Evalúa cada movimiento con `EvaluateMove(from, to, piece)`
   - Ejecuta el mejor movimiento con `MovePiece`
   - Espera a que termine la corrutina de animación antes de la siguiente pieza
   - Al terminar todas, llama `TurnManager.EndTurn()`

2. Heurística `EvaluateMove`:
   - Si el movimiento es un ataque que mata (el enemigo tiene tier menor o igual): **+100**
   - Si es un ataque que no mata: **+50 + (tier Enemigo - tier Atacante) * 10**
   - Si es un avance hacia el lado enemigo (reducir distancia a filas 0-1): **+20**
   - Si es movimiento lateral/retroceso: **+5**
   - Si no hay movimientos válidos: saltar pieza

3. En `TurnManager`, al cambiar a `RedTurn`, si no es hot-seat, invocar `AIController.PlayTurn()`.

4. En `InputManager`, agregar flag `bool aiPlaying` que deshabilita `OnPieceClicked` y `OnTileClicked`.

### Nuevas dependencias

No.

### Riesgos técnicos

- Las corrutinas de animación (`AnimatedMove`, `AnimatedDiceRoll`) deben sincronizarse: la IA debe esperar a que termine cada una antes de evaluar la siguiente pieza.

## 7. Dependencias

### Con otros specs

Ninguna.

### Impacto en documentación

- [ ] `docs/data-model/ERD.md`
- [ ] `docs/architecture/`
- [x] `AGENTS.md`

## 8. Plan de Implementación

1. Crear `AIController.cs` con escaneo de piezas, evaluación y ejecución
2. Modificar `TurnManager.cs` para invocar IA en RedTurn
3. Modificar `InputManager.cs` para bloquear input durante IA
4. Modificar `TurnUI.cs` para ocultar END TURN en turno rojo
5. Probar partida completa vs IA

## 9. Plan de Validación

- Comandos: `Ctrl+B` (Build Player) en Unity
- Tests manuales:
  - Jugar partida completa como Azul vs IA Roja
  - Verificar que la IA ataca cuando tiene oportunidad
  - Verificar que la IA no se salta piezas con movimientos válidos
  - Verificar que el input del jugador está bloqueado durante el turno rojo
- Evidencia esperada: video/gif de partida completa contra IA

## 10. Rollback

Revertir commits en la rama `spec/018-red-ai`. Si ya se mergeó a `main`, revertir el merge.

## 11. Notas

- Decisión: la heurística se mantiene simple en esta iteración. Se puede complejizar después.
- La IA ejecuta una acción por pieza por turno (no múltiples acciones por pieza).
- El orden de actuación de las piezas rojas es: Paladín → Caballero → Ninja → Peón (priorizando las de mayor tier primero).
