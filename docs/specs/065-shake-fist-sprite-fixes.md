# SPEC-065: Puño de Shake + fix sprites Multiple

## Contexto
1. El ícono MAGIC seguía mostrando la mancha procedural ("machita violeta") y el
   círculo ritual no aparecía: `cambio.png` y `ritual.png` se importan como
   spritesheet **Multiple** (mode 2) → sus slices se llaman `cambio_0`,
   `ritual_0`, etc., y todas las búsquedas por nombre exacto fallaban.
2. Usuario subió `punio.png` para dar identidad visual al power-up Shake.

## Cambios

### 1. Loader de textura completa (`LoadFullSprite`)
Orden: `Resources.Load<Texture2D>` → `Sprite.Create` (rect completo, funciona
sea cual sea el import) → `Load<Sprite>` (Single) → `LoadAll[0]` (último recurso).
Aplicado a: `cambio` (glow + ícono MAGIC vía `GetGlowSprite`) y `ritual`.

### 2. Puño de Shake (`ShakeWithFist`)
Al pisar Shake:
1. Puño aparece arriba-izquierda, baja en **parábola** (bezier cuadrática,
   0.55s, rotación -25°→0°) hasta la casilla pisada, escala 2.2× casilla.
2. Impacto: DirtChunks.
3. Corre `ShakeEffect` intacta (temblor, sonidos, empuje de enemigos) con el
   puño plantado en el tablero.
4. Al terminar: nube grande de humo (SmokeBurst ×10), fade del puño 0.25s → destroy.

Fallback sin sprite: espera corta y ShakeEffect normal.

## Criterios de aceptación
1. Pickup MAGIC muestra el arte real de `cambio` (no mancha violeta).
2. Ritual del mago muestra el círculo.
3. Shake: puño cae en arco, golpea la casilla, tablero tiembla, puño se esfuma en humo.
4. Compila sin errores.
