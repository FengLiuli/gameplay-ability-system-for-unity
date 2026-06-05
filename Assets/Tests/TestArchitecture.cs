using NexusFramework.GAS.Config;

namespace NexusFramework.GAS.Tests
{
    public class TestArchitecture : GASArchitecture
    {
        protected override IConfigLoader CreateConfigLoader()
        {
            return new MockConfigLoader();
        }
    }
}
