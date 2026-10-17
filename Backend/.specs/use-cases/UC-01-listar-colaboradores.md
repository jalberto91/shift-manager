# UC-01 · Listar colaboradores
> En curso (iteración a: dominio y spec; iteración b: endpoint e infraestructura) · rama `master` · 2026-10-17 · cubre: R1 (lectura), R7, R9 · decisiones P4, P5, P11, P13

## Requirements
- Devolver todos los colaboradores con `id`, `name`, `email` y `countryCode`.
- Orden estable: por `name` y, a igualdad, por `email`.
- Sin filtros ni paginación (P13).
- La BD arranca con datos semilla de varios países para que la app sea usable (P11).
- Es el UC que prueba la plomería de punta a punta: BD, migración, OpenAPI y Docker (R7, R8, R9).

Criterios de aceptación:
- [ ] `GET /api/collaborators` responde `200` con una lista (vacía si no hay datos).
- [ ] Tras `docker compose up` la lista trae los colaboradores semilla, de al menos tres países.
- [ ] El endpoint aparece documentado en la UI de OpenAPI.
- [ ] El orden es determinista.

Casos límite:
- Lista vacía → `200` con `[]`, nunca `404`.
- Dos colaboradores con el mismo nombre → desempata por email.

## Design
**Endpoint**: `GET /api/collaborators` → `200` con `CollaboratorDto[]`.

```json
[{ "id": "guid", "name": "Ana Pérez", "email": "ana.perez@example.com", "countryCode": "ES" }]
```

**Entidad `Collaborator`** (agregado, `Domain/Collaborators/`):
- `Id` (`Guid`), `Name`, `Email`, `CountryCode`.
- Se crea solo con `Collaborator.Create(name, email, countryCode)`, que devuelve `Result<Collaborator>` y normaliza: nombre recortado, email en minúsculas (P5), país en mayúsculas (P4).
- Invariantes: nombre no vacío y ≤ 200; email con formato válido y ≤ 254; país de dos letras.
- Errores centralizados en `CollaboratorErrors` (códigos estables en inglés): `collaborator.name`, `collaborator.email`, `collaborator.country`. Si fallan varios, se devuelven todos.

**Fuera de este UC** (va en UC-02): unicidad del email, validación del país contra Nager.Date (ADR-09), edición y baja.

**Capas previstas para la iteración b**:
- Application: `ListCollaborators` (`Query`, `Handler`, `CollaboratorDto`) y el puerto `ICollaboratorRepository` con `ListAsync`.
- Infrastructure: `CollaboratorConfiguration`, `CollaboratorRepository`, primera migración y un seeder que corre tras `ApplyMigrationsAsync`. El seeder crea los colaboradores con `Collaborator.Create` (sin `HasData`), para que la semilla pase por las invariantes; si ya hay colaboradores, no hace nada. Verificar en la migración que EF mapea el constructor privado y las propiedades de solo lectura.
- Api: `CollaboratorsController` (ruta `api/collaborators`).

**Decisiones y supuestos**
- Las rutas de la API van en inglés (`/api/collaborators`, `/api/shifts`), coherente con que el código y los textos de error van en inglés. Corrige las rutas provisionales en español de `docs/api/ENDPOINTS.md`.
- La entidad incluye ya sus invariantes de creación aunque UC-01 solo lista, porque la semilla necesita crear colaboradores válidos y una entidad sin reglas sería anémica.
- Supuesto de la Tarea 0: no hay datos reales que revisar; los datos semilla los define este UC (iteración b).
- La semilla usa códigos de país reales y válidos en Nager.Date (`ES`, `AR`, `BR`, `MX`...), para que UC-05 encuentre festivos.

## Tasks
- [ ] Domain: `Collaborator`, `CollaboratorErrors`
- [ ] Application: `ListCollaborators`, `ICollaboratorRepository` (iteración b)
- [ ] Infrastructure: configuración, repositorio, migración y semilla (iteración b)
- [ ] Api: `CollaboratorsController` (iteración b)
- [x] Tests: invariantes y normalización de `Collaborator.Create`
- [ ] Docs: `docs/api/ENDPOINTS.md` y `.http` con respuestas reales (iteración b)

## Verificación
Pendiente (iteración b): `docker compose up --build -d`, `GET /api/collaborators` con la lista semilla, y la UI de OpenAPI.

## TBDs
1. Resuelto: la semilla se aplica solo si la tabla está vacía, con un seeder (no `HasData`), porque `Collaborator` genera su `Guid` en `Create` y la semilla debe pasar por sus invariantes. Un dato semilla inválido hace fallar el arranque a la vista.

## Para la revisión
- La entidad se crea solo por `Create`, que devuelve `Result`: el dominio garantiza sus invariantes y no depende de que cada controller valide.
- Los errores salen todos juntos y con código estable en inglés, para que el front los muestre por campo.
- Normalizar email a minúsculas al crear es lo que permite una unicidad sin distinguir mayúsculas (P5) con un índice simple.
- La semilla usa `Create` y no `HasData`: así los datos de prueba cumplen las mismas reglas que los reales.
- Listar sin paginación es deliberado (P13): el volumen es chico y se documenta como decisión.
