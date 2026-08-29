using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TheAdrift.Cards;
using TheAdrift.Common;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TheAdrift.Powers;

/// <summary>巨石伤害增益（补天）。层数 = 巨石获得的额外伤害。</summary>
[RegisterPower]
public sealed class BoulderDamagePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public static decimal GetAmount(Creature creature)
        => creature.GetPowerAmount<BoulderDamagePower>();
}

/// <summary>本回合攻击命中计数（福音）。施加给玩家，记录本回合对每个目标的攻击次数。</summary>
[RegisterPower]
public sealed class AttackHitTrackerPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    // 目标 -> 命中次数
    private readonly Dictionary<Creature, int> _hits = new(ReferenceEqualityComparer.Instance);

    public static int GetHitCount(Creature owner, Creature target)
        => owner.GetPower<AttackHitTrackerPower>() is { } p && p._hits.TryGetValue(target, out var n) ? n : 0;

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature attacker, CardModel? cardSource)
    {
        if (result.TotalDamage > 0 && attacker == Owner)
            _hits[target] = _hits.GetValueOrDefault(target) + 1;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        // 我方回合结束清零
        if (side == Owner.Side)
            _hits.Clear();
    }
}

/// <summary>关于地球的运动 —— 每抽 5(4) 张牌获得 1 费并加入 1 张疑虑。</summary>
[RegisterPower]
public sealed class MotionsOfEarthPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    private int _drawn;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        _drawn++;
        var threshold = Amount;
        if (_drawn >= threshold)
        {
            _drawn = 0;
            await PlayerCmd.GainEnergy(1, Owner.Player);
            await CardUtils.AddToDiscard(choiceContext, Owner.Player, CardUtils.Canonical<Doubt>());
        }
    }
}

/// <summary>消费主义 —— 每次失去金币，获得 1 点力量和 1 点敏捷。由 GoldTracker 触发。</summary>
[RegisterPower]
public sealed class ConsumerismPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public static void OnGoldLost(PlayerChoiceContext ctx, Creature owner)
    {
        if (owner.GetPower<ConsumerismPower>() is null) return;
        _ = PowerCmd.Apply<StrengthPower>(ctx, [owner], 1, owner, null);
        _ = PowerCmd.Apply<DexterityPower>(ctx, [owner], 1, owner, null);
    }
}

/// <summary>幽体离脱 —— 每抽到 1 张诅咒牌，获得 7(10) 点格挡。</summary>
[RegisterPower]
public sealed class AstralProjectionPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;


    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Type != CardType.Curse) return;
        await CreatureCmd.GainBlock(Owner, new BlockVar(Amount, ValueProp.Move), null);
    }
}

/// <summary>神爱世人 —— 每次获得负面状态，对所有敌人造成 6(9) 点伤害。</summary>
[RegisterPower]
public sealed class GodLovesTheWorldPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;


    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (amount <= 0 || power.Owner != Owner || power.Type != PowerType.Debuff) return;
        foreach (var enemy in CardUtils.Enemies(Owner.Player))
            await CreatureCmd.Damage(choiceContext, [enemy], Amount, ValueProp.Move, Owner);
    }
}

/// <summary>西西弗斯 —— 每回合开始时，把 1 张巨石加入手牌并向弃牌堆加入 1 张愚行。</summary>
[RegisterPower]
public sealed class SisyphusPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        await CardUtils.AddToHand(choiceContext, player, CardUtils.Canonical<GiantRock>(), upgraded: Amount > 0);
        await CardUtils.AddToDiscard(choiceContext, player, CardUtils.Canonical<Folly>());
    }
}

/// <summary>世界之夜 —— 1 回合内抽到第 3 张诅咒牌时，抽 2(3) 张牌并消耗 2 张手牌。</summary>
[RegisterPower]
public sealed class NightOfTheWorldPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    private int _cursesThisTurn;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Type != CardType.Curse) return;
        _cursesThisTurn++;
        if (_cursesThisTurn < 3) return;
        _cursesThisTurn = 0;
        var player = Owner.Player;
        await CardPileCmd.Draw(choiceContext, Amount, player);
        var hand = CardUtils.Hand(player);
        if (hand is null) return;
        var toExhaust = hand.Take(2).ToList();
        foreach (var c in toExhaust)
            await CardCmd.Exhaust(choiceContext, c, false, false);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == Owner.Side)
            _cursesThisTurn = 0;
    }
}

/// <summary>石头记 —— 巨石获得保留；每打出 1 张巨石抽 1(2) 张牌。</summary>
[RegisterPower]
public sealed class StoryOfTheStonePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
    {
        if (card is GiantRock)
            keywords.Add(CardKeyword.Retain);
        return false;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card is not GiantRock) return;
        var player = Owner.Player;
        await CardPileCmd.Draw(choiceContext, Amount, player);
    }
}

/// <summary>大石碎胸口 —— 每次敌人受到你 15 点以上攻击伤害，本回合失去 5(7) 点力量。</summary>
[RegisterPower]
public sealed class RockToTheChestPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    private const decimal Threshold = 15m;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature attacker, CardModel? cardSource)
    {
        if (attacker != Owner || result.TotalDamage < Threshold) return;
        var player = Owner.Player;
        await PowerCmd.Apply<TemporaryStrengthPower>(choiceContext, [Owner],
            -Amount, Owner, null);
    }
}

/// <summary>电瓶车 —— 回合开始时获得 5 金币。</summary>
[RegisterPower]
public sealed class ElectricBikePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        await PlayerCmd.GainGold(Amount, player, true);
    }
}

