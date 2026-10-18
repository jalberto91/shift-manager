# Historial de bloques cerrados

No se lee entero: se consulta por fila cuando hace falta.

| Fecha | Bloque | Resultado | Link |
|---|---|---|---|
| 2026-10-17 | Andamiaje SDD, esqueleto técnico (solución, plomería, EF/PostgreSQL, Docker) y UC-01 (a) | 12 commits, hasta `9fe4e9e`; build limpio y 27 tests verdes; `docker compose up` verificado | [2026-10-17-andamiaje-y-esqueleto.md](2026-10-17-andamiaje-y-esqueleto.md) |
| 2026-10-17 | UC-01 (b): listar colaboradores (query, repositorio, migración inicial, semilla, endpoint, docs) | `2a8b8cd`; build limpio, 27 tests verdes, E2E con `docker compose up` verificado | [2026-10-17-uc01-listar-colaboradores.md](2026-10-17-uc01-listar-colaboradores.md) |
| 2026-10-17 | UC-02 (a): spec, ADR-11 (baja lógica), dominio (`IsActive`, `Update`, `Deactivate`) y migración `AddCollaboratorIsActive` | `9fd35c4`; build limpio, 31 tests verdes | [2026-10-17-uc02-a-dominio.md](2026-10-17-uc02-a-dominio.md) |
| 2026-10-17 | UC-02 (b1): casos de uso Create/Update/Deactivate, repositorio, índice único de email y `NagerDateCountryCatalog` | `e9c5cff`; build limpio, 52 tests verdes; migraciones aplicadas con `docker compose up` | [2026-10-17-uc02-b1-casos-de-uso.md](2026-10-17-uc02-b1-casos-de-uso.md) |
| 2026-10-17 | UC-02 (b2): acciones `POST`/`PUT`/`DELETE` en `CollaboratorsController` y `CollaboratorRequest` | `e75456c`; build limpio, 52 tests verdes; verificado con curl sobre `docker compose up` | [2026-10-17-uc02-b2-endpoints.md](2026-10-17-uc02-b2-endpoints.md) |
| 2026-10-17 | UC-02 (c): `ENDPOINTS.md`, `.http` con respuestas reales, E2E y cierre del UC | `b7a03c5`; build limpio, 52 tests verdes; verificado con curl sobre `docker compose up` | [2026-10-17-uc02-c-docs-y-cierre.md](2026-10-17-uc02-c-docs-y-cierre.md) |
| 2026-10-17 | UC-03: alta de turno con regla de no solape (`Shift`, `CreateShift`, persistencia, migración `AddShifts`, `POST /api/shifts`, docs) | `857f2df`, `36c1c56`, `10471c9`; build limpio, 75 tests verdes; verificado con curl sobre `docker compose up` | [2026-10-17-uc03-alta-de-turno.md](2026-10-17-uc03-alta-de-turno.md) |
| 2026-10-17 | UC-04 (a): spec de gestión de turnos y `Shift.Update` con validación compartida | `2b94b83`, `00c6b78`; build limpio, 78 tests verdes | [2026-10-17-uc04-a-dominio.md](2026-10-17-uc04-a-dominio.md) |
| 2026-10-17 | UC-04 (b1): `ListShifts` y `GetShift` con `ListAsync` y `GetByIdAsync`, `shift.not-found` y `shift.date-range` | `2d5206a`; build limpio, 86 tests verdes; sin E2E (se hace en (c)) | [2026-10-17-uc04-b1-consultas.md](2026-10-17-uc04-b1-consultas.md) |
