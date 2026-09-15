# SPEC-062: Cheat de Reset Total

## Contexto
Para el Sanra Game Fest necesitamos reiniciar el juego a estado fábrica entre demos
(campaña completa, oro acumulado, cofres, insignias, tutorial ya jugado). Sin reset,
cada demo arranca con la progresión del visitante anterior.

## Objetivo
Cheat oculto en el menú principal que borra TODA la progresión persistente y recarga el menú.

## Alcance

### In
- Trigger secreto: 5 taps en zona invisible (esquina inferior derecha del menú) dentro de 3s.
- Popup de confirmación (RESET / CANCEL) para evitar taps accidentales.
- Wipe completo de PlayerPrefs + recarga de `MainMenuScene`.

### Out
- Reset parcial por sistemas.
- Menú de cheats visible.

## Claves borradas
`Campaign_Level_*`, `Campaign_Stars_*`, `Ribbon_Level_*`, `Unlocked_{race}`,
`RankedUnlocked`, `Insignia_*`, `TotalGold`, `LastDailyBonus`, `DailyStreak`,
`ChestSlot_*`, `TutorialPlayed`, `TutorialRewardClaimed`, `TutorialCollectiblesEarned`,
`GoblinDialogueShown`, `Trophy_*`, `SelectedSpecies/Scenario`, `IsCampaign/IsRanked/SelectedLevel`.
→ Implementación: `PlayerPrefs.DeleteAll()` (inventario verificado: no hay otras claves).
Solo `CampaignManager` es DontDestroyOnLoad y lee prefs por llamada → recarga de escena es suficiente.

## Criterios de aceptación
1. 5 taps rápidos en la esquina → aparece popup de confirmación; menos de 5 no hace nada.
2. RESET borra todo: campaña desde nivel 1, oro inicial, sin insignias/listones/cofres,
   ranked bloqueado, goblin/daily bonus no aparecen, tutorial disponible.
3. CANCEL cierra el popup sin borrar nada.
4. Compila sin errores.
