using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace TheAdrift.Common;

/// <summary>
///     通用卡牌工具：引用原版牌、生成牌实例加入各类牌堆。
///     原版诅咒/特殊牌（羞耻、疑虑、巨石等）全部复用，不新建。
/// </summary>
internal static class CardUtils
{
    /// <summary>按类型获取原版/本 mod 卡牌的 canonical 模型。</summary>
    public static CardModel Canonical<T>() where T : CardModel
        => ModelDb.AllCards.First(c => c is T);

    /// <summary>获取 player 当前战斗的 CombatState（不在战斗中返回 null）。</summary>
    public static ICombatState? CombatOf(Player player)
        => player.Creature.CombatState;

    /// <summary>战斗中：向手牌加入 count 张指定牌（可选升级）。</summary>
    public static async Task AddToHand(PlayerChoiceContext ctx, Player player, CardModel canonical,
        int count = 1, bool upgraded = false)
    {
        var combat = CombatOf(player);
        if (combat is null) return;
        for (var i = 0; i < count; i++)
        {
            var card = combat.CreateCard(canonical, player);
            if (upgraded) CardCmd.Upgrade(card, CardPreviewStyle.None);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
        }
    }

    /// <summary>战斗中：向弃牌堆加入 count 张指定牌（可选升级）。</summary>
    public static async Task AddToDiscard(PlayerChoiceContext ctx, Player player, CardModel canonical,
        int count = 1, bool upgraded = false)
    {
        var combat = CombatOf(player);
        if (combat is null) return;
        for (var i = 0; i < count; i++)
        {
            var card = combat.CreateCard(canonical, player);
            if (upgraded) CardCmd.Upgrade(card, CardPreviewStyle.None);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Discard, player);
        }
    }

    /// <summary>战斗中：向抽牌堆加入 count 张指定牌（可选升级）。</summary>
    public static async Task AddToDrawPile(PlayerChoiceContext ctx, Player player, CardModel canonical,
        int count = 1, bool upgraded = false)
    {
        var combat = CombatOf(player);
        if (combat is null) return;
        for (var i = 0; i < count; i++)
        {
            var card = combat.CreateCard(canonical, player);
            if (upgraded) CardCmd.Upgrade(card, CardPreviewStyle.None);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, player);
        }
    }

    /// <summary>局外：向卡组加入 count 张指定牌（可选升级）。</summary>
    public static async Task AddToDeck(PlayerChoiceContext ctx, Player player, CardModel canonical,
        int count = 1, bool upgraded = false)
    {
        for (var i = 0; i < count; i++)
        {
            var card = player.RunState.CreateCard(canonical, player);
            if (upgraded) CardCmd.Upgrade(card, CardPreviewStyle.None);
            await CardPileCmd.Add(card, PileType.Deck);
        }
    }

    /// <summary>把一张手牌变化为指定 canonical（保留升级状态可选）。</summary>
    public static async Task TransformInto(PlayerChoiceContext ctx, CardModel card, CardModel canonical)
        => await CardCmd.Transform(card, canonical, CardPreviewStyle.None);

    /// <summary>把一张手牌变化为 T 类型。</summary>
    public static async Task TransformInto<T>(PlayerChoiceContext ctx, CardModel card) where T : CardModel
        => await CardCmd.Transform(card, Canonical<T>(), CardPreviewStyle.None);

    /// <summary>把一张手牌变化为 T 类型的升级版（如 巨石+）。</summary>
    public static async Task TransformIntoUpgraded<T>(PlayerChoiceContext ctx, CardModel card, Player player)
        where T : CardModel
    {
        var combat = CombatOf(player);
        var target = combat is not null
            ? combat.CreateCard(Canonical<T>(), player)
            : player.RunState.CreateCard(Canonical<T>(), player);
        CardCmd.Upgrade(target, CardPreviewStyle.None);
        await CardCmd.Transform(card, target, CardPreviewStyle.None);
    }

    /// <summary>获得金币（金色漂浮文本）。</summary>
    public static async Task GainGold(PlayerChoiceContext ctx, Player player, int amount)
        => await PlayerCmd.GainGold(amount, player, true);

    /// <summary>失去金币。</summary>
    public static async Task LoseGold(PlayerChoiceContext ctx, Player player, int amount)
        => await PlayerCmd.LoseGold(amount, player, GoldLossType.Lost);

    /// <summary>战斗中所有可被攻击的敌人。</summary>
    public static IEnumerable<Creature> Enemies(Player player)
        => CombatOf(player)?.HittableEnemies ?? [];

    /// <summary>对所有敌人造成一次攻击伤害。</summary>
    public static async Task AttackAllEnemies(PlayerChoiceContext ctx, Player player, CardModel sourceCard, CardPlay cardPlay, decimal damage)
    {
        foreach (var enemy in Enemies(player))
        {
            await DamageCmd.Attack(damage)
                .FromCard(sourceCard, cardPlay)
                .Targeting(enemy)
                .Execute(ctx);
        }
    }

    /// <summary>手牌牌堆。</summary>
    public static CardPile? HandPile(Player player)
        => CardPile.Get(PileType.Hand, player);

    /// <summary>手牌列表。</summary>
    public static IReadOnlyList<CardModel>? Hand(Player player)
        => HandPile(player)?.Cards;

    /// <summary>手牌中的诅咒牌数量。</summary>
    public static int CurseCountInHand(Player player)
        => Hand(player)?.Count(c => c.Type == CardType.Curse) ?? 0;

    /// <summary>手牌中的第一张诅咒牌。</summary>
    public static CardModel? FirstCurseInHand(Player player)
        => Hand(player)?.FirstOrDefault(c => c.Type == CardType.Curse);
}
