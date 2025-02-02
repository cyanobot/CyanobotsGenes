﻿using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;
using System.Collections.Generic;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(JobDriver_Hunt), "MakeNewToils")]
    class HuntToils_Patch
    {
        public static IEnumerable<Toil> Postfix(IEnumerable<Toil> originalToils, JobDriver_Hunt __instance)
        {
            __instance.job.ignoreJoyTimeAssignment = true;

            Pawn pawn = __instance.pawn;
            bool preyDrive = pawn.genes != null && pawn.HasActiveGene(CG_DefOf.PreyDrive);

            foreach (Toil toil in originalToils)
            {
                if (preyDrive)
                {
                    //Log.Message("Trying to add action to toil " + toil.debugName);
                    toil.AddPreTickAction(delegate
                    {
                        //Log.Message("Fired extra action");
                        JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.None);
                    });
                }
                yield return toil;
            }
        }
    }

}
