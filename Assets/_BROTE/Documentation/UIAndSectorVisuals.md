# BROTE: UI y recuperación visual del sector

La escena Acopalca ya tiene todas las referencias asignadas. Si estaba abierta al editarse, recárgala desde disco antes de guardar.

## Archivos nuevos

### Scripts/UI/BroteHUD.cs
Está en el GameObject raíz BROTE_HUD. Crea una UI mínima con UI Toolkit al entrar en Play: “Salud del Valle”, barra, porcentaje y la indicación de interacción abajo. No requiere Canvas ni EventSystem, porque solo muestra información.

Inspector:
- Player Interaction: componente PlayerInteraction de Suyay.
- Restoration Manager: Sector_01/RestorationManager.
- Panel Settings: opcional. Si queda vacío, crea una configuración temporal con resolución de referencia 1280×720 y escalado con la pantalla. Esta configuración se elimina al destruir el HUD.

El porcentaje se lee inicialmente al activar el HUD y después se actualiza mediante RestorationChanged. Se desuscribe al desactivarse. La indicación consulta CurrentInteractable en LateUpdate, después de que PlayerInteraction seleccione el objetivo, comprueba CanInteract y actualiza el texto solo cuando cambia. Así desaparece inmediatamente después de plantar. El texto usa InteractionPrompt del contrato y permite otros interactuables en el futuro.

### Scripts/Progression/SectorVisualState.cs
Está en Sector_01/SectorVisualState. Lee la restauración al activarse y se suscribe a RestorationChanged; no tiene Update.

Inspector:
- Restoration Manager: Sector_01/RestorationManager.
- Thresholds: lista de grupos. Cada grupo contiene Minimum Percentage (0–100) y Objects (GameObjects).

En esta escena:
- 20%: Recovery_20_FirstShoots.
- 40%: Recovery_40_Vegetation.
- 50%: Recovery_50_Recovered.

Los grupos son acumulativos: a 50% están activos los tres. Cada objeto está activo cuando el porcentaje alcanza su umbral y se oculta si está por debajo. Asigna cada objeto a un solo grupo para que su umbral no sea ambiguo. No asignes el controlador ni sus antecesores: se omiten para evitar desactivar el receptor de eventos. Los hijos usan sprites placeholder existentes; puedes sustituirlos por objetos visuales definitivos en Inspector.

## Archivos modificados

### Scripts/Player/CameraFollow2D.cs
Conserva el seguimiento suave y la Z. En Main Camera configura:
- Use Bounds: activa o desactiva los límites (desactivado inicialmente).
- Min X / Max X: -8 / 8.
- Min Y / Max Y: -5 / 6.

Son límites del centro de la cámara en coordenadas del mundo, no de los bordes del encuadre. Se restringe tanto el destino como la posición final para que el suavizado no deje la cámara fuera del límite. Si inviertes min y max se ordenan al calcular el intervalo.

### Scripts/Interaction/PlayerInteraction.cs
Solo se retiró el Debug.Log que mostraba la indicación. La selección y la entrada E conservan su comportamiento. Los mensajes de plantación y restauración permanecen como diagnóstico; la UI ya presenta la información principal.

### Scenes/Acopalca/Acopalca.unity
Añade BROTE_HUD y Sector_01/SectorVisualState con tres grupos visuales, y serializa los límites de cámara. Los spots y los componentes existentes de Suyay conservan sus referencias.

## Prueba en Unity

1. Abre Acopalca, espera la importación y entra en Play.
2. Comprueba “Salud del Valle”, barra vacía y 0%.
3. Acércate a un spot vacío: aparece “[E] Plantar queñual”. Al alejarte se oculta.
4. Pulsa E: se planta una vez y la indicación desaparece si no hay otro spot vacío cerca.
5. Tras 10 segundos, la barra y el número aumentan a 10%.
6. Desarrolla dos, cuatro y cinco plantas: activa respectivamente los grupos de 20%, 40% y 50%.
7. Activa Use Bounds en Main Camera durante Play y camina más allá de los intervalos. El centro debe quedar limitado mientras Suyay sigue moviéndose. Desactívalo y comprueba el seguimiento libre.
8. Salir de Play reinicia la restauración y las plantas. No se añadió guardado.

## Validación

Compilación externa contra Unity 6000.6.3f1 e Input System del proyecto: sin errores. Verificación de referencias locales, GUID de componentes nuevos y conservación de scripts protegidos y bloques anteriores de escena: correcta. La prueba visual y de entrada en Play Mode no se ejecutó desde esta sesión.

