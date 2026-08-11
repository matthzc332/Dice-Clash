# {{Nombre del Proyecto}} — Arquitectura Frontend

> Documento vivo. Actualizar a medida que evoluciona el sistema.

---

## Quick Reference

| Item           | Valor                              |
| -------------- | ---------------------------------- |
| **Framework**  | {{React / Vue / Angular / Svelte}} |
| **Lenguaje**   | {{TypeScript / JavaScript}}        |
| **Build**      | {{Vite / Webpack}}                 |
| **UI Library** | {{shadcn/ui / MUI / Tailwind}}     |
| **Estado**     | {{Zustand / Redux / Pinia}}        |
| **Puerto dev** | {{5173 / 3000 / 8080}}             |

---

## 1. Visión General

> Diagrama de contexto del sistema.

```mermaid
flowchart LR
    User["Usuario"] -->|" Navegador"| App["{{App Web}}"]
    App -->|"HTTP / WS"| API["Backend API"]
    API -->|"Queries"| DB[({{Backend}})]
```

**Descripción**: {{1-2 líneas sobre qué hace esta app}}

---

## 2. Stack

| Capa        | Tecnología       | Notas            |
| ----------- | ---------------- | ---------------- |
| Framework   | {{React 18}}     | SPA              |
| Lenguaje    | {{TypeScript}}   | Strict mode      |
| Build       | {{Vite}}         | Hot reload       |
| Estilos     | {{Tailwind CSS}} | v{{3.x}}         |
| Componentes | {{shadcn/ui}}    | Basados en Radix |
| Estado      | {{Zustand}}      | Client state     |
| Data Fetch  | {{React Query}}  | Server state     |
| Router      | {{React Router}} | v{{6.x}}         |

---

## 3. Estructura

```
src/
├── {{api/}}                 # Clientes HTTP, Axios/Fetch
│   └── {{client.ts}}       # Instancia configurada
├── {{components/}}
│   ├── {{ui/}}             # shadcn/ui o similar
│   └── {{domain/}}         # Componentes de negocio
├── {{features/}}           # Features (agrupación por dominio)
│   └── {{auth/}}          # Ej: login, logout, protected-route
│       ├── {{components/}}
│       ├── {{hooks/}}
│       └── {{types/}}
├── {{hooks/}}              # Custom hooks globales
├── {{pages/}}             # Páginas/rutas
├── {{stores/}}            # Zustand stores
├── {{types/}}             # Tipos TS globales
└── {{App.tsx}}            # Entry point
```

> Esta es una estructura recomendada. Ajustar a la realidad del proyecto.

---

## 4. Cómo Funciona

### Flujo de Datos

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Página    │────▶│   Hook      │────▶│ API Client  │
│ (Componente)│     │ (useQuery)  │     │  (fetch)    │
└─────────────┘     └─────────────┘     └──────┬──────┘
                                               │
                                               ▼
                                        ┌─────────────┐
                                        │ Backend API │
                                        └─────────────┘
```

### Routing

| Ruta     | Componente  | Auth | Notas            |
| -------- | ----------- | ---- | ---------------- |
| `/`      | `HomePage`  | ✓    | Dashboard        |
| `/login` | `LoginPage` | ✗    | Público          |
| `/users` | `UsersPage` | ✓    | Requiere permiso |

### Estado Global

| Store              | Qué guarda                         | Cuándo se usa |
| ------------------ | ---------------------------------- | ------------- |
| `{{useAuthStore}}` | {{user, token, login(), logout()}} | {{全局 auth}} |
| `{{useUIStore}}`   | {{sidebarOpen, theme}}             | {{UI state}}  |

### Comunicación con Backend

- **Cliente**: {{Axios / Fetch / Ky}}
- **Base URL**: {{import.meta.env.VITE_API_URL}}
- **Interceptor auth**:

```ts
// Agregar Bearer token a cada request
instance.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});
```

- **Manejo de errores**:

```ts
// Redirigir a login en 401
instance.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      useAuthStore.getState().logout();
      window.location.href = "/login";
    }
    return Promise.reject(error);
  },
);
```

---

## 5. Componentes

### UI Base (shadcn/ui o similar)

| Componente            | Uso            |
| --------------------- | -------------- |
| `Button`              | Acciones       |
| `Input` / `TextField` | Formularios    |
| `Select` / `Combobox` | Selección      |
| `Table` / `DataTable` | Listas         |
| `Dialog` / `Modal`    | Modales        |
| `Toast` / `Sonner`    | Notificaciones |

### Componentes de Dominio

| Componente      | Descripción                        |
| --------------- | ---------------------------------- |
| `{{UserTable}}` | {{Tabla con filtros, paginación}}  |
| `{{UserForm}}`  | {{Formulario de creación/edición}} |

---

## 6. Ambientes

| Ambiente    | URL                | API URL            |
| ----------- | ------------------ | ------------------ |
| Development | {{localhost:5173}} | {{localhost:8000}} |
| Staging     | {{staging...}}     | {{staging-api...}} |
| Production  | {{prod...}}        | {{prod-api...}}    |

---

## 7. Decisiones de Diseño

> Por cada decisión importante: qué problema resolvimos, qué elegimos, qué descartamos.

### {{Decisión 1: nombre}}

- **Problema**: {{qué necesidad surgía}}
- **Solución**: {{qué se implementó y por qué}}
- **Alternativas**: {{qué otras opciones se evaluaron}}

> Agregar una sección de estas por cada decisión relevante.

## 8. Referencias

| Recurso    | Ruta                                        |
| ---------- | ------------------------------------------- |
| Backend    | `docs/architecture/BACKEND-ARCHITECTURE.md` |
| Specs      | `docs/specs/`                               |
| Data Model | `docs/data-model/`                          |
