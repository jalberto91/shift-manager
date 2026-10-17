# Contrato de errores

Formato: RFC 7807 Problem Details, siempre con `traceId` (ADR-04, ADR-10).

- `title` es el nombre estándar del status HTTP, en inglés (`Bad Request`, `Not Found`, `Conflict`). Es una etiqueta técnica, no texto de pantalla.
- Las claves de `errors` son códigos de negocio estables en inglés (contrato): el front se guía por ellas.
- `detail` y los mensajes dentro de `errors` van en inglés, como el resto del código. El front muestra sus propios textos según el código de `errors`.
- Una respuesta puede traer varios errores en `errors`; el primero decide el status y el `detail`.

| Situación | HTTP | Notas |
|---|---|---|
| Validación / regla de negocio | 400 | `errors` keyed por código estable, p. ej. `collaborator.email` |
| Recurso inexistente | 404 | |
| Conflicto de estado (solape de turnos, email ya usado) | 409 | Código `shift.overlap` (con `conflictingShiftId`) / `collaborator.email-taken` |
| Error inesperado | 500 | Mensaje genérico; el detalle solo va a los logs |

Respuestas reales verificadas (los mensajes son de ejemplo):

```json
// 409
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "The shift overlaps another one",
  "errors": { "shift.overlap": ["The shift overlaps another one"] },
  "traceId": "7a8c2ee0b5f2481c8b4528007e920ad9",
  "conflictingShiftId": "00000000-0000-0000-0000-000000000000"
}

// 400 con varios errores
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Name is required",
  "errors": {
    "collaborator.name": ["Name is required"],
    "collaborator.email": ["Invalid email"]
  },
  "traceId": "b67675ebba89d7f2c7ae39f192a124c1"
}

// 500: sin detalle interno
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500,
  "traceId": "de08b5631b56fc8f51d72e05bb3ff549"
}
```

Rutas inexistentes devuelven `404` con `application/problem+json` y `traceId`.
