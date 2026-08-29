using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using VoidCard = MegaCrit.Sts2.Core.Models.Cards.Void;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TheAdrift.Characters;
using TheAdrift.Common;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TheAdrift.Cards;

// ============================== 罕见技能 ==============================

/// <summary>审核 —— 把卡组中最多 2(3) 张诅咒牌放入弃牌堆，并抽出那个数量。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Audit  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Magic", 2)
    ];

    public Audit() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 参考原版「冲锋」：从抽牌堆中选择诅咒牌（最多 Magic 张），弃掉后抽等量
        var drawPile = CardPile.Get(PileType.Draw, Owner);
        var max = (int)DynamicVars["Magic"].BaseValue;
        var selection = (await CardSelectCmd.FromCombatPile(choiceContext, drawPile, Owner,
            new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 0, max),
            c => c.Type == CardType.Curse)).ToList();

        foreach (var curse in selection)
            await CardCmd.Discard(choiceContext, curse);

        if (selection.Count > 0)
            await CardPileCmd.Draw(choiceContext, selection.Count, Owner);
    }

    protected override void OnUpgrade() => DynamicVars["Magic"].UpgradeValueBy(1);
}

/// <summary>进阶防御 —— 获得 6 点格挡。当前每有 1 级进阶，额外获得 1 点格挡。消耗（升级后去消耗）。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class AscensionDefend  : AdriftCardTemplate
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6m, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public AscensionDefend() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var block = DynamicVars.Block.BaseValue + Owner.RunState.AscensionLevel;
        await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(block, ValueProp.Move), cardPlay);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

/// <summary>退行 —— 获得 7(9) 点格挡。若你脆弱，额外获得 2 次格挡。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Regression  : AdriftCardTemplate
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7m, ValueProp.Move)];

    public Regression() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var times = Owner.Creature.GetPowerAmount<FrailPower>() > 0 ? 3 : 1;
        for (var i = 0; i < times; i++)
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(2m);
}

/// <summary>犹大之银 —— 获得 30 金币。向你的手牌加入 1 张悔恨。消耗。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class JudasSilver  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [ModCardVars.Gold(30)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public JudasSilver() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardUtils.GainGold(choiceContext, Owner, DynamicVars.Gold.IntValue);
        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<Regret>());
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>愚公移山 —— 把手牌中的 1 张诅咒牌变化为巨石（+），向你的弃牌堆加入 1 张愚行。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class MovingMountains  : AdriftCardTemplate
{
    public MovingMountains() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 「手牌中的 1 张诅咒牌」→ 弹选择界面（非随机）
        var curse = (await CardSelectCmd.FromHand(choiceContext, Owner,
            new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1),
            c => c.Type == CardType.Curse, this)).FirstOrDefault();
        if (curse is not null)
        {
            if (IsUpgraded)
                await CardUtils.TransformIntoUpgraded<GiantRock>(choiceContext, curse, Owner);
            else
                await CardUtils.TransformInto<GiantRock>(choiceContext, curse);
        }

        await CardUtils.AddToDiscard(choiceContext, Owner, CardUtils.Canonical<Folly>());
    }
}

/// <summary>万里长城建造时 —— 获得 15(18) 点格挡。把 1 张巨石（+）和 1 张愚行加入你的手牌。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class BuildingTheGreatWall  : AdriftCardTemplate
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(15m, ValueProp.Move)];

    public BuildingTheGreatWall() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<GiantRock>(), upgraded: IsUpgraded);
        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<Folly>());
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>月亮与六便士 —— 获得 3(4) 费和 6 金币。向你的手牌加入 1 张凡庸。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class MoonAndSixpence  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Energy(3),
        ModCardVars.Gold(6)
    ];

    public MoonAndSixpence() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
        await CardUtils.GainGold(choiceContext, Owner, DynamicVars.Gold.IntValue);
        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<Normality>());
    }

    protected override void OnUpgrade() => DynamicVars.Energy.UpgradeValueBy(1);
}

/// <summary>叫魂 —— 向你的弃牌堆加入 1 张执迷。抽 3(4) 张牌。消耗（升级后去消耗）。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class SummonSpirit  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(3)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public SummonSpirit() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardUtils.AddToDiscard(choiceContext, Owner, CardUtils.Canonical<Enthralled>());
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

/// <summary>再教育 —— 把你手牌中所有未升级的牌变化为愚行。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Reeducation  : AdriftCardTemplate
{
    public Reeducation() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hand = CardUtils.Hand(Owner);
        if (hand is null) return;
        foreach (var card in hand.Where(c => !c.IsUpgraded).ToList())
            await CardUtils.TransformInto<Folly>(choiceContext, card);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>时代的晚上 —— 向你的手牌加入 1 张羞耻。获得等同于你脆弱层数 2 倍的格挡。消耗（升级后去消耗）。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class EveningOfAnEra  : AdriftCardTemplate
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Magic", 2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public EveningOfAnEra() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<Shame>());
        var block = Owner.Creature.GetPowerAmount<FrailPower>() * DynamicVars["Magic"].BaseValue;
        await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(block, ValueProp.Move), cardPlay);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

/// <summary>前途 —— X 费：获得 7X(+1) 点格挡。下回合获得 X(+1) 费。向你的手牌加入 1 张疑虑。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Future  : AdriftCardTemplate
{
    public override bool GainsBlock => true;
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7m, ValueProp.Move)];

    public Future() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var x = ResolveEnergyXValue();
        await CreatureCmd.GainBlock(Owner.Creature,
            new BlockVar(DynamicVars.Block.BaseValue * x + (IsUpgraded ? 1 : 0), ValueProp.Move), cardPlay);
        await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, [Owner.Creature], x + (IsUpgraded ? 1 : 0), Owner.Creature, this);
        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<Doubt>());
    }

    protected override void OnUpgrade() { }
}

/// <summary>质朴 —— 升级你的全部打击和防御。永恒。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Simplicity  : AdriftCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Eternal];

    public Simplicity() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var deck = CardPile.Get(PileType.Deck, Owner).Cards;
        if (deck is null) return;
        foreach (var card in deck.Where(c =>
                     c.Tags.Contains(CardTag.Strike) || c.Tags.Contains(CardTag.Defend)))
            CardCmd.Upgrade(card, CardPreviewStyle.None);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>石之海（多人）—— 向所有玩家的手牌中加入 1 张虚无和 1 张愚行。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class SeaOfStones  : AdriftCardTemplate
{
    public SeaOfStones() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var player in Owner.RunState.Players)
        {
            await CardUtils.AddToHand(choiceContext, player, CardUtils.Canonical<VoidCard>());
            await CardUtils.AddToHand(choiceContext, player, CardUtils.Canonical<Folly>());
        }
    }
}

/// <summary>饼与鱼（多人）—— 保留。其他所有玩家获得 5 金币和 2 费。向你的手牌加入 1 张债务。消耗。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class LoavesAndFishes  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [ModCardVars.Gold(5)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain, CardKeyword.Exhaust];

    public LoavesAndFishes() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var player in Owner.RunState.Players.Where(p => p != Owner))
        {
            await CardUtils.GainGold(choiceContext, player, DynamicVars.Gold.IntValue);
            await PlayerCmd.GainEnergy(2, player);
        }

        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<Debt>());
    }

    protected override void OnUpgrade() => DynamicVars.Gold.UpgradeValueBy(2);
}
