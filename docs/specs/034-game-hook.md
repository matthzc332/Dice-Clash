# SPEC-034: Game Hook — primeros 3 minutos post-tutorial

- ID: `034-game-hook`
- Rama: `spec/034-game-hook`
- Estado: `in-progress`
- Autor: Matías Cabello
- Fecha: 2026-06-26

## 1. Contexto

Después del tutorial, el jugador inicia su primera partida real. Si los primeros minutos no generan engagement (feedback positivo, sensación de progreso, curiosidad), el jugador abandona. La retención D1 depende críticamente de esta experiencia inicial. Se necesita diseñar los primeros ~3 minutos con eventos definidos para maximizar el enganche.

## 2. Objetivo

Configurar la primera partida post-tutorial para que el jugador experimente progresión clara, al menos una victoria en combate, y vea el scoreboard con la promesa del siguiente trofeo. Todo en los primeros 3 minutos de juego.

## 3. Alcance

### Incluido

- Primera partida contra IA con dificultad reducida y **enemigos especiales**:
  - **Esbirros** (nuevos): piezas débiles con 1 HP, fáciles de eliminar para dar sensación de poder
  - **Campeón Enemigo** (nuevo): pieza única con más HP que aparece hacia el final como mini-boss
  - Mezcla de piezas normales (Peon, Ninja, Caballero, Paladín) pero en cantidad reducida
- **Power-ups disponibles durante la partida** (mismos que en el tutorial):
  - **Sacudida**: Empuja todas las piezas enemigas 1 casilla
  - **Explosión**: Daño en área 3x3
  - **Bola de fuego** (Mago): Invoca mago que elimina al enemigo más cercano
  - **Rayos**: Golpea una fila completa de enemigos
- Los power-ups aparecen como paquetes aleatorios en casillas del tablero al inicio de ciertos turnos
- Eventos scriptados en los primeros turnos:
  - Turno 1: Aparece un power-up cerca del jugador con highlight
  - Turno 2-3: Combate favorable (enemigo esbirro débil para enseñar eliminación rápida)
  - Turno 4-5: Un enemigo se posiciona mal (deja un flanco abierto)
  - Turno 6+: Aparece el Campeón Enemigo + power-up de bola de fuego o rayo para derrotarlo
  - Si el jugador va perdiendo: spawn de power-up adicional de apoyo
- Feedback visual y sonoro reforzado en estas acciones (partículas, sonidos de "bien hecho")
- Texto de "¡PRIMER BLOQUEO!" o "¡PRIMER ELIMINACIÓN!" con celebración
- Al llegar al scoreboard: mensaje "¡SIGUIENTE TROFEO: ORCO!" con icono bloqueado
- Duración estimada: 3-5 minutos (partida acortada, 4 piezas por bando en vez de 8)

### Excluido

- Sistema de progresión completo (trofeos fragmentados, oro) — será spec separado
- Boosters o power-ups permanentes — solo evento puntual en primera partida
- SKDip para anuncios — no aplica en esta fase
- Personalización por plataforma (CrazyGames vs Android)

## 4. Criterios de Aceptación

- [ ] La primera partida dura entre 3 y 5 minutos
- [ ] El jugador experimenta al menos un combate ganado
- [ ] Aparece al menos un evento scriptado (enemigo mal posicionado o boost)
- [ ] Al finalizar, el scoreboard muestra el próximo trofeo bloqueado
- [ ] El jugador llega al GameOverUI sin frustración evidente
- [ ] La dificultad es notablemente más fácil que una partida normal

## 5. UX/UI

- **Pantallas impactadas**: BoardManager (spawn reducido), ScoreboardUI (texto "siguiente trofeo"), GameOverUI (celebración extra en primera victoria)
- **Estados**:
  - Inicio de partida hook (texto "¡TU PRIMERA BATALLA!")
  - Eventos especiales (notificación tipo toast con icono)
  - Scoreboard con indicación de progresión
- **Accesibilidad**: Notificaciones grandes con iconos, duración suficiente para leer

## 6. Diseño Técnico

- **Arquitectura**: El hook es la Shadow Phase del tutorial, no un modo separado. `TutorialManager` maneja ambas fases: maniquíes (fase 1) y sombras con power-ups (fase 2 = hook).
- **Archivos a modificar/crear**:
  - `Assets/Scripts/Game/TutorialManager.cs` — shadow phase con IA, power-ups, y transición a ScoreboardUI
  - `Assets/Scripts/Game/PowerUpManager.cs` — 4 power-ups (Shake, Explosion, Fireball, Lightning) reutilizados
  - `Assets/Scripts/Game/AIController.cs` — IA para sombras
  - `Assets/Scripts/Game/BoardManager.cs` — `SpawnShadowPieceAt()`, `isShadowPhase`, midground/darken scene
- **Nuevas dependencias**: No
- **Riesgos técnicos**: 
  - Asegurar que los power-ups no rompan el flujo de turnos de la IA
  - Null safety en corrutinas de animación de destrucción

## 7. Dependencias

### Con otros specs

- Depende de `033-tutorial` (el hook ocurre inmediatamente después del tutorial)
- Si el tutorial no está implementado, el hook puede probarse independientemente desde el menú

### Impacto en documentación

- [ ] `docs/data-model/ERD.md`
- [ ] `docs/architecture/`
- [x] `AGENTS.md`

## 8. Plan de Implementación

1. Agregar flag `isHookGame` y `piecesPerSide` a `GameConfig`
2. Modificar `BoardManager.SetupInitialBoard()` para spawnear 4 piezas por bando + esbirros + campeón si `isHookGame`
3. Extender `PowerUpManager` para spawn de power-ups en casillas del tablero al inicio de turnos específicos
4. Crear `GameHookManager` con eventos por turno:
   - Turno 1: power-up cerca del jugador con highlight
   - Turno 2-3: esbirro débil aparece como target fácil
   - Turno 4-5: IA posiciona mal una pieza
   - Turno 6+: campeón enemigo + power-up fuerte para derrotarlo
   - Si el jugador pierde piezas: spawn de power-up adicional
5. Modificar `AIController` para modo fácil (50% de probabilidad de no atacar aunque pueda, mover en dirección menos óptima)
6. Agregar notificaciones toast para eventos especiales
7. Modificar `ScoreboardUI` para mostrar "¡SIGUIENTE TROFEO: [especie]!" si es hook
8. Modificar `GameOverUI` para celebración extra (más partículas, texto especial)
9. Probar flujo completo desde tutorial → hook → scoreboard → game over

## 9. Plan de Validación

- Comandos: `Ctrl+B` (Build Player) en Unity
- Tests manuales:
  - Jugar partida hook completa (asegurar duración 3-5 min)
  - Verificar eventos aparecen en turns correctos
  - Probar con diferentes acciones del jugador (los eventos no deben romperse)
  - Verificar que la IA modo fácil comete errores pero no se siente rota
  - Confirmar que scoreboard muestra próximo trofeo
- Evidencia esperada: Video de partida hook completa

## 10. Rollback

Revertir commits en la rama `spec/034-game-hook`.

## 11. Notas

- Decisión abierta: ¿el bonus de daño se aplica automáticamente o se muestra como "power-up" que el jugador activa?
- La duración de 3-5 minutos se logra con equipos de 4 piezas (vs 8 en partida normal) y IA menos agresiva.
- Este hook es crítico para la métrica de retención D1 — debe iterarse con testing de usuarios reales.
