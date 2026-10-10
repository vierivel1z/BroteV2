> HISTORICAL SNAPSHOT: prior project state. Current implementation and validation are in STATUS.md, Architecture.md and ValidationReport.md. Preserve this document as history.

# BROTE — auditoría del estado real, 4 de octubre de 2026

Proyecto inspeccionado y modificado: `D:/UnityGames/Brote/Brote`. Escena: `Assets/_BROTE/Scenes/Acopalca/Acopalca.unity`.

## Alcance y grado de verificación

Se leyeron los 13 scripts de runtime, los dos scripts de Editor, la escena serializada, los tres prefabs de juego, la paleta, el Animator, nueve clips, 144 Tile assets, los PNG y sus importadores, las referencias de GUID/fileID y la configuración de proyecto/build. También se revisaron las carpetas vacías y los documentos existentes. Acopalca está habilitada en Build Settings; SampleScene está deshabilitada. Fuera de `_BROTE` hay assets de plantilla URP y Welcome.

`[FUNCIONA]` en los sistemas de juego significa que su implementación y conexiones actuales están completas según código y datos serializados. **Esta sesión no comprobó movimiento, entrada E, colisiones, cámara, HUD ni crecimiento dentro de Play.** La selección de terreno sí se ejecutó directamente desde el ensamblado C# compilado y se validó externamente.

No hay Unity MCP disponible. Se intentó abrir el proyecto con Unity **6000.6.3f1** en batch: salió con código 198 por falta de licencia/entitlement `com.unity.editor.headless`, antes de ejecutar la auditoría en Unity. Los resultados históricos de otros documentos no se presentan como pruebas actuales. La compilación externa de runtime y Editor contra las DLL instaladas sí terminó con **cero errores**. Persisten advertencias de campos serializados y APIs de diagnóstico obsoletas.

Existe otra copia completa en `D:/UnityGames/Brote/Brote/Brote`, con su propio `.git`, Assets y ProjectSettings. No se modificó esa copia. La raíz solicitada no tiene repositorio Git propio; se conservaron snapshots y evidencia en `.brote-work/audit-2026-10-04/`. Abrir en Unity la raíz indicada arriba para observar estos cambios.

## Estado por sistema

