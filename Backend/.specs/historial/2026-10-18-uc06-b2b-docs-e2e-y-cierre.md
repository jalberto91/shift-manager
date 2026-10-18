# UC-06 (b2b): docs, plantilla `.xlsx`, E2E y cierre del UC (archivado 2026-10-18)

Commit: `870b4af`. Bloque archivado tal como estaba en `ESTADO-ACTUAL.md`:

## ⏭️ Próximo paso
**UC-06 (b2b) · Docs, `.xlsx` de ejemplo y E2E del import de turnos (R4, R3)**. Spec: `.specs/use-cases/UC-06-importar-turnos.md` (quedan sin marcar las Tasks de Docs y E2E, y la sección Verificación). Ya está hecho todo el código: (a) ADR-08, `ShiftPlacementChecker`, puerto, `ImportShiftsHandler`; (b1) `ClosedXmlShiftSheetReader`; (b2a) `POST /api/shifts/import` con límite de 5 MB y lector registrado en DI. Falta, en este orden: (1) el `.xlsx` de ejemplo en el repo (con filas válidas, un solape dentro del archivo, un email desconocido y una fecha en texto; generarlo con un script o programa descartable, sin dejar código muerto en `src`); (2) `docs/api/ENDPOINTS.md` (contrato `multipart/form-data`, campo `file`, límite de 5 MB, informe `{ importedRows, failedRows: [{ row, reasons }] }`, los `400` y la nota de que Kestrel responde `413` por encima de ~30 MB) y `shifts.http` con respuestas reales; (3) E2E con la skill `verify` sobre `docker compose up --build` (la imagen del compose es de UC-05): filas válidas, solape en archivo, solape contra BD, email desconocido y de baja, fecha en texto y celda fecha, archivo que no es `.xlsx`, columnas faltantes, archivo de más de 5 MB, campo `file` ausente; (4) cierre del UC: sección Verificación, Tasks marcadas y TBDs. Si crece, cortarla en (b2b1) `.xlsx` + docs y (b2b2) E2E + cierre. No encadenar sin "seguí".

## Bloque en curso
UC-06 (a), (b1) y (b2a) cerrados y archivados. UC-06 (b2b) sin empezar. Ninguna iteración abierta.

Resumen de lo hecho en (b2b) (no estaba en el estado vivo):
- **UC-06 cerrado.** Spec con criterios y Tasks marcados y sección Verificación.
- E2E con curl sobre `docker compose up --build` (136 tests verdes, build sin warnings):
  - Plantilla `docs/samples/shifts-import-example.xlsx`: `200` con `importedRows [2,3,4,5,6,7]` y `failedRows []`; reimportada, las seis filas fallan por solape contra la BD.
  - Archivo de prueba (fuera del repo, en el scratchpad) con texto `dd/MM/yyyy`, `yyyy-MM-dd` y `HH:mm`, un solape en el archivo, un email desconocido y un fin anterior al inicio: `importedRows [2,3,4,8]`, fallan 5, 6 y 7.
  - Archivo con columnas en otro orden y en minúsculas, un email con mayúsculas y espacios, un colaborador dado de baja, un email vacío, una fecha `10 March`, una hora `nine` y una hora `25:00`: cada fila falla o se importa como corresponde, el mensaje nombra la columna y la fila vacía conserva la numeración de Excel.
  - `400`: columnas faltantes (`shift.import.columns`), archivo que no es `.xlsx` (`shift.import.file`), 6 MB (`shift.import.file-too-large`) y campo `file` ausente (`shift.import.file`).
  - Sin ejercitar por HTTP: más de ~30 MB (Kestrel responde `413`; documentado).
- La plantilla `.xlsx` es solo documentación (`docs/samples/`): ningún código ni test la lee y la API no la sirve. Seis turnos válidos para los colaboradores de la semilla (2026-07-06 a 2026-07-08), con la fecha y las horas como celdas reales de Excel. Se regeneró una vez porque la primera mezclaba texto y celdas, que confundía al abrirla.
- `docs/api/ENDPOINTS.md` (sección UC-06), `docs/api/shifts.http` y enlace en `docs/README.md`.
- TBD 3 de la spec: un `file` ausente y un archivo ilegible comparten `shift.import.file` y el mensaje.
- La BD del compose quedó con turnos de mayo, junio y julio de 2026 y `baja.e2e@example.com` dado de baja; `docker compose down -v` los limpia.
