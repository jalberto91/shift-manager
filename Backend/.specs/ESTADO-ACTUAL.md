# Estado actual (handoff vivo)

## Forma de trabajo (regla)
Iteraciones chicas. Al terminar cada una: **parar**, mostrar archivos tocados, sugerir mensaje de commit (Conventional Commits, **en inglés**, con scope) y **esperar "seguí"**. El usuario commitea a mano. Nunca encadenar dos iteraciones sin "seguí". Si una crece más de lo previsto, cortarla en dos antes de seguir.

## ⏭️ Próximo paso
**UC-04 (a)**: implementar según `.specs/use-cases/UC-04-gestionar-turnos.md` (ya redactado, pendiente de tu visto bueno): `Shift.Update`, `ShiftErrors.NotFound` y `DateRange`, y sus tests en el dominio. Después, (b1) consultas, (b2) edición y borrado, (c) endpoints, docs y E2E. Los reintentos de ADR-09 (resilience) quedan para el cliente de festivos.

## Dónde estamos
- Fases 1 y 2, **UC-01, UC-02 y UC-03 completos** y commiteados (último código: `10471c9`). Árbol limpio hasta este handoff.
- Build sin warnings, 75 tests verdes. `docker compose up` verificado con las 4 migraciones sobre la semilla (API en `localhost:5080`, BD en `127.0.0.1:5433`), con los endpoints de colaboradores y `POST /api/shifts` ejercitados. Quedan en la BD de pruebas turnos del 2026-03-10 de Ana y Bruno, y colaboradores dados de baja (`test.user@…`, `temp.baja@…`); `docker compose down -v` los limpia. La API sigue levantada en el compose.
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
UC-03 cerrado y archivado. UC-04: spec redactado (sin código); (a), (b1), (b2) y (c) pendientes. Ninguna iteración abierta.

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
- 2026-10-17 · UC-03: alta de turno con regla de no solape, persistencia y `POST /api/shifts` → [historial](historial/2026-10-17-uc03-alta-de-turno.md)
- 2026-10-17 · UC-02 (c): `ENDPOINTS.md`, `.http` con respuestas reales, E2E y cierre del UC → [historial](historial/2026-10-17-uc02-c-docs-y-cierre.md)
- 2026-10-17 · UC-02 (b2): acciones `POST`/`PUT`/`DELETE` de colaboradores y `CollaboratorRequest` → [historial](historial/2026-10-17-uc02-b2-endpoints.md)
- 2026-10-17 · UC-02 (b1): casos de uso de escritura, índice único de email y catálogo de países Nager.Date → [historial](historial/2026-10-17-uc02-b1-casos-de-uso.md)
- 2026-10-17 · UC-02 (a): spec, ADR-11, dominio de edición y baja lógica, migración `IsActive` → [historial](historial/2026-10-17-uc02-a-dominio.md)
- 2026-10-17 · UC-01 (b): query, repositorio, migración inicial, semilla y endpoint → [historial](historial/2026-10-17-uc01-listar-colaboradores.md)
- 2026-10-17 · Andamiaje SDD, esqueleto técnico y UC-01 (a) → [historial](historial/2026-10-17-andamiaje-y-esqueleto.md)
