# Sector_01 — demo integrada

> Documento histórico. Para el estado actual, la auditoría y los cambios del terreno del 4 de octubre de 2026, consultar [ProjectAudit-2026-10-04.md](ProjectAudit-2026-10-04.md).

Escena: Assets/_BROTE/Scenes/Acopalca/Acopalca.unity.

## Terreno

GroundTilemap existente, bajo World/Grid: 40 × 30 = 1200 celdas, x −20…19, y −15…14. Empieza totalmente Degraded. Sorting Layer Ground, orden −50, sin collider. La cámara conserva seguimiento suave y usa límites de centro x ±11, y ±10, apropiados para la vista actual 16:9.

Tres atlas 1448 × 1086: Sprite Multiple, 8 × 6 celdas de 181 × 181, PPU 181, pivot central, Full Rect, sin formas físicas, mipmaps ni compresión, Clamp y Bilinear. Se usaron los nombres reales Ground_Recovery.png y Ground_Restored.png.

144 tiles en Assets/_BROTE/Tiles/Acopalca/Ground/{Degraded,Recovery,Restored}. Los 48 Degraded existentes fueron trasladados conservando sus GUIDs; la paleta Ground_Degraded_Palette conserva sus referencias. No hay duplicados.

## Editor y prefabs

BroteGroundGenerator.cs: BROTE > Ground > Generate Sector 01 / Clear Sector 01 / Regenerate Sector 01. Semilla 2026, ruido coherente, selección aproximada 70/25/5. Generate y Regenerate dejan diez spots y un suelo degradado reproducible. Clear borra solamente GroundTilemap y los spots bajo Sector_01/PlantingSpots. No borra el vivero ni los otros Tilemaps o sistemas. Guardar la escena después de usar estos menús; nunca se generan spots durante Play.

Prefab reutilizable: Assets/_BROTE/Prefabs/Interactables/PlantingSpot.prefab. Contiene la lógica y el trigger existentes, el prefab del queñual y el punto de plantación. Las referencias de RestorationManager e inventario se asignan como overrides en las instancias de escena, porque pertenecen a la escena.

Diez spots con las seis variantes visuales, capa Default, orden −1, sin YSort2D. Separación mínima 3,1 unidades; excluidos el vivero, colliders sólidos y árboles existentes. Todos permanecen dentro de la zona jugable.

## Restauración

GroundRestorationController.cs en Sector_01/GroundRestoration: registra árboles maduros, convierte su posición con WorldToCell y actualiza únicamente por eventos. Radio inicial 2,6 celdas, expansión irregular con ruido y porcentaje global. Hasta 39% crea Recovery; desde 40% aparecen manchas Restored que crecen hasta 100%. Las zonas alejadas de árboles mantienen tierra degradada. Las variantes por celda/estado son estables.

QuenualGrowth.cs: solamente se añadió el evento Matured y su emisión antes de aportar los 10 puntos. No se cambiaron tiempos, etapas, sprites ni aportes. PlayerMovement, Animator, PlantingSpot, inventario, HUD, RestorationManager y vivero permanecen intactos. Se añadió el componente YSort2D existente a Suyay porque faltaba en la escena guardada; el script de sorting no se modificó.

Restoration_Props.png no estaba en el proyecto; no se agregó decoración opcional. SectorVisualState conserva sus objetos existentes.

## Verificación realizada

Unity 6000.6.3f1, copia aislada, recarga final desde disco. Comprobados slices/importación, 144 tiles, referencias de escena/prefab, generación determinista y Clear/Regenerate. Play Mode real: interacciones simuladas llamando a los métodos existentes; planta inicial sin recurso rechazada, recogida/consumo, crecimiento con sus corutinas originales, diez árboles maduros y 100%, límite de inventario, HUD por eventos, Y-Sorting delante/detrás y recuperación local. El terreno empieza nuevamente degradado al volver a Play.

Resultado final de la simulación: Restoration=100; recovery=57; restored=720; mature=10.

La inyección de WASD/E en Unity sin Game View no fue fiable, por lo que esas teclas no se consideran verificadas automáticamente. Movimiento, Animator y PlayerInteraction permanecen byte-idénticos.

## Coordenadas mundiales

PlantingSpot_01: (-7.48, -3.46, 0.00)
PlantingSpot_02: (9.19, 7.90, 0.00)
PlantingSpot_03: (9.16, -6.79, 0.00)
PlantingSpot_04: (3.72, 5.68, 0.00)
PlantingSpot_05: (2.81, -2.19, 0.00)
PlantingSpot_06: (-2.01, 0.11, 0.00)
PlantingSpot_07: (0.22, -6.18, 0.00)
PlantingSpot_08: (0.48, 4.67, 0.00)
PlantingSpot_09: (-10.72, 6.40, 0.00)
PlantingSpot_10: (-5.15, -6.44, 0.00)

## Único paso manual

Reabrir Acopalca desde disco en Unity y entrar en Play; comprobar WASD y E (vivero → recoger → spot → plantar). No hay slicing, pintura, colocación ni referencias que configurar a mano. Si la escena ya estaba abierta, cargar la versión del disco antes de volver a guardarla.
