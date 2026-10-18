# UC-06 · Importar turnos desde Excel con informe
> En curso (a: spec, regla compartida y handler; b: lector ClosedXML, endpoint, docs y E2E) · rama `main` · 2026-10-18 · cubre: R4, R3 · decisiones P5, P7, P8, P9 · ADR-08, ADR-10

## Requirements
- Subir un `.xlsx` con turnos y recibir un informe con las filas importadas y las fallidas, con el motivo de cada error.
- El colaborador se identifica por email (P5), sin distinguir mayúsculas, y debe estar activo.
- Import parcial (P7): las filas válidas se guardan aunque otras fallen.
- Se respeta la regla de no solape (R3) contra la BD y dentro del propio archivo (P8): la fila que llega segunda es la que falla.
- Columnas por nombre (`Email`, `Fecha`, `HoraInicio`, `HoraFin`), sin importar orden ni mayúsculas. Fecha y hora como celda de tipo fecha/hora o como texto `yyyy-MM-dd` / `dd/MM/yyyy` y `HH:mm` (P9).
- Límite de tamaño de 5 MB.

Criterios de aceptación:
- [ ] `POST /api/shifts/import` con un `.xlsx` válido → `200` con `{ importedRows: [2, 3], failedRows: [] }` y los turnos guardados.
- [ ] Con filas inválidas → `200` con `importedRows` (filas guardadas) y `failedRows` con `{ row, reasons: [...] }` por cada una; las válidas quedan guardadas.
- [ ] Una fila con varios problemas de forma (p. ej. fecha vacía y fin anterior al inicio) lista todos sus motivos.
- [ ] Email desconocido o de un colaborador dado de baja → fila fallida `shift.collaborator-unavailable`.
- [ ] Fecha u hora ilegible, o celda obligatoria vacía → fila fallida `shift.import.cell`, con el nombre de la columna en el mensaje.
- [ ] Fin no posterior al inicio → fila fallida `shift.time-range` (regla del dominio).
- [ ] Solape con un turno de la BD → fila fallida con el motivo de `shift.overlap`.
- [ ] Solape con una fila anterior aceptada del mismo archivo → fila fallida con el motivo "overlaps an earlier row of the same file".
- [ ] Faltan columnas obligatorias → `400` `shift.import.columns`, nada se guarda.
- [ ] Archivo que no es un `.xlsx` legible → `400` `shift.import.file`; mayor de 5 MB → `400` `shift.import.file-too-large`.
- [ ] Sin archivo en el formulario → `400` `shift.import.file`.

Casos límite:
- Filas totalmente vacías se ignoran (no cuentan ni como importadas ni como fallidas).
- Número de fila en el informe = número de fila de Excel (el encabezado es la fila 1, los datos empiezan en la 2).
- Dos filas idénticas: la primera se importa, la segunda falla por solape.
- Turnos que solo se tocan en el borde (`08:00-12:00` y `12:00-16:00`) no se solapan (regla de `Shift.OverlapsWith`).
- Email con espacios o mayúsculas (`" Ana@Mail.com "`) se normaliza antes de buscar.
- Archivo solo con encabezado → `200` con `importedRows = []` y `failedRows = []`.

## Design
**Endpoint** (`api/shifts`):

| Método | Ruta | Éxito | Errores |
|---|---|---|---|
| `POST` | `/api/shifts/import` (`multipart/form-data`, campo `file`) | `200` + `ImportReportDto` | `400` |

`200` aunque haya filas fallidas: la petición se procesó y el resultado es el informe.

```json
{ "importedRows": [2, 3], "failedRows": [ { "row": 4, "reasons": ["The shift overlaps another shift of the same collaborator."] } ] }
```

El informe es lo que pide R4 y nada más: qué filas se importaron y, de las que fallaron, el motivo de cada error. `reasons` lleva los mensajes (sin código ni `conflictingShiftId`): para el usuario del Excel basta el texto, y el código sigue existiendo en las respuestas de `POST`/`PUT`.

**Puerto de lectura** `IShiftSheetReader` (`Application/Abstractions`): recibe un `Stream` y devuelve `Result<IReadOnlyList<ShiftSheetRow>>`.
- `ShiftSheetRow(int RowNumber, string Email, DateOnly Date, TimeOnly StartTime, TimeOnly EndTime)` cuando la fila se leyó bien, o la misma fila con un `Error` de celda (`shift.import.cell`) cuando no. Una sola forma con `Error?` evita dos tipos.
- Errores del archivo entero (no es `.xlsx`, faltan columnas) → `Result` fallido.
- El handler no sabe nada de Excel: se prueba con un fake a mano. La implementación con ClosedXML es de la iteración (b) (ADR-08).

**Regla compartida** `ShiftPlacementChecker` (`Application/Shifts`, servicio concreto registrado en `Program.cs`): `CheckAsync(Shift, Guid? excludingId, ct)` → `Error?`.
- Comprueba que el colaborador exista y esté activo (`ICollaboratorRepository.GetByIdAsync`) y que no haya solape contra la BD (`IShiftRepository.ListByCollaboratorAndDateAsync` + `Shift.OverlapsWith`).
- `CreateShiftHandler`, `UpdateShiftHandler` e `ImportShiftsHandler` lo usan: es la tercera repetición que UC-04 dejó anotada para extraer.
- No es un `IRepository` ni una abstracción nueva: es la lógica que ya estaba en los handlers, movida a un solo lugar.

