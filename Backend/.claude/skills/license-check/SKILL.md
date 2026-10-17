---
name: license-check
description: Verifica la licencia vigente de un paquete NuGet antes de añadirlo. Úsalo siempre antes de instalar cualquier dependencia.
---

# license-check

Antes de añadir **cualquier** NuGet:
1. Consultar la licencia **actual** en nuget.org y en el repositorio del proyecto (la licencia puede cambiar entre versiones; revisar la versión exacta a instalar).
2. Confirmar que sea OSI/gratuita y compatible con .NET 9 (versión del paquete que soporte `net9.0`).
3. Rechazar automáticamente: MediatR, AutoMapper, MassTransit, FluentAssertions v8+, EPPlus.
4. Reportar una tabla:

| Paquete | Versión | Licencia | Veredicto |
|---|---|---|---|

5. Si hay duda, decirlo; **no asumir**. Mostrar la tabla y esperar OK antes de instalar.
