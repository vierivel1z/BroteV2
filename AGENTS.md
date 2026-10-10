# BROTE — Reglas para agentes

## Proyecto

Ruta principal:

D:/UnityGames/Brote/Brote

Escena principal:

Assets/_BROTE/Scenes/Acopalca/Acopalca.unity

BROTE es un videojuego 2D de restauración ecológica ambientado en
Acopalca, Huancayo, Junín.

El jugador controla a Suyay y restaura progresivamente un ecosistema
altoandino mediante vivero, plántulas y plantación de queñuales.

---

# OBJETIVO DE LOS AGENTES

Avanzar el proyecto de forma incremental y segura.

Priorizar:

1. estabilidad;
2. funcionalidad;
3. feedback satisfactorio;
4. presentación visual;
5. nuevas mecánicas.

No agregar sistemas grandes sin autorización.

---

# REGLA PRINCIPAL

ANTES de modificar algo:

1. inspeccionar el estado REAL actual;
2. leer TASKS.md;
3. leer DECISIONS.md;
4. leer STATUS.md;
5. no asumir que reportes antiguos siguen vigentes.

La escena y archivos actuales son la fuente de verdad.

---

# PUEDE HACER SIN PREGUNTAR

El agente puede:

- revisar scripts;
- corregir errores de compilación;
- resolver referencias Missing;
- crear componentes auxiliares pequeños;
- crear ParticleSystems;
- mejorar feedback visual;
- mejorar feedback del HUD;
- preparar prefabs;
- organizar archivos;
- documentar;
- hacer backups;
- ejecutar comprobaciones estáticas;
- limpiar código claramente no usado si es seguro;
- crear herramientas de Editor;
- realizar tareas marcadas SAFE en TASKS.md;
- actualizar STATUS.md;
- continuar con otra tarea si una queda bloqueada.

---

# DEBE PEDIR DECISIÓN

Detener esa tarea y marcarla BLOCKED si requiere:

- modificar una mecánica principal;
- cambiar el diseño del mapa;
- mover más de 3 objetos importantes;
- modificar PlayerMovement profundamente;
- cambiar PPU del arte existente;
- cambiar tamaño del Grid;
- regenerar completamente Sector_01;
- eliminar un sistema funcional;
- reemplazar arte oficial;
- tomar una decisión estética importante;
- crear una nueva mecánica no solicitada.

No detener todo el trabajo.

Si existe otra tarea independiente y segura, continuar con ella.

---

# NUNCA HACER AUTOMÁTICAMENTE

- borrar arte original;
- eliminar escenas sin backup;
- cambiar escala global del proyecto;
- modificar todos los sprites de Suyay;
- rehacer el GroundTilemap;
- reemplazar el sistema de restauración;
- cambiar el GDD;
- implementar mecánicas inventadas;
- descargar assets externos;
- sobrescribir manualmente archivos de arte sin conservar original.

---

# UNITY Y ESCENA

Cuando se modifica directamente:

- .unity
- prefab
- Animator
- Tilemap

hacer backup antes.

Si Unity está abierto y existe riesgo de conflicto con modificaciones
directas de YAML, solicitar que Unity sea cerrado antes de continuar.

Para cambios solamente en scripts .cs puede trabajarse normalmente.

---

# SISTEMAS QUE DEBEN PRESERVARSE

Actualmente existen y no deben reescribirse sin necesidad:

- PlayerMovement
- PlayerInteraction
- SeedlingInventory
- NurserySeedlingSource
- PlantingSpot
- QuenualGrowth
- RestorationManager
- GroundRestorationController
- SectorVisualState
- BroteHUD
- CameraFollow2D
- YSort2D
- SectorBounds
- ScenicBoundary

---

# FLUJO DE CADA TAREA

Para cada tarea:

1. inspeccionar;
2. hacer backup si corresponde;
3. implementar solo lo necesario;
4. compilar;
5. comprobar referencias;
6. realizar validaciones posibles;
7. actualizar STATUS.md;
8. cambiar estado de la tarea en TASKS.md;
9. continuar con la siguiente tarea SAFE.

---

# ARTE FALTANTE

Si una tarea necesita arte que no existe:

NO inventar un placeholder permanente.

Crear:

ART_REQUESTS/XXX_nombre.md

Incluir:

- nombre del asset;
- objetivo;
- estilo;
- elementos necesarios;
- transparencia;
- dimensiones aproximadas;
- ruta destino;
- cómo será usado en Unity.

Marcar la tarea como:

WAITING_ART

y continuar con otra tarea independiente.

---

# DOCUMENTACIÓN

STATUS.md debe representar siempre el estado actual.

No presentar como verificado algo que no se probó en Play.

Usar explícitamente:

VERIFIED
STATICALLY_VERIFIED
PLAY_REQUIRED
BLOCKED
WAITING_ART

---

# ALCANCE

Si no hay tareas SAFE disponibles:

detenerse.

No inventar trabajo solo para mantenerse ocupado.