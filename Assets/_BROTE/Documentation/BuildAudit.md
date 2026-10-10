# Build audit - 2026-10-04

STATICALLY_VERIFIED. Acopalca is the only enabled scene; SampleScene is disabled. Windows player build has not been executed. No build settings were changed.

## SampleScene references

- Assets\_BROTE\README.md:64: SampleScene y la lista de escenas de Build Settings se conservaron.
- Assets\Settings\Build Profiles\Web.asset:289: - line: '|   templateDefaultScene: Assets/Scenes/SampleScene.unity'
- Assets\_BROTE\Documentation\ProjectAudit-2026-10-04.md:7: Se leyeron los 13 scripts de runtime, los dos scripts de Editor, la escena serializada, los tres prefabs de juego, la paleta, el Animator, nueve clips, 144 Tile assets, los PNG y sus importadores, las referencias de GUID/fileID y la configuración de proyecto/build. También se revisaron las carpetas vacías y los documentos existentes. Acopalca está habilitada en Build Settings; SampleScene está deshabilitada. Fuera de `_BROTE` hay assets de plantilla URP y Welcome.
- ProjectSettings\EditorBuildSettings.asset:9: path: Assets/Scenes/SampleScene.unity
- ProjectSettings\ProjectSettings.asset:264: templateDefaultScene: Assets/Scenes/SampleScene.unity

The templateDefaultScene and Web build-profile text are template configuration; no runtime BROTE code loads SampleScene. Retain them until deliberate template cleanup.

## Package inventory

- com.unity.2d.animation: 16.0.1
- com.unity.2d.aseprite: 6.0.0
- com.unity.2d.psdimporter: 15.0.1
- com.unity.2d.sprite: 1.0.0
- com.unity.2d.spriteshape: 16.0.0
- com.unity.2d.tilemap: 1.0.0
- com.unity.2d.tilemap.extras: 9.0.1
- com.unity.2d.tooling: 4.0.1
- com.unity.collab-proxy: 2.13.6
- com.unity.ide.rider: 3.0.38
- com.unity.ide.visualstudio: 2.0.26
- com.unity.inputsystem: 1.20.0
- com.unity.learn.iet-framework: 6.0.7
- com.unity.render-pipelines.universal: 17.6.0
- com.unity.test-framework: 1.8.0
- com.unity.timeline: 6.6.0
- com.unity.ugui: 2.6.0
- com.unity.visualscripting: 1.9.12
- com.unity.modules.accessibility: 1.0.0
- com.unity.modules.adaptiveperformance: 1.0.0
- com.unity.modules.ai: 1.0.0
- com.unity.modules.androidjni: 1.0.0
- com.unity.modules.animation: 1.0.0
- com.unity.modules.assetbundle: 1.0.0
- com.unity.modules.audio: 1.0.0
- com.unity.modules.cloth: 1.0.0
- com.unity.modules.director: 1.0.0
- com.unity.modules.imageconversion: 1.0.0
- com.unity.modules.imgui: 1.0.0
- com.unity.modules.jsonserialize: 1.0.0
- com.unity.modules.particlesystem: 1.0.0
- com.unity.modules.physics: 1.0.0
- com.unity.modules.physics2d: 1.0.0
- com.unity.modules.physicscore2d: 1.0.0
- com.unity.modules.screencapture: 1.0.0
- com.unity.modules.terrain: 1.0.0
- com.unity.modules.terrainphysics: 1.0.0
- com.unity.modules.tetgen: 1.0.0
- com.unity.modules.tilemap: 1.0.0
- com.unity.modules.timelinefoundation: 1.0.0
- com.unity.modules.ui: 1.0.0
- com.unity.modules.uielements: 1.0.0
- com.unity.modules.umbra: 1.0.0
- com.unity.modules.unityanalytics: 1.0.0
- com.unity.modules.unitywebrequest: 1.0.0
- com.unity.modules.unitywebrequestassetbundle: 1.0.0
- com.unity.modules.unitywebrequestaudio: 1.0.0
- com.unity.modules.unitywebrequesttexture: 1.0.0
- com.unity.modules.unitywebrequestwww: 1.0.0
- com.unity.modules.vectorgraphics: 1.0.0
- com.unity.modules.vehicles: 1.0.0
- com.unity.modules.video: 1.0.0
- com.unity.modules.wind: 1.0.0
- com.unity.modules.xr: 1.0.0

Required by current scripts: Input System, 2D Tilemap, URP/unlit rendering, UI Toolkit engine modules, physics2D/animation/particles/audio. NUnit/Test Framework now supports the authorized EditMode suite. Sprite importer packages and tooling support the authoring pipeline; presence alone does not prove runtime use. Timeline, Visual Scripting, SpriteShape, Aseprite, PSD importer, tutorial framework, collaboration and IDE integrations are candidates for a HUMAN dependency review, not safe automatic removal. Engine modules can have indirect dependencies. No packages were removed. Versions above are installed project data, not upgrade recommendations.
