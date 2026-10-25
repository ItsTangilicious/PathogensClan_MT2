using ShinyShoe;
using System.Collections;

/* class StatusEffectContagionState : StatusEffectState
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
            if (damageAmount >= associatedCharacter.GetHP() && combatManager != null)
            {
                yield return combatManager.ApplyDamageToTarget(damageAmount, target, parameters);
       
            }

        if (numStacks > 0)
        {
            List<CharacterState> list = new List<CharacterState>();
            RoomState? roomState = associatedCharacter.GetSpawnPoint()?.GetRoomOwner();
            if (roomState != null)
            {                    
                roomState.AddCharactersToList(list, Team.Type.Heroes);
                foreach (CharacterState characterState in list)
                {                        
                    characterState.AddStatusEffect(StatusEffectContagionState.statusID, 1);
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
    } */