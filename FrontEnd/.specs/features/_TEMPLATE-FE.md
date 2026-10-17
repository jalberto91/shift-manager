# FE-NN · Nombre
> Estado · rama · fecha · consume: UC-NN del backend (`../Backend/.specs/use-cases/UC-NN-*.md`) · cubre: R# y decisiones P# de `.specs/00-requisitos.md`

## Requirements
Qué ve y hace el usuario. Criterios de aceptación verificables y casos límite.

## Design
- Rutas, pantallas y componentes (página / ui)
- Endpoints que consume y su contrato real (confirmado con `curl` u OpenAPI)
- Modelo de vista (y mapeo desde el DTO)
- Estados de la pantalla: cargando / vacío / error / con datos
- Errores de negocio por código → dónde se muestran y con qué texto
- Supuestos corregidos (cada uno se anota)

## Tasks
- [ ] data-access (DTOs, `*.api.ts`, store)
- [ ] ui
- [ ] pages
- [ ] rutas y entrada en el menú
- [ ] tests
- [ ] docs

## Verificación
Recorrido manual contra el backend real: pasos y qué se vio.

## TBDs
Dudas numeradas (incluidas las que hay que llevarle al backend). No bloquean.

## Para la revisión
3-5 bullets: decisiones y su porqué, para defenderlas en vivo.
