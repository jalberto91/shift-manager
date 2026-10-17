# Arquitectura del front — fuente de verdad

Si hay que desviarse de algo de este documento, **se actualiza el ADR antes** de tocar código.

## §1 Vista global

```
 ┌───────────┐  HTTP   ┌─────────────────────────────┐  /api (proxy)  ┌──────────────┐     ┌────────────┐
 │ Navegador │ ──────► │ nginx (Docker) / ng serve   │ ─────────────► │ API .NET 9   │ ──► │ PostgreSQL │
 │           │ ◄────── │  └ app Angular (standalone) │ ◄───────────── │ ../Backend   │     └────────────┘
 └───────────┘  SPA    └─────────────────────────────┘ Problem Details└──────┬───────┘
                                                                              ▼
                                                                    Nager.Date (festivos)
```

En desarrollo `ng serve` usa `proxy.conf.json` hacia `http://localhost:5080`; en Docker nginx hace de proxy de `/api` hacia el servicio `api`. En ambos casos el front llama a `/api` relativo: no hace falta CORS.

## §2 Convenciones transversales

| Tema | Convención | Estado |
|---|---|---|
| Estructura de carpetas | Feature-first: `core/`, `shared/`, `features/<f>/{data-access,ui,pages}` | ✅ ADR-02 |
| Componentes | Standalone, OnPush, `inject()`, control flow nativo, sin NgModules | ✅ ADR-01 |
| Estado | Signals en un store por feature (`loading`, `error`, `data`, `computed`); sin NgRx | ✅ ADR-05 |
| Errores | Problem Details → interceptor → servicio de notificaciones; errores de negocio por código → control del form; textos propios en español | ✅ ADR-04 |
| Fechas/horas | Texto sin zona: fecha `yyyy-MM-dd`, hora `HH:mm` (el backend responde `HH:mm:ss`; se muestra `HH:mm`). Hora de reloj local del colaborador, sin conversiones (P12). No se usa `Date` para fechas puras | ✅ ADR-04 |
| Números/moneda | No aplica | ✅ |
| Locale | `es` para pipes y componentes de PrimeNG | TBD (Fase 2) |
| Paginación y filtros | Sin paginación (P13). Filtro por rango de fechas en el listado de turnos, enviado como query al backend | ✅ |
| Autenticación | Ninguna: sin guards ni interceptor de auth | ✅ ADR-06 |
| Config | URL de la API siempre relativa (`/api`) | ✅ ADR-07 |
| Accesibilidad mínima | Labels asociados, foco visible, mensajes de error ligados al control, no solo color | ✅ |
| Nombres de archivo | `kebab-case` con sufijo de tipo: `collaborators.api.ts`, `collaborators.store.ts`, `collaborator-form.component.ts`, `collaborators.routes.ts` | ✅ |
| Un tipo por archivo | Un componente/servicio/interfaz principal por archivo | ✅ |

## §3 ADRs

### ADR-01 · Angular standalone + signals
- **Contexto**: front pequeño, se defiende en vivo; el CLI moderno genera standalone por defecto.
- **Alternativas**: NgModules; estado con RxJS puro.
- **Decisión**: Angular 21 (la última estable es la 22; se fija la 21 por la licencia de PrimeNG, ver ADR-03), standalone, signals, `inject()`, control flow nativo, OnPush, `strict`.
- **Consecuencias**: (+) menos boilerplate, detección de cambios predecible. (−) APIs de signals/formularios aún evolucionan entre versiones.
- **Estado**: Aceptado

### ADR-02 · Arquitectura feature-first
- **Contexto**: cinco pantallas en dos dominios (colaboradores, turnos).
- **Alternativas**: capas globales (`components/`, `services/`).
- **Decisión**: `core/`, `shared/` y `features/<f>/{data-access,ui,pages}` con rutas lazy por feature.
- **Consecuencias**: (+) cada feature se entiende y borra sola. (−) hay que vigilar que `shared/` no acumule lógica.
- **Estado**: Aceptado

### ADR-03 · Librería de UI y estilos
- **Contexto**: se pide front funcional y limpio, sin diseño propio (P15).
- **Alternativas**: Angular Material; UI a mano.
- **Decisión**: PrimeNG **21.1.x** (MIT) con el tema por defecto (tabla, diálogo, formularios, `FileUpload`, toasts) y **Angular 21**, no 22. PrimeNG 22 pasó a la licencia PrimeUI (license key y renovación anual), que no es libre del todo (E2); PrimeNG 21 es MIT y exige Angular 21. SCSS mínimo, sin Tailwind. Cada paquete pasa por `license-check`.
- **Consecuencias**: (+) casi cero CSS propio y sin license key. (−) Angular 21 queda una mayor por detrás de la última estable; dependencia pesada; fijar versiones para que `npm` no suba `@primeuix/*` a ramas con otra licencia.
- **Estado**: Aceptado

### ADR-04 · Contrato de errores consumido desde el backend
- **Contexto**: la API responde Problem Details + `traceId`; `errors` va keyed por código estable y sus mensajes están en inglés.
- **Alternativas**: mostrar `detail`; manejar errores en cada componente.
- **Decisión**: un interceptor funcional parsea el Problem Details a un `ApiError` tipado. Los `400/409` los resuelve la pantalla por código (al control del formulario o aviso); `404` y `500` van al servicio de notificaciones (en `500`, aviso genérico con `traceId`). Los textos viven en el front, indexados por código. Un código desconocido cae a un mensaje genérico.
- **Consecuencias**: (+) UI coherente y en español. (−) cada código nuevo del backend exige un texto en el front.
- **Estado**: Aceptado

### ADR-05 · Estado con signals
- **Contexto**: estado por pantalla, sin compartir entre features.
- **Alternativas**: NgRx; RxJS con `BehaviorSubject`.
- **Decisión**: un store por feature, servicio con signals; NgRx solo con ADR que lo justifique.
- **Consecuencias**: (+) simple y testeable. (−) sin devtools de time-travel.
- **Estado**: Aceptado

### ADR-06 · Sin autenticación
- **Contexto**: no se pide.
- **Alternativas**: JWT básico.
- **Decisión**: sin auth, sin guards de sesión; se lista en "qué quedó fuera" del README.
- **Consecuencias**: (+) alcance acotado. (−) la app no es apta para exponerse públicamente.
- **Estado**: Aceptado

### ADR-07 · Docker con nginx como proxy de `/api`
- **Contexto**: `docker compose up` debe dejar todo andando (R8) y el front llama a `/api` relativo.
- **Alternativas**: CORS entre `:4200` y `:5080`; `ng serve` dentro del contenedor.
- **Decisión**: `Dockerfile` multi-stage (Node compila, nginx sirve `dist` con fallback a `index.html`) y `location /api` hacia `http://api:8080`. Servicio `web` agregado a `../docker-compose.yml`; el puerto publicado se documenta en el README.
- **Consecuencias**: (+) mismo origen, sin CORS. (−) la ruta del proxy debe mantenerse alineada con el servicio `api`.
- **Estado**: Aceptado

## Plantilla de ADR

```
### ADR-NN · Título
- **Contexto**:
- **Alternativas**:
- **Decisión**:
- **Consecuencias**: (+) … (−) …
- **Estado**: Propuesto | Aceptado | Superseded por ADR-NN
```
Un ADR superseded no se borra.
