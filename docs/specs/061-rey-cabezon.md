# SPEC-061: Rey Cabezón — Jefes de Copa

- ID: `061-rey-cabezon`
- Rama: `spec/061-rey-cabezon`
- Estado: `draft`
- Autor: ox-alpha
- Fecha: 2026-08-24

## 1. Contexto

Presentación en Sanra Game Fest (5-sep). Se necesita un contenido destacado: un jefe por copa de campaña que dé un momento memorable. El tester/diseñor propuso el "Rey Cabezón" con duelos de dados prolongados, fases y animaciones nuevas de power-ups.

## 2. Objetivo

Cuatro batallas de jefe (una al final de cada copa) contra el Rey Cabezón: estático, ocupa 2×2 casillas, con su ejército (sin Paladines), 5 corazones, fase furiosa con d20, inmune a power-ups, HUD lateral con cara y corazones, más animaciones de puño y mago para power-ups existentes.

## 3. Alcance

### Incluido

- Niveles 23–26 en `CampaignData.json` (flag `isBoss`), uno al final de cada copa, con nombre propio para `EnemyBanner`.
- Spawn del rey: celda lógica ancla + 3 casillas adyacentes marcadas ocupadas (bloquean paso, no targeteables individualmente). Atacar cualquiera de las 4 resuelve contra el rey.
- Ejército enemigo normal junto al rey, **sin Paladines**, IA existente lo mueve.
- **5 corazones**: duelo ganado = -1 corazón y la pieza atacante muere igual (salvo el golpe final n°5); duelo perdido = muere la pieza atacante.
- El rey es **estático** (defiende su trono).
- **Fase 2** (≤2 corazones): el rey tira **d20** en vez de 2d6+DEF; cara furiosa.
- **Inmunidad a power-ups**: no es targeteable ni le afectan efectos (Shake/Explosion/Fireball/Lightning/MAGIC).
- **HUD derecho**: cara del rey según HP (feliz → preocupado → furioso → desesperado) + corazones.
- Victoria: matar al rey gana aunque queden piezas enemigas (override de condición actual). Derrota normal si mueren todas las azules o timer.
- Recompensas normales de campaña + completa la copa.
- **Animación puño**: al lanzar power-up ofensivo, un puño golpea el tablero.
- **Animación mago**: MAGIC ahora muestra círculo mágico → mago aparece → poder → círculo → desaparece.

### Excluido

- Movimiento del rey (queda estático para balancear antes del evento).
- Nuevos power-ups con mecánica propia (puño/mago son animaciones de existentes).
- Sprites finales (los genera el diseñador; primero placeholders procedurales).

## 4. Criterios de Aceptación

- [ ] Cada copa termina en una batalla contra el rey correspondiente.
- [ ] El rey ocupa 2×2 y bloquea el paso; ninguna pieza pisa sus 4 casillas.
- [ ] Cada duelo ganado quita 1 corazón; la pieza atacante muere salvo en el golpe final.
- [ ] Duelo perdido mata la pieza atacante sin dañar al rey.
- [ ] Con ≤2 corazones el rey tira d20 (visible en BattleResultUI) y su HUD muestra cara furiosa.
- [ ] Ningún power-up puede seleccionar o afectar las casillas del rey.
- [ ] Matar al rey dispara ScoreboardUI → victoria aunque queden enemigos.
- [ ] HUD derecho muestra cara + corazones sincronizados con el HP real.
- [ ] Puño aparece al lanzar power-up ofensivo; mago+círculo en MAGIC.

## 5. UX/UI (si aplica)

- Pantallas: tablero de los niveles 23–26, BattleResultUI (d20), HUD del jefe, EnemyBanner.
- Arte esperado (rutas en Resources, el diseñador provee PNGs):
  - `Sprites/Boss/King_{Human|Orc|Beast|Nigromante}` cuerpo completo.
  - `Sprites/Boss/KingFace_{Happy|Worried|Angry|Desperate}`.
  - `Sprites/PowerUps/Efect/Puno{1|2|3}` frames de golpe.
  - `Sprites/PowerUps/Efect/Circulo{1|2}` + `Mago{1|2|3}` frames.

## 6. Diseño Técnico

- `PieceData`: flag `isBoss` (+ `bossHp`, `bossMaxHp`).
- `CampaignLevel`: campos `isBoss`, `kingName`; niveles 23–26 con ejércitos sin paladines.
- `BoardManager`: spawn multicelda (ocupación de 4 celdas), reglas de muerte del atacante, decremento de HP, override de condición de victoria, escala visual del sprite del rey cubriendo 2×2.
- `CombatManager.Resolve`: rama defensor jefe — fase 1 usa DEF normal; fase 2 reemplaza 2d6 por d20.
- `PowerUpManager`: exclusión de casillas del jefe en targeting/efectos.
- Nuevo `BossHUD` (canvas overlay derecho): cara + corazones.
- `PowerUpManager`: corrutinas `FistSlam()` y `MageRitual()`.
- `EnemyBanner`: texto desde `kingName`.

## 7. Dependencias

### Con otros specs

- SPEC-060 (rama base limpia antes de empezar el jefe).

### Impacto en documentación

- [x] `AGENTS.md` (al cerrar)

## 8. Plan de Implementación

1. Datos + spawn multicelda + ejército sin paladines.
2. Combate (HP, muerte del atacante, d20 fase 2) + victoria override.
3. BossHUD (caras + corazones).
4. Inmunidad a power-ups + animaciones puño/mago.
5. Balance + build EXE para el evento.

## 9. Plan de Validación

- Compilar en Unity.
- Manual: jugar cada nivel de jefe (con TestButtons LVL 23-26); verificar reglas de muerte, d20, inmunidad, HUD, victoria.
- AutoPlayManager smoke test para regresión de partidas normales.

## 10. Rollback

Revertir commits de la rama `spec/061-rey-cabezon`.

## 11. Notas

- Fecha límite dura: 5-sep (Sanra Game Fest). Si el tiempo aprieta, las animaciones puño/mago son recortables sin afectar el core.
- Balance inicial sugerido: ejército del rey crece por copa (copa1: rey+3 … copa4: rey+6).
