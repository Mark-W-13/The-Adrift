using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using TheAdrift.Common;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TheAdrift.Powers;

/// <summary>历史终结的计数能力：记录本场战斗打出过的打击/防御次数。</summary>
[RegisterPower]
public sealed class EndOfHistoryCounterPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    private int _count;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public static int GetCount(Creature creature)
        => creature.GetPower<EndOfHistoryCounterPower>() is { } p ? p._count : 0;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Player.Creature == Owner &&
            (cardPlay.Card.Tags.Contains(CardTag.Strike) || cardPlay.Card.Tags.Contains(CardTag.Defend)))
            _count++;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        // 战斗结束时自动移除（临时能力语义）
    }
}

/// <summary>冰红茶（药水）：接下来 5 个回合开始时获得 4 金币。</summary>
[RegisterPower]
public sealed class IcedBlackTeaPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    private int _turnsRemaining = 5;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        if (_turnsRemaining <= 0) return;
        _turnsRemaining--;
        await PlayerCmd.GainGold(4, player, true);
        if (_turnsRemaining <= 0)
            await PowerCmd.Remove<IcedBlackTeaPower>(Owner);
    }
}

/// <summary>南北绿豆浆（药水）/ 尖塔闪购（能力）：战斗结束后额外获得一次卡牌奖励。</summary>
[RegisterPower]
public sealed class ExtraCardRewardPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterCombatVictory(MegaCrit.Sts2.Core.Rooms.CombatRoom room)
    {
        var player = Owner.Player;
        await RewardsCmd.OfferCustom(player,
            [new CardReward(CardCreationOptions.ForNonCombatWithDefaultOdds([player.Character.CardPool]), 3, player)]);
        await PowerCmd.Remove<ExtraCardRewardPower>(Owner);
    }
}

/// <summary>天地银行：本场战斗结束时失去 20X 金币。</summary>
[RegisterPower]
public sealed class BankDebtPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterCombatEnd(MegaCrit.Sts2.Core.Rooms.CombatRoom room)
    {
        var player = Owner.Player;
        var debt = GoldTracker.ConsumeBankDebt(player);
        if (debt > 0)
            await PlayerCmd.LoseGold(debt, player, MegaCrit.Sts2.Core.Entities.Gold.GoldLossType.Lost);
        await PowerCmd.Remove<BankDebtPower>(Owner);
    }
}
