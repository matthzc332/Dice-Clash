# {{Nombre del Proyecto}} — Arquitectura Backend

> Documento vivo. Actualizar a medida que evoluciona el sistema.

---

## Quick Reference

| Item           | Valor                                     |
| -------------- | ----------------------------------------- |
| **Framework**  | {{FastAPI / Django / Express / ...}}      |
| **Lenguaje**   | {{Python 3.12 / Node 20 / Go 1.22 / ...}} |
| **DB**         | {{PostgreSQL / MySQL / MongoDB / ...}}    |
| **Puerto**     | {{3000 / 8000 / 8080 / ...}}              |
| **Puerto dev** | {{3001 / 8001 / ...}}                     |

---

## 1. Visión General

> Diagrama de contexto del sistema.

```mermaid
flowchart LR
    Users["Usuarios"] -->|HTTP| API["API Backend"]
    API -->|"Queries / Updates"| DB[({{Base de datos}})]
    API -->|"Webhooks / REST"| External["{{Servicio externo}}"]
```

**Descripción**: {{1-2 líneas sobre qué hace este backend}}

---

## 2. Stack

| Capa          | Tecnología     | Versión    |
| ------------- | -------------- | ---------- |
| Framework     | {{FastAPI}}    | {{2024.x}} |
| Lenguaje      | {{Python}}     | {{3.12}}   |
| ORM           | {{SQLAlchemy}} | {{2.0}}    |
| Base de datos | {{PostgreSQL}} | {{16}}     |
| Auth          | {{JWT}}        | -          |
| Infra         | {{Docker}}     | {{24.x}}   |

> Si hay más tecnologías (migraciones, cache, etc.), agregarlas como filas adicionales.

---

## 3. Estructura

```
{{carpeta-raíz}}/
├── {{src/ o app/}}              # Código fuente
│   ├── {{models/}}              # Entidades de BD
│   ├── {{schemas/ o dtos/}}    # DTOs de entrada/salida
│   ├── {{services/}}            # Lógica de negocio
│   ├── {{routers/ o controllers/}}  # Endpoints
│   ├── {{deps/ o middlewares/}} # Dependencias compartidas
│   └── main.py                  # Entry point
├── {{tests/}}                   # Tests
├── {{alembic/ o migrations/}}  # Migraciones
├── .env.example                 # Variables de entorno ejemplo
├── Dockerfile
└── docker-compose.yml
```

---

## 4. Cómo Funciona

### Request Lifecycle

```
HTTP Request
    │
    ▼
┌─────────────────────┐
│ Router / Endpoint   │  # Recibe request, extrae params
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Schema Validation   │  # Pydantic / Zod / class-validator
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Dependencies       │  # Auth, DB session, permisos
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Service             │  # Lógica de negocio
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ Repository          │  # Acceso a datos
└──────────┬──────────┘
           │
           ▼
    DB / External API
```

### Autenticación

- **Tipo**: {{JWT Bearer / OAuth2 / Session}}
- **Token**: {{access token con expiry, refresh token}}
- **Flujo típico**:

```
1. Client → POST /auth/login (email + password)
2. Server → Valida credenciales
3. Server → Genera access_token (15min) + refresh_token (7d)
4. Server → Retorna tokens
5. Client → Guarda en storage
6. Client → Envía access_token en header: Authorization: Bearer <token>
```

### Autorización

- **Modelo**: {{RBAC (Roles) / ABAC (Permissions)}}
- **Estructura de permisos**: {{ej: MODULOrecurso_ACCION}}

| Permiso           | Descripción               |
| ----------------- | ------------------------- |
| `{{USERS_READ}}`  | {{Ver usuarios}}          |
| `{{USERS_WRITE}}` | {{Crear/editar usuarios}} |

---

## 5. Ambientes

| Ambiente    | DB               | URL                | Propósito  |
| ----------- | ---------------- | ------------------ | ---------- |
| Development | {{Docker local}} | {{localhost:8000}} | Desarrollo |
| Staging     | {{Cloud DB}}     | {{staging.api...}} | QA         |
| Production  | {{Cloud DB HA}}  | {{api...}}         | Producción |

### Variables de Entorno

```bash
# Obligatorias
{{DB_URL}}=postgresql://user:pass@host:5432/db
{{SECRET_KEY}}={{generar con: openssl rand -hex 32}}

# Opcionales
{{DEBUG}}=true
{{LOG_LEVEL}}=DEBUG
{{CORS_ORIGINS}}=http://localhost:5173
```

---

## 6. Integraciones

| Servicio       | Propósito          | Tipo             | Datos clave           |
| -------------- | ------------------ | ---------------- | --------------------- |
| {{AD / Auth0}} | {{Autenticación}}  | {{OAuth2/LDAP}}  | {{tenant, client_id}} |
| {{ERP}}        | {{Sincronización}} | {{REST webhook}} | {{endpoint, secret}}  |

---

## 7. Decisiones de Diseño

> Por cada decisión importante: qué problema resolvimos, qué elegimos, qué descartamos.

### {{Decisión 1: nombre}}

- **Problema**: {{qué necesidad surgía}}
- **Solución**: {{qué se implementó y por qué}}
- **Alternativas**: {{qué otras opciones se evaluaron}}

> Agregar una sección de estas por cada decisión relevante (no más de 5-6).

---

## 8. Referencias

| Recurso    | Ruta                                         |
| ---------- | -------------------------------------------- |
| Data Model | `docs/data-model/`                           |
| Specs      | `docs/specs/`                                |
| Frontend   | `docs/architecture/FRONTEND-ARCHITECTURE.md` |
