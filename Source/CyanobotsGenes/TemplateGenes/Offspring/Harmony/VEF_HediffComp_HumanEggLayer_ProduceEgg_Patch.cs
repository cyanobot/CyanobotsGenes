using HarmonyLib;
using Verse;
using RimWorld;
using static CyanobotsGenes.OffspringUtility;
using VanillaGenesExpanded;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(HediffComp_HumanEggLayer),nameof(HediffComp_HumanEggLayer.ProduceEgg))]
    public static class VEF_HediffComp_HumanEggLayer_ProduceEgg_Patch
    {
        public static void Postfix(ref Thing __result)
        {
            CompHumanHatcher comphumanHatcher = __result.TryGetComp<CompHumanHatcher>();
            if (comphumanHatcher == null) return;

            Pawn mother = comphumanHatcher.hatcheeParent;
            Pawn father = comphumanHatcher.otherParent;

            XenotypeDef offspringXenotype = GetOffspringXenotype(mother, father);
            if (offspringXenotype == null) return;

            comphumanHatcher.femaleDominant = true;
            comphumanHatcher.maleDominant = false;
            comphumanHatcher.motherGenes = offspringXenotype.AllGenes;
        }
    }

    /*
    [HarmonyPatch(typeof(HediffComp),nameof(HediffComp.CompGetGizmos))]
    public static class HediffComp_CompGetGizmos_Patch
    {
        public static FieldInfo f_eggProgress = AccessTools.Field(typeof(HediffComp_HumanEggLayer), "eggProgress");
        public static FieldInfo f_fertilizationCount = AccessTools.Field(typeof(HediffComp_HumanEggLayer), "fertilizationCount");
        public static FieldInfo f_fertilizedBy = AccessTools.Field(typeof(HediffComp_HumanEggLayer), "fertilizedBy");

        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> result, HediffComp __instance)
        {
            if (!result.EnumerableNullOrEmpty())
            {
                foreach (Gizmo gizmo in result)
                {
                    yield return gizmo;
                }
            }

            if (__instance is HediffComp_HumanEggLayer compEggLayer)
            {
                Command_Action logInfo = new Command_Action();
                logInfo.defaultLabel = "Log Info";
                logInfo.action = delegate
                {
                    Log.Message("HediffComp_HumanEggLayer - "
                        + "pawn - " + __instance.parent.pawn
                        + ", eggProgress: " + f_eggProgress.GetValue(compEggLayer)
                        + ", fertilizationCount: " + f_fertilizationCount.GetValue(compEggLayer)
                        + ", fertilizedBy: " + f_fertilizedBy.GetValue(compEggLayer)
                        + ", motherGenes: [" + compEggLayer.motherGenes.ToStringSafeEnumerable()
                        + "], fatherGenes: [" + compEggLayer.fatherGenes.ToStringSafeEnumerable()
                        + "]"
                        );
                };
                yield return logInfo;


                Command_Action layNow = new Command_Action();
                layNow.defaultLabel = "Lay Now";
                layNow.action = delegate
                {
                    f_eggProgress.SetValue(compEggLayer, 1f);
                };
                yield return layNow;

                Command_Target fertilize = new Command_Target();
                fertilize.defaultLabel = "Fertilize...";
                fertilize.targetingParams = new TargetingParameters
                {
                    canTargetPawns = true,
                    canTargetBuildings = false,
                    canTargetHumans = true,
                    canTargetAnimals = false,
                    canTargetMechs = false
                };
                fertilize.action = delegate(LocalTargetInfo target)
                {
                    Pawn mother = __instance.parent.pawn;
                    if (target.TryGetPawn(out Pawn targetPawn))
                    {
                        compEggLayer.Fertilize(targetPawn);
                        if (targetPawn?.genes != null)
                        {
                            foreach (Gene endogene in targetPawn.genes.Endogenes)
                            {
                                compEggLayer.fatherGenes.Add(endogene.def);
                            }
                        }
                        if (mother.genes == null)
                        {
                            return;
                        }
                        foreach (Gene endogene2 in mother.genes.Endogenes)
                        {
                            compEggLayer.motherGenes.Add(endogene2.def);
                        }
                    }                    
                };
                yield return fertilize;

            }
        }
    }
    */
}
