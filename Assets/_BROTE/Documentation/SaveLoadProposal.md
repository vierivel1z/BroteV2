# Save/Load proposal

DOCUMENTATION ONLY - UNAPPROVED. No serialization, persistence, new runtime IDs or resume hooks implemented.

Candidate versioned snapshot: schema version, sector key, saved restoration points/percentage, inventory count, and one record per planted spot (stable spot identifier, growth stage, remaining stage time, whether its10-point maturity contribution is already included). Store values, not Unity instance IDs or scene references. Current spots have names and prefab references, but no persistence identity contract; choosing stable IDs requires approval.

Load should validate schema, sector, count0..5, stage0..2 and restoration0..100. Missing/duplicate spot identities must report a recoverable error. Define how restoration is reconciled with developed trees before implementation; do not blindly replay AddRestoration or Matured and duplicate points. Rebuild presentation/Ground from authoritative gameplay state, rather than saving VFX or hundreds of tile indices. Preserve5+5 growth and existing tile selection.

Use a temporary file plus atomic replacement and backup only after a storage path/format is chosen. Corrupt/missing files should permit a new session without deleting the backup. Approval needed: slots/autosave triggers, platform storage, offline growth or frozen time, persistent identity, migration and partial-save handling. Suggested future tests: round trip at each stage, inventory limits, duplicate maturity prevention, interrupted write, corrupt/version-mismatched file. These tests and this system do not currently exist.
