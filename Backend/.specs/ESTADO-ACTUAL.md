# Estado actual (handoff vivo)

## Forma de trabajo (regla)
Iteraciones chicas. Al terminar cada una: **parar**, mostrar archivos tocados, sugerir mensaje de commit (Conventional Commits, **en inglés**, con scope) y **esperar "seguí"**. El usuario commitea a mano. Nunca encadenar dos iteraciones sin "seguí". Si una crece más de lo previsto, cortarla en dos antes de seguir.

## ⏭️ Próximo paso
**UC-05 (b) · cliente de festivos, DI, docs y E2E**. Spec: `.specs/use-cases/UC-05-festivos-en-el-listado.md` (Design → "Infraestructura" y Tasks pendientes). Hacer, en orden: (1) `license-check` de `Microsoft.Extensions.Http.Resilience` 9.x antes de instalarlo; (2) cliente tipado de `GET api/v3/PublicHolidays/{year}/{countryCode}` que implemente `IHolidayCatalog` (caché `IMemoryCache` por país+año solo de aciertos, `LogWarning` y `null` al fallar, resilience handler con reintentos acotados y timeout corto) y su registro en `Infrastructure/DependencyInjection.cs`; (3) decidir y aplicar cómo acotar la latencia con Nager.Date caído: `Task.WhenAll` en el handler o timeout total corto en el cliente (si es `WhenAll`, `FakeHolidayCatalog.Calls` debe ser seguro entre hilos); (4) tests del cliente con `HttpMessageHandler` falso (patrón de `NagerDateCountryCatalogTests`); (5) `docs/api/ENDPOINTS.md` y `shifts.http` con respuestas reales; (6) E2E con la skill `verify`, incluido Nager.Date inaccesible; (7) cerrar el spec (Tasks, Verificación, "Para la revisión"). Si crece, cortar en dos antes de seguir. Nota: v3 de Nager.Date tiene fin de soporte el 2027-01-31 y el cliente queda detrás del puerto.

## Dónde estamos
- Fases 1 y 2, **UC-01 a UC-04 completos** y **UC-05 (a)** hecha (último código: `c9ed836`). Árbol limpio hasta este handoff.
- Build sin warnings, 105 tests verdes (el E2E de abajo es previo a UC-05). `docker compose up --build` verificado con las 4 migraciones sobre la semilla (API en `localhost:5080`, BD en `127.0.0.1:5433`), con los endpoints de colaboradores y de turnos (`GET`, `POST`, `PUT`, `DELETE`) ejercitados. Quedan en la BD de pruebas turnos de marzo (Ana y Bruno) y de abril (Carla y Diego) de 2026, y colaboradores dados de baja (`test.user@…`, `temp.baja@…`); `docker compose down -v` los limpia. La API sigue levantada en el compose con la imagen actual.
- Existen las migraciones `InitialCreate`, `AddCollaboratorIsActive`, `AddCollaboratorEmailUniqueIndex` y `AddShifts`, y la semilla de 5 colaboradores.
- `ICountryCatalog` tiene implementación (`NagerDateCountryCatalog`, config `NagerDate:BaseUrl`). `dotnet-ef` 9.0.21 es herramienta local (`.config/dotnet-tools.json`): `dotnet dotnet-ef`.
- Migrar con `dotnet dotnet-ef migrations add <Name> --project src/ShiftManager.Infrastructure --startup-project src/ShiftManager.Api --output-dir Persistence/Migrations`. El `HostAbortedException` que imprime es ruido normal de las herramientas.

## Decisiones vigentes (las que no son obvias leyendo el código)
- Rutas, campos, códigos y mensajes de error de la API, en inglés (`/api/collaborators`, `/api/shifts`, `shift.overlap`).
- Solape → `409` con `errors["shift.overlap"]` y `conflictingShiftId` (ADR-10). Otros errores de negocio → `400`.
- Fechas y horas del turno: `DateOnly` + `TimeOnly`, formatos JSON por defecto de .NET (hora de salida `HH:mm:ss`, de entrada `HH:mm` o `HH:mm:ss`), sin converters propios.
- `Result` admite varios errores; el primero fija el status. `ApiControllerBase` solo mapea errores; cada controller inyecta lo que usa.
- Mediator con vida útil `Scoped`. Los handlers dependen de puertos (`Application/Abstractions`), no del `DbContext`.
- Semilla con seeder vía `Create`, no `HasData`; corre en todos los entornos (aceptado para la prueba).
- Si Nager.Date no responde al validar el país de un colaborador, se acepta el alta con formato ISO alfa-2 y un warning (ADR-09).
- pgAdmin de escritorio en Windows: host `127.0.0.1` y `GSS encryption mode = disable` (ver skill `verify`).

## Bloque en curso
UC-05 en curso: (a) cerrada y archivada; (b) pendiente. Ninguna iteración abierta. Hasta cerrar (b) no hay `IHolidayCatalog` registrado: probablemente la API no arranca (sin verificar), no levantar el compose.

## Abierto / pendientes con terceros
- ADR-08 (Excel: ClosedXML vs MiniExcel) pendiente de `license-check`.
- P5: email normalizado a minúsculas + índice único incluyendo inactivos (ADR-11); el índice ya existe (b1).
- Carreras de concurrencia **aceptadas para el demo, no se harán**: email único simultáneo (`DbUpdateException` y `500`, TBD 3 de UC-02) y altas de turnos solapados simultáneas (TBD 1 de UC-03). Van a "qué quedó fuera" del README.
- Regla de código: un tipo por archivo. Pendiente separar `Domain/Common/Error.cs`, `Domain/Common/Result.cs` y `tests/.../Application/Fakes.cs` (iteración aparte, a confirmar).
- Cuando el front entre al compose, ajustar `Cors__AllowedOrigins__0` al puerto que publique.

## Checklist de cierre del proyecto
- [ ] Docker completo (`docker compose up` desde `shift-manager/`), con el front
- [ ] Tests revisados (solape, import fila a fila, degradación de Nager.Date)
- [ ] `.xlsx` de ejemplo en el repo
- [ ] `README.md` en la raíz: cómo ejecutarlo, decisiones (ADRs), qué quedó fuera, **sección "Uso de IA"**
- [ ] Revisión contra "qué valoran" (`00-requisitos.md` §2)

## Cerrado recientemente
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
