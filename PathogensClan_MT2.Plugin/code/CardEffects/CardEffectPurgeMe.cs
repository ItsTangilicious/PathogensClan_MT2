using System.Collections;
using TrainworksReloaded.Base;
using TrainworksReloaded.Base.Extensions;
using TrainworksReloaded.Core;
using TrainworksReloaded.Core.Interfaces;
using UnityEngine;

namespace PathogensClan_MT2.Plugin
{
public class CardEffectPurgeMe : CardEffectBase
{
	public override bool CanApplyInPreviewMode => false;

	public override PropDescriptions CreateEditorInspectorDescriptions()
	{
		return new PropDescriptions
		{
			
		};
	}

	public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
	{
		CardManager cardManager = coreGameManagers.GetCardManager();
		SaveManager saveManager = coreGameManagers.GetSaveManager();
		if (!(cardManager == null) && !saveManager.PreviewMode)
		{
			CardState? originalCard = cardEffectState.GetParentCardState();
			if (cardManager.IsCardInHand(originalCard))
			{
				CardManager.DiscardCardParams discardCardParams = new CardManager.DiscardCardParams
				{
					discardCard = originalCard,
					triggeredByCard = true,
					wasPlayed = false,
					overrideDiscardEffect = HandUI.DiscardEffect.Purged
				};
				yield return cardManager.PurgeCardFromHand(discardCardParams, applyOnPurgeRelicEffects: false);
			}
			else
			{
				yield return cardManager.PurgeCard(originalCard, applyOnPurgeRelicEffects: false);
			}
		}
	}

}

}
