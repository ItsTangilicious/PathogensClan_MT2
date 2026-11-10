using ShinyShoe;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TrainworksReloaded.Core.Interfaces;
using TrainworksReloaded.Core;

namespace PathogensClan_MT2.Plugin
{
class StatusEffectContagionState : StatusEffectState
    {
        public override bool TestTrigger(StatusEffectState.InputTriggerParams inputTriggerParams, OutputTriggerParams outputTriggerParams, ICoreGameManagers coreGameManagers)
        {
           if (inputTriggerParams.associatedCharacter != null && inputTriggerParams.associatedCharacter.IsAlive)
            {
                this.associatedCharacter = inputTriggerParams.associatedCharacter;
            }
            else
            {
                this.associatedCharacter = null;
            }
            CharacterState? characterState = associatedCharacter;
            this.stacks = ((characterState != null) ? characterState.GetStatusEffectStacks(base.GetStatusId()) : 0);
            return this.stacks > 0 && this.associatedCharacter != null;
        }

        protected override IEnumerator OnTriggered(StatusEffectState.InputTriggerParams inputTriggerParams, StatusEffectState.OutputTriggerParams outputTriggerParams, ICoreGameManagers coreGameManagers)
       {     
            if (associatedCharacter == null || associatedCharacter.IsDead)
            {               
                yield break;     
       
            }        

            CoreSignals.DamageAppliedPlaySound.Dispatch(Damage.Type.Poison);
            int numStacks = associatedCharacter.GetStatusEffectStacks(GetStatusId());
            int damageAmount = this.GetDamageAmount(this.stacks);
            CharacterState? target = associatedCharacter;
            CombatManager combatManager = coreGameManagers.GetCombatManager();
            CombatManager.ApplyDamageToTargetParameters parameters = default(CombatManager.ApplyDamageToTargetParameters);
            parameters.damageType = Damage.Type.Poison;
            StatusEffectData sourceStatusEffectData = base.GetSourceStatusEffectData();
            parameters.affectedVfx = ((sourceStatusEffectData != null) ? sourceStatusEffectData.GetOnAffectedVFX() : null);
            parameters.relicState = inputTriggerParams.suppressingRelic;
            
            //Pray for me, trying to get this to also increase Toxicity
            //Holy crap, got it to work on try one :') 
            var container = Railend.GetContainer();
            var toxicityManager = container.GetInstance<ToxicityManager>();
            if (damageAmount >= associatedCharacter.GetHP() && combatManager != null)
            {
                yield return combatManager.ApplyDamageToTarget(damageAmount, target, parameters);
            }
            //Had to separate out the two effects. Otherwise either Contagion wouldn't show proper combat math or Toxicity would trigger in preview.
            if (damageAmount >= associatedCharacter.GetHP() && combatManager != null)
            {
                if (this.IsPreviewModeCopy())
            {
                yield break;
            }
                yield return toxicityManager.RewardToxicity(1);
            }

       if (numStacks > 0)
        {
            var list = new List<CharacterState>();
            RoomState? roomState = associatedCharacter.GetSpawnPoint()?.GetRoomOwner();
            if (roomState != null)
            {                    
                roomState.AddCharactersToList(list, Team.Type.Heroes);
                foreach (CharacterState characterState in list)
                {                        
                    characterState.AddStatusEffect("PathogensClan_MT2.Plugin_contagion", 1);
                }
            };
        } 
    }

        public override int GetEffectMagnitude(int stacks = 1)
        {
            return GetDamageAmount(stacks);
        }

        private int GetDamageAmount(int stacks)
        {
            return (GetParamInt() + relicManager.GetModifiedStatusMagnitudePerStack(GetStatusId(), GetAssociatedCharacter().GetTeamType())) * stacks;
        }
        public const string statusID = "status_contagion";
        private CharacterState? associatedCharacter;
        int stacks;
    } 
}