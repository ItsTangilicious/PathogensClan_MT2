using System.Collections;
using TrainworksReloaded.Base;
using TrainworksReloaded.Base.Extensions;
using TrainworksReloaded.Core;
using TrainworksReloaded.Core.Interfaces;
using UnityEngine;

namespace PathogensClan_MT2.Plugin
{
    
   public class CardEffectPlayUnitTriggerScaling : CardEffectBase
    {
        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            PropDescriptions propDescriptions = new PropDescriptions();
            string fieldName = CardEffectFieldNames.ParamTrigger.GetFieldName();
            //string fieldName2 = CardEffectFieldNames.ParamBool.GetFieldName();
            return propDescriptions;
        }
        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            CombatManager combatManager = coreGameManagers.GetCombatManager();
            CharacterTriggerData.Trigger trigger = cardEffectState.GetParamTrigger();
            
            int numTimesToTrigger = 1;
            CardState? parentCardState = cardEffectState.GetParentCardState();
            if (parentCardState != null)
            {
                foreach (CardTraitState item in (IEnumerable<CardTraitState>)parentCardState.GetTraitStates())
                {
                     if (item is CardTraitScalingFeederRules cardTraitScalingFeederRules)
                    {
                      numTimesToTrigger = cardTraitScalingFeederRules.GetAdditionalTriggers(coreGameManagers.GetCardStatistics());
                         break;
                     }
                }
             }

             foreach (CharacterState target in cardEffectParams.targets)
             {
                combatManager.QueueTrigger(target, trigger, null, true, true, null, numTimesToTrigger);
			    CardTriggerType? associatedCardTrigger = CardTriggerTypeMethods.GetAssociatedCardTrigger(trigger);
			    if (target.GetSpawnerCard() == null || !associatedCardTrigger.HasValue || !target.GetSpawnerCard().HasTriggerType(associatedCardTrigger.Value))
			    {
				    continue;
			    }
                string notificationTriggeredKey = coreGameManagers.GetAllGameData().GetBalanceData().GetStatusEffectsDisplayData()
				.GetNotificationTriggeredKey(trigger);
			    if (!string.IsNullOrEmpty(notificationTriggeredKey))
			    {
				    string text = notificationTriggeredKey.Localize();
				    if (!string.IsNullOrEmpty(text))
				    {
					    target.ShowNotification(text, PopupNotificationUI.Source.Trigger);
				    }
			    }
                yield return combatManager.ApplyCardTriggers(associatedCardTrigger.Value, target.GetSpawnerCard(), fireAllMonsterTriggersInRoom: false, -1, ignoreDeadInTargeting: true, target);
             }
          yield return combatManager.RunTriggerQueue();
         }
    }

}
