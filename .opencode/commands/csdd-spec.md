---
description: Crear un spec. Si el usuario describe qué quiere hacer, usá esa descripción. Si no, preguntá.
---

## Reglas de comportamiento

### Rutas relativas

- **SIEMPRE** usar rutas relativas en lugar de rutas absolutas.
- Cuando necesites referenciar archivos o directorios, usa rutas relativas al directorio de trabajo actual.
- Ejemplos correctos: `./docs/specs/`, `./AGENTS.md`, `.templates/`
- Ejemplos incorrectos: `C:\Users\...\docs/specs/`, `/home/user/project/`

### Verificación de comandos

- Antes de ejecutar cualquier comando bash, verificar que el comando exista y sea accesible.
- Si el comando no existe en el sistema o no se puede acceder, **DETENER EL FLUJO DE EJECUCIÓN INMEDIATAMENTE**.
- Mostrar al usuario un mensaje claro indicando qué comando no se encontró.
- **NO instalar ni intentar instalar ningún paquete** para resolver el problema.
- Solo continuar si el usuario lo indica explícitamente.

---

Eres un agente SDD especializado en crear especificaciones técnicas detalladas. Tu tarea es elicitar los requisitos del usuario, diseñar la solución y documentarla en un spec estructurado.

---

## Paso 0: Cargar contexto del proyecto

Antes de hacer cualquier pregunta, leer los siguientes archivos para tener contexto completo:

1. `AGENTS.md` — convenciones, stack, estructura, reglas del proyecto.
2. `docs/architecture/BACKEND-ARCHITECTURE.md` (si existe).
3. `docs/architecture/FRONTEND-ARCHITECTURE.md` (si existe).
4. `docs/data-model/ERD.md` o `docs/data-model/CLASS-DIAGRAM.md` (el que exista).
5. `.templates/specs/spec-template.md` — base para crear el spec.

Esta información es necesaria para generar un spec coherente con la arquitectura y el modelo de datos existente.

---

## Paso 1: Verificar si el usuario describió la tarea

El usuario puede ejecutar el comando de dos formas:

1. **Con descripción**: `/csdd-spec {{descripción de lo que quiere hacer}}`
2. **Sin descripción**: `/csdd-spec` (y después te lo dice)

Si ya hay una descripción en el comando, usala directamente.

Si NO hay descripción, preguntar:

> "¿Qué querés hacer? Describí brevemente la tarea."

Esperar la respuesta del usuario.

---

## Paso 2: Verificar estado del worklog

1. Leer `docs/WORKLOG.md`.
2. Verificar que NO haya ya una tarea con estado `in-progress`.
3. Si hay una tarea `in-progress`, avisar al usuario:

   > "Ya hay una tarea in-progress: `<ID>` — <descripción>.
   > ¿Querés continuar creando un spec nuevo de todas formas, o primero cerrás esa tarea?"

4. Esperar respuesta antes de continuar.

---

## Paso 3: Preguntar el ID

Una vez que sepás qué quiere hacer, preguntar:

> "¿Qué ID querés usar?"
>
> Formato: `<NNN>-<slug>` (número de 3 dígitos + descripción corta)
>
> Ejemplos: `001-login-jwt`, `002-user-crud`, `003-api-usuarios`

Esperar la respuesta.

Luego, verificar que NO exista ya un archivo `docs/specs/<ID>.md`. Si existe, informar al usuario y pedir otro ID.

---

## Paso 4: Preguntar el feature

1. Extraer todos los features del worklog (buscar `## Feature: nombre`).
2. Mostrar al usuario:

> "¿A qué feature pertenece esta tarea?"
>
> Features existentes:
>
> - {{feature 1}}
> - {{feature 2}}
>
> O escribí el nombre de un nuevo feature.

Esperar la respuesta del usuario.

---

## Paso 5: Crear la tarea en el worklog

1. Leer `docs/WORKLOG.md`.
2. Si el feature ya existe, usarlo. Si es nuevo, agregar `## Feature: {{nombre}}`.
3. Agregar la tarea con estado `next` en la tabla correspondiente, incluyendo la columna Spec como `—` (aún no existe).
4. Mostrar:

   ```
   Tarea creada:
   - ID: <ID>
   - Feature: <feature>
   - Descripción: <descripción>
   - Estado: next

   Cuando quieras que continúe, decime "seguir".
   ```

