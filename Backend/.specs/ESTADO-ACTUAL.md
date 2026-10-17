# Estado actual (handoff vivo)

## Forma de trabajo (regla)
Iteraciones chicas. Al terminar cada una: **parar**, mostrar archivos tocados, sugerir mensaje de commit (Conventional Commits, **en inglés**, con scope) y **esperar "seguí"**. El usuario commitea a mano. Nunca encadenar dos iteraciones sin "seguí". Si una crece más de lo previsto, cortarla en dos antes de seguir.

## ⏭️ Próximo paso
**UC-02** (alta, edición y baja de colaboradores): crear su spec desde `_TEMPLATE-UC.md`. Incluye unicidad de email (índice único, P5) y validación del país contra Nager.Date (ADR-09).

## Dónde estamos
- Fase 1 (andamiaje SDD) y Fase 2 (esqueleto técnico) cerradas y commiteadas; UC-01 (a) commiteado (`9fe4e9e`). Árbol limpio.
- Build sin warnings, 27 tests verdes, `docker compose up` verificado (API en `localhost:5080`, BD en `127.0.0.1:5433`).
- UC-01 (b) implementado y verificado, pendiente de commit: migración `InitialCreate`, semilla de 5 colaboradores y `GET /api/collaborators` (200). `dotnet-ef` 9.0.21 es herramienta local (`.config/dotnet-tools.json`): `dotnet dotnet-ef`.

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
UC-01 cerrado, a la espera de commit.

## Abierto / pendientes con terceros
- ADR-08 (Excel: ClosedXML vs MiniExcel) pendiente de `license-check`.
- P5: email normalizado a minúsculas + índice único; se detalla en UC-02.
- Cuando el front entre al compose, ajustar `Cors__AllowedOrigins__0` al puerto que publique.

## Checklist de cierre de UC-01 (Paso 4 de `implement-use-case`)
- [x] Build sin warnings y tests verdes (parte a)
- [x] E2E con `docker compose up`
- [x] ¿ADR nuevo? (ninguno)
- [x] Spec con Tasks marcadas
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
- 2026-10-17 · Andamiaje SDD, esqueleto técnico y UC-01 (a) → [historial](historial/2026-10-17-andamiaje-y-esqueleto.md)
