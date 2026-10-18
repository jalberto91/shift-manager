# UC-07 (a): spec, puerto de escritura y handler del export (archivado 2026-10-18)

Commit: `f31ea6e`. Bloque archivado tal como estaba en `ESTADO-ACTUAL.md`:

## ⏭️ Próximo paso
**UC-07 · Exportar turnos por rango a Excel (R5)**. Todavía no tiene spec: empezar por la Tarea 0 de la skill `implement-use-case` (spec `.specs/use-cases/UC-07-exportar-turnos.md` desde `_TEMPLATE-UC.md`, Requirements → Design → Tasks y "Para la revisión"). Leer antes: `00-requisitos.md` (R5 y la fila de UC-07: "reutiliza filtro de UC-04"), ADR-08 en `architecture.md` (ClosedXML 0.105.1, ya instalado y detrás de un puerto de `Application`), `UC-04-gestionar-turnos.md` (filtro `from`/`to`, extremos incluidos, errores `shift.date-range`), y de `UC-06-importar-turnos.md` el Design del puerto `IShiftSheetReader` y de `ClosedXmlShiftSheetReader`. Puntos a decidir: contrato de `GET /api/shifts/export?from&to` (descarga `.xlsx` con `Content-Type` de Excel y nombre de archivo; mismos `400` que el listado); columnas y orden (lo razonable es las mismas que pide el import: `Email`, `Fecha`, `HoraInicio`, `HoraFin`, para que lo exportado se pueda volver a importar, lo que obliga a resolver el email del colaborador porque `ShiftDto` solo trae `collaboratorId`; decidir si va una columna extra con el nombre o la marca de festivo, o nada más de lo que pide R5); formato de fecha y hora como celdas reales de Excel; si se reutiliza `ListShiftsHandler` o solo la consulta `IShiftRepository.ListAsync`; y el puerto de escritura (p. ej. `IShiftSheetWriter` en `Application/Abstractions`, implementado con ClosedXML en `Infrastructure/Spreadsheets`, a la medida del lector). No hace falta paquete nuevo. La iteración (a) es spec, puerto y handler con tests; no encadenar con (b) (endpoint, docs, E2E) sin "seguí". Si el endpoint devuelve un archivo, `ENDPOINTS.md` y `shifts.http` deben decir cómo probarlo (`curl -o`).

## Bloque en curso
UC-06 cerrado y archivado. UC-07 sin empezar (falta su spec). Ninguna iteración abierta.

Resumen de lo hecho en (a) (no estaba en el estado vivo):
- Spec `UC-07-exportar-turnos.md`. Columnas: las mismas del import (`Email`, `Fecha`, `HoraInicio`, `HoraFin`), sin nombre ni marca de festivo (R5 solo pide los turnos del rango), así que el export no depende de Nager.Date. Nombre de archivo fijo `shifts.xlsx`, sin paginación ni streaming.
- Puerto `IShiftSheetWriter.Write(IReadOnlyList<ShiftExportRow>)` → `byte[]` y `ShiftExportRow(Email, Date, StartTime, EndTime)`, tipo propio (el `ShiftSheetRow` del import lleva número de fila y un error de celda que no sirven al escribir).
- `ExportShiftsQuery` / `ExportShiftsHandler` (`Result<byte[]>`): valida `from > to` (`shift.date-range`), usa `IShiftRepository.ListAsync` (filtro de UC-04) y resuelve los emails con `ICollaboratorRepository.GetEmailsAsync` (una consulta, incluye inactivos). No reutiliza `ListShiftsHandler` para no depender de Nager.Date.
- `GetEmailsAsync` en `CollaboratorRepository` y en el fake de los tests.
- 141 tests verdes (136 previos + 5 del handler) y build sin warnings. Sin E2E: no hay escritor ni endpoint.
- Revisado con `/revisa`: sin violaciones ni anti-patrones.
- 🚨 Aceptado por decisión del usuario: hasta que (b) registre `IShiftSheetWriter`, la API **no arranca en Development** (la validación de DI al construir el host falla por `ExportShiftsHandler`; verificado levantándola con `ASPNETCORE_ENVIRONMENT=Development`). En Production no se valida al arrancar, así que el compose seguiría funcionando con la imagen actual. Anotado en la sección Verificación de la spec.
- Duplicación aceptada: la comprobación `from > to` está en `ListShiftsHandler` y en `ExportShiftsHandler` (tres líneas); se extrae si aparece una tercera.
