# Existing architecture

## Gameplay

PlayerMovement reads Input System Keyboard in Update, sets Animator parameters and uses Rigidbody2D.MovePosition in FixedUpdate. PlayerInteraction scans an overlap radius1.5, selects one nearest IInteractable and dispatches E. Reusable lists avoid an array per collider per frame.

SeedlingInventory owns count0..5 and SeedlingsChanged. NurserySeedlingSource adds one per interaction, guards repeated same-frame calls and requests existing HUD contextual feedback when full. PlantingSpot validates availability, locks reentrant calls, instantiates the existing Quenual prefab once, consumes one seedling, and rolls back failed planting. Existing prefab/scene references remain intact.

QuenualGrowth owns stages and the coroutine5s+5s. StageChanged notifies planting and stage transitions. Developed publishes Matured, then contributes10 once through RestorationManager; observer exceptions cannot interrupt point contribution. RestorationManager clamps to100 and publishes RestorationChanged through BroteEvents. BroteDiagnostics reports invalid configuration once in development/Editor Play.

## Ground and presentation

GroundRestorationController subscribes to Matured and restoration changes. Local monotonic Degraded/Recovery/Restored transitions use cached per-cell noise and nearest-mature-tree distance; scratch changes are reused. GroundTileSelection remains unchanged and deterministic. It does not regenerate the40x30 Ground.

SectorVisualState activates existing serialized groups at20/40/50; unchanged. YSort2D assigns int16-clamped order from world support Y to SortingGroup or SpriteRenderer. CameraFollow2D caches its Camera and applies the existing smooth follow, center bounds and viewport constraint; no parallax or scene parameter changes.

QuenualGrowthFeedback subscribes to stages, animates only the visible stage sprite, lazily reuses PlantDust/GrowthLeaves/RestorationPulse ParticleSystems. BroteTween contains small pure curves. GrowthVFXLimiter gives at most3 expiring versioned leases; events queued within the frame favor visible trees near the camera. Every tree changes stage and contributes regardless of visual budget; excess effects receive a minimal pop.

BroteHUD builds one UI Toolkit HUD, subscribes to manager/inventory/contextual feedback, smoothly updates the existing health bar and displays short milestone messages. No tree health bars. BroteAudioFeedback plays optional clips through at most3 lazy reused AudioSources, at most2 growth voices, with0.18s growth cooldown. Null clips are silent; no supplied audio assets exist.

## Scene and authoring

World contains Background, Midground and Gameplay. AcopalcaDistantLandscape is Midground0 at(0,12.4,0),scale(2.4,2.4,1). LandscapeTransition contains the solid ScenicBoundary; SectorBounds supplies four perimeter boxes. These scene objects were not edited during this run.

Editor validators inspect references, prefab stage sprites, interaction colliders, restoration banks, camera, sorting and boundaries. Quick Status shows counts and validation buttons. Scene overlays are Editor-only; configuration-only transition slots never create visible GameObjects. RestorationPropSets.json catalogs existing imported sprites. NUnit tests use reflection to retain the existing runtime assembly layout. No save/load, transport, upgrades, unlock system or global pool framework has been implemented.


## Ola2 - current implementation

Ola2 replaces per-tree particle allocation with a shared3-instance BroteVFXPool and actual Resources/VFX prefabs. Adds pause/restart confirmation, debug overlay, grouped deltas/milestone state, audio volume/focus manager and silent Resources/Audio AmbientZones. These are connected by HUD and StageChanged; see Wave2Implementation.md.
