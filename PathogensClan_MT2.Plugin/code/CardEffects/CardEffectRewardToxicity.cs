using System.Collections;
using TrainworksReloaded.Base;
using TrainworksReloaded.Base.Extensions;
using TrainworksReloaded.Core;
using TrainworksReloaded.Core.Interfaces;
using UnityEngine;

namespace PathogensClan_MT2.Plugin
{
    public class CardEffectRewardToxicity : CardEffectBase
    {
        public override bool CanApplyInPreviewMode => true;
        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            PropDescriptions propDescriptions = new PropDescriptions();
            string fieldName = CardEffectFieldNames.ParamInt.GetFieldName();
            propDescriptions[fieldName] = new PropDescription("Toxicity Amount", "", typeof(int), false);
            return propDescriptions;
        }
        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            var container = Railend.GetContainer();
            var toxicityManager = container.GetInstance<ToxicityManager>();
            if (toxicityManager == null)
            {
                return false;
            }
            var toxicityAmount = cardEffectState.GetParamInt();
            return toxicityManager.GetCurrentToxicity() + toxicityAmount >= 0;
        }
        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            var saveManager = coreGameManagers.GetSaveManager();
            if(saveManager.PreviewMode)
            {
                yield break;
            }
            var toxicityAmount = cardEffectState.GetParamInt();
            var container = Railend.GetContainer();
            var toxicityManager = container.GetInstance<ToxicityManager>();
            
            if (toxicityManager != null)
            {
                yield return toxicityManager.RewardToxicity(toxicityAmount);
            }
            
            yield break;
        }
    }
}