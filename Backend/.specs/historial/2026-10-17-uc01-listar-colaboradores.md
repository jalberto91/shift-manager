# 2026-10-17 · UC-01 (b) Listar colaboradores

Archivo del estado vivo tal como estaba al cerrar este bloque, más el commit resultante (`2a8b8cd`).

## Próximo paso (verbatim del estado)
**UC-01 (b)**: abrir `.specs/use-cases/UC-01-listar-colaboradores.md` (sección Design) y construir:
- Application: `ListCollaborators` (Query, Handler, `CollaboratorDto`) y el puerto `ICollaboratorRepository` con `ListAsync` (orden por nombre y luego email).
- Infrastructure: `CollaboratorConfiguration`, `CollaboratorRepository`, seeder que usa `Collaborator.Create` y corre tras `ApplyMigrationsAsync` (solo con la tabla vacía), y la **primera migración**. Antes, `license-check` ya hecho: agregar `Microsoft.EntityFrameworkCore.Design` (en `Api`, MIT) y `dotnet-ef` 9.x como herramienta local (`dotnet-tools.json`); el `dotnet-ef` global es la 10.0.7.
- Api: `CollaboratorsController` en `api/collaborators`, heredando de `ApiControllerBase`.
- Docs: `docs/api/ENDPOINTS.md` y un `.http` con respuestas reales; verificar con `docker compose up`.
Confirmar que EF mapea el constructor privado y las propiedades de solo lectura de `Collaborator`.

## Dónde estamos (verbatim del estado, al cerrar)
- UC-01 (b) implementado y verificado: migración `InitialCreate`, semilla de 5 colaboradores y `GET /api/collaborators` (200). `dotnet-ef` 9.0.21 es herramienta local (`.config/dotnet-tools.json`): `dotnet dotnet-ef`.

## Checklist de cierre de UC-01 (verbatim del estado)
- [x] Build sin warnings y tests verdes (parte a)
- [x] E2E con `docker compose up`
- [x] ¿ADR nuevo? (ninguno)
- [x] Spec con Tasks marcadas
- [x] `docs/api/ENDPOINTS.md` actualizado
- [x] `.http` con respuestas reales
- [x] TBDs anotados

## Resultado
- Build sin warnings, 27 tests verdes, E2E con `docker compose up --build -d`: migración aplicada, 5 colaboradores sembrados, `GET /api/collaborators` devuelve 200 ordenado por nombre y email.
- EF mapea el constructor privado y las propiedades de solo lectura sin configuración extra; el `Id` se declara `ValueGeneratedNever()` porque lo genera el dominio.
- Sin tests nuevos en esta parte: el handler solo mapea y el orden vive en SQL; la verificación es el E2E.
- El `.http` y `ENDPOINTS.md` usan `<guid>` en los `id`, porque cambian al recrear la BD.
