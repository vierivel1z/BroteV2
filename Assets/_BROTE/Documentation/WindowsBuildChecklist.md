# Windows build checklist

Status: PLAY_REQUIRED / BUILD_REQUIRED. This is a checklist, not a successful player build.

- Wait for Unity import/compilation; resolve Console errors.
- Open Acopalca and run BROTE > Validate > Full Project; review Spot_08 warning without moving it automatically.
- Run Window > General > Test Runner > EditMode > Brote.EditModeTests. Record XML results.
- Select Windows Standalone and an isolated output directory outside Assets. Keep Acopalca as the only enabled scene.
- Make a Development Build first, then a release build after testing. No build profile/platform setting was changed by this run.
- Launch the executable: WASD/arrows, E, nursery pickup, cap5, ten spots, 5s+5s growth, +10 each, maximum100.
- Check ScenicBoundary and four perimeter sides; slide laterally, confirm nursery front access and Spot_08 planting.
- Check landscape, sorting, smooth HUD and growth particles at target resolution; compare 16:9,16:10,4:3.
- Confirm no camera shake, errors, missing art, audio spam or stuck pop scale. Audio is intentionally silent while clips remain null.
- Test exiting and relaunching: current MVP has NO save/load; progress restarting is expected.
- Keep executable and associated *_Data directory together, include any build-generated DLLs, and record Unity version/build date.
