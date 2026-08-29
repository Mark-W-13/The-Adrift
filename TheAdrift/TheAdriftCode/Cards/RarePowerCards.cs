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

// ============================== 稀有能力 ==============================

/// <summary>恶性无限 —— 你每生成 1 张诅咒牌，就抽 1 张牌。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class MalignantInfinity  : AdriftCardTemplate
{
    public MalignantInfinity() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<MalignantInfinityPower>(choiceContext, [Owner.Creature], 1, Owner.Creature, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>背叛之夜 —— 获得 1(2) 层无实体。每个回合结束时向手牌加入 1 张羞耻。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class NightOfBetrayal  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Magic", 1)
    ];

    public NightOfBetrayal() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<IntangiblePower>(choiceContext, [Owner.Creature],
            IsUpgraded ? 2 : 1, Owner.Creature, this);
        await PowerCmd.Apply<NightOfBetrayalPower>(choiceContext, [Owner.Creature], 1, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Magic"].UpgradeValueBy(1);
}

/// <summary>咖啡因 —— 回合开始时获得 7(10) 点格挡并向手牌加入 1 张睡眠不佳。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Caffeine  : AdriftCardTemplate
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7m, ValueProp.Move)];

    public Caffeine() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<CaffeinePower>(choiceContext, [Owner.Creature],
            IsUpgraded ? 10 : 7, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>生活在树上 —— 向手牌加入 1 张凡庸。当你被凡庸束缚时获得 1 层无实体。回合结束时消耗手牌的凡庸。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class LivingInTheTrees  : AdriftCardTemplate
{
    public LivingInTheTrees() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<Normality>());
        await PowerCmd.Apply<LivingInTheTreesPower>(choiceContext, [Owner.Creature], 1, Owner.Creature, this);
    }
}

/// <summary>无敌之人 —— 当你的金币为 0 时，造成的伤害增加 50%(100%)。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class TheInvincible  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Magic", 50)
    ];

    public TheInvincible() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<InvinciblePower>(choiceContext, [Owner.Creature],
            IsUpgraded ? 100 : 50, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Magic"].UpgradeValueBy(50);
}

/// <summary>尖塔闪购 —— 失去 33 金币。本场战斗结束后额外获得一次卡牌奖励。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class SpireFlashSale  : AdriftCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    public SpireFlashSale() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardUtils.LoseGold(choiceContext, Owner, 33);
        await PowerCmd.Apply<ExtraCardRewardPower>(choiceContext, [Owner.Creature], 1, Owner.Creature, this);
    }
}

/// <summary>升格 —— 你拥有和生成的打击、防御获得附魔升格。永恒。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class Ascension  : AdriftCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Eternal];

    public Ascension() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<AscensionPower>(choiceContext, [Owner.Creature], 1, Owner.Creature, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>象征形态 —— 你可以消耗 1 费打出原本不能打出的诅咒牌。打出诅咒牌时获得 5(8) 点格挡并抽 1 张牌。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class SymbolicForm  : AdriftCardTemplate
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5m, ValueProp.Move)];

    public SymbolicForm() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SymbolicFormPower>(choiceContext, [Owner.Creature],
            IsUpgraded ? 8 : 5, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}
