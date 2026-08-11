# SPEC-024: Feedback del tester y ajustes

- ID: `024-tester-feedback`
- Rama: `spec/024-tester-feedback`
- Estado: `done`
- Autor: —
- Fecha: 2026-06-17

## 1. Contexto

El juego tiene una partida completa funcional (single-player vs IA) con menú, combate por dados, y pantalla de game over. Antes de expandir contenido (mundos, trofeos), se necesita pasar el build a testers, recopilar feedback, y aplicar correcciones de bugs, balance y pulido.

## 2. Objetivo

Distribuir el build actual a testers, recibir su feedback, y aplicar los ajustes necesarios hasta que el juego esté estable y divertido en el mundo humano.

## 3. Alcance

### Incluido

- Preparar y distribuir build a testers
- Recopilar y priorizar feedback recibido
- Corrección de bugs reportados
- Ajustes de balance (daño, probabilidades, movimiento)
- Mejoras de UX/UI según feedback
- Ajustes de IA si el tester reporta comportamiento anómalo
- Pulido visual y de sonido menor
- Corrección de errores de consola o warnings en Unity

### Excluido

- Nuevos mundos (Orco, Bestia) — serán specs separados
- Persistencia de trofeos — spec 020
- Integración con CrazyGames/Android
- Nuevas mecánicas o habilidades no contempladas en specs anteriores
- Refactors mayores fuera del alcance de los bugs reportados

## 4. Criterios de Aceptación

- [x] Al menos 1 tester ha jugado una partida completa y entregado feedback
- [x] Todos los bugs reportados están documentados y corregidos o tienen un plan de acción
- [x] No hay errores fatales (crash, freeze, loop infinito) en partidas normales
- [x] El build compila sin errores

## 5. UX/UI

- Pantallas/Rutas impactadas: cualquiera según feedback recibido
- Estados: loading, empty, error, success — aplicar ajustes según reportes

## 6. Diseño Técnico

- Archivos a modificar: según bugs reportados
- Estrategia de implementación: recopilar feedback → priorizar → corregir incrementalmente
- Nuevas dependencias: no
- Riesgos técnicos: feedback vago o contradictorio; priorizar lo que afecte la jugabilidad central

## 7. Dependencias

### Con otros specs

Ninguna.

### Impacto en documentación

- [ ] `docs/data-model/ERD.md`
- [ ] `docs/architecture/`
- [x] `AGENTS.md` — puede requerir actualización si se ajustan comportamientos

## 8. Plan de Implementación

1. Build y distribución a tester
2. Esperar feedback
3. Priorizar issues reportados
4. Corregir bugs y aplicar ajustes
5. Build actualizado
6. Repetir según sea necesario

## 9. Plan de Validación

- Comandos: `Ctrl+B` (Build Player) en Unity
- Tests manuales:
  - Partida completa Azul vs IA Roja sin crashes
  - Probar todas las interacciones de UI (menú, game over, botones)
  - Verificar que el feedback reportado está resuelto
- Evidencia esperada: lista de issues cerrados

## 10. Rollback

Revertir commits en la rama `spec/024-tester-feedback`.

## 11. Notas

- Spec deliberadamente abierto para cubrir cualquier ajuste que surja del testing.
- Cada corrección específica debe documentarse en los detalles del WORKLOG.
