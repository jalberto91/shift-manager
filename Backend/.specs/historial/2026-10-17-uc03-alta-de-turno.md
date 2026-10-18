# UC-03: alta de turno con regla de no solape (archivado 2026-10-17)

Commits: (a) `857f2df`, (b) `36c1c56`, (c) `10471c9`. Bloque archivado tal como estaba en `ESTADO-ACTUAL.md`:

## Bloque en curso
UC-02 cerrado y archivado. UC-03: (a) y (b) commiteadas (`857f2df`, `36c1c56`); (c) hecha (persistencia, migración `AddShifts`, `POST /api/shifts`, docs, `shifts.http` y E2E), pendiente de commit y de archivar. Ninguna iteración abierta.

Resumen de lo hecho en UC-03 (no estaba en el estado vivo):
- (a) `Shift` en `Domain/Shifts/` con `Create` (valida fecha y `endTime > startTime`, acumula errores) y `OverlapsWith` (intervalos semiabiertos, P2); `ShiftErrors` con `shift.date`, `shift.time-range` y `Overlap(conflictingShiftId)`.
- (b) `CreateShiftHandler`: valida la forma, exige colaborador activo (`shift.collaborator-unavailable`, `400`) y aplica `OverlapsWith` en memoria sobre los turnos del colaborador y día (`IShiftRepository.ListByCollaboratorAndDateAsync`). Sin `excludingId` hasta que UC-04 lo necesite.
- (c) `ShiftConfiguration` (FK sin cascada, índice `(CollaboratorId, Date)`), `ShiftRepository`, migración `AddShifts`, `ShiftsController` + `CreateShiftRequest`, `docs/api/shifts.http` y sección en `ENDPOINTS.md`.
- Verificado con curl sobre `docker compose up --build`: `201`, `409` con `conflictingShiftId`, borde compartido `201`, otro colaborador `201`, `400` combinado, colaborador inexistente y dado de baja `400`. Build sin warnings, 75 tests verdes.
- Límites aceptados para el demo (no se harán): carrera de altas solapadas simultáneas (TBD 1) y de email único (TBD 3 de UC-02); hora o fecha mal formada la rechaza el binder con `400` y la ruta JSON como clave (TBD 2).
- Quedaron en la BD de pruebas turnos del 2026-03-10 de Ana y Bruno y colaboradores dados de baja (`test.user@…`, `temp.baja@…`).
