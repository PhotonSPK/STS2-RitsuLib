# STS2-RitsuLib

Shared framework library for Slay the Spire 2 mods.

## Features

- Shared framework bootstrap via `RitsuLibFramework`
- Unified logger and patcher factories (`CreateLogger`, `CreatePatcher`)
- Reusable patching infrastructure built on Harmony
- Expanded lifecycle events (framework init, profile services init)
- Shared persistence lifecycle via `DataReadyLifecycle` (`ProfileDataReady`, `ProfileDataChanged`, `ProfileDataInvalidated`)
- Per-mod persistent storage via `ModDataStore`
- `using`-based batch registration via `RitsuLibFramework.BeginModDataRegistration(modId)` (auto unified load on scope exit)
- Multi-instance localization helpers (`CreateLocalization`, `CreateModLocalization`)

## License

MIT
