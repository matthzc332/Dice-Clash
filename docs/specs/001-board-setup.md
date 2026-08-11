# SPEC-001: Definición Técnica y Matriz [7][7]

- ID: `001-board-setup`
- Rama: `spec/001-board-setup`
- Estado: `next`
- Autor: IA
- Fecha: 2026-06-05

## 1. Contexto

El juego requiere un tablero de 7x7 con posiciones simétricas espejo para dos facciones (Azul y Rojo). Actualmente no existe ningún script ni estructura de datos en el proyecto. Es la base sobre la que se construirán todas las mecánicas posteriores.

## 2. Objetivo

Implementar la matriz bidimensional [7][7], la estructura de datos de fichas, y el setup inicial del tablero con posiciones espejo, respetando las columnas vacías 1 y 7 en la línea de peones.

## 3. Alcance

### Incluido

- Crear estructura `Tile` para representar casillas (fila, columna, ocupante, etc.)
- Crear `PieceData` como struct/class con: ID, Bando (enum Rojo/Azul), Tipo (enum Peón/Ninja/Caballero/Paladín), Defensa Base
- Crear `BoardManager` que gestione la matriz [7][7] de Tiles
- Implementar `SetupInitialBoard()` con posiciones espejo:
  - Fila 1 (Azul retaguardia): [C, N, P, P, P, N, C]
  - Fila 2 (Azul peones): [·, p, p, p, p, p, ·]
  - Filas 3-5: vacías
  - Fila 6 (Rojo peones): [·, p, p, p, p, p, ·]
  - Fila 7 (Rojo retaguardia): [C, N, P, P, P, N, C]
- Columnas 1 y 7 vacías en líneas de peones (Filas 2 y 6)
- Que el setup sea visible en la escena (instanciación visual de fichas en el tablero)

### Excluido

- Lógica de movimiento de fichas
- Input del jugador (clicks/raycasts)
- Sistema de turnos
- Combate/Dados
- Efectos visuales o de sonido

## 4. Criterios de Aceptación

- [ ] La matriz [7][7] se inicializa correctamente con celdas vacías por defecto
- [ ] `PieceData` contiene los campos: ID, Bando (enum), Tipo (enum), DefensaBase (int)
- [ ] Al llamar `SetupInitialBoard()`, las 20 fichas (10 por bando) se colocan en posiciones espejo
- [ ] Las columnas 1 y 7 en filas 2 y 6 están vacías
- [ ] Cada ficha tiene un ID único
- [ ] Las fichas se instancian visualmente en la escena de Unity
- [ ] El proyecto compila sin errores

## 5. UX/UI (si aplica)

- Crear prefabs básicos para cada tipo de ficha (sprites placeholder de colores)
- Las fichas Azules en la parte superior, Rojas en la inferior
- Las casillas vacías se ven como espacios sin ficha

## 6. Diseño Técnico

### Archivos a crear

- `Assets/Scripts/Game/PieceType.cs` — Enum de tipos
- `Assets/Scripts/Game/Team.cs` — Enum de bandos
- `Assets/Scripts/Game/PieceData.cs` — Struct/class de datos de ficha
- `Assets/Scripts/Game/Tile.cs` — Clase para cada casilla
- `Assets/Scripts/Game/BoardManager.cs` — Manager del tablero
- `Assets/Scripts/Game/InitialPosition.cs` — Configuración de posiciones iniciales
- Prefabs de fichas bajo `Assets/Prefabs/Pieces/`

### Estrategia

1. Crear enums para `PieceType` (Pawn, Ninja, Knight, Paladin) y `Team` (Blue, Red)
2. Crear `PieceData` como ScriptableObject serializable con ID, Team, Type, BaseDefense
3. Crear `Tile` class con estado de ocupación
4. Implementar `BoardManager` con matriz `Tile[7,7]`
5. Implementar `SetupInitialBoard()` usando un array de configuración
6. Instanciar GameObjects para cada ficha usando prefabs

### Nuevas dependencias

No. Solo uso básico de Unity (GameObject, Transform).

### Riesgos técnicos

- Coordenadas de instanciación visual: asegurar que row 0 = parte superior de la pantalla
- IDs únicos: usar contador estático o GUID

## 7. Dependencias

### Con otros specs

- Ninguno (es el spec base)

### Impacto en documentación

- [x] `docs/data-model/ERD.md` o `CLASS-DIAGRAM.md` (agregar entidades del juego)
- [ ] `docs/architecture/BACKEND-ARCHITECTURE.md`
- [ ] `docs/architecture/FRONTEND-ARCHITECTURE.md`
- [ ] `AGENTS.md`

## 8. Plan de Implementación

1. Crear carpeta `Assets/Scripts/Game/`
2. Crear `PieceType.cs` y `Team.cs` (enums)
3. Crear `PieceData.cs` (struct con campos)
4. Crear `Tile.cs` (casilla con referencia a ficha)
5. Crear `InitialPosition.cs` (configuración de posiciones iniciales)
6. Crear `BoardManager.cs` con matriz y setup
7. Crear prefabs visuales básicos para cada tipo de ficha
8. Montar escena de prueba con el BoardManager funcionando

## 9. Plan de Validación

- Comandos: Build en Unity (Ctrl+B) — sin errores
- Tests manuales:
  - Ejecutar escena y verificar que las 20 fichas aparecen en posiciones correctas
  - Verificar que columnas 1 y 7 de filas 2 y 6 están vacías
  - Verificar simetría espejo entre Azul y Rojo
- Evidencia esperada: Screenshot del tablero con fichas posicionadas

## 10. Rollback

Eliminar los archivos creados en `Assets/Scripts/Game/` y `Assets/Prefabs/Pieces/`. Revertir cambios en la escena.

## 11. Notas

- Los prefabs iniciales pueden ser simples sprites de colores (cuadrado azul/rojo con inicial del tipo)
- La fila 0 = Azul retaguardia, fila 6 = Rojo retaguardia
