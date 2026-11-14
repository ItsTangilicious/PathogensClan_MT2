using System.Collections;
using System.Collections.Generic;
using TrainworksReloaded.Base;
using TrainworksReloaded.Base.Extensions;
using TrainworksReloaded.Core;
using TrainworksReloaded.Core.Interfaces;
using UnityEngine;

namespace PathogensClan_MT2.Plugin
{
    //based off of RelicEffectModifyCardAttack, works when a unit is summoned, temp upgrade
	//I will go back and delete unneccesary items eventually (maybe, I might use it elsewhere outside the starter artifact)
 public sealed class RelicEffectAdjustAttackByToxicity : RelicEffectBase, /*ICardModifierRelicEffect,*/ IRelicEffect, ICardModifierUndoableRelicEffect
{
	private string _characterName = string.Empty;

	private SubtypeData _characterSubtype = SubtypeData.None;

	private int _attackModifier;

	private int _unmodifiedAttackMultiplier;

	private float _attackMultiplier;

	private CardType _cardType;

	private bool _multiplyByUpgradeCount;

	private bool _multiplyByRelicCount;

	private bool _excludeChampions;

	private bool _excludeStartRelics;

	private List<(CardState cardState, int modifiedAttack)> _appliedEffects = new List<(CardState, int)>();

	private CardUpgradeMaskData? _cardFilter;

	public override PropDescriptions CreateEditorInspectorDescriptions()
	{
		return new PropDescriptions
		{
			[RelicEffectFieldNames.ParamString.GetFieldName()] = new PropDescription("CardData Asset Name", "If non-empty, the card must be this asset name."),
			[RelicEffectFieldNames.ParamCharacterSubtype.GetFieldName()] = new PropDescription("Character Subtype", "If specified, the card must spawn a monster of this subtype."),
			[RelicEffectFieldNames.ParamInt.GetFieldName()] = new PropDescription("Attack Modifier", "How much to add to the card's attack. Can only use without the attack multiplier."),
			[RelicEffectFieldNames.ParamFloat.GetFieldName()] = new PropDescription("Attack Multiplier", "How much to multiply the card's attack. Can only use without the attack modifier."),
			[RelicEffectFieldNames.ParamInt2.GetFieldName()] = new PropDescription("Unmodified Attack Multiplier", "How much to multiply the card's base attack. Can only use without the attack modifier."),
			[RelicEffectFieldNames.ParamCardType.GetFieldName()] = new PropDescription("Card Type", "If asset name and subtype aren't specified, the card must be this type."),
			[RelicEffectFieldNames.ParamBool.GetFieldName()] = new PropDescription("Multiply by Upgrade Count", "Multiplies the amount by the number of permanent upgrades on the card."),
			[RelicEffectFieldNames.ParamBool2.GetFieldName()] = new PropDescription("Multiply by Relic Count", "Multiplies the amount by the number of collected relics."),
			[RelicEffectFieldNames.ParamBool3.GetFieldName()] = new PropDescription("Exclude Champions", "If turned on, champions do not get bonus attack."),
			[RelicEffectFieldNames.ParamBool4.GetFieldName()] = new PropDescription("Exclude Starter Relics", "If turned on, starter relics will not be counted."),
			[RelicEffectFieldNames.ParamCardFilter.GetFieldName()] = new PropDescription("Card Filter", "Optional filter applied before all other checks on target cards")
		};
	}

	public override void Initialize(RelicState relicState, RelicData srcRelicData, RelicEffectData relicEffectData)
	{
		base.Initialize(relicState, srcRelicData, relicEffectData);
		_characterName = relicEffectData.GetParamString();
		_characterSubtype = relicEffectData.GetParamCharacterSubtype();
		_attackModifier = relicEffectData.GetParamInt();
		_attackMultiplier = relicEffectData.GetParamFloat();
		_unmodifiedAttackMultiplier = relicEffectData.GetParamInt2();
		_cardType = relicEffectData.GetParamCardType();
		_multiplyByUpgradeCount = relicEffectData.GetParamBool();
		_multiplyByRelicCount = relicEffectData.GetParamBool2();
		_excludeChampions = relicEffectData.GetParamBool3();
		_excludeStartRelics = relicEffectData.GetParamBool4();
		_cardFilter = relicEffectData.GetParamCardFilter();
	}

