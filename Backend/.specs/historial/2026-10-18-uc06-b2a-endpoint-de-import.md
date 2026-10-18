# UC-06 (b2a): endpoint de import (archivado 2026-10-18)

Commit: `58fa5db`. Bloque archivado tal como estaba en `ESTADO-ACTUAL.md`:

## ⏭️ Próximo paso
**UC-06 (b2) · Endpoint, docs y E2E del import de turnos (R4, R3)**. Spec: `.specs/use-cases/UC-06-importar-turnos.md` (Design completo; quedan sin marcar las Tasks de Api y Docs). Ya está hecho: (a) ADR-08, `ShiftPlacementChecker`, puerto `IShiftSheetReader`, `ImportShiftsHandler` con `ImportReportDto`; (b1) `ClosedXmlShiftSheetReader`, ClosedXML 0.105.1 instalado, `shift.import.file` y `shift.import.columns`. Falta, en este orden: (1) `shift.import.file-too-large` en `ShiftErrors` y registro de `IShiftSheetReader` → `ClosedXmlShiftSheetReader` en `Infrastructure/DependencyInjection.cs` (el handler ya resuelve el puerto por DI); (2) acción `POST /api/shifts/import` en `ShiftsController` (`multipart/form-data`, campo `file` como `IFormFile`, límite 5 MB validado antes de abrir el archivo, `200` con `ImportReportDto`, `400` si falta el archivo, pesa de más o el lector devuelve error; contrato en `Api/Contracts` solo si hace falta) y revisar que `ApiControllerBase.ToProblem` sirva tal cual para esos `400`; (3) `docs/api/ENDPOINTS.md`, `shifts.http` con respuestas reales y el `.xlsx` de ejemplo en el repo (con filas válidas, un solape dentro del archivo, un email desconocido y una fecha en texto); (4) E2E con la skill `verify` sobre `docker compose up --build` (filas válidas, solape en archivo, solape contra BD, email desconocido, fecha en texto y celda fecha, archivo que no es `.xlsx`, columnas faltantes, archivo de más de 5 MB, campo `file` ausente) y cierre del UC con la sección Verificación y Tasks marcadas. Si crece, cortarla en (b2a) endpoint + tests de la acción y (b2b) docs + `.xlsx` + E2E. No encadenar sin "seguí".

## Bloque en curso
UC-06 (a) y (b1) cerrados y archivados. UC-06 (b2) sin empezar. Ninguna iteración abierta.

Resumen de lo hecho en (b2a) (no estaba en el estado vivo):
- (b2) se cortó en (b2a) endpoint y (b2b) docs, `.xlsx` de ejemplo y E2E.
- `POST /api/shifts/import` en `ShiftsController`: `IFormFile? file` (opcional para devolver nuestro error y no el del binder), límite de 5 MB (`MaxImportBytes`) comprobado con `file.Length` antes de abrir el stream. Falta el campo → `400` `shift.import.file`; pesa más → `400` `shift.import.file-too-large`; error del lector (`shift.import.file`, `shift.import.columns`) → `400`; éxito → `200` con `ImportReportDto`. Reutiliza `ToProblem` tal cual.
- `ShiftErrors.FileTooLarge` con mensaje genérico (sin el número de MB, para no acoplar el dominio al límite del controller).
- `IShiftSheetReader` registrado con `ClosedXmlShiftSheetReader` en `Infrastructure/DependencyInjection.cs` (scoped, como los demás servicios).
- 136 tests verdes y build sin warnings. Sin tests de la acción (el proyecto no tiene tests de API; la lógica está cubierta en el handler y el lector) y sin E2E: va en (b2b).
- Límite fuera de nuestro control: un archivo mucho mayor que 5 MB (Kestrel corta alrededor de 30 MB) lo rechaza el servidor con `413` antes de llegar a la validación. Va a `ENDPOINTS.md`.
