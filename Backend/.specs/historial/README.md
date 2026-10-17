# Historial de bloques cerrados

No se lee entero: se consulta por fila cuando hace falta.

| Fecha | Bloque | Resultado | Link |
|---|---|---|---|
| 2026-10-17 | Andamiaje SDD, esqueleto técnico (solución, plomería, EF/PostgreSQL, Docker) y UC-01 (a) | 12 commits, hasta `9fe4e9e`; build limpio y 27 tests verdes; `docker compose up` verificado | [2026-10-17-andamiaje-y-esqueleto.md](2026-10-17-andamiaje-y-esqueleto.md) |
| 2026-10-17 | UC-01 (b): listar colaboradores (query, repositorio, migración inicial, semilla, endpoint, docs) | `2a8b8cd`; build limpio, 27 tests verdes, E2E con `docker compose up` verificado | [2026-10-17-uc01-listar-colaboradores.md](2026-10-17-uc01-listar-colaboradores.md) |
| 2026-10-17 | UC-02 (a): spec, ADR-11 (baja lógica), dominio (`IsActive`, `Update`, `Deactivate`) y migración `AddCollaboratorIsActive` | `9fd35c4`; build limpio, 31 tests verdes | [2026-10-17-uc02-a-dominio.md](2026-10-17-uc02-a-dominio.md) |
| 2026-10-17 | UC-02 (b1): casos de uso Create/Update/Deactivate, repositorio, índice único de email y `NagerDateCountryCatalog` | `e9c5cff`; build limpio, 52 tests verdes; migraciones aplicadas con `docker compose up` | [2026-10-17-uc02-b1-casos-de-uso.md](2026-10-17-uc02-b1-casos-de-uso.md) |
