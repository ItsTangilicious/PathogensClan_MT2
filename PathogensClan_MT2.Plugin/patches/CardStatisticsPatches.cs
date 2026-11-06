using HarmonyLib;
using PathogensClan_MT2.Plugin;
using TrainworksReloaded.Core;
using TrainworksReloaded.Core.Interfaces;
using ShinyShoe;

namespace PathogensClan_MT2.Plugin
{
    [HarmonyPatch(typeof(CardStatistics), "GetStatValue")]
    public static class CardStatisticsGetStatValuePatch
    {
        static void Postfix(
            CardStatistics.StatValueData statValueData,
            ref int __result)
        {
            var trackedValueType = statValueData.trackedValue;
            if (trackedValueType == TrackedValues.Toxicity)
            {
                var toxicityManager = Railend.GetContainer().GetInstance<ToxicityManager>();
                if (toxicityManager == null)
                {
                    return;
                }
                __result = toxicityManager.GetCurrentToxicity();
            }
        }
    }
}