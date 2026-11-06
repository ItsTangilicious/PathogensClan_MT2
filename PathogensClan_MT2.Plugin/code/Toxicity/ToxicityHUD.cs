using Conductor.TrackedValues;
using Conductor.UI;
using PathogensClan_MT2.Plugin;

namespace PathogensClan_MT2.Plugin
{
    public class ToxicityHud : ClassMechanicHud
    {
        protected override void HandleValueChanged(TrackedValueChangedParams changedParams)
        {
            Plugin.Logger.LogInfo($"ToxicityHud: Value changed from {changedParams.previousValue} to {changedParams.value}");
            base.HandleValueChanged(changedParams);
        }
    }
}