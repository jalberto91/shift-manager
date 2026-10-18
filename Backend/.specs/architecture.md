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
| Fechas/horas | Turno: `DateOnly` + `TimeOnly` sin zona (hora local del colaborador, P12). `DateTimeOffset` solo para auditoría (`CreatedAt`) si hace falta. Contrato JSON con los formatos por defecto de .NET, sin converters propios: fecha `yyyy-MM-dd`; hora de salida `HH:mm:ss` (el front puede mostrar `HH:mm`), de entrada se acepta `HH:mm` o `HH:mm:ss` | ✅ ADR-10 |
| Errores HTTP | RFC 7807 Problem Details + `traceId`. Negocio vía `Result<T>` → `400` con `errors` keyed por código estable **en inglés** (`collaborator.email`); `404` recurso inexistente; **`409` para conflictos de estado** (solape: `errors["shift.overlap"]` con el mensaje y `conflictingShiftId`; email ya usado: `collaborator.email-taken`); `500` sin filtrar internals | ✅ ADR-04, ADR-10 |
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
- **Contexto**: los logs se consultan con `docker compose logs`.
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
- **Ajuste (UC-02)**: el catálogo de países usa timeout de 5 s y degrada a `null` sin reintentos, con la lista de `AvailableCountries` cacheada 24 h; el resilience handler se aplica al cliente de festivos.
- **Ajuste (UC-05)**: sustituye lo dicho sobre el resilience handler en la decisión y en el ajuste de UC-02. El cliente de festivos no usa resilience handler ni reintentos: timeout de 3 s, degradación a `null` y fallos sin cachear, de modo que reintentar lo hace la siguiente petición. Evita una dependencia (`Microsoft.Extensions.Http.Resilience`) y que los reintentos alarguen el listado con la API caída. Los pares país+año de un listado se consultan en paralelo para que el peor caso sea un solo timeout.
- **Ajuste (UC-05, versión)**: los dos clientes usan la v4 de Nager.Date (`api/v4/Holidays/{countryCode}/{year}` y `api/v4/Countries/Available`), no la v3 que cita el enunciado: la v3 figura como deprecada en la documentación oficial (fin de soporte 2027-01-31). Cambios que nos afectan: `global` pasa a `nationalHoliday`; la ruta de festivos lleva el país antes del año. El spec está en `https://nagerholidays.com/openapi/community-v4.json`.
- **Estado**: Aceptado

### ADR-10 · Regla de solape y fechas
- **Contexto**: R3 es el corazón de la evaluación; P2, P3, P12.
- **Alternativas**: intervalos cerrados; turnos que cruzan medianoche; `400` con código de negocio para el solape.
- **Decisión**: intervalos semiabiertos `[inicio, fin)`: solape si `inicioA < finB && inicioB < finA`; solo mismo colaborador y misma fecha; `fin > inicio` (sin cruce de medianoche). Tipos `DateOnly`/`TimeOnly`; en el JSON se usan los formatos por defecto de .NET, sin converters propios: fecha `yyyy-MM-dd`, hora de salida `HH:mm:ss`, de entrada `HH:mm` o `HH:mm:ss`. Al editar se excluye el propio turno. El conflicto devuelve **`409 Conflict`** con Problem Details y `errors["shift.overlap"]` con el mensaje, más `conflictingShiftId` (el id del turno con el que choca) (excepción documentada a "negocio → 400" de ADR-04: es un conflicto con el estado de otro recurso, no un dato mal formado).
- **Consecuencias**: (+) regla simple de explicar, el front distingue el solape de un error de validación. (−) dos familias de código para errores de negocio (400/409).
- **Estado**: Aceptado

### ADR-11 · Baja lógica de colaboradores
- **Contexto**: R1 pide CRUD de colaboradores y los turnos pertenecen a un colaborador; UC-02.
- **Alternativas**: borrado físico con `409` si tiene turnos; borrado físico en cascada.
- **Decisión**: la baja es lógica (`Collaborator.IsActive`, `DELETE` → `204`). El listado devuelve solo activos. El email de un colaborador dado de baja sigue reservado (el índice único lo incluye) y no hay reactivación en v1. Reemplaza `collaborator.hasShifts`: ya no existe un borrado que choque con turnos.
- **Consecuencias**: (+) se conserva el historial de turnos sin bloquear la baja. (−) una columna más; UC-03 (alta de turnos) y UC-06 (import por email) deben decidir qué hacer con colaboradores inactivos.
- **Excepción a ADR-04**: `collaborator.email-taken` es `409`, igual que ADR-10, porque es un conflicto con el estado de otro recurso y no un dato mal formado.
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
