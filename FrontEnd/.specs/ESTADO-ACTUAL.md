# Estado actual (handoff vivo)

## Forma de trabajo (regla)
Iteraciones chicas. Al terminar cada una: **parar**, mostrar archivos tocados, sugerir mensaje de commit (Conventional Commits, **en inglés**, con scope) y **esperar "seguí"**. El usuario commitea a mano. Nunca encadenar dos iteraciones sin "seguí".

## ⏭️ Próximo paso
**FE-01 Colaboradores** (UC-01 y UC-02). Crear `.specs/features/FE-01-colaboradores.md` desde `_TEMPLATE-FE.md`, con Tarea 0 (`curl` a `http://localhost:5080/api/collaborators`) y seguir `implement-feature`. Después FE-02 → FE-03 → FE-04 → FE-05 y el cierre (README raíz y checklist de valoración).

## Dónde estamos
- Fase 2 (esqueleto) hecha, sin commitear: Angular 21, PrimeNG 21.1.10 (tema Aura), ESLint, Prettier, `core/` (interceptor de errores, textos por código, notificaciones, shell, 404), `shared/ui/data-state`, proxy de desarrollo, `Dockerfile` + nginx y servicio `web` en `../docker-compose.yml`.
- Verificado: `ng build` sin warnings, 5 tests verdes, `ng lint` limpio, `docker compose up --build` sirve el front en `http://localhost:4200` y `/api` llega a la API por el proxy.
- Backend completo: UC-01 a UC-07 hechos.
- Las rutas `/shifts` y `/collaborators` del menú aún no existen (caen en 404) hasta FE-01 y FE-02.
- Entorno: Node 22.17.1 (`nvm use 22.17.1`).

## Decisiones vigentes
- Angular 21 + PrimeNG 21.1.10 (MIT) por la licencia de PrimeNG 22 (ADR-03). Versiones de PrimeNG, `@primeuix/themes`, `primeicons` y `@angular/cdk` fijadas exactas.
- Una instalación desde cero (sin lockfile) falla con `Cannot read properties of null (reading 'edgesOut')` en npm 10.5 y 10.9 (bug del resolvedor de peers con `@angular/build` 21; npm 11 no falla). El lockfile se generó con `--legacy-peer-deps`; con él, `npm install` y `npm ci` funcionan normales. Si al agregar un paquete vuelve a fallar: `--legacy-peer-deps` o npm 11.
- Fechas y horas con inputs nativos (`type="date"`, `type="time"`): sin locale ni DatePicker de PrimeNG.
- Textos de error propios en español, indexados por el código de negocio; el interceptor solo avisa 0, 5xx y red, el resto lo resuelve cada pantalla.

## Bloque en curso
Ninguno.

## Pendientes con el backend
- `POST /api/collaborators` y `POST /api/shifts` devuelven `Location` hacia un `GET /{id}` que existe solo para turnos; el front usa el cuerpo de la respuesta.
- `ShiftDto` trae solo `collaboratorId`: el front cruza con el listado de colaboradores (los dados de baja no aparecen en él).
- Los motivos de fila del import son mensajes en inglés sin código; se muestran tal cual como dato del informe.

## Checklist de cierre del proyecto (parte front)
- [x] `docker compose up` desde `shift-manager/` sirve el front en `http://localhost:4200`
- [ ] FE-01…FE-05 verificadas contra el backend real
- [ ] Tests: store de turnos (con mapeo de errores) y formulario con el caso de solape
- [ ] `../README.md` con la parte del front (ejecución, decisiones) y uso de IA
- [ ] Recorrido de los criterios de valoración (`00-requisitos.md` §2) con ✅/❌

## Cerrado recientemente
- 2026-10-20 · Fase 2: esqueleto técnico del front y Docker (sin commitear aún).
- 2026-10-17 · Fase 1: andamiaje SDD del front → commit `090ac50`.
