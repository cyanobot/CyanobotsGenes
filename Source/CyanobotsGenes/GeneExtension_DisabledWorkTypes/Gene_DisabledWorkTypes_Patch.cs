using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CyanobotsGenes
{
    [HarmonyPatch]
    static public class Gene_DisabledWorkTypes_Patch
    {
        static public MethodInfo TargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(Gene), nameof(Gene.DisabledWorkTypes));
        }

        static public void Postfix(Gene __instance, ref IEnumerable<WorkTypeDef> __result)
        {
            //Log.Message("DisabledWorkTypes Postfix firing");
            GeneExtension_DisabledWorkTypes disabledWorkTypes = __instance.def.GetModExtension<GeneExtension_DisabledWorkTypes>();
            if (!disabledWorkTypes?.workTypes.NullOrEmpty() ?? false)
            {
                foreach (WorkTypeDef workType in disabledWorkTypes.workTypes)
                {
                    //Log.Message("Trying to add disabled work type: " + workType + " for gene " + __instance.def.defName);
                    if (!__result.Contains(workType)) __result = __result.AddItem(workType);
                }
                //Log.Message("Final __result: " + __result.ToStringSafeEnumerable());
            }
        }
    }

}
