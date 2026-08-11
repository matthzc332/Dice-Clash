---
description: Inicializar proyecto csdd generando AGENTS.md, docs/ y toda la estructura de documentación a partir de los templates.
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

Eres un agente SDD especializado en setup de proyectos. Tu tarea es generar la estructura de documentación inicial usando los templates del proyecto.

---

## Paso 0: Verificar prerequisitos

1. Verificar que `.templates/` existe en el proyecto. Si no existe, detener e informar:
   > "No se encontró `.templates/`. Ejecutá `csdd init .` primero para crear la estructura base."
2. Verificar si el directorio es un repositorio git ejecutando `git rev-parse --is-inside-work-tree`.
   - Si NO es un repo git, sugerir (sin detener el proceso):
     > "Este directorio no es un repositorio git. Considerá ejecutar `git init` antes de continuar."

---

## Paso 0b: Detectar y adaptar estructuras SDD existentes

Antes de generar algo nuevo, buscar si ya existe alguna estructura similar a SDD en el proyecto. Si existe, ofrecer migrarla a la estructura csdd.

### 0b.1 Buscar specs existentes

Buscar directorios o archivos que contengan specs:

- `specs/*.md`, `docs/specs/*.md`, `documentation/specs/*.md`, `requirements/specs/*.md`
- `SPEC.md`, `spec.md`, `Specs/*.md`
- `docs/requirements/*.md`, `docs/features/*.md`
- Archivos con patrón `###-*.md` (formato csdd: `001-*.md`, `002-*.md`, etc.)

**Si se encuentran specs existentes:**

1. Listar los specs encontrados al usuario
2. Ofrecer migrarlos a `docs/specs/`:

   > "Encontré specs existentes en [ruta]. ¿Querés migrarlos a `docs/specs/`?"

3. Si el usuario confirma:
   - Crear `docs/specs/` si no existe
   - Mover o copiar los specs a `docs/specs/`
   - Adaptar el formato si es necesario (agregar secciones que falten del template)
   - Actualizar referencias en el worklog si existe

### 0b.2 Buscar worklogs/backlogs existentes

Buscar archivos de backlog o tareas:

- `WORKLOG.md`, `backlog.md`, `TODO.md`, `tasks.md`
- `docs/WORKLOG.md`, `docs/backlog.md`, `docs/tasks.md`
- `docs/TODO.md`, `documentation/backlog.md`
- Archivos que contengan tablas de tareas con estados (backlog, todo, in-progress, done)

**Si se encuentra un worklog:**

1. Mostrar al usuario lo encontrado
2. Ofrecer:

   > "Encontré un backlog en [ruta]. ¿Querés migrarlo a `docs/WORKLOG.md`?"

3. Si confirma:
   - Copiar el contenido a `docs/WORKLOG.md`
   - Mantener el estado de las tareas
   - Agregar columnas faltantes si es necesario (Feature, Spec)

### 0b.3 Buscar documentación de arquitectura

Buscar documentos de arquitectura existentes:

- `docs/architecture.md`, `docs/ARCHITECTURE.md`
- `architecture.md`, `ARCHITECTURE.md`
- `docs/backend-architecture.md`, `docs/frontend-architecture.md`

**Si se encuentran:**

- Ofrecer migrarlos a `docs/architecture/BACKEND-ARCHITECTURE.md` o `FRONTEND-ARCHITECTURE.md`

### 0b.4 Buscar modelos de datos

Buscar ERDs, diagramas de clases, o modelos de datos:

- `docs/data-model.md`, `docs/erd.md`, `docs/ERD.md`
- `data-model/`, `models/`, `schemas/`
- Archivos con extensión `.erd`, diagramas en `docs/diagrams/`

**Si se encuentra:**

- Ofrecer migrarlo a `docs/data-model/ERD.md` o `CLASS-DIAGRAM.md`

### 0b.5 Resumen de migración

Si se detectaron y migraron estructuras existentes, mostrar:

```
Estructuras SDD detectadas y migradas:
  - specs: X archivos migrateados a docs/specs/
  - worklog: migrateado a docs/WORKLOG.md
  - arquitectura: X documentos migrateados
  - modelo de datos: migrateado
```

**Importante:** Si no se encuentra ninguna estructura SDD previa, simplemente continuar con la generación normal (no mostrar mensaje al respecto).

