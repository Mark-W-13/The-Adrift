using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Godot;

namespace TheAdrift.Characters;

/// <summary>
///     彷徨者（The Adrift）—— 66 生命值 / 66 金币的诅咒与金钱主题角色。
///     自定义资源：战斗立绘（battle_image）、选人图（chara_select）、顶部面板图标（logo）；
///     未覆盖的部分（能量表盘、商店/篝火模型、动画等）回退到铁甲战士原版资源。
/// </summary>
[RegisterCharacter]
public sealed class TheAdriftCharacter : ModCharacterTemplate<TheAdriftCardPool, TheAdriftRelicPool, TheAdriftPotionPool>
{
    // 主题色：#f3e5f5（淡紫白），用于卡池卡牌颜色、角色名与地图绘制
    public static readonly Color ThemeColor = new(0.9529f, 0.8980f, 0.9608f);

    public override Color NameColor => ThemeColor;
    public override Color EnergyLabelOutlineColor => new(0.32f, 0.24f, 0.42f);
    public override Color MapDrawingColor => ThemeColor;

    // 性别：女
    public override CharacterGender Gender => CharacterGender.Feminine;

    // 66 生命值 / 66 金币（低于常规角色）
    public override int StartingHp => 66;
    public override int StartingGold => 66;

    // 自定义资源：以铁甲战士为基底，逐项覆盖有素材的部分；无素材的字段保持回退
    public override CharacterAssetProfile AssetProfile => CharacterAssetProfiles.Merge(
        CharacterAssetProfiles.Ironclad(),
        new(
            Scenes: new(
                // 战斗立绘场景（battle_image）
                VisualsPath: $"{Entry.ResPath}/scenes/characters/TheAdrift_character.tscn"),
            Ui: new(
                // 顶部面板/统计页图标（方形，对齐原版 85x85 头像比例）
                IconTexturePath: $"{Entry.ResPath}/images/characters/logo_square.png",
                // 游戏左上角头像（the_adrift_logo 方形场景）
                IconPath: $"{Entry.ResPath}/scenes/characters/TheAdrift_icon.tscn",
                // 人物选择背景场景（chara_select）
                CharacterSelectBgPath: $"{Entry.ResPath}/scenes/characters/TheAdrift_bg.tscn",
                // 已解锁选人肖像（chara_select_button2）
                CharacterSelectIconPath: $"{Entry.ResPath}/images/characters/chara_select_button.png")));

    // 自动从 AssetProfile.Scenes.VisualsPath 创建战斗模型节点
    protected override NCreatureVisuals? TryCreateCreatureVisuals()
        => RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.Scenes!.VisualsPath!);

    // 时间线小故事暂不需要
    public override bool RequiresEpochAndTimeline => false;

    public override float AttackAnimDelay => 0f;
    public override float CastAnimDelay => 0f;

    public override List<string> GetArchitectAttackVfx()
    {
        return
        [
            "vfx/vfx_attack_blunt",
            "vfx/vfx_heavy_blunt",
            "vfx/vfx_attack_slash",
            "vfx/vfx_bloody_impact",
            "vfx/vfx_rock_shatter"
        ];
    }
}
