using Sirenix.OdinInspector;

namespace NexusFramework.GAS.ECS
{
    public enum AttributeCaptureType
    {
        [LabelText(SdfIconType.Watch, Text = "追踪")]
        Track,
        [LabelText(SdfIconType.Camera, Text = "快照")]
        SnapShot
    }
}