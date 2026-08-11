# SPEC-013: Balance de Combate y Feedback Visual

- ID: `013-combat-balance-and-feedback`
- Rama: `spec/013-combat-balance-and-feedback`
- Estado: `in-progress`
- Autor: IA
- Fecha: 2026-06-12

## 1. Contexto

El combate actual usa `1d6 + baseStat + bonus` con stats de rango pequeño (ATK 1-3, DEF 0-3). El dado tiene demasiado peso: un Peón (ATK 1) puede vencer a un Paladín (DEF 3) ~17% de las veces. El director del proyecto reporta que "se rompe bastante", que ganó con dos peones y un paladín, y que el azar domina demasiado.

Además, el panel de resultado (DicePanelUI + StatsPanelUI) muestra ecuaciones textuales (ej: `ATK 3+5=8`) que el director considera "feo". Quiere un diseño visual con cartas, iconos (espada/escudo/banderas), animación del ganador y sonidos de impacto.

También se reporta que la animación idle de las piezas (PulsePiece y ciclo de sprites Idle) "no tiene el efecto lindo, es cansador" y debe removerse.

## 2. Objetivo

- Rebalancear el combate para que el rango de stats sea significativamente mayor que la varianza del dado, y las fichas de tier inferior no puedan vencer a las de tier superior.
- Rediseñar el panel de resultado de batalla con cartas visuales, iconos desde Resources/Sprites/Decor, animación del ganador y sonidos.
- Remover animaciones idle (PulsePiece y ciclo de sprites Idle) para dejar sprites estáticos.

## 3. Alcance

### Incluido

- Cambio de 1d6 a 2d6 en la resolución de combate
- Multiplicación de stats base ×3
- Sistema de tiers con penalización por diferencia de tier
- Nueva tabla de stats en PieceData
- Actualización de ability bonuses ×3
- Nuevo `BattleResultUI` que reemplaza DicePanelUI y StatsPanelUI
- Diseño visual con cartas usando Espada.png, Escudo.png, BlueFlag.png, RedFlag.png desde Decor
- Animación de entrada de cartas, revelación del ganador, y botón interactivo
- Efectos de sonido sincronizados (dados, impacto, fanfarria)
- Remover `PulsePiece` coroutine y todas las referencias a idle sprite animation
- Actualización del hover preview en InputManager

### Excluido

- Nuevos sprites de dados (se reusan Dado1-Dado6)
- Cambios en reglas de movimiento o habilidades
- HP system
- IA enemiga
- Cambios en GameOverUI, TurnUI, CharacterCardUI

## 4. Criterios de Aceptación

- [ ] Combate usa 2d6 en vez de 1d6
- [ ] Stats base actualizadas según tabla ×3
- [ ] Penalización por tier aplicada
- [ ] Peón NO puede ganar a Paladín bajo ninguna combinación de dados
- [ ] Peón NO puede ganar a Caballero bajo ninguna combinación de dados
- [ ] Peón puede ganar a Ninja (raro pero posible)
- [ ] Bonuses de habilidades actualizados (Flank +6, Charge +3, Aura +3)
- [ ] BattleResultUI muestra dos cartas (atacante azul con espada, defensor rojo con escudo)
- [ ] Cada carta muestra: sprite de personaje, nombre, desglose de dados + stats con iconos, total
- [ ] Banner del ganador usa BlueFlag/RedFlag según equipo
- [ ] El ganador es un botón animado: al hacer clic se cierra el panel
- [ ] PulsePiece eliminado
- [ ] Sin ciclo de sprites Idle en CreatePieceVisual ni AnimatedMove
- [ ] DicePanelUI.cs, StatsPanelUI.cs, CombatPanelUI.cs eliminados
- [ ] Efectos de sonido: dados al caer, impacto/escudo, fanfarria
- [ ] El proyecto compila sin errores

## 5. UX/UI

### Layout del BattleResultUI

```
┌─────────────────────────────────────────────────────┐
│  ┌──────────────┐           ┌──────────────┐        │
│  │  AZUL        │    VS     │  ROJO        │        │
│  │  ┌────────┐  │           │  ┌────────┐  │        │
│  │  │ sprite │  │           │  │ sprite │  │        │
│  │  │ carta  │  │           │  │ carta  │  │        │
│  │  └────────┘  │           │  └────────┘  │        │
│  │  CABALLERO   │           │  PEÓN        │        │
│  │  [Espada]    │           │  [Escudo]    │        │
│  │  2d6+6 = 12  │           │  2d6+0 = 5  │        │
│  └──────────────┘           └──────────────┘        │
│                                                      │
│  ┌─────────────────────────────────────────┐         │
│  │  ATAQUE 12 vs DEFENSA 5                 │         │
│  │  ⚔ CABALLERO supera a PEÓN             │         │
│  └─────────────────────────────────────────┘         │
│                                                      │
│  ┌─────────────────────────────────────┐             │
│  │  [BlueFlag]  GANADOR: CABALLERO     │ ← click    │
│  └─────────────────────────────────────┘             │
└─────────────────────────────────────────────────────┘
```

### Estados

| Estado | Acción |
|--------|--------|
| Entrada | Cartas deslizan desde izquierda/derecha (0.3s) |
| Dados | Dados caen con animación física |
| Revelación | Resultado numérico aparece |
| Ganador | Banner pulsa/brilla, botón se activa |
| Cierre | Click en ganador → panel se desvanece |

### Sonidos