| Sistema | Estado | Evidencia actual y límites |
| --- | --- | --- |
| Player: movimiento | [FUNCIONA] | Suyay tiene PlayerMovement, Animator y Rigidbody2D dinámico; velocidad 4 u/s, gravedad 0, rotación bloqueada. WASD/flechas mediante Input System, diagonal normalizada y MovePosition en FixedUpdate. BoxCollider2D sólido 0,55×0,8. No hay límites de movimiento. |
| Player: animaciones | [FUNCIONA] | Suyay.controller asignado; ocho clips Idle/Walk para cuatro direcciones con sprites válidos. BlendTrees Idle/Walk conectados mediante IsMoving y parámetros direccionales. Idle frontal tiene dos claves y loop; Walk frontal cuatro claves y loop. Los ocho campos de clips de PlayerMovement están vacíos, pero ese script no los usa: el Animator controla los clips. |
| Player: interacción | [FUNCIONA] | PlayerInteraction conectado; radio 1,5, LayerMask Default, incluye triggers y selecciona el IInteractable disponible más cercano. E consulta Keyboard.current. Falta prueba real de teclado. |
| Player: inventario | [FUNCIONA] | SeedlingInventory: empieza en 0, capacidad 5, eventos, suma limitada y consumo de una unidad. Es inventario de plántulas; no existe inventario general ni persistencia. |
| Player/árbol/vivero: Y-Sorting | [FUNCIONA] | Suyay usa SpriteRenderer y YSort2D. Queñual y vivero usan SortingGroup + YSort2D en sus raíces. Orden = −Y×100. Capa WorldObjects; suelo en Ground, spots en Default. Pivotes de Suyay y etapas de queñual inferiores. Verificación delante/detrás en Play pendiente. |
| Plantación | [FUNCIONA] | **Diez** PlantingSpot_01…10, instancias del prefab PlantingSpot. Cada instancia tiene manager e inventario mediante overrides. Trigger circular, punto de plantación propio y prefab de queñual asignado. Sin plántula no planta; consume una, bloquea el spot y evita duplicados. |
| Prefab PlantingSpot | [FUNCIONA] | Referencias al queñual y a su Transform válidas. Manager e inventario nulos en el prefab son intencionales: pertenecen a la escena y están asignados en las diez instancias. Seis sprites de spot disponibles. |
| Vivero | [FUNCIONA] | NurseryArea en (−8,2; 0), con instancia Nursery_Acopalca del asset `Nursery_Acopalpa.prefab`. NurserySeedlingSource en hijo Interaction a Y −0,65; trigger radio 0,5 y offset Y 0,25. Tres BoxCollider2D sólidos separados de la interacción: cuerpo y laterales. Recoge una unidad por pulsación, sin límite de stock, respetando capacidad del jugador. Arte real Nursery_Acopalca_0 asignado; no es un rectángulo placeholder. Accesibilidad y acabado visual necesitan Play. |
| Queñual | [PARCIAL] | QuenualPlaceholder.prefab conserva ese nombre, pero **ya usa tres sprites reales de Quenual_Growth.png**. Etapas plántula/joven/maduro, 5 s + 5 s, coroutine iniciada por Initialize; al madurar aporta 10 puntos una sola vez y emite Matured. Es progresión automática de demo, sin cuidados, guardado ni ciclo más amplio. |
| RestorationManager | [FUNCIONA] | Uno en Sector_01; máximo 100. Cada maduro suma 10, por lo que diez spots permiten 100%. Evento RestorationChanged alimenta HUD, terreno y SectorVisualState. Empieza en cero. |
| GroundRestorationController | [FUNCIONA] | GroundTilemap, manager, sectorRoot y tres bancos de 48 tiles están asignados. Recuperación local por eventos de árboles maduros; radio inicial 2,6 celdas y expansión adicional 4,5. Recovery aparece desde el primer maduro; Restored requiere ≥40%. Transiciones monotónicas; el porcentaje global no implica que todo el terreno esté restaurado. |
| SectorVisualState | [PLACEHOLDER] | Conectado a grupos acumulativos de 20%, 40% y 50%. Sus diez objetos usan Seedling/Young/Developed geométricos de Art/Placeholders. No son referencias rotas; sí son arte pendiente. |
| GroundTilemap | [FUNCIONA] | 40×30 = **1.200 celdas**, X −20…19, Y −15…14. Grid 1×1, escala 1. Estado inicial Degraded. Ground, orden −50, modo Chunk, sin collider; Global Light incluye Ground/Default/WorldObjects. |
| Tilesets | [FUNCIONA] | Degraded/Recovery/Restored: 48 sprites y 48 Tile assets cada uno; referencias válidas, sin collider. Atlas 1448×1086, slices 181×181, PPU 181. Importación existente Bilinear, Clamp, sin mipmaps ni compresión. |
| Generación | [FUNCIONA] | Editor BroteGroundGenerator con Generate/Clear/Regenerate y nuevo Repaint Existing Ground (Keep Objects). La selección compartida se ejecutó externamente. Generate/Regenerate todavía regeneran posiciones y dejan diez spots: para mejorar solo el suelo usar **Repaint**. No se ejecutó Generate en esta sesión. |
| Paths/Water/Cliffs | [PENDIENTE] | Tilemaps existentes pero vacíos. Actualmente no hay rutas principales pintadas que proteger. |
| Decoración | [PENDIENTE] | DecorationTilemap existe, está vacío y no tiene collider. Restoration_Props no está importado. No se inventaron props ni se añadieron colliders/decoración masiva. |
| UI | [PARCIAL] | BroteHUD construye UI Toolkit en runtime: Salud del Valle, barra/porcentaje, contador 0…5 y prompt contextual. Referencias al jugador, manager e inventario asignadas. PanelSettings nulo es válido: crea uno en runtime. Es HUD mínimo; no hay menús ni presentación final. Textos mal codificados corregidos. |
| Cámara | [PARCIAL] | CameraFollow2D apunta a Suyay, suavizado 0,18, ortográfica tamaño 5, límites del centro X ±11 / Y ±10. A 16:9 cubre el suelo con margen horizontal pequeño. No calcula límites según aspect ratio; Suyay puede salir del terreno y del encuadre. |

