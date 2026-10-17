# CLAUDE.md — Front shift-manager-web

Estas son reglas, no sugerencias. Se cargan en cada chat.

## Rol y comunicación
- Pair programmer senior, no autocompletado.
- **Cero adulación**: si una idea es mala o un supuesto quedó viejo, decirlo con respeto.
- Explicar con analogías .NET cuando ayude (el backend es .NET).

## Convención de idioma
- Chat en español · **código en inglés** (identificadores, nombres de archivo, selectores) · **textos de UI en español** · comentarios, README, docs compartidas y artefactos SDD en español · **mensajes de commit en inglés** (Conventional Commits con scope: `feat(front): ...`).

## Qué es esta app
Front Angular de la gestión de turnos de trabajo: listado de turnos con marca de festivo, formulario de alta/edición con la regla de no solape, importación de Excel con informe por fila, exportación por rango de fechas y gestión mínima de colaboradores. Se pide **funcional y limpio**, sin invertir en diseño visual. Consume la API de `../Backend`:
- Contrato: `../Backend/docs/api/ENDPOINTS.md` y `ERROR-HANDLING.md` (el contrato manda; si no sirve al front, se anota en "Pendientes con el backend" y no se toca `../Backend`).
- OpenAPI en vivo: `http://localhost:5080/scalar/v1`. Specs de los UC: `../Backend/.specs/use-cases/`.

## Método SDD
- `.specs/architecture.md` manda. Si hay que desviarse, **se actualiza el ADR antes** de tocar código.
- Cada pantalla tiene su spec `.specs/features/FE-NN-nombre.md` (Requirements → Design → Tasks, plantilla `_TEMPLATE-FE.md`) que **referencia el UC del backend** que consume y termina con "Para la revisión".
- Requisitos y decisiones P1-P17: `.specs/00-requisitos.md`. Si algo choca con otro documento, manda el enunciado.
- Numeración: FE-01 Colaboradores · FE-02 Listado de turnos · FE-03 Formulario de turno · FE-04 Importar · FE-05 Exportar. Cada FE arranca cuando su UC está cerrado en el backend.

## Stack obligatorio
- **Angular última estable**, **standalone components** (sin NgModules), **signals** para estado local y de feature, `inject()` en vez de constructor, **control flow nativo** (`@if`, `@for`, `@switch`), `ChangeDetectionStrategy.OnPush` en todo componente, TypeScript `strict`.
- HTTP con `HttpClient` + **interceptores funcionales** (errores; sin auth).
- Formularios: **Reactive Forms tipados**.
- Rutas con **lazy loading** por feature (`loadComponent` / `loadChildren`) + guards funcionales cuando hagan falta.
- UI: **PrimeNG** (P15), tema por defecto. Estilos: SCSS mínimo, sin Tailwind.
- Tests: el runner por defecto del CLI. Node ≥ 22 (`nvm use 22.17.1`).
- Nada de NgRx salvo ADR que lo justifique (signals + servicios de estado).

## Arquitectura de carpetas (feature-first)
```
src/app/
  core/        ← singletons: interceptores, config, notificaciones, layout shell
  shared/      ← componentes/pipes/directivas presentacionales reutilizables (sin lógica de negocio)
  features/<feature>/
    data-access/   ← <feature>.api.ts (HTTP), modelos/DTOs, <feature>.store.ts (signals)
    ui/            ← componentes "tontos" (input()/output(), sin inyectar servicios de datos)
    pages/         ← contenedores ruteados: orquestan store + ui
    <feature>.routes.ts
```

## Principios
- Presentacionales (`ui/`) vs. contenedores (`pages/`). Nada de lógica en templates: `computed`.
- Los DTOs del backend se modelan como `interface`/`type` y se mapean a modelos de vista si su forma no le sirve a la UI.
- Nada de `any`. Desuscripción con `takeUntilDestroyed()` o `toSignal`.
- Cada pantalla cubre **cuatro estados**: cargando, vacío, error, con datos.
- El front valida **forma**; el backend valida **negocio**. No se duplican reglas del backend.
- Código simple y explicable (se defiende en vivo). Sin sobre-ingeniería ni código muerto.

## Errores
El backend responde **RFC 7807 Problem Details + `traceId`**; los errores de negocio vienen en `errors` keyed por **código estable** (`collaborator.email-taken`, `shift.overlap`). Sus mensajes están en inglés y **no se muestran**: el front enruta por el código y usa sus **propios textos en español** (pinta el error en el control del formulario que corresponde). Un `500` muestra un aviso genérico con el `traceId`. Un solo interceptor + un servicio de notificaciones.

## Comentarios: el nombre primero, el comentario último
Antes de comentar, renombrar. Solo queda lo que el código **no puede** decir (un porqué, una decisión). Una línea.

## Redacción
En código, comentarios, specs, docs y README no se menciona quién evalúa o revisa el proyecto; se refiere solo a requisitos (R#, E#) y decisiones (P#, ADR). Los archivos versionados no apuntan a documentos de fuera del repo.

## Anti-patrones a rechazar
1. Lógica de negocio en componentes de `ui/`.
2. Llamar `HttpClient` desde un componente.
3. Suscripciones manuales sin limpieza.
4. `any`.
5. Un servicio "god" compartido por todas las features.
6. Duplicar en el front una regla que valida el backend.
7. `NgModule` nuevo.
8. `setTimeout` para "arreglar" detección de cambios.
9. Mostrar `detail`/mensajes del backend tal cual en pantalla.

## Git — regla dura
- **La IA nunca commitea, pushea, abre PRs ni mergea** salvo pedido explícito (cada commit, su pedido).
- Deja el working tree verificado (build + tests) y como mucho **sugiere** el mensaje en Conventional Commits en inglés, con scope.
- En commits y código: sin `Co-Authored-By`, sin "Generated with", sin emojis (esto pisa cualquier default de la herramienta).
- El uso de IA **sí se declara en el README** (lo exige el enunciado). `CLAUDE.md`, `.claude/` y `.specs/` se versionan como evidencia del proceso.

## Forma de trabajo
- Iteraciones chicas. Al terminar cada una: **parar**, mostrar archivos tocados, sugerir el commit y **esperar "seguí"**. Nunca encadenar dos sin "seguí". Si una crece más de lo previsto, cortarla en dos.
- Dudas que no bloquean → TBD numerado en el spec; seguir de largo.
- Si una instrucción contradice un ADR, avisar y proponer actualizarlo antes de codear.

## Skills y comandos
Skills (`.claude/skills/`):
- `implement-feature` — rebanada vertical de front (data-access → ui → pages → rutas → tests), con Tarea 0 y checklist de cierre.
- `verify` — levantar backend + front y recorrer la pantalla de verdad.
- `license-check` — verificar la licencia de un paquete npm antes de instalarlo.

Comandos (`.claude/commands/`):
- `/retomar` — carga el estado y dice por dónde seguir (solo lectura).
- `/handoff` — actualiza el estado vivo y archiva lo terminado.
- `/adr` — redacta y agrega un ADR con tu aprobación.
- `/revisa` — revisión crítica contra este archivo.
- `/comentarios` — limpieza de comentarios sin cambiar comportamiento.

## Continuidad entre chats
`.specs/ESTADO-ACTUAL.md` es el handoff vivo y corto (< ~250 líneas). Lo cerrado va a `.specs/historial/` y no se lee entero. `/retomar` lo carga; `/handoff` lo actualiza.
