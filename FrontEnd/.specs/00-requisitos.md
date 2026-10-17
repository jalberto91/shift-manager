# 00 · Requisitos del front

> Copia de lo que le toca al front del enunciado de la prueba técnica. Si algo choca con otro documento, **manda el enunciado**.

## 1. Objetivo
API REST en .NET 9 + front sencillo en Angular para gestionar turnos de trabajo de colaboradores de distintos países. Se evalúan buenas decisiones, código claro y alcance delimitado. Plazo 7 días, 6-8 h. Revisión de 30 min recorriendo el código, con preguntas y **un pequeño cambio en vivo**.

## 2. Requisitos que tocan al front

| # | Requisito |
|---|---|
| R1 | Colaboradores: CRUD con nombre, email y país. |
| R2 | Turnos: CRUD con colaborador, fecha, hora de inicio y hora de fin. |
| R3 | No se pueden crear ni modificar turnos que se solapen para el mismo colaborador. |
| R4 | Importación desde Excel (`.xlsx`) con informe de filas importadas y fallidas, con el motivo de cada error. |
| R5 | Exportación a Excel de los turnos filtrados por rango de fechas. |
| R6 | Señalar en el listado los turnos que caen en festivo del país del colaborador. |
| R8 | Todo se levanta con un único comando (`docker compose up`). |
| R10 | Pruebas automatizadas relevantes; calidad sobre cantidad. |
| R11 | README: cómo ejecutarlo, decisiones y qué quedó fuera. |

**Front (literal):** basta con listado de turnos, formulario de alta/edición y botones de importar y exportar. No invertir en diseño visual.

**Entrega**: E1 repositorio Git con historial legible · E2 librerías libres · E3 se puede usar IA pero se declara en el README (qué y para qué) · E4 ambigüedades resueltas con decisión razonable documentada.

**Criterios de valoración** (checklist final): que funcione y cumpla las reglas incluidos los casos límite · organización y claridad · calidad y sentido de las pruebas · autonomía (decisiones justificadas, alcance gestionado) · manejo de errores y de fallos de la API externa · facilidad para ejecutar · front funcional y limpio.

## 3. Pantallas

| FE | Pantalla | Consume | Cubre |
|---|---|---|---|
| FE-01 | Colaboradores: listado + alta/edición/baja | UC-01, UC-02 | R1 |
| FE-02 | Listado de turnos con filtro por fechas y marca de festivo | UC-04, UC-05 | R2, R6 |
| FE-03 | Formulario de alta/edición de turno (error de solape en pantalla) | UC-03, UC-04 | R2, R3 |
| FE-04 | Importar Excel y mostrar el informe por fila | UC-06 | R4 |
| FE-05 | Exportar a Excel por el rango filtrado | UC-07 | R5 |

Cada FE arranca cuando su UC está cerrado en el backend.

## 4. Decisiones que afectan al front

| # | Decisión |
|---|---|
| P2 | Los turnos que se tocan en el borde (09-13 y 13-17) no se solapan: intervalos `[inicio, fin)`. |
| P3 | No se admiten turnos que cruzan la medianoche: `HoraFin > HoraInicio` el mismo día. |
| P4 | País = código ISO 3166-1 alfa-2 (`ES`, `AR`, `BR`). |
| P5 | Email de colaborador único, sin distinguir mayúsculas. |
| P6 | Borrar un colaborador con turnos se bloquea con `409`. (Hoy la baja es lógica, ver contrato.) |
| P7 | Import parcial: se guardan las filas válidas y se informan las inválidas. |
| P9 | Excel: encabezados por nombre; fecha `yyyy-MM-dd` o `dd/MM/yyyy`, hora `HH:mm`; tamaño máximo p. ej. 5 MB. |
| P10 | Festivos de Nager.Date (globales y regionales, con nombre). Si la API falla: `holiday = unknown`, el listado sigue funcionando. Tres estados en UI: festivo (con nombre), no festivo, desconocido. |
| P11 | El front del enunciado no menciona colaboradores, pero un turno necesita uno: pantalla mínima de colaboradores y datos semilla. |
| P12 | Las horas son de reloj local del colaborador, sin conversiones; fechas como texto `yyyy-MM-dd`. |
| P13 | Sin paginación en v1; el filtro por fechas acota. |
| P15 | Librería de UI: PrimeNG, tema por defecto, sin Tailwind. |
| P16 | Idioma: UI, README, comentarios y docs en español; código en inglés. |
| P17 | Duración del turno: solo `fin > inicio`. |
