# BROTE: suelo degradado de Acopalca

> Documento histórico. Para el estado actual, la auditoría y los cambios del terreno del 4 de octubre de 2026, consultar [ProjectAudit-2026-10-04.md](ProjectAudit-2026-10-04.md).

## Resultado

Se reutilizó World/Grid/GroundTilemap en Assets/_BROTE/Scenes/Acopalca/Acopalca.unity. Tiene 3072 celdas pintadas: 64 × 48, desde (-32,-24) hasta (31,23). Cubre el inicio de Suyay, el vivero y los cinco spots con margen para la cámara y el movimiento del prototipo.

La selección usa principalmente tierra marrón/neutra, grietas y piedras, con aproximadamente 2.6% de tiles con pequeños detalles de vegetación. La distribución es fija y evita duplicados inmediatos cuando las variantes disponibles lo permiten. El tileset combina texturas con distintas tonalidades; es una primera zona de prueba, no un mapa final.

## Tileset e importación

Archivo: Assets/_BROTE/Art/Environment/Tiles/Ground/Degraded/Ground_Degraded_Tileset.png.

Se usó la versión más reciente encontrada durante el trabajo: 1448 × 1086 px, RGB opaco.
- Texture Type: Sprite (2D and UI).
- Sprite Mode: Multiple.
- Mesh Type: Full Rect.
- Filter Mode: Point.
- Compression: None; sin Crunch. Los overrides de plataforma se desactivaron para heredar estos valores.
- Sin mipmaps.
- NPOT Scale: None; Max Size: 2048 para conservar la resolución original.
- Pixels Per Unit: 181. Cada sprite ocupa exactamente 1 × 1 unidades y encaja en el Grid existente.
- Pivot: Center.
- Alpha as Transparency: desactivado porque el PNG es RGB sin transparencia.
- Sin generación de formas físicas de sprites.

Slicing uniforme: Grid by Cell Size, 181 × 181, Offset (0,0), Padding (0,0); equivalente a Grid by Cell Count 8 × 6. Los 48 rectángulos cubren el atlas completo. Se conservaron los nombres Ground_Degraded_Tileset_0…47, los GUID y los IDs de sprites del slicing existente.

La configuración se aplicó y verificó mediante las APIs de importación/Sprite Editor del Unity instalado, ejecutadas en una copia temporal del proyecto. No se abrió la ventana gráfica del Sprite Editor del proyecto principal ni se alteraron los píxeles del PNG. Puedes seleccionar la textura y abrir Sprite Editor para inspeccionar los cortes ya guardados; no hace falta repetir el slicing.

## Tiles y Tile Palette

- Tiles: Assets/_BROTE/Tiles/Ground/Degraded/Ground_Degraded_00.asset…Ground_Degraded_47.asset.
- Palette: Ground_Degraded_Palette.
- Prefab: Assets/_BROTE/TilePalettes/Ground_Degraded_Palette.prefab.
- Grid de la paleta: Rectangle, tamaño manual (1,1,0); contiene los 48 tiles en el orden visual 8 × 6 del atlas.

Para pintar más: Window > 2D > Tile Palette, selecciona Ground_Degraded_Palette y usa World/Grid/GroundTilemap como Active Tilemap.

Los Tiles no tienen collider. El suelo no añade obstáculos ni modifica el movimiento.

## Orden de renderizado y escena

Se añadió la Sorting Layer Ground antes de Default, conservando los IDs de Default y WorldObjects.
GroundTilemap usa Ground, Order in Layer -50, Tilemap Renderer en modo Chunk y conserva su material existente. La separación por Sorting Layer mantiene todo el suelo detrás de los spots, Suyay, árboles y props existentes, aunque el Y-Sorting produzca órdenes negativos.

En la escena solo cambiaron:
- Datos pintados de GroundTilemap.
- Configuración de su TilemapRenderer.
- Color de respaldo de Main Camera, de azul a marrón tierra.

No se cambiaron los otros Tilemaps, los objetos de juego ni sus referencias. Todos los scripts existentes, PlayerMovement, Animator Controller, clips, prefabs, HUD, inventario, vivero, plantación, restauración y Y-Sorting permanecen idénticos a sus archivos de partida.

El área es finita. Fuera de las 64 × 48 celdas no hay tiles; puedes extenderla con la paleta. El color marrón de cámara es solo un fondo de respaldo y no sustituye al suelo pintado.

## Comprobaciones realizadas

Unity 6000.6.3f1 importó los assets y comprobó:
- 48 sprites, cada uno de 181 × 181 px, geometría Full Rect y dimensiones mundiales 1 × 1.
- 48 Tile assets con referencias a los sprites.
- Paleta con los 48 tiles y su subasset GridPalette.
- 3072 celdas, sin huecos, incluidos inicio y vivero.
- Ground ordenada antes de Default, sin TilemapCollider2D.
- Guardado y recarga de la escena con los tiles persistidos y PlayerMovement presente.

Al transferir a BROTE se conservaron los demás bloques de la escena y se verificó que los archivos existentes no cambiaron. La comprobación fue en el Editor por batch; no se ejecutó una prueba visual ni de entrada en Play Mode.

Se guardaron copias previas de la escena, el meta del tileset y TagManager.asset en .brote-work/ground-backup/.

## Prueba manual pendiente

1. Si Acopalca estaba abierta antes de los cambios, recárgala desde disco antes de guardar. Si tienes otros cambios de escena sin guardar, consérvalos en una copia separada antes de recargar.
2. Espera la importación. En Scene, selecciona GroundTilemap y pulsa F: debe verse la zona de suelo.
3. Entra en Play. Suyay debe caminar sobre el suelo, sin que el azul sea el fondo principal.
4. Ve al vivero y a los cinco spots: comprueba que se vean por encima del suelo.
5. Recoge una plántula con E, planta y espera 5 + 5 segundos. El crecimiento, HUD y restauración deben seguir funcionando.
6. Camina delante y detrás de un queñual para comprobar que el Y-Sorting conserve su comportamiento.

