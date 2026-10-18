# UC-06 (a): spec, ADR-08 y handler del import (archivado 2026-10-18)

Commit: `0d9c089`. Bloque archivado tal como estaba en `ESTADO-ACTUAL.md`:

## ⏭️ Próximo paso
**UC-06 · Importar turnos desde Excel con informe (R4, R3)**. Todavía no tiene spec: empezar por la Tarea 0 de la skill `implement-use-case` (spec `.specs/use-cases/UC-06-importar-turnos.md` desde `_TEMPLATE-UC.md`, Requirements → Design → Tasks y "Para la revisión"). Leer antes: `00-requisitos.md` (R4, P5, P7, P8, P9), ADR-08 y ADR-10 en `architecture.md`, `UC-03-alta-de-turno.md` y `UC-04-gestionar-turnos.md`. Orden: (1) **`license-check` de ClosedXML y MiniExcel** (ADR-08 está "Propuesto"): mostrar la tabla y esperar el OK antes de instalar nada, y probar lectura de celdas fecha vs. texto; (2) cerrar ADR-08 con la decisión (ADR antes que código); (3) spec con: columnas por nombre sin importar orden ni mayúsculas, fecha como celda fecha o texto `yyyy-MM-dd` / `dd/MM/yyyy`, hora como celda o `HH:mm` (P9), límite de tamaño (~5 MB), import parcial (P7), solape dentro del archivo y contra la BD con falla en la fila que llega segunda (P8), colaborador por email normalizado y solo activos, e informe `importadas` / `fallidas` con fila y motivo. Puntos a decidir: contrato de `POST /api/shifts/import` (`multipart/form-data`, `200` con informe aunque haya filas fallidas), y la **tercera repetición** de las comprobaciones de colaborador activo y solape (`CreateShift`, `UpdateShift`, ahora el import): UC-04 dejó anotado extraerlas en ese momento, así que lo razonable es un puerto o servicio de aplicación compartido en vez de copiar. El `.xlsx` de ejemplo del repo (checklist del proyecto) sale de este UC. La iteración (a) es spec, ADR-08 y la regla de validación por fila; no encadenar con (b) sin "seguí". Ojo: P6 (bloquear el borrado con turnos) quedó reemplazada por ADR-11 (baja lógica).

## Bloque en curso
UC-05 cerrado y archivado. UC-06 sin empezar (falta su spec). Ninguna iteración abierta.

Resumen de lo hecho en (a) (no estaba en el estado vivo):
- `license-check`: ClosedXML 0.105.1 (MIT) y MiniExcel 1.46.0 (Apache-2.0), ambas aprobadas; se elige ClosedXML. ADR-08 pasa a Aceptado. El paquete no se instala todavía: entra en (b), donde se usa.
- Spec `UC-06-importar-turnos.md`. El informe es lo que pide R4 y nada más: `{ importedRows: [..], failedRows: [{ row, reasons: [..] }] }`, con `reasons` en mensajes (todos los de la fila) y sin código ni `conflictingShiftId`.
- `ShiftPlacementChecker` (`Application/Shifts`): colaborador activo y solape contra la BD, extraído de `CreateShift` y `UpdateShift` (tercera repetición anotada en UC-04). Registrado en `Program.cs`.
- Puerto `IShiftSheetReader.Read(Stream)` → `Result<IReadOnlyList<ShiftSheetRow>>`; `ShiftSheetRow` lleva `CellError?` para filas ilegibles (`Unreadable`).
- `ICollaboratorRepository.GetIdsByEmailsAsync` (email en minúsculas → id, una consulta por archivo) y su implementación.
- `ImportShiftsHandler`: lee filas, resuelve emails, valida por fila (celda, colaborador, forma, solape con filas aceptadas del archivo, solape contra la BD) y hace un solo `SaveChangesAsync` con las aceptadas. `ShiftErrors.InvalidCell` y `OverlapsEarlierRow`.
- 124 tests verdes (112 previos + 12 del handler) y build sin warnings. Sin E2E: no existe el lector real ni el endpoint.
- Revisado con `/revisa` y `/comentarios`: sin violaciones ni anti-patrones; sin comentarios en el código nuevo.
- Riesgo anotado: el lector de (b) debe devolver cadena vacía (no `null`) en una celda de email vacía, porque `NormalizeEmail` hace `Trim` sobre un `string` no nulo.
