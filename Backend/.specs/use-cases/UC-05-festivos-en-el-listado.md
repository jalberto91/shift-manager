# UC-05 · Festivos en el listado de turnos
> En curso (a: spec, puerto y handler; b: cliente Nager.Date, endpoint, docs y E2E) · rama `main` · 2026-10-17 · cubre: R6 · decisión P10 · ADR-09

## Requirements
- El listado de turnos señala los que caen en festivo del país del colaborador, con el nombre del festivo.
- Se distinguen festivos globales (todo el país) y regionales (solo algunas regiones). El colaborador solo tiene país, así que los regionales se marcan igual, con `isGlobal = false`.
- Si Nager.Date no responde, el listado sale igual y cada turno lleva `holiday.status = Unknown` (ADR-09).
- Los turnos de colaboradores dados de baja se siguen marcando (ADR-11).

Criterios de aceptación:
- [ ] `GET /api/shifts` → cada turno incluye `holiday: { status, name, isGlobal }`.
- [ ] Turno en festivo global → `status = Holiday`, `name`, `isGlobal = true`.
- [ ] Turno en festivo regional → `status = Holiday`, `name`, `isGlobal = false`.
- [ ] Turno en día normal → `status = None`, `name = null`, `isGlobal = null`.
- [ ] Nager.Date caído o con error → `200` con `status = Unknown` en los turnos afectados; nunca `5xx` por la API externa.
- [ ] Un fallo no se cachea: la siguiente petición vuelve a intentarlo.

Casos límite:
- Colaboradores de países distintos en el mismo listado → cada turno usa el país de su colaborador.
- Turnos de distintos años → se consulta cada país+año una sola vez.
- Listado vacío → no se llama a Nager.Date.

## Design
**Contrato** (TBD 1 de UC-04): `ShiftDto` no cambia (lo usan `POST`, `PUT` y `GET /{id}`). `GET /api/shifts` devuelve `ShiftListItemDto`: los mismos campos que `ShiftDto` más `holiday`. Es un cambio aditivo, el front que ya lee `GET /api/shifts` no se rompe.

```json
{ "id": "…", "collaboratorId": "…", "date": "2026-03-19", "startTime": "09:00:00", "endTime": "13:00:00",
  "holiday": { "status": "Holiday", "name": "Saint Joseph", "isGlobal": false } }
```
`status` es `None`, `Holiday` o `Unknown` (string, por el `JsonStringEnumConverter` ya configurado).

**Puerto** (`Application/Abstractions`): `IHolidayCatalog.GetAsync(countryCode, year, ct)` → `IReadOnlyList<PublicHoliday>?` con `PublicHoliday(Date, Name, IsGlobal)`. `null` significa que la fuente externa no respondió, como en `ICountryCatalog`.

**País de cada turno**: `ICollaboratorRepository.GetCountryCodesAsync(ids)`. Incluye inactivos; `ListAsync` solo devuelve activos y no sirve aquí.

**Handler** (`ListShiftsHandler`): lista los turnos, obtiene los países, pide una vez cada par país+año al puerto y arma la marca por turno. Lista `null` → `Unknown`; fecha presente → `Holiday`; si no → `None`. No hay regla de dominio nueva: es una proyección de lectura, `Domain` no cambia.

**Infraestructura** (iteración b): cliente tipado de `GET api/v3/PublicHolidays/{year}/{countryCode}` (respuesta con `date`, `name`, `global`, `counties`; verificado contra la API real), caché `IMemoryCache` por país+año solo para aciertos, `LogWarning` y `null` al fallar, y resilience handler con reintentos acotados y timeout corto (ADR-09). El paquete `Microsoft.Extensions.Http.Resilience` pasa por `license-check` antes de instalarlo.

**Decisiones y supuestos**
- Marca como objeto `holiday` con `status` en vez de un booleano: `unknown` es un tercer estado real (P10) y un booleano lo escondería.
- Las llamadas a Nager.Date son secuenciales: el número de pares país+año de un listado es pequeño y el resultado queda cacheado. Paralelizar sería complejidad sin dato que lo justifique.
- Un festivo regional se marca aunque la región del colaborador no se conozca: el modelo no guarda región y `isGlobal` deja que el front lo distinga.
- Mientras no exista el cliente real (iteración b), nada registra `IHolidayCatalog` y es probable que el contenedor de DI no deje arrancar la API (sin verificar); no se levanta el compose entre (a) y (b).
- Con Nager.Date caído, las llamadas secuenciales suman un timeout por cada par país+año y los fallos no se cachean. En (b) se decide entre paralelizar con `Task.WhenAll` o acotar con un timeout total corto en el cliente.
- Nager.Date v3 tiene fin de soporte el 2027-01-31 (hay una v4); el cliente queda detrás de `IHolidayCatalog`, así que migrar es cambiar una sola clase.

## Tasks
- [x] Application: `IHolidayCatalog`, `PublicHoliday`, `ShiftListItemDto`, `HolidayInfo`, `HolidayStatus` y `ListShiftsHandler` con la marca (iteración a)
- [x] Infrastructure: `GetCountryCodesAsync` en `CollaboratorRepository` (iteración a)
- [x] Tests: handler con festivo global, regional, día normal, `Unknown`, países distintos, una consulta por país+año, colaborador de baja y listado vacío (iteración a)
- [ ] Infrastructure: cliente de festivos con caché y resilience handler, y registro en `AddInfrastructure` (iteración b)
- [ ] Tests: cliente con `HttpMessageHandler` falso (acierto, caché, fallo sin cachear) (iteración b)
- [ ] Api: sin cambios de código más allá del tipo documentado en `ProducesResponseType` (ya hecho en a)
- [ ] Docs: `docs/api/ENDPOINTS.md` y `shifts.http` con respuestas reales (iteración b)

## Verificación
Iteración a: build sin warnings, 105 tests verdes. E2E pendiente de (b), incluido el caso con Nager.Date inaccesible.

## TBDs
1. Si el modelo gana una región por colaborador, el festivo regional podría filtrarse por `counties` en vez de solo marcarse.

## Para la revisión
- El listado nunca depende de la API externa: un fallo degrada a `Unknown`, no a error (ADR-09).
- `ShiftDto` queda intacto y el listado tiene su propio DTO: el cambio es aditivo y no arrastra la consulta de festivos a `POST`/`PUT`/`GET /{id}`.
- El puerto reutiliza la convención de `ICountryCatalog` (`null` = no respondió), sin tipos de error nuevos.
- Una consulta por país+año por listado, y caché solo de aciertos para que un fallo no quede fijado.
