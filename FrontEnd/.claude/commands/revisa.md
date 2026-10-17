---
description: Revisión crítica del código pendiente contra CLAUDE.md
---

Revisa $ARGUMENTS (si está vacío, los cambios sin commitear) contra `CLAUDE.md` y `.specs/architecture.md`.

Formato:
1. Lo que está bien (breve).
2. Violaciones de reglas (con la sección de `CLAUDE.md`).
3. Anti-patrones.
4. Riesgos: rendimiento (detección de cambios, listas sin `track`), fugas de suscripciones, seguridad (XSS por `innerHTML`/`bypassSecurityTrust*`, datos sensibles en `localStorage`).
5. Refactors propuestos, con código.

Si todo está bien, dilo; no inventes problemas.
