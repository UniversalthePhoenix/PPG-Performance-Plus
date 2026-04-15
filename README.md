# PPG Performance+

`PPG Performance+` is now structured as an actual `People Playground` script mod.

## Mod Layout

- `mod.json` is the People Playground metadata and script manifest.
- `Scripts/` contains the runtime-loaded C# files the game compiles.
- `src/PPGPerformancePlus/` contains the original library-style scaffold for future refactoring or external builds.

## What is implemented

- hidden persistent runtime controller created from the official script entry point shape
- JSON config persistence at `Application.persistentDataPath/PPGPerformancePlus/config.json`
- first-run setup prompt and `F10` settings overlay
- frame-time tracking and sustained lag warnings
- auto-sleep for inactive dynamic rigidbodies
- offscreen interpolation reduction and sleep bias
- debris detection with optional cleanup
- adaptive `Physics2D` iteration profiles
- emergency conservative mode during severe stalls
- scene spike detection for heavy spawn events
- session report output at `Application.persistentDataPath/PPGPerformancePlus/session-report.txt`

## Install

Copy this repository folder into your `People Playground/Mods/` directory so the game sees `mod.json` at the root of the mod folder.

## Limits

- This version avoids hard dependencies on undocumented internal hooks, so spawn handling is based on scene spike detection instead of direct contraption interception.
- The settings panel uses Unity `OnGUI`, which is compatible but intentionally simple.
- `mod.json` currently uses `"GameVersion": "latest"`; if your local mod loader enforces an exact build string, update that field to match your installed game build.