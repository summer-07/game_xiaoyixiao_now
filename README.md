# 羊羊大反击 · game_xiaoyixiao_now

一款 3D 体素消除小游戏，目标平台是**微信小游戏**和**抖音小游戏**。狼、狐狸、熊和野猪由彩色方块拼成，
玩家派出带着同色羊毛弹的小羊去攻击它们，把方块一层层打掉。

- 引擎：Unity **2022.3 LTS**（`ProjectSettings/ProjectVersion.txt` 写的是 2022.3.62f1，任何 2022.3.x 都能打开；团结引擎 1.x 也可以）
- 渲染：Built-in 渲染管线，自定义的轻量着色器 `SheepStrike/VoxelToon`
- 架构：Bob 大叔的 Clean Architecture 加 Clean Code，约束写在 `.claude/skills/`

## 玩法

1. 屏幕上方是一只体素动物，可以拖动旋转。
2. 下方是羊群队列，每只羊有一个颜色和一个羊毛弹数量。点一列最前排的羊，把它派到发射位（羊圈）。
3. 发射位上的羊会自动朝**暴露在外**的同色方块发射羊毛弹，一发打掉一个方块。弹药用完，羊就离开。
4. 打掉全部方块就胜利。时间耗尽会失败。发射位都占满、而且场上没有一只羊能打到同色方块时，进入“羊圈挤满”状态。
5. 道具：
   - **羊角冲锋**：射速 ×2，持续 10 秒。每关送 1 次。
   - **扩羊圈**：增加一个发射位。
   - **牧羊犬**：下一次可以派出任意一只羊，不限最前排。
   - **赶羊群**：打乱队列。

   免费次数用完后，要看激励视频才能使用。

关卡无限循环：狼 → 狐狸 → 熊 → 野猪 → 狼王。每循环一轮，弹药分布更散、时间更紧，开局发射位也更少。

## 目录结构

```
Assets/_Project/
├─ Scripts/
│  ├─ Domain/          纯 C# 规则：体素模型、羊、羊圈、关卡生成、GameSession（不依赖 UnityEngine）
│  ├─ UseCases/        用例与端口：StartLevel / DeploySheep / UseBooster / FinishLevel；IPlatformService 等
│  ├─ Infrastructure/  适配器：微信 / 抖音 / 编辑器平台，PlayerPrefs 存档，动物体素库，关卡目录
│  ├─ Presentation/    视图与 Presenter：体素渲染、羊毛弹、碎块、uGUI HUD（全部用代码构建）
│  ├─ Bootstrap/       组合根 GameBootstrap（唯一负责 new 具体类的地方）
│  └─ Editor/          一键配置场景和 PlayerSettings，美术导入规则
├─ Resources/
│  ├─ Art/             Codex 生成的原创素材（图标、按钮、背景、App 图标）
│  ├─ Shaders/         VoxelToon.shader
│  └─ Fonts/           放 GameFont.ttf（中文字体，见下文）
└─ Tests/EditMode/     NUnit 单元测试（Domain + UseCases）
```

依赖方向：`Bootstrap → Presentation / Infrastructure → UseCases → Domain`。每一层是一个独立的 asmdef，
依赖方向由编译器强制检查。

## 在远程主机上运行

1. 用 Unity Hub 添加本项目，用 Unity 2022.3.x 打开。首次打开会自动生成 `Library` 和 `.meta`，
   并执行 `SheepStrike ▸ Setup Project`，创建 `Assets/_Project/Scenes/Main.unity`、设置竖屏。
2. 打开 `Main.unity`，点 Play。Game 视图建议选 1080×1920 竖屏分辨率。
3. 跑单元测试：`Window ▸ General ▸ Test Runner ▸ EditMode ▸ Run All`。
   命令行方式：
   ```bash
   Unity -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults results.xml
   ```
4. 重置进度：菜单 `SheepStrike ▸ Reset Progress`。

## 发布到小游戏

1. 切换平台到 WebGL。
2. **中文字体**：WebGL 没有系统字体可以回退，要把含中文的字体子集放到
   `Assets/_Project/Resources/Fonts/GameFont.ttf`（例如思源黑体子集）。不放的话，中文在真机上不显示。
3. 微信：导入 [minigame-unity-webgl-transform](https://github.com/wechat-miniprogram/minigame-unity-webgl-transform)
   （SDK 会定义 `WEIXINMINIGAME`），在 `PlatformIds` 里填激励视频广告位 ID，然后用“微信小游戏 ▸ 转换小游戏”构建。
4. 抖音：导入抖音 Unity SDK，在 Scripting Define Symbols 里加上 `DOUYINMINIGAME`。
   `DouyinPlatformService` 的 SDK 调用还没接（目前激励广告会安全地返回“未完成”），需要按所用 SDK 版本补上。

## 美术素材

`Assets/_Project/Resources/Art` 下的图片都是用 Codex 的图像生成功能制作的原创素材，清单见该目录下的
`README.md`。小羊图标的羊毛是白色，运行时按颜色染色。动物本体是代码里的体素模型
（`Infrastructure/Levels/AnimalLibrary.cs`），不需要 3D 美术资源。
