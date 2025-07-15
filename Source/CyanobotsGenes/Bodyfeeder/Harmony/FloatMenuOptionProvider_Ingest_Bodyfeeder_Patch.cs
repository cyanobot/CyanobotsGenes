using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using System.Collections.Generic;
using static CyanobotsGenes.BodyfeederUtility;
using System;
using System.Net.NetworkInformation;

#if RW_1_5
#else
namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(FloatMenuOptionProvider_Ingest), "GetSingleOptionFor",new Type[] {typeof(Thing),typeof(FloatMenuContext)})]
    public static class FloatMenuOptionProvider_Ingest_Bodyfeeder_Patch
    {
        public static FloatMenuOption Postfix(FloatMenuOption result, Thing clickedThing, FloatMenuContext context)
        {
            //only want to interfere with things that aren't already edible
            if (result != null) return result;

            Pawn pawn = context.FirstSelectedPawn;

            if (clickedThing is Corpse corpse && IsBodyFeeder(pawn) && CorpseIngestibleForHemogenNow(corpse))
            {
                string text = (!clickedThing.def.ingestible.ingestCommandString.NullOrEmpty()
                ? (string)clickedThing.def.ingestible.ingestCommandString.Formatted(clickedThing.LabelShort)
                : (string)"ConsumeThing".Translate(clickedThing.LabelShort, clickedThing));

                text = string.Format("{0}: ({1})", text, "WarningFoodDisliked".Translate());

                FloatMenuOption opt;
                if (!pawn.CanReach(clickedThing, PathEndMode.OnCell, Danger.Deadly))
                {
                    opt = new FloatMenuOption(text + ": " + "NoPath".Translate().CapitalizeFirst(), null);
                }
                else
                {
                    opt = FloatMenuUtility.DecoratePrioritizedTask(
                        new FloatMenuOption(text, delegate
                        {
                            clickedThing.SetForbidden(false);
                            Job job = JobMaker.MakeJob(CG_DefOf.CYB_IngestForHemogen, clickedThing);
                            job.count = 1;
                            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                        }, MenuOptionPriority.Low),
                        pawn, clickedThing);
                }

                return opt;
            }
            return result;
        }
    }
}
#endif