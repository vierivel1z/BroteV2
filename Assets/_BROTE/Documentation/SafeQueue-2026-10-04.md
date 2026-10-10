> HISTORICAL SNAPSHOT: prior project state. Current implementation and validation are in STATUS.md, Architecture.md and ValidationReport.md. Preserve this document as history.

# BROTE — Ejecución de tareas SAFE, 2026-10-04

Se inspeccionó el estado real. DONE significa implementación o revisión completada; STATICALLY_VERIFIED no equivale a prueba en Play. No se editan YAML con Unity abierto.

## 001 — BLOCKED

PLAY_REQUIRED: no hay acceso para controlar Play del Unity abierto en esta sesión. La aproximación desde (0.479, 3.85) queda bajo el borde y dentro del radio de interacción; el solapamiento del trigger no prueba un fallo. No se mueve Spot_08 ni se cambia ScenicBoundary. Validación humana en Play pendiente.

VERIFIED: Runtime + Editor, 0 errores. Warnings: 51. Evidencia: `.brote-work/safe-queue-2026-10-04/task-001/`.

## 002 — DONE

STATICALLY_VERIFIED: diez instancias conservadas; referencias al prefab de queñual, RestorationManager 7002 e inventario 3010 válidas; sprites resueltos, triggers y capas coherentes; separación mínima 3.657 unidades. Todos los centros están bajo el límite. La excepción del trigger de Spot_08 queda en 001; Play pendiente. Evidencia: planting-spots.json. Sin cambios de escena ni prefab.

VERIFIED: Runtime + Editor, 0 errores. Warnings: 51. Evidencia: `.brote-work/safe-queue-2026-10-04/task-002/`.

## 003 — DONE

STATICALLY_VERIFIED: QuenualGrowthFeedback ya existe y está conectado al prefab; usa StageChanged y anima solo el renderer activo. Prueba de contrato recompilada: 5+5 s, +10 una vez y callbacks visuales fallidos no bloquean crecimiento. No se duplica el sistema. PLAY_REQUIRED para presentación.

VERIFIED: Runtime + Editor, 0 errores. Warnings: 51. Evidencia: `.brote-work/safe-queue-2026-10-04/task-003/`.

## 004 — DONE

STATICALLY_VERIFIED: pop de plantado ya implementado 0.85→1.05→1 en 0.26 s; escala original restaurada al terminar o desactivar. Sin cambios de posición ni escala raíz. PLAY_REQUIRED.

VERIFIED: Runtime + Editor, 0 errores. Warnings: 51. Evidencia: `.brote-work/safe-queue-2026-10-04/task-004/`.

## 005 — DONE

STATICALLY_VERIFIED: Young ya tiene pop 0.90→1.05→1 en 0.32 s, sin shake de cámara. Se mantiene transición a los 5 s. PLAY_REQUIRED.

VERIFIED: Runtime + Editor, 0 errores. Warnings: 51. Evidencia: `.brote-work/safe-queue-2026-10-04/task-005/`.

## 006 — DONE

STATICALLY_VERIFIED: Developed ya tiene pop 0.90→1.08→1 en 0.38 s, mayor que Young; +10 sin duplicación. Sin shake ni balance nuevo. PLAY_REQUIRED.

VERIFIED: Runtime + Editor, 0 errores. Warnings: 51. Evidencia: `.brote-work/safe-queue-2026-10-04/task-006/`.

## 007 — DONE

STATICALLY_VERIFIED: PlantDust existente, reutilizable, cuatro motas de tierra al plantar; emisión manual, sin colisiones ni gameplay. Solicitud de arte definitivo ampliada con nombres, dimensiones y uso en Unity; 012 sigue WAITING_ART. PLAY_REQUIRED.

VERIFIED: Runtime + Editor, 0 errores. Warnings: 51. Evidencia: `.brote-work/safe-queue-2026-10-04/task-007/`.

## 008 — DONE

STATICALLY_VERIFIED: GrowthLeaves existente, sprites procedurales reemplazables; 5 hojas en Young, 8 en Developed y 3 al plantar; máximo 12 por sistema. Sin arte externo. PLAY_REQUIRED.

VERIFIED: Runtime + Editor, 0 errores. Warnings: 51. Evidencia: `.brote-work/safe-queue-2026-10-04/task-008/`.

## 009 — DONE

STATICALLY_VERIFIED: RestorationPulse existente, anillo tenue en Young y mayor al madurar, alfa decreciente y lifetime ≤0.46 s; sorting detrás del sprite del árbol. No se cambia cámara. PLAY_REQUIRED para comprobar que no tapa a Suyay.

VERIFIED: Runtime + Editor, 0 errores. Warnings: 51. Evidencia: `.brote-work/safe-queue-2026-10-04/task-009/`.

## 010 — DONE

STATICALLY_VERIFIED: presupuesto global de tres efectos completos con reserva durante 0.48 s; libera al terminar/desactivar; excedentes y fuera de vista reciben pop mínimo sin partículas. Eventos de etapa aislados de errores visuales; prueba de contrato PASS. PLAY_REQUIRED para simultaneidad renderizada.

VERIFIED: Runtime + Editor, 0 errores. Warnings: 51. Evidencia: `.brote-work/safe-queue-2026-10-04/task-010/`.

## 011 — DONE

STATICALLY_VERIFIED: HUD existente escucha RestorationChanged; barra única suavizada con SmoothDamp 0.16 s y pulso de color 0.4 s, porcentaje real inmediato. Eventos repetidos no acumulan brillo ni escala. PLAY_REQUIRED.

VERIFIED: Runtime + Editor, 0 errores. Warnings: 51. Evidencia: `.brote-work/safe-queue-2026-10-04/task-011/`.

## 013 — DONE

STATICALLY_VERIFIED: Restoration_Props conserva 36 slices (12 Deg, 12 Rec, 12 Res), PPU 256; 26 props iniciales y 10 Rec/Res enlazados correctamente, sin collider ni scripts propios. SectorVisualState conserva manager y umbrales 20/40/50; sorting Default -3/-2 detrás de WorldObjects. No se rehace slicing ni se añaden props. PLAY_REQUIRED para aspecto y progresión. Evidencia: restoration-props.json.

VERIFIED: Runtime + Editor, 0 errores. Warnings: 51. Evidencia: `.brote-work/safe-queue-2026-10-04/task-013/`.

## 014 — DONE

STATICALLY_VERIFIED: BroteTilemapDiagnostics usa la sobrecarga vigente FindObjectsByType(FindObjectsInactive.Include), conservando búsqueda sin orden y objetos inactivos. Se eliminan seis avisos CS0618. CS0649 corresponden a campos asignados por Inspector/JsonUtility o defaults; CS0169 a campos serializados sin uso actual. Se conservan para no borrar datos ni tocar gameplay. Compilación externa: 0 errores, 45 avisos conservados.

VERIFIED: Runtime + Editor, 0 errores. Warnings: 45. Evidencia: `.brote-work/safe-queue-2026-10-04/task-014/`.

## 015 — DONE

STATICALLY_VERIFIED: STATUS refleja los archivos guardados y separa compilación, revisión estática y Play pendiente. TASKS documenta cada resultado; se corrige la documentación obsoleta del límite y se marca la preparación 3/4 como histórica. Informe técnico y evidencias por tarea disponibles. Ninguna tarea TODO SAFE restante; 001 BLOCKED y 012 WAITING_ART. Sin nuevas tareas ni mecánicas.

VERIFIED: Runtime + Editor, 0 errores. Warnings: 45. Evidencia: `.brote-work/safe-queue-2026-10-04/task-015/`.

