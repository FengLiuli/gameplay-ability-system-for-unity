using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace NexusFramework.GAS.ECS
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(SGAbility))]
    [UpdateAfter(typeof(STryActivateAbility))]
    public partial struct STryCancelAbility : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CAbilityInTryCancel>();
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var globalTimer = SystemAPI.GetSingletonRW<GlobalTimer>();
            
            foreach (var (_,ability) in SystemAPI.Query<RefRO<CAbilityInTryCancel>>().WithEntityAccess())
            {
                bool result = state.EntityManager.HasComponent<CAbilityActive>(ability);
                if (result)
                {
                    ecb.RemoveComponent<CAbilityActive>(ability);
                    RestoreDynamicTags(state.EntityManager, ability);
                    var abilityLogic = state.EntityManager.GetComponentData<MCAbilityLogic>(ability);
                    abilityLogic.logic.CancelAbility(globalTimer.ValueRO);
                    // TODO: EventBridge
                }
                ecb.RemoveComponent<CAbilityInTryCancel>(ability);
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

        private static void RestoreDynamicTags(EntityManager entityManager, Entity source)
        {
            bool hasActivationOwnedTags = entityManager.HasComponent<CAbilityActivationOwnedTags>(source);
            if (hasActivationOwnedTags)
            {
                var activationOwnedTags = entityManager.GetComponentData<CAbilityActivationOwnedTags>(source);
                var abilityBaseInfo = entityManager.GetComponentData<CAbilityBaseInfo>(source);
                foreach (var tag in activationOwnedTags.tags)
                {
                    var tempTags = entityManager.GetBuffer<BTemporaryTag>(abilityBaseInfo.Owner);
                    for (var i = 0; i < tempTags.Length; i++)
                    {
                        if (tempTags[i].tag == tag && tempTags[i].source == source)
                        {
                            tempTags.RemoveAt(i);
                            // TODO: EventBridge
                            break;
                        }
                    }
                }
            }
        }
    }
}