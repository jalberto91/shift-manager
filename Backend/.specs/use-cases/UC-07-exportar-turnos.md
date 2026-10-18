# UC-07 · Exportar turnos por rango a Excel
> Completo (a: spec, puerto de escritura y handler; b1: escritor ClosedXML; b2: endpoint, docs y E2E) · rama `main` · 2026-10-18 · cubre: R5 · decisiones P9 (formato de celdas) · ADR-08

## Requirements
- Descargar un `.xlsx` con los turnos de un rango de fechas (R5).
- El rango sigue la misma regla que el listado (UC-04): `from` y `to` opcionales, extremos incluidos, uno solo vale.
- El archivo usa las mismas columnas que el import (UC-06), así que lo exportado se puede volver a importar.

Criterios de aceptación:
- [x] `GET /api/shifts/export?from=2026-03-01&to=2026-03-31` → `200` con un `.xlsx` que trae solo los turnos con `from <= date <= to`.
- [x] Sin `from` ni `to` → todos los turnos.
- [x] Una fila por turno con `Email`, `Fecha`, `HoraInicio` y `HoraFin`, ordenadas por fecha y hora de inicio.
- [x] `Fecha` y las horas son celdas reales de Excel (no texto), con formato `yyyy-mm-dd` y `hh:mm`.
- [x] `from` posterior a `to` → `400` `shift.date-range`.
- [x] Rango sin turnos → `200` con un archivo que solo tiene el encabezado.
- [x] El archivo exportado se importa con el import de UC-06 (en una BD vacía de esos turnos).

Casos límite:
- Los turnos de colaboradores dados de baja se exportan (son historial, ADR-11).
- Un rango de un solo día (`from == to`) exporta los turnos de ese día.
- Un `from` o `to` con formato inválido lo rechaza el binder con `400`, igual que en el listado.

## Design
**Endpoint** (`api/shifts`):

| Método | Ruta | Éxito | Errores |
|---|---|---|---|
| `GET` | `/api/shifts/export?from&to` | `200` + `.xlsx` (`application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`, `Content-Disposition: attachment; filename="shifts.xlsx"`) | `400` |

**Columnas** (primera hoja, encabezado en la fila 1): `Email`, `Fecha`, `HoraInicio`, `HoraFin`. Nada más de lo que pide R5: no se exporta la marca de festivo ni el nombre, así que el export no depende de Nager.Date.

**Puerto de escritura** `IShiftSheetWriter.Write(IReadOnlyList<ShiftExportRow>)` → `byte[]` (`Application/Abstractions`), a la medida del lector de UC-06. `ShiftExportRow(Email, Date, StartTime, EndTime)` es un tipo propio: el `ShiftSheetRow` del import lleva número de fila y un error de celda que no tienen sentido al escribir. El handler no conoce Excel; la implementación con ClosedXML es de la iteración (b).

**`ExportShiftsHandler`** (`Application/Shifts/ExportShifts/`), consulta que devuelve `Result<byte[]>`:
1. `from > to` → `shift.date-range` (la misma regla y el mismo error que el listado).
2. `IShiftRepository.ListAsync(from, to)` (ya ordena por fecha y hora de inicio; es el filtro de UC-04).
3. `ICollaboratorRepository.GetEmailsAsync(ids)` resuelve los emails de una vez, como `GetCountryCodesAsync` (incluye inactivos).
4. Arma las filas y las pasa al escritor.

No reutiliza `ListShiftsHandler`: ese consulta festivos a Nager.Date, que el export no necesita.

**Errores**: ninguno nuevo; `shift.date-range` ya existe.

**Capas previstas**
- Domain: nada.
- Application: `IShiftSheetWriter`, `ShiftExportRow`, `ExportShifts` (query + handler) y `GetEmailsAsync` en `ICollaboratorRepository`.
- Infrastructure: `GetEmailsAsync` en el repositorio (a) y `ClosedXmlShiftSheetWriter` (b). Sin migración ni paquete nuevo.
- Api: acción `Export` en `ShiftsController` (b), que devuelve `File(bytes, contentType, "shifts.xlsx")`.

**Decisiones y supuestos**
- Solo se exportan las cuatro columnas del import: R5 pide los turnos del rango y nada más. Si el front quisiera nombre o festivo, es una decisión nueva.
- El nombre del archivo es fijo (`shifts.xlsx`); no se arma con el rango.
- El archivo se construye en memoria y se devuelve entero. Sin paginación ni streaming: el rango acota el volumen, como en el listado (P13).
- La comprobación `from > to` se repite con `ListShiftsHandler` (tres líneas). Si aparece una tercera, se extrae.

## Tasks
- [x] Application: `IShiftSheetWriter`, `ShiftExportRow`, `ExportShifts` y `GetEmailsAsync` (iteración a)
- [x] Infrastructure: `GetEmailsAsync` en `CollaboratorRepository` (iteración a)
- [x] Tests: handler con fake del escritor (filas con email, rango con extremos incluidos, `from > to`, rango vacío, colaborador dado de baja) (iteración a)
- [x] Infrastructure: `ClosedXmlShiftSheetWriter` y su registro en DI (iteración b1)
- [x] Tests: escritor con ida y vuelta contra `ClosedXmlShiftSheetReader` (iteración b1)
- [x] Api: acción `Export` (iteración b2)
- [x] Docs: `docs/api/ENDPOINTS.md` y `shifts.http` con la forma de descargar (`curl -o`) (iteración b2)
- [x] E2E con `verify` y sección Verificación (iteración b2)

## Verificación
Build sin warnings y 145 tests verdes (141 de (a) + 4 del escritor). E2E con `docker compose up --build` sobre la BD de pruebas (22 turnos): sin filtro 22 filas; `from=2026-03-01&to=2026-03-31` 3 filas, las mismas que el listado; un solo día (`2026-02-28`) 1 fila; rango sin turnos solo encabezado; `from > to` `400` con `shift.date-range`; `from=xx` `400` del binder. Cabeceras: `Content-Type` xlsx y `Content-Disposition: attachment; filename=shifts.xlsx`. El archivo completo se reimporta: las 22 filas se leen y fallan solo por solape (los turnos ya existen). La API arranca en Development en cuanto hay BD (la validación de DI ya pasa).

## TBDs
1. Si el front necesita el nombre del colaborador o la marca de festivo en el archivo, hay que ampliar las columnas (hoy exactamente las del import).

## Para la revisión
- El export usa las mismas columnas que el import: lo exportado se puede volver a importar y el contrato del archivo es uno solo.
- El handler no conoce Excel: escribe a través de un puerto, igual que el import lee a través de otro. ClosedXML vive solo en Infrastructure (ADR-08).
- Reutiliza el filtro de UC-04 (`IShiftRepository.ListAsync`) y su error `shift.date-range`; no usa `ListShiftsHandler` para no depender de Nager.Date.
- Nada más de lo que pide R5: sin columnas extra, sin nombre de archivo calculado, sin streaming.
