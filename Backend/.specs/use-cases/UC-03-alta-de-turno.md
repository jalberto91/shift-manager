# UC-03 · Alta de turno con regla de no solape
> En curso (iteración a: spec, dominio y tests de la regla; b: caso de uso con fakes; c: persistencia, endpoint, docs y E2E) · rama `main` · 2026-10-17 · cubre: R2 (alta), R3 · decisiones P2, P3, P12, P17 · ADR-10

## Requirements
- Crear un turno con `collaboratorId`, `date`, `startTime` y `endTime`.
- Un colaborador no puede tener dos turnos que se solapen el mismo día (R3). Intervalos semiabiertos `[inicio, fin)` (P2, ADR-10): dos turnos que se tocan en el borde no se solapan.
- El turno no cruza la medianoche: `endTime > startTime` (P3). Sin mínimos ni máximos de duración (P17).
- Solo se crean turnos para colaboradores activos (TBD 2 de UC-02).

Criterios de aceptación:
- [ ] `POST /api/shifts` con datos válidos → `201`, cuerpo `ShiftDto` y cabecera `Location`.
- [ ] Solape con un turno del mismo colaborador y fecha → `409` con `errors["shift.overlap"]` y `conflictingShiftId`.
- [ ] `endTime <= startTime` → `400` `shift.time-range`.
- [ ] Fecha ausente → `400` `shift.date`.
- [ ] Colaborador inexistente o dado de baja → `400` `shift.collaborator-unavailable`.
- [ ] Varios datos mal formados → `400` con todos los errores a la vez.

Casos límite de la regla (cada uno es un test):
- Se tocan en el borde (09:00-13:00 y 13:00-17:00, en cualquier orden) → no solapan.
- Se solapan por un lado (09:00-13:00 y 12:00-16:00, en ambos sentidos), uno contiene al otro y turnos idénticos → solapan.
- Mismo horario en otro día, o en otro colaborador → no solapan.
- `endTime == startTime` y `endTime < startTime` → `shift.time-range`.
- Con varios turnos ese día, el conflicto informado es uno de los que realmente chocan.

## Design
**Endpoint** (`api/shifts`): body `{ "collaboratorId", "date", "startTime", "endTime" }`, formatos de ADR-10.

| Método | Ruta | Éxito | Errores |
|---|---|---|---|
| `POST` | `/api/shifts` | `201` + `ShiftDto` + `Location` | `400`, `409` |

**Entidad `Shift`** (`Domain/Shifts/`), nueva:
- Campos `Id`, `CollaboratorId` (solo el id: el colaborador es otro agregado), `Date` (`DateOnly`), `StartTime` y `EndTime` (`TimeOnly`), todos de solo lectura. La edición llega en UC-04.
- `Create(collaboratorId, date, startTime, endTime)`: `Result<Shift>`, valida `date` y `endTime > startTime` y acumula los errores.
- `OverlapsWith(Shift other)`: misma `CollaboratorId`, misma `Date` y `StartTime < other.EndTime && other.StartTime < EndTime`. Es la única definición de la regla.

**Errores nuevos** (`ShiftErrors`): `shift.time-range` y `shift.date` (validación, `400`), `shift.collaborator-unavailable` (validación, `400`), `shift.overlap` (conflicto, `409`, con `conflictingShiftId` en las extensiones de `Error.Conflict`, que `ApiControllerBase` ya vuelca al Problem Details).

**Regla fuera del dominio** (necesita datos, vive en el handler):
1. El dominio valida la forma (`Create`).
2. El colaborador existe y está activo: `ICollaboratorRepository.GetByIdAsync` (ya existe).
3. Solape: el puerto nuevo `IShiftRepository.ListByCollaboratorAndDateAsync(collaboratorId, date, excludingId, ct)` devuelve los turnos de ese día y el handler aplica `OverlapsWith` en memoria. El SQL solo acota por colaborador y día; la regla no se duplica en una consulta. `excludingId` queda para la edición de UC-04 y el import de UC-06 reutiliza `OverlapsWith` contra los turnos del archivo.

**Capas previstas**
- Domain: `Shift`, `ShiftErrors`.
- Application: `CreateShift` (Command + Handler), `ShiftDto`, puerto `IShiftRepository` (`ListByCollaboratorAndDateAsync`, `Add`, `SaveChangesAsync`).
- Infrastructure: `ShiftConfiguration`, `ShiftRepository`, `DbSet<Shift>`, índice por `(CollaboratorId, Date)`, FK a colaboradores sin cascada y migración `AddShifts`.
- Api: `ShiftsController` con `POST` y `CreateShiftRequest`.

**Decisiones y supuestos**
- La regla vive en `Shift.OverlapsWith` y los handlers la aplican en memoria: un solo lugar que UC-03, UC-04 y UC-06 comparten, y volumen acotado (turnos de un colaborador en un día).
- Colaborador inexistente o dado de baja responde `400` y no `404`: el id viaja en el cuerpo, no en la ruta, y el recurso pedido (`/api/shifts`) existe.
- Fecha ausente: el binding deja `DateOnly` en su valor por defecto; se rechaza como `shift.date` sin tipos nullables ni converters.
- Sin servicio de dominio ni value object `TimeRange`: con un solo cálculo, el método de la entidad basta.
- `Location` apunta a `/api/shifts/{id}` aunque aún no exista `GET /{id}` (llega con UC-04), igual que en UC-02.

## Tasks
- [x] Domain: `Shift`, `ShiftErrors` y `OverlapsWith` (iteración a; `shift.collaborator-unavailable` se agrega en (b), donde se usa)
- [x] Tests: dominio, casos límite de la regla (iteración a)
- [ ] Application: `CreateShift`, `ShiftDto` y puerto `IShiftRepository` (iteración b)
- [ ] Tests: handler con fakes (iteración b)
- [ ] Infrastructure: configuración, repositorio, migración `AddShifts` (iteración c)
- [ ] Api: `ShiftsController` y `CreateShiftRequest` (iteración c)
- [ ] Docs: `docs/api/ENDPOINTS.md` y `.http` con respuestas reales (iteración c)

## Verificación
Pendiente.

## TBDs
1. Carrera entre dos altas solapadas simultáneas: ambas pasan la comprobación del handler y las dos se guardan. La red de seguridad sería una restricción de exclusión de PostgreSQL (`btree_gist`); queda fuera de v1 y va a "qué quedó fuera".

## Para la revisión
- La regla de solape se escribe una sola vez, en `Shift.OverlapsWith`, y se prueba sin base de datos; el repositorio solo trae los turnos del día.
- Intervalos semiabiertos (P2): el borde compartido no es solape, y hay tests de los dos sentidos.
- El solape es `409` con `conflictingShiftId` (ADR-10) para que el front lo distinga de un error de validación.
- Sin turnos que cruzan la medianoche (P3): `endTime > startTime` mantiene la regla en una comparación.
