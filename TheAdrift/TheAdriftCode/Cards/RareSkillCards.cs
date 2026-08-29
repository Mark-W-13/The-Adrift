using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;
using TheAdrift.Characters;
using TheAdrift.Common;
using TheAdrift.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TheAdrift.Cards;

// ============================== 稀有技能 ==============================

/// <summary>填海 —— 把抽牌堆或弃牌堆中最多 1(2) 张诅咒牌变化为巨石。打出你抽牌堆中所有的巨石。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class FillingTheSea  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Magic", 1)
    ];

    public FillingTheSea() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var max = (int)DynamicVars["Magic"].BaseValue;
        var drawPile = CardPile.Get(PileType.Draw, Owner);
        var discardPile = CardPile.Get(PileType.Discard, Owner);

        var targets = drawPile.Cards.Concat(discardPile.Cards)
            .Where(c => c.Type == CardType.Curse)
            .Take(max)
            .ToList();
        foreach (var curse in targets)
            await CardUtils.TransformInto<GiantRock>(choiceContext, curse);

        // 打出抽牌堆中所有的巨石
        var enemies = CardUtils.Enemies(Owner).ToList();
        var firstEnemy = enemies.FirstOrDefault();
        if (firstEnemy is not null)
        {
            foreach (var boulder in drawPile.Cards.Where(c => c is GiantRock).ToList())
                await CardCmd.AutoPlay(choiceContext, boulder, firstEnemy, AutoPlayType.Default, true, true);
        }
    }

    protected override void OnUpgrade() => DynamicVars["Magic"].UpgradeValueBy(1);
}

/// <summary>天地银行 —— X 费：获得 20 金币 X 次，本场战斗结束时失去 20 金币 X 次。消耗。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class HeavenAndEarthBank  : AdriftCardTemplate
{
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Magic", 1)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public HeavenAndEarthBank() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var x = ResolveEnergyXValue();
        for (var i = 0; i < x; i++)
            await CardUtils.GainGold(choiceContext, Owner, 20);
        if (x > 0)
        {
            GoldTracker.RegisterBankDebt(Owner, x * 20);
            await PowerCmd.Apply<BankDebtPower>(choiceContext, [Owner.Creature], 1, Owner.Creature, this);
        }
    }
}

/// <summary>默示录 —— 手牌每有 1 张诅咒，获得 1 费并抽 1 张牌。立即触发手牌所有诅咒的回合结束效果（，并将它们消耗）。消耗。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Apocalypse  : AdriftCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public Apocalypse() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hand = CardUtils.Hand(Owner);
        if (hand is null) return;
        var curses = hand.Where(c => c.Type == CardType.Curse).ToList();

        foreach (var _ in curses)
        {
            await PlayerCmd.GainEnergy(1, Owner);
            await CardPileCmd.Draw(choiceContext, 1, Owner);
        }

        foreach (var curse in curses)
        {
            if (curse.HasTurnEndInHandEffect)
                await curse.OnTurnEndInHandWrapper(choiceContext);
            // 只有升级后才会消耗诅咒（设计文档：括号内「并将它们消耗」为升级内容）
            if (IsUpgraded)
                await CardCmd.Exhaust(choiceContext, curse, false, false);
        }
    }
}

/// <summary>班级觉悟 —— 获得 30 减去当前金币数的格挡。消耗（升级后去消耗）。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class ClassConsciousness  : AdriftCardTemplate
{
    public override bool GainsBlock => true;

    /// <summary>参考原版「蜃景」：CalculatedBlockVar 动态计算格挡并实时预览（金币变化时更新）。</summary>
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedBlockVar(ValueProp.Move).WithMultiplier((card, _) =>
            Math.Max(0, 30 - (card.Owner?.Gold ?? 0)))
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public ClassConsciousness() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.CalculatedBlock.Calculate(cardPlay.Target),
            DynamicVars.CalculatedBlock.Props, cardPlay);
    }

    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);
}

/// <summary>不万能的喜剧 —— 获得 20(25) 点格挡。向手牌加入 1 张凡庸。当你被凡庸束缚时，把这张卡放在抽牌堆顶端。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class FlawedComedy  : AdriftCardTemplate
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(20m, ValueProp.Move)];

    public FlawedComedy() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<Normality>());
        await PowerCmd.Apply<FlawedComedyPower>(choiceContext, [Owner.Creature], 1, Owner.Creature, this);
        if (FlawedComedyPower.IsBound(Owner))
            await FlawedComedyPower.PutComedyOnTop(choiceContext, Owner, this);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(5m);
}

