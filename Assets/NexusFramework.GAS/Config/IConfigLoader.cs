using NexusFramework;
using NexusFramework.GAS.ECS;

namespace NexusFramework.GAS.Config
{
    public interface IConfigLoader : IUtility
    {
        GameplayEffectComponentConfig[] GetGameplayEffectConfig(int id);
        AbilityComponentConfig[] GetAbilityConfig(int id);
        GameplayCueConfig GetGameplayCueConfig(int id);
        MMCConfig GetMmcConfig(int id);
        TagHierarchyData GetTagHierarchy();
    }

    public struct GameplayCueConfig
    {
        public string CueType;
        public XParam Param;
        public int[] RequiredTags;
        public int[] ImmunityTags;
    }

    public struct MMCConfig
    {
        public string MmcType;
        public XParam Param;
    }

    public struct TagHierarchyData
    {
        public TagNode[] Tags;
    }

    public struct TagNode
    {
        public int Code;
        public string Name;
        public int[] Children;
    }
}