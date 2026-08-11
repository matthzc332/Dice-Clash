# SPEC-046: Rediseño de Recompensas, Modos de Juego y Exhibidor

- ID: `046-reward-redesign`
- Estado: `draft`
- Fecha: `2026-07-23`

## 1. Contexto

El doc del diseñador define un sistema de recompensas completo con 4 modos de juego, exhibidor de trofeos, listones por victoria, insignias para ranked, onboarding con goblin, y un cofre con sistema de progresión. Muchos sistemas ya existen (CampaignData, ChestManager, InsigniaManager, EconomyManager) pero necesitan reajustes significativos.

### Doc del diseñador (resumen)

- **Power-ups**: 5 tipos (Shake, Boom, Fireball, Lightning, Magic)
- **Obstáculos**: 4 tipos (Roca=Árbol, Celda destruida, Pegote, Mina)
- **22 niveles de campaña** con power-ups/obstáculos específicos por nivel
- **4 modos de juego**: Campaign con/sin power-ups, Ranked con/sin power-ups
- **Exhibidor de trofeos**: pantalla completa con copas, listones e insignias
- **Listones**: medalla por victoria de campaña, color por mundo, tonalidad de claro a oscuro
- **Insignias**: 35 total (22 campaña + 13 cofre/ranked)
- **Cofre**: da oro + insignias con sistema de progresión
- **Ranked**: modo libre con power-ups por raza, obstáculo aleatorio
- **Onboarding**: tutorial → sombras → goblin dialogue → selección de modo
- **Desbloqueo de personaje**: por completar copa

## 2. Alcance

### Incluido

1. Actualizar CampaignData.json con power-ups/obstáculos del doc del diseñador
2. GameConfig con modos de juego (Campaign/Ranked, WithPowerups/WithoutPowerups)
3. Power-ups siempre spawn en tablero (modo solo afecta uso del jugador)
4. RibbonManager — listones por victoria de campaña
5. ExhibidorUI — pantalla completa reemplaza estante del menú
6. Verificar insignias de ranked en InsigniaData.json
7. Cofre con oro + progresión inteligente de insignias + duplicadas
8. EnemyBanner — banner de nombre enemigo al inicio de cada nivel
9. RankedManager — modo ranked libre
10. ModeSelectionUI — menú con 4 opciones de modo
11. GoblinDialogue — onboarding post-tutorial
12. Desbloqueo de personaje por copa

### Excluido

- Multijugador online
- Tienda real de monedas
- Animaciones 3D
- Ranking entre jugadores (solo ranking interno)
- Nuevos sprites de arte (se usan placeholders/procedurales)

## 3. Fases de implementación

---

### Fase 1: Actualizar CampaignData.json

**Objetivo**: Que los 22 niveles coincidan exactamente con el doc del diseñador.

**Cambios en `CampaignData.json`**:

| Nivel | Copa | Enemigo | Power-ups | Obstáculos | Oro |
|-------|------|---------|-----------|------------|-----|
| 1 | 0 | Human | — | — | 10 |
| 2 | 0 | Human | Shake | — | 15 |
| 3 | 1 | Human | Fireball | — | 20 |
| 4 | 1 | Orc | Fireball, Shake | — | 25 |
| 5 | 1 | Wolf | Fireball, Shake | Roca | 30 |
| 6 | 1 | Human | Shake, Boom | — | 35 |
| 7 | 1 | Wolf | Boom, Fireball | Roca | 50 |
| 8 | 2 | Orc | Boom, Lightning | — | 30 |
| 9 | 2 | Human | Shake, Lightning | — | 35 |
| 10 | 2 | Wolf | Fireball, Lightning | Celda destruida | 40 |
| 11 | 2 | Orc | Shake | Roca | 45 |
| 12 | 2 | Human | Lightning | Roca, Celda destruida | 60 |
| 13 | 3 | Wolf | Shake, Boom | Celda destruida | 45 |
| 14 | 3 | Human | Lightning, MAGIC | — | 50 |
| 15 | 3 | Orc | Shake, MAGIC | Roca | 55 |
| 16 | 3 | Human | Fireball, MAGIC | Celda destruida | 60 |
| 17 | 3 | Wolf | Boom, MAGIC | Pegote | 75 |
| 18 | 4 | Wolf | Shake, Boom | Pegote | 60 |
| 19 | 4 | Human | Lightning, MAGIC | — | 65 |
| 20 | 4 | NewRace | Shake, MAGIC | Celda destruida | 70 |
| 21 | 4 | Orc | Fireball, MAGIC | Roca | 80 |
| 22 | 4 | NewRace | Boom, MAGIC | Mina | 100 |

