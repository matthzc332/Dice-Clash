# Dice Clash Tactics — Game Design Document (Actualizado)

> **Versión:** 2.0 — Refleja el estado actual del proyecto (Julio 2026)
> **Target:** WebGL (CrazyGames) → Android
> **Stack:** Unity 6000.0.71f1 (URP 2D), C#, Unity Input System
> **Equipo:** 2 Programadores + 1 Artista + IA

---

## 1. Visión del Producto

**X-Statement:** Juego de táctica posicional en tablero 7×7 donde las fichas desatan duelos de dados (2d6 + modificador). El atacante tiene +1 de ventaja, pero el defensor puede ganar con un empate. Combina movimiento geométrico al estilo ajedrez con la emoción del azar controlado.

**Plataforma principal:** WebGL (CrazyGames)
**Métrica clave:** Retención D1 18-20%, sesión >7 min

---

## 2. Loop Principal

```
SELECCIONAR pieza → MOSTrar movimientos válidos → MOVER a casilla
  ↓
¿Casilla ocupada por enemigo?
  Sí → DUELO (2d6 + modificadores) → GANADOR ocupa casilla, PERDEDOR destruido
  No → Movimiento normal
  ↓
Finalizar turno → Turno oponente (IA o jugador)
```

---

## 3. Tablero

- **Tamaño:** 7×7 (filas 0-6, columnas 0-6)
- **Disposición simétrica espejo:**

```
        1   2   3   4   5   6   7
Fila 0:  C   N   P   P   P   N   C    ← AZUL (retaguardia)
Fila 1:  ·   p   p   p   p   p   ·    ← AZUL (peones)
Fila 2:  ·   ·   ·   ·   ·   ·   ·
Fila 3:  ·   ·   ·   ·   ·   ·   ·    ← Zona de combate
Fila 4:  ·   ·   ·   ·   ·   ·   ·
Fila 5:  ·   p   p   p   p   p   ·    ← ROJO (peones)
Fila 6:  C   N   P   P   P   N   C    ← ROJO (retaguardia)
```

- **Columnas 1 y 7** en filas de peones vacías para dar apertura a Caballeros
- **3 Escenarios:** Human (default), Orc, Beastfolk — cambian sprites, fondo y obstáculos
- **Obstáculos:** Gárgolas, barriles, árboles (sortingOrder -1, detrás de piezas)

---

## 4. Piezas y Stats

| Pieza | Tipo | Movimiento | Rango | DEF (2d6+N) | ATK (2d6+N) | Puntos | Escala Visual |
|-------|------|-----------|-------|------------|------------|--------|--------------|
| Peón | Pawn | 1 casilla cualquier dirección | 1 | +0 | +1 | 0 | 0.85 |
| Ninja | Ninja | 1-3 en diagonal | 3 | +1 | +1 | 1 | 0.37 |
| Caballero | Knight | 1-3 ortogonal (N/S/E/O) | 3 | +2 | +1 | 2 | 1.15 |
| Paladín | Paladin | 1-3 cualquier dirección | 3 | +3 | +1 | 3 | 0.267 (Orco) |

- **ATK siempre:** `2d6 + 1` (bonus ofensivo fijo)
- **DEF variable:** `2d6 + defBonus` (0/1/2/3 según tipo)
- **Empate:** Defensor gana (mitiga avance irresponsable)
- **Cada pieza tiene id único, team (Blue/Red), type, species**

---

## 5. Habilidades Especiales

| Habilidad | Pieza | Efecto | Condición |
|-----------|-------|--------|-----------|
| Flank | Ninja | ATK +6 | Enemigo aislado (sin aliados adyacentes) |
| Charge | Caballero | ATK +1 | Movimiento ≥ 2 casillas |
| Aura | Paladín | DEF +1 a aliados adyacentes | Ser vecino de un Paladín aliado |

---

## 6. Sistema de Turnos

- **BlueTurn → RedTurn → alternancia**
- Turno azul: timer de 20s por turno, advertencia a 5s
- Turno rojo: controlado por IA
- **Fin de turno automático** si timer expira o jugador hace clic en "Skip Turn"
- **Turnos de IA:** espera 2s antes de mover, bloquea input durante ejecución

