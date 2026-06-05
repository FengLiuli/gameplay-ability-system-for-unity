using NexusFramework;
using System;
using System.Collections.Generic;
using UnityEngine;
using NexusFramework.GAS.ECS;

namespace NexusFramework.GAS.Config
{
    public class JsonConfigLoader : IConfigLoader
    {
        private Func<string, string> _jsonLoader;
        private Dictionary<int, GameplayEffectComponentConfig[]> _effectConfigs = new();
        private Dictionary<int, AbilityComponentConfig[]> _abilityConfigs = new();
        private TagHierarchyData _tagHierarchy;

        public bool Initialized { get; set; }

        public void Init(Func<string, string> jsonLoader)
        {
            _jsonLoader = jsonLoader;
        }

        void ICanInit.Init()
        {
        }

        void ICanInit.Deinit()
        {
            _effectConfigs.Clear();
            _abilityConfigs.Clear();
            _jsonLoader = null;
        }

        public void LoadAll()
        {
            if (_jsonLoader == null) return;
            LoadTagHierarchy();
        }

        public GameplayEffectComponentConfig[] GetGameplayEffectConfig(int id)
        {
            return _effectConfigs.TryGetValue(id, out var config) ? config : null;
        }

        public AbilityComponentConfig[] GetAbilityConfig(int id)
        {
            return _abilityConfigs.TryGetValue(id, out var config) ? config : null;
        }

        public GameplayCueConfig GetGameplayCueConfig(int id)
        {
            return default;
        }

        public MMCConfig GetMmcConfig(int id)
        {
            return default;
        }

        public TagHierarchyData GetTagHierarchy()
        {
            return _tagHierarchy;
        }

        private void LoadTagHierarchy()
        {
            var json = _jsonLoader("exgas_tbgameplaytags");
            if (string.IsNullOrEmpty(json))
            {
                _tagHierarchy = new TagHierarchyData { Tags = Array.Empty<TagNode>() };
                return;
            }

            var nodes = new List<TagNode>();
            var node = JsonUtility.FromJson<TagNode>(json);
            nodes.Add(node);
            _tagHierarchy = new TagHierarchyData { Tags = nodes.ToArray() };
        }
    }
}