**Notas**:
- "Roca" es el obstáculo de árbol/barril/gárgola actual (bloquea movimiento)
- "NewRace" = Nigromantes (placeholder hasta tener sprites)
- Cups: Iron Crown(Human 3-7), Blood Fang(Orc 8-12), Wild Heart(Beastfolk 13-17), Void Seal(Nigromantes 18-22)
- Nombres de ejércitos ya están correctos en el JSON actual

**Archivos a modificar**:
- `Assets/Resources/Data/CampaignData.json`
- `Assets/Scripts/Game/CampaignData.cs` (si necesita campos nuevos)

---

### Fase 2: GameConfig — Modos de juego

**Objetivo**: Agregar flags para distinguir Campaign/Ranked y WithPowerups/WithoutPowerups.

**Cambios en `GameConfig.cs`**:

```csharp
public enum GameMode { Campaign, Ranked }
public enum PowerupMode { WithPowerups, WithoutPowerups }

public static GameMode currentGameMode { get; set; }
public static PowerupMode currentPowerupMode { get; set; }

// Modificar PlayCampaign existente:
public static void PlayCampaign(int levelId, PowerupMode powerupMode)

// Nuevo:
public static void PlayRanked(PowerupMode powerupMode)

// Existentes (mantener para compatibilidad):
// Play(species) — modo libre sin campaña
// PlayTutorial()
```

**Nuevo flujo**:
- `PlayCampaign(levelId, powerupMode)` → carga nivel de campaña con/sin power-ups
- `PlayRanked(powerupMode)` → carga nivel random de ranked con/sin power-ups

**Archivos a modificar**:
- `Assets/Scripts/Game/GameConfig.cs`

---

### Fase 3: Power-ups siempre spawn, modo solo afecta uso

**Objetivo**: Los power-ups SIEMPRE aparecen en el tablero. La diferencia entre modos es si el jugador puede usarlos.

**Lógica**:

| Modo | Power-ups en tablero | Jugador puede usarlos | Enemigos pueden usarlos |
|------|---------------------|----------------------|------------------------|
| WITH powerups | Sí | **Sí** | Sí |
| WITHOUT powerups | Sí | **No** | Sí |

**Implementación**:
- `PowerUpManager.SpawnOnBoard()` se ejecuta siempre (sin cambios)
- `PowerUpManager.CheckCollectionForTeam()` verifica `currentPowerupMode`:
  - Si `WithoutPowerups` y `team == Blue` → no ejecuta power-up, solo muestra feedback visual
  - Si `WithPowerups` → ejecuta normalmente
- Los enemigos (AI) siempre pueden usar power-ups (sin restricción)
- El jugador ve los power-ups en el tablero pero no los activa en modo WithoutPowerups

**Archivos a modificar**:
- `Assets/Scripts/Game/PowerUpManager.cs` (o `InputManager.cs`)

---

### Fase 4: RibbonManager — Listones por victoria

**Objetivo**: Cada victoria en campaña da 1 listón con color por mundo y tonalidad de claro a oscuro.

**Nueva clase `RibbonManager.cs`**:

- Persistencia: PlayerPrefs (`Ribbon_Level_{id}` = 1 si ganado)
- Métodos:
  - `GrantRibbon(int levelId)` — otorga listón por nivel
  - `HasRibbon(int levelId)` — verifica si tiene listón
  - `GetRibbonsByCup(int cupId)` — lista de listones de una copa
  - `GetTotalRibbons()` — total de listones
  - `GetRibbonColor(int levelId)` — color según mundo y tonalidad

**Colores por mundo**:
- Cup 1 (Human): Azul (RGB: 0.3-0.7, 0.5-0.9, 1.0) — 5 tonalidades de claro a oscuro
- Cup 2 (Orc): Verde (RGB: 0.2-0.6, 0.7-1.0, 0.3-0.6) — 5 tonalidades
- Cup 3 (Beastfolk): Marrón/verde (RGB: 0.5-0.8, 0.4-0.7, 0.2-0.4) — 5 tonalidades
- Cup 4 (Nigromantes): Violeta (RGB: 0.5-0.8, 0.2-0.5, 0.8-1.0) — 5 tonalidades