---

## 7. IA (AIController)

- **Selección:** Escanea todas las piezas rojas movibles, elige una al azar
- **Evaluación de movimientos** (con ruido ±5):
  - Matar enemigo de Tier ≥ propio: +100
  - Matar enemigo de Tier > propio: +50 + (diff × 10)
  - Mover a power-up: +30
  - Avanzar (↓): +20
  - Lateral: +10
  - Retroceder (↑): +5
- **Un movimiento por turno** (no toda la IA en un turno)

---

## 8. Power-Ups

4 tipos con iconos visibles en el tablero (sortingOrder 14 icono, 13 glow):

| Power-Up | Color | Efecto |
|----------|-------|--------|
| SHAKE! | Verde | Reubica aleatoriamente todas las piezas enemigas |
| BOOM! | Naranja | Explosión 3×3, destruye enemigos en radio |
| FIREBALL! | Rojo | Proyectil al enemigo más cercano, lo destruye |
| LIGHTNING! | Amarillo | Rayo al enemigo más cercano, lo destruye |

- Aparecen aleatoriamente en casillas vacías durante la partida normal
- Timer de reaparición: 5-12s (4-9s si <6 piezas vivas)
- Animación de flotación + pulso + brillo
- Efectos visuales: cámara shake, partículas, mago invocador (Fireball/Lightning)
- Flag `IsExecuting` bloquea input del jugador durante 2s

---

## 9. Monedas y Copa

- **+10 monedas** por cada enemigo eliminado por equipo azul
- **Copa** en esquina superior derecha (791, 289) con fill vertical
- Partículas de monedas vuelan desde el punto de kill hacia la copa
- Al llenar la copa (100 monedas): desbloquea el siguiente trofeo/mundo
- Tutorial usa `copaTuto`

---

## 10. Progresión de Mundos

| Mundo | Especie | Escenario | Desbloqueo |
|-------|---------|-----------|------------|
| Human | Human | Human | Siempre disponible |
| Orc | Orc | Orc | Completar Human (copa llena) |
| Beastfolk | Beastfolk | Beastfolk | Completar Orc |

- **Trofeos:** 3 copas en el menú principal (estante), doradas si completadas, grises si no
- **Persistencia:** PlayerPrefs (`Trophy_X`, `Unlocked_X`)
- **Next button** en GameOver avanza Human→Orc→Beastfolk→MainMenu

---

## 11. Tutorial

Dos fases:
1. **Maniquíes:** 4 dummies de madera sin IA, el jugador aprende a seleccionar y atacar
2. **Shadow Phase:** Enemigos sombra con IA, power-ups reales, victoria→ScoreboardUI→GameOverUI

- Overlay con retrato y texto instructivo
- Botón "SALTAR" para jugadores experimentados
- Power-ups explicados al acercarse
- Sin timer, sin End Turn button, sin Scenario/Species buttons

---

## 12. Condiciones de Victoria

- **Aniquilación:** Un equipo se queda con 0 piezas → ScoreboardUI
- **Timer de partida:** 5 minutos → ScoreboardUI (cuenta puntos)
- **Empate en puntos:** Gana el azul (desempate)
- **Victoria tutorial:** ScoreboardUI → GameOverUI (solo botón "Next")

### ScoreboardUI
- Muestra puntaje acumulado por piezas vivas de cada equipo
- Icono grande de Paladín por equipo (230×230)
- Animación: cada pieza suma puntos con martillazo + hit effect
- Banner de VICTORIA/DERROTA con pulso/humo
- Transición a GameOverUI tras 2s

### GameOverUI
- Background de victoria (BlueWin) o derrota (RedWin)
- Botones: Next (victoria), Retry (ambos), Quit (derrota)
- Partículas: confeti + chispas + glow (victoria), cenizas + humo rojo (derrota)
- Música: win-lose.mp3 + fanfarria/defeat SFX
- Botón Next avanza mundo si copa llena

---

