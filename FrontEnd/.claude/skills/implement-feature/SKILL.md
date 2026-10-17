---
name: implement-feature
description: Implementa una pantalla (FE-NN) como rebanada vertical de front data-access → ui → pages → rutas → tests. Úsalo cuando se pida construir o continuar una feature del front.
---

# implement-feature

Unidad de trabajo habitual: una **rebanada vertical de front**. El ejemplo trabajado a copiar será FE-01 (`.specs/features/FE-01-colaboradores.md`) cuando exista; referenciarlo aquí al cerrarlo.

## Paso 0 · Tarea 0 (siempre primero): confirmar el contrato real
1. Leer el spec `FE-NN` y el **UC del backend** que consume (`../Backend/.specs/use-cases/UC-NN-*.md`), más `../Backend/docs/api/ENDPOINTS.md` y `ERROR-HANDLING.md`.
2. Confirmar el contrato **real** con OpenAPI (`http://localhost:5080/scalar/v1`) o un `curl` al endpoint corriendo, no el que suponemos. Anotar en el spec cada supuesto corregido.
3. Si el endpoint no existe o no sirve: anotarlo en "Pendientes con el backend" de `ESTADO-ACTUAL.md` y decidir si se mockea temporalmente (marcándolo). No se toca `../Backend`.
4. Redactar/actualizar `FE-NN-nombre.md` desde `_TEMPLATE-FE.md`.

## Paso 1 · Capas
- **data-access**: tipos del DTO (`interface`/`type`) + `*.api.ts` con `HttpClient` + `*.store.ts` con signals (`loading`, `error`, `data`, `computed`). Mapeo a modelo de vista si la forma no sirve a la UI.
- **ui**: componentes presentacionales con `input()`/`output()`, sin inyectar servicios de datos.
- **pages**: contenedor ruteado que conecta store y ui.
- **rutas**: `<feature>.routes.ts` lazy + entrada en el menú del shell.

## Paso 2 · Reglas transversales
`license-check` antes de cualquier paquete npm · OnPush · sin `any` · los **cuatro estados** (cargando, vacío, error, con datos) siempre cubiertos · errores de negocio mapeados por **código** a un texto propio en español (ADR-04) · comentarios solo con porqué.

## Paso 3 · Tests
Unit tests del store y de la lógica de mapeo (el grueso, incluido el mapeo de errores de negocio); tests de componente solo para lo que tenga lógica de presentación (p. ej. formulario con error de solape). Verificación manual con el skill `verify`. Calidad sobre cantidad.

## Paso 4 · Cierre (checklist)
- [ ] `ng build` sin warnings
- [ ] `ng lint` limpio
- [ ] Tests verdes
- [ ] Verificado contra el backend real (skill `verify`)
- [ ] ¿ADR nuevo?
- [ ] Spec con Tasks marcadas y sección "Para la revisión"
- [ ] `ESTADO-ACTUAL.md` actualizado
- [ ] Pendientes con el backend anotados; TBDs anotados
- [ ] **No commitear**: sugerir el mensaje en inglés y parar para revisión

## Anti-patrones
1. Lógica de negocio en componentes de `ui/`.
2. `HttpClient` en un componente.
3. Suscripciones manuales sin limpieza.
4. `any`.
5. Un servicio "god" compartido por todas las features.
6. Duplicar en el front una regla que valida el backend.
7. `NgModule` nuevo.
8. `setTimeout` para arreglar detección de cambios.
9. Mostrar mensajes del backend tal cual.
