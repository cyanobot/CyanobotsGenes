using RimWorld;
using Verse;
using HarmonyLib;
using System.Collections.Generic;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(Pawn),nameof(Pawn.GetReasonsForDisabledWorkType))]
    static public class GetReasonsForDisabledWorkType_Patch
    {
        static public void Postfix(Pawn __instance, ref List<string> __result, WorkTypeDef workType, Dictionary<WorkTypeDef, List<string>> ___cachedReasonsForDisabledWorkTypes)
        {
            if (__instance.genes == null && !__instance.genes.GenesListForReading.NullOrEmpty()) return;
            foreach (Gene gene in __instance.genes.GenesListForReading)
            {
                GeneExtension_DisabledWorkTypes disabledWorkTypes = gene.def.GetModExtension<GeneExtension_DisabledWorkTypes>();
                if (disabledWorkTypes?.workTypes?.Contains(workType) ?? false)
                {
                    string reason = "CYB_WorkDisabledGene".Translate(gene.LabelCap);
                    //__result.Add(reason);
                    if (!___cachedReasonsForDisabledWorkTypes.ContainsKey(workType))
                    {
                        ___cachedReasonsForDisabledWorkTypes.Add(workType, new List<string>());
                    }
                    if (!___cachedReasonsForDisabledWorkTypes[workType].Contains(reason))
                    {
                        ___cachedReasonsForDisabledWorkTypes[workType].Add(reason);
                    }
                    break;
                }
            }
            //___cachedReasonsForDisabledWorkTypes[workType] = __result;
        }

    }

}
