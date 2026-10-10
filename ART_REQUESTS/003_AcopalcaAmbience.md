# Acopalca ambience — INTEGRATED

Entrega oficial integrada 2026-10-10: Ambience_Wind/Birds/River/GrassRustle.wav,
ruta real Assets/_BROTE/Audio/Ambient/. Config Resources/Audio/BroteAmbienceClips.
Cuatro loops estéreo de 27 s; río espacial conservado en (-18,8,0). La petición
original de río mono se sustituye por la instrucción actual de ForceToMono OFF.
Import Streaming/Vorbis70%; WAV intactos. PLAY_REQUIRED escucha y mezcla final.
Requisitos originales, conservados como historial:

Objetivo: ambiente altoandino tranquilo para Sector_01. No se suministran ni descargan sonidos provisionales. La infraestructura funciona con clips null y permanece silenciosa.

| Asset | Contenido y estilo | Duración aproximada | Uso |
| --- | --- | --- | --- |
| Acopalca_Wind_Loop.wav | Viento suave, abierto; sin ráfagas agresivas ni ruido saturado | 20–40 s | AmbientZone global |
| Acopalca_Birds_Distant_Loop.wav | Aves lejanas y dispersas, sin llamadas dominantes; revisar pertinencia local | 30–60 s | AmbientZone global |
| Acopalca_River_Distant_Loop.wav | Agua de río lejana, continua, sin oleaje marino | 20–40 s | Punto espacial de río |
| Acopalca_Leaves_Loop.wav | Roce sutil de hojas, pausado y natural | 20–40 s | AmbientZone global |

Formato: WAV PCM 48 kHz, 16/24 bits. Río mono para espacialización; globales mono o estéreo. Loops sin clicks, sin música o voces, headroom suficiente. Transparencia y dimensiones visuales: no aplican al audio. Entregar únicamente material con derechos de uso claros.

Destino: `Assets/_BROTE/Audio/Ambience/`.

Integración: asignar viento/aves/hojas en `Assets/_BROTE/Resources/Audio/Sector01Ambience.prefab`; río en `DistantRiverAudio.prefab`. Se instancian automáticamente en Play bajo `World/Audio`. Gain inicial 0.22; Master aplica también a ambience, SFX solo a sonidos de acciones. La posición de río (-18,8,0) es un punto técnico provisional, sin representación visual: ajustar su dirección/distancia cuando se valide el audio, sin mover objetos del mapa.

Validación manual: loops discretos, transición sin clicks, volumen confortable, silencio con null, foco de aplicación y Master en cero. No es arte/audio oficial hasta revisión humana. Solicitud complementa `002_AudioRequirements.md` para sonidos de acciones.
