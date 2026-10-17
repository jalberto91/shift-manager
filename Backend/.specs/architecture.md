# Arquitectura — fuente de verdad

Si hay que desviarse de algo de este documento, **se actualiza el ADR antes** de tocar código.

## §1 Vista global

```
 ┌──────────────┐   HTTP/JSON    ┌───────────────────────────────┐        ┌──────────────┐
 │ Front Angular│ ─────────────► │  ShiftManager.Api             │ ─────► │ PostgreSQL   │
 │ (../FrontEnd)│ ◄───────────── │  Application (CQRS/Mediator)  │  EF    │              │
 └──────────────┘  Problem Det.  │  Domain (reglas puras)        │  Core  └──────────────┘
                                 │  Infrastructure               │
                                 └───────────────┬───────────────┘
                                                 │ HttpClient tipado (timeout, reintentos, caché)
                                                 ▼
                                      Nager.Date (festivos, países)
```

Dependencias: `Domain ← Application ← Infrastructure ← Api` (Api compone todo en `Program.cs`). Sin mensajería.

## §2 Convenciones transversales

| Tema | Convención | Estado |
|---|---|---|
| IDs | `Guid` generado en el dominio (`Guid.NewGuid()`), no autoincrementales | ✅ ADR-02 |
| Dinero | No aplica (si apareciera: `decimal`) | ✅ |
| Fechas/horas | Turno: `DateOnly` + `TimeOnly` sin zona (hora local del colaborador, P12). `DateTimeOffset` solo para auditoría (`CreatedAt`) si hace falta. Contrato JSON, entrada y salida: fecha `yyyy-MM-dd`, hora `HH:mm` (sin segundos), con un converter de `TimeOnly` | ✅ ADR-10 |
| Errores HTTP | RFC 7807 Problem Details + `traceId`. Negocio vía `Result<T>` → `400` con `errors` keyed por código estable **en inglés** (`collaborator.email`); `404` recurso inexistente; **`409` para conflictos de estado** (solape: `errors["shift.overlap"]` con el mensaje y `conflictingShiftId`; borrar colaborador con turnos: `collaborator.hasShifts`); `500` sin filtrar internals | ✅ ADR-04, ADR-10 |
| Versionado de rutas | Sin versionado en v1; prefijo `/api/` | ✅ |
| Paginación | No en v1 (P13); el filtro por fechas acota | ✅ |
| Auditoría | Solo `CreatedAt` si aporta; sin usuario (no hay auth) | ✅ ADR-06 |
| Logging | Serilog estructurado a consola, `traceId` en errores | ✅ ADR-05 |
| Health checks | `GET /health` (incluye BD) | ✅ |
| Acceso a datos | Una `IEntityTypeConfiguration<T>` por entidad + `ApplyConfigurationsFromAssembly`; constantes con nombre, nunca magic strings | ✅ ADR-02 |
| Serialización de enums | Como string (`JsonStringEnumConverter`) | ✅ |
| Emails | Normalizados a minúsculas al guardar + índice único (P5) | ✅ |
| Mapeo | A mano, sin AutoMapper | ✅ ADR-03 |

## §3 ADRs

### ADR-01 · Clean Architecture + .NET 9
- **Contexto**: el enunciado impone .NET 9; hay SDKs 10/11 instalados que `dotnet` podría tomar por defecto.
- **Alternativas**: .NET 10 (LTS más reciente); arquitectura en una sola capa.
- **Decisión**: .NET 9 y EF Core 9, fijado con `global.json`. Capas `Domain → Application → Infrastructure → Api` + `UnitTests`.
- **Consecuencias**: (+) cumple el enunciado, reglas testeables sin infraestructura. (−) más proyectos de los que el tamaño justificaría; se compensa con código mínimo en cada capa.
- **Estado**: Aceptado

### ADR-02 · Motor de BD, EF Core y proveedor
- **Contexto**: BD relacional libre (R7), arranque rápido en Docker.
- **Alternativas**: SQL Server (imagen pesada), SQLite (no representa un entorno real).
- **Decisión**: PostgreSQL + EF Core 9 + Npgsql. Migraciones EF aplicadas al arrancar y datos semilla. IDs `Guid`.
- **Consecuencias**: (+) imagen liviana, `date`/`time` nativos para `DateOnly`/`TimeOnly`. (−) migrar al arrancar no es ideal para producción multi-instancia; aceptable aquí y se documenta.
- **Estado**: Aceptado

### ADR-03 · Mediator y librerías prohibidas por licencia
- **Contexto**: MediatR, AutoMapper, MassTransit y FluentAssertions v8+ pasaron a licencia comercial; EPPlus también.
- **Alternativas**: MediatR; handlers inyectados directamente.
- **Decisión**: `martinothamar/Mediator` (MIT, source-generated) con CQRS; mapeo a mano; assertions con xUnit nativo o Shouldly. Todo NuGet pasa por `license-check`.
- **Consecuencias**: (+) sin riesgo de licencia, sin reflexión. (−) menos documentación que MediatR.
- **Estado**: Aceptado

