# SPEC-045: Campaña, Economía y Progresión Completa

- ID: `045-campaign-progression`
- Rama: `spec/045-campaign-progression`
- Estado: `draft`
- Autor: `opencode`
- Fecha: `2026-07-14`

## 1. Contexto

El juego actualmente tiene un modo arcade con 3 mundos (Human, Orc, Beastfolk) sin progresión real. El usuario pide una campaña de 22 niveles con 4 copas, un powerup nuevo (MAGIC!), 3 obstáculos nuevos (Celda destruida, Pegote, Mina), un sistema de economía con oro, cofres e insignias, y una "nueva raza" desbloqueable.

## 2. Objetivo

Implementar una campaña de 22 niveles con progresión de dificultad, powerups/obstáculos que se desbloquean por nivel, economía de oro con cofres gacha, libro de insignias, y ranking libre post-campaña.

## 3. Alcance

### Incluido

- **Powerup MAGIC!** (nuevo): Convierte una unidad enemiga en Peón
- **Obstáculo Celda destruida**: Casilla que se destruye en tiempo real, pieza cae al vacío
- **Obstáculo Pegote**: Pieza queda atrapada 1 turno al pasar
- **Obstáculo Mina**: Explosión 3×3 al tocar, mata a todas las piezas (aliadas y enemigas)
- **Campaña 22 niveles**: 4 copas con 5 niveles cada una (niveles 3-22)
- **Progresión de powerups/obstáculos por nivel**: Cada nivel introduce combinaciones específicas
- **Nombres de ejércitos**: 22 nombres contextuales con iconos de raza
- **Nueva raza**: Desbloqueable en Copa 4 (nivel 20+)
- **Economía de oro**: Se gasta para usar powerups y para entrar a partidas
- **Daily bonus**: Recompensa de oro diaria
- **Cofres**: 3 insignias random, timer 8hs, 1-2 slots máximo
- **35 insignias**: 22 de campaña + 13 exclusivas de cofres
- **Libro de victorias**: UI para ver insignias coleccionadas
- **Indicador de nombre/ejército en combate**: Nombre del oponente + icono de raza
- **Animación de apertura de cofre**: Flash + reveal secuencial + "DUPLICADA!"
- **Ranking libre post-campaña**: Powerups/obstáculos aleatorios por raza

### Excluido

- Multijugador online
- Tienda real de monedas
- Modo torneo
- Animaciones 3D
- Música/soundtrack nuevos

## 4. Criterios de Aceptación

- [ ] Powerup MAGIC! funciona: convierte enemigo no-Peón en Peón
- [ ] Celda destruida se destruye con animación, pieza cae y muere
- [ ] Pegote atrapa pieza 1 turno, se libera automáticamente
- [ ] Mina explota 3×3 al tocar, mata todas las piezas en radio
- [ ] Campaña tiene 22 niveles accesibles secuencialmente
- [ ] Cada nivel tiene powerups/obstáculos correctos según tabla
- [ ] Nombres de ejércitos se muestran en combate
- [ ] Oro se gasta al usar powerups
- [ ] Oro se gasta al iniciar partida
- [ ] Daily bonus entrega oro cada 24h
- [ ] Cofres se abren con animación correcta
- [ ] 35 insignias visibles en libro de victorias
- [ ] Ranking libre funciona post-campaña
- [ ] Build pasa sin errores

## 5. UX/UI

### Pantallas impactadas
- **MainMenu**: Agregar botón "CAMPAÑA", mostrar copa actual, daily bonus popup
- **Campaña**: Mapa de niveles 1-22 con copas, nivel actual desbloqueado
- **Combate**: Indicador nombre/ejército en parte superior
- **Gameplay**: Powerups en UI, obstáculos visuales en tablero
- **GameOver**: Mostrar insignia ganada si aplica, cofre si ganó
- **Libro de victorias**: Grid de 35 insignias, coleccionadas vs pendientes
- **Cofre**: Animación de apertura con reveal secuencial
- **Tienda/Powerups**: Comprar powerups con oro antes de entrar a nivel

### Estados
- **Campaña**: Mapa con niveles bloqueados/desbloqueados/completados
- **Combate**: Nombre oponente + icono raza en HUD
- **Cofre**: Timer de apertura, slots disponibles, animación reveal
- **Daily**: Popup al iniciar si no reclamado hoy

## 6. Diseño Técnico

### Archivos a crear/modificar
- `Assets/Scripts/Game/CampaignManager.cs` — Nuevo: maneja progresión 22 niveles
- `Assets/Scripts/Game/ChestManager.cs` — Nuevo: cofres, timer, apertura
- `Assets/Scripts/Game/InsigniaManager.cs` — Nuevo: 35 insignias, libro
- `Assets/Scripts/Game/EconomyManager.cs` — Nuevo: oro, daily bonus, costos
- `Assets/Scripts/Game/ObstacleManager.cs` — Modificar: agregar celda destruida, pegote, mina
- `Assets/Scripts/Game/PowerUpManager.cs` — Modificar: agregar MAGIC!
- `Assets/Scripts/Game/BoardManager.cs` — Modificar: obstáculos en tablero
- `Assets/Scripts/Game/BattleResultUI.cs` — Modificar: indicador nombre/ejército
- `Assets/Scripts/Game/GameOverUI.cs` — Modificar: insignia ganada, cofre
- `Assets/Scripts/Game/MainMenuManager.cs` — Modificar: botón campaña, daily bonus

