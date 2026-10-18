# Estado actual (handoff vivo)

## Forma de trabajo (regla)
Iteraciones chicas. Al terminar cada una: **parar**, mostrar archivos tocados, sugerir mensaje de commit (Conventional Commits, **en inglés**, con scope) y **esperar "seguí"**. El usuario commitea a mano. Nunca encadenar dos iteraciones sin "seguí". Si una crece más de lo previsto, cortarla en dos antes de seguir.

## ⏭️ Próximo paso
**UC-07 (b) · Escritor ClosedXML, endpoint, docs y E2E del export de turnos (R5)**. Spec: `.specs/use-cases/UC-07-exportar-turnos.md` (Design completo; quedan sin marcar las Tasks de (b) y la sección Verificación). Ya está hecho en (a): `IShiftSheetWriter` + `ShiftExportRow`, `ExportShiftsQuery`/`ExportShiftsHandler` (`Result<byte[]>`) y `GetEmailsAsync`. 🚨 **El primer paso es registrar el escritor en DI**: hasta entonces la API no arranca en Development (la validación de DI falla por `ExportShiftsHandler`; en el compose, que es Production, no se valida). Falta, en este orden: (1) `Infrastructure/Spreadsheets/ClosedXmlShiftSheetWriter` (hoja `Shifts`, encabezado `Email`/`Fecha`/`HoraInicio`/`HoraFin` en negrita, `Fecha` y horas como celdas reales con formato `yyyy-mm-dd` y `hh:mm`, columnas ajustadas, como la plantilla `docs/samples/shifts-import-example.xlsx`) y `services.AddScoped<IShiftSheetWriter, ClosedXmlShiftSheetWriter>()` en `Infrastructure/DependencyInjection.cs`; comprobar que la API arranca con `ASPNETCORE_ENVIRONMENT=Development`; (2) tests del escritor con ida y vuelta contra `ClosedXmlShiftSheetReader` (lo escrito se lee igual, también con rango vacío); (3) acción `GET /api/shifts/export?from&to` en `ShiftsController` que devuelve `File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "shifts.xlsx")` y mapea el `400` con `ToProblem` (`from`/`to` mal formados los rechaza el binder); (4) `docs/api/ENDPOINTS.md` (la fila de UC-07 pasa a "hecho") y `shifts.http` con la forma de descargar (`curl -o`); (5) E2E con la skill `verify` sobre `docker compose up --build` (imagen actual: la del import): rango con extremos incluidos, sin filtro, `from > to`, rango vacío (solo encabezado), abrir el archivo y **reimportarlo** (en una BD sin esos turnos, o comprobando que todas las filas dan solape), y cierre del UC con Verificación y Tasks marcadas. Si crece, cortarla en (b1) escritor + DI + tests y (b2) endpoint + docs + E2E. No encadenar sin "seguí".

## Dónde estamos
- Fases 1 y 2, **UC-01 a UC-06 completos** y **UC-07 (a) hecho** (commit `f31ea6e`). Árbol limpio hasta este handoff.
- Build sin warnings, 141 tests verdes. 🚨 La API **no arranca en Development** hasta que (b) registre `IShiftSheetWriter` (decisión aceptada; el compose en Production no valida DI al arrancar y sigue funcionando con la imagen actual). El último E2E (`docker compose up --build`) fue el del import de UC-06: API en `localhost:5080`, BD en `127.0.0.1:5433`, imagen actual. Quedan en la BD de pruebas turnos de marzo (Ana y Bruno), de abril (Carla y Diego), de mayo, junio y julio de 2026, cinco turnos de Ana (2026-01-01, 2026-02-10, 2026-02-28, 2026-04-23 y 2027-01-01) y colaboradores dados de baja (`test.user@…`, `temp.baja@…`, `baja.e2e@example.com`); `docker compose down -v` los limpia. La plantilla `docs/samples/shifts-import-example.xlsx` ya está importada en esa BD (una segunda importación da solape en sus seis filas).
- Existen las migraciones `InitialCreate`, `AddCollaboratorIsActive`, `AddCollaboratorEmailUniqueIndex` y `AddShifts`, y la semilla de 5 colaboradores (`ana.perez`, `bruno.silva`, `carla.gomez`, `diego.hernandez` y `elena.rossi` en `@example.com`). UC-06 no agrega migraciones.
- `ICountryCatalog` y `IHolidayCatalog` tienen implementación (`NagerDateCountryCatalog` y `NagerDatePublicHolidayCatalog`, config `NagerDate:BaseUrl`). `dotnet-ef` 9.0.21 es herramienta local (`.config/dotnet-tools.json`): `dotnet dotnet-ef`.
- Migrar con `dotnet dotnet-ef migrations add <Name> --project src/ShiftManager.Infrastructure --startup-project src/ShiftManager.Api --output-dir Persistence/Migrations`. El `HostAbortedException` que imprime es ruido normal de las herramientas.

