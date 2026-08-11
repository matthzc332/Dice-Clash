# SPEC-004: Combate con Dados

- ID: `004-combat`
- Rama: `spec/004-combat`
- Estado: `in-progress`
- Autor: IA
- Fecha: 2026-06-08

## 1. Contexto

Las fichas ya pueden moverse y capturar (SPEC-003), pero la captura es instantánea sin resolución. El juego base necesita la mecánica central: combate con dados donde atacante y defensor se enfrentan, y el perdedor es destruido.

## 2. Objetivo

Implementar combate con dados d6 al mover una ficha a una casilla enemiga. El perdedor del duelo es destruido. Se muestra el resultado en un panel UI fijo en la parte inferior de la pantalla.

## 3. Alcance

### Incluido

- Al clickear destino enemigo → se resuelve combate con dados
- Atacante tira 1d6 + ATK, defensor tira 1d6 + DEF
- Empate → gana el defensor
- El perdedor (atacante o defensor) es destruido
- Si atacante gana: ocupa la casilla del defensor
- Si defensor gana: el atacante es destruido, defensor se queda
- Panel UI en la parte inferior muestra el resultado del duelo
- El panel se limpia al iniciar el siguiente turno
- El turno termina automáticamente tras el combate

### Excluido

- Animaciones de dados 3D
- Efectos de sonido
- IA enemiga para seleccionar objetivo
- Estrategia o bonus por posición
- Modificadores de dados por habilidad especial

## 4. Criterios de Aceptación

- [ ] Click en ficha enemiga → panel muestra `{TIPO} (ATK{n}+{dado}) vs {TIPO} (DEF{n}+{dado}) → {WINNER} WINS!`
- [ ] Si atacante gana: defensor destruido, atacante ocupa la casilla
- [ ] Si defensor gana: atacante destruido, defensor intacto
- [ ] En ambos casos el turno termina automáticamente
- [ ] El panel de combate se oculta al iniciar el siguiente turno
- [ ] `baseAttack` está correcto por tipo de ficha (Pawn=1, Ninja=3, Knight=2, Paladin=1)
- [ ] El proyecto compila sin errores

## 5. UX/UI

- Panel fijo en la parte inferior del viewport
- Fondo semitransparente oscuro con texto blanco
- Formato: `Blue_Ninja (ATK 3+5=8) vs Red_Pawn (DEF 0+2=2) → ATTACKER WINS!`
- Se limpia automáticamente al cambiar de turno

## 6. Diseño Técnico

### Archivos a modificar

- `Assets/Scripts/Game/PieceData.cs` — Agregar propiedad `baseAttack`
- `Assets/Scripts/Game/BoardManager.cs` — `MovePiece` usa `CombatManager` si hay enemigo
- `Assets/Scripts/Game/InputManager.cs` — `ExecuteMove` siempre termina turno

### Archivos nuevos

- `Assets/Scripts/Game/CombatManager.cs` — Lógica de resolución de dados
- `Assets/Scripts/Game/CombatPanelUI.cs` — UI del panel de combate

### Estrategia

1. `PieceData` agrega `baseAttack` como computed property según tipo
2. `CombatManager.Resolve()` tira dos d6, compara totals, devuelve `CombatResult`
3. `BoardManager.MovePiece()`: si hay enemigo, llama a `CombatManager`, destruye al perdedor, mueve al ganador si corresponde
4. `InputManager.ExecuteMove()`: siempre llama a `turnManager.EndTurn()` tras mover
5. `CombatPanelUI` singleton se auto-crea y suscribe a `OnTurnChanged` para limpiarse

### Riesgos técnicos

- Sincronizar correctamente la destrucción de GameObjects con la limpieza del grid
- El panel UI debe crearse en runtime sin prefabs (como el resto de la UI del proyecto)

## 7. Dependencias

### Con otros specs

- Depende de SPEC-003 (003-piece-movement) ✅ done

### Impacto en documentación

- Ninguno por ahora

## 8. Plan de Implementación

1. Agregar `baseAttack` en `PieceData.cs`
2. Crear `CombatManager.cs` con `Resolve()` estático
3. Crear `CombatPanelUI.cs` con singleton y UI en runtime
4. Modificar `BoardManager.MovePiece()` para usar combate
5. Modificar `InputManager.ExecuteMove()` para siempre terminar turno

## 9. Plan de Validación

- Test manual:
  - Mover ficha a casilla enemiga → ver panel de combate
  - Verificar que perdedor es destruido
  - Verificar que el turno cambia
  - Verificar que el panel se limpia al siguiente turno

## 10. Rollback

Revertir cambios en `PieceData.cs`, `BoardManager.cs`, `InputManager.cs`. Eliminar `CombatManager.cs` y `CombatPanelUI.cs`.

## 11. Notas

- El perdedor siempre es destruido (no hay repliegue)
- El empate favorece al defensor
- Los valores de ATK/DEF están hardcodeados por tipo de ficha; se podrán modificar con un ScriptableObject en el futuro
