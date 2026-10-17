# Endpoints

Contrato que lee el front. Se actualiza al cerrar cada UC.

| UC | Método | Ruta | Descripción | Estado |
|---|---|---|---|---|
| UC-01 | GET | `/api/colaboradores` | Listar colaboradores | pendiente |
| UC-02 | POST/PUT/DELETE | `/api/colaboradores[/{id}]` | Alta, edición, baja | pendiente |
| UC-03 | POST | `/api/turnos` | Alta de turno (409 si solapa) | pendiente |
| UC-04 | GET/PUT/DELETE | `/api/turnos[/{id}]` | Listar con rango de fechas, leer, editar, borrar | pendiente |
| UC-05 | GET | `/api/turnos` | Incluye marca de festivo | pendiente |
| UC-06 | POST | `/api/turnos/importar` | Importar `.xlsx` con informe | pendiente |
| UC-07 | GET | `/api/turnos/exportar` | Exportar `.xlsx` por rango | pendiente |
| — | GET | `/health` | Health check | pendiente |

Las rutas son provisionales hasta cerrar cada UC.