## 13. UI General

| Elemento | Canvas | Sorting Order |
|----------|--------|--------------|
| CharacterCardUI | CardCanvas | 95 |
| BattleResultUI | BattleCanvas | 88 |
| TurnUI (HUD) | TurnCanvas | 100 |
| CoinManager | CoinParticleCanvas | 99 |
| CoinCupPanel | (own canvas) | 50 |
| ScoreboardUI | ScoreboardCanvas | 199 |
| GameOverUI | GameOverCanvas | 200 |
| GameOver Particles | ParticleCanvas | 201 |
| Menu | MenuCanvas | 100 |

### HUD (TurnCanvas)
- InfoPanel: bandera del turno, texto "BLUE/RED TURN", timer de partida, timer de turno
- SkipTurnButton (visible solo en BlueTurn)
- SoundToggle (esquina superior derecha)
- QuitButton (solo PC, oculto en WebGL)
- ScenarioButton / SpeciesButton (debug, oculto en tutorial)
- Ctrl+R: reset completo

### CharacterCardUI
- Cartas 370×400, ancladas a (0, 1) del padre
- OwnCard en (20, -79), EnemyCard en (20, -450)
- Muestra: nombre, puntos (☆), criatura, grid de movimiento 3×3, ATK/DEF con dados
- ATK muestra "2d6+1", DEF muestra "2d6+N"

### BattleResultUI
- Panel de dados con 2 dados por lado (72×72 cada uno)
- Animación: dados giran y chocan, flash, regresan
- Textos "ATK N" / "DEF N" para cada equipo
- Banner "GANADOR: {type}" con pulsación
- Confeti (victoria) o humo (derrota)
- Click para cerrar

---

## 14. Combate (Fórmula Detallada)

```
atkTotal = (d6 + d6) + 1 + atkAbilityBonus
defTotal = (d6 + d6) + defBonus + defAbilityBonus

Resultado:
  atkTotal > defTotal → Atacante gana
  atkTotal <= defTotal → Defensor gana
```

- Tutorial: los maniquíes (Red) siempre pierden
- En tutorial shadow phase: combate normal

---

## 15. Audio

### SFX Procedurales
- `PlaySelect()` — tono de selección
- `PlayHit()` — golpe seco
- `PlayVictory()` — acorde C mayor + fanfarria ascendente
- `PlayDefeat()` — tono descendente
- `PlayDiceRoll()` — dice1.mp3 → dice2.mp3 secuencial
- `PlayMove()` — movimiento
- `PlayHammer()` — martillo para scoreboard
- `PlaySelectSound(species)` — yes.mp3 por raza (Human con skip, Orc truncado)

### Música
- `PlayMenuMusic()` — loop de menú
- `PlayGameplayMusic()` — loop de gameplay
- `PlayWinLoseMusic()` — `Sounds/Fondo/win-lose.mp3`
- `StopMusic()` — silencia y cancela colas

---

## 16. Partículas

Todas las partículas son UI Image–based coroutines (no ParticleSystem):

| Contexto | Tipo | Descripción |
|----------|------|-------------|
| Movimiento | MoveTrail | Estela de color del equipo |
| Combate | Confetti/Humo | Sobre panel de dados (BattleResultUI) |
| Kill | Emojis | Por raza (`emote{key}{emotion}_0`) |
| GameOver victoria | Confetti, chispas, glow | Bucles infinitos |
| GameOver derrota | Cenizas, humo rojo, partículas oscuras | Bucles infinitos |
| Scoreboard | Hammer Hit, Smoke Puff, Sparkles | Durante animación |
| Power-Up Shake | ShakePuff | Pequeñas bolitas verdes |
| Power-Up Explosion | ExplosionBurst | 20 partículas naranjas |
| Power-Up Fireball | FireTrail + FireImpact | Estela + impacto |
| Power-Up Lightning | LightningSeg + Sparks | Segmentos de rayo |

---

## 17. Timer de Partida

- **Duración:** 5 minutos (300s)
- **Visual:** Formato MM:SS en HUD, color dorado, parpadea rojo ≤60s
- **Al expirar:** llama `ScoreboardUI.Instance.Show()`
- **En tutorial:** desactivado

