using RimWorld;
using Verse;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CyanobotsGenes
{
    [HarmonyPatch]
    public static class PlayLogEntry_Interaction_Patch
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.Constructor(typeof(PlayLogEntry_Interaction), new Type[] { typeof(InteractionDef), typeof(Pawn), typeof(Pawn), typeof(List<RulePackDef>) });
        }

        public static void Postfix(ref InteractionDef ___intDef, InteractionDef intDef, Pawn initiator, Pawn recipient)
        {
            InteractionDef newIntDef;

            if (initiator.genes != null)
            {
                foreach (Gene gene in initiator.genes.GenesListForReading)
                {
                    if (!gene.Active) continue;
                    GeneExtension_ReplacesInteractionRules extension = gene.def.GetModExtension<GeneExtension_ReplacesInteractionRules>();
                    if (extension == null || !extension.AffectedInteractions.Contains(intDef)) continue;
                    newIntDef = extension.ReplacementFor(intDef, true);
                    if (newIntDef != intDef)
                    {
                        ___intDef = newIntDef;
                        return;
                    }
                }
            }
            if (recipient.genes != null)
            {
                foreach (Gene gene in recipient.genes.GenesListForReading)
                {
                    if (!gene.Active) continue;
                    GeneExtension_ReplacesInteractionRules extension = gene.def.GetModExtension<GeneExtension_ReplacesInteractionRules>();
                    if (extension == null || !extension.AffectedInteractions.Contains(intDef)) continue;
                    newIntDef = extension.ReplacementFor(intDef, false);
                    if (newIntDef != intDef)
                    {
                        ___intDef = newIntDef;
                        return;
                    }
                }
            }
        }
    }

}
