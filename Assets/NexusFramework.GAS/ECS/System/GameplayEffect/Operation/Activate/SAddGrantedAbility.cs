using System;
using Unity.Burst;  
using Unity.Entities;

namespace NexusFramework.GAS.ECS
{
    [UpdateInGroup(typeof(SGActivateEffect))]
    [UpdateBefore(typeof(SActivateEnd))]
    [DisableAutoCreation]
    public partial struct SAddGrantedAbility : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WipActivateEffect>();
            state.RequireForUpdate<CEffectInstance>();
            state.RequireForUpdate<MCGrantedAbility>();
            state.RequireForUpdate<CEffectInUsage>();
        }

        // 不能BurstCompile，因为涉及托管组件操作  
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (_, _, grantedAbilityComp, inUsage, ge) in
                     SystemAPI.Query<
                         RefRO<CEffectInstance>,
                         RefRO<WipActivateEffect>,
                         MCGrantedAbility,
                         RefRO<CEffectInUsage>>().WithEntityAccess())
            {
                var targetAsc = inUsage.ValueRO.Target;
                var grantedAbilities = grantedAbilityComp.GrantedAbilities;
                if (grantedAbilities == null || grantedAbilities.Length == 0) continue;

                // 检查是否已有运行时数据（GE重新激活的情况）  
                bool isReactivation = state.EntityManager.HasComponent<MCGrantedAbilityRuntime>(ge);

                if (!isReactivation)
                {
                    // === 首次激活：创建Ability Entity并挂载到ASC ===  
                    var abilityEntities = new Entity[grantedAbilities.Length];
                    var abilityBuffer = SystemAPI.GetBuffer<BAbility>(targetAsc);

                    for (int i = 0; i < grantedAbilities.Length; i++)
                    {
                        var ga = grantedAbilities[i];

                        // 创建Ability Entity（复用AbilityHelper）  
                        var abilityEntity = CreateAbilityEntity(state.EntityManager, ga.AbilityConfig.ComponentConfigs);

                        // 设置Owner为目标ASC  
                        var baseInfo = state.EntityManager.GetComponentData<CAbilityBaseInfo>(abilityEntity);
                        baseInfo.Owner = targetAsc;
                        state.EntityManager.SetComponentData(abilityEntity, baseInfo);

                        // 挂载到ASC的BAbility Buffer  
                        abilityBuffer.Add(new BAbility { Ability = abilityEntity });

                        abilityEntities[i] = abilityEntity;

                        // 根据ActivationPolicy决定是否激活  
                        if (ga.ActivationPolicy == GrantedAbilityActivationPolicy.WhenAdded
                            || ga.ActivationPolicy == GrantedAbilityActivationPolicy.SyncWithEffect)
                        {
                            state.EntityManager.AddComponent<CAbilityInTryActivate>(abilityEntity);
                        }

                        // TODO: EventBridge - RegisterSelfRemovalCallback
                        // RegisterSelfRemovalCallback(ga.RemovePolicy, abilityEntity, targetAsc);
                    }

                    // 存储运行时引用到GE Entity  
                    state.EntityManager.AddComponent<MCGrantedAbilityRuntime>(ge);
                    state.EntityManager.SetComponentData(ge, new MCGrantedAbilityRuntime(abilityEntities));
                }
                else
                {
                    // === 重新激活：只处理SyncWithEffect策略的激活 ===  
                    var runtime = state.EntityManager.GetComponentData<MCGrantedAbilityRuntime>(ge);
                    if (runtime.GrantedAbilityEntities == null) continue;

                    for (int i = 0; i < grantedAbilities.Length; i++)
                    {
                        if (grantedAbilities[i].ActivationPolicy != GrantedAbilityActivationPolicy.SyncWithEffect)
                            continue;

                        var abilityEntity = runtime.GrantedAbilityEntities[i];
                        if (abilityEntity == Entity.Null) continue;
                        if (state.EntityManager.HasComponent<CAbilityActive>(abilityEntity)) continue;

                        state.EntityManager.AddComponent<CAbilityInTryActivate>(abilityEntity);
                    }
                }
            }
        }

        private static void RegisterSelfRemovalCallback(
            GrantedAbilityRemovePolicy removePolicy,
            Entity abilityEntity,
            Entity targetAsc)
        {
            // TODO: EventBridge - Wire up self-removal callbacks
        }

        /// <summary>  
        /// 从ASC的BAbility Buffer中移除指定Ability  
        /// </summary>  
        private static void RemoveAbilityFromAsc(Entity abilityEntity, Entity targetAsc, EntityManager entityManager)
        {
            if (!entityManager.Exists(targetAsc)) return;
            var buffer = entityManager.GetBuffer<BAbility>(targetAsc);
            for (int j = 0; j < buffer.Length; j++)
            {
                if (buffer[j].Ability == abilityEntity)
                {
                    buffer.RemoveAt(j);
                    break;
                }
            }
        }

        private static Entity CreateAbilityEntity(EntityManager entityManager, AbilityComponentConfig[] configs)
        {
            var entity = entityManager.CreateEntity();
            entityManager.SetName(entity, $"Ability_{entity.ToString()}");
            foreach (var config in configs)
                config.LoadToGameplayAbilityEntity(entity);
            return entity;
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}
