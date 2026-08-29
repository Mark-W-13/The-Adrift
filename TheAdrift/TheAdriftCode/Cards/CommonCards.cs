using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TheAdrift.Characters;
using TheAdrift.Common;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TheAdrift.Cards;

// ============================== 普通攻击 ==============================

/// <summary>嗟怨 —— 手牌每有 1 张诅咒，造成 7(9) 点伤害。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Grievance  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7, ValueProp.Move)];

    public Grievance() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var total = DynamicVars.Damage.BaseValue * CardUtils.CurseCountInHand(Owner);
        await DamageCmd.Attack(total).FromCard(this, cardPlay).Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}

/// <summary>进阶打击 —— 造成 8(12) 点伤害。当前每有 1 级进阶，额外造成 1 点伤害。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class AscensionStrike  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8, ValueProp.Move)];

    protected override HashSet<CardTag> CanonicalTags => new() { CardTag.Strike };

    public AscensionStrike() : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var total = DynamicVars.Damage.BaseValue + Owner.RunState.AscensionLevel;
        await DamageCmd.Attack(total).FromCard(this, cardPlay).Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4);
}

/// <summary>爆米 —— 对所有敌人造成 10(13) 点伤害。失去 5 金币。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Popcorn  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Move)];

    public Popcorn() : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardUtils.AttackAllEnemies(choiceContext, Owner, this, cardPlay, DynamicVars.Damage.BaseValue);
        await CardUtils.LoseGold(choiceContext, Owner, 5);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
}

/// <summary>原生创伤 —— 造成 12(15) 点伤害。永恒。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class PrimalWound  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Eternal];

    public PrimalWound() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
}

/// <summary>读博 —— 造成 8(11) 点伤害，再造成随机 1-6 点伤害。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Roulette  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8, ValueProp.Move)];

    public Roulette() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).Execute(choiceContext);
        var bonus = Owner.RunState.Rng.CombatTargets.NextInt(6) + 1;
        await DamageCmd.Attack(bonus).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
}

/// <summary>透支 —— 造成 12(13) 点伤害。本回合失去 4(3) 点力量。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Overdraft  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12, ValueProp.Move),
        ModCardVars.Int("Magic", 4)
    ];

    public Overdraft() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).Execute(choiceContext);
        await PowerCmd.Apply<TemporaryStrengthPower>(choiceContext, [Owner.Creature],
            -DynamicVars["Magic"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
        DynamicVars["Magic"].UpgradeValueBy(-1);
    }
}

/// <summary>毁伤 —— 造成 6(9) 点伤害。若自己有脆弱，给予对方 1 层易伤。若自己有虚弱，给予对方 1 层虚弱。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Ruin  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move)];

    public Ruin() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).Execute(choiceContext);
        if (Owner.Creature.GetPowerAmount<FrailPower>() > 0)
            await PowerCmd.Apply<VulnerablePower>(choiceContext, [cardPlay.Target], 1, Owner.Creature, this);
        if (Owner.Creature.GetPowerAmount<WeakPower>() > 0)
            await PowerCmd.Apply<WeakPower>(choiceContext, [cardPlay.Target], 1, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
}

/// <summary>奋斗 —— 造成 21(26) 点伤害。下回合少抽 2 张牌。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Strive  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(21, ValueProp.Move)];

    public Strive() : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).Execute(choiceContext);
        await PowerCmd.Apply<DrawCardsNextTurnPower>(choiceContext, [Owner.Creature], -2, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(5);
}

/// <summary>变形记 —— 造成 8(11) 点伤害。这张牌在攀登 3 层后变化为巨石。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Metamorphosis  : AdriftCardTemplate
{
    private int? _transformAtFloor;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8, ValueProp.Move)];

    public Metamorphosis() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).Execute(choiceContext);
    }

    /// <summary>回合结束仍在手中时检查：爬满 3 层后变化为巨石（+）。</summary>
    protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    {
        _transformAtFloor ??= Owner.RunState.TotalFloor + 3;
        if (Owner.RunState.TotalFloor >= _transformAtFloor)
            await CardUtils.TransformInto<GiantRock>(choiceContext, this);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
}

/// <summary>伤仲永 —— 对所有敌人造成 11(13) 点伤害，获得 11(13) 点格挡。这张牌变化为凡庸。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class FallenProdigy  : AdriftCardTemplate
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(11, ValueProp.Move),
        new BlockVar(11m, ValueProp.Move)
    ];

    public FallenProdigy() : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardUtils.AttackAllEnemies(choiceContext, Owner, this, cardPlay, DynamicVars.Damage.BaseValue);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await CardUtils.TransformInto<Normality>(choiceContext, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars.Block.UpgradeValueBy(2m);
    }
}

// ============================== 普通技能 ==============================

