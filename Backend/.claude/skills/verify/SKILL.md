---
name: verify
description: Levanta la API (dotnet run o docker compose) y la ejercita de verdad con curl/.http. Úsalo para verificar de punta a punta un UC o el estado de Docker.
---

# verify

Completar a medida que se descubra. Valores marcados "a confirmar" se fijan en la Fase 2.

## Opción A · `dotnet run` (desarrollo)
- Requiere una PostgreSQL accesible (p. ej. `docker compose up db -d` desde `shift-manager/`).
- Comando: `dotnet run --project src/ShiftManager.Api` (puerto fijo 5080 por `launchSettings.json`). Con `--no-build` si ya se compiló.
- UI de OpenAPI (Scalar): `http://localhost:5080/scalar/v1`; documento: `/openapi/v1.json`.
- Esperar a que responda: reintentar `curl -s http://localhost:5080/health` hasta `Healthy`.
- **Matar por puerto** (en Windows `dotnet run` deja un hijo `.exe` escuchando):
  `Get-NetTCPConnection -LocalPort 5080 | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }`

## Opción B · Docker (R8: entorno completo con un comando)
- Desde `shift-manager/`: `docker compose up --build -d`; logs: `docker compose logs -f api`.
- Verificar: `/health`, la UI de OpenAPI (ruta a confirmar) y los datos semilla.
- Apagar: `docker compose down` (con `-v` para resetear la BD).

## Autenticación
Ninguna (ADR-06).

## Ejemplos
```
curl -s http://localhost:5080/health
curl -s http://localhost:5080/api/colaboradores
```
Cada UC deja un `.http` con requests y respuestas reales (ver `implement-use-case`, Paso 4).