---

## 18. Menú Principal

- **Fondo:** Menu.png fullscreen
- **Título:** No hay título textual (sprite-based)
- **Estante + Trofeos:** 3 copas (Human, Orc, Beastfolk) con estado LOCKED/desbloqueado
- **Carrusel:** 3 slots con sprites de especie, flechas < > para navegar
- **PlayButton:** `botonplay2_0`, con aura pulsante azul
- **TutorialButton:** Botón "TUTORIAL" que inicia tutorial
- **Especies bloqueadas** no responden al clic

---

## 19. Esquema de Archivos

```
Assets/Scripts/Game/
├── PieceType.cs            — Enum (Pawn, Ninja, Knight, Paladin)
├── Team.cs                 — Enum (Blue, Red)
├── PieceData.cs            — Struct con id, team, type, species, Tier, defBonus, PointValue
├── GameConfig.cs           — Static: selectedSpecies, selectedScenario, isTutorial
├── BoardManager.cs         — Tablero 7×7, grid, movement, animations, victory check
├── InputManager.cs         — Input del jugador, selección, hover, movimiento
├── TurnManager.cs          — TurnState, timer de turno, EndTurn
├── AIController.cs         — IA para piezas rojas
├── CombatManager.cs        — Fórmula de combate, habilidades
├── GameManager.cs          — Bootstrap, creación de sistemas
├── PowerUpManager.cs       — 4 tipos de power-ups, spawn, ejecución, efectos
├── GameOverUI.cs           — Pantalla final, partículas, botones
├── ScoreboardUI.cs         — Scoreboard animado
├── BattleResultUI.cs       — Panel de duelo con dados
├── CharacterCardUI.cs      — Cartas de personaje
├── TurnUI.cs               — HUD del juego
├── MainMenuManager.cs      — Menú principal, carrusel, trofeos
├── TutorialManager.cs      — Tutorial con fases maniquí + sombra
├── CoinManager.cs          — Monedas, copa, persistencia
├── TimerManager.cs         — Timer de partida global
├── SoundManager.cs         — Audio procedural y música
└── TestButtons.cs          — Botones GANAR/PERDER para testing
```

---

## 20. Bugs Conocidos (No Arreglados)

1. **Ninja idle scale:** Después de atacar, la ninja se queda con escala 0.6 en vez de restaurar la original (~0.37/0.45). Causa: keyeado con `GameObject.GetInstanceID()` pero buscado con `SpriteRenderer.GetInstanceID()`.

2. **MissingReferenceException en power-ups:** `AnimateAttackLunge` sin null checks. Sombra destruida durante power-up → coroutine accede a `visual.transform`.

3. **Scoreboard win condition:** Si ambos equipos tienen 0 piezas (solo Peones, PointValue=0), declara ganador al azul por `blueTotal >= redTotal` (0 >= 0).

4. **Paladin attack animation:** `FilterSprites` elimina frame 2 (169×169) por umbral 1.5×, dejando solo 2 frames. `PlayAnimation` requiere ≥3 frames.

---

## 21. Cambios Respecto al GDD Original

| Original GDD | Implementación Actual |
|-------------|----------------------|
| D6 + modificador | 2d6 + modificador |
| Sin habilidades especiales | Flank (+6), Charge (+1), Aura (+1 DEF) |
| Sin power-ups | 4 power-ups con efectos completos |
| Sin tutorial | Tutorial con 2 fases + shadow phase |
| Sin scoreboard | ScoreboardUI con animación martillo |
| Sin timer de partida | Timer de 5 minutos |
| Sin monedas/copa | Sistema de monedas + copa + persistencia |
| Sin progresión | 3 mundos desbloqueables |
| Sin Game Hook | GameHookManager pendiente (spec 034) |
| Sin partículas UI | Sistema completo de partículas Image-based |
| IA simple | IA con heurística + ruido + power-up targeting |
| 1 semana de desarrollo | ~3 meses de desarrollo continuo |
