using NexusFramework;
using NexusFramework.GAS.Config;
using NexusFramework.GAS.ECS;
using Unity.Entities;
using Unity.Collections;

namespace NexusFramework.GAS.Tests
{
    public class MockConfigLoader : IConfigLoader
    {
        public bool Initialized { get; set; }

        public void Init() => Initialized = true;
        public void Deinit() => Initialized = false;

        public GameplayEffectComponentConfig[] GetGameplayEffectConfig(int id)
        {
            switch (id)
            {
                case 1:
                    // 瞬时 GE：纯 Add 修饰器，无 Duration
                    return new GameplayEffectComponentConfig[]
                    {
                        new TestModConfig(attrSetCode: 1, attrCode: 1, magnitude: 10f)
                    };

                case 2:
                    // 持续 GE：Duration(50帧) + Add 修饰器
                    return new GameplayEffectComponentConfig[]
                    {
                        new TestDurationConfig(duration: 50),
                        new TestModConfig(attrSetCode: 1, attrCode: 1, magnitude: 10f)
                    };

                case 3:
                    // 可堆叠持续 GE：Duration(100帧) + Stacking(Limit=3) + Add 修饰器
                    return new GameplayEffectComponentConfig[]
                    {
                        new TestDurationConfig(duration: 100),
                        new TestStackingConfig(stackingCode: 100, limitCount: 3),
                        new TestModConfig(attrSetCode: 1, attrCode: 1, magnitude: 5f)
                    };

                case 10:
                    // 带需求标签的 GE：要求目标有标签 10
                    return new GameplayEffectComponentConfig[]
                    {
                        new TestDurationConfig(duration: 50),
                        new TestTagRequireConfig(
                            allTags: new[] { 10 }, anyTags: null, noneTags: null)
                    };

                case 11:
                    // 带免疫标签的 GE：目标有标签 20 则免疫
                    return new GameplayEffectComponentConfig[]
                    {
                        new TestDurationConfig(duration: 50),
                        new TestTagImmuneConfig(
                            allTags: new[] { 20 }, anyTags: null, noneTags: null)
                    };

                case 12:
                    return new GameplayEffectComponentConfig[]
                    {
                        new TestDurationConfig(duration: 50),
                        new TestGrantedTagConfig(tags: new[] { 10 })
                    };

                case 20:
                    // 带 CueOnActivate 的持续 GE
                    return new GameplayEffectComponentConfig[]
                    {
                        new TestDurationConfig(duration: 50),
                        new TestCueConfig()
                    };

                default:
                    return null;
            }
        }

        public AbilityComponentConfig[] GetAbilityConfig(int id)
        {
            // abilityCode=1: 带空逻辑的最简能力
            if (id == 1)
                return new AbilityComponentConfig[]
                {
                    new TestAbilityLogicConfig()
                };
            return null;
        }
        NexusFramework.GAS.Config.GameplayCueConfig IConfigLoader.GetGameplayCueConfig(int id) => default;
        NexusFramework.GAS.Config.MMCConfig IConfigLoader.GetMmcConfig(int id) => default;
        public global::NexusFramework.GAS.Config.TagHierarchyData GetTagHierarchy() => default;
    }

    /// <summary>测试用：加载 Duration 组件到 GE</summary>
    internal class TestDurationConfig : GameplayEffectComponentConfig
    {
        private readonly int _duration;

        public TestDurationConfig(int duration) { _duration = duration; }

        public override void LoadToGameplayEffectEntity(Entity ge)
        {
            _entityManager.AddComponent<CDuration>(ge);
            _entityManager.SetComponentData(ge, new CDuration
            {
                duration = _duration,
                timeUnit = TimeUnit.Frame,
                active = false
            });
        }
    }

    /// <summary>测试用：加载 Modifiers 组件到 GE</summary>
    internal class TestModConfig : GameplayEffectComponentConfig
    {
        private readonly int _attrSetCode;
        private readonly int _attrCode;
        private readonly float _magnitude;

        public TestModConfig(int attrSetCode, int attrCode, float magnitude)
        {
            _attrSetCode = attrSetCode;
            _attrCode = attrCode;
            _magnitude = magnitude;
        }

        public override void LoadToGameplayEffectEntity(Entity ge)
        {
            _entityManager.AddComponent<MCModifiers>(ge);
            _entityManager.SetComponentData(ge, new MCModifiers(new EffectModifier[]
            {
                new EffectModifier
                {
                    AttrSetCode = _attrSetCode,
                    AttrCode = _attrCode,
                    Operation = GEOperation.Add,
                    Magnitude = _magnitude,
                    MMC = null
                }
            }));
        }
    }

    /// <summary>测试用：加载 Stacking 组件到 GE（无溢出效果）</summary>
    internal class TestStackingConfig : GameplayEffectComponentConfig
    {
        private readonly int _stackingCode;
        private readonly int _limitCount;

        public TestStackingConfig(int stackingCode, int limitCount)
        {
            _stackingCode = stackingCode;
            _limitCount = limitCount;
        }

