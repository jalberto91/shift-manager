# UC-05 (a): spec, puerto de festivos y handler del listado (archivado 2026-10-17)

Commit: `c9ed836`. Bloque archivado tal como estaba en `ESTADO-ACTUAL.md`:

## ⏭️ Próximo paso
**UC-05 · Festivos en el listado de turnos (R6)**. Todavía no tiene spec: empezar por la Tarea 0 de la skill `implement-use-case` (spec `.specs/use-cases/UC-05-festivos-en-el-listado.md` desde `_TEMPLATE-UC.md`, Requirements → Design → Tasks, y "Para la revisión"). Leer antes: `00-requisitos.md` (P10, R6, §resiliencia), ADR-09 en `architecture.md` y el TBD 1 de UC-04 (ampliar `ShiftDto` o usar uno propio con la marca de festivo sin romper el contrato de `GET /api/shifts`). Puntos a decidir en el spec: festivos globales y regionales con nombre, `holiday = unknown` si Nager.Date falla, caché por país+año (`IMemoryCache`), y el cliente de festivos con resilience handler (hacer `license-check` del paquete antes de instalarlo). La iteración (a) es el spec y el dominio/puerto; no encadenar con (b) sin "seguí".

## Bloque en curso
UC-04 cerrado y archivado. UC-05 sin empezar (falta su spec). Ninguna iteración abierta.

Resumen de lo hecho en (a) (no estaba en el estado vivo):
- Spec `UC-05-festivos-en-el-listado.md`. `ShiftDto` queda intacto; `GET /api/shifts` devuelve `ShiftListItemDto` (mismos campos más `holiday { status, name, isGlobal }`, con `status` = `None`, `Holiday` o `Unknown`). Cierra el TBD 1 de UC-04.
- Puerto `IHolidayCatalog.GetAsync(countryCode, year, ct)` → `IReadOnlyList<PublicHoliday>?` (`null` = la fuente no respondió, como `ICountryCatalog`) y `PublicHoliday(Date, Name, IsGlobal)`.
- `ICollaboratorRepository.GetCountryCodesAsync(ids)` (incluye inactivos, ADR-11) y su implementación.
- `ListShiftsHandler` pide cada par país+año una sola vez, de forma secuencial, y arma la marca por turno.
- 105 tests verdes y build sin warnings. Sin E2E: no existe el cliente real.
- Revisado con `/revisa`: sin anti-patrones. Se aplicaron los nombres en el bucle y `FakeHolidayCatalog` en su propio archivo.
- Nager.Date: el v3 gratuito responde con `date`, `name`, `global`, `counties` (verificado con curl contra `date.nager.at` y `nagerholidays.com`). El README del repo da el v3 con fin de soporte el 2027-01-31 y una v4 que no pude sondear (las rutas probadas dan `404`).
- Riesgo anotado en el spec: con Nager.Date caído, las llamadas secuenciales suman un timeout por par país+año y los fallos no se cachean.
- Probablemente el contenedor de DI no deje arrancar la API sin un `IHolidayCatalog` registrado (sin verificar); no levantar el compose hasta cerrar (b).
