# UC-06 (b1): lector ClosedXML del import (archivado 2026-10-18)

Commit: `b617dae`. Bloque archivado tal como estaba en `ESTADO-ACTUAL.md`:

## ⏭️ Próximo paso
**UC-06 (b) · Lector ClosedXML, endpoint, docs y E2E del import de turnos (R4, R3)**. Spec: `.specs/use-cases/UC-06-importar-turnos.md` (Design completo; Tasks de (b) sin marcar). Ya está hecho en (a): ADR-08 aceptado (ClosedXML 0.105.1, **todavía sin instalar**), `ShiftPlacementChecker`, puerto `IShiftSheetReader` + `ShiftSheetRow`, `ImportShiftsHandler` con `ImportReportDto` y `GetIdsByEmailsAsync`. Falta, en este orden: (1) instalar ClosedXML 0.105.1 en Infrastructure y revisar en el `restore` la licencia de sus dependencias (DocumentFormat.OpenXml, SixLabors.Fonts); (2) `ClosedXmlShiftSheetReader` (primera hoja, encabezados `Email`/`Fecha`/`HoraInicio`/`HoraFin` por nombre sin importar orden ni mayúsculas, fecha y hora como celda o texto `yyyy-MM-dd` / `dd/MM/yyyy` y `HH:mm`, filas vacías ignoradas, número de fila = el de Excel, celda de email vacía → cadena vacía y no `null`) con sus tests de celdas fecha vs. texto; (3) errores `shift.import.file`, `shift.import.file-too-large` y `shift.import.columns` en `ShiftErrors`; (4) acción `POST /api/shifts/import` (`multipart/form-data`, campo `file`, límite 5 MB validado antes de abrir el archivo, `200` con el informe, `400` si el archivo no sirve) y registro del lector en `DependencyInjection.cs`; (5) `docs/api/ENDPOINTS.md`, `shifts.http` con respuestas reales y el `.xlsx` de ejemplo en el repo; (6) E2E con la skill `verify` sobre `docker compose up --build` (filas válidas, solape en archivo, solape contra BD, email desconocido, fecha en texto y celda fecha) y cierre del UC. Si (b) crece, cortarla en (b1) lector + tests y (b2) endpoint + docs + E2E. No encadenar sin "seguí".

## Bloque en curso
UC-06 (a) cerrado y archivado. UC-06 (b) sin empezar. Ninguna iteración abierta.

Resumen de lo hecho en (b1) (no estaba en el estado vivo):
- (b) se cortó en (b1) lector + tests y (b2) endpoint + docs + E2E.
- ClosedXML 0.105.1 instalado en Infrastructure. Licencias de sus dependencias revisadas en el `restore` y anotadas en ADR-08: MIT (ClosedXML.Parser, DocumentFormat.OpenXml, ExcelNumberFormat, RBush.Signed) y Apache-2.0 (SixLabors.Fonts 1.0.0).
- `Infrastructure/Spreadsheets/ClosedXmlShiftSheetReader`: primera hoja, columnas por nombre sin importar orden ni mayúsculas (las extra se ignoran), fecha como celda o texto `yyyy-MM-dd` / `dd/MM/yyyy`, hora como celda (fecha/hora o duración) o texto `HH:mm`, con cultura invariante. Filas vacías ignoradas y número de fila = el de Excel. Una celda ilegible o un email vacío marca la fila con `InvalidCell(columna)` (primera columna que falla). Una duración de 24 h o más se trata como celda ilegible (`TimeOnly.FromTimeSpan` lanzaría excepción).
- `ShiftErrors.InvalidFile` (`shift.import.file`) y `MissingColumns` (`shift.import.columns`). `shift.import.file-too-large` queda para (b2), donde lo usa el controller.
- El email vacío ya no llega al handler: el lector lo marca como celda ilegible, así que `NormalizeEmail` nunca recibe una cadena vacía por esa vía.
- 136 tests verdes (124 previos + 12 del lector, con workbooks construidos en memoria con ClosedXML) y build sin warnings. Sin E2E: el lector no está registrado en DI ni hay endpoint.
- El lector se registra en DI en (b2), junto al endpoint que lo consume.
