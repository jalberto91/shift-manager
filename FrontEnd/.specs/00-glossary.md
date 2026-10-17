# Glosario

| Término | Significado |
|---|---|
| Colaborador | Persona que trabaja turnos. Tiene nombre, email único y país. |
| Baja | Desactivación lógica de un colaborador: deja de aparecer en el listado y conserva sus turnos. |
| Turno | Franja de trabajo de un colaborador: fecha + hora de inicio + hora de fin, en hora local del colaborador. |
| Solape | Dos turnos del mismo colaborador, el mismo día, con `inicioA < finB && inicioB < finA`. Tocarse en el borde no es solape. |
| Festivo | Día feriado público del país del colaborador, según Nager.Date. |
| Festivo desconocido | Estado cuando Nager.Date no respondió: el listado sigue funcionando y la marca se muestra como desconocida. |
| Código de negocio | Identificador estable de un error en `errors` del Problem Details, p. ej. `shift.overlap`. En inglés: es contrato de la API. |
| Problem Details | Formato de error RFC 7807 que devuelve el backend, siempre con `traceId`. |
| traceId | Identificador de la petición fallida; se muestra en los errores 500 para soporte. |
| Informe de importación | Resultado del import: filas importadas y fallidas, con número de fila y motivo. |
| Modelo de vista | Forma de un dato pensada para la UI, mapeada desde el DTO del backend. |
| Store | Servicio de feature que guarda el estado de la pantalla en signals (`loading`, `error`, `data`). |
| Página / UI | Página: componente contenedor ruteado. UI: componente presentacional con `input()`/`output()`. |
