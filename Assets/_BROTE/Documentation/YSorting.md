# Y-Sorting de BROTE

YSort2D.cs se guarda en Assets/_BROTE/Scripts/Rendering/. Usa la Y mundial del Transform donde se instala: debe ser el punto de apoyo (pies o base del tronco).

La regla es Round(-Y × Multiplier) + Offset, limitada al rango de Sorting Order de Unity (-32768…32767).
Con Multiplier 100 y Offset 0: Y=3 da -300; Y=0 da 0; Y=-1 da 100. Una posición más baja dibuja delante, dentro de la misma Sorting Layer.

## Configuración aplicada

- Suyay en Acopalca: YSort2D sobre su SpriteRenderer, Multiplier 100, Offset 0.
- QuenualPlaceholder: SortingGroup y YSort2D en la raíz, Multiplier 100, Offset 0. El grupo ordena como unidad los tres SpriteRenderer de crecimiento y conserva sus sprites, posiciones y pivotes.
- Ambos usan la nueva Sorting Layer WorldObjects, situada sobre Default.
- Los Planting Spots y el suelo conservan Default y sus órdenes fijos. No tienen YSort2D. La Sorting Layer garantiza que queden debajo aunque los objetos dinámicos tengan órdenes negativos.

El script busca SortingGroup y SpriteRenderer una sola vez al activarse; si existen ambos, prioriza SortingGroup. LateUpdate realiza un cálculo ligero y solo escribe sortingOrder cuando cambia el valor calculado. ExecuteAlways permite previsualizar el orden al mover objetos en el Editor.

## Reutilización

Para un NPC o prop de un solo sprite, añade YSort2D al mismo GameObject del SpriteRenderer, con su pivote en el apoyo. Para varios sprites, añade SortingGroup y YSort2D a una raíz situada en el apoyo. Asigna WorldObjects al renderer o grupo y usa el mismo Multiplier que los demás objetos.

Offset modifica el orden manualmente: positivo adelanta y negativo retrasa. Mantén 0 para la comparación natural por Y; los empates de orden no tienen una prioridad adicional garantizada.

## Prueba

Recarga Acopalca desde disco si estaba abierta antes de los cambios. Entra en Play, planta un queñual y espera el crecimiento. Camina por arriba y por abajo de la base del tronco: Suyay debe quedar detrás arriba y delante abajo. Comprueba los árboles de los spots en Y=3 y verifica que el óvalo de suelo siga debajo.

PlayerMovement y Animator no se modificaron. Los scripts compilan sin errores contra el Unity instalado; las referencias y los bloques protegidos de escena y prefab fueron verificados. La prueba visual en Play Mode queda pendiente.

