using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace NexusFramework.GAS.ECS
{
    public static class GeneralGasChoiceHelper
    {
        public static List<ValueDropdownItem> Tags() => new();
        public static List<ValueDropdownItem> GameplayEffects() => new();
        public static List<ValueDropdownItem> GameplayCues() => new();
        public static List<ValueDropdownItem> AttrSets() => new();
        public static List<ValueDropdownItem> Attrs(int attrSetCode) => new();
    }
}
