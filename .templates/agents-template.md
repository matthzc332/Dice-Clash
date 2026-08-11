# Project Instructions

> Instrucciones para agentes AI y desarrolladores. Este es el documento principal del proyecto.

---

## 1. Proyecto

**{{nombre del proyecto}}** — {{descripción breve en una línea}}.

### Stack

| Capa     | Tecnología                 |
| -------- | -------------------------- |
| Backend  | {{ej: Python + FastAPI}}   |
| Frontend | {{ej: React + TypeScript}} |
| Database | {{ej: PostgreSQL}}         |
| Infra    | {{ej: Docker}}             |

<!-- Eliminar las filas que no apliquen -->

### Estructura

```
{{árbol de carpetas relevante del proyecto}}
```

### Quick Start

```bash
{{pasos mínimos para levantar el proyecto localmente}}
```

### Idioma

- Documentación: **{{es / en}}**
- Código y términos técnicos: inglés

---

## 2. Workflow SDD

Metodología: **Spec-Driven Development**. Cada unidad de trabajo es un spec. No se implementa nada sin spec previo.

Rama base: `{{main / develop}}`

### Ciclo de vida de un spec

1. Tomar el item con estado `next` en `docs/WORKLOG.md`.
2. Crear `docs/specs/{{NNN}}-{{slug}}.md` usando `.templates/specs/spec-template.md` como base.
3. Cambiar el estado del item a `in-progress` en el worklog.
4. Crear la rama `spec/{{NNN}}-{{slug}}` e implementar solo el alcance definido.
5. Verificar localmente (tests / build).
6. Abrir PR referenciando el spec. **Esperar aprobación antes de mergear.**
7. Al hacer merge, cambiar estado a `done` y promover el siguiente item de `backlog` a `next`.

---

## 3. Convenciones

### Specs

- Archivo: `docs/specs/<NNN>-<slug>.md`
- Base: `.templates/specs/spec-template.md`
- Contenido mínimo: Contexto · Objetivo · Alcance (in/out) · Criterios de aceptación

### Ramas

- Formato: `spec/<NNN>-<slug>`
- Un spec por rama. No mezclar cambios de distintos specs en la misma rama.
- Si el alcance cambia, actualizar el spec antes de modificar el código.

### Commits

- **No commitear hasta que el usuario lo solicite explícitamente.**
- Formato: `<tipo>(<alcance>): <mensaje>`
- Tipos válidos: `feat` · `fix` · `refactor` · `docs` · `chore` · `test`
- Incluir referencia al spec en el cuerpo del commit cuando aplique.

### Pull Requests

- Título: `[SPEC <NNN>] <resumen>`
- Target: `{{rama base}}`
- Debe incluir: enlace al spec · checklist de criterios de aceptación · evidencia de verificación · screenshots si hay cambios visuales.

### Code Style

<!-- Completar con las convenciones del proyecto -->

**{{Lenguaje principal}}:**

- {{convención 1}}
- {{convención 2}}

---

## 4. Reglas

### Para implementar

- Trabajar solo dentro del alcance del spec activo. Sin refactors amplios fuera del spec.
- Preservar la estructura existente del proyecto.
- No agregar dependencias nuevas salvo que el spec lo indique explícitamente.
- Antes de crear o modificar modelos, revisar `docs/data-model/ERD.md`. No agregar campos que no estén definidos allí.
- Si el spec tiene definiciones ambiguas o incompletas, **pausar y consultar** antes de implementar.

### Prohibido

- Implementar sin spec.
- Commitear o hacer push sin autorización explícita del usuario.
- Commitear archivos de secretos (`.env`, credenciales, claves).
- Hacer push directo a `{{rama protegida}}` sin autorización.
- Modificar migraciones ya aplicadas.

---

## 5. Verificación

> Comandos que el agente debe ejecutar para validar que no rompió nada. Completar con los comandos reales del proyecto.

| Tipo      | Comando                                           |
| --------- | ------------------------------------------------- |
| Tests     | `{{pytest / npm run test / go test ./...}}`       |
| Build     | `{{npm run build / go build / cargo build}}`      |
| Lint      | `{{eslint . / ruff check . / golangci-lint run}}` |
| Typecheck | `{{mypy . / npx tsc --noEmit}}`                   |

> Eliminar las filas que no apliquen y completar con los comandos reales del proyecto.

---

## 6. Referencias

| Documento    | Ruta                     |
| ------------ | ------------------------ |
| Architecture | `docs/architecture/`     |
| Data Model   | `docs/data-model/ERD.md` |
| Specs        | `docs/specs/`            |
| Worklog      | `docs/WORKLOG.md`        |
