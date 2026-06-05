using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace NexusFramework.GAS.ECS
{
    [UpdateInGroup(typeof(SGEffectDestroy))]
    [DisableAutoCreation]
    public partial struct SDestroyEffects : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CEffectDestroy>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var em = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (_, ge) in SystemAPI.Query<RefRO<CEffectDestroy>>().WithEntityAccess())
            {
                CleanupFromAscBuffer(em, ge);
                DisposeAllNativeArrays(em, ge);
                ecb.DestroyEntity(ge);
            }

            ecb.Playback(state.EntityManager);
        }

        private static void CleanupFromAscBuffer(EntityManager em, Entity ge)
        {
            if (!em.HasComponent<CEffectInUsage>(ge)) return;
            var inUsage = em.GetComponentData<CEffectInUsage>(ge);
            var target = inUsage.Target;
            if (target == Entity.Null || !em.Exists(target)) return;
            if (!em.HasBuffer<BGameplayEffect>(target)) return;

            var buffer = em.GetBuffer<BGameplayEffect>(target);
            for (var i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].GameplayEffect == ge)
                {
                    buffer.RemoveAt(i);
                    break;
                }
            }
        }

        private static void DisposeAllNativeArrays(EntityManager em, Entity ge)
        {
            DisposeEntityIfExists<CStacking>(em, ge, s => s.overflowEffects);
            DisposeEntityIfExists<CPeriod>(em, ge, p => p.GameplayEffects);

            DisposeTagRequirementIfExists<CApplicationRequiredTags>(em, ge, c => c.requirement);
            DisposeTagRequirementIfExists<COngoingRequiredTags>(em, ge, c => c.requirement);
            DisposeTagRequirementIfExists<CRemoveEffectWithTags>(em, ge, c => c.requirement);
            DisposeTagRequirementIfExists<CEffectImmunityTags>(em, ge, c => c.requirement);

            DisposeIfExists<CEffectAssetTags>(em, ge, c => c.tags);
            DisposeIfExists<CEffectGrantedTags>(em, ge, c => c.tags);
            DisposeIfExists<CApplicationCondition>(em, ge, c => c.conditions);
            DisposeEntityIfExists<CCueOnApply>(em, ge, c => c.cues);
            DisposeEntityIfExists<CCueOnActivate>(em, ge, c => c.cues);
            DisposeEntityIfExists<CCueOnDeactivate>(em, ge, c => c.cues);
            DisposeEntityIfExists<CCueOnAdd>(em, ge, c => c.cues);
            DisposeEntityIfExists<CCueOnRemove>(em, ge, c => c.cues);
            DisposeEntityIfExists<CCueOnTick>(em, ge, c => c.cues);
        }

        private static void DisposeIfExists<T>(EntityManager em, Entity ge, System.Func<T, NativeArray<int>> selector)
            where T : unmanaged, IComponentData
        {
            if (em.HasComponent<T>(ge))
            {
                var comp = em.GetComponentData<T>(ge);
                var arr = selector(comp);
                if (arr.IsCreated)
                    arr.Dispose();
            }
        }

        private static void DisposeEntityIfExists<T>(EntityManager em, Entity ge, System.Func<T, NativeArray<Entity>> selector)
            where T : unmanaged, IComponentData
        {
            if (em.HasComponent<T>(ge))
            {
                var comp = em.GetComponentData<T>(ge);
                var arr = selector(comp);
                if (arr.IsCreated)
                    arr.Dispose();
            }
        }

        private static void DisposeTagRequirementIfExists<T>(EntityManager em, Entity ge,
            System.Func<T, TagRequirementData> selector) where T : unmanaged, IComponentData
        {
            if (em.HasComponent<T>(ge))
            {
                var comp = em.GetComponentData<T>(ge);
                var req = selector(comp);
                if (req.all.IsCreated) req.all.Dispose();
                if (req.any.IsCreated) req.any.Dispose();
                if (req.none.IsCreated) req.none.Dispose();
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}
