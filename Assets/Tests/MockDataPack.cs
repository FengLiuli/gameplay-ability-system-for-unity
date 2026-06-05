using NexusFramework.GAS.Config;
using NexusFramework.GAS.ECS;

namespace NexusFramework.GAS.Tests
{
    /// <summary>测试用数据包——委托给 MockConfigLoader</summary>
    public class MockDataPack : IDataPack
    {
        private readonly MockConfigLoader _loader = new();

        public string PackName => "MockTest";

        public GameplayEffectComponentConfig[] GetGameplayEffectConfig(int id)
            => _loader.GetGameplayEffectConfig(id);

        public AbilityComponentConfig[] GetAbilityConfig(int id)
            => _loader.GetAbilityConfig(id);

        public Config.GameplayCueConfig GetGameplayCueConfig(int id)
            => ((IConfigLoader)_loader).GetGameplayCueConfig(id);

        public Config.MMCConfig GetMmcConfig(int id)
            => ((IConfigLoader)_loader).GetMmcConfig(id);

        public Config.TagHierarchyData GetTagHierarchy()
            => _loader.GetTagHierarchy();
    }
}
