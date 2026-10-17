---
name: implement-use-case
description: Implementa un caso de uso (UC-NN) como rebanada vertical Domain→Application→Infrastructure→Api→Tests. Úsalo cuando se pida construir o continuar un UC del backend.
---

# implement-use-case

Unidad de trabajo habitual: una **rebanada vertical**. El ejemplo trabajado a copiar será UC-01 (`.specs/use-cases/UC-01-listar-colaboradores.md`) cuando exista; referenciarlo aquí al cerrarlo.

## Paso 0 · Tarea 0 (siempre primero): entender la regla antes de codear
No hay legado. Entonces:
1. Releer los **R# del enunciado** que cubre el UC y las **decisiones P#** relevantes en `.specs/00-requisitos.md`, más lo que ya existe en el código.
2. **Reconciliar**: listar supuestos y casos límite; anotar en el spec cada supuesto corregido.
3. Si hay datos reales (semilla, `.xlsx` de ejemplo), mirarlos en solo lectura para confirmar nulos, formatos y volúmenes.
4. Redactar/actualizar `UC-NN-nombre.md` (desde `_TEMPLATE-UC.md`) y registrar entidades nuevas en `.specs/01-domain-spec.md`.

## Paso 1 · Capas
- **Domain**: entidad rica sin frameworks; `Result<T>` (con warnings no bloqueantes si aplica); reglas puras en `Domain/Services/`; value objects; errores centralizados por agregado (`XxxErrors.cs`: código estable + mensaje canónico).
- **Application**: `UseCases/<Nombre>/` con `Command`/`Query` + `Handler` + `Dto` (`sealed record`). Puertos en `Abstractions/` con **métodos del negocio**. No referencia Infrastructure.
- **Infrastructure**: `Persistence/Configurations/<Entidad>Configuration.cs`; repositorios que implementan los puertos; sin transacciones que crucen agregados sin razón.
- **Api**: controller **fino** (valida, despacha con `ISender`, devuelve DTO); errores según ADR-04 / ADR-10.

## Paso 2 · Reglas transversales
`license-check` antes de cualquier NuGet · idioma (código en inglés, comentarios/logs en español) · logs estructurados · `CancellationToken` · sin código muerto.

## Paso 3 · Tests
Unit tests = el grueso (dominio, invariantes, `Result<T>`, handlers con fakes). E2E real con el skill `verify`. Calidad sobre cantidad; los obligatorios están en `CLAUDE.md`/requisitos (solape y bordes, import fila a fila, degradación de Nager.Date).

## Paso 4 · Cierre (checklist)
- [ ] Build sin warnings
- [ ] Tests verdes
- [ ] E2E verificado (skill `verify`)
- [ ] ¿ADR nuevo?
- [ ] Spec con Tasks marcadas y sección "Para la revisión"
- [ ] `ESTADO-ACTUAL.md` actualizado
- [ ] `docs/api/ENDPOINTS.md` actualizado (**el front lo lee**)
- [ ] Archivo `.http` con request + respuestas **reales**
- [ ] TBDs anotados
- [ ] **No commitear**: sugerir el mensaje y parar para revisión

## Anti-patrones
1. Anemic domain disfrazado de DDD.
2. `IRepository<T>` genérico.
3. Entidad EF como DTO de la API.
4. `OnModelCreating` gigante con Fluent inline.
5. Eventos tipo `EntityUpdated` (hechos de negocio en pasado).
6. Excepciones para flujo de negocio.
