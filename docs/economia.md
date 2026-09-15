# Sistema Económico — Diseño y Balance

> Documento de revisión del sistema de oro. Todos los valores viven en datos (JSON), no en código.

## 1. Objetivos de diseño

1. **Onboarding gratis e inmediato**: el jugador puede empezar a jugar sin pagar, el ticket FREE (0g) existe siempre.
2. **El ticket PREMIUM no es infinito**: para jugar con power-ups el jugador debe volver al juego: reclama el bonus diario y/o abre cofres.
3. **Primeros niveles generosos**: conseguir oro al inicio es fácil para que el PREMIUM se sienta accesible y no frustre.
4. **La campaña paga una sola vez**; los cofres + bonus diario son el motor de repetición y de ranked.

## 2. Fuentes de oro

| Fuente | Valor | Cadencia | Una vez / repetible |
| --- | --- | --- | --- |
| Oro inicial | 100 | instalación | una vez |
| Tutorial | +150 | al terminar | una vez (`TutorialRewardClaimed`) |
| Campaña (victorias) | 20–100 por nivel | por victoria | **solo la 1ª victoria** de cada nivel (rejugar NO da oro) |
| Cofre | 50 | drop 40/55/70% según tramo, apertura 8h reales, máx 2 slots | repetible (limitado por tiempo) |
| Bonus diario | 25/30/35/40/50/60/75 | 1 al día, racha 7 (si fallas >48h la racha vuelve a 1) | repetible |

### Recompensas de campaña (tras rebalanceo)

| Nivel | Oro |
| --- | --- |
| 1 (Training Grounds) | 20 |
| 2 (First Blood) | 25 |
| 3 (The Garrison) | 25 |
| 4 | 25 |
| 5 | 30 |
| 6 | 35 |
| 7 | 50 |
| 8 | 30 |
| 9 | 35 |
| 10 | 40 |
| 11 | 45 |
| 12 | 60 |
| 13 | 45 |
| 14 | 50 |
| 15 | 55 |
| 16 | 60 |
| 17 | 75 |
| 18 | 60 |
| 19 | 65 |
| 20 | 70 |
| 21 | 80 |
| 22 | 100 |

Total campaña completa (1ª victoria de los 22 niveles): **1080g**.

### Por copa

| Copa | Niveles | Total oro |
| --- | --- | --- |
| Cup 1 | 3–7 | 165 |
| Cup 2 | 8–12 | 210 |
| Cup 3 | 13–17 | 285 |
| Cup 4 | 18–22 | 375 |

## 3. Gastos de oro

| Concepto | Costo | Notas |
| --- | --- | --- |
| Ticket PREMIUM campaña | **5/10/20/30** | por nivel jugado, según la copa del nivel (Cup1=5, Cup2=10, Cup3=20, Cup4=30); Cup0 (niveles 1-2) gratis. Cableado en `ModeSelectionUI` |
| Ticket FREE campaña | 0 | el jugador juega sin power-ups (los enemigos sí) |
| Ranked CON power-ups | 30 | por partida — devuelve **15g** por victoria |
| Ranked SIN power-ups | 15 | por partida — devuelve **10g** por victoria |
| Rejugada de nivel completo en FREE | 0 | migaja de **5g** por victoria en rejugada |
| Power-ups / MAGIC | 0 | quitado por completo (087) |
| Reroll de cofre | 5 | config existe, no implementado |

Total de entradas de la campaña en PREMIUM (una vuelta completa): **325g** (25+50+100+150).

Ranked reparte **10-15g por victoria**: ya no es un pozo sin retorno, pero solo con rachas sostenidas cubre la entrada.

## 4. Matemática «sin boot» (jugador nuevo real)

Sin el boot de expo (que daba 500g + tutorial marcado + niveles 1-2 completos), el jugador nuevo:

1. **Instala** → 100g.
2. **Tutorial** → +150g → **250g**.
3. **Campaña PRIMERA VUELTA en PREMIUM** (325g de entrada por copas) → +1080g de victorias → **neto +755g**.
4. **Cofres** en el camino: ≈ 11-12 esperados (≈590g) pero limitados por slots/8h.
5. Botín final realista tras la campaña en premium: **≈ 950-1450g** (según cofres abiertos y mis jugadas de ranked intermedias).

