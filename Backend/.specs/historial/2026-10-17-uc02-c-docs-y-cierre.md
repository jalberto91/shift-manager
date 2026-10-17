# UC-02 (c): documentación y cierre de colaboradores (archivado 2026-10-17)

Commit: `b7a03c5`. Bloque archivado tal como estaba en `ESTADO-ACTUAL.md`:

## Bloque en curso
UC-02: (a), (b1) y (b2) cerradas y archivadas; (c) hecha (docs, `.http` y E2E), pendiente de commit y de archivar. Ninguna iteración abierta.

## Checklist de cierre de UC-02 (Paso 4 de `implement-use-case`)
- [x] Build sin warnings y tests verdes (52)
- [x] E2E con `docker compose up`
- [x] ¿ADR nuevo? No: ADR-11 ya cubre la baja lógica
- [x] Spec con Tasks marcadas y "Para la revisión"
- [x] `docs/api/ENDPOINTS.md` actualizado
- [x] `.http` con respuestas reales
- [x] TBDs anotados

Resumen de lo hecho en (c) (no estaba en el estado vivo):
- Solo docs y verificación, sin código de producción: `docs/api/collaborators.http` (6 requests con respuestas reales), `docs/api/ENDPOINTS.md` (sección UC-02 con acciones y códigos de error; UC-01 corregido a "colaboradores activos") y spec con "Verificación" y TBDs 3 y 4.
- E2E tras `docker compose down -v` + `up --build`: `POST` 201/400/400 país desconocido/409, `PUT` 200/404/409, `DELETE` 204 (idempotente)/404; el dado de baja sale del listado y su email sigue ocupado.
- El mapeo `DbUpdateException` → `409` quedó fuera de (c) y registrado como TBD 3 (iteración opcional).
- Quedó en la BD del compose el colaborador `test.user@example.com` dado de baja; su email sigue ocupado.
