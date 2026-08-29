using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using TheAdrift.Cards;
using TheAdrift.Common;
using TheAdrift.Relics;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace TheAdrift;

/// <summary>
///     Mod 入口。ModId 需与 TheAdrift.json 中的 id 保持一致。
///     res://TheAdrift/... 为 PCK 资源目录，不是 C# namespace。
/// </summary>
[ModInitializer(nameof(Initialize))]
public partial class Entry
{
    public const string ModId = "TheAdrift";
    public const string ResPath = $"res://{ModId}";

    public static Logger Logger { get; } = new(ModId, LogType.Generic);

    public static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();

        // 让 pck 中的 Godot 脚本能被引擎找到
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);

        // 自动注册扫描程序集中的 RegisterCard/RegisterRelic 等 attribute
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);

        // 先古关联：
        // 欧洛巴斯「古老牙齿」：回想 -> 青春之诗
        RitsuLibFramework.RegisterArchaicToothTranscendenceMapping<Recall, SongOfYouth>();
        // 欧洛巴斯之触：搬史群 -> 智人TV
        RitsuLibFramework.RegisterTouchOfOrobasRefinementMapping<BanShiQun, ZhiRenTV>();
        // 达弗「尘封魔典」：阵亡形态（已通过 [RegisterDustyTomeCard] 注册候选）

        // 局内/战斗统计（摘下苹果光束、谁点燃了世界等）
        RitsuLibFramework.SubscribeLifecycle<GameReadyEvent>(_ => GoldTracker.OnRunStarted());
        RitsuLibFramework.SubscribeLifecycle<CombatStartingEvent>(_ => GoldTracker.OnCombatStarted());
        RitsuLibFramework.SubscribeLifecycle<GoldGainedEvent>(evt => GoldTracker.OnGoldChanged(evt.Player, true));
        RitsuLibFramework.SubscribeLifecycle<GoldLostEvent>(evt => GoldTracker.OnGoldChanged(evt.Player, false));
        RitsuLibFramework.SubscribeLifecycle<RewardTakenEvent>(evt => GoldTracker.OnRewardTaken(evt.Player, evt.Reward));

        Logger.Info("TheAdrift (彷徨者) initialized.");
    }
}
