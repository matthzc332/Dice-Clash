# SPEC-005: Condición de Victoria y Reinicio

- ID: `005-game-over`
- Rama: `spec/005-game-over`
- Estado: `in-progress`
- Autor: IA
- Fecha: 2026-06-08

## 1. Contexto

El juego ya permite movimiento y combate con dados (SPEC-004), pero no hay una condición de victoria. Las fichas pueden destruirse pero el juego nunca termina. Falta detectar cuándo un equipo pierde todas sus fichas, mostrar al ganador y permitir reiniciar.

## 2. Objetivo

Detectar victoria cuando todas las fichas de un equipo son destruidas, mostrar pantalla de Game Over con el ganador y botón de reinicio.

## 3. Alcance

### Incluido

- Al destruir una ficha, verificar si algún equipo se quedó sin fichas
- Si un equipo pierde todas sus fichas: mostrar pantalla de Game Over
- La pantalla muestra el equipo ganador (BLUE / RED)
- Botón "PLAY AGAIN" que reinicia la escena actual
- El juego se detiene (no se puede interactuar más) al mostrar Game Over

### Excluido

- Animaciones de victoria/derrota
- Pantalla de título o menú principal
- Música o efectos de sonido
- Historial de partidas

## 4. Criterios de Aceptación

- [ ] Al destruir la última ficha de un equipo, aparece la pantalla de Game Over
- [ ] La pantalla muestra el equipo ganador (azul o rojo)
- [ ] Click en "PLAY AGAIN" reinicia la partida
- [ ] Mientras Game Over está activo, no se puede interactuar con el tablero
- [ ] El proyecto compila sin errores

## 5. UX/UI

- Overlay semitransparente que cubre toda la pantalla
- Texto grande: "BLUE WINS!" o "RED WINS!"
- Botón "PLAY AGAIN" centrado debajo del texto

## 6. Diseño Técnico

### Archivos nuevos

- `Assets/Scripts/Game/GameOverUI.cs` — Overlay de victoria con botón de reinicio

### Archivos a modificar

- `Assets/Scripts/Game/BoardManager.cs` — Agregar `CheckVictory()` que recorre el grid
- `Assets/Scripts/Game/InputManager.cs` — Verificar victoria después de `MovePiece()`

### Estrategia

1. `BoardManager.CheckVictory()` recorre el grid. Si solo un team tiene fichas, devuelve ese team. Si ambos tienen fichas, devuelve null.
2. `InputManager.ExecuteMove()` llama a `CheckVictory()` después de `MovePiece()`. Si hay ganador, muestra GameOverUI y NO llama a `EndTurn()`.
3. `GameOverUI` overlay impide clicks (se renderiza encima con `GraphicRaycaster`).

### Riesgos técnicos

- El GraphicRaycaster del overlay bloquea inputs del tablero automáticamente
- Asegurar que el estado de la partida se reinicia correctamente (SceneManager.LoadScene)

## 7. Dependencias

### Con otros specs

- Depende de SPEC-004 (004-combat) ✅ done

### Impacto en documentación

- Ninguno

## 8. Plan de Implementación

1. Agregar `CheckVictory()` en `BoardManager.cs`
2. Crear `GameOverUI.cs` con overlay y botón de reinicio
3. Modificar `InputManager.ExecuteMove()` para verificar victoria

## 9. Plan de Validación

- Test manual:
  - Jugar hasta destruir todas las fichas enemigas → ver pantalla de Game Over
  - Click "PLAY AGAIN" → juego se reinicia
  - Verificar que no se puede interactuar durante Game Over

## 10. Rollback

Revertir cambios en `BoardManager.cs` y `InputManager.cs`. Eliminar `GameOverUI.cs`.

## 11. Notas

- El reinicio usa `SceneManager.LoadScene` con la escena activa actual
- El overlay tiene sortingOrder alto (200) para estar sobre todo
