using Sirenix.OdinInspector;

namespace NexusFramework.GAS.ECS
{
    public enum PeriodResetPolicy
    {
        [LabelText("不重置Period计时", SdfIconType.XCircleFill)]
        NeverRefresh, //不重置Effect的周期计时

        [LabelText("应用成功后重置Period计时", SdfIconType.HourglassTop)]
        ResetOnSuccessfulApplication //每次apply成功后重置Effect的周期计时
    }
}