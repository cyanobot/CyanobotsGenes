using RimWorld;
using Verse;
using HarmonyLib;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(JoyUtility),nameof(JoyUtility.JoyTickCheckEnd))]
    class JoyTick_Patch
    {
        static bool Prefix(Pawn pawn)
        {
            //don't terminate hunting jobs or visiting sick pawns if we have no joy need
            if ((pawn.CurJob.def == JobDefOf.Hunt || pawn.CurJob.def == JobDefOf.VisitSickPawn) && pawn.needs.joy == null) return false;
            return true;
        }
    }

}
