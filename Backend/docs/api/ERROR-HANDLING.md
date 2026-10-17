# Contrato de errores

Formato: RFC 7807 Problem Details, siempre con `traceId`. Se completa en la Fase 2 al implementar el middleware (ADR-04, ADR-10).

| Situación | HTTP | Notas |
|---|---|---|
| Validación / regla de negocio | 400 | `errors` keyed por código estable, p. ej. `colaborador.email` |
| Recurso inexistente | 404 | |
| Conflicto de estado (solape de turnos, borrar colaborador con turnos) | 409 | Código `turno.solape` / `colaborador.conTurnos` |
| Error inesperado | 500 | Mensaje genérico; el detalle solo va a los logs |

Ejemplo (a confirmar con respuestas reales):

```json
{
  "type": "https://httpstatuses.io/409",
  "title": "Conflicto",
  "status": 409,
  "detail": "El turno se solapa con otro del mismo colaborador.",
  "traceId": "00-...",
  "errors": { "turno.solape": ["..."] }
}
```
