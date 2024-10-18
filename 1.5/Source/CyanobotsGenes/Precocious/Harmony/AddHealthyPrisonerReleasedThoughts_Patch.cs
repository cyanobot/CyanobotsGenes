using RimWorld;
using Verse;
using HarmonyLib;
using static CyanobotsGenes.PrecociousUtil;

namespace CyanobotsGenes
{
    //no positive thought for releasing precocious prisoners
    //to prevent farming
    //and also because how well are they really going to do if kicked out into the wild?
    [HarmonyPatch(typeof(GenGuest),nameof(GenGuest.AddHealthyPrisonerReleasedThoughts))]
    public static class AddHealthyPrisonerReleasedThoughts_Patch
    {
        public static bool Prefix(Pawn prisoner)
        {
            if (IsPrecociousBaby(prisoner, out var _))
            {
                return false;
            }
            return true;
        }
    }
}
