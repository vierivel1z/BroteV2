# Organización del vivero de BROTE

## Jerarquía de Acopalca

Sector_01/NurseryArea/Nursery_Acopalca
- Visual
- Interaction
- SortingPoint
- SolidColliders

NurseryArea conserva su posición mundial (-6,0,0). La raíz de la instancia está en local (0,0,0), rotación (0,0,0), escala (1,1,1). No se movieron los spots ni otros objetos.

Prefab: Assets/_BROTE/Prefabs/Environment/Nursery/Nursery_Acopalpa.prefab.
El archivo usa “Acopalpa” como se pidió en el entregable; la instancia de escena se llama Nursery_Acopalca. La raíz interna del asset toma el nombre del archivo por la creación de prefab de Unity; la instancia tiene su nombre explícito Nursery_Acopalca.

## Visual

El SpriteRenderer existente se trasladó a Visual mediante copia de su configuración y retirada del renderer original. Conserva escala (1,1,1), rotación y Flip X. No tiene collider.

El sprite realmente importado se llama Nursery_Acopalca_0, del PNG Nursery_Acopalca.png. Este es el nombre disponible, aunque la solicitud lo indicaba como Nursery_Acopala_0. En la escena de partida seguía asignado el placeholder, por lo que se sustituyó por el sprite definitivo. Se retiró el tinte verde del placeholder para mostrar los colores del sprite.

El PNG, su meta, PPU y slicing no se modificaron. El sprite mide 7.545 × 5.12 unidades con su configuración actual. Visual se desplaza respecto a la raíz para que el centro inferior de la estructura coincida aproximadamente con el apoyo de la raíz. No se cambió su escala.

## Interaction

Se reutilizó el GameObject original NurserySeedlingSource y se renombró Interaction. Conserva el mismo componente NurserySeedlingSource y CircleCollider2D; no se duplicaron ni se reescribió el script.

Su posición local es (0,-0.65,0), en el frente. El trigger conserva radio 0.5 y offset (0,0.25). El radio 1.5 de PlayerInteraction puede detectarlo desde esta zona.

La indicación y la recogida siguen siendo las existentes: “[E] Recoger plántula”, una por interacción, limitada por SeedlingInventory.

## SolidColliders

Tres BoxCollider2D estáticos, Is Trigger false:
- Estructura posterior/mesas: centro local (0,1.8432), tamaño (5.2815,1.536).
- Lateral izquierdo: centro (-3.018,1.1264), tamaño (0.82995,1.792).
- Lateral derecho: centro (3.018,1.1264), mismo tamaño.

La zona frontal central queda libre. Los colliders laterales se ajustaron para que ninguno contenga el centro de los cinco PlantingSpot existentes. Son una aproximación simple al volumen de postes y mesas; puedes afinar las cajas desde Inspector tras la prueba visual.

## SortingPoint y renderizado

SortingPoint es un Empty en local (0,0,0), junto al apoyo de la raíz.

YSort2D no tiene un campo de referencia a SortingPoint. No se modificó el script ni se añadió esa propiedad: se creó el Empty y la raíz se colocó en el mismo apoyo. El Y-Sorting lee la posición Y de la raíz; mover solo SortingPoint no cambia el orden.

Nursery_Acopalca tiene SortingGroup en WorldObjects y YSort2D con multiplier 100, offset 0. Visual también conserva WorldObjects. El vivero queda sobre GroundTilemap. La relación delante/detrás con Suyay usa el punto de apoyo de la raíz y el sistema existente.

## Verificación

En una copia aislada del proyecto, Unity 6000.6.3f1 compiló, guardó y recargó la escena y el prefab, y comprobó:
- Una única fuente de plántulas antes y después de recargar.
- Instancia conectada al prefab correcto.
- Jerarquía de cuatro hijos y componentes de sorting.
- Visual sin colliders, un trigger y tres cajas sólidas.
- Zona frontal libre para el tamaño de la cápsula de Suyay.
- Trigger detectable mediante una consulta Physics2D con radio 1.5.
- Ningún centro de spot dentro de las cajas.
- Llamar a la interacción existente añade una plántula al inventario real del jugador.

Esta verificación fue en el Editor por batch, no una simulación completa en Play Mode. No hay un Unity MCP disponible en esta sesión para manejar el Editor abierto. Falta comprobar visualmente movimiento, entrada E y superposición de sprites.

Al transferir a Acopalca se modificó únicamente el subárbol de NurseryArea y se añadió el prefab. Se comprobó que todos los scripts, PNG/meta, otros prefabs, animaciones, controlador y bloques de los spots conservaran sus bytes originales. Se guardó una copia previa de la escena y los informes en .brote-work/nursery-backup/.

## Prueba manual pendiente

1. Si Acopalca estaba abierta, recárgala desde disco antes de guardar. Conserva por separado cualquier otro cambio de escena que tengas sin guardar.
2. Entra en Play y revisa la posición, escala y aspecto del vivero.
3. Acércate a postes y mesas: las cajas deben bloquear a Suyay.
4. Acércate por el frente, cerca de (-6,-0.65): debe aparecer “[E] Recoger plántula”.
5. Pulsa E y comprueba el incremento de Plántulas, hasta el máximo 5.
6. Ve al spot cercano y planta para verificar que conserva el acceso y la funcionalidad.
7. Rodea el vivero por delante y detrás para comprobar Y-Sorting y que siga sobre GroundTilemap.

