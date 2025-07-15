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
    [Flags]
    public enum CG_FoodTypeFlags
    {
        CarnivoreOnly = 0b101010,
        HerbivoreOnly = 0b1000011010001
    }

    [Flags]
    public enum CG_FoodKind
    {
        Any = 0,
        Meat = 1,
        Vegetable = 2,
        MeatAndVeg = 3,
        AnimalProduct = 4,
        AnimalProductAndVeg = 6
    }

    public enum DietCategory
    {
        Default,
        Carnivore,
        Hypercarnivore,
        Herbivore,
        StrictHerbivore
    }

    public static class GeneticDietUtility
    {
        public const float INDIGESTION_FACTOR = 1f;

        public static bool HasFlags_HerbivoreOnly(ThingDef foodDef)
        {
            if (!foodDef.IsIngestible) return false;

            return ((int)foodDef.ingestible.foodType & (int)CG_FoodTypeFlags.HerbivoreOnly) != 0;
        }

        public static bool HasFlags_CarnivoreOnly(ThingDef foodDef)
        {
            if (!foodDef.IsIngestible) return false;

            return ((int)foodDef.ingestible.foodType & (int)CG_FoodTypeFlags.CarnivoreOnly) != 0;
        }
        
        public static bool HasIngredients(ThingDef def)
        {
            return def.HasComp(typeof(CompIngredients));
        }

        public static bool HasIngredients(Thing thing, out List<ThingDef> ingredients)
        {
            CompIngredients compIngredients = thing.TryGetComp<CompIngredients>();
            if (compIngredients == null)
            {
                ingredients = null;
                return false;
            }
            else
            {
                ingredients = compIngredients.ingredients;
                return true;
            }
        }

        //returns a named category
        //eg meat+animal product returns meat
        //and all three returns meat+veg
        public static CG_FoodKind CG_FoodKindForDisplay(CG_FoodKind fk)
        {
            if (fk.HasFlag(CG_FoodKind.Meat))
            {
                if (fk.HasFlag(CG_FoodKind.Vegetable)) return CG_FoodKind.MeatAndVeg;
                else return CG_FoodKind.Meat;
            }
            if (fk.HasFlag(CG_FoodKind.AnimalProduct))
            {
                if (fk.HasFlag(CG_FoodKind.Vegetable)) return CG_FoodKind.AnimalProductAndVeg;
                else return CG_FoodKind.AnimalProduct;
            }
            if (fk.HasFlag(CG_FoodKind.Vegetable)) return CG_FoodKind.Vegetable;
            return CG_FoodKind.Any;
        }

        public static CG_FoodKind GetCG_FoodKind(ThingDef def)
        {
            if (def == null || !def.IsIngestible || def.IsDrug) return CG_FoodKind.Any;

            if (def == ThingDefOf.HemogenPack) return CG_FoodKind.Meat;

            CG_FoodKind cg_FoodKind = CG_FoodKind.Any;

            if (HasFlags_HerbivoreOnly(def))
            {
                cg_FoodKind |= CG_FoodKind.Vegetable;
            }
            if (HasFlags_CarnivoreOnly(def))
            {
                //distinguish between meats and animal products for vegetarians
                if (def.ingestible.foodType.HasFlag(FoodTypeFlags.Meat) || def.ingestible.foodType.HasFlag(FoodTypeFlags.Corpse))
                {
                    cg_FoodKind |= CG_FoodKind.Meat;
                }
                else cg_FoodKind |= CG_FoodKind.AnimalProduct;
            }

            if (HasIngredients(def))
            {
                FoodKind noIngredientsFoodKind = def.GetCompProperties<CompProperties_Ingredients>()?.noIngredientsFoodKind ?? FoodKind.Any;
                if (noIngredientsFoodKind == FoodKind.Meat) cg_FoodKind |= CG_FoodKind.Meat;
                else if (noIngredientsFoodKind == FoodKind.NonMeat) cg_FoodKind |= CG_FoodKind.Vegetable;
            }

            return cg_FoodKind;
        }

        public static CG_FoodKind GetCG_FoodKind(Thing food)
        {
            if (food == null) return CG_FoodKind.Any;

            CG_FoodKind cg_FoodKind = GetCG_FoodKind(food.def);

            List<ThingDef> ingredients;
            if (HasIngredients(food, out ingredients) && !ingredients.NullOrEmpty())
            {
                foreach (ThingDef ingredient in ingredients)
                {
                    //if it's null or not a food, ignore it
                    if (ingredient == null || !ingredient.IsIngestible) continue;

                    //if it's a drug, ignore it
                    if (ingredient.IsDrug) continue;

                    cg_FoodKind |= GetCG_FoodKind(ingredient);
                }
            }

            return cg_FoodKind;
        }

        public static DietCategory GetDietCategory(Pawn pawn)
        {
            if (pawn.genes == null) return DietCategory.Default;
            if (pawn.HasActiveGene(CG_DefOf.CYB_Carnivore)) return DietCategory.Carnivore;
            if (pawn.HasActiveGene(CG_DefOf.CYB_Hypercarnivore)) return DietCategory.Hypercarnivore;
            if (pawn.HasActiveGene(CG_DefOf.CYB_Herbivore)) return DietCategory.Herbivore;
            if (pawn.HasActiveGene(CG_DefOf.CYB_StrictHerbivore)) return DietCategory.StrictHerbivore;
            return DietCategory.Default;
        }

        public static bool NeverForbidden(ThingDef foodDef, Pawn pawn)
        {
            if (foodDef.IsDrug) return true;

            //hemogenic pawns can always eat hemogen packs
            if (foodDef == ThingDefOf.HemogenPack && pawn.HasActiveGene(GeneDefOf.Hemogenic)) return true;

            return false;
        }

        //whether their diet renders a food absolutely inedible
        public static bool DietForbids(ThingDef foodDef, Pawn pawn)
        {
            //LogUtil.DebugLog($"DietForbids(def) - foodDef: {foodDef}, pawn: {pawn}");

            //covers drugs, hemogen packs for hemogenic pawns
            if (NeverForbidden(foodDef, pawn)) return false;

            //obligate cannibals can't eat anything but:
            // - hemogen packs
            // - corpses (can't check if human at this stage)
            // - things with ingredients (could have human meat in)
            // - human meat
            if (pawn.HasActiveGene(CG_DefOf.CYB_ObligateCannibal))
            {
                if (foodDef != ThingDefOf.HemogenPack
                    && !foodDef.IsCorpse
                    && !foodDef.HasComp(typeof(CompIngredients))
                    && FoodUtility.GetMeatSourceCategory(foodDef) != MeatSourceCategory.Humanlike)
                {
                    return true;
                }
            }

            CG_FoodKind cg_FoodKind = GetCG_FoodKind(foodDef);
            DietCategory dietCategory = GetDietCategory(pawn);

            //LogUtil.DebugLog($"DietForbids(def) - foodDef: {foodDef}, pawn: {pawn}, dietCategory: {dietCategory}" +
            //    $", cg_FoodKind: {cg_FoodKind}");


            //no one who isn't a strict herbivore is allowed to eat hay
            if (dietCategory != DietCategory.StrictHerbivore && foodDef == ThingDefOf.Hay) return true;

            //otherwise don't mess with non-genetic-diet pawns
            if (dietCategory == DietCategory.Default) return false;

            if (dietCategory == DietCategory.Hypercarnivore && cg_FoodKind == CG_FoodKind.Vegetable)
            {
                return true;
            }
            else if (dietCategory == DietCategory.StrictHerbivore &&
                (cg_FoodKind == CG_FoodKind.Meat || cg_FoodKind == CG_FoodKind.AnimalProduct))
            {
                return true;
            }

            else
            {
                //LogUtil.DebugLog($"Diet does not forbid whole def - foodDef: {foodDef}, pawn: {pawn}");
                return false;
            }
        }

        public static bool DietForbids(Thing food, Pawn pawn)
        {
            //LogUtil.DebugLog($"DietForbids(thing) - food: {food}, pawn: {pawn}, def: {food.def}");

            //covers drugs, hemogen packs for hemogenic pawns
            if (NeverForbidden(food.def, pawn)) return false;

            //LogUtil.DebugLog($"Def is not NeverForbidden");

            //if the def is always forbidden
            if (DietForbids(food.def, pawn)) return true;

            //LogUtil.DebugLog($"Def is not forbidden");

            //obligate cannibals
            if (pawn.HasActiveGene(CG_DefOf.CYB_ObligateCannibal))
            {
                if (FoodUtility.IsHumanlikeCorpseOrHumanlikeMeatOrIngredient(food) || food.def == ThingDefOf.HemogenPack)
                    return false;
                else return true;
            }

            DietCategory dietCategory = GetDietCategory(pawn);
            CG_FoodKind cg_FoodKind = GetCG_FoodKind(food);

            //LogUtil.DebugLog($"DietForbids(thing) - food: {food}, pawn: {pawn}, dietCategory: {dietCategory}" +
            //    $", cg_FoodKind: {cg_FoodKind}"
            //    );

            if (cg_FoodKind == CG_FoodKind.Any) return false;

            if (dietCategory == DietCategory.Hypercarnivore && cg_FoodKind == CG_FoodKind.Vegetable)
            {
                return true;
            }
            else if (dietCategory == DietCategory.StrictHerbivore && 
                !cg_FoodKind.HasFlag(CG_FoodKind.Vegetable))
            {
                return true;
            }
            else return false;
        }

        //whether their diet means they will suffer ill effects from eating a thing
        public static bool DietDislikes(Thing food, Pawn pawn)
        {
            if (NeverForbidden(food.def, pawn)) return false;

            DietCategory dietCategory = GetDietCategory(pawn);
            if (dietCategory == DietCategory.Default) return false;

            CG_FoodKind cg_foodKind = GetCG_FoodKind(food);
            if (cg_foodKind == CG_FoodKind.Any) return false;

            if (dietCategory == DietCategory.Carnivore || dietCategory == DietCategory.Hypercarnivore)
            {
                if (cg_foodKind.HasFlag(CG_FoodKind.Vegetable)) return true;
                else return false;
            }
            if (dietCategory == DietCategory.Herbivore || dietCategory == DietCategory.StrictHerbivore)
            {
                if (cg_foodKind == CG_FoodKind.Vegetable) return false;
                else return true;
            }
            //fall-through : shouldn't happen
            return false;
        }

        public static float NutritionFactorFromGeneticDiet(Thing food, Pawn pawn)
        {
            if (food.def.IsDrug || food.def.ingestible ==  null) return 1f;

            if (food.def == ThingDefOf.HemogenPack && pawn.HasActiveGene(GeneDefOf.Hemogenic))
            {
                return 1f;
            }

            if (pawn.genes != null && pawn.HasActiveGene(CG_DefOf.CYB_ObligateCannibal))
            {
                return ProportionHumanlike(food);
            }

            DietCategory dietCategory = GetDietCategory(pawn);
            //don't fuck with normal pawns
            if (dietCategory == DietCategory.Default) return 1f;

            if (food.def == ThingDefOf.HemogenPack 
                && (dietCategory == DietCategory.Herbivore || dietCategory == DietCategory.StrictHerbivore))
            {
                return pawn.GetStatValue(CG_DefOf.CYB_AnimalNutritionFactor);
            }

            CG_FoodKind cg_foodKind = GetCG_FoodKind(food);
            if (cg_foodKind == CG_FoodKind.Any) return 1f;

            if (dietCategory == DietCategory.Carnivore || dietCategory == DietCategory.Hypercarnivore)
            {
                if (!cg_foodKind.HasFlag(CG_FoodKind.Vegetable)) return 1f;
                else
                {
                    float nutritionFactor = pawn.GetStatValue(CG_DefOf.CYB_VegetableNutritionFactor);
                    //if there are ingredients other than vegetables, halve the effect
                    if (cg_foodKind != CG_FoodKind.Vegetable) nutritionFactor += (1 - nutritionFactor) / 2;
                    return nutritionFactor;
                }
            }
            if (dietCategory == DietCategory.Herbivore || dietCategory == DietCategory.StrictHerbivore)
            {
                if (cg_foodKind == CG_FoodKind.Vegetable) return 1f;
                else
                {
                    float nutritionFactor = pawn.GetStatValue(CG_DefOf.CYB_AnimalNutritionFactor);
                    //if there are some vegetables, halve the effect
                    if (cg_foodKind.HasFlag(CG_FoodKind.Vegetable))
                    {
                        nutritionFactor += (1 - nutritionFactor) / 2;
                    }
                    return nutritionFactor;
                }
            }
            //fall-through : shouldn't happen
            return 1f;
        }

        public static float OptimalityOffsetFromGeneticDiet(Pawn pawn, Thing food)
        {
            DietCategory dietCategory = GetDietCategory(pawn);
            //don't mess with pawns without a genetic diet
            if (dietCategory == DietCategory.Default) return 0f;

            //don't mess with generic food
            CG_FoodKind cg_foodKind = GetCG_FoodKind(food);
            if (cg_foodKind == CG_FoodKind.Any) return 0f;

            switch (dietCategory)
            {
                case (DietCategory.Carnivore):
                    if (cg_foodKind == CG_FoodKind.Vegetable) return -150f;
                    else if (cg_foodKind.HasFlag(CG_FoodKind.Vegetable)) return -75f;
                    else return 0f;
                case (DietCategory.Hypercarnivore):
                    if (cg_foodKind == CG_FoodKind.Vegetable) return -9999999f;
                    else if (cg_foodKind.HasFlag(CG_FoodKind.Vegetable)) return -150f;
                    else return 0f;
                case (DietCategory.Herbivore):
                    if (cg_foodKind == CG_FoodKind.Vegetable) return 0f;
                    else if (cg_foodKind.HasFlag(CG_FoodKind.Vegetable)) return -75f;
                    else return -150f;
                case (DietCategory.StrictHerbivore):
                    if (cg_foodKind == CG_FoodKind.Vegetable) return 0f;
                    else if (cg_foodKind.HasFlag(CG_FoodKind.Vegetable)) return -150f;
                    else return -9999999f;
                default: return 0f;
            }  
        }

        public static ThoughtDef GetDietThought(Pawn pawn, Thing food)
        {
            DietCategory dietCategory = GetDietCategory(pawn);
            //don't fuck with pawns without a genetic diet
            if (dietCategory == DietCategory.Default) return null;

            CG_FoodKind cg_foodKind = GetCG_FoodKind(food);
            if (cg_foodKind == CG_FoodKind.Any && food.def != ThingDefOf.HemogenPack) return null;

            if (dietCategory == DietCategory.Carnivore || dietCategory == DietCategory.Hypercarnivore)
            {
                if (!cg_foodKind.HasFlag(CG_FoodKind.Vegetable)) return null;
                else return CG_DefOf.CYB_AtePlantCarnivore;
            }
            if (dietCategory == DietCategory.Herbivore || dietCategory == DietCategory.StrictHerbivore)
            {
                if (cg_foodKind == CG_FoodKind.Vegetable) return null;
                else if (cg_foodKind.HasFlag(CG_FoodKind.Meat) ||
                    (food.def == ThingDefOf.HemogenPack && !pawn.HasActiveGene(GeneDefOf.Hemogenic)))
                {
                    return CG_DefOf.CYB_AteMeatHerbivore;
                }
                else return CG_DefOf.CYB_AteAnimalProductHerbivore;
            }
            //fall-through : shouldn't happen
            return null;
        }

        public static void AddIndigestionHediff(Pawn pawn, Thing food)
        {
            float nutritionFactor = NutritionFactorFromGeneticDiet(food, pawn);
            if (nutritionFactor == 1f) return;

            float indigestionOffset = INDIGESTION_FACTOR * (1 - nutritionFactor) * food.def.GetStatValueAbstract(StatDefOf.Nutrition, null);

            Hediff indigestionHediff = pawn.health.hediffSet.GetFirstHediffOfDef(CG_DefOf.CYB_DietaryIndigestion, false);
            if (indigestionHediff == null)
            {
                pawn.health.AddHediff(HediffMaker.MakeHediff(CG_DefOf.CYB_DietaryIndigestion, pawn, null), null, null, null);
            }

            indigestionHediff = pawn.health.hediffSet.GetFirstHediffOfDef(CG_DefOf.CYB_DietaryIndigestion, false);

            indigestionHediff.Severity += indigestionOffset;

            if (indigestionHediff.Severity > 1) indigestionHediff.Severity = 1;
        }

        //this is a bit of a hack and does not take into account what proportion of each ingredient was atually used
        //but it's an approximation
        public static float ProportionHumanlike(Thing food)
        {
            if (food.def == ThingDefOf.HemogenPack) return 1f;
            CompIngredients compIngredients = food.TryGetComp<CompIngredients>();
            if (compIngredients == null)
            {
                return FoodUtility.IsHumanlikeCorpseOrHumanlikeMeat(food, food.def)
                    ? 1f : 0f;
            }
            float countHumanlike = 0f;
            float countNonHumanlike = 0f;
            foreach (ThingDef ingredient in compIngredients.ingredients)
            {
                if (food.def.ingestible == null) continue;
                if (FoodUtility.IsHumanlikeCorpseOrHumanlikeMeat(food, ingredient))
                {
                    countHumanlike += 1f;
                }
                else countNonHumanlike += 1f;
            }
            if (countHumanlike + countNonHumanlike != 0f)
            {
                return countHumanlike / (countHumanlike + countNonHumanlike);
            }
            return 0f;
        }
    }

}
