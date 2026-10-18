# Dominio — índice de entidades

Las entidades **emergen de los casos de uso**; aquí solo se indica qué spec define cada una. Se agregan al implementarlas.

| Entidad / VO | Definida en | Estado |
|---|---|---|
| Collaborator (`Domain/Collaborators/`) | UC-01 (creación e invariantes); UC-02 (unicidad, edición, baja) | creación, edición y baja lógica en el dominio (ADR-11); unicidad en Application |
| Turno (`Shift`, `Domain/Shifts/`) | UC-03 (alta y regla de solape); UC-04 (edición, borrado y consultas) | creación y regla de solape en el dominio; la comprobación contra los turnos existentes va en Application |
| Festivo (modelo de lectura, no persistido) | UC-05 | por definir |
| Informe de importación (modelo de aplicación) | UC-06 | por definir |
