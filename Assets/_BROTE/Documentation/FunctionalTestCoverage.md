# Functional test coverage - ola2

STATICALLY_VERIFIED source inventory. Compilation is recorded separately; native execution pending, batch could not open the already-open project. No measured coverage percentage.

## Assets\_BROTE\Tests\Editor\BroteEditModeTests.cs

- InventoryClamps
- InventoryConsumesExactlyOneAndNotBelowZero
- RestorationStartsAtZeroAndClamps
- GrowthAdvancesWithTwoFiveSecondWaitsAndContributesOnce
- PlantingGuardsRejectMissingSetupAndReentry
- TileSelectionIsDeterministicAndInBankBounds
- TileSelectionAcceptsInvalidProfileAndCleanCells
- YSortingClampsToRendererRange
- CameraViewportStaysInsideGround
- GrowthBudgetExpiresAndStaleReleaseDoesNotReleaseReplacement

## Assets\_BROTE\Tests\Editor\BroteWave2LogicTests.cs

- SimultaneousIncreasesAggregateWithoutRepeatingMilestone
- InteractionSelectionHasSmallStableMargin

## Assets\_BROTE\Tests\PlayMode\BroteLoopSmokeTests.cs

- PlantingCreatesOneTreeConsumesOneAndOccupiesSpot
- RealGrowthCoroutinePublishesStagesAndTenPointsOnce
- TenActualTreesReachOneHundred
- EmptyInventoryLeavesSpotAndManagerUnchanged
- FullNurseryDoesNotOverflow
- RepeatedContributionsClampAtOneHundred

## Assets\_BROTE\Tests\PlayMode\BroteWave2Tests.cs

- PauseRestoresPreviousSpeedAndDisableCannotLeaveTimeFrozen
- ResourcePresetsAreRealConnectedComponentsAndAmbienceNullIsSilent
- NativeOverlapSelectionIsStableAndClearsOutOfRange
- PoolReusesActualParticleSystemsAndRejectsFourthEffect
- MasterSfxAndFocusAreIndependentAndPreferencesAreRestored

Manual gaps: rendering, font/layout, keyboard input, perceived sound, actual boundary navigation and Windows executable. Native overlap/pause/resource/audio/null/pool test cases are created but not yet executed.
