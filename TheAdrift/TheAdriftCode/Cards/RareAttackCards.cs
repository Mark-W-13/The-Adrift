using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TheAdrift.Characters;
using TheAdrift.Common;
using TheAdrift.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;
using STS2RitsuLib.Scaffolding.Content;

namespace TheAdrift.Cards;

// ============================== 稀有攻击 ==============================

/// <summary>落阳 —— 12 费。造成 75(99) 点伤害。无论何处，你每有 1 张诅咒牌，这张牌的耗能就减少 1。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
[RegisterDefaultModelCapability(typeof(SettingSunCost))]
public sealed class SettingSun  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(75, ValueProp.Move)];

    public SettingSun() : base(12, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(24);
}

/// <summary>落阳减费能力：你拥有的诅咒牌数量（卡组+手牌+牌堆）。</summary>
[RegisterModelCapability]
public sealed class SettingSunCost : CardCapability, ICardEnergyCostContributor
{
    public int ModifyEnergyCost(CardModel card, int currentCost, CostModifiers modifiers)
    {
        var owner = card.Owner;
        if (owner is null) return currentCost;
        var count = CardPile.Get(PileType.Deck, owner).Cards.Count(c => c.Type == CardType.Curse)
                    + CardUtils.CurseCountInHand(owner);
        return currentCost - count;
    }
}

/// <summary>末世论 —— 造成 10 点伤害。你每有 1 个负面状态，这张卡的伤害就增加 10(13)。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Eschatology  : AdriftCardTemplate
{
    private const int BaseDamage = 10;
    private const int StatusBonus = 10;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(BaseDamage, ValueProp.Move)];

    public Eschatology() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var debuffs = Owner.Creature.Powers.Count(p => p.Type == PowerType.Debuff);
        var total = DynamicVars.Damage.BaseValue + debuffs * StatusBonus;
        await DamageCmd.Attack(total).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
}

/// <summary>摘下苹果光束 —— 固有。本局游戏每拾取过 1 次卡牌奖励，造成 1 次 5(6) 点伤害。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class PickAppleLightBeam  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Magic", 5)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];

    public PickAppleLightBeam() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var times = GoldTracker.GetRewardsPicked(Owner);
        var damage = IsUpgraded ? 6 : 5;
        for (var i = 0; i < times; i++)
        {
            await DamageCmd.Attack(damage).FromCard(this, cardPlay)
                .Targeting(cardPlay.Target).Execute(choiceContext);
        }
    }

    protected override void OnUpgrade() => DynamicVars["Magic"].UpgradeValueBy(1);
}

/// <summary>逆行 —— 保留。造成 7(9) 点伤害，给予攻击对象你的负面状态（脆弱转为易伤）。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Reversal  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    public Reversal() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).Execute(choiceContext);

        var me = Owner.Creature;
        foreach (var power in me.Powers.Where(p => p.Type == PowerType.Debuff).ToList())
        {
            if (power is VulnerablePower)
                await PowerCmd.Apply<VulnerablePower>(choiceContext, [cardPlay.Target], power.Amount, Owner.Creature, this);
            else if (power is WeakPower)
                await PowerCmd.Apply<WeakPower>(choiceContext, [cardPlay.Target], power.Amount, Owner.Creature, this);
            else if (power is FrailPower)
                await PowerCmd.Apply<VulnerablePower>(choiceContext, [cardPlay.Target], power.Amount, Owner.Creature, this);
            await PowerCmd.Apply(choiceContext, power, me, -power.Amount, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}

/// <summary>经济危机 —— 对所有敌人造成 26(30) 点伤害。用债务填满你的手牌。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class EconomicCrisis  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(26, ValueProp.Move)];

    public EconomicCrisis() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardUtils.AttackAllEnemies(choiceContext, Owner, this, cardPlay, DynamicVars.Damage.BaseValue);

        var hand = CardUtils.Hand(Owner);
        if (hand is null) return;
        var toFill = 10 - hand.Count;
        if (toFill > 0)
            await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<Debt>(), toFill);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4);
}

/// <summary>谁点燃了世界 —— 失去所有金币。本场战斗中金币每增加或减少过 1 次，造成 1 次 5(6) 点伤害。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class WhoLitTheWorld  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Magic", 5)
    ];

    public WhoLitTheWorld() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var changes = GoldTracker.GetCombatGoldChanges(Owner);
        var damage = IsUpgraded ? 6 : 5;
        for (var i = 0; i < changes; i++)
        {
            await DamageCmd.Attack(damage).FromCard(this, cardPlay)
                .Targeting(cardPlay.Target).Execute(choiceContext);
        }

        await PlayerCmd.SetGold(0, Owner);
    }

    protected override void OnUpgrade() => DynamicVars["Magic"].UpgradeValueBy(1);
}

/// <summary>历史终结 —— 造成 6(7) 点伤害。本场战斗每打出过一次打击或防御，这张牌的伤害就增加 4(5) 点。永恒。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class EndOfHistory  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Eternal];

    public EndOfHistory() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) { }

    /// <summary>进入战斗时挂上计数能力（打击/防御打出次数）。</summary>
    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        if (card == this)
            await PowerCmd.Apply<EndOfHistoryCounterPower>(new ThrowingPlayerChoiceContext(), [Owner.Creature], 1, Owner.Creature, this);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var played = EndOfHistoryCounterPower.GetCount(Owner.Creature);
        var bonus = (IsUpgraded ? 5 : 4) * played;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + bonus).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(1);
}

/// <summary>苦旅 —— 固有。造成等同于你已攀爬过楼层的伤害。消耗。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class BitterJourney  : AdriftCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate, CardKeyword.Exhaust];

    public BitterJourney() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var damage = Owner.RunState.TotalFloor;
        await DamageCmd.Attack(damage).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>采石（多人）—— 造成 6 点伤害。这回合，所有玩家每攻击过 1 次该敌人，把 1 张巨石加入你的手牌。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Quarry  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move)];

    public Quarry() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).Execute(choiceContext);
        // 本回合内所有玩家对该敌人的每次攻击 -> 1 张巨石（简化：由 QuarryTargetPower 记录）
        await PowerCmd.Apply<QuarryTargetPower>(choiceContext, [cardPlay.Target], 0, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(1);
}
