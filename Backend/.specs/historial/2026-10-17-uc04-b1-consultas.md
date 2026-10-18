# UC-04 (b1): consultas de turnos (archivado 2026-10-17)

Commit: `2d5206a`. Bloque archivado tal como estaba en `ESTADO-ACTUAL.md`:

## ⏭️ Próximo paso
**UC-04 (b1)**: según `.specs/use-cases/UC-04-gestionar-turnos.md` (Design, "Capas previstas"): `ListShifts` (filtro `From`/`To`, ambos incluidos) y `GetShift`, con `ListAsync` y `GetByIdAsync` en `IShiftRepository`, `ShiftRepository` y `FakeShiftRepository`; errores `shift.not-found` y `shift.date-range`; tests con fakes. Después, (b2) edición y borrado y (c) endpoints, docs y E2E. Los reintentos de ADR-09 (resilience) quedan para el cliente de festivos.

## Bloque en curso
UC-03 cerrado y archivado. UC-04 en curso: (a) cerrada y archivada; (b1), (b2) y (c) pendientes. Ninguna iteración abierta.

Resumen de lo hecho en (b1) (no estaba en el estado vivo):
- `ShiftErrors.NotFound` y `ShiftErrors.DateRange`.
- `IShiftRepository.ListAsync(from, to)` y `GetByIdAsync`; `ShiftRepository` filtra por rango (extremos incluidos, ausente = sin límite) y ordena por fecha e inicio; `GetByIdAsync` con tracking para que (b2) lo reutilice.
- `ListShifts` (`From > To` → `shift.date-range`) y `GetShift` (inexistente → `shift.not-found`), ambos devuelven `ShiftDto`.
- 8 tests nuevos con `FakeShiftRepository`: 86 verdes, build sin warnings. Sin migración.
- Sin ejercitar contra PostgreSQL: la traducción del filtro con parámetros nulos y el orden se confirman en el E2E de (c).
