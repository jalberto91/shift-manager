---
name: verify
description: Levanta backend y front y recorre la pantalla de verdad (consola y red incluidas). Úsalo para verificar una feature de punta a punta o el estado de Docker.
---

# verify

Completar a medida que se descubra.

## Opción A · desarrollo (`ng serve`)
- API: desde `shift-manager/`, `docker compose up db api -d` (API en `http://localhost:5080`, health: `curl -s http://localhost:5080/health` hasta `Healthy`). Alternativa: `dotnet run --project src/ShiftManager.Api` en `../Backend` con la BD arriba.
- Front: `nvm use 22.17.1` y luego `npm start` (`http://localhost:4200`), con `proxy.conf.json` hacia `http://localhost:5080`.
- Matar `ng serve` por puerto: `Get-NetTCPConnection -LocalPort 4200 | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }`

## Opción B · Docker (R8)
- Desde `shift-manager/`: `docker compose up --build -d`. Front en `http://localhost:4200` (servicio `web`); la API sigue en `http://localhost:5080`.
- Verificar que `/api/...` desde el front responde por el proxy de nginx.
- Apagar: `docker compose down` (con `-v` resetea la BD).

## Recorrido de una pantalla
1. Abrir la pantalla y comprobar los cuatro estados: cargando, vacío, error (API apagada), con datos.
2. Ejercitar el flujo feliz y cada error de negocio por código (p. ej. email repetido, solape).
3. Revisar consola (sin errores ni warnings) y red (rutas, métodos, códigos, cuerpos).
4. Anotar lo visto en la sección "Verificación" del spec.

## Autenticación
Ninguna (ADR-06).
