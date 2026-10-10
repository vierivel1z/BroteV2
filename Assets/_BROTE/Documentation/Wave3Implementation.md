# Ola3 — integrada y validada nativamente

2026-10-05. GeneratedEnvironment26Deg+10Pending, transición8, stableIDs01–10,
Session SaveSystem/Tutorial yMainMenu reales guardados. Build Settings0MainMenu/1Acopalca.
35 EditMode/16 PlayMode nativosPASS;0refsGUID/fileID faltantes;61assets protegidos intactos.
PLAY_REQUIRED revisión visual/inputs;BUILD_REQUIRED Windows;210 BLOCKED;004WAITING_ART.
Informe: .brote-work/wave-3-integration-final.md.

## Código terminado

SaveData v1 para Sector_01 con diez IDs, EMPTY/PLANTED, stages, inventory 0–5,
puntos y únicamente tutorialCompleted. SaveSystem escribe un JSON bajo
persistentDataPath, reemplazo atómico con .tmp; carga validada antes de mutar.
Solo carga en una escena fresca; Developed no reinicia Grow ni aporta +10;
Young retoma únicamente cinco segundos pendientes y Seedling retoma 5+5.
Timers parciales y posición del jugador no se guardan en este alcance.
Matured publica reconstrucción del Ground sin repetir StageChanged/VFX/audio;
HUD hace Sync silencioso durante carga. Autoguardado diferido a LateUpdate tras
plantar/cambio de stage/inventario/restauración evita persistir stage2 antes de +10.
Save corrupto/futuro no se sobrescribe automáticamente; Nueva Partida confirmada
permite borrarlo. Pausa ofrece Guardar con resultado y Nueva Partida confirmada.

BroteMainMenu: JUGAR/continuar, Nueva Partida confirmada, Controles, Audio Master/SFX
existentes y Salir seguro Editor/build. Carga asíncrona con mensaje sencillo.
Tutorial muestra un único objetivo del primer ciclo en UI Toolkit, concluye con
primer Developed, desaparece tras 5 s, y persiste solamente Completed. Sin quests.
Estas clases están conectadas realmente a Session/MainMenu; tests nativos PASS.

## Camino

ART_REQUESTS/004_Path_Acopalca.md WAITING_ART. PathDefinition y
Settings/Sector01PathDefinition.json conservan los nodos/ancho de VisualLayout;
Painter muestra bandas en Scene View, comprueba spots/colliders/boundary/bounds,
rechaza cualquier sprite ajeno a Paths/Path_Acopalca y cualquier conflicto.
Clear solo elimina GeneratedPath. No Ground tiles ni camino pintado.
Seis nodos del ramal norte están sobre/cerca de la frontera; también hay trazados
sobre spots y clearings antiguos. No se corrigen mediante cambios de mapa.

## Validación

VERIFIED: compilación externa Runtime/Editor/tests, cero errores;
54 warnings runtime/23 Editor (campos serializados/configuración, no se borran).
VERIFIED: 50028 contratos con adaptadores gestionados sobre código real,
incluyendo regresión del loop tocado y 10000 combinaciones de SaveData;
Developed silencioso y Young/Seedling retoman y aportan una sola vez.
STATICALLY_VERIFIED: Spot_08 se conserva; aproximación (.47924554,3.85), distancia
.816937 al centro (<1.5 interacción), clearance .841097 contra frontera.
No nueva colisión decorativa. Protected 61 archivos PNG/meta/prefab/animación intactos.
35EditMode/15PlayMode nativos PASS, incluidos12casosSave y4PlayMode deOla3.

VERIFIED: ejecución nativa35EditMode/15PlayMode PASS. PLAY_REQUIRED: renderizado y navegación manual.
BUILD_REQUIRED: ejecutable Windows.

## Checklist de densidad (193)

- [ ] 0%: 26 props bajos, espacios vacíos y vivero/spots accesibles.
- [ ] 20%: Pending_20 brotes/pasto visibles; sin ocultar prompt/Suyay.
- [ ] 40%: Pending_40 vegetación, sorting coherente y baja densidad.
- [ ] 50%+: flores/arbustos/pasto recuperado, referencias de umbrales preservadas.
- [ ] 100%: sector legible con 10 árboles y VFX simultáneos limitados.
- [ ] Spot_08 se planta desde abajo; ningún prop oculta la aproximación.
- [ ] Transición deja huecos y representa frontera visual cerca de la física.
- [ ] Regenerar misma seed produce mismas posiciones/sprites/escala/flip.
- [ ] Clear/Regenerate conservan props manuales y referencias Pending.

## Checklist funcional

- [ ] MainMenu carga Acopalca; Controles/Audio/Salir funcionan.
- [ ] Guardar/cerrar/reabrir con 0/5/10 maduros y inventory 0/5 conserva progreso.
- [ ] Save 50% no se convierte en 100% al cargar; esperar no suma por maduros viejos.
- [ ] Young cargado no vuelve a Seedling; carga no reproduce VFX/milestones.
- [ ] Ground y props reflejan los maduros cargados.
- [ ] Cancelar Nueva Partida conserva save; confirmar elimina únicamente ese JSON.
- [ ] Tutorial no reaparece después de completar/cargar.
- [ ] Windows build inicia en MainMenu y guarda en persistentDataPath.
