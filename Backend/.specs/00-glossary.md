# Glosario

| Término | Significado |
|---|---|
| Colaborador | Persona que trabaja turnos. Tiene nombre, email único y país. |
| Turno | Franja de trabajo de un colaborador: fecha + hora de inicio + hora de fin, en hora local del colaborador. |
| Solape | Dos turnos del mismo colaborador, el mismo día, con `inicioA < finB && inicioB < finA`. Tocarse en el borde (13:00 y 13:00) no es solape. |
| Intervalo semiabierto | `[inicio, fin)`: incluye el inicio, excluye el fin. |
| Festivo | Día feriado público del país del colaborador, según Nager.Date. |
| Festivo desconocido | Estado cuando Nager.Date no respondió: el listado sigue funcionando. |
| País | Código ISO 3166-1 alfa-2 (`ES`, `AR`, `BR`). |
| Informe de importación | Resultado del import: filas importadas y filas fallidas con su motivo (número de fila + código). |
| Código de negocio | Identificador estable de un error, p. ej. `shift.overlap`, `collaborator.email`. En inglés: son parte del contrato de la API. |
| Datos semilla | Colaboradores de varios países cargados al arrancar para que la app sea usable. |
