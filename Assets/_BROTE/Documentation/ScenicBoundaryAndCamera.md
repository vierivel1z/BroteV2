# Acopalca: saved landscape, scenic boundary and camera

The existing transparent Acopalca_DistantLandscape sprite is assigned. Its position (0, 12.4, 0), scale (2.4, 2.4, 1), Midground sorting order 0 and unlit material are preserved. The original PNG and both landscape assets/importers are unchanged.

World/Midground/LandscapeTransition/ScenicBoundary contains an EdgeCollider2D with 10 points, edge radius 0.05, non-trigger, and the existing zero-friction/zero-bounce SectorBounds material. It overlaps the side walls. LandscapeTransition remains at Y=1.75; collider points below are WORLD coordinates (subtract 1.75 from Y for Inspector local points).

CURRENT STATUS: the collider is ENABLED. Existing PlantingSpot_02 moved to (7.4, 0.7), PlantingSpot_09 to (-11, 2.55), and PlantingSpot_04 to (2.75, 1.8), preserving their prefab instances and references. The other seven were not moved. PlantingSpot_08 remains unchanged by DECISIONS.md; its trigger crosses the boundary slightly and needs Play validation (task 001 BLOCKED).

World points:
- (-19.5, 6.99)
- (-16, 5.6)
- (-12, 4.98)
- (-8, 4.53)
- (-4, 4.89)
- (0, 4.83)
- (5, 3.63)
- (10, 2.46)
- (14, 2.48)
- (19.5, 5.39)

CameraFollow2D adds framingOffset (0, 1.5), preserving the old offset through FormerlySerializedAs. Orthographic size is 4.3. Camera-center bounds: X [-12.35, 12.35], Y [-10.7, 10]. The optional ground viewport constraint is enabled on this scene camera and tightens horizontal bounds using size * aspect, and the bottom bound using size. Ground world extents stay (-20, -15) to (20, 15). Top is allowed to show the landscape. Other camera components retain their settings.

No parallax was added: initial proposed factors (0.15, 0.05) would move the opaque silhouette relative to the fixed collider. Applied movement is zero. A fixed landscape cannot maintain a 25-35% strip across the whole map. Its sky and mountain peaks are too far above the player for consistent visibility with this zoom and offset; no landscape transform change was made.

Validation: runtime and editor C# compile successfully with warnings. Existing scene documents are identical except the transition children, camera size and CameraFollow settings. Mathematical viewport checks pass for 4:3, 16:9, 21:9 and 32:9. Boundary centerline stays at least 0.164 world units below the alpha contour (threshold 32). Play and Game View were not executed.

Manual checks (PLAY_REQUIRED): walk into each active boundary segment, slide sideways and test diagonal movement and side-wall joins. Visit all ten spots, especially Spot_08, and plant at the three relocated spots. Verify nursery access, no jitter or stuck movement, the landscape/ground join and HUD readability. Do not move Spot_08 without a demonstrated Play problem and a design decision.
