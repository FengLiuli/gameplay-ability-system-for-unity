using NexusFramework.GAS.Config;
using NUnit.Framework;

namespace NexusFramework.GAS.Tests.Config
{
    [TestFixture]
    public class ConfigLoaderTests
    {
        /// <summary>JsonConfigLoader 可通过 Init 接受加载函数</summary>
        [Test]
        public void JsonLoader_Accepts_LoaderFunc()
        {
            var loader = new JsonConfigLoader();
            loader.Init(key => null);
            Assert.DoesNotThrow(() => loader.LoadAll());
        }

        /// <summary>无 loader 时 LoadAll 不抛异常</summary>
        [Test]
        public void LoadAll_WithoutInit_DoesNotThrow()
        {
            var loader = new JsonConfigLoader();
            Assert.DoesNotThrow(() => loader.LoadAll());
        }

        /// <summary>不存在的配置 ID 返回 null</summary>
        [Test]
        public void GetEffectConfig_NonExistent_ReturnsNull()
        {
            var loader = new JsonConfigLoader();
            Assert.That(loader.GetGameplayEffectConfig(999), Is.Null);
        }

        /// <summary>不存在的 Ability 配置 ID 返回 null</summary>
        [Test]
        public void GetAbilityConfig_NonExistent_ReturnsNull()
        {
            var loader = new JsonConfigLoader();
            Assert.That(loader.GetAbilityConfig(999), Is.Null);
        }

        /// <summary>IConfigLoader 接口完整——所有方法可调用不抛异常</summary>
        [Test]
        public void IConfigLoader_All_Methods_Accessible()
        {
            IConfigLoader loader = new JsonConfigLoader();
            Assert.That(loader.GetGameplayEffectConfig(0), Is.Null);
            Assert.That(loader.GetAbilityConfig(0), Is.Null);
            Assert.That(loader.GetMmcConfig(0).MmcType, Is.Null);
            Assert.That(loader.GetTagHierarchy().Tags, Is.Null);
        }

        /// <summary>MockConfigLoader 接口完整</summary>
        [Test]
        public void MockLoader_All_Methods_Accessible()
        {
            IConfigLoader loader = new MockConfigLoader();
            Assert.That(loader.GetGameplayEffectConfig(1), Is.Not.Null, "configId=1 应返回有效配置");
            Assert.That(loader.GetGameplayEffectConfig(999), Is.Null);
            Assert.That(loader.GetAbilityConfig(1), Is.Not.Null, "abilityCode=1 应返回有效配置");
            Assert.That(loader.GetAbilityConfig(999), Is.Null);
        }

        /// <summary>Mod 数据包覆盖基础包配置</summary>
        [Test]
        public void MergedLoader_ModOverridesBase()
        {
            var merged = new MergedConfigLoader();
            merged.RegisterPack(new MockDataPack()); // base

            // mod: 覆盖 configId=1 为不同的配置
            merged.RegisterPack(new OverridePack(configId: 1,
                new NexusFramework.GAS.Tests.TestDurationConfig(duration: 99)));

            var result = merged.GetGameplayEffectConfig(1);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.EqualTo(1), "Mod 包覆盖了 base，只返回 mod 的配置");
        }

        /// <summary>Mod 数据包不覆盖未冲突的配置</summary>
        [Test]
        public void MergedLoader_NonConflicting_FallsThrough()
        {
            var merged = new MergedConfigLoader();
            merged.RegisterPack(new MockDataPack());
            merged.RegisterPack(new OverridePack(configId: 99,
                new NexusFramework.GAS.Tests.TestDurationConfig(duration: 99)));

            // configId=2 只在 base 包中存在
            var result = merged.GetGameplayEffectConfig(2);
            Assert.That(result, Is.Not.Null, "未冲突的 ID 应穿透到 base 包");
        }

        /// <summary>Mod 覆盖包——指定 ID 返回自定义配置</summary>
        private class OverridePack : IDataPack
        {
            private readonly int _id;
            private readonly NexusFramework.GAS.ECS.GameplayEffectComponentConfig[] _configs;

            public OverridePack(int configId, params NexusFramework.GAS.ECS.GameplayEffectComponentConfig[] configs)
            {
                _id = configId;
                _configs = configs;
            }

            public string PackName => "ModOverride";
            public NexusFramework.GAS.ECS.GameplayEffectComponentConfig[] GetGameplayEffectConfig(int id)
                => id == _id ? _configs : null;
            public NexusFramework.GAS.ECS.AbilityComponentConfig[] GetAbilityConfig(int id) => null;
            public NexusFramework.GAS.Config.GameplayCueConfig GetGameplayCueConfig(int id) => default;
            public NexusFramework.GAS.Config.MMCConfig GetMmcConfig(int id) => default;
            public NexusFramework.GAS.Config.TagHierarchyData GetTagHierarchy() => default;
        }
    }
}