## Decisiones vigentes (las que no son obvias leyendo el código)
- Rutas, campos, códigos y mensajes de error de la API, en inglés (`/api/collaborators`, `/api/shifts`, `shift.overlap`).
- Solape → `409` con `errors["shift.overlap"]` y `conflictingShiftId` (ADR-10). Otros errores de negocio → `400`.
- Fechas y horas del turno: `DateOnly` + `TimeOnly`, formatos JSON por defecto de .NET (hora de salida `HH:mm:ss`, de entrada `HH:mm` o `HH:mm:ss`), sin converters propios.
- `Result` admite varios errores; el primero fija el status. `ApiControllerBase` solo mapea errores; cada controller inyecta lo que usa.
- Mediator con vida útil `Scoped`. Los handlers dependen de puertos (`Application/Abstractions`), no del `DbContext`.
- Semilla con seeder vía `Create`, no `HasData`; corre en todos los entornos (aceptado para la prueba).
- Si Nager.Date no responde al validar el país de un colaborador, se acepta el alta con formato ISO alfa-2 y un warning (ADR-09).
- Nager.Date: los clientes usan la **v4** (`api/v4/Holidays/{cc}/{year}`, `api/v4/Countries/Available`), no la v3 que cita el enunciado (deprecada, fin de soporte 2027-01-31); sin resilience handler ni reintentos, timeout de 3 s y fallos sin cachear (ADR-09). `00-requisitos.md` línea 41 conserva las rutas v3 del enunciado.
- El listado de turnos devuelve `ShiftListItemDto` con `holiday { status, name, isGlobal }`; `ShiftDto` (POST, PUT, GET `{id}`) no cambia.
- Colaborador activo y solape contra la BD viven en `ShiftPlacementChecker` (`Application/Shifts`), compartido por `CreateShift`, `UpdateShift` e `ImportShifts`.
- El import lee valores guardados, no lo que muestra Excel: celdas de fecha/hora reales o texto con formatos fijos (cultura invariante), sin depender de la configuración regional.
- El informe del import es lo que pide R4 y nada más: `{ importedRows: [..], failedRows: [{ row, reasons: [..] }] }`, con `reasons` en mensajes (todos los de la fila) y sin código ni `conflictingShiftId`. `200` aunque haya filas fallidas.
- pgAdmin de escritorio en Windows: host `127.0.0.1` y `GSS encryption mode = disable` (ver skill `verify`).

## Bloque en curso
UC-06 cerrado y archivado. UC-07 (a) cerrado y archivado; UC-07 (b) sin empezar. Ninguna iteración abierta.

## Abierto / pendientes con terceros
- ADR-08 resuelto: ClosedXML 0.105.1 (MIT), instalado, con las licencias de sus dependencias revisadas (MIT y Apache-2.0).
- P5: email normalizado a minúsculas + índice único incluyendo inactivos (ADR-11); el índice ya existe (b1).
- Carreras de concurrencia **aceptadas para el demo, no se harán**: email único simultáneo (`DbUpdateException` y `500`, TBD 3 de UC-02) y altas de turnos solapados simultáneas (TBD 1 de UC-03). Van a "qué quedó fuera" del README.
- UC-06 TBDs: el import hace dos consultas por fila (colaborador por id y turnos del día), aceptable con el límite de 5 MB; reimportar el mismo archivo cae como solape, sin definición de duplicado; un `file` ausente y un archivo ilegible comparten `shift.import.file` y el mensaje.
- Un archivo muy por encima de 5 MB (~30 MB) lo corta Kestrel con `413` antes de la validación del import (documentado en `ENDPOINTS.md`); va a "qué quedó fuera" del README.
- UC-07 TBD: si el front necesita nombre o marca de festivo en el archivo exportado, hay que ampliar las columnas (hoy exactamente las del import). La comprobación `from > to` está duplicada en `ListShiftsHandler` y `ExportShiftsHandler`; extraer si aparece una tercera.
- Regla de código: un tipo por archivo. Pendiente separar `Domain/Common/Error.cs`, `Domain/Common/Result.cs` y `tests/.../Application/Fakes.cs` (iteración aparte, a confirmar).
- Cuando el front entre al compose, ajustar `Cors__AllowedOrigins__0` al puerto que publique.

