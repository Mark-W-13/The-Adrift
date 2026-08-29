# 彷徨者 The Adrift

《杀戮尖塔2》（Slay the Spire 2）人物 Mod：**彷徨者**——66 生命 / 66 金币，使用**金币**与**诅咒**的力量，在尖塔中追求天国。

基于 [RitsuLib](https://github.com/BAKAOLC/STS2-RitsuLib) 框架开发，适配游戏版本 `public-beta 0.111.0`。

## 内容

- **卡牌**：90 张（初始 4 种 + 普通/罕见/稀有/先古），含多人合作卡与 X 费牌
- **遗物** 8 / **药水** 3 / **能力** 27 / **附魔** 2（升格 + 完美契合）
- **完整自定义美术**：88 张卡面、人物立绘、选人背景、能量图标
- **9 位先古之民对话**（涅奥、欧洛巴斯、佩尔、特兹卡塔拉、达弗、瓦库、诺奴佩普、坦克斯、建筑师）
- 诅咒与特殊牌全部**复用原版**（羞耻、悔恨、凡庸、巨石、虚无、完美契合…）

## 目录结构

```
TheAdrift/
├── TheAdriftCode/       # C# 源码（人物/卡牌/能力/遗物/药水/附魔）
├── TheAdrift/           # Godot 资源（images / scenes / localization）
└── TheAdrift.csproj     # 构建配置
```

## 构建

```bash
# 1. 编译 C#（需要 local.props 中 Sts2Dir 指向游戏目录）
cd TheAdrift
dotnet build

# 2. 导出 pck（Godot 4.5.1 Mono 无头导出）
#    见 _godot_export.cmd（需要 C:\dotnet9 指向便携 .NET SDK）
```

部署：`游戏目录/mods/STS2-RitsuLib/`（RitsuLib v0.5.15）+ `游戏目录/mods/TheAdrift/`（dll + json + pck）。

> 详细文档见 [TheAdrift/README.md](TheAdrift/README.md)。
