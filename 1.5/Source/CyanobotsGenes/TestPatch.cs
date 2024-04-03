using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(PreceptComp_SelfTookMemoryThought), nameof(PreceptComp_SelfTookMemoryThought.Notify_MemberTookAction))]
    class PreceptComp_SelfTookMemoryThought_TestPatch
    {
        static void Prefix(HistoryEvent ev, Precept precept, bool canApplySelfTookThoughts, PreceptComp_SelfTookMemoryThought __instance)
        {
            if (precept.def.issue == DefDatabase<IssueDef>.GetNamed("Cannibalism"))
            {

                Log.Message("Notify_MemberTookAction - precept: " + precept.def
                    + ", PreceptComp: " + __instance.GetType()
                    + ", historyEvent: " + ev.def + ", eventDef: " + __instance.eventDef
                    + ", canApplySelfTookThoughts: " + canApplySelfTookThoughts);
            }
        }
    }

    /*
    [HarmonyPatch(typeof(PreceptComp), nameof(PreceptComp.Notify_MemberTookAction))]
    class PreceptComp_TestPatch
    {
        static void Prefix(HistoryEvent ev, Precept precept, bool canApplySelfTookThoughts, PreceptComp __instance)
        {
            if (precept.def.issue == DefDatabase<IssueDef>.GetNamed("Cannibalism"))
            {

                Log.Message("Notify_MemberTookAction - precept: " + precept.def
                    + ", PreceptComp: " + __instance.GetType()
                    + ", historyEvent: " + ev.def
                    + ", canApplySelfTookThoughts: " + canApplySelfTookThoughts);
            }
        }
    }
    */

    [HarmonyPatch(typeof(Ideo),nameof(Ideo.Notify_MemberTookAction))]
    class Ideo_TestPatch
    {
        static void Prefix(HistoryEvent ev, bool canApplySelfTookThoughts)
        {
            Log.Message("Notify_MemberTookAction - historyEvent: " + ev.def
                + ", canApplySelfTookThoughts: " + canApplySelfTookThoughts);
        }
    }

    [HarmonyPatch(typeof(IdeoUtility), nameof(IdeoUtility.Notify_HistoryEvent))]
    class IdeoUtility_TestPatch
    {
        static void Prefix(HistoryEvent ev, bool canApplySelfTookThoughts)
        {
            Log.Message("Notify_HistoryEvent - historyEvent: " + ev.def
                + ", canApplySelfTookThoughts: " + canApplySelfTookThoughts);
            ev.args.TryGetArg(HistoryEventArgsNames.Doer, out Pawn pawn);
            Log.Message("try get doer: " + pawn);
        }
    }

    [HarmonyPatch(typeof(HistoryEventsManager),nameof(HistoryEventsManager.RecordEvent))]
    class HistoryEventsManager_TestPatch
    {
        static void Prefix(HistoryEvent historyEvent, bool canApplySelfTookThoughts)
        {
            Log.Message("RecordEvent - historyEvent: " + historyEvent.def
                + ", canApplySelfTookThoughts: " + canApplySelfTookThoughts);
        }
    }

    [HarmonyPatch(typeof(CompIngredients),nameof(CompIngredients.AllowStackWith))]
    class AllowStackWith_TestPatch
    {
        static MethodInfo mtd_MergeIngredients = AccessTools.Method(typeof(CompIngredients), "MergeIngredients");

        static void Postfix(bool __result, CompIngredients __instance, Thing otherStack)
        {
            Thing firstStack = __instance.parent;

            Log.Message("AllowStackWith, firstStack: " + firstStack + ", otherStack: " + otherStack
                + ", result: " + __result);
            CompIngredients otherComp = otherStack.TryGetComp<CompIngredients>();
            Log.Message("performMergeCompatibilityChecks - first: " + __instance.Props.performMergeCompatibilityChecks
                + ", other:  " + otherComp.Props.performMergeCompatibilityChecks);
            Log.Message("GetFoodKindForStacking - first: " + FoodUtility.GetFoodKindForStacking(firstStack)
                    + ", other: " + FoodUtility.GetFoodKindForStacking(otherStack)
                    + ", equal: " + (FoodUtility.GetFoodKindForStacking(firstStack) == FoodUtility.GetFoodKindForStacking(otherStack)));
            Log.Message("MergeCompatibilityTags - first: " + __instance.MergeCompatibilityTags.ToStringSafeEnumerable()
                + "; other: " + otherComp.MergeCompatibilityTags.ToStringSafeEnumerable()
                + "; counts equal: " + (__instance.MergeCompatibilityTags.Count == otherComp.MergeCompatibilityTags.Count));

            List<ThingDef> tmpIngredients = new List<ThingDef>();
            tmpIngredients.AddRange(__instance.ingredients);
            object[] parms = new object[] { tmpIngredients , otherComp.ingredients, null, firstStack.def };
            mtd_MergeIngredients.Invoke(null, parms);
            Log.Message("Aftermath of MergeIngredients - tmpIngredients: " + tmpIngredients.ToStringSafeEnumerable()
                + "; lostImportantIngredients: " + parms[2]);

            foreach (ThingDef ingredient in otherComp.ingredients)
            {
                tmpIngredients.AddDistinct(ingredient);
            }
            Log.Message("tmpIngredients after AddDistinct: " + tmpIngredients.ToStringSafeEnumerable()
                + "; count: " + tmpIngredients.Count);
        }
    }

}
