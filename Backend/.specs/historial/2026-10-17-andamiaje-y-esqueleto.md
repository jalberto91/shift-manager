# 2026-10-17 · Andamiaje SDD, esqueleto técnico y UC-01 (a)

Archivo del estado vivo tal como estaba al cerrar estos bloques, más los commits resultantes.

## Corte de iteraciones (verbatim del estado)
- Fase 1: (1) `docs(sdd)` requisitos + glosario · (2) `docs(sdd)` architecture.md + ADR-01…10 · (3) `docs(sdd)` CLAUDE.md + ESTADO-ACTUAL + historial/README + plantilla UC · (4) `chore(sdd)` skills · (5) `chore(sdd)` comandos
- Fase 2: (6) `chore(backend)` global.json, .sln, proyectos, Directory.Build.props, .gitignore · (7) `feat(backend)` plomería (Result<T>, Problem Details + traceId, Serilog, OpenAPI, /health) · (8) `feat(backend)` DbContext PostgreSQL + migración inicial · (9) `chore(docker)` Dockerfile + docker-compose.yml
- Cada UC: (a) dominio + reglas + tests unitarios · (b) endpoint + infraestructura + docs/api + .http

## Dónde estamos (verbatim del estado)
- Fase 0 hecha: enunciado y requisitos cruzados sin discrepancias; SDK 9.0.308 y Docker 29.5.3 / Compose v5.1.4 disponibles.
- Iteraciones 1-5 escritas juntas (antes de fijar esta regla); pendientes de tu revisión y de commits separados. `global.json` y `.dockerignore` ya existen y se commitean con la iteración 6.
- Sin código de negocio todavía.

## Pendiente para la iteración 9 (Docker) (verbatim del estado)
Hecho: el compose fija `Cors__AllowedOrigins__0=http://localhost:4200`. Cuando el front entre al compose, ajustar ese origen al puerto que publique.

## Decisión sobre migraciones (verbatim del estado)
**No hay migraciones hasta que existan entidades**. La primera migración (con `Collaborator` y la semilla) llega en UC-01; en ese momento se agregan `Microsoft.EntityFrameworkCore.Design` (en `Api`) y `dotnet-ef` 9.x como herramienta local (`dotnet-tools.json`), con su `license-check` ya hecho (MIT).

## Commits resultantes (de `git log`)
| Commit | Mensaje |
|---|---|
| 5840f54 | docs(sdd): add exercise requirements and domain glossary |
| 7363691 | docs(sdd): add cross-cutting conventions and ADR-01 to ADR-10 |
| 2147d2a | docs(sdd): add CLAUDE.md, use case template and docs/api skeleton |
| 18d8efa | chore(sdd): add implement-use-case, verify and license-check skills |
| 5e82a7f | chore(sdd): add retomar, handoff, adr, revisa, comentarios and estudia commands |
| e4ea56e | chore(backend): add solution, layered projects, Directory.Build.props and .gitignore |
| 4fc8550 | docs(sdd): switch commit messages to English |
| c58df3b | docs(sdd): use English business error codes and define shift overlap contract |
| 8790e5d | feat(backend): add Result<T>, error handling, Serilog, OpenAPI and health check |
| 88a9fc1 | feat(backend): add AppDbContext with PostgreSQL, database health check and startup migrations |
| 2221579 | chore(docker): add backend Dockerfile and docker-compose with PostgreSQL and API |
| 9fe4e9e | feat(backend): add Collaborator entity with creation rules and UC-01 spec |
