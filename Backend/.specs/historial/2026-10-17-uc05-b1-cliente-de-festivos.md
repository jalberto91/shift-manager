# UC-05 (b1): cliente de festivos Nager.Date v4 (archivado 2026-10-17)

Commit: `5757055`. Bloque archivado tal como estaba en `ESTADO-ACTUAL.md`:

## ⏭️ Próximo paso
**UC-05 (b) · cliente de festivos, DI, docs y E2E**. Spec: `.specs/use-cases/UC-05-festivos-en-el-listado.md` (Design → "Infraestructura" y Tasks pendientes). Hacer, en orden: (1) `license-check` de `Microsoft.Extensions.Http.Resilience` 9.x antes de instalarlo; (2) cliente tipado de `GET api/v3/PublicHolidays/{year}/{countryCode}` que implemente `IHolidayCatalog` (caché `IMemoryCache` por país+año solo de aciertos, `LogWarning` y `null` al fallar, resilience handler con reintentos acotados y timeout corto) y su registro en `Infrastructure/DependencyInjection.cs`; (3) decidir y aplicar cómo acotar la latencia con Nager.Date caído: `Task.WhenAll` en el handler o timeout total corto en el cliente (si es `WhenAll`, `FakeHolidayCatalog.Calls` debe ser seguro entre hilos); (4) tests del cliente con `HttpMessageHandler` falso (patrón de `NagerDateCountryCatalogTests`); (5) `docs/api/ENDPOINTS.md` y `shifts.http` con respuestas reales; (6) E2E con la skill `verify`, incluido Nager.Date inaccesible; (7) cerrar el spec (Tasks, Verificación, "Para la revisión"). Si crece, cortar en dos antes de seguir. Nota: v3 de Nager.Date tiene fin de soporte el 2027-01-31 y el cliente queda detrás del puerto.

## Bloque en curso
UC-05 en curso: (a) cerrada y archivada; (b) pendiente. Ninguna iteración abierta. Hasta cerrar (b) no hay `IHolidayCatalog` registrado: probablemente la API no arranca (sin verificar), no levantar el compose.

Resumen de lo hecho en (b1) (no estaba en el estado vivo):
- (b) se cortó en dos: b1 = cliente, DI y tests; b2 = docs, E2E y cierre del spec.
- `NagerDatePublicHolidayCatalog` implementa `IHolidayCatalog`: caché de 24 h por país+año solo de aciertos, timeout de 3 s, `LogWarning` y `null` al fallar. Registrado en `AddInfrastructure` (la `Uri` base se comparte con el catálogo de países).
- Sin `Microsoft.Extensions.Http.Resilience` ni reintentos: reintentar lo hace la siguiente petición porque el fallo no se cachea. Decisión registrada en ADR-09 (ajuste UC-05), que además aclara que sustituye lo dicho sobre el resilience handler.
- Ambos clientes pasaron a la v4 de Nager.Date (la v3 está deprecada, fin de soporte 2027-01-31; el enunciado cita la v3, el choque queda en ADR-09). Rutas: `api/v4/Holidays/{countryCode}/{year}` (país primero) y `api/v4/Countries/Available`; `global` pasó a `nationalHoliday`. Spec OpenAPI en `https://nagerholidays.com/openapi/community-v4.json` (la UI de Scalar no sirve para leerlo). Verificado con curl; ambos hosts (`date.nager.at` y `nagerholidays.com`) responden igual y sin clave.
- `ListShiftsHandler` consulta los pares país+año en paralelo (`Task.WhenAll`) para que con Nager.Date caído el peor caso sea un solo timeout; `FakeHolidayCatalog.Calls` es un `ConcurrentBag`.
- Revisado con `/revisa`: la lista real de España trae fechas repetidas (regional y nacional el mismo día), así que `HolidayOn` prioriza el festivo nacional (`OrderByDescending(IsGlobal)`) y tiene su test. `StubHandler` pasó a un archivo compartido de tests.
- 112 tests verdes y build sin warnings. Sin E2E todavía.
