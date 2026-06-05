using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace NexusFramework.GAS.Config
{
    /// <summary>编辑器下拉选项工具，由 ConfigModel 在加载时填充数据</summary>
    public static class GeneralGasChoiceHelper
    {
        private static List<ValueDropdownItem> _tags = new();
        private static List<ValueDropdownItem> _effects = new();
        private static List<ValueDropdownItem> _cues = new();
        private static List<ValueDropdownItem> _mmcs = new();
        private static readonly Dictionary<int, List<ValueDropdownItem>> _attrsBySet = new();

        public static List<ValueDropdownItem> Tags() => _tags;
        public static List<ValueDropdownItem> GameplayEffects() => _effects;
        public static List<ValueDropdownItem> GameplayCues() => _cues;
        public static List<ValueDropdownItem> MmcTypes() => _mmcs;
        public static List<ValueDropdownItem> AttrSets() => new();
        public static List<ValueDropdownItem> Attrs(int attrSetCode) =>
            _attrsBySet.TryGetValue(attrSetCode, out var list) ? list : new();

        // ── 数据填充（由 ConfigModel 调用） ──

        internal static void SetTags(List<ValueDropdownItem> tags) => _tags = tags;
        internal static void SetEffects(List<ValueDropdownItem> effects) => _effects = effects;
        internal static void SetCues(List<ValueDropdownItem> cues) => _cues = cues;
        internal static void SetMmcs(List<ValueDropdownItem> mmcs) => _mmcs = mmcs;

        private static readonly List<Type> _mmcTypes = new();
        internal static void RegisterMmcType(Type mmcType)
        {
            if (!_mmcTypes.Contains(mmcType))
            {
                _mmcTypes.Add(mmcType);
                _mmcs.Add(new ValueDropdownItem(mmcType.Name, mmcType.Name));
            }
        }
    }
}
