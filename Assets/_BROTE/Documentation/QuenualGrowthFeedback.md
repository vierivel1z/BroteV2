# Feedback del crecimiento del queñual

El prefab `Assets/_BROTE/Prefabs/Plants/QuenualPlaceholder.prefab` incorpora `QuenualGrowthFeedback`. No requiere cambios en los spots ni en la escena.

## Eventos y balance

`PlantingSpot` conserva su flujo: instancia el prefab e invoca `Initialize`. `QuenualGrowth.StageChanged` notifica la etapa inicial una sola vez al inicializar y después de cambiar los renderers en cada transición. `CurrentStageRenderer` permite que presentación anime únicamente el sprite activo. Los callbacks de presentación se aíslan para que una excepción no interrumpa el crecimiento.

Los tiempos continúan siendo 5 segundos hasta Young y otros 5 hasta Developed. Al madurar se conserva el evento `Matured` y una sola entrega de 10 puntos. No se añadió otro temporizador de crecimiento ni otra fuente de restauración.

## Feedback

| Momento | Escala relativa del sprite | Duración del pop | Partículas |
| --- | --- | --- | --- |
| Plantar | 0.85 → 1.05 → 1 | 0.26 s | 4 motas de tierra y 3 hojas |
| Seedling → Young | 0.9 → 1.05 → 1 | 0.32 s | 5 hojas y un anillo verde discreto |
| Young → Developed | 0.9 → 1.08 → 1 | 0.38 s | 8 hojas y un anillo verde algo mayor |

Las partículas terminan en un máximo de 0.46 s. No hay shake local ni de cámara. El pop afecta únicamente al Transform del renderer de la etapa: no cambia la raíz del árbol, colliders, posiciones ni Y-Sorting.

## ParticleSystems reutilizables

Al primer efecto completo, cada árbol crea tres hijos de presentación en Play:

- `PlantDust`
- `GrowthLeaves`
- `RestorationPulse`

Se reutilizan en sus siguientes etapas. Son sistemas sin looping, sin emisión automática ni colisiones, con un máximo de 12 partículas por sistema y simulación en espacio mundial. No se guardan nuevos ParticleSystems en la escena.

Los sprites provisionales son procedurales y compartidos: una mota suave, una hoja y un anillo. En el componente del prefab se pueden reemplazar `Dust Sprite`, `Leaf Sprite`, `Pulse Sprite` y `Particle Material` desde Inspector. Los sprites de reemplazo pueden estar en una hoja de sprites, con UV asignadas por Texture Sheet Animation. No se descargó arte externo.

## Saturación

El presupuesto global reserva como máximo tres efectos completos simultáneos, solo para árboles dentro del frustum de la cámara principal. Una reserva dura aproximadamente 0.48 s, cubriendo todas sus partículas; se libera también al desactivar o destruir el árbol. Los demás cambios conservan un pop mínimo 0.97 → 1.02 → 1 durante 0.18 s y omiten las partículas. No hay cola que retrase sprites ni puntos.

`Max Full Effects` está configurado en 3 en el prefab y admite 1–3. Modificarlo en el prefab mantiene un criterio compartido por sus instancias.

## Salud del Valle

`BroteHUD` sigue escuchando `RestorationChanged`. El ancho de la barra existente se aproxima suavemente al valor nuevo mediante SmoothDamp (0.16 s); el verde pulsa durante 0.4 s. El porcentaje textual refleja inmediatamente el total real. No hay otra barra ni indicadores sobre árboles. Cambios simultáneos reinician un pulso acotado, sin acumular escala o brillo.

## Validación y Play pendiente

Runtime y Editor compilan con Unity 6000.6.3f1. Una prueba con los fuentes reales de crecimiento y restauración y un adaptador de coroutines verifica 5 + 5 s, una sola contribución, tres notificaciones de etapa por árbol, 100 puntos tras diez árboles y crecimiento correcto sin presentación o con callbacks visuales que lanzan excepciones. No es una prueba de renderizado en Unity.

