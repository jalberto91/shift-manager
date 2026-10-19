# Shift Manager — Gestor de turnos de trabajo

Aplicación para gestionar los turnos de trabajo de colaboradores de distintos países: API REST en
**.NET 9** + front en **Angular**. Impide turnos solapados para un mismo colaborador, importa y exporta
turnos en Excel y marca los turnos que caen en un festivo del país del colaborador (Nager.Date).

> **Estado:** el **backend está completo** (todos los requisitos de la API). El **front** está en
> desarrollo en `FrontEnd/` y se sumará al mismo `docker compose`.

## Cómo ejecutarlo

Requisito: **Docker** (con Docker Compose v2).

```bash
docker compose up --build
```

Desde la raíz del repositorio. Levanta PostgreSQL y la API, aplica las migraciones y carga datos de
ejemplo (5 colaboradores de distintos países).

| Qué | URL |
|---|---|
| Documentación interactiva de la API (Scalar) | http://localhost:5080/scalar |
| Documento OpenAPI | http://localhost:5080/openapi/v1.json |
| Health check (incluye la BD) | http://localhost:5080/health |
| PostgreSQL (opcional, para inspeccionar) | `127.0.0.1:5433` · usuario, contraseña y base: `shiftmanager` |

Para empezar con la base de datos limpia: `docker compose down -v` y de nuevo `docker compose up --build`.

**Probar el import:** hay un archivo de ejemplo en
[`Backend/docs/samples/shifts-import-example.xlsx`](Backend/docs/samples/shifts-import-example.xlsx)
para subir a `POST /api/shifts/import` desde Scalar.

### Desarrollo local (sin Docker para la API)

Requiere el SDK de **.NET 9** (fijado en `Backend/global.json`).

```bash
docker compose up -d db          # solo la base de datos
cd Backend
dotnet test                      # tests
dotnet run --project src/ShiftManager.Api
```

Ejemplos de requests listos para usar en [`Backend/docs/api/*.http`](Backend/docs/api/).

## Qué hace la API

| Método | Ruta | Qué hace |
|---|---|---|
| `GET` | `/api/collaborators` | Lista los colaboradores activos |
| `POST` `PUT` `DELETE` | `/api/collaborators[/{id}]` | Alta, edición y baja (lógica) de colaboradores |
| `GET` | `/api/shifts?from=&to=` | Lista turnos por rango de fechas, con marca de festivo |
| `GET` `POST` `PUT` `DELETE` | `/api/shifts[/{id}]` | Leer, crear, editar y borrar turnos (`409` si se solapa) |
| `POST` | `/api/shifts/import` | Importa un `.xlsx` y devuelve un informe por fila |
| `GET` | `/api/shifts/export?from=&to=` | Descarga los turnos del rango en `.xlsx` |

Contrato completo: [`Backend/docs/api/ENDPOINTS.md`](Backend/docs/api/ENDPOINTS.md) · formato de errores:
[`Backend/docs/api/ERROR-HANDLING.md`](Backend/docs/api/ERROR-HANDLING.md).

## Arquitectura

Clean Architecture en cuatro proyectos, con dependencias hacia adentro:

```
ShiftManager.Domain          entidades y reglas puras (Collaborator, Shift, regla de solape), Result<T>
ShiftManager.Application     casos de uso (CQRS con Mediator), puertos (repositorios, catálogos, Excel)
ShiftManager.Infrastructure  EF Core + PostgreSQL, clientes de Nager.Date, lector/escritor Excel (ClosedXML)
ShiftManager.Api             controllers finos, Problem Details, OpenAPI, composición en Program.cs
tests/ShiftManager.UnitTests dominio, handlers con fakes e infraestructura aislada
```

## Decisiones de diseño

Cada decisión está registrada como ADR en
[`Backend/.specs/architecture.md`](Backend/.specs/architecture.md). Las principales:

- **Regla de solape** — intervalos semiabiertos `[inicio, fin)`: dos turnos del mismo colaborador y el
  mismo día se solapan si `inicioA < finB && inicioB < finA`. Un turno de 09:00–13:00 y otro de
  13:00–17:00 **no** se solapan. Al editar, el propio turno se excluye. El conflicto devuelve
  **`409 Conflict`** con el código `shift.overlap` y el id del turno con el que choca: es un conflicto con
  otro recurso, no un dato mal formado (los errores de validación son `400`).
- **Fechas y horas sin zona horaria** — un turno es "fecha + hora de inicio + hora de fin" en la hora
  local del colaborador (`DateOnly` + `TimeOnly`). Se ve igual desde cualquier país y no hay
  conversiones que puedan correr un turno de día.