	/*public bool ApplyCardStateModifiers(CardState cardState, RelicManager relicManager, SaveManager saveManager, CardManager? cardManager)
	{
		if (cardState == null)
		{
			return false;
		}
		GetScalingValues(cardState, saveManager, out var upgradesCount, out var collectedRelicsCount);
		int modifiedAttack = GetModifiedAttack(cardState.GetTotalAttackDamage(), cardState.GetSpawnCharacterData()?.GetAttackDamage() ?? 0, upgradesCount, collectedRelicsCount);
		if (modifiedAttack == 0)
		{
			return false;
		}
		if (_cardFilter != null && !_cardFilter.FilterCard(cardState, relicManager))
		{
			return false;
		}
		bool flag = false;
		if (!string.IsNullOrEmpty(_characterName) && _characterName == cardState.GetAssetName())
		{
			flag = true;
		}
		else if (!_characterSubtype.IsNone)
		{
			foreach (CardEffectState effectState in cardState.GetEffectStates())
			{
				if (effectState.GetCardEffect() is CardEffectSpawnMonster && CardManager.DoesCardEffectPassSubtypeCheck(effectState, _characterSubtype, relicManager))
				{
					flag = true;
				}
			}
		}
		else if (_cardType == cardState.GetCardType())
		{
			flag = true;
		}
		if (cardState.IsChampionCard() && _excludeChampions)
		{
			flag = false;
		}
		if (flag)
		{
			_appliedEffects.Add((cardState, modifiedAttack));
			cardState.GetTemporaryCardStateModifiers().IncrementAdditionalDamage(modifiedAttack);
			cardState.GetTemporaryCardStateModifiers().AddAdditionalStatTooltip(_srcRelicEffectData.GetTooltipTitle(), _srcRelicEffectData.GetTooltipBody());
			cardState.UpdateDamageText();
			cardState.UpdateCardBodyText();
			cardManager?.RefreshCardInHand(cardState);
		}
		return flag;
	} */

	public bool UndoCardStateModifiers(CardState cardState, RelicManager relicManager, SaveManager saveManager, CardManager? cardManager)
	{
		bool result = false;
		for (int num = _appliedEffects.Count - 1; num >= 0; num--)
		{
			(CardState, int) tuple = _appliedEffects[num];
			if (tuple.Item1 == cardState)
			{
				cardState.GetTemporaryCardStateModifiers().IncrementAdditionalDamage(-tuple.Item2);
				cardState.GetTemporaryCardStateModifiers().RemoveAdditionalStatTooltip(_srcRelicEffectData.GetTooltipTitle(), _srcRelicEffectData.GetTooltipBody());
				cardState.UpdateDamageText();
				cardState.UpdateCardBodyText();
				cardManager?.RefreshCardInHand(cardState);
				_appliedEffects.RemoveAt(num);
				result = true;
			}
		}
		return result;
	}

	public void ResetCardStateModifierTrackingData(CardState cardState)
	{
		for (int num = _appliedEffects.Count - 1; num >= 0; num--)
		{
			if (_appliedEffects[num].cardState == cardState)
			{
				_appliedEffects.RemoveAt(num);
			}
		}
	}

	public bool GetTooltip(out string title, out string body)
	{
		title = string.Empty;
		body = string.Empty;
		return false;
	}

