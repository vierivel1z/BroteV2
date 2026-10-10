# BROTE: prueba mínima de plantación

> Documento histórico. Para el estado actual, la auditoría y los cambios del terreno del 4 de octubre de 2026, consultar [ProjectAudit-2026-10-04.md](ProjectAudit-2026-10-04.md).

Abre Assets/_BROTE/Scenes/Acopalca/Acopalca.unity. Si ya estaba abierta durante los cambios, recárgala desde disco antes de guardar. Espera a que Unity importe y compile; abre Console y pulsa Play.

1. Mueve Suyay con WASD. Main Camera debe seguir X/Y manteniendo Z.
2. Los cinco óvalos marrones están en (2,0), (5,0), (-3,0), (0,3), (3,3).
3. Acércate a menos de 1.5 unidades del centro. La UI muestra “[E] Plantar queñual” mientras hay interacción disponible. Si hay dos cerca, se selecciona solo el más cercano.
4. Pulsa E con Game enfocado. Aparece una plántula; pulsar E otra vez no planta encima.
5. Después de 5 segundos aparece el joven; después de otros 5, el desarrollado.
6. Console muestra “Restauración: 10%”, luego 20%, etc. Los cinco spots permiten 50%; el máximo configurado es 100. Esperar más no vuelve a sumar.
7. Salir de Play reinicia esta prueba: no hay guardado.

## Componentes y configuración

Todos los scripts se guardan debajo de Assets/_BROTE/Scripts/. Las referencias de esta escena ya están asignadas.

| Script / carpeta | Función y objeto | Inspector |
| --- | --- | --- |
| CameraFollow2D / Player | Sigue suavemente en LateUpdate. Está en Main Camera, que ya es ortográfica. | Target: Suyay (Transform); Offset: (0,0); Smooth Time: 0.18. Conserva Z. Use Bounds activa límites configurables del centro en coordenadas del mundo; consulta UIAndSectorVisuals.md. |
| IInteractable / Interaction | Contrato reutilizable: disponibilidad, posición, indicación e interacción. Lo implementa PlantingSpot; no se añade como componente. | Ninguno. Futuros objetos implementan este contrato y tienen Collider2D. |
| PlayerInteraction / Interaction | Está en Suyay. Busca colliders en un radio, selecciona un interactuable y lee E mediante Keyboard.current del Input System actual. | Interaction Radius: 1.5; Interaction Layers: Default. Incluye triggers y omite componentes desactivados. CurrentInteractable alimenta la indicación del BroteHUD. |
| PlantingSpot / Plants | Está en cada PlantingSpot_01…05 de Sector_01/PlantingSpots. Cambia de EMPTY a PLANTED al instanciar la planta. | Plant Prefab: QuenualPlaceholder; Restoration Manager: Sector_01/RestorationManager; Planting Point: su propio Transform (puede ser un hijo). El CircleCollider2D es trigger para no bloquear movimiento. |
| QuenualGrowth / Plants | Está en la raíz del prefab Assets/_BROTE/Prefabs/Plants/QuenualPlaceholder.prefab. Una coroutine cambia las tres etapas y aporta una sola vez. PlantingSpot llama Initialize con el manager. | Seedling, Young, Developed: los tres SpriteRenderer hijos; Seconds To Young: 5; Seconds To Developed: 5; Restoration Points: 10. Cada hijo permite sustituir su Sprite en Inspector. |
| RestorationManager / Progression | Está en Sector_01/RestorationManager. Suma puntos hasta el máximo. Expone CurrentRestoration, MaxRestoration y RestorationPercentage (0–100). | Max Restoration: 100. El evento RestorationChanged entrega el porcentaje para la barra del BroteHUD y SectorVisualState. |

Los placeholders son PNG geométricos en Assets/_BROTE/Art/Placeholders, con 32 píxeles por unidad, pivote inferior y filtro Point. La planta no tiene collider sólido. No se modificaron PlayerMovement, Animator Controller, clips ni sprites de Suyay.

## Verificación realizada

Los scripts compilan contra las DLL del Unity 6000.6.3f1 instalado y Unity.InputSystem del proyecto, sin errores. Las advertencias de campos serializados asignados por Inspector y los campos de clips existentes no impiden compilar. Se verificaron los IDs locales y GUID nuevos en escena y prefab, y la conservación de los componentes existentes de Suyay.

La prueba visual y de entrada en Play Mode queda pendiente de realizar en el Editor: la compilación externa no ejecuta el juego ni valida la importación de texturas por Unity.

Se conserva una copia del texto original de la escena en Documentation/Acopalca.before-vertical-slice.txt.


La etapa de UI y recuperación visual está documentada en [UIAndSectorVisuals.md](UIAndSectorVisuals.md).
