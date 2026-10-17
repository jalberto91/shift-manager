# UC-02 (b2): endpoints de escritura de colaboradores (archivado 2026-10-17)

Commit: `e75456c`. Bloque archivado tal como estaba en `ESTADO-ACTUAL.md`:

## Bloque en curso
UC-02 en curso: (a) y (b1) cerradas y archivadas; (b2) y (c) pendientes. Ninguna iteración abierta.

Resumen de lo hecho en (b2) (no estaba en el estado vivo):
- `CollaboratorsController`: `POST` (201 + `Location` armado con la ruta), `PUT {id:guid}` (200) y `DELETE {id:guid}` (204), todos con `ToProblem`. Body de `POST`/`PUT` en `CollaboratorRequest` (archivo propio; el id de `PUT` viene de la ruta).
- Regla de código acordada: un tipo por archivo. Quedan por separar `Domain/Common/Error.cs`, `Domain/Common/Result.cs` y `tests/.../Application/Fakes.cs` (dos tipos cada uno).
- Spec corregido: `DELETE` repetido sobre un colaborador dado de baja responde `204` (idempotente), no `404`; el `404` aplica a id inexistente y a `PUT` sobre un baja.
- Verificado con curl sobre `docker compose up`: `POST` 201 + `Location`, `POST` con email repetido 409, `POST` con `{}` 400, `PUT` 200, `PUT` con id inexistente 404, `DELETE` 204 (y 204 al repetirlo). Build sin warnings, 52 tests verdes.
- Quedó en la BD del compose el colaborador `b2.test@example.com` dado de baja; su email sigue ocupado.
