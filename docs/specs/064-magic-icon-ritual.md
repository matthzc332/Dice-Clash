# SPEC-064: Icono cambio para MAGIC + ritual del mago

## Contexto
El power-up MAGIC no tiene ícono propio en el tablero (busca sprite "MAGIC"
inexistente → fallback procedural). El usuario subió `ritual.png` para la
invocación del mago. Hoy `MagicEffect` NO muestra al mago (solo chispas).

## Objetivo
1. Pickup MAGIC del tablero usa sprite `cambio`.
2. MAGIC ejecuta secuencia ritual: círculo → mago → ataque → conversión →
   mago desaparece → círculo desaparece → humo.

## Secuencia ritual (MagicEffect)
1. Círculo (`Sprites/PowerUps/Efect/ritual`, import Multiple → LoadAll[0])
   en la casilla objetivo, ancho = 1.15× casilla, fade-in 0.3s + rotación lenta,
   tinte violeta MAGIC. Chispas violetas alrededor. PlayLightning.
2. Mago aparece (magoIdle, escala 0.35, orden 16, sobre el centro).
3. Ataque: magoAttack + PlayFireRayo + shake → conversión existente
   (tint→shrink→ConvertPieceType(Peon)→restore color→chispas blancas).
4. Mago desaparece: magoback + fade 0.25s → destroy.
5. Círculo fade-out 0.3s (sigue rotando) → destroy → SmokeBurst(targetPos, 6).

## Otros cambios
- Ícono pickup MAGIC: en ambos spawns (`SpawnOnBoard`, `SpawnSpecificAt`),
  si no hay sprite "MAGIC"/"MAGIC_0" usar `GetGlowSprite()` (= `cambio`,
  ya cargado desde `Sprites/PowerUps/Icon/cambio`).
- Helper `GetCellWorldSize()` mide distancia entre centros de casillas
  (independiente de resolución del sprite).
- Helpers compartidos `SpawnRitualCircle(pos, tint)` + `FadeRitualCircle(circle, a0, a1, dur)`
  (fade + rotación 60°/s). Usados también por `MageAndProjectile` (Fireball,
  tinte naranja) y `MageLightning` (Lightning, tinte amarillo): el mago aparece
  sobre su círculo ritual y este se desvanece cuando el mago se va.
- Quien pisa el pickup ejecuta el efecto para su equipo (azul o IA roja);
  MAGIC convierte una pieza aleatoria del equipo contrario.

## Criterios de aceptación
1. Pickup MAGIC muestra `cambio` flotando sobre su glow.
2. Al ejecutar MAGIC se ve la secuencia completa círculo→mago→ataque→humo.
3. Null-safety tras cada yield (pieza/tablero pueden morir a mitad).
4. Compila sin errores.
