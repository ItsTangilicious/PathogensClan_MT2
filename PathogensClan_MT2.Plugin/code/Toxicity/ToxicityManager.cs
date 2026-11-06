using TrainworksReloaded.Core.Interfaces;
using TrainworksReloaded.Core.Impl;
using HarmonyLib;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using TrainworksReloaded.Core.Enum;
using TrainworksReloaded.Core;
using System.Collections;
using UnityEngine;
using TrainworksReloaded.Base.Enums;
using Conductor.UI;
using Conductor.TrackedValues;

namespace PathogensClan_MT2.Plugin
{
    public class ToxicityManager : AbstractTrackedValueHandler, IProvider, IClient
    {
        public const string ToxicitySaveDataKeyMagic = "__toxicity";
        public const int BaseMaxToxicity = 100;
        private SaveManager? saveManager = null;
        private RelicManager? relicManager = null;
        private CardManager? cardManager = null;
        private StatusEffectManager? statusEffectManager = null;
        private RoomManager? roomManager = null;

        public ToxicityManager()
        {
        }

        public int GetMaxToxicity()
        {
            return BaseMaxToxicity /*+ GetAdditionalMaxToxicityFromRelics()*/;
        }

        public IRegister<ToxicityData> GetToxicityDataRegister()
        {
            var container = Railend.GetContainer();
            return container.GetInstance<IRegister<ToxicityData>>();
        }

    // I currently do not have any Toxicity relics
      /* public int GetAdditionalMaxToxicityFromRelics()
        {
            if (relicManager == null)
            {
                return 0;
            }
            int num = 0;
            var relicEffects = new List<IModifyMaxToxicityRelicEffect>();
            relicEffects = relicManager.GetRelicEffects<IModifyMaxToxicityRelicEffect>(relicEffects);
            foreach (var relicEffect in relicEffects)
            {
                num += relicEffect.GetMaxToxicityAmount();
            }
            return num;
        }*/

        public int GetCurrentToxicity()
        {
            var toxicitySaveData = GetToxicitySaveData();
            return toxicitySaveData?.Toxicity ?? 0;
        }


        public override int GetValue(CardStatistics.StatValueData statValueData)
        {
            return GetCurrentToxicity();
        }

        public override void IncrementValue(CardState? card, int amount, CardStatistics.EntryDuration entryDuration = CardStatistics.EntryDuration.ThisTurn)
        {
            RewardToxicity(amount);
        }
        public IEnumerator RewardToxicity(int amount)
        {
            var toxicitySaveData = GetToxicitySaveData() ?? new ToxicitySaveData
            {
                Toxicity = 0
            };
            var previousToxicity = toxicitySaveData.Toxicity;
            var newToxicity = Mathf.Clamp(toxicitySaveData.Toxicity + amount, 0, GetMaxToxicity());
            toxicitySaveData.Toxicity = newToxicity;
            var isFullToxicity = newToxicity == GetMaxToxicity();
            if (!toxicitySaveData.IsFullToxicity && isFullToxicity)
            {
                toxicitySaveData.IsFullToxicity = true;
                Plugin.Logger.LogInfo("Triggering Full Toxicity");
                yield return TriggerFullToxicity();
            }
            else if (toxicitySaveData.IsFullToxicity && !isFullToxicity)
            {
                toxicitySaveData.IsFullToxicity = false;
                Plugin.Logger.LogInfo("Triggering Full Toxicity Lost");
                yield return TriggerFullToxicityLost();
            } 
            SaveToxicitySaveData(toxicitySaveData);
            this.valueChangedSignal.Dispatch(new TrackedValueChangedParams() { value = newToxicity, previousValue = previousToxicity });
        } 
        // I do not have TriggerFullToxicity
        public IEnumerator TriggerFullToxicity()
        {
            if (statusEffectManager != null && roomManager != null)
            {
                StatusEffectState.InputTriggerParams inputParams = new StatusEffectState.InputTriggerParams();
                StatusEffectState.OutputTriggerParams outputParams = new StatusEffectState.OutputTriggerParams();
                for (int i = 0; i < roomManager.GetNumRooms(); i++)
                {
                    var room = roomManager.GetRoom(i);
                    if (room != null)
                    {
                        var characters = new List<CharacterState>();
                        room.AddCharactersToList(characters, Team.Type.Monsters, false, true);
                        room.AddCharactersToList(characters, Team.Type.Heroes, false, true);
                        foreach (var character in characters)
                        {
                            inputParams.associatedCharacter = character;
                            yield return statusEffectManager.TriggerStatusEffectOnUnit(StatusEffectTriggers.OnFullToxicity, character, inputParams, outputParams, true);
                        }
                    }
                }
            }
        } 
        // I do not have TriggerFullToxicityLost
        public IEnumerator TriggerFullToxicityLost()
        {
            if (statusEffectManager != null && roomManager != null)
            {
                StatusEffectState.InputTriggerParams inputParams = new StatusEffectState.InputTriggerParams();
                StatusEffectState.OutputTriggerParams outputParams = new StatusEffectState.OutputTriggerParams();
                for (int i = 0; i < roomManager.GetNumRooms(); i++)
                {
                    var room = roomManager.GetRoom(i);
                    if (room != null)
                    {
                        var characters = new List<CharacterState>();
                        room.AddCharactersToList(characters, Team.Type.Monsters, false, true);
                        room.AddCharactersToList(characters, Team.Type.Heroes, false, true);
                        foreach (var character in characters)
                        {
                            inputParams.associatedCharacter = character;
                            yield return statusEffectManager.TriggerStatusEffectOnUnit(StatusEffectTriggers.OnFullToxicityLost, character, inputParams, outputParams, true);
                        }
                    }
                }
            }
        } 

