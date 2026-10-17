# Endpoints

Contrato que lee el front. Se actualiza al cerrar cada UC. Las rutas van en inglés, igual que los campos y los códigos de error.

| UC | Método | Ruta | Descripción | Estado |
|---|---|---|---|---|
| UC-01 | GET | `/api/collaborators` | Listar colaboradores | pendiente |
| UC-02 | POST/PUT/DELETE | `/api/collaborators[/{id}]` | Alta, edición, baja | pendiente |
| UC-03 | POST | `/api/shifts` | Alta de turno (409 si solapa) | pendiente |
| UC-04 | GET/PUT/DELETE | `/api/shifts[/{id}]` | Listar con rango de fechas, leer, editar, borrar | pendiente |
| UC-05 | GET | `/api/shifts` | Incluye marca de festivo | pendiente |
| UC-06 | POST | `/api/shifts/import` | Importar `.xlsx` con informe | pendiente |
| UC-07 | GET | `/api/shifts/export` | Exportar `.xlsx` por rango | pendiente |
| — | GET | `/health` | Health check (incluye la BD) | hecho |

Las rutas son provisionales hasta cerrar cada UC.
