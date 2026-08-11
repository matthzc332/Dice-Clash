# SPEC-020: Persistencia de Trofeos

- ID: `020-trophy-persist`
- Rama: `spec/020-trophy-persist`
- Estado: `in-progress`
- Autor: AI
- Fecha: 2026-06-23

## 1. Contexto

Actualmente los trofeos en el menú principal y el bloqueo de especies en el carrusel usan valores hardcodeados (`isLocked = { false, true, true }`). El jugador puede ganar partidas pero el progreso no se guarda entre sesiones.

## 2. Objetivo

Persistir los trofeos desbloqueados vía `PlayerPrefs` y reflejarlos visualmente en el menú principal (trofeos coloreados, carrusel desbloqueado).

## 3. Alcance

### Incluido

- Guardar en `PlayerPrefs` cuando el jugador gana (Blue) una partida en una especie/mundo.
- Cargar el estado de trofeos al iniciar el menú principal.
- Trofeo desbloqueado: sprite a color completo, label cambia de "LOCKED" al nombre de la especie.
- Carrusel: especie desbloqueada se muestra a color y sin label "LOCKED".
- Humanos siempre desbloqueados por defecto.

### Excluido

- Pantalla de selección de mundo más allá del carrusel actual.
- Guardar estadísticas adicionales (puntajes, tiempo, etc.).
- Desbloqueo de trofeos para el equipo rojo (solo aplica cuando gana el jugador/azul).

## 4. Criterios de Aceptación

- [ ] Al ganar una partida como Humanos, se desbloquea el trofeo Orco.
- [ ] Al ganar una partida como Orcos, se desbloquea el trofeo Bestia.
- [ ] Al reiniciar el juego, los trofeos desbloqueados persisten.
- [ ] El trofeo desbloqueado se muestra a color (sin tinte gris) y sin label "LOCKED".
- [ ] La especie desbloqueada aparece en el carrusel a color y sin label "LOCKED".
- [ ] PlayerPrefs se resetea con `DeleteAll` para pruebas (no requiere UI).

## 5. UX/UI

- **Pantallas impactadas**: MainMenu (trofeos + carrusel)
- **Trofeo bloqueado**: tinte gris `(0.5, 0.5, 0.5, 0.8)` + label "LOCKED" (sin cambios)
- **Trofeo desbloqueado**: tinte blanco `(1, 1, 1, 1)` + label con nombre de la especie ("HUMAN", "ORC", "BEASTFOLK")
- **Carrusel bloqueado**: tinte gris `(0.5, 0.5, 0.5, 0.8)` + label "LOCKED"
- **Carrusel desbloqueado**: tinte blanco + label de especie a color `(0.9, 0.7, 0.2)`

## 6. Diseño Técnico

### Archivos a modificar

- `Assets/Scripts/Game/MainMenuManager.cs` — leer PlayerPrefs para determinar trofeos desbloqueados
- `Assets/Scripts/Game/ScoreboardUI.cs` — guardar trofeo cuando Blue gana

### Estrategia

1. Usar `PlayerPrefs.SetInt("Trophy_Orc", 1)` y `PlayerPrefs.SetInt("Trophy_Beastfolk", 1)` para persistencia.
2. En `MainMenuManager.Start()`, leer `PlayerPrefs.GetInt("Trophy_Orc", 0)` y `PlayerPrefs.GetInt("Trophy_Beastfolk", 0)` para determinar `isLocked`.
3. En `ScoreboardUI.AnimateScoreboard()`, cuando el ganador es Blue, determinar qué trofeo desbloquear según `GameConfig.selectedSpecies`:
   - Si `selectedSpecies == "Human"` → desbloquear `Trophy_Orc`
   - Si `selectedSpecies == "Orc"` → desbloquear `Trophy_Beastfolk`
4. Actualizar el color/label de trofeos y carrusel según el estado guardado.

### Dependencias

Ninguna.

## 7. Plan de Implementación

1. Modificar `MainMenuManager` para leer PlayerPrefs y actualizar `isLocked` dinámicamente.
2. Modificar `BuildTrophies` para mostrar trofeos desbloqueados a color.
3. Modificar `BuildSlot` para usar el nuevo estado de bloqueo.
4. Modificar `ScoreboardUI` para guardar trofeo al ganar.
5. Probar en Unity.

## 8. Plan de Validación

- Ejecutar partida como Humanos, ganar, verificar que trofeo Orco se desbloquea en el menú.
- Cerrar y abrir el juego, verificar persistencia.
- Build de Windows para validar.
