# 00 · Requisitos del backend

> Copia de lo que le toca al backend del enunciado de la prueba técnica. Si algo choca con otro documento, **manda el enunciado**.

## 1. Objetivo
API REST en **.NET 9** + front Angular para gestionar turnos de trabajo de colaboradores de distintos países. Se evalúan buenas decisiones, código claro y alcance delimitado. Plazo 7 días, 6-8 h. Revisión de 30 min recorriendo el código, con preguntas y **un pequeño cambio en vivo**.

## 2. Requisitos obligatorios

| # | Requisito |
|---|---|
| R1 | **Colaboradores**: CRUD con nombre, email y país. |
| R2 | **Turnos**: CRUD con colaborador, fecha, hora de inicio y hora de fin. |
| R3 | **Regla de negocio**: no se pueden crear ni modificar turnos que se solapen para el mismo colaborador. |
| R4 | **Importación Excel**: `.xlsx` con turnos (p. ej. `Email`, `Fecha`, `HoraInicio`, `HoraFin`). Valida fila a fila y devuelve un informe con filas importadas y fallidas, con el motivo de cada error. |
| R5 | **Exportación Excel**: turnos filtrados por rango de fechas. |
| R6 | **API externa**: festivos del país de cada colaborador (p. ej. Nager.Date); señalar en el listado los turnos que caen en festivo. |
| R7 | **Persistencia** en BD relacional (a elección). |
| R8 | **Docker**: todo se levanta con un único comando (`docker-compose up`). |
| R9 | **Documentación de la API** con OpenAPI/Swagger. |
| R10 | **Pruebas automatizadas**: las más relevantes; calidad sobre cantidad. |
| R11 | **README**: cómo ejecutarlo, decisiones de diseño y qué quedó fuera (y por qué). |

**Entrega**
- E1 Repositorio Git con historial de commits legible.
- E2 Librerías libres a elección.
- E3 Se puede usar IA, pero hay que indicarlo en el README (qué y para qué). Después piden explicar el código.
- E4 Ambigüedades: decisión razonable documentada en el README.

**Qué valoran**: que funcione y cumpla las reglas incluidos los casos límite · organización y claridad del backend · calidad y sentido de las pruebas · autonomía (decisiones justificadas, alcance gestionado) · manejo de errores y fallos de la API externa · facilidad para ejecutar · front funcional y limpio.

## 3. Restricciones técnicas

| Tema | Restricción |
|---|---|
| Backend | .NET 9 (impuesto) → ADR-01. EF Core 9. |
| BD | Relacional; elegida PostgreSQL (P1). |
| Ejecución | `docker-compose up` único: BD + API (+ front después), migraciones al arrancar, API espera a la BD (healthcheck). |
| Docs API | OpenAPI de .NET 9 + UI (Swagger UI o Scalar), visible también en Docker. |
| Excel | EPPlus prohibido (licencia comercial). Candidatos: ClosedXML (MIT) o MiniExcel (Apache-2.0) → `license-check`. |
| Festivos | Nager.Date: `GET https://date.nager.at/api/v3/PublicHolidays/{year}/{countryCode}` y `GET /api/v3/AvailableCountries`. País en ISO 3166-1 alfa-2. |
| Auth | No pedida. |
| Licencias | Prohibidos por regla propia: MediatR, AutoMapper, MassTransit, FluentAssertions v8+. |
| IA | Permitida y declarada en el README. El SDD se versiona (P14). |

## 4. Bonus que suman (sin salirse del alcance)
- Casos límite de R3 con tests (bordes que se tocan, editar el mismo turno…).
- Resiliencia ante Nager.Date: timeout, reintentos acotados, caché, listado que sigue funcionando con festivo "desconocido".
- Import que detecta solapes dentro del mismo archivo.
- Un `.xlsx` de ejemplo en el repo.
- Commits pequeños, uno por paso.

## 5. Casos de uso (orden de implementación)

| UC | Nombre | Cubre |
|---|---|---|
| UC-01 | Listar colaboradores | R1, R7, R9 — prueba toda la plomería de punta a punta |
| UC-02 | Alta/edición/baja de colaborador | R1 — email único, país ISO válido |
| UC-03 | Alta de turno con regla de no solape | R2, R3 — el corazón; tests de bordes |
| UC-04 | Listar / leer / editar / borrar turnos (filtro por rango de fechas) | R2, R3 — editar excluye el propio turno |
| UC-05 | Festivos en el listado de turnos | R6 — cliente HTTP + caché + degradación |
| UC-06 | Importar turnos desde Excel con informe | R4, R3 — reutiliza validación de UC-03 |
| UC-07 | Exportar turnos por rango a Excel | R5 — reutiliza filtro de UC-04 |
| — | Cierre: Docker completo, README, tests revisados | R8, R10, R11 |

## 6. Decisiones P1-P17

| # | Decisión |
|---|---|
| P1 | BD: **PostgreSQL**, EF Core 9 + Npgsql. |
| P2 | Dos turnos que se tocan en el borde (09-13 y 13-17) **no** se solapan: intervalos semiabiertos `[inicio, fin)`. Solape si `inicioA < finB && inicioB < finA`. |
| P3 | Turnos que cruzan la medianoche **no se admiten**: `HoraFin > HoraInicio` el mismo día. Va a "qué quedó fuera". |
| P4 | País = código **ISO 3166-1 alfa-2** (`ES`, `AR`, `BR`), validado contra `AvailableCountries` de Nager.Date. |
| P5 | Email **único**, comparación sin distinguir mayúsculas (el import identifica al colaborador por email). |
| P6 | Borrar un colaborador con turnos: **se bloquea** con `409` y mensaje claro. Sin cascada. |
| P7 | Import **parcial**: se guardan las filas válidas y se informan las inválidas. |
| P8 | Import valida solapes **dentro del archivo y contra la BD**; la fila que llega segunda falla con el motivo. |
| P9 | Excel: celdas de tipo fecha/hora **y** texto `yyyy-MM-dd` / `dd/MM/yyyy` y `HH:mm`. Encabezados por nombre, sin importar orden ni mayúsculas. Límite de tamaño (p. ej. 5 MB). |
| P10 | Festivos de Nager.Date para el país y año del turno; globales y regionales se marcan (con nombre). Si la API falla: el listado sale igual con `holiday = unknown`. Caché por país+año. |
| P11 | El front tendrá gestión mínima de colaboradores y la BD lleva **datos semilla** para arrancar usable. |
| P12 | Las horas del turno son **hora local del colaborador**; sin conversiones. |
| P13 | **Sin paginación** en v1 (volumen chico); el filtro por fechas acota. Se documenta. |
| P14 | `CLAUDE.md`, `.claude/` y `.specs/` **se versionan**. |
| P15 | UI del front: PrimeNG (decisión del front). |
| P16 | README, comentarios y textos de UI en **español**; código en inglés. |
| P17 | Duración del turno: solo `fin > inicio`; sin mínimos/máximos. |

## 7. Decisiones tomadas en la Fase 0 (sesión de arranque)
- Conflicto de solape → **409 Conflict** + Problem Details (ADR-10).
- Fecha y horas del turno → `DateOnly` + `TimeOnly` sin zona (coherente con P3/P12).

## 8. Repositorio
Monorepo en `shift-manager/`: `Backend/`, `FrontEnd/`, `docker-compose.yml`, `README.md`. Commits con scope: `feat(backend): …`, hechos por la persona, uno por paso.
