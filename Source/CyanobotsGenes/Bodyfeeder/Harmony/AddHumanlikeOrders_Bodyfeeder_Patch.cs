using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using System.Collections.Generic;
using static CyanobotsGenes.BodyfeederUtility;
#if RW_1_5
namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(FloatMenuMakerMap),"AddHumanlikeOrders")]
    public static class AddHumanlikeOrders_Bodyfeeder_Patch
    {
        static void Postfix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            IntVec3 c = IntVec3.FromVector3(clickPos);
            foreach (Thing thing in c.GetThingList(pawn.Map))
            {
                if (thing is Corpse corpse && IsBodyFeeder(pawn) && !thing.IngestibleNow && CorpseIngestibleForHemogenNow(corpse))
                {
                    string text = (!thing.def.ingestible.ingestCommandString.NullOrEmpty()
                        ? (string)thing.def.ingestible.ingestCommandString.Formatted(thing.LabelShort)
                        : (string)"ConsumeThing".Translate(thing.LabelShort, thing));

                    text = string.Format("{0}: ({1})", text, "WarningFoodDisliked".Translate());

                    FloatMenuOption opt;
                    if (!pawn.CanReach(thing, PathEndMode.OnCell, Danger.Deadly))
                    {
                        opt = new FloatMenuOption(text + ": " + "NoPath".Translate().CapitalizeFirst(), null);
                    }
                    else
                    {
                        opt = FloatMenuUtility.DecoratePrioritizedTask(
                            new FloatMenuOption(text, delegate
                            {
                                thing.SetForbidden(false);
                                Job job = JobMaker.MakeJob(CG_DefOf.IngestForHemogen, thing);
                                job.count = 1;
                                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                            }, MenuOptionPriority.Low),
                            pawn, thing);
                    }
                    opts.Add(opt);
                }
            }
        }
    }
}
#endif