**Fórmula de tonalidad**: `t = (nivelEnCopa - 1) / 4.0f` (0.0 = claro, 1.0 = oscuro)

**Integración**:
- Llamar `RibbonManager.GrantRibbon(levelId)` desde `CampaignManager.CompleteLevel()`
- El scoreboard o GameOverUI puede mostrar animación de listón ganado

**Archivos**:
- `Assets/Scripts/Game/RibbonManager.cs` (NUEVO)
- `Assets/Scripts/Game/CampaignManager.cs` (modificar `CompleteLevel()`)

---

### Fase 5: Exhibidor de trofeos — Pantalla completa

**Objetivo**: Reemplazar el estante de trofeos en el menú principal con un botón que abre una pantalla completa.

**Nueva clase `ExhibidorUI.cs`**:

**Layout de pantalla completa**:

```
┌──────────────────────────────────────────────┐
│  [BACK]           TROPHIES                   │
│                                              │
│  ┌─────────────┐  ┌─────────────────────┐   │
│  │             │  │    RIBBONS          │   │
│  │   COPAS     │  │  [ribbon] [ribbon]  │   │
│  │             │  │  [ribbon] [ribbon]  │   │
│  │  [Copa 1]   │  │  [ribbon] [ribbon]  │   │
│  │  [Copa 2]   │  │  12/22 ribbons      │   │
│  │  [Copa 3]   │  ├─────────────────────┤   │
│  │  [Copa 4]   │  │    INSIGNIAS        │   │
│  │             │  │  [ins] [ins] [ins]  │   │
│  │  2/4 cups   │  │  [ins] [ins] [ins]  │   │
│  │             │  │  5/35 insignias      │   │
│  └─────────────┘  └─────────────────────┘   │
│                                              │
│  Gold: 150                                   │
└──────────────────────────────────────────────┘
```

**Elementos**:
- **Izquierda (40%)**: 4 copas grandes, doradas si completadas, gris si no
- **Derecha arriba (60%)**: Grid de listones (5 columnas), opacos/grises si no conseguidos
- **Derecha abajo (60%)**: Grid de insignias (5 columnas), opacas/grises si no conseguidas
- **Contadores**: "X/4 cups", "X/22 ribbons", "X/35 insignias"
- **Botón BACK**: vuelve al menú principal

**Integración en `MainMenuManager.cs`**:
- Eliminar `BuildTrophies()` (el estante actual)
- Crear botón "TROPHIES" en posición similar al estante
- Click → crea `ExhibidorUI` como componente, llama `Show(canvas)`
- Botón BACK → destruye panel

**Archivos**:
- `Assets/Scripts/Game/ExhibidorUI.cs` (NUEVO)
- `Assets/Scripts/Game/MainMenuManager.cs` (modificar `BuildTrophies()` → botón)

---

### Fase 6: Verificar insignias de ranked

**Objetivo**: Asegurar que las 13 insignias de cofre/ranked existan correctamente en `InsigniaData.json`.

**Estado actual**: Las 13 insignias ya existen (chest_01 a chest_13). Verificar que:
- Nombres y descripciones son correctos
- Raridades están bien distribuidas (4 common, 3 rare, 3 epic, 3 legendary)
- Source es "chest" para todas

**Si el diseñador quiere cambiar nombres/descripciones**: actualizar el JSON.

**Archivos**:
- `Assets/Resources/Data/InsigniaData.json` (verificar/actualizar)

---

### Fase 7: Cofre con oro + progresión inteligente de insignias

**Objetivo**: El cofre da oro + insignias con un sistema que prioriza insignias nuevas al principio y luego permite duplicadas.

**Sistema de progresión de insignias**:

```
Algoritmo por cada insignia drop del cofre:
1. Calcular % de insignias de cofre coleccionadas
   collected = InsigniaManager.GetCollectedBySource("chest")
   total = InsigniaManager.GetTotalBySource("chest")
   pct = collected / total

2. Decidir si dar nueva o repetida:
   Si pct < 0.50 → 80% nueva, 20% repetida
   Si pct 0.50-0.80 → 50% nueva, 50% repetida
   Si pct > 0.80 → 20% nueva, 80% repetida
   Si collected == total → 100% repetida

3. Si nueva: filtrar pool de no-coleccionadas
   Si repetida: pool completo
```