/// <summary>延命治疗 —— 恢复 10(13) 点生命值。向手牌加入 1 张悔恨。消耗。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class LifeExtension  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [ModCardVars.Heal(10)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public LifeExtension() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Heal(Owner.Creature, IsUpgraded ? 13 : 10);
        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<Regret>());
    }

    protected override void OnUpgrade() => DynamicVars.Heal.UpgradeValueBy(3m);
}

/// <summary>克服 —— 获得 7(10) 点格挡。这回合不受脆弱、虚弱、易伤的影响。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Overcome  : AdriftCardTemplate
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7m, ValueProp.Move)];

    public Overcome() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<OvercomePower>(choiceContext, [Owner.Creature], 1, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>大游行 —— 这回合每打出 1 张牌获得 1 金币，每抽到 1 张诅咒牌获得 3 金币。战斗结束时金币变成一半。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class GrandParade  : AdriftCardTemplate
{
    public GrandParade() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<GrandParadePower>(choiceContext, [Owner.Creature], 1, Owner.Creature, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>从头越 —— 消耗所有手牌。向手牌加入 4 张打击、4 张防御和 1 张愚行。永恒。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class StartAnew  : AdriftCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Eternal];

    public StartAnew() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hand = CardUtils.Hand(Owner);
        if (hand is not null)
        {
            foreach (var card in hand.Where(c => c != this).ToList())
                await CardCmd.Exhaust(choiceContext, card, false, false);
        }

        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<TheAdriftStrike>(), 4, upgraded: IsUpgraded);
        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<TheAdriftDefend>(), 4, upgraded: IsUpgraded);
        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<Folly>());
    }
}

/// <summary>受国之垢（多人）—— 另一名玩家选择自己任意数量的诅咒牌，加入你的手牌。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class BearTheNationsFilth  : AdriftCardTemplate
{
    public BearTheNationsFilth() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 简化实现：取其他玩家手牌中的所有诅咒牌加入自己手牌（正式版应让玩家选择）
        foreach (var player in Owner.RunState.Players.Where(p => p != Owner))
        {
            var hand = CardUtils.Hand(player);
            if (hand is null) continue;
            foreach (var curse in hand.Where(c => c.Type == CardType.Curse).ToList())
            {
                await CardPileCmd.Add(curse, PileType.Hand, CardPilePosition.Top, this, false);
            }
        }
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>走向明天（多人）—— 给其他所有玩家的手牌加入 1 张睡眠不佳。下回合，其他玩家获得 1 费并抽 1(2) 张牌。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class TowardTomorrow  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(1)
    ];

    public TowardTomorrow() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var player in Owner.RunState.Players.Where(p => p != Owner))
        {
            await CardUtils.AddToHand(choiceContext, player, CardUtils.Canonical<PoorSleep>());
            await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, [player.Creature], 1, Owner.Creature, this);
            await PowerCmd.Apply<DrawCardsNextTurnPower>(choiceContext, [player.Creature],
                IsUpgraded ? 2 : 1, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1);
}

/// <summary>尖塔中的圣诞快乐（多人）—— 失去 5 金币。另一名玩家升级 1 张（全部）手牌。向你的手牌加入 1 张羞耻。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class SpireChristmas  : AdriftCardTemplate
{
    public SpireChristmas() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardUtils.LoseGold(choiceContext, Owner, 5);
        var other = Owner.RunState.Players.FirstOrDefault(p => p != Owner);
        if (other is not null)
        {
            // 与登神同款：未升级由另一名玩家选择 1 张升级，升级后升级全部可升级牌
            if (IsUpgraded)
            {
                var hand = CardUtils.Hand(other);
                if (hand is { Count: > 0 })
                {
                    foreach (var card in hand.Where(c => c.IsUpgradable))
                        CardCmd.Upgrade(card);
                }
            }
            else
            {
                var card = await CardSelectCmd.FromHandForUpgrade(choiceContext, other, this);
                if (card is not null)
                    CardCmd.Upgrade(card);
            }
        }

        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<Shame>());
    }
}

/// <summary>黑线 —— 为手牌的 1 张牌附魔「完美契合」（洗牌后这张牌总会在牌堆顶）。消耗（升级后去消耗）。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class BlackLine  : AdriftCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public BlackLine() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 注意：CardSelectorPrefs 无参默认 MinSelect=MaxSelect=0，会导致选择界面无法选牌卡死；
        // 必须显式指定选择 1 张 + 附魔提示
        var selected = (await CardSelectCmd.FromHand(choiceContext, Owner,
            new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1),
            c => c != this, this)).FirstOrDefault();
        if (selected is not null)
            CardCmd.Enchant<PerfectFit>(selected, 1);
    }

    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);
}
