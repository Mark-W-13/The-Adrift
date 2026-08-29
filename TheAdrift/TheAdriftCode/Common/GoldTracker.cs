using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rewards;

namespace TheAdrift.Common;

/// <summary>
///     局内/战斗级统计（摘下苹果光束、谁点燃了世界、天地银行、尖塔闪购、南北绿豆浆）。
///     由 Entry.Initialize 中的生命周期订阅驱动。
/// </summary>
internal static class GoldTracker
{
    // 玩家 -> 本局拾取的卡牌奖励次数
    private static readonly Dictionary<Player, int> RewardsPicked = new(ReferenceEqualityComparer.Instance);
    // 玩家 -> 本场战斗金币变动次数
    private static readonly Dictionary<Player, int> CombatGoldChanges = new(ReferenceEqualityComparer.Instance);
    // 玩家 -> 天地银行贷款（战斗结束扣款）
    private static readonly Dictionary<Player, int> BankDebts = new(ReferenceEqualityComparer.Instance);
    // 玩家 -> 待发的额外卡牌奖励
    private static readonly HashSet<Player> PendingExtraCardRewards = new(ReferenceEqualityComparer.Instance);

    public static int GetRewardsPicked(Player player)
        => RewardsPicked.GetValueOrDefault(player);

    public static int GetCombatGoldChanges(Player player)
        => CombatGoldChanges.GetValueOrDefault(player);

    public static void RegisterBankDebt(Player player, int amount)
        => BankDebts[player] = BankDebts.GetValueOrDefault(player) + amount;

    public static int ConsumeBankDebt(Player player)
    {
        var debt = BankDebts.GetValueOrDefault(player);
        BankDebts[player] = 0;
        return debt;
    }

    public static void QueueExtraCardReward(Player player)
        => PendingExtraCardRewards.Add(player);

    public static bool ConsumeExtraCardReward(Player player)
        => PendingExtraCardRewards.Remove(player);

    // ---- 生命周期回调（Entry.Initialize 中订阅） ----

    public static void OnRunStarted()
    {
        RewardsPicked.Clear();
        BankDebts.Clear();
        PendingExtraCardRewards.Clear();
    }

    public static void OnCombatStarted()
    {
        CombatGoldChanges.Clear();
    }

    public static void OnGoldChanged(Player player, bool gained)
    {
        if (player.Creature.CombatState is not null)
            CombatGoldChanges[player] = CombatGoldChanges.GetValueOrDefault(player) + 1;

        if (!gained && player.Creature.GetPower<TheAdrift.Powers.ConsumerismPower>() is not null)
        {
            _ = PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), [player.Creature], 1, player.Creature, null);
            _ = PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), [player.Creature], 1, player.Creature, null);
        }
    }

    public static void OnRewardTaken(Player player, Reward reward)
    {
        if (reward is CardReward)
            RewardsPicked[player] = RewardsPicked.GetValueOrDefault(player) + 1;
    }
}