### ADR-04 · Contrato de errores HTTP
- **Contexto**: el front necesita errores uniformes; el negocio no debe usar excepciones.
- **Alternativas**: excepciones + middleware; envelope propio.
- **Decisión**: Problem Details (RFC 7807) + `traceId`. Los handlers devuelven `Result<T>`; los errores de validación → `400` con `errors` por código estable. Los conflictos de estado → `409` (ver ADR-10). Excepciones inesperadas → `500` genérico, detalle solo en logs.
- **Consecuencias**: (+) contrato único documentado en `docs/api/ERROR-HANDLING.md`. (−) mapeo `Error` → HTTP a mantener.
- **Estado**: Aceptado

### ADR-05 · Observabilidad
- **Contexto**: los evaluadores verán `docker compose logs`.
- **Alternativas**: OpenTelemetry + collector.
- **Decisión**: Serilog estructurado a consola + `traceId` en cada error. OpenTelemetry solo si queda tiempo, sin collector en el compose.
- **Consecuencias**: (+) simple. (−) sin trazas distribuidas.
- **Estado**: Aceptado

### ADR-06 · Sin autenticación
- **Contexto**: el enunciado no la pide.
- **Alternativas**: JWT básico.
- **Decisión**: sin auth; se lista en "qué quedó fuera" del README.
- **Consecuencias**: (+) alcance acotado. (−) la API no es apta para exponer públicamente.
- **Estado**: Aceptado

### ADR-07 · Docker
- **Contexto**: R8, un solo comando.
- **Alternativas**: scripts manuales.
- **Decisión**: `Backend/Dockerfile` multi-stage (sdk → aspnet). `../docker-compose.yml` con `db` (healthcheck `pg_isready`) y `api` (`depends_on: service_healthy`); migraciones y semilla al arrancar; OpenAPI UI habilitada también en el contenedor. El front se suma después.
- **Consecuencias**: (+) `docker compose up` desde `shift-manager/` lo levanta todo. (−) la imagen ignora `CLAUDE.md`, `.claude/`, `.specs/` vía `.dockerignore`.
- **Estado**: Aceptado

### ADR-08 · Librería de Excel
- **Contexto**: import/export `.xlsx`; EPPlus prohibido.
- **Alternativas**: ClosedXML (MIT), MiniExcel (Apache-2.0).
- **Decisión**: pendiente de `license-check` y de una prueba corta (lectura de celdas fecha vs. texto).
- **Consecuencias**: por definir.
- **Estado**: Propuesto

### ADR-09 · Integración con Nager.Date
- **Contexto**: R6 y "manejo de fallos de la API externa".
- **Alternativas**: llamar sin caché; fallar el listado si la API cae.
- **Decisión**: `HttpClient` tipado, timeout corto, reintentos acotados (resilience handler), caché por país+año (`IMemoryCache`), y **degradación**: si falla, el listado responde igual con `holiday = unknown`. Validación de país de un colaborador: si Nager.Date responde, se valida contra `AvailableCountries`; si no responde, se acepta el alta cuando el código tiene formato ISO alfa-2 (dos letras mayúsculas) y se loguea un warning. Un fallo externo no bloquea el alta.
- **Consecuencias**: (+) el listado nunca depende de la API externa. (−) un festivo puede mostrarse "desconocido" hasta el siguiente acierto; los fallos no se cachean.
- **Estado**: Aceptado

### ADR-10 · Regla de solape y fechas
- **Contexto**: R3 es el corazón de la evaluación; P2, P3, P12.
- **Alternativas**: intervalos cerrados; turnos que cruzan medianoche; `400` con código de negocio para el solape.
- **Decisión**: intervalos semiabiertos `[inicio, fin)`: solape si `inicioA < finB && inicioB < finA`; solo mismo colaborador y misma fecha; `fin > inicio` (sin cruce de medianoche). Tipos `DateOnly`/`TimeOnly`; en el JSON la fecha es `yyyy-MM-dd` y la hora `HH:mm` (sin segundos), en entrada y salida, con un converter de `TimeOnly`. Al editar se excluye el propio turno. El conflicto devuelve **`409 Conflict`** con Problem Details y `errors["shift.overlap"]` con el mensaje, más `conflictingShiftId` (el id del turno con el que choca) (excepción documentada a "negocio → 400" de ADR-04: es un conflicto con el estado de otro recurso, no un dato mal formado).
- **Consecuencias**: (+) regla simple de explicar, el front distingue el solape de un error de validación. (−) dos familias de código para errores de negocio (400/409).
- **Estado**: Aceptado

---

### Plantilla de ADR
```
### ADR-NN · Título
- **Contexto**:
- **Alternativas consideradas**:
- **Decisión**:
- **Consecuencias**: (+) … (−) …
- **Estado**: Propuesto | Aceptado | Superseded por ADR-NN
```
Un ADR superseded **no se borra**.
