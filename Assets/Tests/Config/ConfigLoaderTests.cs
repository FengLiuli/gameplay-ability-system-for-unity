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
    }
}
