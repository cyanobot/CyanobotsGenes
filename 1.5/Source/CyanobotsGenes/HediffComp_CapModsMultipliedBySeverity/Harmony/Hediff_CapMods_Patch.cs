using RimWorld;
using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace CyanobotsGenes
{
    [HarmonyPatch]
    static public class Hediff_CapMods_Patch
    {
        static public MethodInfo TargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(Hediff), nameof(Hediff.CapMods));
        }

        static public void Postfix(Hediff __instance, ref List<PawnCapacityModifier> __result)
        {
            //Log.Message("Firing CapMods Postfix for hediff: " + __instance + ", pawn: " + __instance.pawn);
            //Log.Message("CapMods: " + __result.Select<PawnCapacityModifier, string>(
            //    x => "[capacity: " + x.capacity + ", offset: " + x.offset + ", postFactor: " + x.postFactor + "]"
            //    ).ToStringSafeEnumerable());
            if (__result.NullOrEmpty()) return;
            if (__instance.TryGetComp<HediffComp_CapModsMultipliedBySeverity>(out HediffComp_CapModsMultipliedBySeverity comp))
            {
                //Log.Message("Found comp. Severity: " + __instance.Severity);
                List<PawnCapacityModifier> newResult = new List<PawnCapacityModifier>();
                foreach (PawnCapacityModifier modifier in __result)
                {
                    //Log.Message("modifier:" + StringFromCapMod(modifier));
                    PawnCapacityModifier newModifier = new PawnCapacityModifier
                    {
                        capacity = modifier.capacity,
                        offset = modifier.offset * __instance.Severity,
                        postFactor = StatWorker.ScaleFactor(modifier.postFactor, __instance.Severity)
                    };
                    newResult.Add(newModifier);
                }
                __result = newResult;
                //Log.Message("Final result: " + __result.Select<PawnCapacityModifier, string>(
                //    x => StringFromCapMod(x)
                //    ).ToStringSafeEnumerable());
            }
        }
        
        static public string StringFromCapMod(PawnCapacityModifier capMod)
        {
            return "[capacity: " + capMod.capacity
                + ", offset: " + capMod.offset
                + ", postFactor: " + capMod.postFactor + "]"
                + ", setMax: " + capMod.setMax;
        }
    }

}
