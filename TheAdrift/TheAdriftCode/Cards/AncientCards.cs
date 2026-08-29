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
using VoidCard = MegaCrit.Sts2.Core.Models.Cards.Void;

namespace TheAdrift.Cards;

/// <summary>
///     青春之诗 —— 先古卡（欧洛巴斯「古老牙齿」：回想 -> 青春之诗）。
///     固有。给予 2 层虚弱，给予 2 层易伤，获得 1 金币，向手牌中加入 1 张虚无和 1 张羞耻。回合结束时失去 1 金币。
/// </summary>
[RegisterCard(typeof(TheAdriftCardPool))]
public sealed class SongOfYouth  : AdriftCardTemplate
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Magic", 2),
        ModCardVars.Int("Magic2", 2)
    ];

    public SongOfYouth() : base(1, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var enemies = CardUtils.Enemies(Owner).ToList();
        await PowerCmd.Apply<WeakPower>(choiceContext, enemies, 2, Owner.Creature, this);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, enemies, 2, Owner.Creature, this);
        await CardUtils.GainGold(choiceContext, Owner, 1);
        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<VoidCard>(), upgraded: IsUpgraded);
        await CardUtils.AddToHand(choiceContext, Owner, CardUtils.Canonical<Shame>());
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>
///     阵亡形态 —— 先古卡（达弗「尘封魔典」）。
///     当生命值将要降低到 0 或以下时，消耗全部诅咒牌并恢复等量生命值（只能起效 1 次）。
/// </summary>
[RegisterCard(typeof(TheAdriftCardPool))]
[RegisterDustyTomeCard(typeof(TheAdriftCharacter))]
public sealed class DeathForm  : AdriftCardTemplate
{
    public DeathForm() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<DeathFormPower>(choiceContext, [Owner.Creature], 1, Owner.Creature, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
