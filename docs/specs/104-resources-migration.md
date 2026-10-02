# SPEC-104: Migración de Assets fuera de Resources

## Contexto
El proyecto utiliza Resources.Load y Resources.LoadAll de forma extensiva. Unity empaqueta todo lo que esté dentro de Resources/ en el build.

## Objetivo
Reducir el peso del build WebGL moviendo sprites fuera de Resources/, manteniendo funcionalidad idéntica. Sin añadir dependencias nuevas (Addressables no usado en esta fase).

## Alcance IN
- Migrar sprites (UI, piezas, decor, efectos, power-ups, fondos, tutorial) a referencias directas vía SpriteRegistry + ThemeSpriteSet (ScriptableObjects).
- Mantener wrapper con fallback a Resources (migración incremental segura).
- Dejar en Resources/ únicamente lo estrictamente necesario (allowlist).

## Allowlist
- Data/*.json (CampaignData, InsigniaData, EconomyData) – TextAsset pequeño.
- Sounds/Fondo/*, Sounds/Efectos/* – AudioClip (fase posterior opcional).
- Video/* – StreamingAssets (no toca).

## Estrategia
- ThemeSpriteSet (SO) por tema: Human/Orc/Beastfolk/Nigromantes/Tutorial.
- SpriteRegistry (MonoBehaviour, DontDestroyOnLoad): agrupa themes + UI global + PowerUps/Efects/FightCloud/Emojis/Campaign.
- BoardManager.GetPieceSprites reemplaza llamadas críticas con fallback a LoadSprites.
- Editor tools: ThemeSpriteSetCreator, ThemeSpriteSetFiller, SpriteRegistryCreator para auto-rellenar desde Resources/.

## Criterios de aceptación
1. Compilación 0 errores CS.
2. Play Mode funcional, sin MissingReference/NullReference.
3. Reducción significativa de rutas Resources.Load/LoadAll (objetivo < 20 rutas en Resources).
4. Sin cambios visuales perceptibles.
5. Build WebGL posterior con menor initialLoadSize.

## Fases
- Fase 0: Inventario (completo) ?
- Fase 1: Scaffolding + Editor tools ?
- Fase 2: Crear SOs + rellenar + SpriteRegistry en escena (Unity Editor) — pendiente
- Fase 3: Migrar UI (single-loads) — pendiente
- Fase 4: Piezas (wrapper activo + validación) ? (wrapper listo)
- Fase 5: PowerUps/Efectos/FightCloud/Emojis — pendiente
- Fase 6: Backgrounds/Tutorial/Decor/Win/Menu — pendiente
- Fase 7: Limpiar usos Resources obsoletos — pendiente