El jugador que juega en FREE (0g de entrada) acumula aún más: la 1ª victoria de cada nivel es oro puro (+1080) y la rejugada de un nivel completo da +5g de migaja.

**Conclusión del cálculo**: la campaña por sí sola es **superavitaria** (el premium se "autofinancia" en la primera vuelta). El cuello de botella se suaviza cuando el jugador:
- rejuega niveles completos en FREE (+5g de migaja), o
- juega ranked (30/15 de entrada, 10-15g de vuelta por victoria).

Ahí entra el **loop diario**:

| Día típico | Oro |
| --- | --- |
| Bonus diario | 25-75 |
| 1-2 cofres (8h) | 50-100 |
| **Total disponible** | **≈ 75-175g/día** |
| Partidas PREMIUM que financia | ≈ 7-35 (según copa, a 5-30g) |

## 5. Rebalanceo aplicado (para Claudio)

| Cambio | Antes | Después | Motivación |
| --- | --- | --- | --- |
| L1 goldReward | 10 | 20 | Primer nivel neto 0 en premium (-20+20), ganancia pura en free |
| L2 goldReward | 15 | 25 | Primeros niveles = "oro fácil" para seguir jugando |
| L3 goldReward | 20 | 25 | Igualar mínimo de cobertura del premium |

Total campaña: 1055 → **1080g** (+25).

Ajustes adicionales aprobados (misma iteración):

| Cambio | Antes | Después | Motivación |
| --- | --- | --- | --- |
| Ticket PREMIUM campaña | plano 20 | **por copa 5/10/20/30** | dificultad creciente; Cup0 free para no frenar al nuevo jugador |
| Reward ranked | 0 | **+15 CON / +10 SIN** | ranked deja de ser pozo sin retorno |
| Rejugada nivel completo en FREE | 0 | **+5g** | migaja para no abandonar el contenido completado |

No se tocan: startingGold 100, tutorial +150, ranked entradas 30/15, cofre 50, daily 25-75. Así se preserva el objetivo 2 (el premium requiere cofre/diario una vez agotada la campaña).

## 6. Puntos abiertos a decidir

- **Boot de expo**: `Assets/Resources/ExpoBuild.txt` activa `ExpoConfig.Enabled` (editor y el build de evento: 500g iniciales + tutorial saltado + niveles 1-2 completos). Para el build de producción/tutorial **hay que borrar el archivo**.
- **Feedback visual del oro** (implementado): el HUD muestra separador de miles con punto, el texto hace count-up animado 0.5s y aparece un popup flotante "+N" sobre el panel al recibir oro. Pendiente de revisión visual en Unity.
- **`levelEntryCosts` cableado** (sí): el PREMIUM de campaña ahora cobra 5/10/20/30 por copa según el nivel destino (`GetCampaignEntryCost()` en `ModeSelectionUI`). El ticket FREE sigue en 0.

## 7. Archivos de datos

- `Assets/Resources/Data/EconomyData.json` — startingGold, dailyBonus, powerupCosts, levelEntryCosts, chestConfig, rerollCost.
- `Assets/Resources/Data/CampaignData.json` — goldReward por nivel.
- `Assets/Scripts/Game/EconomyManager.cs` — TotalGold/SpendGold/AddGold, bonus diario (racha ≤7, reset >48h), `FormatGold` (separador de miles), count-up animado y popup "+N".
- `Assets/Scripts/Game/EconomyConfig.cs` — carga del JSON y getters (`GetLevelEntryCost(cupId)`).
- `Assets/Scripts/Game/ChestManager.cs` — BASE_GOLD=50, 2 slots, 8h.
- `Assets/Scripts/Game/ModeSelectionUI.cs` — costos de tickets (FREE 0 / PREMIUM campaña por copa vía `GetCampaignEntryCost()`, ranked 30/15).
- `Assets/Scripts/Game/ScoreboardUI.cs` — reward ranked (+15/+10) y migaja de rejugada FREE (+5).
- `Assets/Scripts/Game/TutorialRewardUI.cs` — +150 oro del tutorial.