## Errores, faltantes y componentes revisados

- **Cero errores de compilación externa**, cero GUID ausentes, cero referencias locales o subassets externos inválidos detectados. No hay componentes con `m_Script: {fileID: 0}` ni SpriteRenderer con sprite nulo en escena/prefabs inspeccionados. Los sprites Single con fileID 21300000 fueron comprobados como válidos, aunque ese ID no figure literalmente en su meta.
- Los tres bancos de restauración tienen 48 referencias válidas. Los 144 Tiles tienen sprite y ColliderType None. Prefabs de plantación, queñual y vivero tienen referencias resolubles.
- No se detectaron scripts de contenido duplicado, managers/jugador/HUD duplicados ni spots repetidos por nombre. Los Placeholder_01… son hijos diferentes de tres grupos distintos. `Vivero` es una raíz organizativa; la fuente interactiva efectiva está en NurseryArea.
- **Problema jugable:** PlayerMovement carece de límites y no hay colliders de perímetro. La cámara limitada no evita que Suyay se aleje del suelo finito.
- **Problema visual:** diez objetos geométricos reaparecen al alcanzar 20/40/50%; no representan la decoración final.
- **Corregido:** textos `queÃ±ual` / `plÃ¡ntula` / `PlÃ¡ntulas` en PlantingSpot y BroteHUD.
- **Limpieza pendiente de bajo impacto:** ocho campos de AnimationClip no usados en PlayerMovement; clip extra `SuyayWalkBackAnim.anim` sin referencias serializadas dentro de `_BROTE`; estados direccionales antiguos sin transiciones de entrada en el Animator. Se conservaron para evitar limpieza innecesaria.
- Documentación histórica contradictoria: GroundTilemap.md describe 3.072 celdas y cinco spots; VerticalSlice.md describe el prototipo inicial. El estado actual tiene 1.200 celdas y diez spots. Este informe es la referencia actual.
- No se observó un error de Sorting Layer ni un collider en terreno/decoración pequeña. El collider del jugador y la accesibilidad del vivero requieren prueba física/visual en el Editor; una inspección de YAML no demuestra su comportamiento completo.

## Diagnóstico visual del terreno

**La causa principal es la selección y el arte, no un error de encaje o PPU.** Un slice de 181 px / PPU 181 ocupa 1×1 unidades y coincide con Grid y transforms de escala 1. Los tilesets alternan variantes claras y oscuras, con grietas, rocas y vegetación ya dibujadas dentro de cada tile. Hay líneas/bordes visibles en el atlas; las variantes no forman un conjunto seamless uniforme.

La selección anterior usaba BASE/VARIATION/SPECIAL con 70/25/5, pero BASE incluía mezclas luminosas incompatibles (por ejemplo, Degraded 10 ≈0,313 y 21 ≈0,373; otros detalles rondan 0,49). La lista no estaba ordenada por tono. Combinaba Perlin a escala 0,12 con un 16% de detalle aleatorio; cambiar de categoría por celda podía introducir una variante mucho más clara. Eso explica el tablero incluso donde ya había ruido coherente.

