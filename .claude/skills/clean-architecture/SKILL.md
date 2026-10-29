---
name: clean-architecture
description: Layering, dependency and Clean Code rules for the 羊羊大反击 (SheepStrike) Unity project. Use before adding or changing any C# under Assets/_Project — new gameplay rules, boosters, levels, views, platform SDK calls, or tests — and when reviewing such changes.
---

# SheepStrike Clean Architecture

This project follows Robert C. Martin's *Clean Architecture* and *Clean Code*. The rules below are not
suggestions: a change that breaks them is wrong even if it works in Play mode.

## 1. The rings (dependency rule)

Source dependencies point **inwards only**. Every ring is its own assembly, so the compiler enforces it.

```
 Bootstrap (composition root)  ──►  everything
 Presentation (views, presenter) ─┐
 Infrastructure (SDKs, storage)  ─┼──►  UseCases  ──►  Domain
                                  └──────────────────►  Domain
```

| Ring | Folder / asmdef | May reference | Must NOT reference |
| --- | --- | --- | --- |
| Domain (Entities) | `Scripts/Domain` · `SheepStrike.Domain` | nothing (`noEngineReferences: true`) | `UnityEngine`, platform SDKs, any other ring |
| Use Cases | `Scripts/UseCases` · `SheepStrike.UseCases` | Domain (`noEngineReferences: true`) | `UnityEngine`, Infrastructure, Presentation |
| Infrastructure (Gateways) | `Scripts/Infrastructure` · `SheepStrike.Infrastructure` | Domain, UseCases, UnityEngine, SDKs | Presentation, Bootstrap |
| Presentation (Interface adapters) | `Scripts/Presentation` · `SheepStrike.Presentation` | Domain, UseCases, UnityEngine, UGUI | Infrastructure, SDKs, Bootstrap |
| Bootstrap (Main) | `Scripts/Bootstrap` · `SheepStrike.Bootstrap` | all rings | — |
| Editor tooling | `Scripts/Editor` · `SheepStrike.Editor` | Bootstrap | runtime code must never reference it |

Rules that follow from this:

- **Game rules live in Domain.** Matching colours, exposure of voxels, slot capacity, win/lose/stuck,
  booster effects, level balancing. If you are writing an `if` about gameplay inside a MonoBehaviour, stop
  and move it into Domain.
- **Domain is pure C#.** No `UnityEngine.Random`, `Time`, `Debug`, `Vector3`, `Mathf`. Time is passed in
  (`GameSession.Tick(deltaSeconds)`), randomness is injected (`IRandom`), and coordinates use `VoxelPos`.
- **Use cases talk to the outside only through ports** in `UseCases/Ports` (`IPlatformService`,
  `IProgressStore`, `ILevelCatalog`). Infrastructure implements them; Bootstrap wires them.
- **Platform SDK code (WeChat `WX`, Douyin `TT`) exists only in `Infrastructure/Platform`**, wrapped in
  `#if WEIXINMINIGAME` / `#if DOUYINMINIGAME`, and is reached through `PlatformServiceFactory`.
- **Views are passive.** `HudView` & co. render what they are told and raise callbacks
  (`Action` fields). They never call use cases, never read PlayerPrefs, never decide outcomes.
- **The presenter drains domain events.** `GameSession` records `GameEvent`s; `GamePresenter.Tick`
  drains them each frame and translates them into animations. Domain never calls back into views.
- **Only `GameBootstrap` uses `new` on concrete infrastructure / view classes.** No singletons, no
  `FindObjectOfType` outside Bootstrap, no static service locators.
- Use the `SheepStrike.UseCases` name for the use-case ring. Do not create a namespace called
  `SheepStrike.Application`: it shadows `UnityEngine.Application` everywhere under `SheepStrike.*`.

## 2. Where does my change go?

| I want to… | Put it in |
| --- | --- |
| add a booster | `Domain/Session/BoosterType` + `GameSession.CanApply/Apply` + test; copy in `Presentation/Theme/Texts`; icon in `Resources/Art/Icons` + `ArtLibrary` |
| add an animal | `Infrastructure/Levels/AnimalLibrary` (+ `Rotation`), name in `Texts.Animal`, icon `target_<id>.png` |
| change difficulty curve | `Infrastructure/Levels/ProceduralLevelCatalog` (tuning) or `Domain/Levels/LevelGenerator` (algorithm) |
| new platform (Kuaishou, Alipay…) | new `IPlatformService` in `Infrastructure/Platform` behind a define + factory branch |
| persist new data | new port in `UseCases/Ports`, implementation in `Infrastructure/Persistence` |
| new HUD widget | `Presentation/Views`, built with `UiFactory`, driven by `GamePresenter` |
| new player flow (e.g. daily reward) | new interactor in `UseCases/Interactors`, called by the presenter |

## 3. Clean Code rules

- **Names reveal intent.** Use the game's language: `Sheep`, `WoolFired`, `LaunchPad`, `ExtraPen`;
  not `Obj`, `Mgr`, `Data2`, `Helper`. Booleans read as questions (`IsFull`, `CanApply`).
- **Small functions, one level of abstraction.** Aim for < 20 lines; extract a well-named private
  method instead of adding a comment that explains a block.
- **Small classes, single responsibility.** A class has one reason to change. Over ~200 lines is a
  smell; split it (see how `HudView` delegates to `SlotRowView`, `SheepQueueView`, `BoosterBarView`).
- **Few arguments.** ≤ 3 parameters; group the rest into an object (`LevelTuning`, `GamePresenter.ViewSet`).
- **No magic numbers.** Use named `const`s at the top of the class (`FlightSeconds`, `MaxPitch`).
- **Command/query separation.** A method either changes state or answers a question
  (`TryX(out ...)` is the accepted exception).
- **No null returns for collections; no swallowed exceptions.** Fail fast with `ArgumentException`
  / `InvalidOperationException` inside Domain.
- **Comments explain *why*, not *what*.** XML `<summary>` on public types describing their role.
- **Boy-scout rule.** Leave every file you touch a little cleaner — but don't reformat unrelated code.
- **Player-facing text** is Chinese and lives only in `Presentation/Theme/Texts.cs`.
  Code, identifiers and log messages are English.

## 4. Tests (TDD for rules)

- Every rule change in Domain or UseCases ships with an NUnit test in `Assets/_Project/Tests/EditMode`
  (assembly `SheepStrike.Tests.EditMode`). Write the failing test first.
- Tests are fast and deterministic: build tiny voxel models with `VoxelModelBuilder`, use
  `SeededRandom`, advance time with `Tick`, and use hand-written fakes for ports (see `UseBoosterTests`).
- Do not test MonoBehaviours for rules; if you feel the need, the rule is in the wrong ring.
- Run: Unity Test Runner (Window ▸ General ▸ Test Runner ▸ EditMode) or
  `Unity -batchmode -projectPath . -runTests -testPlatform EditMode -testResults results.xml`.

## 5. Review checklist

Before finishing a change, confirm:

1. `Domain` and `UseCases` still have no `using UnityEngine` (grep it).
2. No new asmdef reference points outwards.
3. New rule ⇒ new test; all EditMode tests pass.
4. No SDK symbol outside `Infrastructure/Platform`, and it is behind its `#if`.
5. No gameplay decision in a view; no `new` of infrastructure outside `GameBootstrap`.
6. Names, function size and magic numbers meet section 3.
