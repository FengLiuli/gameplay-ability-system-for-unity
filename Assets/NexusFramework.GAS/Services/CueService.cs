using System;
using NexusFramework;
using NexusFramework.GAS.ECS;

namespace NexusFramework.GAS.Services
{
    /// <summary>
    /// Cue 服务——对外暴露 CarrierId 级别的 Cue 操作。
    /// 内部委托给 ECS 工具类 CueHelper。
    /// </summary>
    public class CueService : AbstractService
    {
        protected override void OnInit() { }
        protected override void OnDeinit() { }

        /// <summary>注册 Cue 类型（初始化阶段调用）</summary>
        public void RegisterCueType(string typeName, Type logicType, Type paramType)
        {
            CueHelper.RegisterCue(typeName, logicType, paramType);
        }

        /// <summary>注册 Cue 类型（泛型版本）</summary>
        public void RegisterCueType<T>(string typeName, Type paramType) where T : GameplayCueBase
        {
            CueHelper.RegisterCue<T>(typeName, paramType);
        }
    }
}
