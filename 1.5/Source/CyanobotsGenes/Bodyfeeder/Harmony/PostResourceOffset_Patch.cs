using HarmonyLib;
using RimWorld;
using Verse;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(GeneResourceDrainUtility),nameof(GeneResourceDrainUtility.PostResourceOffset))]
    class PostResourceOffset_Patch
    {
        public static bool Prefix(IGeneResourceDrain drain, float oldValue)
        {
            Pawn pawn = drain.Pawn;
            if (drain.Resource.GetType() == typeof(Gene_Hemogen) && pawn.HasActiveGene(CG_DefOf.Bodyfeeder))
            {
                if (oldValue > 0f && drain.Resource.Value <= 0f)
                {
                    if (!pawn.health.hediffSet.HasHediff(CG_DefOf.BodyfeederStarvation))
                    {
                        pawn.health.AddHediff(CG_DefOf.BodyfeederStarvation);
                    }
                }
                return false;
            }
            return true;
        }
    }
}
