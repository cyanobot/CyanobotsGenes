using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(FoodUtility), "NutritionForEater")]
    class Nutrition_Patch
    {
        static void Postfix(ref float __result, Pawn eater, Thing food)
        {
            //uninterested in nonhumans
            if (eater == null || !eater.RaceProps.Humanlike) return;

            float nutritionFactor = GeneticDietUtility.NutritionFactorFromGeneticDiet(food, eater);
            __result *= nutritionFactor;
        }
    }

    [HarmonyPatch(typeof(Corpse), "IngestedCalculateAmounts")]
    class CorpseNutrition_Patch
    {
        static void Postfix(ref float nutritionIngested, Corpse __instance, Pawn ingester)
        {
            //uninterested in nonhumans
            if (ingester == null || !ingester.RaceProps.Humanlike) return;
            nutritionIngested *= ingester.GetStatValue(CG_DefOf.AnimalNutritionFactor);
        }
    }

    [HarmonyPatch(typeof(FoodUtility), "ThoughtsFromIngesting")]
    class FoodThoughts_Patch
    {
        static void Postfix(ref List<FoodUtility.ThoughtFromIngesting> __result, Pawn ingester, Thing foodSource)
        {
            if (!ingester.RaceProps.Humanlike || ingester.genes == null) return;

            if (GeneticDietUtility.IsGeneticCannibal(ingester) && FoodUtility.IsHumanlikeCorpseOrHumanlikeMeat(foodSource, foodSource.def))
            {
                if (foodSource is Corpse)
                {
                    FoodUtility.ThoughtFromIngesting thoughtAteCorpse = __result.Find(x => x.thought == ThoughtDefOf.AteCorpse);
                    if (thoughtAteCorpse.thought != null)
                    {
                        __result.Remove(thoughtAteCorpse);
                    }
                }
                FoodUtility.ThoughtFromIngesting thoughtRawFood = __result.Find(x => x.thought == ThoughtDefOf.AteRawFood);
                if (thoughtRawFood.thought != null)
                {
                    __result.Remove(thoughtRawFood);
                }
            }

            DietCategory dietCategory = GeneticDietUtility.GetDietCategory(ingester);
            if (dietCategory == DietCategory.Default) return;

            if (GeneticDietUtility.DietDislikes(foodSource, ingester))
            {
                FoodUtility.ThoughtFromIngesting thoughtFromIngesting = new FoodUtility.ThoughtFromIngesting
                {
                    thought = GeneticDietUtility.GetDietThought(ingester, foodSource),
                    fromPrecept = null
                };
                __result.Add(thoughtFromIngesting);
                __result.RemoveAll(x => x.thought == ThoughtDefOf.AteFineMeal || x.thought == ThoughtDefOf.AteLavishMeal);
            }
            if (foodSource is Corpse && dietCategory == DietCategory.Hypercarnivore)
            {
                FoodUtility.ThoughtFromIngesting thoughtAteCorpse = __result.Find(x => x.thought == ThoughtDefOf.AteCorpse);
                if (thoughtAteCorpse.thought != null)
                {
                    __result.Remove(thoughtAteCorpse);
                    __result.Add(new FoodUtility.ThoughtFromIngesting { thought = CG_DefOf.AteCorpseHypercarnivore, fromPrecept = null });
                }
            }
            
        }
    }

    [HarmonyPatch(typeof(Thing), "Ingested")]
    class Indigestion_Patch
    {
        static void Postfix(Pawn ingester, Thing __instance)
        {
            if (!ingester.RaceProps.Humanlike || ingester.genes == null) return;

            GeneticDietUtility.AddIndigestionHediff(ingester, __instance);
        }
    }

    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.WillEat_NewTemp), new Type[] { typeof(Pawn), typeof(Thing), typeof(Pawn), typeof(bool), typeof(bool) })]
    class WillEat_Patch
    {
        static void Postfix(ref bool __result, Pawn p, Thing food)
        {
            if (!p.RaceProps.Humanlike || p.genes == null) return;

            if (!__result) return;

            else __result = !GeneticDietUtility.DietForbids(food, p);
        }
    }

    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.FoodOptimality))]
    class FoodOptimality_Patch
    {
        static void Postfix(ref float __result, Pawn eater, Thing foodSource)
        {
            if (!eater.RaceProps.Humanlike || eater.genes == null) return;

            __result += GeneticDietUtility.OptimalityOffsetFromGeneticDiet(eater, foodSource);
        }
    }

    [HarmonyPatch(typeof(FoodUtility), "TryFindBestFoodSourceFor_NewTemp")]
    class TryFindBestFoodSourceFor_Patch
    {
        static void Prefix(ref bool allowCorpse, bool desperate, Pawn eater)
        {
            if (allowCorpse || !eater.RaceProps.Humanlike || eater.genes == null) return;

            if (GeneticDietUtility.GetDietCategory(eater) == DietCategory.Hypercarnivore || GeneticDietUtility.IsGeneticCannibal(eater))
            {
                allowCorpse = desperate;
            }
        }
    }

    [HarmonyPatch(typeof(FloatMenuMakerMap),"AddHumanlikeOrders")]
    class FloatMenu_Patch
    {
        static void Postfix(ref List<FloatMenuOption> opts, Pawn pawn, Vector3 clickPos)
        {
            IntVec3 c = IntVec3.FromVector3(clickPos);
            foreach (Thing t in c.GetThingList(pawn.Map))
            {
                //not interested in non-foods or nonhuman pawns
                if (t.def.ingestible == null || !pawn.RaceProps.CanEverEat(t) || !t.IngestibleNow || !pawn.RaceProps.Humanlike || pawn.genes == null) continue;

                //not interested in anything this mod doesn't interfere with
                if (!GeneticDietUtility.DietForbids(t, pawn)) continue;

                //copied directly from source
                //this will allow us to identify the menu options related to consuming this object
                string text;
                if (t.def.ingestible.ingestCommandString.NullOrEmpty())
                {
                    text = "ConsumeThing".Translate(t.LabelShort, t);
                }
                else
                {
                    text = t.def.ingestible.ingestCommandString.Formatted(t.LabelShort);
                }

                //for hay we should replicate the vanilla behaviour of not showing a Consume option at all
                if (t.def == ThingDefOf.Hay)
                {
                    opts.RemoveAll(x => x.Label.Contains(text));
                }

                //for other foods, we should disable the float menu option and tell the player why
                else
                {
                    foreach (FloatMenuOption consume in opts.FindAll(x => x.Label.Contains(text)))
                    {
                        //if it's already disabled, leave it alone
                        if (consume.Disabled) continue;
                        if (GeneticDietUtility.DietForbids(t, pawn))
                        {
                            consume.Label = text += " : Inedible";
                            consume.Disabled = true;
                        }
                    }
                }

            }
        }
    }

    
    [HarmonyPatch(typeof(FoodUtility),nameof(FoodUtility.GetFoodKindForStacking))]
    class GetFoodKindForStacking_Patch
    {
        static bool Prefix(ref FoodKind __result, Thing food)
        {
            if (!CG_Settings.changeMealStacking) return true;

            //pass null object issues back to vanilla/whatever other patches to be handled there
            if (food == null) return true;

            //Log.Message("Applying GetFoodKindForStacking_Patch");

            CG_FoodKind cg_foodKind = GeneticDietUtility.GetCG_FoodKind(food);
            int i = (int)cg_foodKind;
            __result = (FoodKind)i;
            return false;
        }
    }

    [HarmonyPatch(typeof(CompIngredients),"GetFoodKindInspectString")]
    class GetFoodKindInspectString_Patch
    {
        static bool Prefix(ref string __result, CompIngredients __instance)
        {
            //pass any null object problems back to vanilla to be handled there
            if (__instance == null | __instance.parent == null) return true;

            CG_FoodKind cg_foodKind = GeneticDietUtility.GetCG_FoodKind(__instance.parent);
            switch (cg_foodKind)
            {
                case (CG_FoodKind.AnimalProduct):
                    __result = "Vegetarian animal product".Colorize(ColorLibrary.Peach);
                    break;
                case (CG_FoodKind.AnimalProductAndVeg):
                    __result = "Vegetarian animal product".Colorize(ColorLibrary.Peach) + " + " + "veg".Colorize(Color.green);
                    break;
                case (CG_FoodKind.Any):
                    __result = "MealKindAny".Translate().Colorize(Color.white);
                    break;
                case (CG_FoodKind.Meat):
                    __result = "Meat".Colorize(ColorLibrary.RedReadable);
                    break;
                case (CG_FoodKind.MeatAndVeg):
                    __result = "Meat".Colorize(ColorLibrary.RedReadable) + " + " + "veg".Colorize(Color.green);
                    break;
                case (CG_FoodKind.Vegan):
                    __result = "Vegan".Colorize(Color.green);
                    break;
                default:
                    //if we fall through, default back to vanilla behaviour
                    return true;
            }
            return false;
        }
    }
    
    [HarmonyPatch(typeof(Graphic_MealVariants),nameof(Graphic_MealVariants.SubGraphicTypeIndex))]
    class MealVariants_SubGraphicTypeIndex_Patch
    {
        static bool Prefix(ref int __result, Thing thing)
        {
            //pass any null object problems back to vanilla
            if (thing == null) return true;

            //only fuck with vanilla meals
            List<string> mealDefNames = new List<string>
            {
                "MealSurvivalPack",
                "MealSimple",
                "MealFine",
                "MealFine_Veg",
                "MealLavish",
                "MealLavish_Veg"
            };
            if (!mealDefNames.Contains(thing.def.defName)) return true;

            if (CG_Settings.changeMealStacking)
            {

                CG_FoodKind cg_foodKind = GeneticDietUtility.GetCG_FoodKind(thing);
                __result = (int)cg_foodKind * 3;
            }
            else
            {
                FoodKind vanilla_foodKind = FoodUtility.GetFoodKind(thing);
                switch (vanilla_foodKind)
                {
                    case (FoodKind.Any): 
                        __result = 0;
                        break;
                    case (FoodKind.Meat): 
                        __result = 1;
                        break;
                    case (FoodKind.NonMeat): 
                        __result = 2;
                        break;
                    default: 
                        __result = 0;
                        break;
                }
                __result *= 3;
            }

            return false;
        }
    }
    
    [HarmonyPatch(typeof(Graphic_MealVariants),nameof(Graphic_MealVariants.SubGraphicIndexOffset))]
    class MealVariants_SubGraphicIndexOffset_Patch
    {
        static bool Prefix(ref int __result, Thing thing)
        {
            if (thing == null) return true;

            //only fuck with vanilla meals
            List<string> mealDefNames = new List<string>
            {
                "MealSurvivalPack",
                "MealSimple",
                "MealFine",
                "MealFine_Veg",
                "MealLavish",
                "MealLavish_Veg"
            };
            if (!mealDefNames.Contains(thing.def.defName)) return true;

            if (thing.stackCount == 1) __result = 0;
            else if (thing.stackCount == thing.def.stackLimit) __result = 2;
            else __result = 1;

            return false;
        }
    }

    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.AddFoodPoisoningHediff))]
    class AddFoodPoisoningHediff_Patch
    {
        static bool Prefix(Pawn pawn, Thing ingestible, FoodPoisonCause cause)
        {
            if (GeneticDietUtility.IsGeneticCannibal(pawn) 
                && cause == FoodPoisonCause.DangerousFoodType
                && FoodUtility.IsHumanlikeCorpseOrHumanlikeMeatOrIngredient(ingestible))
            {
                return false;
            }
            return true;
        }
    }
}
