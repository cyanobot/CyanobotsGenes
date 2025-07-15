using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using static CyanobotsGenes.BodyfeederUtility;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(JobGiver_GetHemogen), "TryGiveJob")]
    class GetHemogen_Patch
    {
        static bool Prefix(ref Job __result, Pawn pawn)
        {
            if (!pawn.HasActiveGene(CG_DefOf.CYB_Bodyfeeder))
                return true;

            Gene_Hemogen gene_Hemogen = pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>();
            if (gene_Hemogen == null)
            {
                __result = null;
                return false;
            }
            if (!gene_Hemogen.ShouldConsumeHemogenNow())
            {
                __result = null;
                return false;
            }

            Job jobIngest = MakeIngestForHemogenJob(pawn);
            Thing target = jobIngest?.targetA.Thing;
            //Log.Message("jobIngest: " + jobIngest + ", target: " + target);

            //if we couldn't find any human meat to eat, pass back to vanilla
            //in case other mods have added other hemogen-containing foods, etc
            if (jobIngest == null || target == null)
            {
                //Log.Message("passing back to vanilla");
                __result = null;
                return true;
            }

            Pawn holder = (target.ParentHolder as Pawn_InventoryTracker)?.pawn;
            if (holder != null && holder != pawn)
            {
                Job jobTakeFromOther = JobMaker.MakeJob(JobDefOf.TakeFromOtherInventory, target, holder);
                jobTakeFromOther.count = jobIngest.count;
                __result = jobTakeFromOther;
                //Log.Message("Returning takeFromOther, count: " + jobTakeFromOther.count);
                return false;
            }
            __result = jobIngest;
            //Log.Message("Returning jobIngest, count: " + jobIngest.count);
            return false;
        }

    }
}