La luminosidad media medida sobre los slices Degraded completos va de **0,292 a 0,558**. PPU/escala no resuelven bordes o cambios de paleta. Se mantuvieron PNG, importadores, slicing y escalas. Bilinear puede suavizar muestras, pero no une bordes dibujados incompatibles.

## Mejora aplicada y cómo ajustarla

1. `GroundTileSelection.Profile`, dentro de GroundRestorationController, permite configurar categorías por estado, peso BASE (0,80), VARIATION (0,17), tamaño de región (2…4, por defecto 3) y tolerancia tonal (0,08). SPECIAL usa el peso restante (0,03).
2. Se reclasificaron variantes de **tonalidad oscura compatible**. BASE Degraded: 3,10,26,35,40,42; VARIATION: 1,17,33,44; SPECIAL: 5,12,23,39. Recovery/Restored tienen sus propias listas configurables.
3. Ruido coherente de valores interpolados suavemente entre anclas cada tres celdas; variación del candidato estable por región. La categoría conserva aleatoriedad dispersa para evitar bloques densos de rocas/vegetación. La luminosidad medida filtra candidatos demasiado alejados del tono objetivo y permite fallback BASE. No es un autotile ni garantiza seamless.
4. Zonas BASE alrededor de spots (radio 1,75 celdas), vivero (bounds visuales/interacción con margen de 1 unidad) y celdas de PathsTilemap con un margen de una celda cuando haya rutas pintadas. Esta escena protege **171 celdas**. Mismo criterio al restaurar durante Play.
5. Nuevo menú **BROTE > Ground > Repaint Existing Ground (Keep Objects)**: repinta únicamente celdas existentes de los tres bancos, conservando su estado y los objetos. No reimporta imágenes ni mueve spots. Guardar escena después de usar el menú.
6. Se repintó el Ground guardado con la salida del **selector C# de producción compilado y ejecutado**, manteniendo 1.200 celdas. Solo cambió el bloque serializado GroundTilemap de la escena; los transforms, referencias y todos los otros objetos permanecen idénticos al snapshot.

| Medida del Ground guardado | Antes | Después |
| --- | ---: | ---: |
| Diferencia media de luminosidad entre vecinos horizontales/verticales | 0,05168 | 0,00278 |
| Diferencia máxima | 0,1937 | 0,0229 |
| Pares vecinos con diferencia >0,10 | 502 | 0 |
| Distribución total BASE / VARIATION / SPECIAL | selección anterior | 1.005 / 173 / 22 |
| Celdas protegidas con tile no BASE | sin protección específica | 0 |

Fuera de las zonas protegidas la distribución obtenida es **81,05% / 16,81% / 2,14%**, aproximación finita determinista al 80/17/3 solicitado. En el mapa completo es 83,75% / 14,42% / 1,83% porque las zonas limpias fuerzan BASE. La diferencia media de luminosidad baja aproximadamente **94,6%**. Los bordes dibujados y motivos repetidos siguen siendo visibles: la mejora no convierte el atlas en seamless. La imagen comparativa es una composición de los datos de tiles, **no una captura de Unity**.

## Escala comprobada

Dimensiones de rectángulos importados, a escala 1; el contenido opaco puede ocupar menos que su rectángulo.

| Objeto | Slice en px | PPU | Tamaño en unidades | Recomendación |
| --- | --- | ---: | --- | --- |
| Ground, los tres estados | 181×181 | 181 | 1×1 | Conservar; el encaje es correcto. |
| Suyay, Walk Front | 543×724 | 512 | 1,061×1,414 | Conservar como referencia. |
| PlantingSpot | 512×512 | 400 | 1,28×1,28 | Conservar; revisar huella opaca en Play. |
| Queñual plántula | 180×221 | 256 | 0,703×0,863 | Conservar. |
| Queñual joven | 486×530 | 256 | 1,898×2,070 | Conservar. |
| Queñual maduro | 834×695 | 256 | 3,258×2,715 | Conservar; comprobar solapamiento de copas entre spots. |
| Vivero, sprite principal | 1509×1024 | 200 | 7,545×5,12 | Conservar; su tamaño corresponde a un edificio. |

