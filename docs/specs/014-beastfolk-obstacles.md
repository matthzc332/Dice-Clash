# SPEC-014: Mapa BeastFolk y Obstáculos

- ID: `014-beastfolk-obstacles`
- Rama: `spec/014-beastfolk-obstacles`
- Estado: `in-progress`
- Autor: IA
- Fecha: 2026-06-14

## 1. Contexto

El juego tiene 3 especies (Human, Orc, Beastfolk) seleccionables desde el botón "ESPECIE". Beastfolk ya está en el ciclo de especies pero no tiene implementación visual ni de gameplay. Se quiere que el mapa Beastfolk tenga 3 árboles como obstáculos que spawnen en casilleros aleatorios vacíos para agregar variación táctica.

## 2. Objetivo

- Completar la integración de Beastfolk como especie jugable con assets placeholder generados proceduralmente
- Agregar 3 obstáculos (árboles) en posiciones aleatorias del tablero cuando se juega en mapa Beastfolk
- Los obstáculos bloquean el paso y no pueden ser atacados ni ocupados
- El botón "ESCENARIO" debe ciclar Human → Orc → Beastfolk

## 3. Alcance

### Incluido

- Asset placeholder procedural para Beastfolk (Background, Floor, Decor/Pieces)
- `Cell.isObstacle` para marcar celdas bloqueadas
- Spawn de 3 árboles en posiciones aleatorias vacías del tablero en `CreateBoardDecorations`
- `GetValidMoves` ignora celdas con obstáculo (no se mueve ni ataca a través)
- Botón "ESCENARIO" cicla Human → Orc → Beastfolk en `TurnUI.cs`
- Destruir obstáculos al hacer `SwitchSpecies` o `SwitchScenario`

### Excluido

- Pantalla de inicio (spec separado)
- IA enemiga (spec separado)
- Sprites reales de Beastfolk (se usan placeholders procedurales)
- Cobertura / bonus defensivo por árboles

## 4. Criterios de Aceptación

- [ ] Al seleccionar Beastfolk como especie, el tablero tiene 3 árboles en posiciones distintas
- [ ] Los árboles nunca aparecen sobre fichas
- [ ] Los árboles nunca se superponen entre sí
- [ ] No se puede mover una ficha a una casilla con árbol
- [ ] No se puede atacar a través de un árbol
- [ ] Al cambiar de especie los árboles se destruyen
- [ ] El botón ESCENARIO cicla Human → Orc → Beastfolk → Human...
- [ ] El proyecto compila sin errores

## 5. Diseño Técnico

### Cell.cs

```csharp
public bool isObstacle;
```

### BoardManager.cs

- En `CreateBoardDecorations()`, si `scenarioTheme == "Beastfolk"`, spawnear 3 árboles:
  - Elegir posiciones aleatorias `(row, col)` donde la celda no esté ocupada ni sea obstáculo
  - Crear GameObject visual con SpriteRenderer de un cuadrado verde (placeholder)
  - Marcar `cell.isObstacle = true`
  - Guardar referencias para limpiar al cambiar de escenario
- En `GetValidMoves()`, si `cell.isObstacle` es true, no agregar la celda ni continuar la línea de movimiento
- En `SwitchSpecies()`: destruir árboles existentes

### TurnUI.cs

- En `OnScenarioButtonClicked()`, reemplazar el array por `{ "Human", "Orc", "Beastfolk" }`

### Sprite

- Placeholder procedural: cuadrado verde con `CreateSquareSprite()`

## 6. Archivos a modificar

| Archivo | Cambio |
|---------|--------|
| `Cell.cs` | Agregar `bool isObstacle` |
| `BoardManager.cs` | Spawn de árboles, `GetValidMoves` con obstáculos |
| `TurnUI.cs` | Escenario cicla Human → Orc → Beastfolk |

## 7. Dependencias

Ninguna.

## 8. Plan de Implementación

1. Cell.cs — agregar `isObstacle`
2. BoardManager.cs — spawn obstáculos + validación de movimiento
3. TurnUI.cs — ciclo de escenarios
4. Build de prueba

## 9. Plan de Validación

- Build en Unity
- Test manual: seleccionar Beastfolk, verificar 3 árboles
- Test manual: intentar mover ficha a casilla con árbol (no debe poder)
- Test manual: cambiar de especie, verificar que árboles desaparecen

## 10. Rollback

Revertir cambios en Cell.cs, BoardManager.cs, TurnUI.cs.

## 11. Notas

- Los sprites reales de Beastfolk se agregarán después; por ahora se usa placeholder procedural
