# UC-02 · Alta, edición y baja de colaboradores
> En curso (iteración a: spec y dominio; b: casos de uso, persistencia y endpoints; c: Nager.Date, docs y E2E) · rama `main` · 2026-10-17 · cubre: R1 (escritura) · decisiones P4, P5, P11 · ADR-09, ADR-11

## Requirements
- Crear, editar y dar de baja colaboradores con `name`, `email` y `countryCode`.
- El email es único sin distinguir mayúsculas (P5), también al editar (el propio colaborador no cuenta como duplicado).
- El país es un código ISO alfa-2 válido; si Nager.Date responde se valida contra `AvailableCountries` (P4, ADR-09).
- La baja es lógica (ADR-11): el colaborador deja de aparecer en el listado.

Criterios de aceptación:
- [ ] `POST /api/collaborators` con datos válidos → `201`, cuerpo `CollaboratorDto` y cabecera `Location`.
- [ ] `PUT /api/collaborators/{id}` → `200` con el colaborador actualizado.
- [ ] `DELETE /api/collaborators/{id}` → `204`; el colaborador desaparece de `GET /api/collaborators`.
- [ ] Email repetido (aunque cambie la capitalización) → `409` con `errors["collaborator.email-taken"]`.
- [ ] Nombre, email o país mal formados → `400` con todos los errores a la vez.
- [ ] País con formato válido pero que Nager.Date no lista → `400` `collaborator.country-unknown`.
- [ ] Nager.Date caído → el alta/edición se acepta si el formato es alfa-2 y se loguea un warning.
- [ ] Id inexistente o dado de baja en `PUT`/`DELETE` → `404` `collaborator.not-found`.

Casos límite:
- `PUT` conservando el propio email → `200` (no choca consigo mismo).
- `DELETE` repetido sobre un colaborador ya dado de baja → `404` (para la API ya no existe).
- Carrera entre dos altas con el mismo email → gana una; la otra recibe `409` por el índice único.

## Design
**Endpoints** (`api/collaborators`): body de `POST` y `PUT` `{ "name", "email", "countryCode" }`.

| Método | Ruta | Éxito | Errores |
|---|---|---|---|
| `POST` | `/api/collaborators` | `201` + `CollaboratorDto` + `Location` | `400`, `409` |
| `PUT` | `/api/collaborators/{id}` | `200` + `CollaboratorDto` | `400`, `404`, `409` |
| `DELETE` | `/api/collaborators/{id}` | `204` | `404` |

**Entidad `Collaborator`** (ya creada en UC-01), añade:
- `IsActive` (`true` al crear, `private set`).
- `Update(name, email, countryCode)`: `Result`; aplica la misma normalización e invariantes que `Create` (una sola función privada `Normalize`) y no modifica nada si falla.
- `Deactivate()`: idempotente.

**Errores nuevos** (`CollaboratorErrors`): `collaborator.email-taken` (conflicto, `409`), `collaborator.country-unknown` (validación, `400`), `collaborator.not-found` (`404`).

**Reglas fuera del dominio** (necesitan consultar datos, viven en los handlers):
1. El dominio valida la forma (`Create`/`Update`).
2. País conocido: puerto `ICountryCatalog` (`true`/`false`/`null` si Nager.Date no respondió).
3. Unicidad del email: `ICollaboratorRepository.EmailExistsAsync(email, excludingId)`.
4. El índice único sobre `Email` es la red de seguridad ante la carrera.

**Capas previstas**
- Application: `CreateCollaborator`, `UpdateCollaborator`, `DeactivateCollaborator` (Command + Handler cada uno, reutilizan `CollaboratorDto`); puertos `ICountryCatalog` y ampliación de `ICollaboratorRepository` (`GetByIdAsync`, `EmailExistsAsync`, `Add`, `SaveChangesAsync`); `ListAsync` filtra `IsActive`.
- Infrastructure: columna `IsActive` e índice único en `CollaboratorConfiguration`, migración, `NagerDateCountryCatalog` (iteración c).
- Api: tres acciones en `CollaboratorsController`.

**Decisiones y supuestos**
- Baja lógica en lugar de borrado físico (ADR-11); sustituye al `collaborator.hasShifts` de los documentos iniciales.
- El email de un colaborador dado de baja sigue ocupado: simplifica el índice (sin filtro parcial) y evita ambigüedad al importar por email.
- Corregido: el supuesto inicial de borrado físico con `409` si había turnos queda descartado.

## Tasks
- [x] Domain: `IsActive`, `Update`, `Deactivate`, errores nuevos (iteración a)
- [ ] Application: casos de uso y puertos (iteración b)
- [x] Infrastructure: columna `IsActive` con `HasDefaultValue(true)` y migración `AddCollaboratorIsActive` (iteración a: sin ella EF 9 aborta el arranque por cambios de modelo pendientes, y el valor por defecto evita que los colaboradores existentes queden inactivos)
- [ ] Infrastructure: índice único de email, repositorio (iteración b); Nager.Date (iteración c)
- [ ] Api: acciones de `CollaboratorsController` (iteración b)
- [x] Tests: `Update`, `Deactivate`, `IsActive` inicial (dominio, iteración a)
- [ ] Tests: handlers (iteración b) y degradación de Nager.Date (iteración c)
- [ ] Docs: `docs/api/ENDPOINTS.md` y `.http` con respuestas reales (iteración c)

## Verificación
Pendiente.

## TBDs
1. Reactivar un colaborador dado de baja o reutilizar su email: fuera de v1. UC-06 (import por email) debe decidir qué hace si el email pertenece a un inactivo.
2. UC-03 debe rechazar turnos nuevos para colaboradores inactivos.

## Para la revisión
- La baja es lógica para no perder turnos históricos ni bloquear la baja; está registrada como ADR-11.
- `Create` y `Update` comparten una sola normalización/validación: no hay dos fuentes de verdad para las invariantes.
- La unicidad vive en el handler (consulta) y en el índice único (carrera): la regla se explica fácil y la BD garantiza la última palabra.
- Un fallo de Nager.Date no bloquea el alta: la validación de formato del dominio es el mínimo y el fallo se loguea (ADR-09).
