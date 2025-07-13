using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(Alert_LowHemogen),"CalculateTargets")]
    class LowHemogenTargets_Patch
    {
        static FieldInfo fld_targets = AccessTools.Field(typeof(Alert_LowHemogen), "targets");
        static FieldInfo fld_targetLabels = AccessTools.Field(typeof(Alert_LowHemogen), "targetLabels");

        static void Postfix(Alert_LowHemogen __instance)
        {
            List<GlobalTargetInfo> targets = (List<GlobalTargetInfo>)fld_targets.GetValue(__instance);
            List<string> targetLabels = (List<string>)fld_targetLabels.GetValue(__instance);


            //Log.Message("LowHemogenTargets Postfix, targets: " + targets.ToStringSafeEnumerable()
            //    + ", targetLabels: " + targetLabels.ToStringSafeEnumerable());

            List<GlobalTargetInfo> toIterate = targets.ToList<GlobalTargetInfo>();

            foreach (GlobalTargetInfo gti in toIterate)
            {
                if (gti.HasThing && gti.Thing is Pawn pawn && pawn.genes?.GetFirstGeneOfType<Gene_Bodyfeeder>() != null)
                {
                    targets.Remove(gti);
                    targetLabels.Remove(pawn.NameShortColored.Resolve());
                }
            }

            //Log.Message("new targets: " + targets.ToStringSafeEnumerable()
            //    + ", targetLabels: " + targetLabels.ToStringSafeEnumerable());
        }
    }
}
