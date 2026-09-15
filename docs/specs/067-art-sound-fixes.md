# SPEC-067: Fixes misc — puño, lock, estrellas, música, fondo, reward

## Contexto
Feedback del tester / art+sfx:
1. Puño de Shake caía de la izquierda; siempre debe caer desde la derecha.
2. Lock de tarjetas en mapa de campaña se veía como cuadrado blanco.
3. Estrellas necesitaban criterio de rendimiento y tiempo, con insignia solo a 3★.
4. Última pista de gameplay suena más baja que la primera.
5. Fondo nigromantes no se mostraba en partidas con NewRace.
6. Contenido interno del popup REWARD! demasiado chico.

## Cambios
1. **Puño**: bezier invertido (start +X, apex +X, rotación Lerp(25°→0°)).
2. **Lock**: textura 16×16 redibujada con grillete + cuerpo + ojala, RGB16 con
   FilterMode Point, sizeDelta 34×34.
3. **Estrellas**: `blueAlive/7f ≥ 0.5` = +1★; `elapsed ≤ 180s` = +1★.
   `CompleteLevel(levelId, stars)` — insignia solo cuando `stars >= 3`.
4. **Música**: volúmenes por pista (`0.45` horizons / `0.85` deuslower).
   Estructura extensible con arrays `campaignTracks`/`campaignTrackVolumes` vacíos.
5. **Fondo**: LoadAll con fallback adicional a `Texture2D` + `Sprite.Create`
   (cubre cualquier import mode). Midground intenta carpeta temática primero
   (`Nigromantes/fondo3`), fallback Human.
6. **Reward**: filas 400→470, iconos fuente ~×1.25, height ×1.2, spacing +20%,
   título 22→24, sub 14→16, estrellas 58→74 + spacing 72→86.

## Criterios de aceptación
1. Puño siempre cae de derecha; rotación espejo del diseño original.
2. Lock se lee claramente como candado.
3. 3★ requiere >50% piezas vivas Y ≤3 min; insignia otorgada solo a 3★.
4. Ambas pistas de gameplay se escuchan al mismo nivel aproximado.
5. Fondo Nigromantes aparece en niveles con enemyRace NewRace.
6. REWARD! muestra contenido más grande y legible.
