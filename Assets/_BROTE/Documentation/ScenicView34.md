> HISTORICAL SNAPSHOT: prior project state. Current implementation and validation are in STATUS.md, Architecture.md and ValidationReport.md. Preserve this document as history.

# Acopalca: preparación de composición escénica 3/4

> HISTORICAL PREPARATION SNAPSHOT: the transparent sprite is now assigned at (0, 12.4, 0), scale (2.4, 2.4, 1), and ScenicBoundary is active. Current state: STATUS.md and ScenicBoundaryAndCamera.md. The empty-sprite/reference-position instructions below are no longer current.

```text
World
├── Background
│   └── AcopalcaBackdrop
├── Midground
│   ├── AcopalcaDistantLandscape   [SpriteRenderer, Sprite pendiente]
│   └── LandscapeTransition       [vacío]
└── Gameplay
```

El Ground opaco tapa cualquier imagen de Background dentro de su huella. La cámara ortográfica permanece dentro de esa huella. Además, el SpriteRenderer de AcopalcaBackdrop sigue sin sprite asignado en el archivo de escena inspeccionado. Importar una imagen no la asigna automáticamente al renderer.

AcopalcaDistantLandscape está preparado en la nueva Sorting Layer **Midground**, Order 0, material URP Sprite-Unlit-Default. El orden es Background → Ground → Midground → Default → WorldObjects → UI: supera al Ground y queda detrás de personajes, árboles y props actuales. No tiene collider, scripts de juego, máscara ni interacción. Su Sprite está deliberadamente vacío para evitar superponer el panorama opaco completo.

## Franja objetivo

Con tamaño ortográfico 5, la cámara muestra 10 unidades verticales. Reservar **3,25 unidades superiores (32,5%)** y **6,75 inferiores (67,5%)**. Para una cámara centrada en `(cx,cy)`, la banda ocupa `Y = cy+1,75 … cy+5`, con centro `(cx,cy+3,375)`. A 16:9, el ancho visible es aproximadamente 17,78 unidades. Los objetos preparados usan `(0,3,375,0)` para el paisaje y `(0,1,75,0)` para la transición, como referencia de una cámara centrada en `(0,0)`.

Es una composición de referencia, no un recorte activo de viewport. El SpriteRenderer Simple no recorta la región superior automáticamente. La cámara sigue a Suyay: **estos objetos estáticos no mantienen por sí solos el porcentaje 32,5% durante todo el recorrido**. La alineación del panorama al seguimiento debe resolverse en la siguiente etapa visual, sin cambiar el movimiento ni los límites del jugador. No se añade ahora seguimiento, rotación real de cámara ni una nueva proyección; se prepara la dirección visual de paisaje sobre el gameplay existente.

## Arte que falta

La imagen actual `Acopalca_Valley_Backdrop.png` mide **1672×941**, es RGB sin canal alfa y completamente opaca. Se conservaron archivo y meta/importador sin cambios.

Crear por separado **Acopalca_DistantLandscape.png**, manteniendo cielo, nubes, cordillera, nevados y profundidad del valle lejano de la mitad superior del panorama. Excluir el suelo cercano, rocas grandes, ichu y arbustos del primer plano inferior: competirían con el suelo y props jugables. Recomponer cielo y montañas para una banda amplia y baja, con borde inferior irregular y transparente; no limitarse a reducir verticalmente todo el panorama ni a subir su sorting.

Para 16:9, la silueta visible debería encajar aproximadamente en **17,78×3,25 unidades** (proporción 5,47:1), más un pequeño margen lateral. Un PNG puede incluir margen transparente adicional: para posicionarlo medir la región opaca, no solo el canvas completo. La transparencia debe conservarse en Unity. El área por debajo del borde inferior tiene que permitir ver el Ground y sus props.

## Pasos manuales en Unity

1. Preparar/importar la segunda imagen como Sprite, con transparencia y pivote adecuado. No sobrescribir ni modificar el panorama original.
2. Abrir Acopalca desde disco conservando cualquier cambio sin guardar. Seleccionar `World/Midground/AcopalcaDistantLandscape` y asignar **la segunda imagen**, no el panorama opaco completo.
3. Mantener Midground, orden 0, material unlit y ausencia de collider. Escalar uniformemente según el tamaño del nuevo sprite: `ancho mundial = anchoPx / PPU × escala`. Ajustar la región opaca para la banda de 3–3,5 unidades, evitando deformaciones. Su posición inicial preparada es Y 3,375; ajustar según el pivote y margen transparente del asset.
4. Usar `LandscapeTransition` en Y 1,75 como contenedor futuro de rocas, ichu, arbustos pequeños o borde de ladera que rompan la unión. Actualmente está vacío: no hay placeholders geométricos visibles ni decoración nueva.
5. Revisar Game View a 16:9 en la posición de referencia y luego durante el recorrido. Comprobar que la banda no tapa la parte inferior jugable; observar también que el HUD actual puede ocupar la franja superior. No se cambió su posición.

Cámara, tamaño/datos del Ground, Grid, jugador, inventario, vivero, spots, SectorBounds, scripts y restauración permanecen intactos. Se comprobaron IDs y jerarquía; dos GameObjects nuevos, un SpriteRenderer, cero colliders o scripts. Los originales y 496 archivos previos quedaron protegidos por comparación de hashes (el número exacto está en el reporte). No se ha probado la composición en Play: todavía falta el nuevo sprite. Backup y evidencia en `.brote-work/scenic-view/`.
