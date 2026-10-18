# Endpoints

Contrato que lee el front. Se actualiza al cerrar cada UC. Las rutas van en inglés, igual que los campos y los códigos de error.

| UC | Método | Ruta | Descripción | Estado |
|---|---|---|---|---|
| UC-01 | GET | `/api/collaborators` | Listar colaboradores | hecho |
| UC-02 | POST/PUT/DELETE | `/api/collaborators[/{id}]` | Alta, edición, baja lógica | hecho |
| UC-03 | POST | `/api/shifts` | Alta de turno (409 si solapa) | hecho |
| UC-04 | GET/PUT/DELETE | `/api/shifts[/{id}]` | Listar con rango de fechas, leer, editar, borrar | pendiente |
| UC-05 | GET | `/api/shifts` | Incluye marca de festivo | pendiente |
| UC-06 | POST | `/api/shifts/import` | Importar `.xlsx` con informe | pendiente |
| UC-07 | GET | `/api/shifts/export` | Exportar `.xlsx` por rango | pendiente |
| — | GET | `/health` | Health check (incluye la BD) | hecho |

Las rutas son provisionales hasta cerrar cada UC.

## UC-01 · GET /api/collaborators

Devuelve `200` con los colaboradores activos, ordenados por nombre y luego email. Sin parámetros. Ejemplos en [collaborators.http](collaborators.http).

```json
[
  { "id": "<guid>", "name": "Ana Pérez", "email": "ana.perez@example.com", "countryCode": "ES" }
]
```

## UC-02 · POST, PUT y DELETE /api/collaborators

Body de `POST` y `PUT`: `{ "name": "...", "email": "...", "countryCode": "ES" }`. El nombre se recorta, el email se guarda en minúsculas y el país en mayúsculas. Ejemplos en [collaborators.http](collaborators.http).

| Acción | Éxito | Errores |
|---|---|---|
| `POST /api/collaborators` | `201` con el colaborador y `Location: /api/collaborators/{id}` | `400`, `409` |
| `PUT /api/collaborators/{id}` | `200` con el colaborador | `400`, `404`, `409` |
| `DELETE /api/collaborators/{id}` | `204` (idempotente) | `404` |

Códigos de `errors` (formato en [ERROR-HANDLING.md](ERROR-HANDLING.md)):

| Código | HTTP | Cuándo |
|---|---|---|
| `collaborator.name` | 400 | Vacío o de más de 200 caracteres |
| `collaborator.email` | 400 | Dirección inválida |
| `collaborator.country` | 400 | No es un código ISO 3166-1 alfa-2 |
| `collaborator.country-unknown` | 400 | Nager.Date no lo soporta (si no responde, se acepta con el formato válido) |
| `collaborator.email-taken` | 409 | Otro colaborador, activo o dado de baja, usa el email |
| `collaborator.not-found` | 404 | El `id` no existe |

La baja es lógica (ADR-11): el colaborador deja de aparecer en el listado, conserva sus turnos y su email sigue ocupado. El `Location` del `POST` apunta a un recurso que aún no tiene `GET /{id}`.

## UC-03 · POST /api/shifts

Body: `{ "collaboratorId": "<guid>", "date": "2026-03-10", "startTime": "09:00", "endTime": "13:00" }`. Fecha `yyyy-MM-dd`; horas `HH:mm` o `HH:mm:ss` (en la respuesta salen como `HH:mm:ss`). Devuelve `201` con el turno y `Location: /api/shifts/{id}`. Ejemplos en [shifts.http](shifts.http).

Un colaborador no puede tener dos turnos que se solapen el mismo día. Los intervalos son semiabiertos: 09:00-13:00 y 13:00-17:00 no se solapan. El turno no cruza la medianoche: `endTime` debe ser posterior a `startTime`.

| Código | HTTP | Cuándo |
|---|---|---|
| `shift.date` | 400 | Falta la fecha |
| `shift.time-range` | 400 | `endTime` no es posterior a `startTime` |
| `shift.collaborator-unavailable` | 400 | El colaborador no existe o está dado de baja |
| `shift.overlap` | 409 | Choca con otro turno del colaborador; `conflictingShiftId` trae el id del turno con el que choca |

Un valor con formato inválido (por ejemplo `"startTime": "nope"`) lo rechaza el binder del framework con `400` antes del caso de uso, y la clave de `errors` es la ruta JSON (`$.startTime`), no un código de negocio. El `Location` apunta a un recurso que aún no tiene `GET /{id}` (llega con UC-04).