	public override IEnumerator OnCharacterAdded(CharacterState character, CardState fromCard, ICoreGameManagers coreGameManagers)
	{
		SaveManager saveManager = coreGameManagers.GetSaveManager();
		GetScalingValues(fromCard, saveManager, out var upgradesCount, out var collectedRelicsCount);
		int attackDamage = character.GetAttackDamage();
		int unmodifiedAttackDamage = character.GetUnmodifiedAttackDamage();
		if (character.HasStatusEffect("horde"))
		{
			attackDamage = character.GetSourceCharacterData().GetAttackDamage();
			unmodifiedAttackDamage = attackDamage;
		}
		int modifiedAttack = GetModifiedAttack(attackDamage, unmodifiedAttackDamage, upgradesCount, collectedRelicsCount);
		if (character.HasStatusEffect("cardless") && !character.GetIsClone() && character.GetCharacterManager().DoesCharacterPassSubtypeCheck(character, _characterSubtype) && (!character.IsChampion() || !_excludeChampions))
		{
			if (modifiedAttack < 0)
			{
				character.DebuffDamage(Mathf.Abs(modifiedAttack), _srcRelicState);
			}
			else
			{
				character.BuffDamage(modifiedAttack, _srcRelicState);
			}
			if (_srcRelicData.GetID() != "24c4ad26-f2b3-4a95-bddc-caab94bd4767" && _srcRelicData.GetID() != "75abded0-b922-4413-84ca-f0095642c600")
			{
				NotifyRelicTriggered(coreGameManagers.GetRelicManager(), character);
			}
			yield return TimingYieldIfNonCovenant(TimingContext.PostFire, saveManager);
		}
		if (!character.HasStatusEffect("cardless") && character.GetTeamType() == Team.Type.Monsters && !character.GetIsClone() && character.GetCharacterManager().DoesCharacterPassSubtypeCheck(character, _characterSubtype) && (!character.IsChampion() || !_excludeChampions))
		{
			if (modifiedAttack < 0)
			{
				character.DebuffDamage(Mathf.Abs(modifiedAttack), _srcRelicState);
			}
			else
			{
				character.BuffDamage(modifiedAttack, _srcRelicState);
			}
			if (_srcRelicData.GetID() != "24c4ad26-f2b3-4a95-bddc-caab94bd4767" && _srcRelicData.GetID() != "75abded0-b922-4413-84ca-f0095642c600")
			{
				NotifyRelicTriggered(coreGameManagers.GetRelicManager(), character);
			}
			yield return TimingYieldIfNonCovenant(TimingContext.PostFire, saveManager);
		}
	}

	private int GetModifiedAttack(int currentAttackDamage, int unmodifiedAttackDamage, int? upgradesCount, int? collectedRelicsCount)
	{
		int num = 0;
        var container = Railend.GetContainer();
        var toxicityManager = container.GetInstance<ToxicityManager>();
		if (_attackModifier != 0)
		{
			num = _attackModifier * toxicityManager.GetCurrentToxicity();
		}
		else if (_attackMultiplier > 1f)
		{
			num = Mathf.FloorToInt((_attackMultiplier - 1f) * (float)currentAttackDamage);
		}
		else if (_unmodifiedAttackMultiplier > 1)
		{
			num = Mathf.FloorToInt((_unmodifiedAttackMultiplier - 1) * unmodifiedAttackDamage);
		}
		if (_multiplyByUpgradeCount && upgradesCount.HasValue)
		{
			num *= upgradesCount.Value;
		}
		if (_multiplyByRelicCount && collectedRelicsCount.HasValue)
		{
			num *= collectedRelicsCount.Value;
		}
		return num;
	}

	private void GetScalingValues(CardState? cardState, SaveManager saveManager, out int upgradesCount, out int collectedRelicsCount)
	{
		upgradesCount = 0;
		if (cardState != null)
		{
			foreach (CardUpgradeState cardUpgrade in cardState.GetCardStateModifiers().GetCardUpgrades())
			{
				if (cardUpgrade.ShouldShowIcon())
				{
					upgradesCount++;
				}
			}
		}
		collectedRelicsCount = 0;
		foreach (RelicState collectedRelic in saveManager.GetCollectedRelics())
		{
			if (!collectedRelic.IsBossGivenRelic() && !collectedRelic.IsSoulSaviorScalingRelic())
			{
				ClassData mainClass = saveManager.GetMainClass();
				bool flag = ((object)mainClass != null && mainClass.IsStarterRelic(collectedRelic.GetRelicDataID())) || (saveManager.GetSubClass()?.IsStarterRelic(collectedRelic.GetRelicDataID()) ?? false);
				if (!(_excludeStartRelics && flag))
				{
					collectedRelicsCount++;
				}
			}
		}
	}
}
    /* public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions
            {
                //[RelicEffectFieldNames.ParamInt.GetFieldName()] = new PropDescription("StatusEffectData.DisplayCategory Enum"),
                [RelicEffectFieldNames.ParamSourceTeam.GetFieldName()] = new PropDescription("Target Team"),
                [CardEffectFieldNames.ParamInt.GetFieldName()] = new PropDescription("Toxicity Amount", "", typeof(int), false)
            };
        }
*/
	
      /*  var container = Railend.GetContainer();
        var toxicityManager = container.GetInstance<ToxicityManager>();
		return toxicityManager.GetCurrentToxicity() * multiplier;*/
}