**Oro en cofre**:
- Base: 50 oro por cofre
- Escalar por rareza (si se implementan cofres de distintas calidades futuro)
- Guardar en `ChestManager.OpenChest()` y devolver junto con insignias

**Cambios en `ChestManager.OpenChest()`**:
```csharp
// Retornar tupla de oro + insignias
public struct ChestReward {
    public int gold;
    public List<string> insignias;
    public List<bool> isDuplicate;
}

public static ChestReward OpenChest(int slotIndex)
```

**Cambios en `ChestUI.ShowRewardPopup()`**:
- Primero: animación "50 GOLD!" con monedas
- Luego: revelar insignias una por una (corrutina)
- Cada insignia: flash blanco 0.3s → mostrar icono + nombre
- Si es duplicada: sello rojo "DUPLICADA!" encima
- Sonido de reveal por cada insignia
- Sonido especial de "DUPLICADA!" si aplica

**Archivos**:
- `Assets/Scripts/Game/ChestManager.cs` (modificar `OpenChest()`, `RollInsignia()`)
- `Assets/Scripts/Game/ChestUI.cs` (modificar `ShowRewardPopup()`, nueva corrutina de reveal)
- `Assets/Scripts/Game/InsigniaManager.cs` (ya tiene `GetCollectedBySource()`, verificar)

---

### Fase 8: EnemyBanner — Banner de nombre enemigo

**Objetivo**: Al inicio de cada nivel de campaña, mostrar un banner/listón grande con el nombre del grupo enemigo en el centro del mapa.

**Nueva clase `EnemyBanner.cs`**:

**Comportamiento**:
1. Al inicio del nivel (después de `SetupInitialBoard()`), buscar nombre del grupo en `CampaignData.json`
2. Crear banner/listón en el centro del canvas
3. Animación: fade in (0.5s) → stay (2s) → fade out (0.5s)
4. Font: Press Start 2P (ya guardado en proyecto)
5. Texto: nombre del grupo (ej: "BANDIDOS DE PONIENTE")
6. Efecto: color del texto según raza enemiga

**Efectos visuales**:
- Banner: imagen de listón/ribbon procedurales o sprite existente
- Texto con sombra o outline para legibilidad
- Posición: centro del canvas (0, 0)

**Integración**:
- Crear desde `GameManager.Start()` o `BoardManager.Start()` después del setup
- Solo mostrar en modo Campaign (no en Ranked ni Free Play)
- Verificar `GameConfig.isCampaign == true`

**Archivos**:
- `Assets/Scripts/Game/EnemyBanner.cs` (NUEVO)
- `Assets/Scripts/Game/GameManager.cs` (agregar llamada)

---

### Fase 9: Modo Ranked

**Objetivo**: Implementar modo libre con enemigos aleatorios, power-ups por raza, obstáculo aleatorio.

**Nueva clase `RankedManager.cs`**:

**Desbloqueo**: Al superar "arena 1" (completar tutorial + sombras). Guardar en PlayerPrefs.

**Configuración por raza enemiga**:

| Raza enemiga | Power-ups en escenario |
|-------------|----------------------|
| Human | Shake, Boom |
| Orc | Fireball, Lightning |
| Wolf (Beastfolk) | Lightning, MAGIC |
| NewRace (Nigromantes) | Fireball, MAGIC |

**Obstáculos**: 1 aleatorio por partida (Roca, Celda destruida, Pegote, o Mina)

**Flujo**:
1. Seleccionar raza enemiga al azar
2. Seleccionar power-ups según tabla
3. Seleccionar 1 obstáculo al azar
4. Crear tablero con configuración
5. Enemigos SÍ pueden usar power-ups
6. Jugador depende del modo (con/sin power-ups)

**Recompensas de ranked**:
- Oro (50-100 base)
- Posible cofre (misma lógica que campaña)
- Insignias (via cofre)

**Integración**:
- `GameConfig.PlayRanked(powerupMode)` carga escena con configuración random
- `GameManager.Start()` verifica `isRanked` y configura tablero accordingly
- `RankedManager` se encarga de elegir raza/power-ups/obstáculos

