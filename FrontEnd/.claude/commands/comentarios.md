---
description: Limpia comentarios del código sin commitear, sin cambiar comportamiento
---

Recorre el código sin commitear ($ARGUMENTS si se indica un alcance). Para cada comentario:
- ¿Lo puede decir un nombre? → renombrar y borrar el comentario.
- ¿Repite el código? → borrar.
- ¿Explica un porqué que el código no puede? → queda, en una línea.

No agregues comentarios. Después corre `ng build` y los tests: **cero cambios de comportamiento**.
Reporta una tabla antes/después.
