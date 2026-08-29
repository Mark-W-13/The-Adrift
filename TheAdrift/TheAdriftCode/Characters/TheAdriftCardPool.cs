using Godot;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace TheAdrift.Characters;

/// <summary>
///     彷徨者专属卡池。卡牌/遗物/药水池共用 EnergyColorName，保证能量图标与主题一致。
///     暂无自定义资源：能量图标等全部回退原版；主题色 #f3e5f5。
/// </summary>
public sealed class TheAdriftCardPool : TypeListCardPoolModel
{
    private static readonly Material? PoolFrameTintMaterial =
        MaterialUtils.CreateRgbShaderMaterial(0.9529f, 0.8980f, 0.9608f);

    public override string Title => "TheAdrift";
    public override string EnergyColorName => "TheAdrift";

    // 能量图标：描述内 24x24、tooltip/卡角 74x74
    public override string? TextEnergyIconPath => $"{Entry.ResPath}/images/ui/energy_icon_24.png";
    public override string? BigEnergyIconPath => $"{Entry.ResPath}/images/ui/energy_icon_74.png";

    public override Color DeckEntryCardColor => TheAdriftCharacter.ThemeColor;
    public override Color EnergyOutlineColor => new(0.32f, 0.24f, 0.42f);
    public override Material? PoolFrameMaterial => PoolFrameTintMaterial;

    public override bool IsColorless => false;
}
