using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;
using TheAdrift.Characters;
using TheAdrift.Common;
using TheAdrift.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;
using STS2RitsuLib.Scaffolding.Content;

namespace TheAdrift.Cards;

// ============================== 罕见攻击 ==============================

/// <summary>鏖杀 —— 造成 8(9) 点伤害。给予 2(3) 层易伤。向你的弃牌堆加入 1 张受伤。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Slaughter  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8, ValueProp.Move),
        ModCardVars.Int("Magic", 2)
    ];

    public Slaughter() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).Execute(choiceContext);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, [cardPlay.Target],
            DynamicVars["Magic"].BaseValue, Owner.Creature, this);
        await CardUtils.AddToDiscard(choiceContext, Owner, CardUtils.Canonical<Wound>());
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
        DynamicVars["Magic"].UpgradeValueBy(1);
    }
}

/// <summary>原石 —— 造成 7(8) 点伤害。这张牌变化为巨石（+）。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class RawStone  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7, ValueProp.Move)];

    public RawStone() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).Execute(choiceContext);
        if (IsUpgraded)
            await CardUtils.TransformIntoUpgraded<GiantRock>(choiceContext, this, Owner);
        else
            await CardUtils.TransformInto<GiantRock>(choiceContext, this);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(1);
}

/// <summary>野蛮不眠 —— 对所有敌人造成 21(26) 点伤害。你的手牌每有 1 张诅咒牌，这张牌的耗能就减少 1。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
[RegisterDefaultModelCapability(typeof(SavageInsomniaCost))]
public sealed class SavageInsomnia  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(21, ValueProp.Move)];

    public SavageInsomnia() : base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardUtils.AttackAllEnemies(choiceContext, Owner, this, cardPlay, DynamicVars.Damage.BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(5);
}

/// <summary>野蛮不眠的减费能力：手牌每有 1 张诅咒牌，耗能减少 1。</summary>
[RegisterModelCapability]
public sealed class SavageInsomniaCost : CardCapability, ICardEnergyCostContributor
{
    public int ModifyEnergyCost(CardModel card, int currentCost, CostModifiers modifiers)
    {
        var owner = card.Owner;
        if (owner is null) return currentCost;
        return currentCost - CardUtils.CurseCountInHand(owner);
    }
}

/// <summary>耻辱柱 —— 造成 10(13) 点伤害。给予 2(3) 层虚弱。向你的手牌加入 1 张羞耻。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Pillory  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10, ValueProp.Move),
        ModCardVars.Int("Magic", 2)
    ];

    public Pillory() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).Execute(choiceContext);
        await PowerCmd.Apply<WeakPower>(choiceContext, [cardPlay.Target],
            DynamicVars["Magic"].BaseValue, Owner.Creature, this);
        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<Shame>());
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars["Magic"].UpgradeValueBy(1);
    }
}

/// <summary>脆刃之剑 —— 造成 10(12) 点伤害。若你虚弱，额外造成 2 次伤害。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class BrittleBlade  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Move)];

    public BrittleBlade() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var hits = Owner.Creature.GetPowerAmount<WeakPower>() > 0 ? 3 : 1;
        for (var i = 0; i < hits; i++)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
                .Targeting(cardPlay.Target).Execute(choiceContext);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}

/// <summary>福音 —— 造成 10(13) 点伤害。这回合你每击中过该敌人 1 次，便获得 1 金币。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Gospel  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Move)];

    public Gospel() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var hits = AttackHitTrackerPower.GetHitCount(Owner.Creature, cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).Execute(choiceContext);
        if (hits > 0)
            await CardUtils.GainGold(choiceContext, Owner, hits);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
}

/// <summary>申冤 —— 造成 10(13) 点伤害。抽牌直到你抽到诅咒牌为止。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Appeal  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Move)];

    public Appeal() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).Execute(choiceContext);

        var drawPile = CardPile.Get(PileType.Draw, Owner);
        while (CardUtils.CurseCountInHand(Owner) == 0 && drawPile.Cards.Count > 0)
        {
            await CardPileCmd.Draw(choiceContext, 1, Owner);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
}

/// <summary>地雷 —— 造成 30(34) 点伤害。下回合少获得 1 费。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Landmine  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(30, ValueProp.Move)];

    public Landmine() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).Execute(choiceContext);
        await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, [Owner.Creature], -1, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4);
}

/// <summary>补天 —— 造成 6(8) 点伤害。你的所有巨石伤害增加本次造成的伤害数值。向你的手牌加入 1 张愚行。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class SkyMending  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move)];

    public SkyMending() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var bonus = BoulderDamagePower.GetAmount(Owner.Creature);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + bonus).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).Execute(choiceContext);
        await PowerCmd.Apply<BoulderDamagePower>(choiceContext, [Owner.Creature],
            DynamicVars.Damage.BaseValue + bonus, Owner.Creature, this);
        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<Folly>());
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}

/// <summary>成义 —— 造成 30(35) 点伤害。向你的弃牌堆加入 1 张进阶之灾。你每有 1 张未升级的牌，这张卡的伤害就减少 1 点。永恒。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class RighteousDeed  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(30, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Eternal];

    public RighteousDeed() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var unupgraded = CardPile.Get(PileType.Deck, Owner).Cards.Count(c => !c.IsUpgraded);
        var total = Math.Max(0, DynamicVars.Damage.BaseValue - unupgraded);
        await DamageCmd.Attack(total).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).Execute(choiceContext);
        await CardUtils.AddToDiscard(choiceContext, Owner, CardUtils.Canonical<AscendersBane>());
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(5);
}

/// <summary>圣战的可打出限制：手牌中不得有诅咒牌、升级过的牌以外的牌。</summary>
[RegisterModelCapability]
public sealed class HolyWarPlayState : CardCapability, ICardPlayStateContributor
{
    public bool? CanPlay(CardModel card)
    {
        if (card.Owner is null) return null;
        var hand = CardUtils.Hand(card.Owner);
        if (hand is not null && hand.Any(c => c != card && c.Type != CardType.Curse && !c.IsUpgraded))
            return false;
        return null;
    }
}

/// <summary>圣战 —— 对所有敌人造成 18(20) 点伤害。抽 1(2) 张牌。这张牌只在你的手牌没有诅咒牌、升级过的牌以外的牌时才能打出。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
[RegisterDefaultModelCapability(typeof(HolyWarPlayState))]
public sealed class HolyWar  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(18, ValueProp.Move),
        ModCardVars.Cards(1)
    ];

    public HolyWar() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardUtils.AttackAllEnemies(choiceContext, Owner, this, cardPlay, DynamicVars.Damage.BaseValue);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
