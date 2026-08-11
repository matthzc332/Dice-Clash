# SPEC-012: Orcos y Decoración del Tablero

- ID: `012-orcs-and-decor`
- Rama: `spec/012-orcs-and-decor`
- Estado: `in-progress`
- Autor: IA
- Fecha: 2026-06-10

## 1. Contexto

El juego usa sprites de caballeros humanos para todas las piezas. Se quiere un segundo set de sprites con temática orco, y decoración arquitectónica alrededor del tablero.

## 2. Objetivo

- Agregar sistema de themes de sprites
- Generar sprites de orcos para las 4 clases
- Añadir columnas y vigas de piedra alrededor del perímetro del tablero

## 3. Alcance

### Incluido

- Reorganizar sprites en carpetas por tema: `Sprites/{theme}/Pieces/`, `Floor/`, `Background/`, `Decor/`
- Mover sprites de personajes existentes a `Human/Pieces/` y `Orc/Pieces/`
- Mover fondo a `Human/Background/`
- Copiar baldosas de suelo a `Human/Floor/` y `Orc/Floor/`
- Copiar decoración compartida a `Human/Decor/` y `Orc/Decor/`
- Banderas (BlueFlag, RedFlag) se mantienen en `Sprites/Decor/` compartidas
- Actualizar BoardManager.cs para cargar recursos desde `Sprites/{theme}/...` con fallback a carpetas compartidas
- Crear carpeta `Beastfolk/` con misma estructura (vacía, lista para sprites)

### Excluido

- Menú de selección de tema (se hará en otro spec)
- Cambios en gameplay, habilidades, stats o layout del tablero
- Nuevos sonidos o animaciones para orcos

## 4. Criterios de Aceptación

- [x] Sprites organizados por tema: `Human/`, `Orc/`, `Beastfolk/`
- [x] Cada tema tiene carpetas: `Pieces/`, `Floor/`, `Background/`, `Decor/`
- [x] Los sprites existentes (Human y Orc) movidos a sus carpetas temáticas
- [x] Las piezas se cargan desde `Sprites/{theme}/Pieces/` con fallback a `Human`
- [x] El fondo se carga desde `Sprites/{theme}/Background/{theme}` con fallback
- [x] Las baldosas se cargan desde `Sprites/{theme}/Floor/` con fallback a `Sprites/Floor/`
- [x] La decoración se carga desde `Sprites/{theme}/Decor/` con fallback a `Sprites/Decor/`
- [x] Las banderas (BlueFlag, RedFlag) se mantienen compartidas en `Sprites/Decor/`
- [x] Banderas iguales para todos los temas
- [x] El proyecto compila sin errores

## 5. Diseño Técnico

### Estructura de carpetas

```
Resources/Sprites/
├── Human/                          ← tema humano
│   ├── Pieces/                     ← sprites de personajes
│   │   ├── (Front sprites)
│   │   └── Back/                   ← sprites vista trasera
│   ├── Floor/                      ← baldosas del tablero
│   ├── Background/                 ← fondo de escenario
│   └── Decor/                      ← decoración (farol, gárgola, etc.)
├── Orc/                            ← tema orco
│   ├── Pieces/
│   │   ├── (Front sprites)
│   │   └── Back/
│   ├── Floor/
│   ├── Background/
│   └── Decor/
├── Beastfolk/                      ← tema beastfolk (furros)
│   ├── Pieces/
│   ├── Floor/
│   ├── Background/
│   └── Decor/
├── Decor/                          ← decoración compartida (banderas, dados, etc.)
├── Floor/                          ← suelo compartido (fallback)
├── Card/
├── Dice/
├── Efect/
└── Win/
```

### BoardManager.cs

- Nueva propiedad: `public string theme = "Orc";`
- `GetSprite()` ahora busca en `Sprites/Pieces/{theme}/...`
- Fallback: si no encuentra en el theme actual, busca en Human

### Decoración

- Crear sprites procedurales de piedra (ladrillos grises)
- Columnas: rectángulos verticales ~0.3×1.5 units
- Vigas: rectángulos horizontales/verticales ~0.2×7 units
- Posiciones calculadas desde CellToWorld de las esquinas

```
    ┌────[viga sup]────┐
    │                    │
 [col izq]  TABLERO  [col der]
    │                    │
    └────[viga inf]────┘
```

## 6. Dependencias

- Ninguna

## 7. Plan de Validación

Test manual: abrir escena, verificar orcos visibles, columnas y vigas presentes.

## 8. Rollback

Revertir cambios en BoardManager.cs, restaurar sprites a la raíz de Pieces/.
