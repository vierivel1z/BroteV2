# BROTE — Etapa 1: carpetas y escena base

Proyecto inspeccionado: Unity 6000.6.3f1, plantilla URP 2D.

Esta entrega cubre los pasos 1 y 2. Todavía no es un prototipo jugable.
No se han añadido scripts C# ni paquetes.

## Abrir la escena

Abre `Assets/_BROTE/Scenes/Acopalca/Acopalca.unity` desde Project.
Si Unity solicita guardar otra escena que estabas editando, guarda tus cambios.

Jerarquía esperada:

```text
Main Camera
Global Light 2D
Acopalca
├── Grid
│   ├── GroundTilemap
│   ├── PathsTilemap
│   ├── WaterTilemap
│   ├── CliffsTilemap
│   └── DecorationTilemap
├── Vivero
└── Sector_01
```

`Acopalca`, `Vivero` y `Sector_01` son contenedores con Transform.
Los sectores restantes se añadirán después de comprobar la plantación inicial.

## Comprobación en el Editor

1. Espera a que Unity importe los archivos y abre la escena Acopalca.
2. Comprueba que Console no muestre errores al abrirla.
3. Selecciona Main Camera: Projection = Orthographic, Size = 5,
   Position = (0, 0, -10), Rotation = (0, 0, 0).
4. Selecciona Grid: Cell Size = (1, 1, 0), Cell Layout = Rectangle,
   Cell Swizzle = XYZ.
5. Cada hijo de Grid debe tener Transform, Tilemap y Tilemap Renderer.
   El material es Sprite-Lit-Default de URP; Global Light 2D está activa.
6. Pulsa Play. Es normal ver únicamente el fondo de la cámara:
   todavía no hay tiles pintados, personaje ni lógica jugable. Sal de Play.

Los Tilemap Renderer usan Sorting Layer Default y estos valores provisionales:

| Tilemap | Order in Layer |
| --- | ---: |
| GroundTilemap | -50 |
| PathsTilemap | -40 |
| WaterTilemap | -30 |
| CliffsTilemap | -20 |
| DecorationTilemap | -10 |

Estos valores separan las capas del terreno. El Y-Sorting de personajes,
árboles y objetos se implementará en la etapa correspondiente.
DecorationTilemap queda destinada por ahora a detalles planos del suelo.
Los objetos altos que deban ocultar al personaje necesitarán ordenación por Y.

No hay colisionadores en el terreno vacío. Al pintar obstáculos, añadiremos
TilemapCollider2D a las capas necesarias y, si corresponde, CompositeCollider2D
con Rigidbody2D de tipo Static. El suelo transitable no debe bloquear al jugador.

SampleScene y la lista de escenas de Build Settings se conservaron.
Para esta comprobación basta con abrir Acopalca y pulsar Play.
Antes de la primera build configuraremos Acopalca como escena inicial.

## Organización para las siguientes etapas

- Scripts/Player: movimiento y dirección del personaje.
- Scripts/Interaction: contrato IInteractable y detección con E.
- Scripts/Plants: puntos de plantación y representación de queñuales.
- Scripts/Resources: semillas, plántulas y Ayni.
- Scripts/World y Scripts/Progression: datos de sectores y Salud del Valle.
- Scripts/Core: conexión entre sistemas, sin concentrar todas las reglas.
- Scripts/Automation: reservado para trabajadores y simulación futura.
- Scripts/UI: presentación de recursos y progreso.

Las carpetas reservadas no implican que esos sistemas estén implementados.
Se crearán únicamente cuando la etapa jugable los necesite.

La simulación futura guardará datos numéricos independientes de los objetos
visibles. Un árbol no necesitará Update ni Animator propio. El crecimiento
se resolverá más adelante con tiempos almacenados y un sistema central.

## Próxima etapa

Crear Suyay con Rigidbody2D, Collider2D y un SpriteRenderer temporal; añadir
PlayerMovement para WASD y conservar la última dirección. Antes de crear
cada script se explicarán su ubicación, función, GameObject, componentes
y valores del Inspector, acompañados del código completo.

Después de comprobar el movimiento: Animator y Y-Sorting, seguimiento de
cámara, interacción y finalmente el circuito de plantación. El Input System
ya está instalado y se utilizará sin añadir dependencias.

## Validación de esta entrega

Se comprobaron estáticamente las carpetas, los archivos meta y las referencias
internas de la escena. La apertura y la prueba en Play Mode quedan pendientes
de comprobar en el Editor; una revisión de archivos no sustituye esa prueba.
