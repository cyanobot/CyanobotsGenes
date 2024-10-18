using RimWorld;
using Verse;
using HarmonyLib;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.PreApplyDamage))]
    public static class PreApplyDamage_Patch
    {
        public static void Postfix(Pawn ___pawn, DamageInfo dinfo, bool absorbed)
        {
            if (___pawn.RaceProps.Humanlike)
            {
                Pawn instigator = dinfo.Instigator as Pawn;
                if (instigator != null && instigator.RaceProps.Humanlike && PawnUtility.ShouldGetThoughtAbout(instigator, ___pawn))
                {
                    if (ThoughtUtility.CanGetThought(instigator, CG_DefOf.CYB_ViolenceAverse_Violence))
                        instigator.needs.mood.thoughts.memories.TryGainMemory(CG_DefOf.CYB_ViolenceAverse_Violence);
                }
            }
        }
    }

}
