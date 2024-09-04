using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using static CyanobotsGenes.GeneticDietUtility;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(FoodUtility), "NutritionForEater")]
    class Nutrition_Patch
    {
        static void Postfix(ref float __result, Pawn eater, Thing food)
        {
            //uninterested in nonhumans
            if (eater == null || !eater.RaceProps.Humanlike) return;

            float nutritionFactor = NutritionFactorFromGeneticDiet(food, eater);
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

    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.AddFoodPoisoningHediff))]
    class FoodPoisoning_Patch
    {
        static bool Prefix(Pawn pawn, Thing ingestible, FoodPoisonCause cause)
        {
            //Log.Message("Firing FoodPoisoning_Patch.Prefix, pawn: " + pawn + ", ingestible: " + ingestible + ", cause: " + cause);
            if (cause == FoodPoisonCause.DangerousFoodType)
            {
                if (BodyfeederUtility.IsBodyFeeder(pawn) && FoodUtility.IsHumanlikeCorpseOrHumanlikeMeat(ingestible, ingestible.def))
                {
                    //Log.Message("Bodyfeeder eating humanlike");
                    return false;
                }
                DietCategory dietCategory = GetDietCategory(pawn);
                if (dietCategory == DietCategory.Hypercarnivore && ingestible.def.IsMeat)
                {
                    //Log.Message("Hypercarnivore eating meat");
                    return false;
                }
                if (dietCategory == DietCategory.StrictHerbivore && !HasFlags_CarnivoreOnly(ingestible.def))
                {
                    //Log.Message("Strict herbivore eating plants");
                    return false;
                }
            }
            //Log.Message("Returning to vanilla...");
            return true;
        }
    }

    [HarmonyPatch(typeof(FoodUtility), "ThoughtsFromIngesting")]
    class FoodThoughts_Patch
    {
        static void Postfix(ref List<FoodUtility.ThoughtFromIngesting> __result, Pawn ingester, Thing foodSource)
        {
            LogUtil.DebugLog("ThoughtsFromIngesting Postfix, initial __result: " + __result.ToStringSafeEnumerable()
                + ", [0]: " + (__result.Count > 0 ? __result[0].thought.ToString() : ""));

            if (!ingester.RaceProps.Humanlike || ingester.genes == null) return;

            DietCategory dietCategory = GeneticDietUtility.GetDietCategory(ingester);
            if (dietCategory == DietCategory.Default) return;

            if (GeneticDietUtility.DietDislikes(foodSource, ingester))
            {
                LogUtil.DebugLog("Disliked food");
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
                LogUtil.DebugLog("Hypercarnivore and corpse");
                FoodUtility.ThoughtFromIngesting thoughtAteCorpse = __result.Find(x => x.thought == ThoughtDefOf.AteCorpse);
                if (thoughtAteCorpse.thought != null)
                {
                    __result.Remove(thoughtAteCorpse);
                    __result.Add(new FoodUtility.ThoughtFromIngesting { thought = CG_DefOf.AteCorpseHypercarnivore, fromPrecept = null });
                }
            }

            LogUtil.DebugLog("final __result: " + __result.ToStringSafeEnumerable()
                + ", [0]: " + (__result.Count > 0 ? __result[0].thought.ToString() : ""));
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

    [HarmonyPatch(typeof(RaceProperties), nameof(RaceProperties.CanEverEat), new Type[] { typeof(ThingDef) })]
    class CanEverEat_Patch
    {
        static bool Postfix(bool __result, RaceProperties __instance, ThingDef t)
        {
            if (__result || !__instance.Humanlike) return __result;
            if (t == ThingDefOf.Hay) return true;
            return false;
        }
    }

    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.WillEat), new Type[] { typeof(Pawn), typeof(ThingDef), typeof(Pawn), typeof(bool), typeof(bool) })]
    class WillEat_Patch
    {
        static bool Postfix(bool __result, Pawn p, ThingDef food)
        {
            if (!p.RaceProps.Humanlike || p.genes == null) return __result;
            if (!__result) return false;
            else return !GeneticDietUtility.DietForbids(food, p);
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

    [HarmonyPatch(typeof(FoodUtility), "TryFindBestFoodSourceFor")]
    class TryFindBestFoodSourceFor_Patch
    {
        static void Prefix(ref bool allowCorpse, bool desperate, Pawn eater)
        {
            if (allowCorpse || !eater.RaceProps.Humanlike || eater.genes == null) return;

            if (GeneticDietUtility.GetDietCategory(eater) == DietCategory.Hypercarnivore /*|| GeneticDietUtility.IsGeneticCannibal(eater)*/)
            {
                allowCorpse = desperate;
            }
        }
    }

    [HarmonyPatch(typeof(FloatMenuMakerMap),"AddHumanlikeOrders")]
    class FloatMenu_Patch_Diet
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
                /*
                if (t.def == ThingDefOf.Hay)
                {
                    opts.RemoveAll(x => x.Label.Contains(text));
                }

                //for other foods, we should disable the float menu option and tell the player why
                else
                {
                */
                    foreach (FloatMenuOption consume in opts.FindAll(x => x.Label.Contains(text)))
                    {
                        //if it's already disabled, leave it alone
                        if (consume.Disabled) continue;
                        if (GeneticDietUtility.DietForbids(t, pawn))
                        {
                            consume.Label = text += " : " + "CYB_Inedible".Translate();
                            consume.Disabled = true;
                        }
                    }
                //}

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

            CG_FoodKind cg_foodKind = CG_FoodKindForDisplay(GetCG_FoodKind(food));
            int i = (int)cg_foodKind;
            __result = (FoodKind)i;
            //Log.Message("GetFoodKindForStacking: " + __result);
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

            CG_FoodKind displayKind = CG_FoodKindForDisplay(GetCG_FoodKind(__instance.parent));

            switch (displayKind)
            {
                case (CG_FoodKind.AnimalProduct):
                    __result = "CYB_AnimalProduct".Translate().Colorize(ColorLibrary.Peach);
                    break;
                case (CG_FoodKind.AnimalProductAndVeg):
                    __result = "CYB_AnimalProduct".Translate().Colorize(ColorLibrary.Peach) + " + " + "CYB_Veg".Translate().Colorize(Color.green);
                    break;
                case (CG_FoodKind.Any):
                    __result = "MealKindAny".Translate().Colorize(Color.white);
                    break;
                case (CG_FoodKind.Meat):
                    __result = "CYB_Meat".Translate().Colorize(ColorLibrary.RedReadable);
                    break;
                case (CG_FoodKind.MeatAndVeg):
                    __result = "CYB_Meat".Translate().Colorize(ColorLibrary.RedReadable) + " + " + "CYB_Veg".Translate().Colorize(Color.green);
                    break;
                case (CG_FoodKind.Vegetable):
                    __result = "CYB_Vegan".Translate().Colorize(Color.green);
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

                CG_FoodKind foodKind = GetCG_FoodKind(thing);
                __result = (int)foodKind;
                /*
                CG_FoodKind displayKind = CG_FoodKindForDisplay(GetCG_FoodKind(thing));
                switch (displayKind)
                {
                    case (CG_FoodKind.Meat):
                        __result = 0;
                        break;
                    case (CG_FoodKind.Vegetable):
                        __result = 1;
                        break;
                    case (CG_FoodKind.Any):
                        __result = 2;
                        break;
                    case (CG_FoodKind.AnimalProduct):
                        __result = 3;
                        break;
                    case (CG_FoodKind.AnimalProductAndVeg):
                        __result = 4;
                        break;
                    case (CG_FoodKind.MeatAndVeg):
                        __result = 5;
                        break;
                    default:
                        __result = 2;
                        break;
                }
                */
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
                //Log.Message("vanilla_foodKind: " + vanilla_foodKind + ", __result: " + __result);
            }
            __result *= 3;
            //Log.Message("SubGraphicIndex: " + __result);
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

            //Log.Message("SubGraphicIndexOffset: " + __result);
            return false;
        }
    }

}
