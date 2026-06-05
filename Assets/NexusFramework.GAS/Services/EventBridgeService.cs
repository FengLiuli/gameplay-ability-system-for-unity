using NexusFramework;
using NexusFramework.GAS.ECS;

namespace NexusFramework.GAS.Services
{
    public class EventBridgeService : AbstractService
    {
        protected override void OnInit()
        {
            GASInternalBridge.OnBeforeDrain += OnBeforeDrain;
        }

        protected override void OnDeinit()
        {
            GASInternalBridge.OnBeforeDrain -= OnBeforeDrain;
        }

        private void OnBeforeDrain()
        {
        }
    }
}
