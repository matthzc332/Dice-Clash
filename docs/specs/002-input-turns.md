# SPEC-002: Input del Jugador y Máquina de Turnos

- ID: `002-input-turns`
- Rama: `spec/002-input-turns`
- Estado: `next`
- Autor: IA
- Fecha: 2026-06-05

## 1. Contexto

Una vez que el tablero y las fichas existen (SPEC-001), el jugador necesita poder interactuar: seleccionar sus fichas, ver movimientos válidos, y ejecutar turnos. Sin input ni turnos, el juego no es jugable.

## 2. Objetivo

Implementar detección de clicks/raycasts sobre casillas y piezas, y una máquina de estados de turnos que restrinja acciones solo al jugador activo.

## 3. Alcance

### Incluido

- InputManager que detecte clicks sobre el tablero (Physics2D.Raycast o IPointerClickHandler)
- Detección de qué casilla se clickeó (mapeo a coordenadas [row, col] de la matriz)
- Resaltado visual de la ficha seleccionada
- Visualización de casillas destino válidas para la ficha seleccionada
- Máquina de estados de turnos: BlueTurn ↔ RedTurn
- Bloqueo de interacción para el jugador que no tiene el turno
- Indicador visual de qué turno está activo (texto UI o color de fondo)

### Excluido

- Movimiento físico de fichas (se hará en spec posterior de vectores de movimiento)
- Lógica de combate o dados
- Animaciones de transición entre turnos
- Temporizador de turno

## 4. Criterios de Aceptación

- [ ] Clickear una ficha propia la selecciona y se resalta visualmente
- [ ] Clickear una ficha enemiga no hace nada (no es tu turno o no es seleccionable)
- [ ] Clickear en el vacío deselecciona la ficha actual
- [ ] La máquina de estados alterna entre BlueTurn y RedTurn
- [ ] Al terminar el turno azul, pasa a turno rojo y viceversa
- [ ] No se puede seleccionar ni mover fichas del equipo que no tiene el turno
- [ ] Hay un indicador visual de qué equipo tiene el turno activo
- [ ] El proyecto compila sin errores

## 5. UX/UI (si aplica)

- Ficha seleccionada: borde o overlay brillante (ej: color amarillo/dorado)
- Casillas destino válidas: puntos o círculos semitransparentes verdes
- Indicador de turno: texto tipo "BLUE TURN" / "RED TURN" con color de fondo
- Click en celda vacía o ficha enemiga: sin respuesta (o feedback sutil)

## 6. Diseño Técnico

### Archivos a crear/modificar

- `Assets/Scripts/Game/InputManager.cs` — Detección de clicks y selección
- `Assets/Scripts/Game/TurnManager.cs` — Máquina de estados de turnos
- `Assets/Scripts/Game/GameState.cs` — Estado global del juego (opcional)
- Modificar `Assets/Scripts/Game/Tile.cs` — Agregar métodos de selección/resaltado
- Modificar `Assets/Scripts/Game/BoardManager.cs` — Agregar referencia a TurnManager y consulta de piezas propias

### Estrategia

1. Usar `IPointerClickHandler` en los GameObjects de casillas y fichas (o Physics2D.Raycast desde InputManager)
2. `TurnManager` con enum de estados (BlueTurn, RedTurn) y métodos `EndTurn()`, `GetCurrentTeam()`
3. `InputManager` consulta a `TurnManager` si la ficha clickeada pertenece al equipo activo
4. Resaltado visual mediante cambio de color/sprite o instanciación de overlay
5. Botón "End Turn" o detección automática cuando se completa una acción

### Nuevas dependencias

No. El Input System package ya está instalado en el proyecto.

### Riesgos técnicos

- Coordenadas de click a matriz: asegurar mapeo correcto entre posición mundial y [row, col]
- EventSystem y Physics2DRaycaster deben estar configurados en la escena

## 7. Dependencias

### Con otros specs

- Depende de **SPEC-001 (001-board-setup)**: necesita la matriz [7][7] y las fichas instanciadas
- Estado de 001: `next` (debe completarse antes)

### Impacto en documentación

- [ ] `docs/data-model/ERD.md` o `CLASS-DIAGRAM.md`
- [ ] `docs/architecture/BACKEND-ARCHITECTURE.md`
- [ ] `docs/architecture/FRONTEND-ARCHITECTURE.md`
- [ ] `AGENTS.md`

## 8. Plan de Implementación

1. Crear `TurnManager` con enum de estados y control de turno
2. Crear `InputManager` con detección de clicks sobre el tablero
3. Integrar selección de fichas (solo del equipo activo)
4. Implementar visualización de casillas destino válidas (placeholder)
5. Agregar indicador visual de turno activo (UI Text)
6. Integrar botón/lógica de finalización de turno

## 9. Plan de Validación

- Comandos: Build en Unity (Ctrl+B) — sin errores
- Tests manuales:
  - Click en ficha azul en turno azul → se selecciona
  - Click en ficha roja en turno azul → no pasa nada
  - Click en vacío → deselecciona
  - Terminar turno → indicador cambia a "RED TURN"
  - Click en ficha roja en turno rojo → se selecciona
  - Verificar que el ciclo continúa correctamente

## 10. Rollback

Eliminar InputManager.cs y TurnManager.cs. Revertir cambios en Tile.cs y BoardManager.cs.

## 11. Notas

- Para el resaltado visual inicial usar cambio de color del sprite o un GameObject hijo tipo "selection-ring"
- El botón "End Turn" puede ser un simple botón UI por ahora
