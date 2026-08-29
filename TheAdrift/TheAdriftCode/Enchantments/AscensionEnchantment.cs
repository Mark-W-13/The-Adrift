using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TheAdrift.Enchantments;

/// <summary>
///     附魔「升格」：这场战斗中每次打出这张牌，就将其升级。这张牌可以多次升级。
/// </summary>
[RegisterEnchantment]
public sealed class AscensionEnchantment : ModEnchantmentTemplate
{
    public override bool ShowAmount => false;

    public override EnchantmentAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        if (Card.IsUpgraded && Card.CurrentUpgradeLevel >= Card.MaxUpgradeLevel)
            return;
        CardCmd.Upgrade(Card, CardPreviewStyle.None);
    }
}
