using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using TheAdrift.Characters;
using TheAdrift.Common;
using TheAdrift.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using VoidCard = MegaCrit.Sts2.Core.Models.Cards.Void;

namespace TheAdrift.Potions;

/// <summary>冰红茶 —— 接下来的 5 个回合开始时，获得 4 金币。</summary>
[RegisterPotion(typeof(TheAdriftPotionPool))]
public sealed class IcedBlackTea : ModPotionTemplate
{
    public override PotionRarity Rarity => PotionRarity.Common;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    public override PotionAssetProfile AssetProfile => new(
        ImagePath: $"{Entry.ResPath}/images/potions/{GetType().Name}.png",
        OutlinePath: $"{Entry.ResPath}/images/potions/{GetType().Name}.png");

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        await PowerCmd.Apply<IcedBlackTeaPower>(choiceContext, [Owner.Creature], 1, Owner.Creature, null);
    }
}

/// <summary>万艳同杯 —— 用虚无填满你的手牌。</summary>
[RegisterPotion(typeof(TheAdriftPotionPool))]
public sealed class WanyanSameCup : ModPotionTemplate
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    public override PotionAssetProfile AssetProfile => new(
        ImagePath: $"{Entry.ResPath}/images/potions/{GetType().Name}.png",
        OutlinePath: $"{Entry.ResPath}/images/potions/{GetType().Name}.png");

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var hand = CardUtils.Hand(Owner);
        var toFill = 10 - (hand?.Count ?? 0);
        if (toFill > 0)
            await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<VoidCard>(), toFill);
    }
}

/// <summary>南北绿豆浆 —— 战斗结束时，额外获得 1 次卡牌奖励。</summary>
[RegisterPotion(typeof(TheAdriftPotionPool))]
public sealed class SouthNorthGreenSoyMilk : ModPotionTemplate
{
    public override PotionRarity Rarity => PotionRarity.Rare;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    public override PotionAssetProfile AssetProfile => new(
        ImagePath: $"{Entry.ResPath}/images/potions/{GetType().Name}.png",
        OutlinePath: $"{Entry.ResPath}/images/potions/{GetType().Name}.png");

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        await PowerCmd.Apply<ExtraCardRewardPower>(choiceContext, [Owner.Creature], 1, Owner.Creature, null);
    }
}