- **Sin turnos que crucen la medianoche** — la hora de fin debe ser mayor que la de inicio el mismo día.
- **Errores uniformes** — RFC 7807 Problem Details con `traceId`; los errores de negocio llegan como
  `Result<T>` (no excepciones) con códigos estables en inglés (`collaborator.email-taken`,
  `shift.overlap`…) para que el front los enrute.
- **Fallos de la API externa (Nager.Date)** — timeout de 3 s y caché de las respuestas correctas. Si la
  API no responde, el listado de turnos **sigue funcionando** y el festivo se marca como desconocido; el
  alta de un colaborador se acepta si el país tiene formato ISO 3166-1 alfa-2, y se registra un warning.
  Se usa la API **v4** de Nager.Date (la v3 está deprecada).
- **Import parcial** — se guardan las filas válidas y se informan las que fallaron con todos sus motivos
  (también solapes contra la base y dentro del mismo archivo). Acepta celdas de fecha/hora reales o texto
  en formatos fijos, sin depender de la configuración regional. Límite de 5 MB.
- **Colaboradores** — email único sin distinguir mayúsculas; el país es un código ISO alfa-2. La baja es
  **lógica**: conserva sus turnos y su email sigue reservado.
- **Licencias** — solo librerías libres: Mediator (`martinothamar/Mediator`, MIT) en lugar de MediatR,
  mapeo a mano en lugar de AutoMapper y **ClosedXML** (MIT) en lugar de EPPlus.
- **PostgreSQL** — imagen liviana y tipos `date`/`time` nativos. Las migraciones y los datos de ejemplo se
  aplican al arrancar, para que el proyecto se levante con un solo comando.

## Tests

Pruebas unitarias con xUnit (`Backend/tests/ShiftManager.UnitTests`), centradas en lo que más importa:

- **Regla de solape** y sus casos límite: bordes que se tocan, editar el mismo turno, otro colaborador,
  otro día.
- **Import fila a fila**: columnas faltantes, formatos de fecha y hora, solapes contra la base y dentro
  del archivo.
- **Degradación ante Nager.Date**: respuestas de error y timeouts simulados con un `HttpMessageHandler`
  de prueba.
- Lectura y escritura real de `.xlsx` con ClosedXML.

```bash
cd Backend && dotnet test
```

## Qué quedó fuera (y por qué)

- **Autenticación** — el enunciado no la pide; la API no está pensada para exponerse públicamente.
- **Zonas horarias por colaborador** — mostrar los turnos en la hora de quien los mira requeriría guardar
  la zona IANA de cada colaborador (con un offset no alcanza por el horario de verano). Se priorizó la
  regla de negocio.
- **Turnos que cruzan la medianoche** — simplifica la regla de solape; se puede modelar después con una
  fecha de fin.
- **Concurrencia** — dos altas simultáneas con el mismo email, o dos turnos solapados creados en el mismo
  instante, no están protegidas a nivel de transacción (la del email termina en un `500` por el índice
  único). Aceptable para el alcance de la prueba.
- **Paginación** — el volumen es chico y el filtro por fechas acota el listado.
- **Archivos muy grandes** — por encima de ~30 MB el servidor corta la subida con `413` antes de la
  validación propia de 5 MB.
- **Reimportar el mismo archivo** — las filas repetidas fallan como solape; no hay una definición propia
  de "duplicado".
- **Observabilidad avanzada** — logs estructurados con Serilog a consola; sin trazas distribuidas.

## Uso de IA

El enunciado permite usar herramientas de IA indicándolo. Usé **Claude (Anthropic), a través de Claude
Code**, como asistente de programación, con una metodología de **Spec-Driven Development** que armé para
trabajar con IA de forma controlada:

- **Para qué la usé:** convertir el enunciado en requisitos y casos de uso, redactar las especificaciones
  y los ADRs, generar el código por iteraciones chicas, escribir los tests y la documentación de la API.
- **Cómo la controlé:** cada caso de uso tiene una especificación (requisitos → diseño → tareas) antes de
  escribir código; el trabajo avanzó en iteraciones que revisé y commiteé a mano una por una (por eso el
  historial es granular); las decisiones de diseño las tomé yo y quedaron registradas como ADRs.
- **Dónde verlo:** los artefactos de la metodología están versionados a propósito, como evidencia del
  proceso: `Backend/CLAUDE.md` (reglas de trabajo), `Backend/.specs/` (requisitos, arquitectura, ADRs,
  especificación de cada caso de uso y su historial) y `Backend/.claude/` (procedimientos reutilizables).

## Estructura del repositorio

```
docker-compose.yml   PostgreSQL + API (el front se suma al terminarlo)
Backend/             API .NET 9 — solución ShiftManager.sln, docs/ y .specs/
FrontEnd/            Front Angular (en desarrollo)
```
