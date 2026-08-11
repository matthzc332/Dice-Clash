# SPEC-033: Tutorial para nuevos usuarios

- ID: `033-tutorial`
- Rama: `spec/033-tutorial`
- Estado: `done`
- Autor: Matías Cabello
- Fecha: 2026-06-26

## 1. Contexto

El juego carece de tutorial. Los nuevos usuarios no tienen guía sobre selección de piezas, movimiento, combate o fin de turno. Esto afecta la retención (D1) porque los jugadores no entienden las mecánicas básicas. Se necesita un tutorial integrado que enseñe el flujo completo en un entorno controlado.

## 2. Objetivo

Crear un tutorial interactivo que lleve al jugador desde cero hasta completar una partida guiada, asegurando que entienda selección → movimiento → combate → fin de turno. Al finalizar, el jugador pasa directamente a la primera partida real (game hook).

## 3. Alcance

### Incluido

- Escena o modo de tutorial accesible desde el menú principal (botón "TUTORIAL" o primer inicio)
- Enemigos maniquíes de madera (sprites estáticos, sin IA, sin movimiento)
- Secuencia scriptada paso a paso:
  1. Explicar selección de pieza (clic en pieza azul → highlight)
  2. Mostrar movimientos válidos (tiles resaltados)
  3. Mover pieza a casilla resaltada
  4. Explicar combate (mover a casilla enemiga → resolver dados)
  5. Explicar fin de turno (botón End Turn)
  6. Turno del enemigo (maniquí no se mueve, solo animación de "espera")
  7. Repetir hasta eliminar todos los maniquíes
- **Power-ups**: Durante el tutorial aparecen paquetes aleatorios que el jugador puede recoger:
  - **Sacudida**: Empuja todas las piezas enemigas 1 casilla al azar
  - **Explosión**: Daña en área 3x3 alrededor del objetivo
  - **Bola de fuego** (Mago): Invoca un mago que lanza una bola de fuego al enemigo más cercano, eliminándolo al instante
  - **Rayos**: Golpea una fila completa de enemigos con daño aturdidor
- Cada power-up se explica con texto + demostración antes de que el jugador lo use
- Texto instructivo en parte superior de la pantalla
- Overlay de resalte en elementos UI relevantes (botón End Turn, piezas seleccionables, power-ups)
- Botón "SALTAR" para jugadores que ya conocen el juego
- Al completar: transición a primera partida real (humanos vs orcos, dificultad baja)

### Excluido

- IA avanzada o comportamiento reactivo de los maniquíes
- Sistema de recompensas por completar tutorial
- Tutorial rejugable desde el menú (solo aparece en primer inicio, con opción de saltar)
- Localización a otros idiomas

## 4. Criterios de Aceptación

- [ ] Un jugador nuevo puede completar el tutorial sin ayuda externa
- [ ] Todas las acciones están explicadas con texto instructivo visible
- [ ] El jugador no puede quedarse trabado (cada paso tiene una acción obligatoria)
- [ ] Al completar, se inicia una partida real automáticamente
- [ ] El botón "SALTAR" funciona y lleva al menú principal o a partida directa
- [ ] No hay errores de consola durante el tutorial

## 5. UX/UI

- **Pantallas impactadas**: Menú principal (nuevo botón "TUTORIAL"), nueva overlay de tutorial sobre el tablero
- **Estados**: 
  - Inicio del tutorial (texto de bienvenida)
  - Paso activo (instrucción + objetivo resaltado)
  - Feedback positivo (check + sonido al completar paso)
  - Tutorial completado (transición a partida real)
- **Accesibilidad**: Texto grande y contrastante, colores llamativos en elementos resaltados

## 6. Diseño Técnico

- **Archivos a modificar/crear**:
  - `Assets/Scripts/Game/TutorialManager.cs` — lógica principal del tutorial
  - `Assets/Scripts/Game/MainMenuManager.cs` — agregar botón "TUTORIAL"
  - `Assets/Scripts/Game/BoardManager.cs` — soporte para modo tutorial (maniquíes, sin IA)
  - `Assets/Scripts/Game/InputManager.cs` — restringir input durante tutorial
  - `Assets/Scripts/Game/PowerUpManager.cs` — sistema de power-ups (sacudida, explosión, bola de fuego, rayos)
  - `Assets/Resources/Sprites/Tutorial/` — sprites para maniquíes de madera
  - `Assets/Resources/Sprites/PowerUps/` — sprites para iconos/efectos de power-ups
- **Estrategia**: `TutorialManager` controla una máquina de estados con pasos secuenciales. Cada paso define qué input espera y qué feedback mostrar. `PowerUpManager` gestiona la lógica de cada power-up (área, daño, efectos visuales) y puede reutilizarse en el game hook y partidas normales.
- **Nuevas dependencias**: No
- **Riesgos técnicos**: 
  - El flujo de turnos debe pausarse durante las explicaciones
  - Asegurar que el jugador no pueda realizar acciones no previstas en el paso actual

## 7. Dependencias

### Con otros specs

- Este spec debe completarse antes o en paralelo con `034-game-hook`, ya que el hook ocurre inmediatamente después del tutorial.

### Impacto en documentación

- [ ] `docs/data-model/ERD.md`
- [ ] `docs/architecture/`
- [x] `AGENTS.md`

## 8. Plan de Implementación

1. Crear `PowerUpManager.cs` con sistema base de power-ups:
   - Sacudida: empuja enemigos 1 casilla aleatoria con animación de tremor
   - Explosión: daño en área 3x3 con partículas y humo
   - Bola de fuego: invoca un proyectil que busca al enemigo más cercano y lo elimina
   - Rayos: animación de rayo en fila con parpadeo de luz
2. Crear sprites/efectos visuales para cada power-up en `Resources/Sprites/PowerUps/`
3. Crear `TutorialManager.cs` con máquina de estados que integra pasos de power-up
4. Crear sprites de maniquí de madera (o reutilizar existentes con tinte gris/madera)
5. Modificar `BoardManager` para spawnear maniquíes en posiciones fijas
6. Modificar `InputManager` para filtrar inputs según el paso del tutorial
7. Agregar overlay de texto instructivo y resaltes UI
8. Agregar botón "TUTORIAL" en el menú principal
9. Integrar transición a partida real al completar
10. Probar flujo completo

## 9. Plan de Validación

- Comandos: `Ctrl+B` (Build Player) en Unity
- Tests manuales:
  - Completar tutorial completo sin saltar
  - Usar botón "SALTAR" en cada paso
  - Verificar que no hay acciones bloqueadas o imposibles
  - Confirmar transición a partida real al finalizar
- Evidencia esperada: Video del flujo completo del tutorial

## 10. Rollback

Revertir commits en la rama `spec/033-tutorial`.

## 11. Notas

- Decisión abierta: ¿mostrar el tutorial solo en primer inicio (PlayerPrefs) o siempre disponible? Por ahora, disponible desde menú principal, con flag de "ya completado" para no forzarlo.
- Los maniquíes pueden ser sprites existentes con color marrón/gris en lugar de assets nuevos.