- `PlayDice()` al empezar animación
- `PlayHit()` o `PlayBlock()` al mostrar resultado
- `PlayVictory()` fanfarria al mostrar ganador

## 6. Diseño Técnico

### Sistema de Balance

#### PieceData.cs

```csharp
public int Tier => type switch
{
    PieceType.Pawn => 1,
    PieceType.Ninja => 2,
    PieceType.Knight => 3,
    PieceType.Paladin => 4,
    _ => 1
};

public int baseAttack => type switch
{
    PieceType.Pawn => 3,
    PieceType.Ninja => 9,
    PieceType.Knight => 6,
    PieceType.Paladin => 3,
    _ => 1
};

public int baseDefense => type switch
{
    PieceType.Pawn => 0,
    PieceType.Ninja => 3,
    PieceType.Knight => 6,
    PieceType.Paladin => 9,
    _ => 0
};
```

#### CombatManager.cs

Fórmula de combate:
```
atkTotal = 2d6 + attacker.baseAttack + atkBonus - tierPenalty(attacker, defender)
defTotal = 2d6 + defender.baseDefense + defBonus - tierPenalty(defender, attacker)

tierPenalty(self, other):
  if self.tier < other.tier → (other.tier - self.tier) * 2
  else → 0

Resultado: atkTotal > defTotal → AttackerWins (empate → DefenderWins)
```

- 2d6: `Random.Range(1,7) + Random.Range(1,7)`
- Bonuses: Flank +6, Charge +3, Aura +3
- Tier penalty calculado en `Resolve()` y `GetAbilityModifiers()`

#### BattleResultUI.cs (nuevo)

Clase singleton, auto-crea `BattleCanvas` con `sortingOrder = 88`.

Componentes:
- CanvasGroup para fade
- GameObject "BlueCard" con: Image (card sprite de CharacterCardUI), Text nombre, Image (Espada.png), Text desglose dados+stats, Text total
- GameObject "RedCard" análogo con Escudo.png
- Text "VS" entre cartas
- GameObject "ResultSection" con: Text comparativo (ATAQUE X vs DEFENSA Y)
- GameObject "WinnerBanner" (Button) con: Image (BlueFlag/RedFlag), Text ganador, Outline/Hover
- Métodos: `ShowResult()`, `ClearUI()`, `AnimateEntry()`, `OnWinnerClick()`

Animaciones:
1. Cartas: slide desde bordes (LeanTween o coroutine con lerp)
2. Dados: reuso de lógica de DicePanelUI (caída física con 2 dados por lado)
3. Banner: pulso de escala (1.0 ↔ 1.05, loop)
4. Hover: color más brillante en el botón del ganador

#### BoardManager.cs — Remover idle

- Eliminar `PulsePiece` coroutine y variable `pulseRoutine`
- Eliminar llamadas `StopCoroutine(pulseRoutine)`, `StartCoroutine(PulsePiece(...))`
- En `CreatePieceVisual`: cargar sprite base sin animación (usar Front/Back normal, no Idle)
- En `AnimatedMove`: cargar sprite base sin animación idle después del combate/movimiento

### Archivos a modificar

| Archivo | Cambios |
|---------|---------|
| `PieceData.cs` | Agregar `Tier`, actualizar `baseAttack`, `baseDefense` |
| `CombatManager.cs` | 2d6, penalización por tier, bonuses ×3 |
| `BoardManager.cs` | Llamar a BattleResultUI, remover idle |
| `InputManager.cs` | Actualizar hover preview con nuevos stats |
| `SoundManager.cs` | Ajustes de sonido si es necesario |

### Archivos a crear

| Archivo | Propósito |
|---------|-----------|
| `BattleResultUI.cs` | Nuevo panel unificado de resultado |

### Archivos a eliminar

| Archivo | Razón |
|---------|-------|
| `DicePanelUI.cs` | Reemplazado |
| `StatsPanelUI.cs` | Reemplazado |
| `CombatPanelUI.cs` | Stub |

## 7. Dependencias

- SPEC-004 (combat) ✅ — modifica el sistema de combate
- SPEC-006 (abilities) ✅ — modifica los bonuses

## 8. Plan de Implementación

1. PieceData.cs — stats ×3 + Tier
2. CombatManager.cs — 2d6 + penalización + bonuses ×3
3. BattleResultUI.cs — panel completo con animaciones
4. BoardManager.cs — integrar BattleResultUI + remover idle
5. InputManager.cs — preview stats nuevos
6. SoundManager.cs — ajustes
7. Eliminar DicePanelUI.cs, StatsPanelUI.cs, CombatPanelUI.cs

## 9. Plan de Validación

- Test manual de matchups:
  - Peón vs Paladín → no puede atacar (o preview muestra 0%)
  - Peón vs Ninja → posible pero raro
  - Paladín vs Peón → victoria segura
- Verificar panel nuevo con cartas e iconos
- Verificar que el clic en ganador cierra el panel
- Verificar que no hay idle animation en piezas
- Build sin errores

## 10. Rollback

Revertir cambios en PieceData.cs, CombatManager.cs, BoardManager.cs, InputManager.cs, SoundManager.cs. Restaurar DicePanelUI.cs, StatsPanelUI.cs desde git. Eliminar BattleResultUI.cs.

## 11. Notas

- Los iconos (Espada, Escudo, banderas) se cargan desde `Resources/Sprites/Decor/`
- La animación de dados existente se adapta para mostrar 2 dados
- El tiempo de espera fijo se reemplaza por clic del jugador en el ganador
