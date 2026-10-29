---
name: minigame-platform
description: Constraints for shipping the 羊羊大反击 Unity project as a WeChat (微信) or Douyin (抖音) mini-game — WebGL limits, SDK adapter rules, package size, fonts, ads and performance budgets. Use when touching platform code, build settings, art import, rendering cost, or preparing a mini-game build.
---

# Mini-game platform constraints

Target: **Unity 2022.3 LTS → WebGL → WeChat / Douyin mini-game converters.** Everything must also run in
the Editor without any SDK installed.

## SDKs and defines

| Platform | SDK | Scripting define | Adapter |
| --- | --- | --- | --- |
| WeChat | `minigame-unity-webgl-transform` (namespace `WeChatWASM`) | `WEIXINMINIGAME` (added by the SDK) | `Infrastructure/Platform/WeChatPlatformService` |
| Douyin | Douyin/ByteGame Unity SDK (namespace `TTSDK`) | `DOUYINMINIGAME` (add manually) | `Infrastructure/Platform/DouyinPlatformService` |
| Editor / plain WebGL | – | – | `EditorPlatformService` |

- SDK calls only inside those adapters, always behind the define. `PlatformServiceFactory` is the only
  switch. Verify SDK method signatures against the SDK version actually imported.
- Ad unit ids live in `PlatformIds`; never hard-code them elsewhere.
- A rewarded ad grants its reward **only** when the platform reports the ad was watched to the end.
  On any error the callback must still fire with `false`, so the game never hangs.

## Budgets

- First package (code + Resources) ≤ 4 MB for WeChat's first-load bundle; move large art to
  Addressables / CDN before it grows past that. Keep `Resources` lean.
- Textures: UI ≤ 256 px, background ≤ 1024 px (enforced by `Editor/ArtImportRules`). No mipmaps for UI.
- Rendering: no realtime shadows, no post-processing, one directional light, custom
  `SheepStrike/VoxelToon` shader (in `Resources/Shaders` so it is never stripped).
- Voxel cubes: only exposed voxels get a GameObject (`VoxelModelView`); pool everything that spawns
  per shot (`WoolProjectiles`, `DebrisBurst`). No `Instantiate`/`Destroy` per frame.
- Avoid `System.Threading`, `System.IO` file access, reflection-heavy code and LINQ in per-frame paths
  (IL2CPP + WebGL). LINQ is fine in tests.
- Target 60 fps (`Application.targetFrameRate = 60`), portrait only.

## Fonts and text

WebGL has no system font fallback: Chinese text renders as empty boxes unless a font with CJK glyphs is
shipped. Put a subset TTF (e.g. Source Han Sans / 思源黑体 subset to the characters used in
`Texts.cs` + digits) at `Assets/_Project/Resources/Fonts/GameFont.ttf`. `ArtLibrary` picks it up automatically.

## Storage, input, lifecycle

- Persist through `PlayerPrefs` (both converters map it to platform storage) behind `IProgressStore`.
- Input uses the legacy Input Manager (`Input.GetMouseButton*`, which maps touches); keep it that way
  unless both converters are verified with the new Input System.
- Do not block on network or SDK init: `Initialize()` must return immediately and degrade gracefully.

## Build checklist

1. Unity menu **SheepStrike ▸ Setup Project** (scene, portrait, WebGL compression off for converters).
2. Switch platform to WebGL. Import the platform SDK; confirm the define is present.
3. Add `GameFont.ttf`. Replace ids in `PlatformIds`.
4. Run EditMode tests. Build with the converter's own menu (微信小游戏 ▸ 转换小游戏 / 抖音 SDK build).
5. Test in the platform devtools on a real phone: first-load size, fps with 1000+ cubes, rewarded ad flow.
