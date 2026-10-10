# BROTE: plántulas de queñual

Esta etapa añade un único recurso; no incorpora inventario general, semillas ni producción pasiva.

## Archivos nuevos

- Assets/_BROTE/Scripts/Inventory/SeedlingInventory.cs: componente de Suyay. CurrentSeedlings comienza en 0. Max Seedlings es editable y está configurado en 5. AddSeedlings(int amount) ignora cantidades no positivas y limita la suma a la capacidad. TryConsumeSeedling() devuelve false sin stock y consume una si puede. SeedlingsChanged(int cantidad) se emite solo cuando cambia la cantidad.
- Assets/_BROTE/Scripts/Plants/NurserySeedlingSource.cs: componente del placeholder Sector_01/NurseryArea/NurserySeedlingSource. Implementa IInteractable y muestra “[E] Recoger plántula”. Busca SeedlingInventory en el GameObject del PlayerInteraction recibido y añade una. Si está lleno no cambia nada. No necesita referencias en Inspector, pero requiere un Collider2D para la detección existente.

NurseryArea está en (-6,0), a la izquierda de Suyay. El óvalo verde es un placeholder; usa un CircleCollider2D trigger en Default y no bloquea movimiento.

## Extensiones mínimas

PlantingSpot.cs conserva sus estados, prefab y crecimiento. Busca el inventario del jugador que interactúa y no crea una planta sin stock. Primero crea e inicializa la planta y después consume una. Una referencia ausente, una planta desactivada o una excepción al crear/inicializar no gastan stock; se elimina la instancia incompleta. El spot queda bloqueado antes de notificar el cambio de inventario para evitar una segunda plantación durante el evento.

Su nuevo campo Prompt Inventory referencia el inventario de Suyay en los cinco spots. Esa referencia se usa exclusivamente para el texto antes de interactuar, porque el contrato actual de InteractionPrompt no recibe un jugador. Esta etapa tiene un único jugador. La autorización y el consumo siempre usan el inventario del PlayerInteraction recibido. Con stock muestra “[E] Plantar queñual”; sin stock, “Necesitas una plántula de queñual”. CanInteract mantiene detectable el spot vacío para mostrar la indicación de falta de recurso.

BroteHUD.cs añade el campo Seedling Inventory, ya conectado a Suyay. Muestra “Plántulas: X / 5” debajo del porcentaje de salud. Lee el estado inicial al activarse y luego se suscribe a SeedlingsChanged; se desuscribe al desactivarse. No consulta la cantidad cada frame para actualizar el contador. El prompt conserva su lectura existente y refleja los cambios de stock.

Acopalca.unity añade el inventario y el vivero placeholder y conecta los campos del HUD y los spots. Se preservan los demás objetos y referencias de la escena actual.

## Prueba en Play Mode

Si Acopalca estaba abierta al editarse desde fuera del Editor, recárgala desde disco antes de guardar.

1. Entra en Play: HUD muestra “Plántulas: 0 / 5”.
2. Acércate a un spot vacío: “Necesitas una plántula de queñual”. E no planta.
3. Ve al óvalo verde en (-6,0): “[E] Recoger plántula”.
4. Pulsa E: el contador pasa a 1 / 5.
5. Vuelve al spot: “[E] Plantar queñual”. Pulsa E: se crea la planta y el contador vuelve a 0 / 5.
6. Otro spot vacío no permite plantar con 0. El spot ya plantado no permite duplicar ni consumir de nuevo.
7. Espera 5 + 5 segundos: siguen funcionando crecimiento, salud del valle y cambios visuales del sector.
8. Recoge repetidamente en el vivero: nunca supera 5 / 5. Cada pulsación entrega una; mantener E no genera producción automática.
9. Salir y volver a Play reinicia en 0: no hay guardado.

## Verificación

- Compilación externa contra las DLL del Unity instalado y el Input System del proyecto: sin errores.
- Prueba ejecutable del código real de SeedlingInventory con stubs mínimos de Unity: inicio en 0, recoger/consumir, capacidad, entrada int.MaxValue, agotamiento y eventos sin emisiones cuando no hay cambios.
- Verificación de referencias de escena y conservación de scripts, animaciones, controlador y prefab protegidos: correcta.
- No se ejecutó la prueba visual ni el flujo completo en Play Mode. Los stubs comprueban la lógica de cantidades, no el ciclo de vida de Unity.

