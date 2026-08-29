using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TheAdrift.Characters;
using TheAdrift.Common;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace TheAdrift.Cards;

/// <summary>打击 —— 初始攻击牌。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
[RegisterCharacterStarterCard(typeof(TheAdriftCharacter), 4)]
public sealed class TheAdriftStrike  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move)];

    protected override HashSet<CardTag> CanonicalTags => new() { CardTag.Strike };

    public TheAdriftStrike() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
}

/// <summary>防御 —— 初始技能牌。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
[RegisterCharacterStarterCard(typeof(TheAdriftCharacter), 4)]
public sealed class TheAdriftDefend  : AdriftCardTemplate
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5m, ValueProp.Move)];

    protected override HashSet<CardTag> CanonicalTags => new() { CardTag.Defend };

    public TheAdriftDefend() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}

/// <summary>回想 —— 初始技能：易伤+虚弱+羞耻。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
[RegisterCharacterStarterCard(typeof(TheAdriftCharacter), 1)]
public sealed class Recall  : AdriftCardTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Magic", 1),
        ModCardVars.Int("Magic2", 1)
    ];

    public Recall() : base(1, CardType.Skill, CardRarity.Basic, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, [cardPlay.Target], 1, Owner.Creature, this);
        await PowerCmd.Apply<WeakPower>(choiceContext, [cardPlay.Target], 1, Owner.Creature, this);
        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<Shame>());
    }

    protected override void OnUpgrade()
    {
        // 升级后 0 费
        EnergyCost.UpgradeBy(-1);
    }
}

/// <summary>逡巡 —— 初始技能：格挡+疑虑。</summary>
[RegisterCard(typeof(TheAdriftCardPool))]
[RegisterCharacterStarterCard(typeof(TheAdriftCharacter), 1)]
public sealed class Linger  : AdriftCardTemplate
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6m, ValueProp.Move)];

    public Linger() : base(0, CardType.Skill, CardRarity.Basic, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<Doubt>());
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(2m);
}
