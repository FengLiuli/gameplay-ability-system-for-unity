namespace NexusFramework.GAS.Config
{
    /// <summary>数据包——封装一个配置数据源</summary>
    public interface IDataPack
    {
        string PackName { get; }
        NexusFramework.GAS.ECS.GameplayEffectComponentConfig[] GetGameplayEffectConfig(int id);
        NexusFramework.GAS.ECS.AbilityComponentConfig[] GetAbilityConfig(int id);
        GameplayCueConfig GetGameplayCueConfig(int id);
        MMCConfig GetMmcConfig(int id);
        TagHierarchyData GetTagHierarchy();
    }
}
