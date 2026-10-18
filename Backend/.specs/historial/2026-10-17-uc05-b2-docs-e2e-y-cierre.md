# UC-05 (b2): docs, E2E y cierre del UC (archivado 2026-10-17)

Commit: `1a23c9e`. Bloque archivado tal como estaba en `ESTADO-ACTUAL.md`:

## ⏭️ Próximo paso
**UC-05 (b2) · docs, E2E y cierre del UC**. Spec: `.specs/use-cases/UC-05-festivos-en-el-listado.md` (Requirements → criterios de aceptación; Tasks pendientes; Verificación). Hacer, en orden: (1) `docs/api/ENDPOINTS.md`: `GET /api/shifts` con `holiday { status, name, isGlobal }` y los tres estados (`None`, `Holiday`, `Unknown`); (2) `docs/api/shifts.http` con respuestas **reales**; (3) E2E con la skill `verify` (`docker compose up --build`; el compose debe reconstruirse porque la imagen actual es previa a UC-05): turno en festivo global, en festivo regional, en día normal, países distintos y, con Nager.Date inaccesible (por ejemplo apuntando `NagerDate__BaseUrl` a un host inexistente), `200` con `Unknown` y respuesta en pocos segundos (timeout de 3 s); (4) cerrar el spec (Tasks, Verificación, TBDs, "Para la revisión") y marcar UC-05 completo. España tiene festivos regionales y nacionales con la misma fecha, útil para probar la prioridad del nacional. No agregar dependencias.

## Bloque en curso
UC-05 en curso: (a) y (b1) cerradas y archivadas; (b2) pendiente. Ninguna iteración abierta. El compose en ejecución tiene una imagen previa a UC-05: reconstruir antes del E2E.

Resumen de lo hecho en (b2) (no estaba en el estado vivo):
- `docs/api/ENDPOINTS.md`: UC-05 pasa a "hecho" y hay una sección nueva con los tres estados de `holiday`; `docs/api/shifts.http` suma cinco requests con respuestas reales y el ejemplo del listado incluye `holiday`.
- E2E con `docker compose up --build`, PostgreSQL y la API real de Nager.Date v4: festivo nacional (`ES`, 2026-01-01 y 2027-01-01), regional (2026-02-28, `isGlobal = false`), día normal, países distintos el mismo día (AR y MX el 2026-04-02) y dos años en el mismo listado.
- Con Nager.Date inaccesible (`NagerDate__BaseUrl` hacia `http://10.255.255.1/`): `200` en 3,5 s con 5 pares país+año, todos `Unknown`, un warning por par. Al volver el servicio: 0,8 s la primera llamada (fallo no cacheado) y 0,04 s la segunda (caché).
- La API arranca con el cliente registrado: se resolvió la duda del contenedor de DI.
- Corrección a lo anotado en este bloque: las fechas repetidas de España 2026 que se miraron (por ejemplo 2026-04-23) son dos festivos regionales, no uno nacional y otro regional. La prioridad del nacional solo está cubierta por el test unitario.
- Spec de UC-05 cerrado (Tasks, Verificación, "Para la revisión"). 112 tests verdes y build sin warnings.
- La BD de pruebas quedó con 5 turnos de Ana (2026-01-01, 2026-02-10, 2026-02-28, 2026-04-23 y 2027-01-01); `docker compose down -v` los limpia. La API quedó levantada con la configuración normal.