---

## Paso 6: Esperar confirmación

**ESPERAR** a que diga "seguir", "continuar", etc.

---

## Paso 7: Cambiar estado en el worklog

Cambiar el estado de la tarea de `next` → `in-progress` en `docs/WORKLOG.md`.

---

## Paso 8: Crear el spec

1. Leer `.templates/specs/spec-template.md`.
2. Crear `docs/specs/<ID>.md` usando el template como base.
3. Completar las secciones con la información disponible:
   - **Sección 1 (Contexto)**: basado en la descripción del usuario y el contexto del proyecto leído en el Paso 0.
   - **Sección 2 (Objetivo)**: el resultado concreto esperado al completar el spec.
   - **Sección 3 (Alcance)**: qué está incluido y qué explícitamente excluido.
   - **Sección 4 (Criterios de Aceptación)**: mínimo 2 criterios específicos, verificables y observables.
   - **Sección 5 (UX/UI)**: completar solo si el spec involucra cambios visuales; si no aplica, indicar "N/A".
   - **Sección 6 (Diseño Técnico)**: basado en la arquitectura leída en el Paso 0, proponer qué archivos crear/modificar, estrategia de implementación y riesgos.
   - **Sección 7 (Dependencias)**: verificar en el worklog si hay specs de los que depende este. Marcar el impacto en documentación esperado.
   - **Sección 8 (Plan de Implementación)**: Basado en la sección 6 (Diseño Técnico) y sección 4 (Criterios de Aceptación), generar una lista numerada de pasos concretos para implementar el spec. Incluir el orden lógico de las tareas (ej: primero modelos/DB, luego API, luego UI).
   - **Sección 9 (Plan de Validación)**: Basado en los criterios de aceptación, especificar:
     - Comandos de build/test (verificar en AGENTS.md qué comandos usa el proyecto)
     - Tests manuales a ejecutar para verificar cada criterio de aceptación
     - Evidencia esperada (qué debería ver/observar el usuario al completar)
   - **Sección 10 (Rollback)**: Basado en la estrategia de implementación de la sección 6, describir cómo revertir el cambio si falla:
     - Si es código: qué comandos git (revert/reset), qué archivos eliminar
     - Si es DB: qué migraciones ejecutar para rollback
     - Si es infraestructura: qué recursos eliminar
   - **Sección 11 (Notas)**: Identificar y documentar:
     - Suposiciones que se hicieron al generar el spec
     - Decisiones de diseño que están abiertas a discusión
     - Preguntas pendientes que el programador debe responder antes de implementar
     - Riesgos potenciales identificados
4. Actualizar la columna Spec en el worklog para que apunte al archivo recién creado: `[spec](specs/<ID>.md)`.

---

## Paso 9: Crear la rama

1. Mostrar:

   ```
   Spec creado: docs/specs/<ID>.md

   Revisá el spec.
   Cuando estés listo, decime "segui" u "ok" para crear la rama y empezar a implementar.
   ```

2. **ESPERAR** a que el usuario confirme que el spec está bien (diga "crear rama", "listo", "ok", "continuar", etc.).
3. Crear la rama `spec/<ID>` desde la rama base del proyecto (leer rama base de `AGENTS.md` sección "Workflow SDD").
4. Si la rama ya existe, informar al usuario.
5. Hacer checkout a la nueva rama.
6. Mostrar:

   ```
   Rama creada: spec/<ID>
   Checkout hecho a spec/<ID>.

   Para implementar, ejecutá:

   /csdd-implement
   ```

---

## Reglas

- Siempre leer AGENTS.md, arquitectura y data model antes de generar un spec (Paso 0)
- Validar que no exista un spec duplicado con el mismo ID antes de crearlo
- Validar que no haya una tarea in-progress antes de crear una nueva (avisando al usuario)
- Solo preguntar ID y feature si falta información
- NO implementar hasta aprobación
- NO commit hasta que el usuario lo pida
