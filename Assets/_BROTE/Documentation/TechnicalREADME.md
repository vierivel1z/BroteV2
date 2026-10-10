# Technical README

Project: D:/UnityGames/Brote/Brote. Unity:6000.6.3f1. Scene:Assets/_BROTE/Scenes/Acopalca/Acopalca.unity. Do not use the nested legacy project.

Read root AGENTS.md, DECISIONS.md, TASKS.md and STATUS.md first. Preserve map transforms, camera size4.3/offset(0,1.5), ten spots, Ground40x30 and growth5+5/+10. Runtime scripts remain in Assembly-CSharp; Editor tools are under _BROTE/Editor. Test assembly uses reflection to inspect runtime code without moving scripts into new assemblies.

Tools: BROTE/Quick Status; BROTE/Validate/Full Project, Scene, Prefabs, Art References, Scenic Area; BROTE/Validate Sorting; BROTE/Validate/Export Last Report; BROTE/Debug/Scene Overlays; BROTE/Art/Restoration Props Catalog. Tools report and draw Editor overlays; they do not move objects or paint tiles. Open Acopalca first. Export overwrites only Documentation/ValidationReport.md.

Tests: Window > General > Test Runner > EditMode, Brote.EditModeTests. Native execution remains pending. External C# builds and contract tests are recorded in .brote-work/autonomous; these adapters verify actual source contracts but do not prove Unity physics, native Perlin, rendering or UI layout.

WAITING_ART: ART_REQUESTS/001_QuenualGrowthVFX.md and002_AudioRequirements.md. AudioClip fields may remain null; no sound objects are created for null clips. Replace procedural particles through QuenualGrowthFeedback Inspector fields after approval. Do not edit scene/prefab YAML while Unity is open.


## Ola2 - current implementation

Ola2 implemented: pause, stable selection, grouped restoration, VFX resource pool, volume/focus audio, silent ambience.23 EditMode cases plus11 Editor PlayMode tests compile; native execution pending because Unity project is open. Scene/prefabs existing unchanged. See Wave2Implementation.md.
