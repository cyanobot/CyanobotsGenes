using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(Pawn_AgeTracker), "RecalculateLifeStageIndex", new Type[] { })]
    public static class RecalculateLifeStageIndex_Patch
    {
        public static bool Prefix(Pawn ___pawn, Pawn_AgeTracker __instance,
            ref int ___cachedLifeStageIndex, ref bool ___lifeStageChange)
        {
            //only interested in precocious pawns
            Gene_Precocious gene_Precocious = (Gene_Precocious)___pawn.genes?.GetGene(CG_DefOf.CYB_Precocious);
            //LogUtil.DebugLog("RecalculateLifeStageIndex_Patch - ___pawn: " + ___pawn
            //    + ", gene_Precocious: " + gene_Precocious
            //    + ", cachedLifeStageIndex: " + ___cachedLifeStageIndex
            //    );

            if (gene_Precocious != null && gene_Precocious.Active)
            {
                //only interfere if they would come out younger than child
                LogUtil.DebugLog("minAgeChild: " + gene_Precocious.MinAgeChild
                    + ", AgeBiologicalYears: " + __instance.AgeBiologicalYears
                    + ", lifeStageIndex: " + gene_Precocious.IndexChild
                    );
                float minAgeChild = gene_Precocious.MinAgeChild;
                if (__instance.AgeBiologicalYears < minAgeChild)
                {
                    //get life stage from Precocious gene instead of from age of pawn
                    int lifeStageIndex = gene_Precocious.IndexChild;

                    //replicate the work done by RecalculateLifeStageIndex since we're going to skip the main function
                    bool updating = lifeStageIndex != ___cachedLifeStageIndex;

                    ___cachedLifeStageIndex = lifeStageIndex;
                    if (updating)
                    {
                        ___lifeStageChange = true;
                        ___pawn.Drawer.renderer.SetAllGraphicsDirty();
                        __instance.CheckChangePawnKindName();


                        __instance.CurLifeStage.Worker.Notify_LifeStageStarted(___pawn, gene_Precocious.LsaChild.def);
                        if (___pawn.SpawnedOrAnyParentSpawned)
                        {
                            PawnComponentsUtility.AddAndRemoveDynamicComponents(___pawn);
                        }
                    }

                    //skip main function
                    return false;
                }
            }
            //otherwise return to vanilla
            return true;
        }
    }
}
