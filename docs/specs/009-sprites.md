# SPEC-009: Sprites de Personajes

- ID: `009-sprites`
- Rama: `spec/009-sprites`
- Estado: `done`
- Fecha: 2026-06-08

## 1. Contexto

Las fichas se renderizan como cuadrados de colores. Los sprites de los 4 personajes ya están diseñados y colocados en el proyecto.

## 2. Objetivo

Reemplazar los placeholders por los sprites reales (Front/Back) con tintado de equipo.

## 3. Alcance

### Incluido

- 8 sprites en `Assets/Resources/Sprites/Pieces/` (cargables vía `Resources.Load`)
- Mapeo PieceType → nombre del sprite (Peon, Ninja, Caballero, Paladin)
- Tintado de equipo: Blue = color original, Red = 30% más oscuro
- CharacterCardUI muestra el sprite de la ficha junto a los stats

### Excluido

- Animación de sprites (idle/walk/attack)
- Cambio de dirección (Front/Back) según movimiento
- Partículas o efectos visuales adicionales

## 4. Criterios de Aceptación

- [ ] Cada ficha muestra su sprite correspondiente en el tablero
- [ ] Equipo azul se ve con colores originales
- [ ] Equipo rojo se ve ligeramente oscurecido
- [ ] Al seleccionar ficha, el CharacterCardUI muestra su sprite
- [ ] El proyecto compila sin errores

## 5. Archivos modificados

- `BoardManager.cs` — `PlacePiece` usa `CreatePieceVisual` que carga sprite vía `Resources.LoadAll`
- `CharacterCardUI.cs` — `Show` carga y muestra el sprite de la ficha

## 6. Rollback

Revertir cambios en BoardManager.cs y CharacterCardUI.cs.
