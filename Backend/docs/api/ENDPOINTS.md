# Endpoints

Contrato que lee el front. Se actualiza al cerrar cada UC. Las rutas van en inglés, igual que los campos y los códigos de error.

| UC | Método | Ruta | Descripción | Estado |
|---|---|---|---|---|
| UC-01 | GET | `/api/collaborators` | Listar colaboradores | hecho |
| UC-02 | POST/PUT/DELETE | `/api/collaborators[/{id}]` | Alta, edición, baja lógica | hecho |
| UC-03 | POST | `/api/shifts` | Alta de turno (409 si solapa) | hecho |
| UC-04 | GET/PUT/DELETE | `/api/shifts[/{id}]` | Listar con rango de fechas, leer, editar, borrar | hecho |
| UC-05 | GET | `/api/shifts` | Incluye marca de festivo | hecho |
| UC-06 | POST | `/api/shifts/import` | Importar `.xlsx` con informe | hecho |
| UC-07 | GET | `/api/shifts/export` | Exportar `.xlsx` por rango | pendiente |
| — | GET | `/health` | Health check (incluye la BD) | hecho |

Las rutas son provisionales hasta cerrar cada UC.

## UC-01 · GET /api/collaborators

Devuelve `200` con los colaboradores activos, ordenados por nombre y luego email. Sin parámetros. Ejemplos en [collaborators.http](collaborators.http).

```json
[
  { "id": "<guid>", "name": "Ana Pérez", "email": "ana.perez@example.com", "countryCode": "ES" }
]
```

## UC-02 · POST, PUT y DELETE /api/collaborators

Body de `POST` y `PUT`: `{ "name": "...", "email": "...", "countryCode": "ES" }`. El nombre se recorta, el email se guarda en minúsculas y el país en mayúsculas. Ejemplos en [collaborators.http](collaborators.http).

| Acción | Éxito | Errores |
|---|---|---|
| `POST /api/collaborators` | `201` con el colaborador y `Location: /api/collaborators/{id}` | `400`, `409` |
| `PUT /api/collaborators/{id}` | `200` con el colaborador | `400`, `404`, `409` |
| `DELETE /api/collaborators/{id}` | `204` (idempotente) | `404` |

Códigos de `errors` (formato en [ERROR-HANDLING.md](ERROR-HANDLING.md)):

| Código | HTTP | Cuándo |
|---|---|---|
| `collaborator.name` | 400 | Vacío o de más de 200 caracteres |
| `collaborator.email` | 400 | Dirección inválida |
| `collaborator.country` | 400 | No es un código ISO 3166-1 alfa-2 |
| `collaborator.country-unknown` | 400 | Nager.Date no lo soporta (si no responde, se acepta con el formato válido) |
| `collaborator.email-taken` | 409 | Otro colaborador, activo o dado de baja, usa el email |
| `collaborator.not-found` | 404 | El `id` no existe |

La baja es lógica (ADR-11): el colaborador deja de aparecer en el listado, conserva sus turnos y su email sigue ocupado. El `Location` del `POST` apunta a un recurso que aún no tiene `GET /{id}`.

## UC-03 · POST /api/shifts

Body: `{ "collaboratorId": "<guid>", "date": "2026-03-10", "startTime": "09:00", "endTime": "13:00" }`. Fecha `yyyy-MM-dd`; horas `HH:mm` o `HH:mm:ss` (en la respuesta salen como `HH:mm:ss`). Devuelve `201` con el turno y `Location: /api/shifts/{id}`. Ejemplos en [shifts.http](shifts.http).

Un colaborador no puede tener dos turnos que se solapen el mismo día. Los intervalos son semiabiertos: 09:00-13:00 y 13:00-17:00 no se solapan. El turno no cruza la medianoche: `endTime` debe ser posterior a `startTime`.

| Código | HTTP | Cuándo |
|---|---|---|
| `shift.date` | 400 | Falta la fecha |
| `shift.time-range` | 400 | `endTime` no es posterior a `startTime` |
| `shift.collaborator-unavailable` | 400 | El colaborador no existe o está dado de baja |
| `shift.overlap` | 409 | Choca con otro turno del colaborador; `conflictingShiftId` trae el id del turno con el que choca |

Un valor con formato inválido (por ejemplo `"startTime": "nope"`) lo rechaza el binder del framework con `400` antes del caso de uso, y la clave de `errors` es la ruta JSON (`$.startTime`), no un código de negocio.
## UC-04 · GET, PUT y DELETE /api/shifts

Ejemplos en [shifts.http](shifts.http). `PUT` recibe el mismo cuerpo que el alta y aplica las mismas reglas y los mismos errores de UC-03; el propio turno no cuenta como solape, así que dejarlo igual o ajustarlo dentro de su hueco es válido. Puede cambiar de colaborador, que debe existir y estar activo.

