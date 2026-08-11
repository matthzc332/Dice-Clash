# SPEC-015: Pantalla de Inicio

- ID: `015-main-menu`
- Rama: `spec/015-main-menu`
- Estado: `in-progress`
- Autor: AI
- Fecha: 2026-06-14

## 1. Contexto

El juego arranca directamente en la partida sin ninguna pantalla previa. Se necesita un menú principal que permita al jugador elegir especie y escenario antes de comenzar, además de dar una presentación visual al título.

## 2. Objetivo

Agregar un overlay de menú principal al inicio del juego con título, selector de especie, selector de escenario y botón de jugar.

## 3. Alcance

### Incluido

- Overlay fullscreen al iniciar el juego con fondo oscuro semitransparente
- Título "DICE CLASH TACTICS" usando PressStart2P
- Botón "PLAY" que inicia la partida
- Selector de especie (Human → Orc → Beastfolk) con botón de ciclo
- Selector de escenario (Human → Orc → Beastfolk) con botón de ciclo
- Música de menú mientras se muestra el overlay
- Al presionar PLAY: destruir overlay, inicializar BoardManager con especie/escenario seleccionados, empezar música de gameplay

### Excluido

- Animaciones de entrada/salida del menú (solo fade simple si es trivial)
- Botón de settings/opciones
- Selección de modo de juego (solo un modo)
- Pantalla de carga

## 4. Criterios de Aceptación

- [ ] Al iniciar el juego se ve el menú con el título y los botones
- [ ] Se puede cambiar la especie con el botón de especie
- [ ] Se puede cambiar el escenario con el botón de escenario
- [ ] Al presionar PLAY, el menú desaparece y comienza la partida con la especie y escenario seleccionados
- [ ] La música cambia de menú a gameplay al iniciar la partida

## 5. UX/UI

- Un solo overlay fullscreen con fondo negro semitransparente
- Título centrado arriba
- Botones de especie y escenario centrados en el medio
- Botón PLAY centrado abajo
- Fuente PressStart2P para todo el texto

## 6. Diseño Técnico

- Escena nueva: `Assets/Scenes/MainMenuScene.unity` (creada por Editor script `Assets/Editor/CreateMainMenuScene.cs`)
- Script nuevo: `Assets/Scripts/Game/MainMenuManager.cs` — crea todo el menú proceduralmente
- Script nuevo: `Assets/Scripts/Game/GameConfig.cs` — clase estática para pasar datos entre escenas
- Archivo eliminado: `Assets/Scripts/Game/MainMenuUI.cs` (reemplazado por MainMenuManager)
- Archivo modificado: `Assets/Scripts/Game/GameManager.cs` — lee `GameConfig.selectedSpecies/scenario` al iniciar
- Archivo modificado: `Assets/Editor/BuildScript.cs` — agrega MainMenuScene al build

### Flujo

1. Unity carga MainMenuScene → MainMenuManager crea el menú, música de menú
2. Jugador selecciona personaje y presiona PLAY
3. `GameConfig.Play(species)` guarda la selección y carga SampleScene
4. SampleScene → GameManager.Start() lee `GameConfig`, inicializa BoardManager con la especie/escenario, cambia a música de gameplay

## 7. Dependencias

Ninguna.

## 8. Plan de Implementación

1. Crear GameConfig.cs — datos entre escenas
2. Crear MainMenuManager.cs — menú procedural
3. Crear Editor script CreateMainMenuScene.cs
4. Modificar GameManager.cs — leer GameConfig
5. Eliminar MainMenuUI.cs
6. Actualizar BuildScript.cs

## 9. Plan de Validación

- Iniciar juego → ver menú
- Cambiar especie → verificar que las fichas usan el sprite correcto
- Cambiar escenario → verificar que el fondo y decoración cambian
- Presionar PLAY → verificar que comienza la partida normalmente