**Resolución del email**: `ICollaboratorRepository.GetIdsByEmailsAsync(IReadOnlyCollection<string> emails, ct)` → `IReadOnlyDictionary<string, Guid>` (email en minúsculas → id, como `GetCountryCodesAsync`), una consulta para todo el archivo. Un email que no esté en el diccionario es `shift.collaborator-unavailable`; que esté inactivo lo detecta el checker.

**`ImportShiftsHandler`** (`Application/Shifts/ImportShifts/`):
1. Lee las filas con `IShiftSheetReader`; si falla, devuelve ese error.
2. Resuelve todos los colaboradores con una consulta.
3. Por cada fila, en orden: error de celda → fallida; colaborador no encontrado → fallida; `Shift.Create` → fallida si la forma es inválida; solape con una fila ya aceptada del archivo (lista en memoria) → fallida; `ShiftPlacementChecker` (BD) → fallida o aceptada.
4. `shifts.Add` de las aceptadas y un único `SaveChangesAsync`.
5. Devuelve `ImportReportDto`.

Con `Result` en vez de excepciones para cada fila: el informe es un resultado esperado, no un error.

**Errores nuevos** (`ShiftErrors`): `shift.import.cell` (por fila) y `OverlapsEarlierRow` (código `shift.overlap`) para el solape con una fila anterior del archivo, ambos en (a); `shift.import.file`, `shift.import.file-too-large` y `shift.import.columns` (validación, `400`) llegan en (b), donde los usan el lector y el controller. Reutiliza `shift.collaborator-unavailable`, `shift.time-range`, `shift.date` y `shift.overlap`.

**Capas previstas**
- Domain: errores nuevos en `ShiftErrors`.
- Application: `IShiftSheetReader`, `ShiftPlacementChecker`, `ImportShiftsCommand`/`Handler`, `ImportReportDto`; `GetByEmailsAsync` en `ICollaboratorRepository`; `CreateShift` y `UpdateShift` pasan a usar el checker.
- Infrastructure: `ClosedXmlShiftSheetReader` (b) y `GetByEmailsAsync` en el repositorio. Sin migración.
- Api: acción `Import` en `ShiftsController` (b).

**Decisiones y supuestos**
- Solo se lee la primera hoja. Varias hojas no están pedidas.
- Un email desconocido y un colaborador dado de baja dan el mismo error que en el alta (`shift.collaborator-unavailable`): menos códigos y mismo contrato.
- El solape dentro del archivo se resuelve en memoria con la misma `Shift.OverlapsWith`. No se guarda fila a fila porque un solo `SaveChanges` es más simple y atómico.
- Las filas aceptadas no se consideran guardadas hasta el final; si `SaveChanges` falla, la petición falla entera (`500`), igual que cualquier otro handler.
- No se detectan duplicados de un mismo turno ya existente como caso especial: caen en el solape.
- El tamaño se valida en la capa Api antes de abrir el archivo.

## Tasks
- [x] Domain: `InvalidCell` y `OverlapsEarlierRow` en `ShiftErrors` (iteración a)
- [x] Application: `ShiftPlacementChecker` y refactor de `CreateShift`/`UpdateShift` (iteración a)
- [x] Application + Infrastructure: `IShiftSheetReader`, `ImportShifts` y `GetIdsByEmailsAsync` (iteración a)
- [x] Tests: handler con fake del lector (solape en archivo, solape en BD, email desconocido/inactivo, celda inválida, parcial); el checker queda cubierto por los tests de Create y Update (iteración a)
- [ ] Infrastructure: `ClosedXmlShiftSheetReader`; instalar ClosedXML 0.105.1 (iteración b)
- [ ] Tests: lector con celdas fecha vs. texto, columnas en otro orden, mayúsculas (iteración b)
- [ ] Api: acción `Import` con límite de 5 MB (iteración b)
- [ ] Docs: `docs/api/ENDPOINTS.md`, `shifts.http` y `.xlsx` de ejemplo (iteración b)

## Verificación
Iteración (a): build sin warnings y 124 tests verdes (112 previos + 12 del handler). Pendiente de (b): `docker compose up --build` y subida del `.xlsx` de ejemplo (iteración b).

## TBDs
1. Si el negocio necesita importar archivos grandes (miles de filas), habría que consultar los turnos de la BD en lote en vez de uno por fila.
2. Si el negocio quiere reimportar el mismo archivo sin errores por duplicado, hay que definir qué es un duplicado (hoy cae como solape).

## Para la revisión
- El handler de import no conoce Excel: lee un puerto y se prueba con un fake a mano. ClosedXML vive solo en Infrastructure (ADR-08).
- Las comprobaciones de colaborador activo y solape se extraen a un servicio la tercera vez que se necesitan, como se anotó en UC-04; Create, Update e Import no las copian.
- Import parcial con informe por fila: un resultado de negocio esperado, por eso `200` y `Result` por fila en vez de excepciones.
- El solape dentro del archivo usa la misma `Shift.OverlapsWith` que contra la BD, sobre una lista en memoria; la fila que llega segunda es la que falla.
- Un solo `SaveChanges` al final: simple y atómico para lo aceptado.
