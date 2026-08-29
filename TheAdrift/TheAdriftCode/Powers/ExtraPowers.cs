using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using TheAdrift.Common;
using TheAdrift.Enchantments;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TheAdrift.Powers;

/// <summary>克服 —— 本回合不受脆弱、虚弱、易伤的影响。</summary>
[RegisterPower]
public sealed class OvercomePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override decimal ModifyPowerAmountGivenMultiplicative(PowerModel power, Creature target,
        decimal amount, Creature source, CardModel? cardSource)
    {
        if (target == Owner && power is VulnerablePower or WeakPower or FrailPower)
            return 0m;
        return amount;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == Owner.Side)
            await PowerCmd.Remove<OvercomePower>(Owner);
    }
}

/// <summary>大游行 —— 本回合每打出 1 张牌 +1 金币，每抽到诅咒 +3 金币；战斗结束时金币减半。</summary>
[RegisterPower]
public sealed class GrandParadePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Player.Creature == Owner)
            await PlayerCmd.GainGold(1, cardPlay.Player, true);
    }

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Type == CardType.Curse)
            await PlayerCmd.GainGold(3, Owner.Player, true);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == Owner.Side)
            await PowerCmd.Remove<GrandParadePower>(Owner);
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        var player = Owner.Player;
        await PlayerCmd.SetGold(player.Gold / 2, player);
    }
}

/// <summary>生活在树上 —— 被凡庸束缚时获得 1 层无实体；回合结束时消耗手牌的凡庸。</summary>
[RegisterPower]
public sealed class LivingInTheTreesPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        // 被凡庸束缚：抽到凡庸时获得 1 层无实体
        if (card is Normality)
            await PowerCmd.Apply<IntangiblePower>(choiceContext, [Owner], 1, Owner, null);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return;
        var player = Owner.Player;
        var hand = CardUtils.Hand(player);
        if (hand is null) return;
        foreach (var card in hand.Where(c => c is Normality).ToList())
            await CardCmd.Exhaust(choiceContext, card, false, false);
    }
}

/// <summary>升格 —— 打击/防御获得附魔「升格」（每打出一次升级一次）。</summary>
[RegisterPower]
public sealed class AscensionPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        if (card.Tags.Contains(CardTag.Strike) || card.Tags.Contains(CardTag.Defend))
            // 用泛型 Enchant 走 ModelDb canonical + ToMutable 标准路径（直接 new 实例会因 Id 未注册而无法生效）
            CardCmd.Enchant<AscensionEnchantment>(card, 1);
    }
}

/// <summary>采石（多人）目标标记 —— 本回合内所有玩家对该敌人的每次攻击，都向攻击者手牌加入 1 张巨石。</summary>
[RegisterPower]
public sealed class QuarryTargetPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature attacker, CardModel? cardSource)
    {
        if (result.TotalDamage <= 0 || attacker.Player is null) return;
        await CardUtils.AddToHand(choiceContext, attacker.Player, CardUtils.Canonical<GiantRock>());
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == Owner.Side)
            await PowerCmd.Remove<QuarryTargetPower>(Owner);
    }
}
