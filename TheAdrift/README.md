# 彷徨者 The Adrift — 杀戮尖塔2 人物 Mod

一个完整的《杀戮尖塔2》人物 mod：**彷徨者**，66 生命 / 66 金币，使用**金币**与**诅咒**的力量在尖塔中追求天国。

基于 [RitsuLib](https://github.com/BAKAOLC/STS2-RitsuLib) 框架开发，支持游戏版本 `public-beta 0.111.0`。

---

## 一、项目结构

```
TheAdrift/
├── TheAdrift.csproj           # 构建配置（游戏路径、Godot 导出、mod 部署）
├── TheAdrift.json             # mod 清单（id: TheAdrift，依赖 STS2-RitsuLib 0.5.15）
├── TheAdrift.sln              # Godot C# 解决方案
├── project.godot              # Godot 工程配置
├── export_presets.cfg         # pck 导出配置
├── local.props                # 本机路径（Sts2Dir / GodotExe）
├── TheAdriftCode/             # C# 代码
│   ├── Entry.cs               # 入口：自动注册 + 先古映射 + 生命周期订阅
│   ├── Common/
│   │   ├── CardUtils.cs       # 原版牌引用、生成入堆、金币/变换助手
│   │   └── GoldTracker.cs     # 局内/战斗统计（摘下苹果光束、谁点燃了世界等）
│   ├── Characters/            # 人物 + 卡牌/遗物/药水池（含能量图标）
│   ├── Cards/                 # 全部卡牌（AdriftCardTemplate 统一卡面图路径）
│   ├── Powers/                # 全部能力
│   ├── Relics/                # 全部遗物
│   ├── Potions/               # 全部药水
│   └── Enchantments/          # 附魔「升格」
└── TheAdrift/                 # Godot 资源（pck 内容）
    ├── images/
    │   ├── cards/             # 88 张卡面图（类名.png，1400px 原图缩放至 512）
    │   ├── relics/            # 5 张遗物图标（256x256）
    │   ├── characters/        # 战斗立绘 / 选人图 / 顶部面板图标
    │   └── ui/                # 能量图标（24 / 74）
    ├── scenes/characters/     # 战斗模型 / 选人背景场景
    └── localization/          # zhs / eng 本地化
```

## 二、内容清单

| 类型 | 数量 | 说明 |
|---|---|---|
| 初始牌 | 4 | 打击×4、防御×4、回想×1、逡巡×1（回想由「古老牙齿」变为青春之诗） |
| 普通卡 | 19 | 白卡攻击/技能 |
| 罕见卡 | 35 | 蓝卡攻击/技能/能力（含 2 张多人合作卡：石之海、饼与鱼） |
| 稀有卡 | 29 | 金卡攻击/技能/能力（含 4 张多人合作卡：受国之垢、走向明天、尖塔中的圣诞快乐、采石） |
| 先古卡 | 2 | 青春之诗（欧洛巴斯）、阵亡形态（达弗） |
| 遗物 | 8 | 初始遗物搬史群（欧洛巴斯之触 → 智人TV）+ 6 专属 |
| 药水 | 3 | 冰红茶、万艳同杯、南北绿豆浆 |
| 能力 | 27 | 专属能力（含统计/奖励类） |
| 附魔 | 2 | 升格（每打出一次升级一次）+ 完美契合（原版 PerfectFit，黑线施加） |
| 对话 | 9 位 | 涅奥/欧洛巴斯/佩尔/特兹卡塔拉/达弗/瓦库/诺奴佩普/坦克斯/建筑师（胜利） |

## 三、原版牌复用（重要设计决策）

按需求，**诅咒与特殊牌全部复用原版**，mod 代码只引用、不新建：

| mod 内名称 | 原版类 | 备注 |
|---|---|---|
| 羞耻 / 疑虑 / 腐朽 / 受伤 / 悔恨 / 愚行 / 债务 / 睡眠不佳 / 凡庸 / 执迷 / 进阶之灾 | Shame / Doubt / Decay / Wound / Regret / Folly / Debt / PoorSleep / Normality / Enthralled / AscendersBane | 全部为原版诅咒 |
| 巨石 / 巨石+ | GiantRock（0 费生成攻击牌，升级 +4 伤） | 原版铁甲战士「原始力量」生成 |
| 虚无（虚无巨石） | Void | 原版状态牌（抽到失去 1 费） |
| 完美契合 | PerfectFit（洗牌后总在牌堆顶） | 原版附魔，由黑线施加 |

## 四、构建与部署

```bash
# 1. 编译 C#（自动复制 dll + json 到游戏 mods 目录）
cd TheAdrift
dotnet build          # 需要 local.props 中 Sts2Dir 指向游戏目录

# 2. 导出 pck（资源：本地化/图片/场景）
#    通过 _godot_export.cmd（Godot 4.5.1 Mono 无头导出，需要 C:\dotnet9 指向便携 SDK）
```

部署结果：
- `游戏目录/mods/STS2-RitsuLib/` — RitsuLib 运行时（v0.5.15）
- `游戏目录/mods/TheAdrift/` — `TheAdrift.dll` + `TheAdrift.json` + `TheAdrift.pck`

启动游戏 → 首次提示启用 mod → 重启 → 右下角「已加载模组」即成功。

> **⚠️ 与其他 mod 共用时的已知冲突（0.111.0 实测）**：「联机大厅」（Sts2LanConnect）mod 与当前游戏版本不兼容，其 patch 调用的 `StartRunLobby.MaxPlayers` 在 0.111.0 中已不存在，会导致点击「标准模式」无反应（界面卡死）。请移除或更新该 mod 后再游玩。彷徨者本体不依赖任何第三方 mod。

## 五、关键实现说明

- **卡面图**：所有卡牌继承 `Cards/AdriftCardTemplate`，自动加载 `images/cards/{类名}.png`；无图片的卡（石之海、饼与鱼）回退 RitsuLib 占位。
- **能量图标**：卡池/遗物池/药水池统一配置 `images/ui/energy_icon_24/74.png`（描述内 24、tooltip/卡角 74）。
- **人物资源**：`TheAdriftCharacter.AssetProfile` 以铁甲战士为基底合并覆盖（战斗立绘场景 + 选人背景/肖像 + 顶部图标），能量表盘等未覆盖项回退原版。
- **减费机制**（野蛮不眠/落阳）：通过 `ICardEnergyCostContributor` 能力实现，手牌/卡组诅咒数实时扣费。
- **圣战** 的可打出限制：`ICardPlayStateContributor.CanPlay` 能力（手牌中不得有"非诅咒且未升级"的牌）。
- **变形记** 爬 3 层变巨石：卡牌在手中回合结束时检查 `TotalFloor`，达到目标层自动变换。
- **谁点燃了世界 / 摘下苹果光束**：由 `GoldTracker` + 生命周期事件（金币增减/奖励拾取）统计。
- **黑线**：`CardSelectCmd.FromHand` 选择手牌 → `CardCmd.Enchant<PerfectFit>` 施加原版「完美契合」附魔。
- **不万能的喜剧**：`FlawedComedyPower` 监听抽牌/回合开始，手牌存在凡庸（被束缚）时把不万能的喜剧放回抽牌堆顶。
- **猪排饭**：通过补偿乘区实现（虚弱/脆弱 90%），常量 `VanillaStatusMultiplier = 0.75m` 需随原版数值校验（见 `Relics.cs` 注释）。
- **多人合作卡**（石之海等）：遍历 `RunState.Players` 实现；受国之垢为简化版（自动取对方全部诅咒）。
- **升格附魔**：`ModEnchantmentTemplate`，打出时 `CardCmd.Upgrade` 自身，支持多次升级。

## 六、后续扩展指引

1. **加新卡**：在 `TheAdriftCode/Cards/` 新建类继承 `AdriftCardTemplate`，`[RegisterCard(typeof(TheAdriftCardPool))]`，并在 `TheAdrift/localization/zhs/cards.json`（与 eng 同步）添加 `THE_ADRIFT_CARD_类名.title/.description`；图片放 `TheAdrift/images/cards/类名.png`。
2. **换美术**：直接替换 `TheAdrift/images/` 与 `TheAdrift/scenes/` 下的同名文件，重新导出 pck 即可；人物动画见 `Characters/TheAdriftCharacter.cs` 的 AssetProfile 与角色动画章节。
3. **人物动画/特殊攻击动画**：已预留场景与 `GetArchitectAttackVfx` 入口，替换 `scenes/characters/TheAdrift_character.tscn` 等即可。
4. **本地化**：新增语言复制 `localization/zhs` 为对应语言目录。
5. **数值平衡**：所有数值集中在各卡类 `CanonicalVars` / `OnUpgrade` 中，改动即生效。

## 七、已知取舍（后续可完善）

- 猪排饭依赖原版 0.75 乘区常量；若原版调整需同步。
- 福音的命中计数为"本回合攻击伤害结算数"的近似实现。
- 受国之垢未做"对方手动选择"交互，取对方全部诅咒。
- 象征形态的诅咒牌打出后按常规结算（不额外消耗）。
- 落阳的减费统计范围为「卡组 + 手牌」（原版 Deck 牌堆语义待确认，若 Deck 为聚合堆则手牌不重复计）。
- 低保/猪排饭/终末之花暂无遗物图标（回退占位）；能力与药水图标为占位。
- 关于地球的运动 / 电瓶车的升级数值（4 张/7 金币）为设计补充，docx 未定义。
- 坦克斯/诺奴佩普/建筑师的对白归属与 docx 叙述稿略有顺序差异（游戏对白树为固定模板结构）。
- `chara_select_button2.png`（选人按钮素材）暂未使用，如需要可作为选人界面的附加资源。
