using System.Collections;
using TrainworksReloaded.Base;
using TrainworksReloaded.Base.Extensions;
using TrainworksReloaded.Core;
using TrainworksReloaded.Core.Interfaces;
using UnityEngine;

namespace PathogensClan_MT2.Plugin
{

public class CardEffectRecursionPerBlessing : CardEffectBase
{
    public override bool CanApplyInPreviewMode => false;
    public override PropDescriptions CreateEditorInspectorDescriptions() => new();
    public override IEnumerator ApplyEffect(CardEffectState state, CardEffectParams p, ICoreGameManagers core, ISystemManagers systems)
    {
        // Hover previews share the real CardManager and hand: never resurrect/draw there.
        if (core.GetSaveManager().PreviewMode || p.selfTarget == null || p.selfTarget.PreviewMode) yield break;
        int count = p.selfTarget?.GetStatusEffectStacks("stygian_blessing") ?? 0;
        var manager = core.GetCardManager();
        for (int i = 0; i < count; i++)
        {
            var candidates = manager.GetExhaustedPile().Where(card => card.GetCardType() == CardType.Spell).ToList();
            if (candidates.Count == 0) yield break;
            var card = candidates[RandomManager.Range(0, candidates.Count, RngId.Battle)];
            if (!manager.RestoreExhaustedOrEatenCard(card)) yield break;
            manager.DrawSpecificCard(card, false, HandUI.DrawSource.Consume, p.playedCard, i + 1, count);
        }
        yield break;
    }
}
}
