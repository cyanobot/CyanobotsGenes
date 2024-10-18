using RimWorld;
using Verse;
using HarmonyLib;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(Pawn_RelationsTracker),nameof(Pawn_RelationsTracker.OpinionOf))]
    public static class OpinionOf_Patch
    {
        public static void Postfix(ref int __result, Pawn ___pawn, Pawn other)
        {
            if (!___pawn.Dead && ___pawn.HasActiveGene(CG_DefOf.CYB_Afamilial))
            {
                foreach (PawnRelationDef relation in ___pawn.GetRelations(other))
                {
                    if (relation.familyByBloodRelation)
                        __result -= relation.opinionOffset;
                }
            }
        }
    }

}
