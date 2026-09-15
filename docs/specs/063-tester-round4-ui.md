# SPEC-063: Tester Round 4 — UI fixes

## Contexto
Feedback del tester jugando la build actual: números grandes de dados en lados
incorrectos, OK del reward popup cae en fallback, layout de tarjetas del mapa,
fondo Nigromantes nuevo, textos chicos en ModeSelectionUI.

## Objetivo
Corregir los 6 puntos de feedback sin tocar lógica de juego.

## Cambios

### 1. BattleResultUI — números grandes
- Asignar totales por EQUIPO (azul izquierda / roja derecha), no por rol
  atk/def. Bug: cuando atacaba rojo, su total aparecía del lado azul.
- Reposición: azul a la izquierda de sus dados (`-395`), roja a la derecha de
  los suyos (`+395`), junto a los sprites de dados como pidió el tester.

### 2. CampaignRewardUI — botón OK real
- Cargaba `Sprites/Menu/botonOK_0` (ruta inexistente) → fallback panel+texto.
- Usar `LoadAll("Sprites/Menu/botin ui/botonOK")` + find `botonOK_0`
  (mismo patrón que GoblinDialogue/ChestUI/TutorialRewardUI).

### 3. CampaignMapUI — tarjetas
- Info "WOLF | E:5 | +30g": más abajo (y 4→-30) y más grande (font 7→9).
- Nombre ("THE ARMORY"): más a la derecha (x -20→8).
- Insignia: 46×46 → 89×73.

### 4. Fondo Nigromantes
- Usuario subió `Sprites/Nigromantes/Background/Nigromantes.jpeg` (import
  Multiple → sprite `Nigromantes_0`).
- La campaña usa raza `NewRace`, no `Nigromantes`.
- `CreateBackground`: resolver carpeta `NewRace`→`Nigromantes` y cargar vía
  `LoadAll` (funciona con jpeg import Multiple; mantiene PNGs existentes).

### 5. ModeSelectionUI — textos más grandes
- Ticket subtitle (WITH/NO POWER-UPS): font 8→10.
- Ticket desc ("Pay gold to enter. Full experience." / free): font 7→9.
- EnemyPreview label "NEXT ENEMY": font 9→11.
- EnemyPreview army name: font 15→18.

## Criterios de aceptación
1. Ataque de rojo: total rojo a la derecha, azul a la izquierda (no cruzados).
2. Reward popup muestra sprite botonOK clickeable (no panel con texto).
3. Tarjetas del mapa con info/nombre/insignia en nueva geometría.
4. Nivel con enemigo NewRace muestra fondo Nigromantes.
5. Compila sin errores.
