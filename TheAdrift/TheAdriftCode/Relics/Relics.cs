using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using TheAdrift.Characters;
using TheAdrift.Common;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TheAdrift.Relics;

/// <summary>
///     搬史群 —— 初始遗物：第一个卡牌奖励将是被升级过的；若拾取，向卡组加入 1 张随机诅咒。
/// </summary>
[RegisterRelic(typeof(TheAdriftRelicPool))]
[RegisterCharacterStarterRelic(typeof(TheAdriftCharacter))]
public sealed class BanShiQun : ModRelicTemplate
{
    private bool _firstRewardDone;

    public override RelicRarity Rarity => RelicRarity.Starter;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png");

    /// <summary>第一个卡牌奖励 100% 升级。</summary>
    public override decimal ModifyCardRewardUpgradeOdds(Player player, CardModel card, decimal odds)
    {
        if (!_firstRewardDone)
            return 1m;
        return odds;
    }

    /// <summary>拾取卡牌奖励后：加入随机诅咒，并标记首次完成。</summary>
    public override async Task AfterRewardTaken(Player player, Reward reward)
    {
        if (reward is not CardReward) return;
        if (!_firstRewardDone)
        {
            _firstRewardDone = true;
            await CardPileCmd.AddCurseToDeck<Shame>(player);
        }
    }
}

/// <summary>智人TV —— 拾起时获得 3 次升级过的卡牌奖励；若拾取，向卡组加入 1 张随机诅咒。</summary>
[RegisterRelic(typeof(TheAdriftRelicPool))]
public sealed class ZhiRenTV : ModRelicTemplate
{
    private int _remaining = 3;

    public override RelicRarity Rarity => RelicRarity.Starter;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png");

    public override decimal ModifyCardRewardUpgradeOdds(Player player, CardModel card, decimal odds)
    {
        if (_remaining > 0)
            return 1m;
        return odds;
    }

    public override async Task AfterRewardTaken(Player player, Reward reward)
    {
        if (reward is not CardReward) return;
        if (_remaining > 0)
        {
            _remaining--;
            await CardPileCmd.AddCurseToDeck<Shame>(player);
        }
    }
}

/// <summary>卡斯特拉蛋糕 —— 每次被施加负面状态时，获得 3 点格挡。</summary>
[RegisterRelic(typeof(TheAdriftRelicPool))]
public sealed class CastellaCake : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png");

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (amount > 0 && power.Type == PowerType.Debuff && power.Owner == Owner.Creature)
            await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(2m, ValueProp.Move), null);
    }
}

/// <summary>低保 —— 以 0 金币进入商店时，获得 200 金币。</summary>
[RegisterRelic(typeof(TheAdriftRelicPool))]
public sealed class Dibao : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png");

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is MerchantRoom && Owner.Gold <= 0)
            await PlayerCmd.GainGold(200, Owner, true);
    }
}

/// <summary>猪排饭 —— 虚弱只会使你的攻击伤害变为 90%，脆弱只会使你获得的格挡变为 90%。</summary>
/// <remarks>
///     原版虚弱/脆弱各提供 0.75 倍乘区，这里补偿为 0.9 倍。
///     若后续版本调整了原版乘区数值，同步修改 <see cref="VanillaStatusMultiplier" />。
/// </remarks>
[RegisterRelic(typeof(TheAdriftRelicPool))]
public sealed class PorkChopRice : ModRelicTemplate
{
    // 原版虚弱/脆弱的伤害/格挡乘区（STS2 与 STS1 一致）
    private const decimal VanillaStatusMultiplier = 0.75m;
    private const decimal TargetMultiplier = 0.9m;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png");

    public override decimal ModifyDamageMultiplicative(Creature target, decimal damage, ValueProp props,
        Creature attacker, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target == Owner.Creature && target.GetPowerAmount<WeakPower>() > 0)
            return damage * (TargetMultiplier / VanillaStatusMultiplier);
        return damage;
    }

    public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props,
        CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target == Owner.Creature && target.GetPowerAmount<FrailPower>() > 0)
            return block * (TargetMultiplier / VanillaStatusMultiplier);
        return block;
    }
}

/// <summary>荒唐石 —— 每场战斗前两次受到伤害时，向手牌放入 1 张免费巨石。</summary>
[RegisterRelic(typeof(TheAdriftRelicPool))]
public sealed class AbsurdStone : ModRelicTemplate
{
    private int _damageTaken;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png");

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature attacker, CardModel? cardSource)
    {
        if (target != Owner.Creature || result.TotalDamage <= 0 || _damageTaken >= 2) return;
        _damageTaken++;
        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<GiantRock>());
    }

    public override async Task BeforeCombatStart()
    {
        _damageTaken = 0;
    }
}

/// <summary>男子汉的骰子 —— 每场战斗开始时，把你的 2 张永恒牌加入手牌。</summary>
[RegisterRelic(typeof(TheAdriftRelicPool))]
public sealed class GentlemanDice : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png");

    public override async Task BeforeCombatStart()
    {
        var drawPile = CardPile.Get(PileType.Draw, Owner);
        var eternal = drawPile.Cards.Where(c => c.Keywords.Contains(CardKeyword.Eternal)).Take(2).ToList();
        foreach (var card in eternal)
        {
            var result = await CardPileCmd.Add(card, PileType.Hand, CardPilePosition.Top, this, false);
        }
    }
}

/// <summary>终末之花 —— 打出打击或防御时，抽 1 张牌。</summary>
[RegisterRelic(typeof(TheAdriftRelicPool))]
public sealed class EndFlower : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Shop;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/{GetType().Name}.png");

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Player != Owner) return;
        if (cardPlay.Card.Tags.Contains(CardTag.Strike) || cardPlay.Card.Tags.Contains(CardTag.Defend))
            await CardPileCmd.Draw(choiceContext, 1, Owner);
    }
}