---

## Paso 1: Generar/Migrar AGENTS.md

1. Si `AGENTS.md` ya existe:
   a. Leer el `AGENTS.md` existente completo.
   b. Leer `.templates/agents-template.md` para conocer la estructura de secciones esperada.
   c. Extraer los encabezados de nivel 2 (##) del template como secciones a verificar.
   d. Para cada sección del template, verificar si ya existe en el AGENTS.md existente:
      - Si la sección existe (buscar encabezado `## Nombre-sección` o similar), NO duplicarla.
      - Si la sección NO existe, agregarla al final del archivo con contenido indicativo.
   e. Informar al usuario:
      > "AGENTS.md ya existe. Se migró agregando las secciones faltantes: {{lista de secciones agregadas}}."
2. Si NO existe:
   a. Leer `.templates/agents-template.md`.
   b. Crear `AGENTS.md` en la raíz del proyecto copiando el contenido del template.
   c. Completar el `AGENTS.md` siguiendo la template pero con la información del proyecto.

---

## Paso 2: Generar docs/architecture/

**Este paso ya fue procesado en el Paso 0b si se detectó documentación de arquitectura existente.**

Si NO se detectó arquitectura previa, seguir estos subpasos:

### 2.1 Detectar el stack del proyecto

Buscar indicios de tecnología en el repositorio:

- **Archivos de configuración**: `package.json`, `pyproject.toml`, `requirements.txt`, `Cargo.toml`, `go.mod`, `pom.xml`, `composer.json`, etc.
- **Carpetas características**: `frontend/`, `backend/`, `src/`, `api/`, `web/`, `app/`, `mobile/`, etc.
- **Archivos de framework**: `manage.py` (Django), `main.py` + FastAPI imports, `next.config.js`, `nuxt.config.ts`, `angular.json`, etc.
- **Archivos de infra**: `Dockerfile`, `docker-compose.yml`, `.env.example`

### 2.2 Determinar qué archivos de arquitectura crear

- **Detectas solo backend** → crear solo `BACKEND-ARCHITECTURE.md`
- **Detectas solo frontend** → crear solo `FRONTEND-ARCHITECTURE.md`
- **Detectas ambos** → crear ambos
- **No detectas nada claro** → preguntar al usuario:

  > "No encontré indicios claros de stack en el proyecto. ¿Qué estás construyendo?"
  >
  > - Solo backend
  > - Solo frontend
  > - Fullstack (backend + frontend)
  > - Lo defino después

  Esperar respuesta antes de continuar. Si elige "Lo defino después", copiar ambas templates sin completar.

### 2.3 Si el proyecto ya tiene código (está iniciado):

Completar la template con lo que puedas inferir del código:

Guía de inferencia por framework:

- **FastAPI**: buscar `main.py` con `FastAPI()`, carpetas `routers/`, `services/`, `models/`, `schemas/`. Puerto en llamada a `uvicorn.run()`.
- **Django**: buscar `manage.py`, `settings.py`, `INSTALLED_APPS`. Puerto default 8000.
- **Express**: buscar `app.listen()`, carpetas `routes/`, `controllers/`, `middleware/`.
- **NestJS**: buscar `@Module()`, `@Controller()`, `@Injectable()`, carpetas `modules/`, `services/`.
- **Next.js**: buscar `next.config.js` o `next.config.ts`, carpeta `pages/` o `app/` (App Router).
- **React SPA**: buscar `vite.config.ts` o `webpack.config.js`, carpeta `src/components/`, `src/pages/`.
- **Go**: buscar `go.mod`, función `main()` en `main.go`, carpetas `handlers/`, `services/`, `models/`.

**Para BACKEND-ARCHITECTURE.md:**

- Stack: lenguaje, framework, ORM, base de datos, infra (de los archivos de configuración)
- Estructura de carpetas: árbol real del proyecto (limitado a carpetas relevantes)
- Patrón de capas: inferir del código (ej: si hay `services/`, `repositories/`, `routers/`)
- Variables de entorno: leer `.env.example` si existe

**Para FRONTEND-ARCHITECTURE.md:**

- Stack: framework, build tool, librerías de estado y fetching (de `package.json`)
- Estructura de carpetas: árbol real de `src/`
- Rutas: inferir de la carpeta de páginas o del router si está definido
- Componentes disponibles: listar los que existan en la carpeta de componentes UI

### 2.4 Si el proyecto NO tiene código aún:

Copiar la template correspondiente sin completar, dejando los `{{placeholders}}` para que sean llenados a medida que el proyecto escala.

---

## Paso 3: Generar docs/WORKLOG.md

**Este paso ya fue procesado en el Paso 0b si se detectó un worklog existente.**

Si NO se detectó ningún worklog previo:

1. Leer `.templates/worklog-template.md`.
2. Crear `docs/WORKLOG.md` copiando el template **sin modificarlo**.
3. NO inferir ni completar tareas del código. Copiar solo la estructura del template.
4. Si `docs/WORKLOG.md` ya existe, omitir e informar.

---

## Paso 4: Generar docs/data-model/

**Este paso ya fue procesado en el Paso 0b si se detectó un modelo de datos existente.**

Si NO se detectó modelo de datos previo, seguir estos subpasos:

### 4.1 Determinar el tipo de base de datos

Analizar el stack del proyecto (detectado en el Paso 2 o informado por el usuario) para determinar qué template usar:

- **Base de datos relacional** (PostgreSQL, MySQL, SQLite, SQL Server, Oracle, etc.) → usar `erd-template.md`
- **Base de datos no relacional** (MongoDB, DynamoDB, Firestore, Redis, Cassandra, etc.) → usar `class-template.md`
- **Si hay ambas** → crear ambos archivos con sus respectivas templates.
- **Si no está claro**, preguntar al usuario antes de continuar.

### 4.2 Buscar modelos existentes en el proyecto

Buscar evidencia de modelos de datos ya definidos:

- **Relacional**: modelos ORM (SQLAlchemy, Django ORM, Prisma schema, TypeORM, Hibernate), archivos `.sql`, migraciones (`alembic/versions/`, `migrations/`).
- **No relacional**: schemas de Mongoose, modelos de Firestore, definiciones de colecciones, clases con decoradores de ODM.
- **Cualquiera**: schemas de validación (Pydantic, Zod, JSON Schema) que describan la forma de los datos.

### 4.3 Si el proyecto tiene modelos existentes:

Usar la template correspondiente como estructura base y completarla con las entidades/colecciones encontradas:

- Documentar cada entidad con sus campos, tipos y relaciones.
- Para ERD: respetar el formato `erDiagram` de Mermaid y las tablas de campos.
- Para clases: respetar el formato `classDiagram` de Mermaid, distinguiendo embebidos (`*--`) de referencias (`-->`).
- El archivo destino es `docs/data-model/ERD.md` (relacional) o `docs/data-model/CLASS-DIAGRAM.md` (no relacional).

### 4.4 Si el proyecto NO tiene modelos aún:

Copiar la template vacía correspondiente al destino sin modificarla, para que sea completada a medida que el proyecto escale:

- Relacional → copiar `erd-template.md` como `docs/data-model/ERD.md`
- No relacional → copiar `class-template.md` como `docs/data-model/CLASS-DIAGRAM.md`

---

## Paso 5: Crear docs/specs/

**Este paso ya fue procesado en el Paso 0b si se migraron specs existentes.**

Si NO se migraron specs previamente, simplemente:

Asegurarse de que el directorio `docs/specs/` exista. No crear ningún spec todavía.

---

## Resumen final

Al terminar, mostrar al usuario un resumen de todo lo generado o migrado:

```
Proyecto csdd inicializado:
```

**Si hubo migración (estructuras SDD detectadas):**

```
  [MIGRADO] specs: X archivos migrados desde <ruta original>
  [MIGRADO] worklog: migrado desde <ruta original>
  [MIGRADO] arquitectura: X documentos migrados
  [MIGRADO] modelo de datos: migrado
```

**Si no hubo migración (estructura nueva):**

```
  AGENTS.md                                  <- instrucciones para agentes AI
  docs/
    WORKLOG.md                               <- backlog del proyecto
    specs/                                   <- aquí irán los specs
    architecture/
      BACKEND-ARCHITECTURE.md
      FRONTEND-ARCHITECTURE.md
    data-model/
      ERD.md
```

Indicar qué archivos fueron creados, cuáles ya existían (omitidos), cuáles fueron migrados y cualquier decisión tomada.
