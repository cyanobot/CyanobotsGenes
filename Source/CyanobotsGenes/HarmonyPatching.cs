using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CyanobotsGenes
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatching
    {
        static HarmonyPatching()
        {
            var harmony = new Harmony("com.cyanobot.flushbiotuning");
            harmony.PatchAll();
        }
    }


    [HarmonyPatch(typeof(Need_Joy), "FallPerInterval", MethodType.Getter)]
    class Need_Joy_Patch
    {
        static void Postfix(ref float __result, Need_Joy __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue() as Pawn;
            float JoyFallFactor = pawn.GetStatValue(CG_DefOf.JoyFallRateFactor, true, -1);
            __result *= JoyFallFactor;
        }
    }

    [HarmonyPatch(typeof(MemoryThoughtHandler), "TryGainMemoryFast")]
    class WetFur_Patch
    {
        static void Prefix(ThoughtDef mem, Pawn ___pawn)
        {
            if (mem == ThoughtDef.Named("SoakingWet") && ___pawn.genes.HasGene(CG_DefOf.LightFur))
            {
                ___pawn.needs.mood.thoughts.memories.TryGainMemoryFast(CG_DefOf.WetFur);
            }
        }
    }

    [HarmonyPatch(typeof(JobDriver_Hunt), "MakeNewToils")]
    class HuntToils_Patch
    {
        public static IEnumerable<Toil> Postfix(IEnumerable<Toil> originalToils, JobDriver_Hunt __instance)
        {
            Pawn pawn = __instance.pawn;
            bool preyDrive = pawn.genes != null && pawn.genes.HasGene(CG_DefOf.PreyDrive);

            foreach (Toil toil in originalToils)
            {
                if (preyDrive)
                {
                    //Log.Message("Trying to add action to toil " + toil.debugName);
                    toil.AddPreTickAction(delegate
                    {
                        Log.Message("Fired extra action");
                        JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.None);
                    });
                }
                yield return toil;
            }
        }
    }

    [HarmonyPatch(typeof(JoyUtility),nameof(JoyUtility.JoyTickCheckEnd))]
    class JoyTick_Patch
    {
        static bool Prefix(Pawn pawn)
        {
            //don't terminate hunting jobs if we have no joy need
            if (pawn.CurJob.def == JobDefOf.Hunt && pawn.needs.joy == null) return false;
            return true;
        }
    }

}
