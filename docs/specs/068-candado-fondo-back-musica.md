# SPEC-068: Fixes — candado, fondo Nigromantes, Back loop, música campaña, UI

## Contexto
Feedback del tester:
1. Lock de tarjetas sigue con panel gris de fondo que parece bug.
2. Fondo de Nigromantes no aparece en partidas con NewRace.
3. Back button en ModeSelectionUI lleva a la partida sin piezas en vez de menú.
4. Archivo `campaignTrackVolumes.mp3` subido — debe sonar en menú de campaña, no en gameplay.
5. Texto "CAMPAIGN" del menú principal fuera de pantalla.
6. REWARD! texto muy chico.
7. Caballero se achica en el aire al saltar hacia atrás (lvl 22 Nigromantes).

## Cambios
1. **Candado**: locked card background `Color.clear` (invisible), outline `Color.clear`. Solo se ve el icono del candado dorado sobre el tablero, sin panel gris.
2. **Fondo Nigromantes**: JPEG meta cambiado de `spriteMode:2` (Multiple) a `spriteMode:1` (Single) —
   `Resources.Load<Sprite>` funciona directo. Sección spriteSheet limpiada. BoardManager: fallback
   adicional `Resources.Load<Sprite>` antes del Texture2D.
3. **Back button**: `ModeSelectionUI.CreateBackButton` ahora carga `MainMenuScene` en vez de solo
   `Destroy(panel)`. Agregado `using UnityEngine.SceneManagement`.
4. **Música campaña**: Campo `campaignMenuTrack`/`campaignMenuTrackVolume` en SoundManager. Método
   `PlayCampaignMenuMusic()` llama `CampaignMapUI.ShowInternal`. `CampaignMapUI.Close()` restaura
   `PlayMenuMusic()`. Array `campaignTracks` eliminado.
5. **CAMPAIGN button**: Position Y de -510 a -470 (junto con chest e insignia).
6. **REWARD!**: Font 24→28, subtitle 16→18.
7. **Knight jump backward**: Escalas hardcoded reemplazadas por cálculo dinámico: `idleScale * 0.55 * sqrt(frontArea/backArea)` — compensa sprites back más grandes manteniendo tamaño visual consistente con el salto forward.

## Criterios de aceptación
1. Candado grande y dorado visible sobre tarjeta gris.
2. Fondo Nigromantes visible al jugar con enemyRace NewRace.
3. Back desde ModeSelectionUI después de sombras → menú principal.
4. Pista de campaña suena al jugar niveles de campaña.