No hay una inconsistencia de escala que justifique un cambio automático. Las rocas/grietas dibujadas dentro de un tile de 1 unidad sí pueden leerse grandes respecto a Suyay; se redujo su presencia mediante selección, sin reducir Grid ni cambiar PPU. Los cinco fragmentos pequeños extra del slicing del vivero no están usados por el prefab principal.

## Pendientes y priorización

**PRIORIDAD 1 — bloqueadores reales para cerrar la demo.** Delimitar el movimiento para impedir salir del Ground/encuadre. Validar importación, WASD/E, entrada al vivero, plantación, HUD y 0→100% en un Editor con licencia utilizable. No hay un bloqueo de compilación confirmado en el código final; la falta de licencia bloqueó esta validación automática. Usar la raíz correcta para evitar abrir la copia anidada anterior.

**PRIORIDAD 2 — terminar visualmente Sector_01.** Sustituir los diez placeholders de SectorVisualState por props definitivos; incorporar Restoration_Props y decoración dispersa sin colliders; pintar rutas principales en PathsTilemap. Ajustar composición y revisar claramente los tres estados en Play. El borde de los tiles requiere arte seamless en una etapa futura si todavía resulta molesto.

**PRIORIDAD 3 — desarrollo pendiente.** Guardado/carga del progreso no existe. Resources, Automation, World, Core, Utilities y ScriptableObjects/Plants, Sectors y Upgrades son estructura de carpetas, no sistemas implementados; Audio/NPC tampoco tienen contenido funcional propio. No se encontró un plan de diseño original completo para asignar mecánicas específicas a esas carpetas. Definir el alcance antes de implementar; no se añadieron nuevas mecánicas.

**PRIORIDAD 4 — polish opcional.** UI definitiva, feedback al llenar inventario, audio, limpieza de estados/clips antiguos, revisión del collider/pivotes y límites de cámara según aspect ratio. Consolidar la documentación histórica y resolver organización de las dos copias del proyecto con conservación de sus datos.

**SIGUIENTE TAREA RECOMENDADA:** implementar un perímetro jugable de Sector_01 que impida que Suyay salga del Ground de 40×30, y validarlo recorriendo los cuatro bordes en Play.

## Evidencia y archivos modificados

Cambios: `Editor/BroteGroundGenerator.cs`, `Scripts/Environment/GroundRestorationController.cs`, `Scripts/Plants/PlantingSpot.cs`, `Scripts/UI/BroteHUD.cs`, bloque GroundTilemap de Acopalca, este informe y notas de vigencia en documentos históricos. No se modificó ningún PNG, meta de arte, Tile asset, prefab, Animator, clip, Package ni ProjectSettings.

Evidencia en `.brote-work/audit-2026-10-04/`: snapshot `Acopalca.before.unity`, scripts anteriores del generador/controlador, `static-audit.json`, `final-verification.json`, `audit-runtime-compile.txt`, `audit-editor-compile.txt`, `selector-validation.txt`, `selection.csv`, `layout-metrics.json`, `ground-before-after.png`, hashes de los 17 PNG y log del intento de Unity. El harness temporal de Play fue retirado de Assets; no se deja código automático de auditoría ejecutándose al abrir el proyecto.

Validación externa: selector determinista, 3.600 selecciones de las celdas de la escena en tres estados, 360.000 muestras adicionales con tamaños 2/3/4, índices dentro de banco, coordenadas negativas, fallback de categorías inválidas/nulas y zonas protegidas BASE. Comprobados refcounts de las tablas serializadas, referencias y conservación del resto de la escena; 17 PNG idénticos byte a byte. Esto no sustituye una prueba de importación y Play dentro de Unity.
