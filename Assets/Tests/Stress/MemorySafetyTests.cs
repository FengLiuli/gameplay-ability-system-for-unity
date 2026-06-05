using NexusFramework;
using NexusFramework.GAS.ECS;
using NexusFramework.GAS.Models;
using NexusFramework.GAS.Services;
using NUnit.Framework;
using Unity.Entities;

namespace NexusFramework.GAS.Tests.Stress
{
    [TestFixture]
    public class MemorySafetyTests
    {
        private TestArchitecture _arch;
        private EffectService _effectService;
        private AbilityService _abilityService;
        private EntityManager _em;
        private World _world;

        [SetUp]
        public void SetUp()
        {
            _arch = new TestArchitecture();
            _arch.Initialize();
            _arch.GetCarrierManager().RegisterType("TestUnit");
            _effectService = _arch.GetService<EffectService>();
            _abilityService = _arch.GetService<AbilityService>();
            var ws = _arch.GetService<WorldService>();
            _em = ws.EntityManager;
            _world = ws.ExWorld;
        }

        [TearDown]
        public void TearDown()
        {
            _arch?.Dispose();
            _arch = null;
        }

        /// <summary>创建和销毁 50 个 Carrier，确认无异常和残留实体</summary>
        [Test]
        public void CreateDestroy_50_Carriers()
        {
            var model = _arch.GetModel<GASEntityMapModel>();

            for (int i = 0; i < 50; i++)
            {
                var id = _arch.CreateGASCarrier("TestUnit");
                Assert.That(id.IsValid, Is.True);
                Assert.That(model.ContainsCarrier(id), Is.True);

                _arch.DestroyGASCarrier(id);
                Assert.That(model.ContainsCarrier(id), Is.False);
            }

            Assert.That(model, Is.Not.Null);
        }

        /// <summary>施加 50 个瞬时 GE 并推进管线，确认全部被销毁</summary>
        [Test]
        public void Apply_50_InstantGEs_AllDestroyed()
        {
            var src = _arch.CreateGASCarrier("TestUnit");
            var tgt = _arch.CreateGASCarrier("TestUnit");

            for (int i = 0; i < 50; i++)
                _effectService.ApplyEffect(configId: 1, target: tgt, source: src);

            TickWorld(20);

            var q = _em.CreateEntityQuery(ComponentType.ReadOnly<CEffectInUsage>());
            Assert.That(q.CalculateEntityCount(), Is.EqualTo(0),
                "所有瞬时 GE 应被管线销毁");
        }

        /// <summary>重复 Init/Dispose 架构不抛异常</summary>
        [Test]
        public void Repeated_InitDispose_DoesNotThrow()
        {
            for (int i = 0; i < 5; i++)
            {
                var arch = new TestArchitecture();
                arch.Initialize();
                arch.GetCarrierManager().RegisterType("RepUnit");
                arch.CreateGASCarrier("RepUnit");
                arch.Dispose();
            }
        }

        /// <summary>授予和移除 20 个能力，确认实体全部清理</summary>
        [Test]
        public void GrantRemove_20_Abilities_CleansUp()
        {
            var carrier = _arch.CreateGASCarrier("TestUnit");

            for (int i = 0; i < 20; i++)
            {
                _abilityService.GrantAbility(carrier, abilityCode: 1, _arch);
                var ownerEntity = _arch.GetModel<GASEntityMapModel>().GetGASEntity(carrier);
                var buf = _em.GetBuffer<BAbility>(ownerEntity);
                Assert.That(buf.Length, Is.GreaterThan(0));
                _abilityService.RemoveAbility(carrier, abilityCode: 1);
            }
        }

        private void TickWorld(int frames)
        {
            for (int i = 0; i < frames; i++)
                _world.Update();
        }
    }
}
