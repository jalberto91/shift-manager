# UC-04 · Listar, leer, editar y borrar turnos
> En curso (iteración a: spec y dominio; b1: consultas; b2: edición y borrado; c: endpoints, docs y E2E) · rama `main` · 2026-10-17 · cubre: R2 (lectura, edición, borrado), R3 · decisiones P2, P3, P13, P17 · ADR-10

## Requirements
- Listar turnos con filtro opcional por rango de fechas, leer uno por id, editarlo y borrarlo.
- Editar mantiene la regla de no solape (R3) y excluye el propio turno de la comparación: dejar el turno como está, o ajustarlo dentro de su hueco, no choca consigo mismo.
- Sin paginación (P13): el filtro por fechas acota el volumen.
- Los turnos de colaboradores dados de baja se siguen listando y leyendo (historial, ADR-11).

Criterios de aceptación:
- [ ] `GET /api/shifts` → `200` con todos los turnos, ordenados por fecha y hora de inicio.
- [ ] `GET /api/shifts?from=2026-03-01&to=2026-03-31` → solo los turnos con `from <= date <= to` (ambos extremos incluidos); `from` o `to` pueden ir solos.
- [ ] `from` posterior a `to` → `400` `shift.date-range`.
- [ ] `GET /api/shifts/{id}` → `200` con el turno; inexistente → `404` `shift.not-found`.
- [ ] `PUT /api/shifts/{id}` → `200` con el turno actualizado.
- [ ] `PUT` que solapa con otro turno del colaborador → `409` `shift.overlap` con `conflictingShiftId`.
- [ ] `PUT` con datos mal formados → `400` con todos los errores a la vez (`shift.date`, `shift.time-range`).
- [ ] `PUT` hacia un colaborador inexistente o dado de baja → `400` `shift.collaborator-unavailable`.
- [ ] `PUT` sobre un id inexistente → `404` `shift.not-found`.
- [ ] `DELETE /api/shifts/{id}` → `204`; inexistente → `404` `shift.not-found`.

Casos límite:
- `PUT` con los mismos valores que ya tiene el turno → `200` (no choca consigo mismo).
- `PUT` que amplía el turno dentro de su propio hueco → `200`.
- `PUT` que mueve el turno a un día donde ya hay otro igual → `409`.
- `PUT` que lo pasa a otro colaborador con un turno que choca → `409`; con el borde compartido → `200`.
- Rango con un solo día (`from == to`) → los turnos de ese día.

## Design
**Endpoints** (`api/shifts`): `PUT` recibe el mismo cuerpo que el alta, `{ "collaboratorId", "date", "startTime", "endTime" }`.

| Método | Ruta | Éxito | Errores |
|---|---|---|---|
| `GET` | `/api/shifts?from&to` | `200` + lista de `ShiftDto` | `400` |
| `GET` | `/api/shifts/{id}` | `200` + `ShiftDto` | `404` |
| `PUT` | `/api/shifts/{id}` | `200` + `ShiftDto` | `400`, `404`, `409` |
| `DELETE` | `/api/shifts/{id}` | `204` | `404` |

**Entidad `Shift`** (ya creada en UC-03), añade:
- Los campos pasan a `private set` y aparece `Update(collaboratorId, date, startTime, endTime)`: `Result`; aplica las mismas validaciones que `Create` (una sola función privada `Validate`, como en `Collaborator`) y no modifica nada si falla.

**Errores nuevos** (`ShiftErrors`): `shift.not-found` (`404`) y `shift.date-range` (validación, `400`).

**Reglas fuera del dominio** (necesitan datos, viven en los handlers):
1. El dominio valida la forma (`Update`).
2. El colaborador destino existe y está activo: `ICollaboratorRepository.GetByIdAsync`, igual que en el alta.
3. Solape: `IShiftRepository.ListByCollaboratorAndDateAsync` gana el parámetro `excludingId` (el propio turno), y el handler aplica `Shift.OverlapsWith` como en el alta.

**Capas previstas**
- Domain: `Shift.Update`, errores nuevos.
- Application: `ListShifts` (query con `From`/`To`), `GetShift`, `UpdateShift` y `DeleteShift`, todos reutilizando `ShiftDto`; el puerto `IShiftRepository` suma `ListAsync(from, to)`, `GetByIdAsync` y `Remove`, y `excludingId` en la consulta existente.
- Infrastructure: `ShiftRepository` implementa los métodos nuevos. Sin migración: el esquema no cambia.
- Api: cuatro acciones en `ShiftsController`; `CreateShiftRequest` pasa a llamarse `ShiftRequest` porque sirve a `POST` y `PUT`, como `CollaboratorRequest`.

**Decisiones y supuestos**
- El filtro es solo por rango de fechas, que es lo que piden R2 y UC-07. Filtrar por colaborador no está pedido y no se agrega.
- Ambos extremos del rango se incluyen. Fecha ausente en un extremo significa sin límite por ese lado.
- El listado devuelve `collaboratorId` y no el nombre: el front ya tiene `GET /api/collaborators` y evitamos un join. UC-05 ampliará el DTO con la marca de festivo.
- `PUT` permite cambiar de colaborador: reutiliza el mismo cuerpo y la misma validación que el alta, sin tratar ese campo como caso especial.
- Editar exige que el colaborador destino esté activo, como el alta. Un turno de un colaborador dado de baja se puede leer y borrar, pero no editar.
- `DELETE` es físico: un turno no tiene historia que preservar. Repetirlo da `404`, a diferencia de la baja de colaboradores (ADR-11), que es una baja lógica idempotente.
- Las comprobaciones de colaborador activo y solape se repiten en `CreateShift` y `UpdateShift` (unas ocho líneas). Si UC-06 las necesita una tercera vez, se extraen entonces.
- Un `from` o `to` con formato inválido lo rechaza el binder con `400`, igual que las horas mal formadas (TBD 2 de UC-03).

## Tasks
- [x] Domain: `Shift.Update` con la validación compartida con `Create` (iteración a; `ShiftErrors.NotFound` y `DateRange` se agregan en (b1), donde se usan)
- [x] Tests: `Update` en el dominio (iteración a)
- [x] Application + Infrastructure: `ListShifts` y `GetShift` con `ListAsync` y `GetByIdAsync` en el puerto y el repositorio, más `shift.not-found` y `shift.date-range` (iteración b1)
- [x] Tests: handlers de consulta con fakes (iteración b1)
- [x] Application + Infrastructure: `UpdateShift` y `DeleteShift`, `excludingId` y `Remove` (iteración b2)
- [x] Tests: handlers de edición y borrado con fakes (iteración b2)
- [ ] Api: acciones de `ShiftsController` y `ShiftRequest` (iteración c)
- [ ] Docs: `docs/api/ENDPOINTS.md` y `shifts.http` con respuestas reales (iteración c)

## Verificación
Pendiente.

## TBDs
1. UC-05 debe ampliar `ShiftDto` (o usar uno propio) con la marca de festivo sin romper el contrato de `GET /api/shifts`.

## Para la revisión
- Editar reutiliza la regla del alta: la misma `OverlapsWith`, la misma validación de forma y la misma comprobación del colaborador, con `excludingId` como única diferencia.
- Un solo cuerpo para `POST` y `PUT`, igual que en colaboradores: menos tipos y un contrato fácil de explicar.
- El filtro por rango incluye ambos extremos y acepta uno solo; sin paginación porque el rango acota (P13).
- Los turnos de colaboradores dados de baja se conservan y se pueden leer, que era la razón de la baja lógica (ADR-11).
