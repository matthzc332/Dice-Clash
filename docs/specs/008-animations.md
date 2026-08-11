# SPEC-008: Animaciones Visuales

- ID: `008-animations`
- Rama: `spec/008-animations`
- Estado: `in-progress`
- Fecha: 2026-06-08

## 1. Contexto

Todas las acciones en el tablero ocurren instantáneamente: la ficha aparece en el destino sin transición, el combate resuelve sin feedback visual, el dado cambia sin efecto.

## 2. Objetivo

Agregar animaciones procedurales (corutinas, sin Animator ni dependencias) para dar feedback visual a movimiento, combate y dados.

## 3. Alcance

### Incluido

| Animación       | Duración | Descripción                                    |
|----------------|----------|------------------------------------------------|
| Movimiento     | 0.2s     | Lerp suave de origen a destino con ease-out    |
| Destrucción    | 0.4s     | Shake (0.1s) + fade out alpha (0.3s)           |
| Dados          | 0.6s     | Ciclo rápido de glifos ⚀-⚅ antes del resultado |

- Las animaciones son bloqueantes: el flujo del juego espera a que terminen antes de continuar (victory check, cambio de turno)
- BoardManager usa corutina para MovePiece
- DicePanelUI usa corutina para ShowResult

### Excluido

- Animator / Mecanim / Animation Clips
- Partículas
- Efectos de cámara (zoom, shake global)
- Ease curves complejas (solo lerp lineal + fade lineal)
- Animación de selección (pulse highlight)

## 4. Criterios de Aceptación

- [ ] Mover ficha a casilla vacía → la ficha se desliza suavemente en 0.2s
- [ ] Atacante gana combate → defensor se sacude 0.1s + se desvanece 0.3s
- [ ] Defensor gana combate → atacante se sacude 0.1s + se desvanece 0.3s
- [ ] Combatir → dados ciclan glifos 0.6s antes de mostrar resultado final
- [ ] El proyecto compila sin errores

## 5. Diseño Técnico

### BoardManager

`MovePiece()` cambia a `void` y arranca una corutina interna que maneja todo el flujo (animación → combate/desplazamiento → victory check → fin de turno).

```csharp
public void MovePiece(int fromRow, int fromCol, int toRow, int toCol)
{
    StartCoroutine(AnimatedMove(from, to));
}

IEnumerator AnimatedMove(Tile from, Tile to)
{
    // 1. Animar movimiento
    // 2. Resolver combate o mover
    // 3. Animar destrucción si aplica
    // 4. CheckVictory / EndTurn
}
```

### DicePanelUI

`ShowResult()` arranca corutina que cicla dados antes de mostrar el resultado real.

### Destroy animation

```csharp
IEnumerator AnimateDestroy(GameObject visual)
{
    // Shake
    Vector3 origin = visual.transform.position;
    for (float t = 0; t < 0.1f; t += Time.deltaTime)
    {
        visual.transform.position = origin + Random.insideUnitCircle * 0.08f;
        yield return null;
    }
    visual.transform.position = origin;
    // Fade
    SpriteRenderer sr = visual.GetComponent<SpriteRenderer>();
    for (float t = 0; t < 0.3f; t += Time.deltaTime)
    {
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f - t / 0.3f);
        yield return null;
    }
    Destroy(visual);
}
```

## 6. Archivos modificados

- `BoardManager.cs` — nuevo `AnimatedMove()`, `AnimateDestroy()`
- `DicePanelUI.cs` — nuevo `AnimateDiceRoll()`, `ShowResult` lo arranca
- `InputManager.cs` — `ExecuteMove()` ya no llama a `board.CheckVictory` ni `turnManager.EndTurn`; eso pasa dentro de la corutina

## 7. Plan de Validación

Test manual:
- Mover ficha → ver slide suave
- Atacar y ganar → ver shake + fade del defensor, dados animados
- Atacar y perder → ver shake + fade del atacante, dados animados

## 8. Rollback

Revertir cambios en BoardManager, DicePanelUI, InputManager.

## 9. Notas

- InputManager.ExecuteMove se simplifica: solo llama a `board.MovePiece()`, el resto lo maneja la corutina
- No bloquear input durante animación por ahora (el turno termina después de la corutina igual)