/// <summary>登神 —— 升级你手牌中的 1(全部) 张牌。向你的手牌加入 1 张进阶之灾。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Ascend  : AdriftCardTemplate
{
    public Ascend() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 参考原版「武装」：未升级弹选择界面选 1 张，升级后升级全部可升级牌（带升级动画预览）
        if (IsUpgraded)
        {
            var hand = CardUtils.Hand(Owner);
            if (hand is { Count: > 0 })
            {
                foreach (var card in hand.Where(c => c.IsUpgradable))
                    CardCmd.Upgrade(card);
            }
        }
        else
        {
            var card = await CardSelectCmd.FromHandForUpgrade(choiceContext, Owner, this);
            if (card is not null)
                CardCmd.Upgrade(card);
        }

        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<AscendersBane>());
    }
}

/// <summary>捷径 —— 获得 8(10) 点格挡。获得 5(7) 金币。向你的手牌加入 1 张腐朽。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Shortcut  : AdriftCardTemplate
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(8m, ValueProp.Move),
        ModCardVars.Gold(5)
    ];

    public Shortcut() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await CardUtils.GainGold(choiceContext, Owner, DynamicVars.Gold.IntValue);
        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<Decay>());
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars.Gold.UpgradeValueBy(2);
    }
}

/// <summary>我正在忘了你 —— 把 2 张手牌变化为打击和防御。永恒。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class ForgettingYou  : AdriftCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Eternal];

    public ForgettingYou() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 弹选择界面选 2 张手牌（按选择顺序：第 1 张变化为打击，第 2 张变化为防御）
        var chosen = (await CardSelectCmd.FromHand(choiceContext, Owner,
            new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 2),
            c => c != this, this)).ToList();
        if (chosen.Count > 0)
        {
            if (IsUpgraded)
                await CardUtils.TransformIntoUpgraded<TheAdriftStrike>(choiceContext, chosen[0], Owner);
            else
                await CardUtils.TransformInto<TheAdriftStrike>(choiceContext, chosen[0]);
        }

        if (chosen.Count > 1)
        {
            if (IsUpgraded)
                await CardUtils.TransformIntoUpgraded<TheAdriftDefend>(choiceContext, chosen[1], Owner);
            else
                await CardUtils.TransformInto<TheAdriftDefend>(choiceContext, chosen[1]);
        }

        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<Decay>());
    }
}

/// <summary>悲伤年代 —— 给予所有敌人 1(2) 层易伤。手牌有诅咒牌时，再给予 1 层虚弱。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class SadEra  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Magic", 1)
    ];

    public SadEra() : base(1, CardType.Skill, CardRarity.Common, TargetType.AllEnemies) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var enemies = CardUtils.Enemies(Owner).ToList();
        await PowerCmd.Apply<VulnerablePower>(choiceContext, enemies, DynamicVars["Magic"].BaseValue, Owner.Creature, this);
        if (CardUtils.CurseCountInHand(Owner) > 0)
            await PowerCmd.Apply<WeakPower>(choiceContext, enemies, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Magic"].UpgradeValueBy(1);
}

/// <summary>清流 —— 获得 6(9) 点格挡。消耗手牌中的 1 张诅咒牌。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class ClearStream  : AdriftCardTemplate
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6m, ValueProp.Move)];

    public ClearStream() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        // 「消耗手牌中的 1 张诅咒牌」→ 弹选择界面（非随机）
        var curse = (await CardSelectCmd.FromHand(choiceContext, Owner,
            new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1),
            c => c.Type == CardType.Curse, this)).FirstOrDefault();
        if (curse is not null)
            await CardCmd.Exhaust(choiceContext, curse, false, false);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>威力贷 —— 失去 5 金币。抽 1(2) 张牌。金币为 0 时，向你的手牌加入 1 张债务。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class PowerLoan  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(1)
    ];

    public PowerLoan() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardUtils.LoseGold(choiceContext, Owner, 5);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
        if (Owner.Gold <= 0)
            await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<Debt>());
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1);
}

/// <summary>褪黑素 —— 向你的手牌加入 1 张睡眠不佳。下回合获得 2 费，抽 1(2) 张牌。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Melatonin  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(1)
    ];

    public Melatonin() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<PoorSleep>());
        await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, [Owner.Creature], 2, Owner.Creature, this);
        await PowerCmd.Apply<DrawCardsNextTurnPower>(choiceContext, [Owner.Creature], DynamicVars.Cards.IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1);
}

/// <summary>劳役 —— 获得 11(14) 点格挡。获得 5(7) 金币。向你的手牌加入 1 张睡眠不佳。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Toil  : AdriftCardTemplate
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(11m, ValueProp.Move),
        ModCardVars.Gold(5)
    ];

    public Toil() : base(2, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await CardUtils.GainGold(choiceContext, Owner, DynamicVars.Gold.IntValue);
        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<PoorSleep>());
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
        DynamicVars.Gold.UpgradeValueBy(2);
    }
}

/// <summary>旅途愉快 —— 获得 10(13) 点格挡。本回合失去 6(5) 点敏捷。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class PleasantJourney  : AdriftCardTemplate
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(10m, ValueProp.Move),
        ModCardVars.Int("Magic", 6)
    ];

    public PleasantJourney() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<TemporaryDexterityPower>(choiceContext, [Owner.Creature],
            -DynamicVars["Magic"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
        DynamicVars["Magic"].UpgradeValueBy(-1);
    }
}