**Archivos**:
- `Assets/Scripts/Game/RankedManager.cs` (NUEVO)
- `Assets/Scripts/Game/GameConfig.cs` (agregar `isRanked`, `PlayRanked()`)
- `Assets/Scripts/Game/GameManager.cs` (verificar `isRanked`)

---

### Fase 10: Menú de selección de modo

**Objetivo**: Cuando el jugador hace clic en PLAY, mostrar menú con 4 opciones.

**Nueva clase `ModeSelectionUI.cs`**:

**Layout**:

```
┌─────────────────────────────────┐
│        SELECT MODE              │
│                                 │
│  ┌──────────────┐ ┌──────────┐ │
│  │  CAMPAIGN    │ │ RANKED   │ │
│  │  WITH ITEMS  │ │ WITH     │ │
│  │  Cost: 20g   │ │ ITEMS    │ │
│  └──────────────┘ │ Cost: 30g│ │
│  ┌──────────────┐ └──────────┘ │
│  │  CAMPAIGN    │ ┌──────────┐ │
│  │  NO ITEMS    │ │ RANKED   │ │
│  │  Cost: 10g   │ │ NO ITEMS │ │
│  └──────────────┘ │ Cost: 15g│ │
│                   └──────────┘ │
│  Gold: 150        [BACK]       │
└─────────────────────────────────┘
```

**Costos** (del EconomyData.json):
- Campaign WITH: nivelEntryCosts[cup] (0/10/20/30)
- Campaign WITHOUT: nivelEntryCosts[cup] / 2
- Ranked WITH: 30 oro fijo
- Ranked WITHOUT: 15 oro fijo

**Comportamiento**:
- Click en opción → verificar oro suficiente
- Si tiene oro → descontar y lanzar modo
- Si no tiene oro → mostrar mensaje "Not enough gold!"
- Botón BACK → volver al menú principal

**Integración en `MainMenuManager.cs`**:
- Modificar `OnPlayClicked()`:
  - Si es primera vez → tutorial
  - Si no → abrir `ModeSelectionUI`

**Archivos**:
- `Assets/Scripts/Game/ModeSelectionUI.cs` (NUEVO)
- `Assets/Scripts/Game/MainMenuManager.cs` (modificar `OnPlayClicked()`)

---

### Fase 11: Goblin Dialogue (Onboarding)

**Objetivo**: Flujo de onboarding post-tutorial con goblin que explica el sistema.

**Nueva clase `GoblinDialogue.cs`**:

**Flujo del doc del diseñador**:
1. Primera vez → Tutorial (maniquíes)
2. Sombras humanas
3. Después de sombras, en nivel 3 de campaña (Human), goblin aparece
4. Goblin dice: "¡Estos power-ups son gratis! Los próximos se pagan con oro recolectado"
5. Click para continuar
6. Siguiente nivel → pregunta si juegas con/sin power-ups
7. Guardar en PlayerPrefs que ya se mostró (`GoblinShown = 1`)

**Visual**:
- Popup modal con retrato de goblin (procedural o placeholder)
- Texto de diálogo con typewriter effect
- Botón "OK" o "CONTINUE"
- Fondo oscuro semi-transparente

**Integración**:
- Verificar `PlayerPrefs.GetInt("GoblinShown", 0) == 0`
- Mostrar después del nivel de sombras (nivel 2) o al inicio del nivel 3
- Una vez mostrado, no volver a mostrar

**Archivos**:
- `Assets/Scripts/Game/GoblinDialogue.cs` (NUEVO)
- `Assets/Scripts/Game/ScoreboardUI.cs` o `GameOverUI.cs` (agregar llamada post-nivel 2)
- `Assets/Scripts/Game/MainMenuManager.cs` (verificar si necesita cambio)

---

### Fase 12: Desbloqueo de personaje por copa

**Objetivo**: Al completar una copa entera, mostrar dialogue de desbloqueo de personaje/raza.

**Comportamiento**:
- Al completar Copa 4 (nivel 22) → mostrar dialogue de desbloqueo de Nigromantes
- Guardar en PlayerPrefs: `Unlocked_Nigromantes = 1`
- El personaje queda disponible en el carrusel del menú
- Dialogue: "¡Has desbloqueado NIGROMANTES!" con sprite placeholder