Comprobar manualmente:

1. Recoger una plántula y plantar: pop, tierra y tres hojas; consumo único de inventario.
2. A los 5 s, comprobar Young, cinco hojas y anillo pequeño.
3. A los 10 s, comprobar Developed, ocho hojas, anillo mayor y aumento único de 10 puntos.
4. Plantar varios árboles seguidos: máximo tres efectos completos, pops mínimos en los demás, todas las etapas y contribuciones correctas.
5. Alejarse mientras crecen: los puntos siguen llegando y los efectos fuera de vista se omiten.
6. Verificar texto de porcentaje, barra suave, pulso sutil, lectura del HUD y consola sin errores.
7. Confirmar que colliders, posiciones, Y-Sorting, cámara y límite escénico conservan su comportamiento.


## 2026-10-04 autonomous update

The current limiter uses three expiring, versioned leases (GrowthVFXLimiter), with same-frame visible/near-camera priority. Cancel/disable restores scale, stops particles and releases the correct lease. Full VFX remains independent of gameplay; excess trees receive a0.97/1.02 minimal pop. Duration/color fields are configurable without scene/prefab edits. Optional AudioClip fields remain null until approved SFX arrive; BroteAudioFeedback limits voices/cooldown. See Architecture.md and ART_REQUESTS/002_AudioRequirements.md. External contract tests passed; actual Play appearance is still PLAY_REQUIRED.


## Ola2 - current implementation

Ola2: Resources/VFX/PlantingVFX, GrowthVFX and MatureVFX now define settings used by feedback. Three pooled BroteGrowthVFX instances, each three lazy ParticleSystems, replace per-tree systems. Original sprite/material Inspector fields remain. Camera offscreen stops/releases particles; minimal pop/gameplay unaffected. See Wave2Implementation.md.

## Variantes oficiales - 2026-10-06

Settings/Leaf Sprites en Resources/VFX reemplaza el campo visible Leaf Sprite.
La referencia anterior se conserva oculta con FormerlySerializedAs por compatibilidad.
Los cuatro slices de GrowthLeavesAtlas.png comparten textura y Full Rect; sus pixeles
provienen de PNG oficiales intactos, importados Single/PPU128.
startFrame aleatorio normalizado 0..1, frameOverTime constante 0.
Las entradas null se omiten y la coleccion vacia mantiene el fallback procedural.
Actualizar sheet/material en cada burst evita datos obsoletos al reutilizar el pool.
Conteos, tiempos, escala, rotacion, velocidad, limite3 y gameplay sin cambios.
Dust/Pulse/Spark pendientes. Aspecto final PLAY_REQUIRED.

## ART REQUEST 001 integrado - 2026-10-06

Dust: settings.dustSprites[] en Resources/VFX contiene dos slices de PlantDustAtlas;
misma textura, Full Rect, variante aleatoria al nacer y frameOverTime0. Cuatro motas
con lifetime.3s/tamano/velocidad previos. Vacio/null conserva fallback.
Pulse: settings.pulseSprite oficial; fallback legacy/procedural si falta.
Expansion, fade, sorting0 y medidas Young.7/.38s y Mature1.1/.46s intactas.
Spark: solo Mature con sparkCount1, size.075/lifetime.24/alpha.28 cerca de copa.
Se reutiliza PlantDust cuando no hay polvo; polvo tiene prioridad si se configuran
ambos. Clamp2; sprite faltante omite acento. Burst limpia y reconfigura cada sistema,
material y sorting; siguen3 sistemas/instancia y pool maximo3.
Fuentes PNG intactas, Sprite Single/PPU128/Alpha/Bilinear/uncompressed/no mipmaps;
atlas derivado Multiple con pixeles identicos. Todos los assets INTEGRATED.
Presentacion visual PLAY_REQUIRED. Evidencia en .brote-work/art-request-001/.
