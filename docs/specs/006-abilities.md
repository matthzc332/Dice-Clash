# SPEC-006: Habilidades Especiales por Ficha

- ID: `006-abilities`
- Rama: `spec/006-abilities`
- Estado: `in-progress`
- Autor: IA
- Fecha: 2026-06-08

## 1. Contexto

Cada tipo de ficha tiene stats base (ATK/DEF) pero no hay diferenciación táctica más allá de esos números. Para dar profundidad al posicionamiento y decisiones, cada tipo necesita una habilidad pasiva única que se active en condiciones específicas durante el combate.

## 2. Objetivo

Implementar una habilidad pasiva por tipo de ficha que modifique el combate bajo ciertas condiciones tácticas.

## 3. Alcance

### Incluido

| Tipo    | Habilidad   | Efecto                                          |
| ------- | ----------- | ----------------------------------------------- |
| Ninja   | Flank       | +2 ATK si el enemigo no tiene aliados adyacentes|
| Knight  | Charge      | +1 ATK si se movió 2+ casillas antes de atacar |
| Paladin | Aura        | +1 DEF a aliados adyacentes                     |
| Pawn    | —           | Sin habilidad                                   |

- Las habilidades se evalúan durante `CombatManager.Resolve()`
- El panel de stats muestra la habilidad activa cuando corresponde
- El panel de dados muestra el bonus aplicado (ej. "Flank +2")
- Aura de Paladin se recalcula al inicio de cada combate

### Excluido

- Habilidades activas (que requieren selección del jugador)
- Efectos visuales/partículas
- Sonidos
- Animaciones

## 4. Criterios de Aceptación

- [ ] Ninja ataca a enemigo aislado → muestra "Flank +2" y suma al ATK
- [ ] Ninja ataca a enemigo con aliado adyacente → sin bonus
- [ ] Knight ataca después de mover 2+ tiles → muestra "Charge +1" y suma al ATK
- [ ] Knight ataca después de mover 1 tile → sin bonus
- [ ] Paladin tiene aliado adyacente → ese aliado recibe +1 DEF en combate
- [ ] StatsPanel muestra la habilidad cuando está activa
- [ ] DicePanel muestra el bonus aplicado
- [ ] El proyecto compila sin errores

## 5. Diseño Técnico

### Archivos a modificar

- `CombatManager.cs` — Evaluar habilidades antes de resolver dados
- `StatsPanelUI.cs` — Mostrar texto de habilidad activa
- `DicePanelUI.cs` — Mostrar bonus por habilidad en totales

### Estrategia

1. En `CombatManager.Resolve()`, antes de tirar dados:
   - Determinar si atacante/defensor tienen habilidades aplicables
   - Calcular bonus temporales
   - Pasar info de habilidades al resultado
2. Agregar `PieceData.GetAbilityModifier(...)` que devuelve bonus según contexto
3. StatsPanel muestra línea de habilidad cuando está activa
4. DicePanel muestra desglose con bonus

### Implementación de habilidades

**Flank (Ninja)**: Escanear tiles adyacentes al defensor. Si ningún tile ocupado por aliado del defensor tiene ficha, +2 ATK al atacante.

```csharp
static bool IsFlanking(PieceData attacker, Tile defenderTile, BoardManager board)
{
    foreach (var (dr, dc) in Directions.All8)
    {
        Tile adj = board.GetTileWorld(defenderTile.row + dr, defenderTile.col + dc);
        if (adj != null && adj.IsOccupied && adj.pieceData?.team == attacker.team)
            return false; // enemy has ally adjacent
    }
    return true; // enemy is isolated
}
```

**Charge (Knight)**: Requiere tracking de distancia recorrida. Simple: si la distancia Manhattan entre origen y destino es >= 2, +1 ATK.

```csharp
static bool IsCharging(int fromRow, int fromCol, int toRow, int toCol)
{
    return Mathf.Abs(fromRow - toRow) + Mathf.Abs(fromCol - toCol) >= 2;
}
```

**Aura (Paladin)**: Escanear tiles adyacentes al defensor. Si algún tile ocupado por Paladin aliado, +1 DEF.

```csharp
static bool HasAura(PieceData defender, Tile defenderTile, BoardManager board)
{
    foreach (var (dr, dc) in Directions.All4)
    {
        Tile adj = board.GetTileWorld(defenderTile.row + dr, defenderTile.col + dc);
        if (adj != null && adj.IsOccupied && adj.pieceData?.type == PieceType.Paladin && adj.pieceData?.team == defender.team)
            return true;
    }
    return false;
}
```

### Flujo de datos

```
CombatManager.Resolve():
  1. Calcular atkBonus, defBonus desde habilidades
  2. atkTotal = dado + atk.baseAttack + atkBonus
  3. defTotal = dado + def.baseDefense + defBonus
  4. Devolver resultado + info de habilidades
```

## 6. Dependencias

### Con otros specs

- Depende de SPEC-004 (004-combat) ✅ done
- Depende de SPEC-005 (005-game-over) ✅ done

### Impacto en documentación

- Ninguno

## 7. Plan de Implementación

1. Agregar métodos de evaluación de habilidades en `CombatManager.cs`
2. Modificar `CombatManager.Resolve()` para aplicar bonuses
3. Agregar texto de habilidad activa en `StatsPanelUI.cs` y `DicePanelUI.cs`

## 8. Plan de Validación

- Test manual:
  - Posicionar Ninja contra enemigo aislado → ver bonus Flank
  - Posicionar Ninja contra enemigo con aliado → sin bonus
  - Mover Knight 2+ tiles y atacar → ver bonus Charge
  - Mover Knight 1 tile y atacar → sin bonus
  - Poner Paladin junto a aliado y atacar al aliado → ver Aura +1 DEF

## 9. Rollback

Revertir cambios en `CombatManager.cs`, `StatsPanelUI.cs`, `DicePanelUI.cs`.

## 10. Notas

- Las habilidades son pasivas y automáticas (sin input del jugador)
- El tracking de Charge usa la distancia Manhattan entre origen y destino del movimiento
- Aura solo aplica en defensa (no cuando el Paladin ataca)
