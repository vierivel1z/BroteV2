# BROTE - Validation Report

Date:2026-10-04. STATICALLY_VERIFIED serialized audit; VERIFIED external source compilation/contracts. Native Unity Editor validator and EditMode Test Runner not executed; PLAY_REQUIRED for navigation, rendering and HUD.

Scene: Assets/_BROTE/Scenes/Acopalca/Acopalca.unity.

- Missing external GUID references: 0; missing local object references: 0. Audit covers _BROTE scenes/prefabs/assets and package GUIDs.
- Ten existing PlantingSpot prefab instances retain manager/inventory/tree/spawn references. No positions changed.
- Expected player, nursery, restoration manager and Ground controller exist. Ground banks retain48 tiles each;40x30 Ground and sprite importers unchanged.
- Expanded saved scene:85 GameObjects,50 SpriteRenderers,5 TilemapRenderers,20 colliders (11 triggers/9 solids). This is not a runtime profiler measurement.
- ScenicBoundary active/solid, ten approved world points, radius0.05, zero friction and bounce. Landscape and camera serialized values preserved.
- WARNING: Spot_08 trigger crosses boundary approximately0.354u; center is playable and approach from below is structurally possible. DECISIONS prohibits moving it without a reproduced Play failure.
- Camera source contract validated for five aspect ratios; actual scene values remain4.3 and(0,1.5).
- Balance contracts:5s+5s, +10 once,100 cap, inventory5, plant reentry guards; external checks pass.
- Ground old/new regression:48003 checks, identical states/tiles and1128 writes. Managed noise adapter is not native Perlin/rendering.
- VFX limiter/easing:10267 checks pass; camera/YSort:33; core gameplay:23.
- NUnit EditMode suite created and compiled; not presented as executed. Compiler logs and test evidence: .brote-work/autonomous/.
- WAITING_ART: official VFX sprites and five optional SFX. Null audio does not block gameplay.

Open Acopalca and use BROTE/Quick Status or BROTE/Validate/Full Project to inspect live imported objects, missing scripts/references, duplicate components, inner SectorBounds and art. Export Last Report replaces this file with that actual Editor inspection; do not confuse this external serialized report with an executed Editor menu.


## Ola2 - current implementation

Ola2 final report: new prefabs referenced via Resources.Load<GameObject> and GetComponent; HUD bootstrap binds pause/audio/ambience. Native batch rejected opening project already in use, exit1; no tests XML produced. Layout geometry checked but151/152 PLAY_REQUIRED. Existing scene/prefabs/official art and gameplay timings remain hash-identical. Additional103036 external checks for actual aggregation/audio/pool/selection sources PASS with managed adapters.
