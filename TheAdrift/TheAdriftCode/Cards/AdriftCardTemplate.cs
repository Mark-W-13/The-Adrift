using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Scaffolding.Content;

namespace TheAdrift.Cards;

/// <summary>
///     彷徨者卡牌基类：统一从 <c>images/cards/{类名}.png</c> 加载卡面图。
///     全部卡牌继承本类即可获得自定义卡图；无对应图片时回退 RitsuLib 内嵌占位。
/// </summary>
public abstract class AdriftCardTemplate : ModCardTemplate
{
    protected AdriftCardTemplate(int energyCost, CardType type, CardRarity rarity, TargetType targetType)
        : base(energyCost, type, rarity, targetType) { }

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/{GetType().Name}.png");
}
