using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TrainworksReloaded.Base;
using TrainworksReloaded.Core;
using TrainworksReloaded.Core.Interfaces;
using TrainworksReloaded.Core.Extensions;
using TrainworksReloaded.Base.Extensions;
using Conductor.UI;

namespace PathogensClan_MT2.Plugin
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger = new(MyPluginInfo.PLUGIN_GUID);
        internal static Lazy<ToxicityManager> Client = new(() => new ToxicityManager());
        // Plugin startup logic. This function is automatically called when your plugin initializes
        public void Awake()
        {
            Logger = base.Logger;

            // Setup Game Client
            var client = Client.Value;
            DepInjector.AddProvider(client);

            var builder = Railhead.GetBuilder();
            builder.Configure(
                MyPluginInfo.PLUGIN_GUID,
                c =>
                {
                    // Be sure to include any new json files if you add more.
                    c.AddMergedJsonFile(
                        "json/global.json",
                        // Clan Definition
                        "json/clan/clan.json",
                        // Required: Clan Map Banner Node definition and Card Pool
                        "json/clan/clan_banner.json",
                        // Optional: Custom Card Border (if removed edit clan.json)
                        "json/clan/clan_card_frame.json",
                        // Optional: Custom Clan Subtypes used by units.
                        "json/clan/clan_subtypes.json",
                        // HUD
                        "json/clan/hud.json",
                        // Required: A Clan Champion with one Upgrade Path.
                        "json/champions/basic_champion.json",
                        // Spell Cards
                        "json/spells/nausea.json",
                        "json/spells/queue_time.json",
                        "json/spells/rampant_infection.json",
                        "json/spells/spelleogenesis.json",
                        "json/spells/survival_of_the_prolific.json",
                        "json/spells/sympathetic_emesis.json",
                        "json/spells/viral_outcropping.json",
                        // Starter (required).
                        "json/spells/basic_starter.json",
                        "json/spells/basic_common.json",
                        // Units
                        "json/units/basic_ability_unit.json",
                        "json/units/basic_banner_unit.json",
                        "json/units/SimplexvirusDiabolicus.json",
                        // Non Banner Unit, that is draftable as a Battle Reward.
                        "json/units/basic_draft_unit.json",
                        "json/units/RecombinantVirus.json",
                        // Equipment
                        "json/equipment/basic_equipment.json",
                        // Rooms
                        "json/rooms/basic_room.json",
                        // Shop Upgrades (Enhancer)
                        "json/enhancers/basic_enhancer.json",
                        // Artifacts
                        "json/relics/basic_artifact.json",
                        // TrackedValues
                        "json/tracked_values/toxicity.json",
                        // StatusEffects
                        "json/status_effects/contagionstatus.json",
                        "json/status_effects/recoilstatus.json",
                        "json/status_effects/replicatestatus.json"
                    );
                }
            );
            Railend.ConfigurePreAction(builder =>
            {
                builder.RegisterInstance(client);
            });
            Railend.ConfigurePostAction(
                c =>
                {
                    var trackedValueRegister = c.GetInstance<IRegister<CardStatistics.TrackedValueType>>();
                    var statusEffectTriggerStageManager = c.GetInstance<IRegister<StatusEffectData.TriggerStage>>();
                   
                    CardStatistics.TrackedValueType GetTrackedValueType(string id)
                    {
                        return trackedValueRegister.GetValueOrDefault(MyPluginInfo.PLUGIN_GUID.GetId(TemplateConstants.TrackedValueTypeEnum, id));
                    }
                    TrackedValues.Toxicity = GetTrackedValueType("Toxicity");

                     StatusEffectData.TriggerStage GetStatusEffectTriggerStage(string id)
                    {
                        return statusEffectTriggerStageManager.GetValueOrDefault(MyPluginInfo.PLUGIN_GUID.GetId(TemplateConstants.StatusEffectTriggerStageEnum, id));
                    }
                    StatusEffectTriggers.OnFullToxicity = GetStatusEffectTriggerStage("OnFullToxicity");
                    StatusEffectTriggers.OnFullToxicityLost = GetStatusEffectTriggerStage("OnFullToxicityLost");

                    
                    var hud = HudManager.GetHUD(MyPluginInfo.PLUGIN_GUID, "PathogensToxicityUI");
                    hud?.SetTrackedValueHandler(Client.Value);
                    
                });

            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            
            var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

        }
    }
}
