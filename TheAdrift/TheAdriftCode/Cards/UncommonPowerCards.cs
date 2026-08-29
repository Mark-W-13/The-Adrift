using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TheAdrift.Characters;
using TheAdrift.Common;
using TheAdrift.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TheAdrift.Cards;

// ============================== 罕见能力 ==============================

/// <summary>关于地球的运动 —— 每抽 5(4) 张牌，获得 1 费并向弃牌堆加入 1 张疑虑。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class OnTheMotionsOfEarth  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Magic", 5)
    ];

    public OnTheMotionsOfEarth() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<MotionsOfEarthPower>(choiceContext, [Owner.Creature],
            IsUpgraded ? 4 : 5, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Magic"].UpgradeValueBy(-1);
}

/// <summary>消费主义 —— 固有。向手牌加入 1 张债务。每次失去金币，获得 1 点力量和 1 点敏捷。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Consumerism  : AdriftCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];

    public Consumerism() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ConsumerismPower>(choiceContext, [Owner.Creature], 1, Owner.Creature, this);
        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<Debt>());
    }
}

/// <summary>幽体离脱 —— 每抽到 1 张诅咒牌，获得 7(10) 点格挡。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class AstralProjection  : AdriftCardTemplate
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7m, ValueProp.Move)];

    public AstralProjection() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<AstralProjectionPower>(choiceContext, [Owner.Creature],
            IsUpgraded ? 10 : 7, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>神爱世人 —— 每次获得负面状态，对所有敌人造成 3(5) 点伤害。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class GodLovesTheWorld  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(3, ValueProp.Move)];

    public GodLovesTheWorld() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<GodLovesTheWorldPower>(choiceContext, [Owner.Creature],
            IsUpgraded ? 5 : 3, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}

/// <summary>西西弗斯 —— 每回合开始时，把 1 张巨石（+）加入手牌并向弃牌堆加入 1 张愚行。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Sisyphus  : AdriftCardTemplate
{
    public Sisyphus() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SisyphusPower>(choiceContext, [Owner.Creature], 1, Owner.Creature, this);
    }
}

/// <summary>世界之夜 —— 1 回合内抽到第 3 张诅咒牌时，抽 2(3) 张牌并消耗 2 张手牌。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class NightOfTheWorld  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(2)
    ];

    public NightOfTheWorld() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<NightOfTheWorldPower>(choiceContext, [Owner.Creature],
            IsUpgraded ? 3 : 2, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1);
}

/// <summary>石头记 —— 巨石获得保留；每打出 1 张巨石抽 1(2) 张牌。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class StoryOfTheStone  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(1)
    ];

    public StoryOfTheStone() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StoryOfTheStonePower>(choiceContext, [Owner.Creature],
            IsUpgraded ? 2 : 1, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1);
}

/// <summary>大石碎胸口 —— 每次敌人受到你 15 点以上攻击伤害，本回合失去 5(7) 点力量。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class RockToTheChest  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Magic", 15)
    ];

    public RockToTheChest() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<RockToTheChestPower>(choiceContext, [Owner.Creature],
            IsUpgraded ? 7 : 5, Owner.Creature, this);
    }

    // 升级只提升失去的力量值（5->7），伤害阈值 15 不变
}

/// <summary>电瓶车 —— 固有。回合开始时获得 5(7) 金币。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class ElectricBike  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [ModCardVars.Gold(5)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];

    public ElectricBike() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ElectricBikePower>(choiceContext, [Owner.Creature],
            IsUpgraded ? 7 : 5, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Gold.UpgradeValueBy(2);
}

/// <summary>异化 —— 获得 2(3) 点力量和 2(3) 点敏捷。每回合开始时把抽牌堆 2 张打击/防御放到卡组顶端。永恒。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Alienation  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Magic", 2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Eternal];

    public Alienation() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var amount = IsUpgraded ? 3 : 2;
        await PowerCmd.Apply<StrengthPower>(choiceContext, [Owner.Creature], amount, Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(choiceContext, [Owner.Creature], amount, Owner.Creature, this);
        await PowerCmd.Apply<AlienationPower>(choiceContext, [Owner.Creature], 1, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Magic"].UpgradeValueBy(1);
}