        public override void LoadToGameplayEffectEntity(Entity ge)
        {
            _entityManager.AddComponent<CStacking>(ge);
            _entityManager.SetComponentData(ge, new CStacking
            {
                StackType = EffectStackType.AggregateByTarget,
                StackingCode = _stackingCode,
                LimitCount = _limitCount,
                EffectDurationRefreshPolicy = EffectDurationRefreshPolicy.RefreshOnSuccessfulApplication,
                EffectPeriodResetPolicy = EffectPeriodResetPolicy.NeverRefresh,
                EffectExpirationPolicy = EffectExpirationPolicy.ClearEntireStack,
                denyOverflowApplication = false,
                clearStackOnOverflow = false,
                overflowEffects = new NativeArray<Entity>(0, Allocator.Persistent)
            });
        }
    }

    /// <summary>测试用：加载需求标签组件</summary>
    internal class TestTagRequireConfig : GameplayEffectComponentConfig
    {
        private readonly int[] _all;
        private readonly int[] _any;
        private readonly int[] _none;

        public TestTagRequireConfig(int[] allTags, int[] anyTags, int[] noneTags)
        {
            _all = allTags ?? new int[0];
            _any = anyTags ?? new int[0];
            _none = noneTags ?? new int[0];
        }

        public override void LoadToGameplayEffectEntity(Entity ge)
        {
            _entityManager.AddComponent<CApplicationRequiredTags>(ge);
            _entityManager.SetComponentData(ge, new CApplicationRequiredTags
            {
                requirement = new TagRequirementData
                {
                    all = new NativeArray<int>(_all, Allocator.Persistent),
                    any = new NativeArray<int>(_any, Allocator.Persistent),
                    none = new NativeArray<int>(_none, Allocator.Persistent)
                }
            });
        }
    }

    /// <summary>测试用：加载免疫标签组件</summary>
    internal class TestTagImmuneConfig : GameplayEffectComponentConfig
    {
        private readonly int[] _all;
        private readonly int[] _any;
        private readonly int[] _none;

        public TestTagImmuneConfig(int[] allTags, int[] anyTags, int[] noneTags)
        {
            _all = allTags ?? new int[0];
            _any = anyTags ?? new int[0];
            _none = noneTags ?? new int[0];
        }

        public override void LoadToGameplayEffectEntity(Entity ge)
        {
            _entityManager.AddComponent<CEffectImmunityTags>(ge);
            _entityManager.SetComponentData(ge, new CEffectImmunityTags
            {
                requirement = new TagRequirementData
                {
                    all = new NativeArray<int>(_all, Allocator.Persistent),
                    any = new NativeArray<int>(_any, Allocator.Persistent),
                    none = new NativeArray<int>(_none, Allocator.Persistent)
                }
            });
        }
    }

    /// <summary>测试用：加载授予标签组件</summary>
    internal class TestGrantedTagConfig : GameplayEffectComponentConfig
    {
        private readonly int[] _tags;

        public TestGrantedTagConfig(int[] tags) { _tags = tags ?? new int[0]; }

        public override void LoadToGameplayEffectEntity(Entity ge)
        {
            _entityManager.AddComponent<CEffectGrantedTags>(ge);
            _entityManager.SetComponentData(ge, new CEffectGrantedTags
            {
                tags = new NativeArray<int>(_tags, Allocator.Persistent)
            });
        }
    }

    /// <summary>测试用：加载空 AbilityLogic 到能力实体</summary>
    internal class TestAbilityLogicConfig : AbilityComponentConfig
    {
        public override void LoadToGameplayAbilityEntity(Entity ability)
        {
            _entityManager.AddComponent<MCAbilityLogic>(ability);
            _entityManager.SetComponentData(ability, new MCAbilityLogic(new NullAbilityLogic(ability, _entityManager)));
        }
    }
}

namespace NexusFramework.GAS.Tests
{
    /// <summary>测试用：加载 Cue 实体并挂载到 GE</summary>
    internal class TestCueConfig : GameplayEffectComponentConfig
    {
        public override void LoadToGameplayEffectEntity(Entity ge)
        {
            var cueEntity = _entityManager.CreateEntity();
            _entityManager.AddComponent<ECCuePlayable>(cueEntity);
            _entityManager.SetComponentEnabled<ECCuePlayable>(cueEntity, false);
            _entityManager.AddComponent<ECCuePlaying>(cueEntity);
            _entityManager.SetComponentEnabled<ECCuePlaying>(cueEntity, false);
            _entityManager.AddComponent<ECKillCue>(cueEntity);
            _entityManager.SetComponentEnabled<ECKillCue>(cueEntity, false);
            _entityManager.AddComponent<MCCue>(cueEntity);
            _entityManager.SetComponentData(cueEntity, new MCCue(new NullCueForTest(_entityManager)));

            _entityManager.AddComponent<CCueOnActivate>(ge);
            _entityManager.SetComponentData(ge, new CCueOnActivate
            {
                cues = new Unity.Collections.NativeArray<Entity>(new[] { cueEntity }, Unity.Collections.Allocator.Persistent)
            });
        }
    }

    /// <summary>测试用空 Cue：不执行任何实际操作</summary>
    public class NullCueForTest : GameplayCueBase
    {
        public NullCueForTest(EntityManager em) : base(em) { }
        public override void InitParameters(XParam xParam) { }
    }
}
