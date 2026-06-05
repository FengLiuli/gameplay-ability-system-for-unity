using Unity.Burst;
using Unity.Entities;

namespace NexusFramework.GAS.ECS
{
    [UpdateInGroup(typeof(SGDeactivateEffect))]
    [UpdateBefore(typeof(SDeactivateEnd))]
    [DisableAutoCreation]
    public partial struct SEffectRemoveGrantedTags : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WipDeactivateEffect>();
            state.RequireForUpdate<CEffectInstance>();
            state.RequireForUpdate<CEffectGrantedTags>();
            state.RequireForUpdate<CEffectInUsage>();
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (_, _, grantedTags, inUsage, ge) in
                     SystemAPI.Query<
                         RefRO<CEffectInstance>,
                         RefRO<WipDeactivateEffect>,
                         RefRO<CEffectGrantedTags>,
                         RefRO<CEffectInUsage>>().WithEntityAccess())
            {

                var tags = grantedTags.ValueRO.tags;
                var targetAsc = inUsage.ValueRO.Target;
                foreach (var tag in tags)
                {
                    var tempTags = state.EntityManager.GetBuffer<BTemporaryTag>(targetAsc);
                    for (var i = 0; i < tempTags.Length; i++)
                    {
                        if (tempTags[i].tag == tag && tempTags[i].source == ge)
                        {
                            tempTags.RemoveAt(i);
                            // TODO: EventBridge
                            break;
                        }
                    }
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}
