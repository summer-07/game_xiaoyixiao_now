# 羊羊大反击 (SheepStrike) — agent guide (Codex / other agents)

Unity 2022.3 LTS 3D voxel colour-match game for WeChat / Douyin mini-games. Sheep fire wool at voxel
wolves, foxes, bears and boars.

**Before writing any C#, load the skills:**

- `.claude/skills/clean-architecture/SKILL.md` — layering (Domain ◄ UseCases ◄ Infrastructure/Presentation ◄ Bootstrap), Clean Code and TDD rules. Mandatory.
- `.claude/skills/minigame-platform/SKILL.md` — WebGL / mini-game budgets, SDK adapters, fonts, build steps.

Key facts:

- Code: `Assets/_Project/Scripts/<Ring>`; one asmdef per ring; Domain and UseCases have `noEngineReferences`.
- The whole scene is built in code by `Bootstrap/GameBootstrap` (auto-created even in an empty scene);
  `Editor/ProjectSetup` creates `Assets/_Project/Scenes/Main.unity` on first open.
- Runtime art: `Assets/_Project/Resources/Art` (generated with Codex, see its README). Loaded via `ArtLibrary`.
- Tests: `Assets/_Project/Tests/EditMode`. Unity is not installed on the dev box; the project is opened,
  run and tested on a remote host. Do not try to install Unity locally.
- Player-facing text is Chinese and lives only in `Presentation/Theme/Texts.cs`.
