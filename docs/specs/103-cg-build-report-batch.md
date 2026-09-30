# SPEC-103: Build report CG en batch + nitidez del arte fullscreen

- ID: `103-cg-build-report-batch`
- Rama: `spec/102-skip-astc-variant`
- Estado: `done`
- Fecha: `2026-09-29`

## 1. Contexto

Dos problemas reportados por Hernán sobre el build de CrazyGames:

**1. "No existe el build report" / "¿estás seguro que compilás para WebGL?"**

El Analyzer del SDK (`CrazySDK → Go to Analyzer`) lee `Library/CGReleaseBuildReportSummary-v1.json` y, si no existe, muestra *"No build report found, please do a Release build"*. El archivo existía pero estaba **congelado con el build del 23/9** (73,4 MiB), aunque se habían hecho dos builds CG posteriores.

Causa raíz: el summary lo escribe `BuildCompleteHandler.OnPostprocessBuild` dentro de un `EditorApplication.delayCall` (`Assets/CrazySDK/Scripts/Editor/Builder/BuildCompleteHandler.cs:23`). `delayCall` corre recién en el próximo tick del editor, así que en **batch mode con `-quit`** el proceso termina antes y la lambda nunca se ejecuta. Como nuestros builds se hacen 100% por CLI, el summary nunca se regeneró.

**2. Pixelado del arte fullscreen**

Los fondos de pantalla completa llegan hasta 2816×1536 y `TextureOptimizer.PickMaxSize` los bajaba a 512, así que Unity estiraba 512×279 a 1920×1080 (×3,7) → pixelado visible en menú, partida, diálogos y pantallas de victoria. `crunchedCompression` ya estaba activo, así que no era culpa del mipmap.

## 2. Objetivo

1. Que el build CG por CLI genere `Library/CGReleaseBuildReportSummary-v1.json` con datos reales del build (fecha, `initialLoadSize`, assets empaquetados), para que el Analyzer deje de mostrar "No build report found".
2. Eliminar el pixelado del arte fullscreen subiendo esos assets a 1024, manteniendo `initialLoadSize` por debajo de la meta de CrazyGames (50 MB).

## 3. Alcance

### Incluido

- `Assets/Editor/BuildScript.cs`:
  - `BuildReportCapture : IPostprocessBuildWithReport` (callbackOrder 1) que guarda el `BuildReport` de forma sincrónica.
  - `WriteCGSummary(BuildReport)`: replica la lógica de `BuildCompleteHandler.GenerateReportSummary` (mismo `OrderByDescending(packedSize)` + `GroupBy(sourceAssetPath).First()` y `initialLoad` = data+wasm+framework+loader vía `BuildReportGenerator.GetMainFiles`) y escribe el summary.
  - `BuildReleaseCGCLI()` la invoca al terminar el build.
  - `WriteCGSummaryCLI()` permite regenerar el summary sin recompilar (fallback sin `BuildReport`: lee `Builds/CrazyGamesRelease/crazygames_build_report.json` + walk de la carpeta).
- `Assets/Editor/TextureOptimizer.cs`:
  - `PickMaxSize`: `Background` / `Win` / `Tutorial` / `Menu/Menu.png` / `Score` a **1024**.
  - `compressionQuality = 100` para todo asset con `maxTextureSize >= 1024`.
  - `/Insignias/`, `/Relleno_Insignias/` y `Tutorial/copaTuto.png` a **256**.
  - Nueva `IsSmallSprite()`: `nube*`, `trumpet*`, `punio*`, `mago*`, `ritual` a **512**; el resto de `/Menu/` queda en 512.

### Excluido

- Quitar `crunchedCompression` de los fondos (1024 + crunch ya se ve bien y crunch pesa en CPU, no en tamaño).
- Re-comprimir o subir el build al Developer Portal de CrazyGames.
- Splash/logo de Unity en el build CG (ver spec 102, decisión pendiente con Hernán).
- Cualquier cambio de lógica de juego.

## 4. Criterios de Aceptación

- [x] `Library/CGReleaseBuildReportSummary-v1.json` existe con la fecha del último build (no la del 23/9).
- [x] `packagedFiles` viene del `BuildReport` real (2618 assets), no 0 → el Analyzer muestra el análisis completo.
- [x] `initialLoadSize` < 50 MB: **45,93 MiB (48.161.551 B)**.
- [x] `Builds/CrazyGamesRelease/crazygames_build_report.json` = `buildVariant: Release`, `buildResult: Succeeded`, `supportsMobile: false`, sin variante ASTC.
- [x] El build contiene solo los 4 archivos WebGL (`data.br`, `wasm.br`, `framework.js.br`, `loader.js`).
- [x] Arte fullscreen sin pixelado visible (pendiente confirmación visual de Hernán).
- [x] Build batch sin errores de compilación (0 `error CS`).

## 5. UX/UI

- Sin cambios de pantalla ni de flujo.
- El Analyzer del SDK pasa de "No build report found" a mostrar el análisis; al reabrir Unity hay que apretar **Analyze** otra vez (`AnalyzeTab.InitOnLoad()` pone el `_report` estático en `null`).
- Riesgo visual: subir a 1024 agrega ~3,1 MiB al `data.br` (36,2 → 39,3 MiB) y deja el total en 45,93 MiB, con 4 MiB de margen bajo la meta.

