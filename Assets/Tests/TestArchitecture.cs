using NexusFramework.GAS.Config;

namespace NexusFramework.GAS.Tests
{
    public class TestArchitecture : GASArchitecture
    {
        protected override IConfigLoader CreateConfigLoader()
        {
            var merged = new MergedConfigLoader();
            merged.RegisterPack(new MockDataPack());
            return merged;
        }
    }
}
