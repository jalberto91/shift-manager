# Estado actual (handoff vivo)

## Forma de trabajo (regla)
Iteraciones chicas. Al terminar cada una: **parar**, mostrar archivos tocados, sugerir mensaje de commit (Conventional Commits, **en inglés**, con scope) y **esperar "seguí"**. El usuario commitea a mano. Nunca encadenar dos iteraciones sin "seguí". Si una crece más de lo previsto, cortarla en dos antes de seguir.

## Corte de iteraciones
- Fase 1: (1) `docs(sdd)` requisitos + glosario · (2) `docs(sdd)` architecture.md + ADR-01…10 · (3) `docs(sdd)` CLAUDE.md + ESTADO-ACTUAL + historial/README + plantilla UC · (4) `chore(sdd)` skills · (5) `chore(sdd)` comandos
- Fase 2: (6) `chore(backend)` global.json, .sln, proyectos, Directory.Build.props, .gitignore · (7) `feat(backend)` plomería (Result<T>, Problem Details + traceId, Serilog, OpenAPI, /health) · (8) `feat(backend)` DbContext PostgreSQL + migración inicial · (9) `chore(docker)` Dockerfile + docker-compose.yml
- Cada UC: (a) dominio + reglas + tests unitarios · (b) endpoint + infraestructura + docs/api + .http

## ⏭️ Próximo paso
UC-01 (a) hecho (sin commitear): spec, `Collaborator` + `CollaboratorErrors` y 22 tests nuevos. Esperando tu "seguí" para **UC-01 (b)**: abrir `.specs/use-cases/UC-01-listar-colaboradores.md` (sección Design) y construir `ListCollaborators` + `ICollaboratorRepository`, la configuración EF, la primera migración con semilla, el controller y `docs/api` + `.http`. Ahí se agregan `Microsoft.EntityFrameworkCore.Design` y `dotnet-ef` 9.x local.

Decisión: **no hay migraciones hasta que existan entidades**. La primera migración (con `Collaborator` y la semilla) llega en UC-01; en ese momento se agregan `Microsoft.EntityFrameworkCore.Design` (en `Api`) y `dotnet-ef` 9.x como herramienta local (`dotnet-tools.json`), con su `license-check` ya hecho (MIT).

## Dónde estamos
- Fase 0 hecha: enunciado y requisitos cruzados sin discrepancias; SDK 9.0.308 y Docker 29.5.3 / Compose v5.1.4 disponibles.
- Iteraciones 1-5 escritas juntas (antes de fijar esta regla); pendientes de tu revisión y de commits separados. `global.json` y `.dockerignore` ya existen y se commitean con la iteración 6.
- Sin código de negocio todavía.

## Pendiente para la iteración 9 (Docker)
Hecho: el compose fija `Cors__AllowedOrigins__0=http://localhost:4200`. Cuando el front entre al compose, ajustar ese origen al puerto que publique.

## Bloque en curso
Revisión de iteraciones 1-5.

## Abierto / pendientes con terceros
- ADR-08 (Excel: ClosedXML vs MiniExcel) pendiente de `license-check`.
- Observación P5: email normalizado a minúsculas + índice único (se detalla en UC-02).

## Checklist de cierre del proyecto
- [ ] Docker completo (`docker compose up` desde `shift-manager/`)
- [ ] Tests revisados (solape, import fila a fila, degradación de Nager.Date)
- [ ] `.xlsx` de ejemplo en el repo
- [ ] `README.md` en la raíz: cómo ejecutarlo, decisiones (ADRs), qué quedó fuera, **sección "Uso de IA"**
- [ ] Revisión contra "qué valoran" (`00-requisitos.md` §2)

## Cerrado recientemente
- (nada aún)
