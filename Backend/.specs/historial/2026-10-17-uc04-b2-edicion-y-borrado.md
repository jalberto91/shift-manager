# UC-04 (b2): edición y borrado de turnos (archivado 2026-10-17)

Commit: `10627a0`. Bloque archivado tal como estaba en `ESTADO-ACTUAL.md`:

## ⏭️ Próximo paso
**UC-04 (b2)**: según `.specs/use-cases/UC-04-gestionar-turnos.md` (Design, "Capas previstas" y "Reglas fuera del dominio"): `UpdateShift` y `DeleteShift`; `excludingId` en `ListByCollaboratorAndDateAsync` y `Remove` en `IShiftRepository`, `ShiftRepository` y `FakeShiftRepository`; `GetByIdAsync` ya existe (con tracking). Tests con fakes (casos límite del spec: mismos valores, ampliar dentro del hueco, mover a día ocupado, cambiar de colaborador). Después (c): endpoints, `ShiftRequest`, docs, `.http` y E2E. Los reintentos de ADR-09 (resilience) quedan para el cliente de festivos.

## Bloque en curso
UC-03 cerrado y archivado. UC-04 en curso: (a) y (b1) cerradas y archivadas; (b2) y (c) pendientes. Ninguna iteración abierta.

Resumen de lo hecho en (b2) (no estaba en el estado vivo):
- `IShiftRepository`: `ListByCollaboratorAndDateAsync` gana `Guid? excludingId` (mismo patrón que `EmailExistsAsync`) y se agrega `Remove`; `ShiftRepository` y `FakeShiftRepository` los implementan. `CreateShift` pasa `excludingId: null`.
- `UpdateShift`: 404 → validación de forma (`Shift.Update`) → colaborador activo → solape excluyendo el propio turno → guardado.
- `DeleteShift`: borrado físico; inexistente → `shift.not-found`.
- 11 tests nuevos con fakes (9 de edición, 2 de borrado): 97 verdes, build sin warnings. Sin migración.
- Revisado: sin violaciones. Riesgos anotados: el turno rastreado queda mutado en memoria si falla el solape (inofensivo, `DbContext` por request); `PUT`/`DELETE` simultáneos sobre el mismo turno darían `DbUpdateConcurrencyException` y `500`, misma familia de carreras aceptadas para el demo.
- Sin ejercitar contra PostgreSQL: `s.Id != excludingId` con `null` y `Remove` se confirman en el E2E de (c).
