using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using static CyanobotsGenes.BodyfeederUtility;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(JobGiver_GetHemogen), "TryGiveJob")]
    class GetHemogen_Patch
    {
        static bool Prefix(ref Job __result, Pawn pawn)
        {
            if (pawn.genes == null || !pawn.genes.HasGene(CG_DefOf.Bodyfeeder))
                return true;

            Gene_Hemogen gene_Hemogen = pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>();
            if (gene_Hemogen == null)
            {
                __result = null;
                return false;
            }
            if (!gene_Hemogen.ShouldConsumeHemogenNow())
            {
                __result = null;
                return false;
            }

            Job jobIngest = MakeIngestForHemogenJob(pawn);
            Thing target = jobIngest?.targetA.Thing;
            //Log.Message("jobIngest: " + jobIngest + ", target: " + target);

            //if we couldn't find any human meat to eat, pass back to vanilla
            //in case other mods have added other hemogen-containing foods, etc
            if (jobIngest == null || target == null)
            {
                //Log.Message("passing back to vanilla");
                __result = null;
                return true;
            }

            Pawn holder = (target.ParentHolder as Pawn_InventoryTracker)?.pawn;
            if (holder != null && holder != pawn)
            {
                Job jobTakeFromOther = JobMaker.MakeJob(JobDefOf.TakeFromOtherInventory, target, holder);
                jobTakeFromOther.count = jobIngest.count;
                __result = jobTakeFromOther;
                //Log.Message("Returning takeFromOther, count: " + jobTakeFromOther.count);
                return false;
            }
            __result = jobIngest;
            //Log.Message("Returning jobIngest, count: " + jobIngest.count);
            return false;
        }

    }

    [HarmonyPatch(typeof(Pawn_GeneTracker))]
    class AggroMentalBreakSelectionChanceFactor_Patch
    {
        static MethodBase TargetMethod()
        {
            return typeof(Pawn_GeneTracker).GetProperty(nameof(Pawn_GeneTracker.AggroMentalBreakSelectionChanceFactor)
                , BindingFlags.Public | BindingFlags.Instance).GetGetMethod(false);
        }

        static float Postfix(float result, Pawn_GeneTracker __instance)
        {
            if (__instance.HasGene(CG_DefOf.Bodyfeeder) && __instance.pawn.health != null 
                && __instance.pawn.health.hediffSet.HasHediff(CG_DefOf.BodyfeederStarvation))
            {
                result *= 1f + (8f * __instance.pawn.health.hediffSet.GetFirstHediffOfDef(CG_DefOf.BodyfeederStarvation).Severity);
            }
            return result;
        }
    }

    [HarmonyPatch(typeof(GeneResourceDrainUtility),nameof(GeneResourceDrainUtility.PostResourceOffset))]
    class PostResourceOffset_Patch
    {
        public static bool Prefix(IGeneResourceDrain drain, float oldValue)
        {
            Pawn pawn = drain.Pawn;
            if (drain.Resource.GetType() == typeof(Gene_Hemogen) && pawn.genes.HasGene(CG_DefOf.Bodyfeeder))
            {
                if (oldValue > 0f && drain.Resource.Value <= 0f)
                {
                    if (!pawn.health.hediffSet.HasHediff(CG_DefOf.BodyfeederStarvation))
                    {
                        pawn.health.AddHediff(CG_DefOf.BodyfeederStarvation);
                    }
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Corpse), "IngestedCalculateAmounts")]
    class IngestedCalculateAmounts_Patch
    {
        public static void Postfix(float nutritionIngested, Pawn ingester, Corpse __instance)
        {
            if (IsBodyFeeder(ingester))
            {
                ingester.genes.GetFirstGeneOfType<Gene_Bodyfeeder>().Notify_IngestedCorpse(__instance, nutritionIngested);
            }
        }
    }

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

    [HarmonyPatch(typeof(FloatMenuMakerMap),"AddHumanlikeOrders")]
    class AddHumanlikeOrders_Bodyfeeder_Patch
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

    //[HarmonyPatch(typeof(RecipeDefGenerator),nameof(RecipeDefGenerator.ImpliedRecipeDefs))]
    class ImpliedRecipeDefs_Patch
    {
        public static void Postfix(ref IEnumerable<RecipeDef> __result, bool hotReload)
        {
            //Log.Message("Postfix ImpliedRecipeDefs");
            __result = __result.Concat(GenerateMeatAdministerDefs.MeatAdministerDefs(hotReload));
        }
    }
}
