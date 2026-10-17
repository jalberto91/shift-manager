---
name: license-check
description: Verifica la licencia vigente de un paquete npm antes de instalarlo. Úsalo siempre antes de añadir cualquier dependencia.
---

# license-check

Antes de añadir **cualquier** paquete npm:
1. Consultar la licencia **actual** en npm y en el repositorio del proyecto (puede cambiar entre versiones; revisar la versión exacta a instalar).
2. Confirmar que sea OSI/gratuita para uso libre.
3. Revisar mantenimiento: fecha del último release e issues abiertos relevantes.
4. Confirmar compatibilidad con la versión de Angular (`peerDependencies`).
5. Reportar una tabla:

| Paquete | Versión | Licencia | Último release | Compatible con Angular | Veredicto |
|---|---|---|---|---|---|

6. Si hay duda, decirlo; **no asumir**. Mostrar la tabla y esperar OK antes de instalar. Incluir dependencias transitivas relevantes (p. ej. las de PrimeNG).
