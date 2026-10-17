# Estado actual (handoff vivo)

## Forma de trabajo (regla)
Iteraciones chicas. Al terminar cada una: **parar**, mostrar archivos tocados, sugerir mensaje de commit (Conventional Commits, **en inglés**, con scope) y **esperar "seguí"**. El usuario commitea a mano. Nunca encadenar dos iteraciones sin "seguí". Si una crece más de lo previsto, cortarla en dos antes de seguir.

## ⏭️ Próximo paso
UC-02 está cerrado a falta del commit de (c). Siguiente: `/handoff` para archivar el bloque y luego elegir entre la iteración opcional de la carrera del email único (`DbUpdateException` → `409`, TBD 3 del spec) o arrancar UC-03 (alta de turno con regla de solape). Los reintentos de ADR-09 (resilience) quedan para el cliente de festivos.

## Dónde estamos
- Fases 1 y 2, **UC-01 completo** y **UC-02 (a), (b1) y (b2)** commiteados (último código: `e75456c`). Árbol limpio hasta este handoff.
- Build sin warnings, 52 tests verdes. `docker compose up` verificado con las 3 migraciones sobre la semilla (API en `localhost:5080`, BD en `127.0.0.1:5433`); `POST`/`PUT`/`DELETE` ejercitados con curl (sin `.http` ni docs todavía). La BD se reseteó para el E2E de (c) y quedó con el colaborador `test.user@example.com` dado de baja (su email sigue ocupado; `docker compose down -v` lo limpia). La API sigue levantada en el compose.
- Existen las migraciones `InitialCreate`, `AddCollaboratorIsActive` y `AddCollaboratorEmailUniqueIndex`, y la semilla de 5 colaboradores.
- `ICountryCatalog` ya tiene implementación (`NagerDateCountryCatalog`, config `NagerDate:BaseUrl`); los handlers de escritura existen pero no hay acciones de controller que los llamen. `dotnet-ef` 9.0.21 es herramienta local (`.config/dotnet-tools.json`): `dotnet dotnet-ef`.
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
UC-02: (a), (b1) y (b2) cerradas y archivadas; (c) hecha (docs, `.http` y E2E), pendiente de commit y de archivar. Ninguna iteración abierta.

## Abierto / pendientes con terceros
- ADR-08 (Excel: ClosedXML vs MiniExcel) pendiente de `license-check`.
- P5: email normalizado a minúsculas + índice único incluyendo inactivos (ADR-11); el índice ya existe (b1).
- Carrera en el índice único de email: hoy da `DbUpdateException` y `500`; mapearla a `409` es el TBD 3 de UC-02 (iteración opcional).
- Regla de código: un tipo por archivo. Pendiente separar `Domain/Common/Error.cs`, `Domain/Common/Result.cs` y `tests/.../Application/Fakes.cs` (iteración aparte, a confirmar).
- Cuando el front entre al compose, ajustar `Cors__AllowedOrigins__0` al puerto que publique.

## Checklist de cierre de UC-02 (Paso 4 de `implement-use-case`)
- [x] Build sin warnings y tests verdes (52)
- [x] E2E con `docker compose up`
- [x] ¿ADR nuevo? No: ADR-11 ya cubre la baja lógica
- [x] Spec con Tasks marcadas y "Para la revisión"
- [x] `docs/api/ENDPOINTS.md` actualizado
- [x] `.http` con respuestas reales
- [x] TBDs anotados

## Checklist de cierre del proyecto
- [ ] Docker completo (`docker compose up` desde `shift-manager/`), con el front
- [ ] Tests revisados (solape, import fila a fila, degradación de Nager.Date)
- [ ] `.xlsx` de ejemplo en el repo
- [ ] `README.md` en la raíz: cómo ejecutarlo, decisiones (ADRs), qué quedó fuera, **sección "Uso de IA"**
- [ ] Revisión contra "qué valoran" (`00-requisitos.md` §2)

## Cerrado recientemente
- 2026-10-17 · UC-02 (b2): acciones `POST`/`PUT`/`DELETE` de colaboradores y `CollaboratorRequest` → [historial](historial/2026-10-17-uc02-b2-endpoints.md)
- 2026-10-17 · UC-02 (b1): casos de uso de escritura, índice único de email y catálogo de países Nager.Date → [historial](historial/2026-10-17-uc02-b1-casos-de-uso.md)
- 2026-10-17 · UC-02 (a): spec, ADR-11, dominio de edición y baja lógica, migración `IsActive` → [historial](historial/2026-10-17-uc02-a-dominio.md)
- 2026-10-17 · UC-01 (b): query, repositorio, migración inicial, semilla y endpoint → [historial](historial/2026-10-17-uc01-listar-colaboradores.md)
- 2026-10-17 · Andamiaje SDD, esqueleto técnico y UC-01 (a) → [historial](historial/2026-10-17-andamiaje-y-esqueleto.md)
