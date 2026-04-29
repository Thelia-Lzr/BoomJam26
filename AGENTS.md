# ReactBabel - Unity Game Project

## Project Overview
Unity 2D visual-novel / battle game. Chinese-language dialog system driven by CSV TextAssets, with scene flow managed by singletons.

## Critical Naming Conflict
`SoundManager` class lives in `SoundManager` namespace. Always use full path:
```csharp
SoundManager.SoundManager.Instance.Play("sfx_name");
```
Do NOT use `using SoundManager;` and then `SoundManager.Instance` alone -- causes ambiguity error.

## Key Gotcha: Class vs File Name Mismatch
`Assets/GagetManager/DialogManager.cs` declares class `Dialogmanager` (lowercase 'm'). Do not rename without checking all references.

## Scene Flow (hardcoded indices)
- `0` = Main Menu (`BackToMainMenu` loads this)
- `1` = Story/Dialog scene (`StartNewGame` and `GoToNextStory` load this)
- `2` = Battle 1, `3` = Battle 2, `4` = Battle 3
- `EditorBuildSettings.asset` only lists one scene; ensure scenes are added before building.

## Source Layout
- `Assets/GagetManager/` -- Core game scripts (`Dialogmanager`, `SceneController`, `QuitGame`)
- `Assets/Scripts/Instances/SoundManager/` -- `SoundManager` singleton (namespace `SoundManager`)
- `Assets/Scripts/Samples/` -- `SoundSample` usage example
- `Assets/Sounds/` and `Assets/Resources/Sounds/` -- Audio clips (Music/, SFX/, UI/)
- `Assets/Scenes/` -- Unity scenes
- `Assets/DOTween/` -- DOTween imported as DLL (not via Package Manager)

## Architecture
- `SceneController` is a `DontDestroyOnLoad` singleton managing scene transitions and story index.
- `Dialogmanager` reads CSV rows (format: `tag, id, name, pos, content, next[, bgIndex]`), plays typewriter text, triggers battle transitions on "end".
- `SoundManager` singleton with two `AudioSource`s: `musicSource` (loops via `Play`) and `sfxSource` (`PlayOneShot`).
- DOTween used for UI animations (`DOColor`, `DOKill`).

## Build & Run
- Unity project builds via Unity Editor (not dotnet CLI).
- Open `ReactBabel.sln` in Rider/Visual Studio with Unity integration.
- Build via Unity Hub or Editor (`Ctrl+Shift+B`).
- Unity version: `2022.3.62f3c1` (China build variant).

## .gitignore Notes
- `.meta` files are excluded from git. This is unusual -- typically Unity projects track `.meta` files.
- `.sln` and `.csproj` files are also excluded (Unity regenerates them).
- `UserSettings/`, `Library/`, `Temp/`, `Logs/` are excluded.

## Package Dependencies
- `cn.unity.uos.launcher` -- UOS Launcher from `cnb.cool` (Chinese Git hosting)
- `com.unity.test-framework` -- Unity Test Framework present but no test files found
- `com.unity.textmeshpro` -- Used for dialog text rendering
- `com.unity.visualscripting` -- Visual Scripting included