## 6. Diseño Técnico

- Archivos modificados: `Assets/Editor/BuildScript.cs`, `Assets/Editor/TextureOptimizer.cs`, `AGENTS.md`.
- Estrategia (build report): en vez de depender del `delayCall` del SDK, se captura el `BuildReport` con un `IPostprocessBuildWithReport` propio (se invoca **dentro** del pipeline, sincrónico) y se escribe el summary al volver de `DoReleaseBuild`.
- Sin dependencias nuevas. Reusa `CrazyGames.BuildReportSummary`, `CrazyGames.PackagedFileSummary`, `CrazyGames.BuildData`, `CrazyGames.GeneratedFile` y `CrazyGames.BuildReportGenerator.GetMainFiles` del propio SDK.
- Riesgos técnicos:
  - `BuildReportCapture` queda registrado para todo build (Android/EXE/WebGL) en cualquier máquina del equipo: solo guarda una referencia estática, costo despreciable.
  - El fallback sin `BuildReport` escribe `packagedFiles` vacío (se ve el tamaño correcto, no el detalle de assets).
  - El summary solo se genera para builds **WebGL no-Development**; con `BuildOptions.Development` el propio SDK lo saltea (`BuildCompleteHandler.cs:36-44`).

## 7. Dependencias

### Con otros specs

- 100 (fix de plataforma `WebGL` en `TextureOptimizer`): prerequisite, sin él nada de esto se aplicaba al build WebGL.
- 101 (bajar pesos y audio mono/Vorbis 0,22): define el piso de 42,8 MiB desde el que se mide el costo de subir a 1024.
- 102 (skip de la variante ASTC): necesario para que el ZIP no duplique el `data.br` en builds desktop-only.

### Impacto en documentación

- [x] `AGENTS.md` (entradas 100, 101, 102 y 103).
- [x] `docs/specs/103-cg-build-report-batch.md` (este documento).
- [x] `docs/WORKLOG.md`.

## 8. Plan de Implementación

1. Confirmar que el build es WebGL y que el summary existe pero está desactualizado. **DONE**
2. Implementar `BuildReportCapture` + `WriteCGSummary` + `WriteCGSummaryCLI`. **DONE**
3. Regenerar el summary sin recompilar para destrabar el Analyzer. **DONE**
4. Subir `PickMaxSize` a 1024 para fullscreen + `compressionQuality 100`, con recortes compensatorios. **DONE**
5. Rebuild CG Release completo. **DONE** (43,6 min, `Succeeded`)
6. Verificar summary (2618 assets) y commitear. **DONE** (`c17b676`, `4b12e82`)

## 9. Plan de Validación

- Comandos:
  - `Unity.exe -batchmode -nographics -quit -projectPath "<proj>" -executeMethod BuildScript.BuildReleaseCGCLI -logFile <log>`
  - `Unity.exe -batchmode -nographics -quit -projectPath "<proj>" -executeMethod BuildScript.WriteCGSummaryCLI -logFile <log>`
  - `git add Assets/Editor/BuildScript.cs Assets/Editor/TextureOptimizer.cs && git commit`
- Tests manuales:
  - Abrir Unity → `CrazySDK → Go to Analyzer` → apretar **Analyze**: debe mostrar 45,93 MiB y la lista de assets.
  - Menú, partida por especie, diálogos del tutorial y pantallas de victoria: verificar que el arte fullscreen se ve nítido.
- Evidencia: `[CGSummary] initialLoad 45,93 MiB | total 46,00 MiB | 2618 archivos` en el log del build.

## 10. Rollback

- Código: `git revert c17b676` y `git revert 4b12e82` (o volver al commit anterior en la rama).
- Texturas: los `.meta` se regeneran corriendo `Optimize/Apply Texture Optimization`; para volver a 512 en fullscreen basta con revertir `PickMaxSize` y reimportar.
- El `Library/CGReleaseBuildReportSummary-v1.json` es regenerable con `WriteCGSummaryCLI`.

## 11. Notas

- **Gotchas de batch (SDK CG Release)**: `Assets/Resources/ExpoBuild.txt` debe estar **ausente** durante el build (se mueve a `.bak` y se restaura); usar un wrapper con `WaitForExit(ms)` + `Stop-Process` porque con errores de compilación `Start-Process -Wait` puede dejar un zombie Reteniendo el lock del proyecto; otro Unity abierto en **otro** proyecto no bloquea el lock.
- En `Start-Process -ArgumentList` hay que pasar el path del proyecto con comillas embebidas (`'"C:\...Unity.exe"'`) o falla con `Couldn't set project path`.
- El build no es reproducible byte a byte: el `data.br` del 29/9 16:41 es `66eaa76f…` y el de 18:38 es `7f8fd12f…` con el mismo tamaño (±2 KB). El ZIP `Builds/CrazyGamesRelease (4).zip` que tiene Hernán contiene el anterior, de contenido equivalente.
- Si el Analyzer sigue vacío después de un build CG, el build no fue WebGL o fue Development: mirar `buildVariant` y `supportsMobile` en `crazygames_build_report.json`.
