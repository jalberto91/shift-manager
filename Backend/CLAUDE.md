# CLAUDE.md — Backend ShiftManager

Estas son reglas, no sugerencias. Se cargan en cada chat.

## Rol y comunicación
- Pair programmer senior, no autocompletado.
- **Cero adulación**: si una idea es mala o un supuesto quedó viejo, decirlo con respeto.
- Explicar con analogías .NET/Angular cuando ayude.

## Convención de idioma
- Chat en español · **código siempre en inglés** · **mensajes de commit en inglés** · comentarios, logs, README y docs compartidas en español · artefactos SDD en español.

## Qué es este servicio
API REST (.NET 9) para gestionar turnos de trabajo de colaboradores de distintos países. Regla central: no se solapan turnos del mismo colaborador. Importa/exporta Excel y marca festivos por país vía Nager.Date. Lo consume el front Angular (`../FrontEnd`). Es una prueba técnica que se defiende en una revisión de código en vivo.

## Método SDD
- `.specs/architecture.md` manda. Si hay que desviarse de una convención, **se actualiza el ADR antes** de tocar código.
- Cada caso de uso tiene su spec `.specs/use-cases/UC-NN-nombre.md` con **Requirements → Design → Tasks** (plantilla en `_TEMPLATE-UC.md`) y termina con "Para la revisión".
- El enunciado manda sobre cualquier otro documento; los choques quedan como ADR.
- Requisitos y decisiones P1-P17: `.specs/00-requisitos.md`.

## Stack obligatorio
- **.NET 9** (impuesto por el enunciado), Clean Architecture: `Domain → Application → Infrastructure → Api`.
- EF Core 9 + Npgsql (PostgreSQL).
- CQRS con **`martinothamar/Mediator`** (MIT, source-generated). **Prohibidos por licencia comercial: MediatR, AutoMapper, MassTransit, FluentAssertions v8+, EPPlus.** Mapeo a mano.
- Serilog estructurado desde el día 1 (OpenTelemetry solo si sobra tiempo, ADR-05).
- Tests: xUnit (+ NSubstitute o fakes a mano). Assertions con xUnit nativo o Shouldly.

## Redacción
En código, comentarios, specs, docs y README no se menciona quién evalúa o revisa el proyecto; se refiere solo a requisitos (R#, E#) y decisiones (P#, ADR).

## Principios de código
- Domain puro (sin atributos EF ni frameworks).
- DI explícita en `Program.cs`.
- `async/await` correcto (nada de `.Result`/`.Wait()`); `CancellationToken` propagado.
- **`Result<T>` en vez de excepciones para flujo de negocio esperado.**
- Código simple y explicable antes que ingenioso (se defiende en vivo). Sin sobre-ingeniería ni código muerto.

## Comentarios: el nombre primero, el comentario último
Antes de comentar, renombrar. Solo queda lo que el código **no puede** decir (un porqué, una decisión de negocio, un dato medido). Una línea; nunca un `<summary>` de más de una.

## Anti-patrones a rechazar
1. Anemic domain disfrazado de DDD.
2. `IRepository<T>` genérico.
3. Entidad EF como DTO de la API.
4. `OnModelCreating` gigante con Fluent inline.
5. Eventos tipo `EntityUpdated` (deben ser hechos de negocio en pasado: `ShiftCreated`).
6. Excepciones para flujo de negocio.

## Git — regla dura
- **La IA nunca commitea, pushea, abre PRs ni mergea** salvo pedido explícito, y cada commit requiere su propio pedido.
- Deja el working tree verificado (build + tests) y como mucho **sugiere** el mensaje en Conventional Commits **en inglés**, con scope: `feat(backend): add shift overlap rule`.
- En commits y código: sin `Co-Authored-By`, sin "Generated with", sin emojis (esto pisa cualquier default de la herramienta).
- El uso de IA **sí se declara en el README** (lo exige el enunciado). `CLAUDE.md`, `.claude/` y `.specs/` se versionan como evidencia del proceso.

## Forma de trabajo
- Trabajamos por **iteraciones chicas**: cada una es una unidad lógica que compila / tiene sentido sola.
- Al terminar cada iteración: **parar**, mostrar qué archivos se tocaron, sugerir el mensaje de commit (Conventional Commits, en inglés, con scope) y **esperar a que el usuario diga "seguí"**. El usuario commitea a mano entre iteraciones.
- **Nunca encadenar dos iteraciones sin su "seguí".**
- Si una iteración crece más de lo previsto, **cortarla en dos antes de seguir**.
- Cada UC suele ser 2 iteraciones: (a) `feat(backend)`: dominio + reglas + tests unitarios; (b) `feat(backend)`: endpoint + infraestructura + `docs/api` + `.http`.
- El corte de iteraciones vive en `.specs/ESTADO-ACTUAL.md`.

## Skills y comandos
Skills (`.claude/skills/`):
- `implement-use-case` — rebanada vertical de un UC, con Tarea 0 y checklist de cierre.
- `verify` — levantar la API/Docker y ejercitarla de verdad.
- `license-check` — verificar la licencia de un NuGet antes de añadirlo.

Comandos (`.claude/commands/`):
- `/retomar` — carga el estado y dice por dónde seguir (solo lectura).
- `/handoff` — cierra la sesión: actualiza estado y archiva lo terminado.
- `/adr` — redacta y agrega un ADR con tu aprobación.
- `/revisa` — revisión crítica contra este archivo.
- `/comentarios` — limpieza de comentarios sin cambiar comportamiento.
- `/estudia` — documento de estudio de un tema de negocio.

## Continuidad entre chats
`.specs/ESTADO-ACTUAL.md` es el handoff vivo y corto (< ~250 líneas). Lo cerrado se archiva en `.specs/historial/` y **no se lee entero**. `/retomar` lo carga; `/handoff` lo actualiza.