| Acción | Éxito | Errores |
|---|---|---|
| `GET /api/shifts?from=yyyy-MM-dd&to=yyyy-MM-dd` | `200` con la lista, ordenada por fecha y hora de inicio | `400` |
| `GET /api/shifts/{id}` | `200` con el turno | `404` |
| `PUT /api/shifts/{id}` | `200` con el turno | `400`, `404`, `409` |
| `DELETE /api/shifts/{id}` | `204` | `404` |

`from` y `to` son opcionales, se pueden usar por separado y ambos extremos se incluyen. Sin paginación. Los turnos de colaboradores dados de baja se siguen listando y leyendo, pero no se pueden editar. El borrado es físico, por lo que repetirlo da `404`.

| Código | HTTP | Cuándo |
|---|---|---|
| `shift.date-range` | 400 | `from` es posterior a `to` |
| `shift.not-found` | 404 | El `id` no existe |

Un `from` o `to` con formato inválido lo rechaza el binder con `400` y la clave de `errors` es el nombre del parámetro (`from`, `to`).

## UC-05 · Festivos en GET /api/shifts

Cada turno del listado lleva `holiday`, calculado con los festivos de Nager.Date del país del colaborador. Solo cambia `GET /api/shifts`; `POST`, `PUT` y `GET /api/shifts/{id}` devuelven el turno sin `holiday`. Ejemplos en [shifts.http](shifts.http).

```json
{ "id": "<guid>", "collaboratorId": "<guid>", "date": "2026-02-28", "startTime": "09:00:00", "endTime": "13:00:00",
  "holiday": { "status": "Holiday", "name": "Day of Andalucía", "isGlobal": false } }
```

| `holiday.status` | `name` | `isGlobal` | Significa |
|---|---|---|---|
| `None` | `null` | `null` | El día no es festivo en el país del colaborador |
| `Holiday` | nombre en inglés | `true` / `false` | Festivo nacional (`true`) o solo de algunas regiones (`false`) |
| `Unknown` | `null` | `null` | Nager.Date no respondió; el turno se lista igual |

- El colaborador solo tiene país, no región: un festivo regional se marca igual, con `isGlobal = false`. Si el mismo día hay uno nacional y uno regional, gana el nacional.
- Con Nager.Date caído el listado responde `200` con `Unknown` y tarda como máximo unos 3 s (timeout del cliente); el fallo no se guarda, así que la siguiente petición vuelve a intentarlo. Los aciertos se guardan 24 h por país y año.
- Los turnos de colaboradores dados de baja también se marcan.

## UC-06 · POST /api/shifts/import

Sube un `.xlsx` con turnos y devuelve un informe por fila. `multipart/form-data` con el archivo en el campo `file` (máximo 5 MB). Ejemplos en [shifts.http](shifts.http). [../samples/shifts-import-example.xlsx](../samples/shifts-import-example.xlsx) sirve de plantilla: seis turnos válidos, con la fecha y las horas como celdas de Excel.

**Formato del archivo**
- Se lee solo la primera hoja. La primera fila es el encabezado; los datos empiezan en la fila 2.
- Columnas obligatorias: `Email`, `Fecha`, `HoraInicio` y `HoraFin`. Se buscan por nombre, sin importar el orden ni las mayúsculas; las columnas extra se ignoran.
- `Fecha`: celda de fecha, o texto `yyyy-MM-dd` o `dd/MM/yyyy`. `HoraInicio` y `HoraFin`: celda de hora, o texto `HH:mm`.
- El colaborador se identifica por `Email` (sin distinguir mayúsculas ni espacios en los bordes) y debe estar activo.
- Las filas completamente vacías se ignoran.

**Respuesta `200`**, aunque haya filas fallidas. El import es parcial: las filas válidas se guardan y las inválidas se informan. Con la plantilla, la primera vez:

```json
{ "importedRows": [2, 3, 4, 5, 6, 7], "failedRows": [] }
```

Con filas inválidas (ejemplo ilustrativo):

```json
{
  "importedRows": [2, 3, 4, 8],
  "failedRows": [
    { "row": 5, "reasons": ["The shift overlaps an earlier row of the same file."] },
    { "row": 6, "reasons": ["Collaborator does not exist or is inactive."] },
    { "row": 7, "reasons": ["End time must be later than start time on the same day."] }
  ]
}
```

`row` es el número de fila de Excel. Cada fila fallida lista todos sus motivos (mensajes, sin código). Si dos filas del archivo se solapan para el mismo colaborador, falla la que viene segunda; el solape contra turnos ya guardados se comprueba igual.

**Errores `400`** (el archivo no se procesa y no se guarda nada):

| Código | Cuándo |
|---|---|
| `shift.import.file` | Falta el campo `file`, o el archivo no es un `.xlsx` legible |
| `shift.import.file-too-large` | El archivo pesa más de 5 MB |
| `shift.import.columns` | Faltan columnas obligatorias; el mensaje las nombra |

Un archivo muy por encima de los 5 MB (alrededor de 30 MB) lo corta el servidor con `413` antes de llegar a esta validación.
