using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(FoodUtility), "ThoughtsFromIngesting")]
    class FoodThoughts_Patch
    {
        static void Postfix(ref List<FoodUtility.ThoughtFromIngesting> __result, Pawn ingester, Thing foodSource)
        {
            //LogUtil.DebugLog("ThoughtsFromIngesting Postfix, initial __result: " + __result.ToStringSafeEnumerable()
            //    + ", [0]: " + (__result.Count > 0 ? __result[0].thought.ToString() : ""));

            if (!ingester.RaceProps.Humanlike || ingester.genes == null) return;

            DietCategory dietCategory = GeneticDietUtility.GetDietCategory(ingester);
            if (dietCategory == DietCategory.Default) return;

            if (GeneticDietUtility.DietDislikes(foodSource, ingester))
            {
                //LogUtil.DebugLog("Disliked food");
                FoodUtility.ThoughtFromIngesting thoughtFromIngesting = new FoodUtility.ThoughtFromIngesting
                {
                    thought = GeneticDietUtility.GetDietThought(ingester, foodSource),
                    fromPrecept = null
                };
                __result.Add(thoughtFromIngesting);
                __result.RemoveAll(x => x.thought == DefDatabase<ThoughtDef>.GetNamed("AteFineMeal") || x.thought == DefDatabase<ThoughtDef>.GetNamed("AteLavishMeal"));
            }
            if (foodSource is Corpse && dietCategory == DietCategory.Hypercarnivore)
            {
                //LogUtil.DebugLog("Hypercarnivore and corpse");
                FoodUtility.ThoughtFromIngesting thoughtAteCorpse = __result.Find(x => x.thought == ThoughtDefOf.AteCorpse);
                if (thoughtAteCorpse.thought != null)
                {
                    __result.Remove(thoughtAteCorpse);
                    __result.Add(new FoodUtility.ThoughtFromIngesting { thought = CG_DefOf.CYB_AteCorpseHypercarnivore, fromPrecept = null });
                }
            }

            //LogUtil.DebugLog("final __result: " + __result.ToStringSafeEnumerable()
            //    + ", [0]: " + (__result.Count > 0 ? __result[0].thought.ToString() : ""));
        }
    }

}
