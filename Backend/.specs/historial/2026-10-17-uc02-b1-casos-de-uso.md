# UC-02 (b1): casos de uso de escritura de colaboradores (archivado 2026-10-17)

Commit: `e9c5cff`. Bloque archivado tal como estaba en `ESTADO-ACTUAL.md`:

## Bloque en curso
UC-02 en curso: (a) cerrada y archivada; (b) y (c) pendientes. Ninguna iteración abierta.

Resumen de lo hecho en (b1) (no estaba en el estado vivo):
- (b) se cortó en b1 y b2. `NagerDateCountryCatalog` pasó de (c) a b1: el contenedor de DI valida los handlers y sin una implementación de `ICountryCatalog` la API (y `dotnet-ef`) no arrancaba.
- Application: `ICountryCatalog`, `ICollaboratorRepository` ampliado, `Create/Update/DeactivateCollaborator`, `CollaboratorDto.From`.
- Infrastructure: repositorio (`ListAsync` filtra `IsActive`), índice único de email, migración `AddCollaboratorEmailUniqueIndex`, `NagerDateCountryCatalog` (timeout 5 s, caché 24 h, `null` + warning si falla), paquete `Microsoft.Extensions.Http` 9.0.21 (MIT).
- `PUT` sobre un colaborador inactivo responde `404` (spec y handler); `DELETE` sigue idempotente. ADR-09 con nota de ajuste.
- Build sin warnings, 52 tests verdes. `docker compose up` aplicó ambas migraciones sobre la semilla y `GET /api/collaborators` devolvió los 5 colaboradores.
