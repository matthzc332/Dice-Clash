# Worklog

Registro de trabajo y backlog del proyecto.

---

## Convenciones

- ID de tarea: `<NNN>-<slug>` (número secuencial de 3 dígitos)
- Cada tarea se convierte en un spec en `docs/specs/<id>.md`
- Cada spec vive en su rama `spec/<id>`
- Estados: `backlog` · `next` · `in-progress` · `blocked` · `done`
- Si una tarea está `blocked`, agregar una nota debajo de la tabla indicando el motivo del bloqueo
- Solo un item puede estar en `next` a la vez
- Solo un item puede estar en `in-progress` a la vez

---

> **Nota**: Las siguientes tareas son un ejemplo de formato.

## Feature: Nombre del Módulo

> Descripción breve del módulo o feature.

| #   | ID              | Tarea                   | Spec                                     | Estado      |
| --- | --------------- | ----------------------- | ---------------------------------------- | ----------- |
| 1   | 001-slug-tarea1 | Descripción de la tarea | [spec](../docs/specs/001-slug-tarea1.md) | done        |
| 2   | 002-slug-tarea2 | Descripción de la tarea | [spec](../docs/specs/002-slug-tarea2.md) | in-progress |
| 3   | 003-slug-tarea3 | Descripción de la tarea | [spec](../docs/specs/003-slug-tarea3.md) | next        |
| 4   | 004-slug-tarea4 | Descripción de la tarea | —                                        | backlog     |
