# Cleanup audit - 2026-10-04

STATICALLY_VERIFIED. No assets, packages, scripts or directories deleted. GUID searches establish known serialized references; absence is not proof of being unused (menu entry points, reflection and code-created objects are valid uses).

## scripts

- Assets\_BROTE\Editor\BroteArtValidator.cs - REVIEW_ONLY_NOT_PROVEN_UNUSED; serialized=0, code=0
- Assets\_BROTE\Editor\BroteGroundGenerator.cs - REVIEW_ONLY_NOT_PROVEN_UNUSED; serialized=0, code=0
- Assets\_BROTE\Editor\BroteProjectValidator.cs - REFERENCED; serialized=0, code=4
- Assets\_BROTE\Editor\BrotePropCatalog.cs - REVIEW_ONLY_NOT_PROVEN_UNUSED; serialized=0, code=0
- Assets\_BROTE\Editor\BroteQuickStatus.cs - REVIEW_ONLY_NOT_PROVEN_UNUSED; serialized=0, code=0
- Assets\_BROTE\Editor\BroteSceneDiagnostics.cs - REVIEW_ONLY_NOT_PROVEN_UNUSED; serialized=0, code=0
- Assets\_BROTE\Editor\BroteSceneValidator.cs - REVIEW_ONLY_NOT_PROVEN_UNUSED; serialized=0, code=0
- Assets\_BROTE\Editor\BroteSectorVisualGuide.cs - REVIEW_ONLY_NOT_PROVEN_UNUSED; serialized=0, code=0
- Assets\_BROTE\Editor\BroteTilemapDiagnostics.cs - REFERENCED; serialized=0, code=1
- Assets\_BROTE\Scripts\Environment\GroundRestorationController.cs - REFERENCED; serialized=1, code=3
- Assets\_BROTE\Scripts\Interaction\IInteractable.cs - REFERENCED; serialized=0, code=4
- Assets\_BROTE\Scripts\Interaction\PlayerInteraction.cs - REFERENCED; serialized=1, code=5
- Assets\_BROTE\Scripts\Inventory\SeedlingInventory.cs - REFERENCED; serialized=1, code=7
- Assets\_BROTE\Scripts\Plants\NurserySeedlingSource.cs - REFERENCED; serialized=1, code=4
- Assets\_BROTE\Scripts\Plants\PlantingSpot.cs - REFERENCED; serialized=1, code=7
- Assets\_BROTE\Scripts\Plants\QuenualGrowth.cs - REFERENCED; serialized=1, code=8
- Assets\_BROTE\Scripts\Plants\QuenualGrowthFeedback.cs - REFERENCED; serialized=1, code=1
- Assets\_BROTE\Scripts\Player\CameraFollow2D.cs - REFERENCED; serialized=1, code=5
- Assets\_BROTE\Scripts\Player\PlayerMovement.cs - REFERENCED; serialized=1, code=3
- Assets\_BROTE\Scripts\Presentation\BroteAudioFeedback.cs - REFERENCED; serialized=0, code=3
- Assets\_BROTE\Scripts\Presentation\BroteDiagnostics.cs - REFERENCED; serialized=0, code=4
- Assets\_BROTE\Scripts\Presentation\BroteEvents.cs - REFERENCED; serialized=0, code=4
- Assets\_BROTE\Scripts\Presentation\BroteTween.cs - REFERENCED; serialized=0, code=2
- Assets\_BROTE\Scripts\Presentation\GrowthVFXLimiter.cs - REFERENCED; serialized=0, code=2
- Assets\_BROTE\Scripts\Progression\RestorationManager.cs - REFERENCED; serialized=1, code=11
- Assets\_BROTE\Scripts\Progression\SectorVisualState.cs - REFERENCED; serialized=1, code=1
- Assets\_BROTE\Scripts\Rendering\YSort2D.cs - REFERENCED; serialized=3, code=4
- Assets\_BROTE\Scripts\UI\BroteHUD.cs - REFERENCED; serialized=1, code=0
- Assets\_BROTE\Tests\Editor\BroteEditModeTests.cs - REVIEW_ONLY_NOT_PROVEN_UNUSED; serialized=0, code=0

## clips

- Assets\_BROTE\Animations\Player\SuyayWalkBackAnim.anim - REVIEW_ONLY_NOT_PROVEN_UNUSED; serialized=0, code=0
- Assets\_BROTE\Animations\Player\Suyay_Idle_Back.anim - REFERENCED; serialized=1, code=0
- Assets\_BROTE\Animations\Player\Suyay_Idle_Front.anim - REFERENCED; serialized=1, code=0
- Assets\_BROTE\Animations\Player\Suyay_Idle_Left.anim - REFERENCED; serialized=1, code=0
- Assets\_BROTE\Animations\Player\Suyay_Idle_Right.anim - REFERENCED; serialized=1, code=0
- Assets\_BROTE\Animations\Player\Suyay_Walk_Back.anim - REFERENCED; serialized=1, code=0
- Assets\_BROTE\Animations\Player\Suyay_Walk_Front.anim - REFERENCED; serialized=1, code=0
- Assets\_BROTE\Animations\Player\Suyay_Walk_Left.anim - REFERENCED; serialized=1, code=0
- Assets\_BROTE\Animations\Player\Suyay_Walk_Right.anim - REFERENCED; serialized=1, code=0

## prefabs

- Assets\_BROTE\TilePalettes\Ground_Degraded_Palette.prefab - REVIEW_ONLY_NOT_PROVEN_UNUSED; serialized=0, code=0
- Assets\_BROTE\Prefabs\Interactables\PlantingSpot.prefab - REFERENCED; serialized=1, code=0
- Assets\_BROTE\Prefabs\Plants\QuenualPlaceholder.prefab - REFERENCED; serialized=1, code=0
- Assets\_BROTE\Prefabs\Environment\Nursery\Nursery_Acopalpa.prefab - REFERENCED; serialized=1, code=0

## Empty directories (retained)

- Assets\_BROTE\Animations\Environment
- Assets\_BROTE\Animations\NPC
- Assets\_BROTE\Animations\UI
- Assets\_BROTE\Art\UI
- Assets\_BROTE\Audio\Ambient
- Assets\_BROTE\Audio\Music
- Assets\_BROTE\Audio\SFX
- Assets\_BROTE\Prefabs\Characters
- Assets\_BROTE\Prefabs\UI
- Assets\_BROTE\Scenes\MainMenu
- Assets\_BROTE\ScriptableObjects\Items
- Assets\_BROTE\ScriptableObjects\Plants
- Assets\_BROTE\ScriptableObjects\Sectors
- Assets\_BROTE\ScriptableObjects\Upgrades
- Assets\_BROTE\Scripts\Automation
- Assets\_BROTE\Scripts\Core
- Assets\_BROTE\Scripts\Resources
- Assets\_BROTE\Scripts\Utilities
- Assets\_BROTE\Scripts\World
- Assets\_BROTE\Art\Characters\NPC
- Assets\_BROTE\Art\Environment\Effects
- Assets\_BROTE\Art\Environment\Trees
- Assets\_BROTE\Art\Characters\Suyay\Portraits
- Assets\_BROTE\Tiles\Ground\Degraded

Editor tools are MenuItem/EditorWindow entry points. BroteEvents/Tween/Diagnostics/AudioFeedback and GrowthVFXLimiter are code-referenced helpers. EditMode tests access runtime through reflection. AnimationClip Inspector fields can remain null while AnimatorController references actual clips. Nursery and PlantingSpot are scene prefab instances; Quenual is spawned from PlantingSpot. No candidate is automatically removed. See cleanup-audit.json for individual reference paths.
