# SPEC-075: Optimización de texturas (tamaño WebGL/APK)

- ID: `075-optimizacion-texturas`
- Estado: `in-progress`
- Fecha: `2026-09-03`

## 1. Contexto

El build WebGL descarga **140 MB** (`.data.br`) al cargar → la carga inicial es muy lenta. El APK Android pesa **179 MB**. Dani (diseñador) solicita optimizar urgentemente a **≤ 50 MB**.

Causa raíz: **391 PNG** en `Assets/Resources/` con `maxTextureSize: 2048`, compresión normal y **`crunchedCompression: 0`**. Los fondos son PNG de ~2816×1536 (~8 MB c/u).

## 2. Objetivo

Reducir el tamaño del build WebGL (`*.data.br`) de ~140 MB a **≤ 50 MB** (ideal 25-40 MB) optimizando texturas, sin pérdida visual perceptible.

## 3. Alcance

### Incluido

- Script de editor `Assets/Editor/TextureOptimizer.cs` con `[MenuItem]`.
- Aplicar `textureCompression=1` + `crunchedCompression=1` a todos los PNG de `Assets/Resources/**`.
- Fijar `maxTextureSize` por clase de asset.
- Redimensionar físicamente los fondos gigantes a 1920×1080.
- Eliminar los 12 PNG legacy de `Assets/Prefabs/Pieces/`.

### Excluido

- **Audio** (`Sounds/Fondo/*.mp3`, ~21 MB) — requiere ffmpeg, se pospone.
- **Deduplicar decor por especie** — cada tema (Human/Orc/Beastfolk/etc.) tiene arte distinto aunque compartan nombre.
- **Sprites de sombra / paredes / decor sin uso** — los borra el usuario manualmente.

## 4. Criterios de Aceptación

- [ ] `Assets/Editor/TextureOptimizer.cs` existe y se puede ejecutar desde el menú "Optimize".
- [ ] Todos los `TextureImporter` de Resources quedan con `crunchedCompression=1`.
- [ ] Los fondos gigantes quedan redimensionados a 1920×1080 (o menor).
- [ ] Build WebGL: `*.data.br` ≤ 50 MB.
- [ ] El juego se ve igual (menú, partida por especie, power-ups) sin sprites distorsionados.
- [ ] Compilación Roslyn = 0 errores.

## 5. UX/UI

- No hay cambios de pantalla. Solo se reduce resolución/compresión de texturas existentes.
- Riesgo: leve pérdida de nitidez en fondos si se ven en 4K (irrelevante móvil/web/RV).

## 6. Diseño Técnico

- Componentes/archivos a modificar:
  - `Assets/Editor/TextureOptimizer.cs` (nuevo).
  - `Assets/Resources/Sprites/**` y `Assets/Resources/Tutorial/**` (meta de import).
  - Fondos gigantes: redimensión in-place.
  - `Assets/Prefabs/Pieces/` (eliminar 12 PNG legacy).
- Estrategia: script de editor recorre `TextureImporter`, cambia compresión/maxSize por carpeta, reimporta con `ForceUpdate`. Redimensión física con `Texture2D`/`System.Drawing` para fondos específicos.
- Nuevas dependencias: ninguna.
- Riesgos técnicos: los `.meta` se modifican; si Unity falla a mitad, reimportar corriendo el script de nuevo. Crunch aumenta tiempo de build.

## 7. Dependencias

### Con otros specs
- Ninguna.

### Impacto en documentación
- [x] `AGENTS.md` (entrada 075 agregada antes de implementar, por si algo se rompe).

## 8. Plan de Implementación

1. Actualizar docs (AGENTS.md + spec) — DONE.
2. Crear `Assets/Editor/TextureOptimizer.cs` (crunch + maxTextureSize por clase).
3. Añadir al script la redimensión física de fondos gigantes a 1920×1080.
4. Eliminar `Assets/Prefabs/Pieces/` (12 PNG legacy).
5. Recompilar WebGL y medir `*.data.br`.

## 9. Plan de Validación

- Comandos:
  - Compilación Roslyn de `Assets/Scripts/Game` + `Assets/Editor` contra DLLs de Unity = 0 errores.
- Tests manuales:
  - Menú principal.
  - Partida con cada especie (Human/Orc/Beastfolk/Nigromantes) y power-ups.
  - Tutorial.
- Evidencia esperada: nuevo tamaño de `*.data.br` y logs de build.

## 10. Rollback

Los cambios son solo a `.meta` (import) y archivos PNG. Rollback: volver a aplicar los settings anteriores o restaurar `.meta`/PNG desde git. Ningún cambio de lógica de juego.

## 11. Notas

- `muneco1/2/3` del tutorial se usan — NO se borran.
- El usuario borró manualmente: sombras (`Tutorial/*Sombra*.PNG`), paredes y decor sin uso.
- Meta principal: `.data.br` ≤ 50 MB.
