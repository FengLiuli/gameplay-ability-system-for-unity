using System;
using NexusFramework;
using NexusFramework.DataCarrier;
using NexusFramework.GAS.Config;
using NexusFramework.GAS.ECS;
using NexusFramework.GAS.Services;
using NexusFramework.GAS.Models;

namespace NexusFramework.GAS
{
    public abstract class GASArchitecture : Architecture
    {
        public override string ArchitectureType => "GAS";

        protected virtual IConfigLoader CreateConfigLoader() => new JsonConfigLoader();

        protected override void OnInit()
        {
            RegisterModel(new GASEntityMapModel());
            RegisterService(new WorldService());
            RegisterService(new TimerService());
            RegisterService(new EventBridgeService());
            RegisterService(new TagService());
            RegisterService(new EffectService());
            RegisterService(new AbilityService());
            RegisterService(new CueService());
            RegisterUtility(CreateConfigLoader());
        }

        public void InitConfig(Func<string, string> jsonLoader)
        {
            var loader = this.GetUtility<IConfigLoader>();
            if (loader is JsonConfigLoader json)
            {
                json.Init(jsonLoader);
                json.LoadAll();
            }
        }

        public CarrierId CreateGASCarrier(string typeName)
        {
            var carrierId = GetCarrierManager().CreateCarrier(typeName);
            var worldService = this.GetService<WorldService>();
            var entity = worldService.EntityManager.CreateEntity();
            worldService.SetupGASEntity(entity);
            this.GetModel<GASEntityMapModel>().Bind(carrierId, entity);
            return carrierId;
        }

        public void DestroyGASCarrier(CarrierId carrierId)
        {
            var model = this.GetModel<GASEntityMapModel>();
            if (!model.ContainsCarrier(carrierId)) return;

            var entity = model.GetGASEntity(carrierId);
            model.Unbind(carrierId);
            var ws = this.GetService<WorldService>();
            if (ws.EntityManager.Exists(entity))
            {
                // 回收 BEAttrSet 中各属性集的 NativeArray
                var em = ws.EntityManager;
                if (em.HasBuffer<ECS.BEAttrSet>(entity))
                {
                    var attrSets = em.GetBuffer<ECS.BEAttrSet>(entity);
                    for (int i = 0; i < attrSets.Length; i++)
                    {
                        var attrs = attrSets[i].Attributes;
                        if (attrs.IsCreated) attrs.Dispose();
                    }
                }
                em.DestroyEntity(entity);
            }

            GetCarrierManager().DestroyCarrier(carrierId);
        }

        protected override void OnShutdown()
        {
            GASInternalBridge.Clear();
        }
    }
}
