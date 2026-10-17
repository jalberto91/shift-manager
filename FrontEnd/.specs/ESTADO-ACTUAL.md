# Estado actual (handoff vivo)

## Forma de trabajo (regla)
Iteraciones chicas. Al terminar cada una: **parar**, mostrar archivos tocados, sugerir mensaje de commit (Conventional Commits, **en inglés**, con scope) y **esperar "seguí"**. El usuario commitea a mano. Nunca encadenar dos iteraciones sin "seguí".

## ⏭️ Próximo paso
**Fase 2 · esqueleto técnico**, tras el OK del usuario. Orden: `license-check` de paquetes (primeng, tema, angular-eslint, prettier) y tabla para aprobar → `nvm use 22.17.1` → `ng new` sin pisar `CLAUDE.md`/`.specs`/`.claude` → `core/` (interceptor de errores, notificaciones, shell, 404, `proxy.conf.json`) → `shared/` mínimo → `Dockerfile` + nginx + servicio `web` en `../docker-compose.yml`. Después, **FE-01 Colaboradores** (única pantalla cuyo backend existe hoy: UC-01 y UC-02).

## Dónde estamos
- Fase 1 (andamiaje SDD) hecha: solo archivos de método, sin código de la app. Sin commitear.
- Backend: UC-01 y UC-02 cerrados (`/api/collaborators`); `/api/shifts*` (UC-03…UC-07) pendiente.
- Entorno: Node por defecto 18.16 (no sirve para el CLI actual); usar `nvm use 22.17.1`. Docker 29.5.

## Decisiones vigentes
- Numeración FE-01…FE-05 según `00-requisitos.md` §5. UI: PrimeNG (P15).
- Textos de error propios en español, indexados por el código de negocio del backend.

## Bloque en curso
Ninguno.

## Pendientes con el backend
- UC-03…UC-07 sin endpoints: bloquean FE-02 (listado y festivo), FE-03 (formulario), FE-04 (importar) y FE-05 (exportar).
- `POST /api/collaborators` devuelve `Location: /api/collaborators/{id}` pero no existe `GET /{id}`; el front no lo necesita (usa el cuerpo de la respuesta).
- `docker-compose.yml`: `Cors__AllowedOrigins__0` apunta a `:4200`; con el proxy de nginx no se usa en Docker, solo si se sirve sin proxy.
- Los mensajes de error del backend están en inglés: el front no los muestra (ver ADR-04).

## Checklist de cierre del proyecto (parte front)
- [ ] `docker compose up` desde `shift-manager/` sirve el front en un puerto documentado
- [ ] FE-01…FE-05 verificadas contra el backend real
- [ ] Tests: store de turnos (con mapeo de errores) y formulario con el caso de solape
- [ ] `../README.md` con la parte del front (ejecución, decisiones) y uso de IA
- [ ] Recorrido de los criterios de valoración (`00-requisitos.md` §2) con ✅/❌

## Cerrado recientemente
- 2026-10-17 · Fase 1: andamiaje SDD del front (sin commitear aún).