## Checklist de cierre del proyecto
- [ ] Docker completo (`docker compose up` desde `shift-manager/`), con el front
- [ ] Tests revisados (solape, import fila a fila, degradación de Nager.Date)
- [x] `.xlsx` de ejemplo en el repo (`docs/samples/shifts-import-example.xlsx`, plantilla válida; solo documentación)
- [ ] `README.md` en la raíz: cómo ejecutarlo, decisiones (ADRs), qué quedó fuera, **sección "Uso de IA"**
- [ ] Revisión contra "qué valoran" (`00-requisitos.md` §2)

## Cerrado recientemente
- 2026-10-18 · UC-07 (a): spec, puerto `IShiftSheetWriter`, `ExportShiftsHandler` y `GetEmailsAsync` → [historial](historial/2026-10-18-uc07-a-spec-y-handler.md)
- 2026-10-18 · UC-06 (b2b): `ENDPOINTS.md`, `shifts.http`, plantilla `.xlsx`, E2E del import y cierre del UC → [historial](historial/2026-10-18-uc06-b2b-docs-e2e-y-cierre.md)
- 2026-10-18 · UC-06 (b2a): `POST /api/shifts/import` con límite de 5 MB, `shift.import.file-too-large` y lector registrado en DI → [historial](historial/2026-10-18-uc06-b2a-endpoint-de-import.md)
- 2026-10-18 · UC-06 (b1): `ClosedXmlShiftSheetReader` (ClosedXML 0.105.1), `shift.import.file` y `shift.import.columns`, y tests del lector → [historial](historial/2026-10-18-uc06-b1-lector-closedxml.md)
- 2026-10-18 · UC-06 (a): spec, ADR-08 (ClosedXML), `ShiftPlacementChecker`, puerto `IShiftSheetReader` y `ImportShiftsHandler` con informe por fila → [historial](historial/2026-10-18-uc06-a-spec-y-handler.md)
- 2026-10-17 · UC-05 (b2): `ENDPOINTS.md`, `shifts.http` con respuestas reales, E2E (incluida Nager.Date inaccesible) y cierre del UC → [historial](historial/2026-10-17-uc05-b2-docs-e2e-y-cierre.md)
- 2026-10-17 · UC-05 (b1): `NagerDatePublicHolidayCatalog` (v4), catálogo de países a v4, consulta en paralelo y ajuste de ADR-09 → [historial](historial/2026-10-17-uc05-b1-cliente-de-festivos.md)
- 2026-10-17 · UC-05 (a): spec, puerto `IHolidayCatalog`, `ShiftListItemDto` con marca de festivo y handler del listado → [historial](historial/2026-10-17-uc05-a-spec-y-handler.md)
- 2026-10-17 · UC-04 (c): `GET`/`PUT`/`DELETE` de turnos, `ShiftRequest`, docs y E2E; cierre del UC → [historial](historial/2026-10-17-uc04-c-endpoints-y-cierre.md)
- 2026-10-17 · UC-04 (b2): `UpdateShift` y `DeleteShift` con `excludingId` y `Remove` → [historial](historial/2026-10-17-uc04-b2-edicion-y-borrado.md)
- 2026-10-17 · UC-04 (b1): `ListShifts` y `GetShift` con `ListAsync` y `GetByIdAsync`, `shift.not-found` y `shift.date-range` → [historial](historial/2026-10-17-uc04-b1-consultas.md)
- 2026-10-17 · UC-04 (a): spec de gestión de turnos y `Shift.Update` con validación compartida → [historial](historial/2026-10-17-uc04-a-dominio.md)
- 2026-10-17 · UC-03: alta de turno con regla de no solape, persistencia y `POST /api/shifts` → [historial](historial/2026-10-17-uc03-alta-de-turno.md)
- 2026-10-17 · UC-02 (c): `ENDPOINTS.md`, `.http` con respuestas reales, E2E y cierre del UC → [historial](historial/2026-10-17-uc02-c-docs-y-cierre.md)
- 2026-10-17 · UC-02 (b2): acciones `POST`/`PUT`/`DELETE` de colaboradores y `CollaboratorRequest` → [historial](historial/2026-10-17-uc02-b2-endpoints.md)
- 2026-10-17 · UC-02 (b1): casos de uso de escritura, índice único de email y catálogo de países Nager.Date → [historial](historial/2026-10-17-uc02-b1-casos-de-uso.md)
- 2026-10-17 · UC-02 (a): spec, ADR-11, dominio de edición y baja lógica, migración `IsActive` → [historial](historial/2026-10-17-uc02-a-dominio.md)
- 2026-10-17 · UC-01 (b): query, repositorio, migración inicial, semilla y endpoint → [historial](historial/2026-10-17-uc01-listar-colaboradores.md)
- 2026-10-17 · Andamiaje SDD, esqueleto técnico y UC-01 (a) → [historial](historial/2026-10-17-andamiaje-y-esqueleto.md)
