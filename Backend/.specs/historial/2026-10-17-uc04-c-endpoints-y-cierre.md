# UC-04 (c): endpoints, docs y cierre del UC (archivado 2026-10-17)

Commit: `3af71d6`. Bloque archivado tal como estaba en `ESTADO-ACTUAL.md`:

## ⏭️ Próximo paso
**UC-04 (c)**: según `.specs/use-cases/UC-04-gestionar-turnos.md` (Requirements: criterios de aceptación; Design: tabla de endpoints y "Capas previstas", Api): cuatro acciones en `ShiftsController` (`GET` lista con `from`/`to`, `GET {id}`, `PUT {id}`, `DELETE {id}`); `CreateShiftRequest` pasa a `ShiftRequest` (sirve a `POST` y `PUT`, como `CollaboratorRequest`). Luego `docs/api/ENDPOINTS.md`, `shifts.http` con respuestas reales, E2E con la skill `verify` (confirma además el filtro con parámetros nulos, el orden y `excludingId`/`Remove` contra PostgreSQL), sección "Para la revisión" y cierre del UC. Los reintentos de ADR-09 (resilience) quedan para el cliente de festivos.

## Bloque en curso
UC-03 cerrado y archivado. UC-04 en curso: (a), (b1) y (b2) cerradas y archivadas; (c) pendiente. Ninguna iteración abierta.

Resumen de lo hecho en (c) (no estaba en el estado vivo):
- `ShiftsController`: `List` (`from`/`to`), `Get`, `Update` y `Delete`, con el patrón de `CollaboratorsController`; `CreateShiftRequest` pasó a `ShiftRequest`. Sin `[FromQuery]` explícito: `[ApiController]` infiere los tipos simples de la query.
- `docs/api/ENDPOINTS.md` (sección UC-04) y `docs/api/shifts.http` con 11 requests y respuestas reales.
- E2E con `docker compose up --build` y curl contra PostgreSQL: filtro con parámetros nulos, orden por fecha e inicio, extremos incluidos, `excludingId` y `Remove` confirmados; 97 tests verdes, build sin warnings.
- Sin ejercitar por la API (cubiertos con fakes): colaborador dado de baja y cambio de colaborador con choque.
- Revisado: sin violaciones. Riesgos aceptados: `GET /api/shifts` sin filtro devuelve toda la tabla (P13, sin paginación) y `PUT`/`DELETE` simultáneos sobre el mismo turno darían `500` (misma familia de carreras aceptadas para el demo).
- La BD de pruebas quedó con turnos de abril de Carla y Diego (`docker compose down -v` los limpia).
