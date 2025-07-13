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
                    + ", ___cachedLifeStageIndex: " + ___cachedLifeStageIndex
                    );
                float minAgeChild = gene_Precocious.MinAgeChild;
                if (__instance.AgeBiologicalYears < minAgeChild)
                {
                    //get life stage from Precocious gene instead of from age of pawn
                    int lifeStageIndex = gene_Precocious.IndexChild;

                    //replicate the work done by RecalculateLifeStageIndex since we're going to skip the main function
                    bool updating = lifeStageIndex != ___cachedLifeStageIndex;

                    if (updating)
                    {
                        ___lifeStageChange = true;
                        __instance.CheckChangePawnKindName();
                        LifeStageWorker newWorker = gene_Precocious.LsaChild.def.Worker;
                        LifeStageDef prevDef = (___cachedLifeStageIndex >= 0 && ___cachedLifeStageIndex < ___pawn.RaceProps.lifeStageAges.Count)
                            ? ___pawn.RaceProps.lifeStageAges[___cachedLifeStageIndex].def
                            : null;
                        LogUtil.DebugLog("newWorker: " + newWorker + ", prevDef: " + prevDef
                            + ", ProgramState: " + Current.ProgramState
                            + ", prevDef.developmentalStage.Baby(): " + prevDef.developmentalStage.Baby()
                            + ", bodyType: " + ___pawn.story.bodyType
                            );

                        ___cachedLifeStageIndex = lifeStageIndex;
                        if (newWorker != null && prevDef != null) newWorker.Notify_LifeStageStarted(___pawn, prevDef);

                        LogUtil.DebugLog("bodyType: " + ___pawn.story.bodyType
                            + ", GetBodyTypeFor: " + PawnGenerator.GetBodyTypeFor(___pawn)
                            );
                        //__instance.CurLifeStage.Worker.Notify_LifeStageStarted(___pawn, gene_Precocious.LsaChild.def);
                        if (___pawn.SpawnedOrAnyParentSpawned)
                        {
                            PawnComponentsUtility.AddAndRemoveDynamicComponents(___pawn);
                        }

                        ___pawn.Drawer.renderer.SetAllGraphicsDirty();
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