/// <summary>异化 —— 每回合开始时把抽牌堆 2 张打击/防御放到卡组顶端。</summary>
[RegisterPower]
public sealed class AlienationPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        var drawPile = CardPile.Get(PileType.Draw, player);
        var candidates = drawPile.Cards
            .Where(c => c.Tags.Contains(CardTag.Strike) || c.Tags.Contains(CardTag.Defend))
            .Take(2)
            .ToList();
        var pile = CardPile.Get(PileType.Draw, player);
        foreach (var card in candidates)
            pile.MoveToTopInternal(card);
    }
}

/// <summary>恶性无限 —— 每生成 1 张诅咒牌，抽 1 张牌。</summary>
[RegisterPower]
public sealed class MalignantInfinityPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterCardGeneratedForCombat(CardModel card, Player player)
    {
        if (player.Creature != Owner || card.Type != CardType.Curse) return;
        await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), 1, player);
    }
}

/// <summary>背叛之夜 —— 回合结束时向手牌加入 1 张羞耻。</summary>
[RegisterPower]
public sealed class NightOfBetrayalPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return;
        await CardUtils.AddToHand(choiceContext, Owner.Player, CardUtils.Canonical<Shame>());
    }
}

/// <summary>咖啡因 —— 回合开始时获得 7(10) 点格挡并加入 1 张睡眠不佳。</summary>
[RegisterPower]
public sealed class CaffeinePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        await CreatureCmd.GainBlock(Owner, new BlockVar(Amount, ValueProp.Move), null);
        await CardUtils.AddToHand(choiceContext, player, CardUtils.Canonical<PoorSleep>());
    }
}

/// <summary>无敌之人 —— 金币为 0 时造成伤害 +50%(+100%)。</summary>
[RegisterPower]
public sealed class InvinciblePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override decimal ModifyDamageMultiplicative(Creature target, decimal damage, ValueProp props,
        Creature attacker, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (attacker == Owner && Owner.Player.Gold <= 0)
            return damage * ((100m + Amount) / 100m);
        return damage;
    }
}

/// <summary>象征形态 —— 诅咒牌可打出（消耗 1 费）；打出诅咒牌获得 5(8) 点格挡并抽 1 张牌。</summary>
[RegisterPower]
public sealed class SymbolicFormPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>诅咒牌可以打出：移除 Unplayable 关键词。</summary>
    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
    {
        if (card.Type == CardType.Curse)
            keywords.Remove(CardKeyword.Unplayable);
        return false;
    }

    /// <summary>诅咒牌消耗 1 费。</summary>
    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal cost)
    {
        cost = card.Type == CardType.Curse ? 1m : originalCost;
        return false;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type != CardType.Curse) return;
        var player = Owner.Player;
        await CreatureCmd.GainBlock(Owner, new BlockVar(Amount, ValueProp.Move), null);
        await CardPileCmd.Draw(choiceContext, 1, player);
    }
}

/// <summary>阵亡形态 —— 生命值将要降到 0 或以下时，消耗全部诅咒牌并恢复等量生命（1 次）。</summary>
[RegisterPower]
public sealed class DeathFormPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    private bool _used;
    private bool _prevented;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    public override decimal ModifyHpLostAfterOstyLate(Creature target, decimal damage, ValueProp props,
        Creature attacker, CardModel? cardSource)
    {
        if (_used || target != Owner || damage <= 0) return damage;
        if (target.CurrentHp - damage <= 0)
        {
            _used = true;
            _prevented = true;
            return 0;
        }
        return damage;
    }

    public override async Task AfterPreventingDeath(Creature target)
    {
        if (target != Owner || !_prevented) return;
        _prevented = false;
        var player = Owner.Player;
        var exhausted = 0;
        foreach (var pileType in new[] { PileType.Hand, PileType.Draw, PileType.Discard })
        {
            var pile = CardPile.Get(pileType, player);
            if (pile is null) continue;
            foreach (var card in pile.Cards.Where(c => c.Type == CardType.Curse).ToList())
            {
                await CardCmd.Exhaust(new ThrowingPlayerChoiceContext(), card, false, false);
                exhausted++;
            }
        }

        if (exhausted > 0)
            await CreatureCmd.Heal(Owner, exhausted);
    }
}

/// <summary>不万能的喜剧的束缚监听：手牌存在凡庸（被束缚）时，把不万能的喜剧放到抽牌堆顶端。</summary>
[RegisterPower]
public sealed class FlawedComedyPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png",
        BigIconPath: $"{Entry.ResPath}/images/powers/{GetType().Name}.png");

    /// <summary>手牌中是否存在凡庸（即被凡庸束缚）。</summary>
    public static bool IsBound(Player player)
        => CardUtils.Hand(player)?.Any(c => c is Normality) ?? false;

    /// <summary>把玩家场上的不万能的喜剧放到抽牌堆顶端。</summary>
    public static async Task PutComedyOnTop(PlayerChoiceContext choiceContext, Player player, AbstractModel source)
    {
        var comedy = CardPile.Get(PileType.Deck, player).Cards.OfType<FlawedComedy>().FirstOrDefault();
        if (comedy is null) return;
        await CardPileCmd.Add(comedy, PileType.Draw, CardPilePosition.Top, source, false);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner || !IsBound(player)) return;
        await PutComedyOnTop(choiceContext, player, this);
    }

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card is not Normality) return;
        var player = Owner.Player;
        if (player is null) return;
        await PutComedyOnTop(choiceContext, player, this);
    }
}
