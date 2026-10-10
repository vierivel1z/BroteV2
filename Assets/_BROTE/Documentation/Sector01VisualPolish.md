# Sector_01 — preparación visual y limpieza

Proyecto: `D:/UnityGames/Brote/Brote`. Escena: `Assets/_BROTE/Scenes/Acopalca/Acopalca.unity`.

## Resultado real

Se ocultaron los diez SpriteRenderer geométricos de prueba. Se mantuvieron sus objetos, sprites originales, grupos y referencias de SectorVisualState; los umbrales 20/40/50 y el código del sistema no cambiaron. Los hijos se llaman ahora Pending_20_Shoots, Pending_40_Shrub y Pending_50_Grass/Flowers con sufijos individuales. Sus posiciones se distribuyeron asimétricamente en zonas para decoración futura, fuera del vivero y a más de 2,7 unidades de los centros de spots. No hay props definitivos colocados: estos son puntos de preparación invisibles.

El cambio visible en Game es la eliminación de las figuras geométricas que aparecían al restaurar. **El entorno natural y el camino definitivo siguen pendientes de arte compatible.** No se presenta la guía de composición como un camino ya pintado.

## Assets inspeccionados

Environment contiene únicamente seis PNG: Ground_Degraded_Tileset, Ground_Recovery, Ground_Restored, Nursery_Acopalca, PlantingSpots_Empty y Quenual_Growth. Los únicos 144 Tile assets son los tres bancos de Ground. No existen tiles de camino, Restoration_Props ni sprites separados de ichu, piedras, ramas, arbustos o flores.

Se conservaron el vivero y los diez PlantingSpots. No se movieron: mínimo de separación entre spots mayor de 3,1 unidades; todos están dentro del perímetro. El spot norte situado cerca del vivero está por encima de sus bounds visuales, sin solapamiento de sus rectángulos. La zona de recogida del vivero queda al sur de su cuerpo sólido; el recorrido propuesto sale por esa entrada. La comprobación física y visual en Play sigue pendiente.

No se reutilizaron etapas de queñual como arbustos o brotes decorativos: se reservaron para los árboles del sistema de plantación. Tampoco se usaron tiles opacos de Ground para fingir caminos o props, porque volverían a introducir bloques y contrastes.

## PathsTilemap y DecorationTilemap

Ambos siguen vacíos, a escala 1, con sus materiales actuales y sin colliders. Paths tiene orden −40 y Decoration −10 en Default, delante de Ground y detrás de WorldObjects. Su configuración ya era adecuada y se conservó. No se cambió ningún dato del Ground, Grid, importadores, PNG, slicing, generador, restauración, límites, cámara, prefabs, HUD o Animator.

Nuevo plan editable: `Assets/_BROTE/Settings/Sector01VisualLayout.json`. Define tres recorridos de ancho objetivo 1,5 unidades, doce zonas limpias y diez centros de composición para props dispersos. Los recorridos conectan entrada del vivero, centro y ramales hacia los principales spots del norte y sur. Las curvas se interpolan suavemente, sin pintar celdas. La futura decoración debe evitar el corredor de 1,5 unidades, los círculos limpios y el acceso al vivero; distribuir pocos elementos alrededor de los centros propuestos, con espacios vacíos y variación de orientación/distancia. Los centros no son puntos de llenado obligatorio.

Nueva herramienta **BROTE > Visuals > Sector 01 Composition Guide**:

- Dibuja recorridos curvos, zonas limpias y centros de composición **solo en Scene View** mientras está abierta.
- Permite seleccionar PathsTilemap o DecorationTilemap y encuadrar el sector.
- Cerrar la ventana retira la guía. No aparece en Game, no genera GameObjects ni pinta tiles.
- El archivo está en Editor y no añade lógica de juego ni coste de runtime.

## Assets exactos que faltan

1. **Restoration_Props.png**, o sprites equivalentes separados, con transparencia y estilo/paleta compatibles con el arte actual: ichu seco, piedra pequeña, rama y arbusto seco para Degraded; brotes, hierba y arbusto pequeño verdes para Recovery; hierba saludable, arbustos y flores para Restored. Deben ocupar aproximadamente 0,25–0,8 unidades los elementos pequeños, con pivote inferior; sin colliders. Conservar los assets existentes y ajustar únicamente el importador del nuevo pack.
2. **Path_Acopalca.png**, o un tileset de camino equivalente: overlays transparentes de tierra pisada, curvas, bordes irregulares, extremos y cruces, visualmente compatibles con Ground. Debe permitir una franja de 1–2 celdas sin convertir cada celda en un cuadrado opaco. Slices que ocupen 1×1 unidades; por ejemplo 181×181 px a PPU 181 para el nuevo atlas, sin modificar los importadores actuales.

## Revisión manual en Unity

Abrir la raíz indicada, recargar Acopalca desde disco conservando previamente cualquier cambio sin guardar, y esperar importación/compilación. Abrir la Composition Guide para revisar las curvas y espacios limpios en Scene. En Game, comprobar que el estado inicial, el vivero y los diez spots se mantienen y que los umbrales 20/40/50 ya no muestran figuras geométricas. Probar recorrido y 0→100%; no se ha ejecutado esta prueba en esta sesión. El intento previo de Unity batch falló por falta de licencia para ese modo; no se volvió a presentar como validación en Play.

Cuando se importen props definitivos, sustituir el Sprite de cada objeto Pending_20/40/50 y **volver a activar su SpriteRenderer**. Mantener los tres grupos de umbral y sus referencias. Pintar decoración pequeña en DecorationTilemap con densidad baja y sin colliders, siguiendo el plan; pintar el recorrido en PathsTilemap solo cuando existan sus tiles adecuados.

## Validación y siguiente tarea

Compilación externa de runtime y Editor: cero errores, incluidas las APIs de la herramienta nueva. Comparación del snapshot: únicamente treinta documentos de escena de los diez hijos geométricos cambiaron (nombre, posición y enabled del renderer). Los demás documentos, incluidos GroundTilemap, PathsTilemap, DecorationTilemap, NurseryArea, spots, SectorBounds y referencias de umbral, permanecen iguales. Se verificaron 479 archivos previos sin modificaciones; no se añadieron GameObjects ni colliders. Evidencia y backup en `.brote-work/visual-polish/`.

**Una siguiente tarea:** importar un pack Restoration_Props compatible y usarlo para sustituir los diez puntos Pending_20/40/50, cerrando la presentación de la recuperación sin cambiar su lógica.
