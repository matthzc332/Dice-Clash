# SPEC-066: Ajustes MAGIC — glow neutral, ícono a medida, sin mago

## Contexto
Feedback tras probar 064/065:
1. `cambio` como glow universal hacía que TODOS los pickups mostraran el arte
   de cambio transparente detrás de su ícono.
2. El ícono MAGIC quedaba más grande que una casilla (escala fija 0.25).
3. El mago aparecía encima de la pieza a convertir — ahí solo puede estar
   el círculo ritual.

## Cambios
1. **Glow neutral**: `GetGlowSprite()` vuelve al círculo procedural para todos
   los pickups; campo `glowSprite` eliminado.
2. **Ícono MAGIC**: nuevo campo `magicIconSprite` = `cambio` (LoadFullSprite);
   special-case usa eso, no el glow.
3. **Tamaño de íconos**: escala calculada por bounds — ancho = 70% de la
   casilla (`GetCellWorldSize`), con fallback 0.25. Nuevo campo
   `ActivePowerUp.baseScale`; pulso de `AnimateSpawned` usa baseScale.
4. **MAGIC sin mago**: `MagicEffect` = círculo ritual fade-in → PlayFireRayo +
   shake → conversión (tint/shrink/convert/restore) → círculo fade-out → humo.
   Sin mage sobre la pieza objetivo.

## Criterios de aceptación
1. Shake/Explosion/Fireball/Lightning: glow circular violeta/naranja/etc.,
   SIN el arte de cambio detrás.
2. Pickup MAGIC: ícono cambio solo, ~70% del ancho de una casilla.
3. Conversión MAGIC: solo aparece el círculo ritual en la casilla objetivo.
4. Compila sin warnings nuevos.
