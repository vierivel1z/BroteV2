> HISTORICAL SNAPSHOT: prior project state. Current implementation and validation are in STATUS.md, Architecture.md and ValidationReport.md. Preserve this document as history.

# Fondo escénico de Acopalca

La escena está preparada con esta organización:

```text
World
├── Background
│   └── AcopalcaBackdrop   [SpriteRenderer]
├── Midground            [reservado, vacío]
└── Gameplay
    ├── Grid             [Tilemaps existentes]
    ├── Vivero           [contenedor organizativo existente]
    ├── Sector_01        [vivero real, spots, props y límites existentes]
    └── Suyay
```

Los nuevos padres están en posición (0,0,0), rotación cero y escala (1,1,1). La reorganización conserva posiciones mundiales, IDs y referencias; no se movieron spots ni NurseryArea. Cámara, luz y HUD conservan su organización anterior. No hay nuevos scripts de juego, colisiones ni interacción en el fondo.

## Asignar el panorama

1. Abrir `Assets/_BROTE/Scenes/Acopalca/Acopalca.unity` desde el proyecto `D:/UnityGames/Brote/Brote`. Si la escena ya estaba abierta, preservar cambios sin guardar y recargar desde disco antes de guardar sobre ella.
2. Importar el panorama como **Sprite (2D and UI)**. Seleccionar `World/Background/AcopalcaBackdrop` y arrastrar el sprite al campo **Sprite** de su SpriteRenderer. El campo vacío actual es intencional; no se creó un placeholder.
3. Conservar **Sorting Layer = Background**, **Order in Layer = −100**, color blanco y Draw Mode Simple. El material asignado es **URP Sprite-Unlit-Default**, que permite mostrar el fondo sin depender de la iluminación de las capas jugables.
4. Ajustar únicamente el Transform de AcopalcaBackdrop. Parte de posición **(0,0,0)**, el centro de la huella del Ground (40×30 unidades), y escala **(1,1,1)**. Para conservar proporciones, usar el mismo factor en X e Y. Si el panorama tiene `anchoPx` y PPU `p`, su ancho a escala 1 será `anchoPx/p`; para un ancho deseado `w`, usar escala uniforme `w/(anchoPx/p)`. No cambiar el PPU del Ground ni la escala de Gameplay o Grid.
5. Un pivote Center del nuevo panorama facilita centrarlo en (0,0). Si el sprite tiene otro pivote, ajustar la posición del objeto para centrar sus bounds. Desplazar Y puede alinear un horizonte; mantener Z dentro del rango de la cámara. El orden de dibujo está determinado por la sorting layer.

## Capas y visibilidad

Orden preparado: **Background → Ground → Default → WorldObjects → UI**. Se conservaron los IDs y el orden relativo de las capas anteriores. PathsTilemap y DecorationTilemap siguen en Default con órdenes −40 y −10; los props también conservan sus órdenes actuales. La nueva capa UI queda disponible para renderers futuros; el HUD actual usa UI Toolkit y no cambió.

**El Ground actual es opaco y cubre prácticamente todo el encuadre de la cámara limitada.** Un panorama colocado detrás puede quedar completamente oculto en Game View, aunque su sprite y sorting estén correctos. Aumentar la escala del panorama no lo hace visible a través del suelo. Para exponer montañas/cielo en una etapa de composición posterior habría que reservar área visible fuera de la huella del suelo, ajustar el encuadre o disponer transparencias apropiadas. No se hicieron esos cambios en esta preparación: se preservan suelo, cámara y loop jugable. Verificar el sprite en Scene View y la composición real en Game antes de elegir el encuadre definitivo.

Midground es únicamente una reserva de organización. No contiene decoración ni afecta a la jugabilidad. No se creó Foreground porque es opcional y no hay arte que lo requiera.

## Comprobación realizada

Referencias locales y jerarquía comprobadas; cuatro objetos nuevos y un SpriteRenderer. Cero colliders, cero scripts nuevos y ninguna pintura automática. Todos los componentes existentes quedaron idénticos; solo cambiaron los enlaces de los padres de World/Grid/Vivero/Sector_01/Suyay y la lista de raíces. Se preservaron 490 archivos anteriores de `_BROTE`, incluidos PNG, prefabs y scripts. Backup de escena/TagManager y evidencia en `.brote-work/backdrop/`. Play y la apariencia del panorama todavía no están verificados: el sprite debe asignarse primero.
