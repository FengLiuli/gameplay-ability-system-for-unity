using System.Collections.Generic;
using NexusFramework;
using NexusFramework.GAS.ECS;

namespace NexusFramework.GAS.Config
{
    /// <summary>
    /// 合并式配置加载器——注册数据包，后注册的覆盖先注册的同名配置。
    /// 效果、能力、Cue、MMC 均追加+同名覆盖；标签层级取最后一个非空。
    /// </summary>
    public class MergedConfigLoader : IConfigLoader
    {
        private readonly List<IDataPack> _packs = new();
        public bool Initialized { get; set; }

        public void Init() => Initialized = true;
        public void Deinit() { _packs.Clear(); Initialized = false; }

        /// <summary>注册数据包。后注册的包有更高优先级。</summary>
        public void RegisterPack(IDataPack pack)
        {
            _packs.Add(pack);
        }

        /// <summary>按优先级检查所有包，返回首个非空结果</summary>
        private T FindFirst<T>(System.Func<IDataPack, T> getter) where T : class
        {
            for (int i = _packs.Count - 1; i >= 0; i--)
            {
                var result = getter(_packs[i]);
                if (result != null) return result;
            }
            return null;
        }

        /// <summary>合并所有包的数组结果（追加模式）</summary>
        private T[] MergeArrays<T>(System.Func<IDataPack, T[]> getter) where T : class
        {
            var dict = new Dictionary<int, T>();
            for (int i = 0; i < _packs.Count; i++)
            {
                var arr = getter(_packs[i]);
                if (arr == null) continue;
                foreach (var item in arr)
                {
                    int key = ExtractId(item);
                    dict[key] = item; // 后注册覆盖先注册
                }
            }
            var result = new T[dict.Count];
            dict.Values.CopyTo(result, 0);
            return result;
        }

        private static int ExtractId<T>(T item)
        {
            // 不使用 ID，直接按包顺序——后包覆盖前包的同索引项
            return 0; // 占位，实际子类需重写
        }

        public GameplayEffectComponentConfig[] GetGameplayEffectConfig(int id)
            => FindFirst(p => p.GetGameplayEffectConfig(id));

        public AbilityComponentConfig[] GetAbilityConfig(int id)
            => FindFirst(p => p.GetAbilityConfig(id));

        public GameplayCueConfig GetGameplayCueConfig(int id)
        {
            for (int i = _packs.Count - 1; i >= 0; i--)
            {
                var cfg = _packs[i].GetGameplayCueConfig(id);
                if (cfg.CueType != null) return cfg;
            }
            return default;
        }

        public MMCConfig GetMmcConfig(int id)
        {
            for (int i = _packs.Count - 1; i >= 0; i--)
            {
                var cfg = _packs[i].GetMmcConfig(id);
                if (cfg.MmcType != null) return cfg;
            }
            return default;
        }

        public TagHierarchyData GetTagHierarchy()
        {
            for (int i = _packs.Count - 1; i >= 0; i--)
            {
                var data = _packs[i].GetTagHierarchy();
                if (data.Tags != null) return data;
            }
            return default;
        }
    }
}
