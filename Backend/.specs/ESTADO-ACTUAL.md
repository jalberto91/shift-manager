# Estado actual (handoff vivo)

## Forma de trabajo (regla)
Iteraciones chicas. Al terminar cada una: **parar**, mostrar archivos tocados, sugerir mensaje de commit (Conventional Commits, **en inglés**, con scope) y **esperar "seguí"**. El usuario commitea a mano. Nunca encadenar dos iteraciones sin "seguí". Si una crece más de lo previsto, cortarla en dos antes de seguir.

## ⏭️ Próximo paso
**UC-02 (c)**: abrir `.specs/use-cases/UC-02-gestionar-colaboradores.md` y cerrar el UC: `docs/api/ENDPOINTS.md`, `.http` con respuestas reales, E2E con `docker compose up`, sección "Verificación" y checklist de cierre. Decidir si el mapeo de la carrera del email único (`DbUpdateException` → `409`) entra en (c) o en una iteración propia. Los reintentos de ADR-09 (resilience) quedan para el cliente de festivos.

## Dónde estamos
- Fases 1 y 2, **UC-01 completo** y **UC-02 (a), (b1) y (b2)** commiteados (último código: `e75456c`). Árbol limpio hasta este handoff.
- Build sin warnings, 52 tests verdes. `docker compose up` verificado con las 3 migraciones sobre la semilla (API en `localhost:5080`, BD en `127.0.0.1:5433`); `POST`/`PUT`/`DELETE` ejercitados con curl (sin `.http` ni docs todavía). Quedó en la BD el colaborador `b2.test@example.com` dado de baja (su email sigue ocupado; `docker compose down -v` lo limpia).
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
UC-02 en curso: (a), (b1) y (b2) cerradas y archivadas; (c) pendiente. Ninguna iteración abierta.

## Abierto / pendientes con terceros
- ADR-08 (Excel: ClosedXML vs MiniExcel) pendiente de `license-check`.
- P5: email normalizado a minúsculas + índice único incluyendo inactivos (ADR-11); el índice ya existe (b1).
- Carrera en el índice único de email: hoy da `DbUpdateException` y `500`; mapearla a `409` queda como TBD de UC-02.
- `Location` del `POST` sin `GET /{id}`: se arma con la ruta del recurso; anotarlo en TBDs al cerrar UC-02.
- Regla de código: un tipo por archivo. Pendiente separar `Domain/Common/Error.cs`, `Domain/Common/Result.cs` y `tests/.../Application/Fakes.cs` (iteración aparte, a confirmar).
- Cuando el front entre al compose, ajustar `Cors__AllowedOrigins__0` al puerto que publique.

## Checklist de cierre de UC-02 (Paso 4 de `implement-use-case`)
- [ ] Build sin warnings y tests verdes
- [ ] E2E con `docker compose up`
- [ ] ¿ADR nuevo?
- [ ] Spec con Tasks marcadas y "Para la revisión"
- [ ] `docs/api/ENDPOINTS.md` actualizado
- [ ] `.http` con respuestas reales
- [ ] TBDs anotados

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