### Datos
- `Resources/Data/CampaignData.json` — 22 niveles con powerups/obstáculos/raza/nombre
- `Resources/Data/InsigniaData.json` — 35 insignias con nombres/iconos
- `Resources/Data/EconomyData.json` — Costos de entrada, costos de powerups, daily amounts

### Estrategia
1. Crear data models (CampaignLevel, Insignia, Economy)
2. Implementar EconomyManager (oro, daily, costos)
3. Implementar CampaignManager (progresión niveles)
4. Agregar MAGIC! a PowerUpManager
5. Agregar obstáculos nuevos (celda destruida, pegote, mina)
6. Crear CampaignUI (mapa de niveles)
7. Crear ChestManager + ChestUI (cofres + animación)
8. Crear InsigniaManager + InsigniaUI (libro de victorias)
9. Agregar indicador nombre/ejército en combate
10. Integrar ranking libre post-campaña

### Nuevas dependencias
- Ninguna externa (solo Unity built-in)

### Riesgos técnicos
- Complejidad de CampaignManager con 22 niveles
- Sincronización de timer de cofres con PlayerPrefs
- Balance de economía (costos vs recompensas)
- La "nueva raza" requiere sprites completos

## 7. Dependencias

### Con otros specs
- Ninguno (primer spec de esta feature)

### Impacto en documentación
- [x] `AGENTS.md` — Actualizar con nueva feature
- [ ] `docs/data-model/ERD.md` — Agregar modelos de campaña/insignias/economía

## 8. Plan de Implementación

### Fase 1: Data Models (1-2h)
1. Crear `CampaignData.json` con 22 niveles
2. Crear `InsigniaData.json` con 35 insignias
3. Crear `EconomyData.json` con costos
4. Crear data classes en C#

### Fase 2: Economía (2-3h)
1. Implementar `EconomyManager.cs` (oro, daily, costos)
2. Integrar con partidas existentes
3. UI de daily bonus popup

### Fase 3: Powerup MAGIC! (1h)
1. Agregar tipo MAGIC a PowerUpManager
2. Lógica de conversión a Peón
3. Efectos visuales

### Fase 4: Obstáculos nuevos (2-3h)
1. Celda destruida (timer + animación + muerte)
2. Pegote (trampa 1 turno)
3. Mina (explosión 3×3)
4. Integrar con BoardManager

### Fase 5: Campaña (3-4h)
1. CampaignManager con progresión
2. CampaignUI (mapa de niveles)
3. Nombres de ejércitos en combate
4. Integración con GameManager

### Fase 6: Cofres + Insignias (3-4h)
1. ChestManager (slots, timer, apertura)
2. ChestUI (animación reveal)
3. InsigniaManager (colección)
4. InsigniaUI (libro de victorias)

### Fase 7: Ranking libre (1-2h)
1. Modo post-campaña
2. Powerups/obstáculos aleatorios por raza
3. UI de selección

### Fase 8: Polish (2-3h)
1. Balance de economía
2. Animaciones y transiciones
3. Testing completo

## 9. Plan de Validación

- Comandos: Unity Build Player
- Tests manuales:
  - Completar nivel 1-22 secuencialmente
  - Verificar powerups/obstáculos por nivel
  - Abrir cofres y verificar insignias
  - Daily bonus funciona
  - Oro se gasta correctamente
  - Ranking libre accesible post-campaña
  - Indicador nombre/ejército visible
- Evidencia: Screenshots de cada pantalla, log de progresión

## 10. Rollback

Cada fase es independiente. Si una falla, se puede deshabilitar sin afectar las demás. Los datos se guardan en PlayerPrefs, se pueden resetear con Ctrl+R.

## 11. Notas

- La "nueva raza" requiere sprites y datos que aún no existen. Se puede usar placeholder (Beastfolk o color diferente) hasta que estén listos.
- El balance de economía (costos de powerups vs oro ganado) necesita iteración con testing.
- Los 22 nombres de ejércitos ya están definidos en el documento del usuario.
- Las 35 insignias: 22 de campaña (una por nivel 3-22) + 13 exclusivas de cofres (rangos 23-35).
- Timer de cofres: 8 horas desde apertura. Se guarda timestamp en PlayerPrefs.
- Slots de cofre: máximo 2. Si ganas con slots llenos, cofre se pierde.
- Daily bonus: se entrega al iniciar juego, resetea cada 24h.
- Ranking libre: desbloqueado al completar campaña (nivel 22).
- POWERUP MAGIC! convierte unidad enemiga (que no sea Peón) en Peón. No afecta Peones existentes.
- Celda destruida: tiene timer visual (barra que se agota). Cuando llega a 0, casilla se destruye con animación. Pieza en esa casilla muere.
- Pegote: pieza que pasa por encima queda atrapada. Siguiente turno no se puede mover. Se libera automáticamente.
- Mina: al mover pieza a casilla con mina, explota 3×3. Todas las piezas en radio mueren (incluyendo la que tocó).