        public ToxicitySaveData? GetToxicitySaveData()
        {
            if (saveManager != null)
            {
                var activeSaveData = (SaveData)AccessTools.Property(typeof(SaveManager), "ActiveSaveData").GetValue(saveManager);
                var permanentlyDisabledAbilities = (List<string>)AccessTools.Field(typeof(SaveData), "permanentlyDisabledAbilities").GetValue(activeSaveData);
                // Find string begining with magic key and load it as ToxicitySaveData
                var toxicitySaveDataJson = permanentlyDisabledAbilities.FirstOrDefault(s => s.StartsWith(ToxicitySaveDataKeyMagic));
                if (toxicitySaveDataJson != null)
                {
                    toxicitySaveDataJson = toxicitySaveDataJson.Replace(ToxicitySaveDataKeyMagic, "");
                    var toxicitySaveData = JsonSerializer.Deserialize<ToxicitySaveData>(toxicitySaveDataJson);
                    return toxicitySaveData;
                }
            }
            return null;
        }

        public void SaveToxicitySaveData(ToxicitySaveData toxicitySaveData)
        {
            if (saveManager != null)
            {
                var activeSaveData = (SaveData)AccessTools.Property(typeof(SaveManager), "ActiveSaveData").GetValue(saveManager);
                var permanentlyDisabledAbilities = (List<string>)AccessTools.Field(typeof(SaveData), "permanentlyDisabledAbilities").GetValue(activeSaveData);
                // Find string begining with magic key and remove it if it exists
                permanentlyDisabledAbilities.RemoveAll(s => s.StartsWith(ToxicitySaveDataKeyMagic));
                // JSON Serialize ToxicitySaveData
                var toxicitySaveDataJson = JsonSerializer.Serialize(toxicitySaveData);
                permanentlyDisabledAbilities.Add(ToxicitySaveDataKeyMagic + toxicitySaveDataJson);
            }
        }

        public void NewProviderAvailable(IProvider provider)
        {
            if (provider is SaveManager saveManager)
            {
                this.saveManager = saveManager;
            }
            if (provider is RelicManager relicManager)
            {
                this.relicManager = relicManager;
            }
            if (provider is CardManager cardManager)
            {
                this.cardManager = cardManager;
            }
            if (provider is StatusEffectManager statusEffectManager)
            {
                this.statusEffectManager = statusEffectManager;
            }
            if (provider is RoomManager roomManager)
            {
                this.roomManager = roomManager;
            }
        }

        public void ProviderRemoved(IProvider provider)
        {
            if (provider is SaveManager _)
            {
                this.saveManager = null;
                this.valueChangedSignal.Dispatch(new TrackedValueChangedParams() { value = 0 });
            }
            if (provider is RelicManager _)
            {
                this.relicManager = null;
            }
            if (provider is CardManager cardManager && this.cardManager != null)
            {
                this.cardManager = null;
            }
            if (provider is StatusEffectManager _)
            {
                this.statusEffectManager = null;
            }
            if (provider is RoomManager _)
            {
                this.roomManager = null;
            }
        }

        public void NewProviderFullyInstalled(IProvider newProvider)
        {

        }

    }
}