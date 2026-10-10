# ART REQUEST 001 — Queñual Growth VFX

Status: INTEGRATED

## Cierre vigente - 2026-10-06

GrowthLeaf = INTEGRATED. PlantDust = INTEGRATED.
NaturalSpark = INTEGRATED. RestorationPulse = INTEGRATED.
Ruta unica: Assets/_BROTE/Art/Environment/Effects/Growth/.
Dust usa dos slices de atlas derivado sin cambiar los PNG; un solo sistema,
cuatro motas y variante estable por particula. Pulse oficial conserva expansion/fade
Young/Mature. Spark solo Mature: una particula.075u/.24s/alpha.28; reutiliza PlantDust
sin polvo simultaneo y conserva3 sistemas/instancia y pool maximo3.
Fallbacks Dust/Pulse conservados; Spark ausente se omite sin bloquear gameplay.
Importaciones Sprite Single/PPU128/Alpha/Bilinear/sin mipmaps ni compresion.
Todos los PNG originales preservados. No se inicia Ola5.
VERIFIED: integracion nativa exit0, compilacion0errores, EditMode73/73 y PlayMode27/27,
sin fallos. Pool/reutilizacion/offscreen y regresiones de gameplay conservadas.
PLAY_REQUIRED: escala, saturacion, transparencia, repeticion, simultaneidad, camara4.3.
Backups/evidencias: .brote-work/art-request-001/. Ver STATUS.md.

Nubes grandes descartadas: no se identificaron archivos adicionales diferenciados.
Si aparecen, conservar como candidatas PotentialPlantingSpot AVAILABLE->PREPARED,
sin integracion automatica. No se elimina arte.

Las secciones siguientes son especificacion/historial anterior al cierre.

## Entrega vigente - 2026-10-06

GrowthLeaf: INTEGRATED. GrowthLeaf_01..04 recibidos en
Assets/_BROTE/Art/Environment/Effects/Growth/ e importados Single/PPU128/Alpha/Bilinear
sin compresion. Un atlas derivado conserva los pixeles originales y proporciona
cuatro slices de textura compartida a settings.leafSprites de los tres presets VFX.
Un solo GrowthLeaves; variante al nacer, frameOverTime constante y fallback conservado.
Dust/Pulse/Spark: RECEIVED / PENDING_INTEGRATION. Archivos presentes, sin asignar.
Aspecto y aleatoriedad visual: PLAY_REQUIRED.
Las instrucciones de entrega siguientes son historicas. Ver STATUS.md.

## Objetivo

Crear pequeños sprites para ParticleSystem utilizados cuando un
queñual se planta o cambia de etapa.

## Necesario

- 3–4 hojas verdes pequeñas diferentes;
- 2 partículas de tierra/polvo;
- pequeño destello natural;
- pequeño círculo/pulso verde.

## Estilo

BROTE.

2D cartoon pintado.

Natural.

Compatible con Suyay, queñuales y escenario.

NO pixel art duro.

NO efectos mágicos exagerados.

## Fondo

Completamente transparente.

## Uso

PlantDust
GrowthLeaves
RestorationPulse

## Tamaño

Assets pequeños para ParticleSystem.

No necesitan gran resolución.

## Destino

Assets/_BROTE/Art/Effects/Growth/

## Importante

No incluir:

- personajes;
- árboles completos;
- escenario;
- UI;
- texto.

## Especificación de entrega — 2026-10-04

Los efectos funcionan con sprites procedurales provisionales. La tarea 012 permanece WAITING_ART; no se reemplaza ni genera arte oficial automáticamente.

Assets base para sustitución directa:

| Archivo | Dimensiones aproximadas | Uso |
| --- | --- | --- |
| PlantDust.png | 64–128 × 64–128 px | Mota suave de tierra, borde difuminado |
| GrowthLeaf.png | 64–128 × 64–128 px | Hoja verde pequeña, silueta legible |
| RestorationPulse.png | 128–256 × 128–256 px | Anillo verde tenue, centro transparente |

Se pueden entregar además las variantes de hojas y polvo solicitadas arriba. La integración actual permite reemplazar un sprite por cada tipo de efecto; no requiere implementar un selector de variantes para la entrega base.

Todos los PNG deben ser RGBA con fondo totalmente transparente, sin escenario ni rectángulo opaco. Mantener el tono cartoon pintado, altoandino y tranquilo; evitar destellos blancos intensos, símbolos mágicos y resplandor que tape al personaje.

Ruta destino: `Assets/_BROTE/Art/Effects/Growth/`.

Uso en Unity: importar como Sprite (2D and UI), Single y con alfa habilitado. Para assets nuevos puede usarse PPU 128; no cambiar PPU del arte existente. Asignar en `QuenualGrowthFeedback` del prefab de queñual a Dust Sprite, Leaf Sprite y Pulse Sprite. Los tamaños y conteos los controla el efecto; mantener el material transparente compatible con URP 2D. El anillo se coloca a los pies y detrás del sprite del árbol.

Validación pendiente: aspecto en Game View, escala legible con cámara 4.3 y ausencia de saturación cuando crecen varios árboles.