**Integración**:
- En `CampaignManager.CompleteLevel()`, verificar si se completó la última copa
- Si sí → mostrar dialogue
- `MainMenuManager` verifica `IsUnlocked()` para mostrar personaje en carrusel

**Archivos**:
- `Assets/Scripts/Game/CampaignManager.cs` (modificar `CompleteLevel()`)
- `Assets/Scripts/Game/MainMenuManager.cs` (verificar `IsUnlocked()`)

---

## 4. Archivos a crear/modificar

### Archivos nuevos (7)

| Archivo | Descripción |
|---------|-------------|
| `Assets/Scripts/Game/RibbonManager.cs` | Listones por victoria de campaña |
| `Assets/Scripts/Game/ExhibidorUI.cs` | Pantalla completa de trofeos |
| `Assets/Scripts/Game/EnemyBanner.cs` | Banner de nombre enemigo |
| `Assets/Scripts/Game/RankedManager.cs` | Modo ranked libre |
| `Assets/Scripts/Game/ModeSelectionUI.cs` | Selección de modo (4 opciones) |
| `Assets/Scripts/Game/GoblinDialogue.cs` | Onboarding con goblin |

### Archivos a modificar (10)

| Archivo | Cambios |
|---------|---------|
| `CampaignData.json` | Actualizar power-ups/obstáculos según doc |
| `GameConfig.cs` | GameMode, PowerupMode, isRanked, PlayRanked() |
| `GameManager.cs` | Verificar isRanked, llamar EnemyBanner |
| `PowerUpManager.cs` | Verificar currentPowerupMode antes de ejecutar |
| `MainMenuManager.cs` | Reemplazar estante por botón Exhibidor, flujo play |
| `CampaignManager.cs` | GrantRibbon, desbloqueo de personaje |
| `ChestManager.cs` | OpenChest con oro, sistema de progresión |
| `ChestUI.cs` | Popup con reveal secuencial, DUPLICADA!, oro |
| `InsigniaData.json` | Verificar insignias de ranked |
| `ScoreboardUI.cs` | Integrar ribbons |

## 5. Orden de implementación recomendado

1. **Fase 1** — CampaignData.json (base de todo)
2. **Fase 2** — GameConfig (modos de juego)
3. **Fase 3** — Power-ups siempre spawn
4. **Fase 4** — RibbonManager
5. **Fase 6** — Verificar insignias
6. **Fase 7** — Cofre con progresión
7. **Fase 8** — EnemyBanner
8. **Fase 11** — GoblinDialogue (antes de modos para que el onboarding funcione)
9. **Fase 10** — ModeSelectionUI
10. **Fase 9** — RankedManager
11. **Fase 5** — ExhibidorUI
12. **Fase 12** — Desbloqueo de personaje

**Razón**: El orden prioriza primero los datos base, luego las mecánicas que otros sistemas dependen (ribbons, insignias, cofres), después el onboarding, los modos, y finalmente el exhibidor que es puramente visual.

## 6. Criterios de aceptación

- [ ] CampaignData.json tiene los 22 niveles con power-ups/obstáculos correctos del doc
- [ ] GameConfig distingue Campaign/Ranked y WithPowerups/WithoutPowerups
- [ ] Power-ups siempre aparecen en tablero, jugador no puede usarlos en WithoutPowerups
- [ ] RibbonManager otorga listón por victoria de campaña con color correcto
- [ ] ExhibidorUI muestra copas, listones e insignias en pantalla completa
- [ ] Cofre da oro + insignias con progresión inteligente (nuevas primero)
- [ ] Insignias duplicadas muestran sello "DUPLICADA!"
- [ ] EnemyBanner muestra nombre del grupo enemigo al inicio de cada nivel
- [ ] RankedManager genera niveles aleatorios con power-ups por raza
- [ ] ModeSelectionUI muestra 4 opciones con costos
- [ ] GoblinDialogue aparece post-tutorial y explica el sistema
- [ ] Desbloqueo de personaje al completar copa
- [ ] Build pasa sin errores

## 7. Notas

- Los sprites de goblin, listones, banner, y insignias son placeholders/procedurales
- "NewRace" (Nigromantes) usa Beastfolk como placeholder hasta tener sprites
- El ranking entre jugadores es futuro (no incluido en este spec)
- Los costos de oro pueden necesitar balance posterior con testing
- El doc del diseñador menciona "la gran victoria" como objetivo a largo plazo (futuro)
