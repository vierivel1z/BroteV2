# BROTE - Ola2 implementada

Fecha2026-10-04. Runtime/Editor/tests compilados externamente; Play/renderizado/audio nativo requieren Unity. La escena guardada y prefabs anteriores no se editaron: la conexion nueva es explicita en el HUD y en eventos StageChanged existentes.

## Conexion

BroteHUD.OnEnable instala BrotePauseMenu y BroteAudioManager cuando Application.isPlaying; en Acopalca instala World/Audio desde dos prefabs Resources. Menu de pausa comparte PanelSettings del HUD; controles y sliders viven en UI Toolkit. No es necesario arrastrar otro componente a la escena. Desactivar HUD desactiva pausa y restaura tiempo. Reinicio requiere confirmacion y usa scene.path.

PlayerInteraction.RefreshTarget es separable del input para probar overlap; hysteresis0.08u evita flicker, respetando rango1.5. PlantingSpot/Nursery notifican exito solo tras consumo/add efectivos. HUD muestra feedback temporal en el canal actual.

BroteRestorationFeedbackState mantiene bitmask por sesion y agrupa deltas0.15s. Valor logico sigue instantaneo, barra SmoothDamp0.16s. Incremento visible1s, milestone2s; audio del mayor milestone de un batch se reproduce una vez. HUD conserva contenido y estilo, ahora abajo-izquierda para liberar paisaje.

QuenualGrowthFeedback conserva StageChanged, animacion solo del sprite y limiter3; Resources/VFX presets suministran configuracion a BroteGrowthVFX. BroteVFXPool limita tres instancias compartidas y nueve ParticleSystems (3 por instancia), creados lazily, sin collider. Pop permanece en feedback; dust/hojas/pulso en instancia reutilizada. Sprites procedurales son temporales y reemplazables por los campos existentes del feedback. Configurar cantidades, duraciones, escalas y velocidad en cada prefab. Defaults plant4dust/3hojas/.26s; Young5hojas+pulso/.32s; Developed8hojas+pulso/.38s. Crecimiento5+5/+10 no cambia.

BroteAudioManager aplica Master a UI/SFX/Ambience y SFX adicional solo aSFX. Preferencias BROTE.MasterVolume/BROTE.SFXVolume. Servicio de3voices/2growth/.18s; no clips: no sources. Focus mute configurable en manager runtime, sin tocar AudioListener global.

Resources/Audio/Sector01Ambience: wind/birds/leaves globales. DistantRiverAudio: river mono espacial, posicion tecnica provisional(-18,8,0),spatialBlend.85/maxDistance35. Clipsnull por defecto; facil asignacion en prefabs. Ambos se instancian bajoWorld/Audio, no mueven objetos ni crean colliders. ART_REQUESTS/003_AcopalcaAmbience.md WAITING_ART/WAITING_AUDIO.

## Validacion manual

1. Esperar importacion/compilacion de Unity; abrir Acopalca y usar BROTE/Validate/Full Project.
2. Ejecutar EditMode (23 casos) y PlayMode (11 tests) en Test Runner. La suite PlayMode es para Editor; usa prefab real y tiempos cortos solo en instancias de test. Mantener libre E y no usar input durante tests.
3. ESC: pausa, no movimiento ni E; continuar restaura velocidad. Confirmar/cancelar reinicio; reinicio recrea sector sin progreso, sin cambiar mapa ni balance. Salir de Play en Editor / escritorio en build.
4. Inspeccionar HUD en16:9,16:10,4:3: salud abajo-izquierda, prompts dentro del viewport, textos legibles sin superposicion.151/152 PLAY_REQUIRED.
5. Recoger+1, vaciar/llenar inventario, plantar y repetir E; mensajes1s, plantado0.9s, seleccionar estable entre dos targets y ocultar fuera de rango.
6. Madurar varios juntos: puntos instantaneos correctos, indicador+20/+30 agrupado0.15s, milestones20/40/50/100 una vez. Sonido de mayor milestone agrupado, sin spam.
7. Observar polvo (4 al plantar), hojas (3/5/8) y pulsos y pop; at most3 efectos, scale vuelve a1, no shake. Salir de camara durante VFX, destruir/desactivar un arbol, reiniciar sector y confirmar pool sin objetos activos atascados.
8. F3 solo Editor/Development: oculto inicialmente, FPS/posicion/count/stages/target. BROTE/Debug/Scene Overlays: radios, vivero, perimetro, camera y scenic.
9. Master/SFX en pausa: guardar/reabrir, Master0 silencia todo, SFX0 conserva ambience/UI. Perder/reganar foco silencia/restaura solo BROTE. Null clips no producen sonido ni AudioSource.
10. Asignar clips aprobados en Nursery/QuenualFeedback/HUD y Resources/Audio prefabs. Verificar loops de viento/aves/hojas globales y river espacial. El punto de rio es tecnico provisional, no nueva geometria.
11. Spot_08 sigue como antes: acercarse desde abajo y plantar; no moverlo sin fallo reproducido. Windows build completo todavia no ejecutado.
