using ShinyShoe.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TrainworksReloaded.Core;
using TrainworksReloaded.Core.Interfaces;
using System.Runtime.CompilerServices;
using HarmonyLib;
using ShinyShoe;

//modifed from my MT1 version
    internal class RoomStateAttackForContagion : RoomStateModifierBase, IRoomStateDamageModifier, IRoomStateModifier, ILocalizationParamInt, ILocalizationParameterContext
    {
        private int contagionPerDamagePoint;
        //protected SaveManager saveManager;       
        private List<CharacterState> charsInTargetRoom = new List<CharacterState>();
        public int currentContagion;
        

        public override void Initialize(RoomModifierData roomModifierData, SaveManager saveManager)
        {
            base.Initialize(roomModifierData, saveManager);
            contagionPerDamagePoint = roomModifierData.GetParamInt();
        }

         public int GetModifiedMagicPowerDamage(ICoreGameManagers coreGameManagers)
    {
        return 0;
    }

        public int GetModifiedAttackDamage(Damage.Type damageType, CharacterState attackerState, bool requestingForCharacterStats, ICoreGameManagers coreGameManagers)
        {
            if (requestingForCharacterStats)
            {
                return GetDynamicInt(attackerState);
            }
            return 0;
        }

        public override int GetDynamicInt(CharacterState characterContext)
        {
            
           /* if (ProviderManager.SaveManager == null)
            {
                return 0;
            }*/
            if (characterContext.GetRoomStateModifiers().Contains(this) && characterContext.GetSpawnPoint() != null)
            {
                RoomState roomOwner = characterContext.GetCurrentRoom(false);
                using (GenericPools.GetList(out List<CharacterState> list2))
                if (roomOwner != null)
                {
                    charsInTargetRoom.Clear();
                    //coreGameManagers.GetHeroManager().AddCharactersInRoomToList(charsInTargetRoom, roomOwner.GetRoomIndex());
                    roomOwner.AddCharactersToList(charsInTargetRoom, Team.Type.Heroes | Team.Type.Monsters);                    
                    int num = 0;                    
                    {
                        foreach (CharacterState item in charsInTargetRoom)
                        {                            
                            int currentContagionStacks = item.GetStatusEffectStacks("PathogensClan_MT2.Plugin_contagion");
                            num += currentContagionStacks;
                        }
                        return num * contagionPerDamagePoint;
                    }                   
                    
                }
            }
                
            
            return 0;
        }
    }