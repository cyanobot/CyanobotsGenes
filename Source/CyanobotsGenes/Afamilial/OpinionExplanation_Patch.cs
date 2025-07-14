using RimWorld;
using Verse;
using HarmonyLib;
using System;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.OpinionExplanation))]
    public static class OpinionExplanation_Patch
    {
        public static void Postfix(ref string __result, Pawn ___pawn, Pawn other)
        {
            if (!___pawn.Dead && ___pawn.HasActiveGene(CG_DefOf.CYB_Afamilial))
            {
                LogUtil.DebugLog("Editing OpinionExplanation for Afamilial pawn " + ___pawn
                    + ", original result: " + __result);
                foreach (PawnRelationDef relation in ___pawn.GetRelations(other))
                {
                    if (relation.familyByBloodRelation)
                    {
                        int relationLineIndex = __result.IndexOf(relation.GetGenderSpecificLabelCap(other)) - 3;
                        if (relationLineIndex == -1) continue;
                        relationLineIndex -= Environment.NewLine.Length;
                        int relationLineLength = 5 
                            + relation.GetGenderSpecificLabelCap(other).Length 
                            + relation.opinionOffset.ToStringWithSign().Length
                            + Environment.NewLine.Length;
                        LogUtil.DebugLog("Found blood relation: " + relation
                            + ", GenderSpecificLabel: " + relation.GetGenderSpecificLabelCap(other)
                            + ", relationLineIndex: " + relationLineIndex
                            + ", relationLineLength: " + relationLineLength
                            + ", __result.Length: " + __result.Length
                            + ", NewLine.Length: " + Environment.NewLine.Length);

                        if (relationLineIndex + relationLineLength > __result.Length)
                        {
                            continue;
                        }

                        __result = __result.Remove(relationLineIndex, relationLineLength);
                    }
                }
            }
        }
    }

}
