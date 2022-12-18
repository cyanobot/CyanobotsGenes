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

    public enum CG_FoodKind
    {
        //fully edible to carnivores, therefore also appropriate for ranchers
        //may or may not include animal products
        //int match to FoodKind.Meat, which also includes meat+veg
        Meat = (int)FoodKind.Meat,

        //fully edible to herbivores, therefore also appropriate for vegetarians
        //int match to FoodKind.NonMeat, which also contains veg+animal products
        Vegan = (int)FoodKind.NonMeat,

        //generic/default foods, edible to anyone
        //int match to FoodKind.Any, which also contains animal products
        Any = (int)FoodKind.Any,

        //inedible to herbivores, fine for vegetarians
        AnimalProduct,

        //mixed veg and animal product, suitable for vegetarians but not herbivores
        AnimalProductAndVeg,

        //mixed veg and meat, suitable for ranchers but not carnivores
        //may or may not include animal products
        MeatAndVeg
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
            if (foodDef.ingestible.foodType == null) return false;

            return ((int)foodDef.ingestible.foodType & (int)CG_FoodTypeFlags.HerbivoreOnly) != 0;
        }

        public static bool HasFlags_CarnivoreOnly(ThingDef foodDef)
        {
            if (!foodDef.IsIngestible) return false;
            if (foodDef.ingestible.foodType == null) return false;

            return ((int)foodDef.ingestible.foodType & (int)CG_FoodTypeFlags.CarnivoreOnly) != 0;
        }

        /*
        public static bool EdibleCarnivore(ThingDef foodDef)
        {
            return ((int)foodDef.ingestible.foodType & (int)CG_FoodTypeFlags.CarnivoreExclusion) == 0;
        }

        public static bool EdibleHerbivore(ThingDef foodDef)
        {
            return ((int)foodDef.ingestible.foodType & (int)CG_FoodTypeFlags.HerbivoreExclusion) == 0;
        }
        */

        public static CG_FoodKind GetCG_FoodKind(Thing food)
        {
            if (food == null) return CG_FoodKind.Any;

            CompIngredients compIngredients = food.TryGetComp<CompIngredients>();
            if (compIngredients == null) return GetCG_FoodKind(food.def);
            if (compIngredients.ingredients.NullOrEmpty<ThingDef>()) 
            {
                //assumes that things assigned Meat are all meat, things assigned NonMeat are all veg, and things assigned Any are default/null
                int noIngredientsFoodKind = (int) compIngredients.Props.noIngredientsFoodKind;
                if (noIngredientsFoodKind == null) return CG_FoodKind.Any;
                return (CG_FoodKind) noIngredientsFoodKind;
            }

            bool flag_Meat = false;
            bool flag_AnyCarnivoreOnly = false;
            bool flag_AnyHerbivoreOnly = false;

            foreach (ThingDef ingredient in compIngredients.ingredients)
            {
                //if it's null or not a food, ignore it
                if (ingredient == null || !ingredient.IsIngestible) continue;

                //if it's a drug, ignore it
                if (ingredient.IsDrug) continue;

                switch (GetCG_FoodKind(ingredient))
                {
                    case (CG_FoodKind.AnimalProduct):
                        flag_AnyCarnivoreOnly = true;
                        break;
                    case (CG_FoodKind.AnimalProductAndVeg):
                        flag_AnyCarnivoreOnly = true;
                        flag_AnyHerbivoreOnly = true;
                        break;
                    case (CG_FoodKind.Any):
                        break;
                    case (CG_FoodKind.Meat):
                        flag_AnyCarnivoreOnly = true;
                        flag_Meat = true;
                        break;
                    case (CG_FoodKind.MeatAndVeg):
                        flag_AnyCarnivoreOnly = true;
                        flag_AnyHerbivoreOnly = true;
                        flag_Meat = true;
                        break;
                    case (CG_FoodKind.Vegan):
                        flag_AnyHerbivoreOnly = true;
                        break;
                    default:
                        break;
                }
            }

            if (flag_AnyCarnivoreOnly)
            {
                if (flag_AnyHerbivoreOnly)
                {
                    if (flag_Meat) return CG_FoodKind.MeatAndVeg;
                    else return CG_FoodKind.AnimalProductAndVeg;
                }
                else
                {
                    if (flag_Meat) return CG_FoodKind.Meat;
                    else return CG_FoodKind.AnimalProduct;
                }
            }
            else if (flag_AnyHerbivoreOnly) return CG_FoodKind.Vegan;
            else return CG_FoodKind.Any;
        }

        public static CG_FoodKind GetCG_FoodKind(ThingDef foodDef)
        {
            //if it's null, not a food, or is a drug, default to Any
            if (foodDef == null) return CG_FoodKind.Any;
            if (!foodDef.IsIngestible) return CG_FoodKind.Any;
            if (foodDef.IsDrug) return CG_FoodKind.Any;
            
            //if it's the sort of thing that would usually have ingredients
            //we are looking at the def so it can't _actually_ have ingredients
            //so attempt to read the noIngredientsFoodKind
            if (foodDef.HasComp(typeof(CompIngredients)))
            {
                CompProperties_Ingredients compProperties_Ingredients = foodDef.GetCompProperties<CompProperties_Ingredients>();
                if (compProperties_Ingredients != null)
                {
                    //assumes that things assigned Meat are all meat, things assigned NonMeat are all veg, and things assigned Any are default/null
                    int noIngredientsFoodKind = (int)compProperties_Ingredients.noIngredientsFoodKind;
                    return (CG_FoodKind)noIngredientsFoodKind;
                }
            }

            //otherwise, calculate the food kind the foodType flags

            //if has herbivore-only flags
            if (HasFlags_HerbivoreOnly(foodDef))
            {
                //and carnivore-only flags
                if (HasFlags_CarnivoreOnly(foodDef))
                {
                    //distinguish between meats and animal products for vegetarians
                    if (foodDef.ingestible != null && (foodDef.ingestible.foodType.HasFlag(FoodTypeFlags.Meat) || foodDef.ingestible.foodType.HasFlag(FoodTypeFlags.Corpse)))
                    {
                        return CG_FoodKind.MeatAndVeg;
                    }
                    else return CG_FoodKind.AnimalProductAndVeg;
                }
                //some herbivore-only but no carnivore-only flags
                else return CG_FoodKind.Vegan;
            }
            //no herbivore-only flags, some carnivore-only
            else if (HasFlags_CarnivoreOnly(foodDef))
            {
                //distinguish between meats and animal products for vegetarians
                if (foodDef.ingestible != null && (foodDef.ingestible.foodType.HasFlag(FoodTypeFlags.Meat) || foodDef.ingestible.foodType.HasFlag(FoodTypeFlags.Corpse)))
                {
                    return CG_FoodKind.Meat;
                }
                else return CG_FoodKind.AnimalProduct;
            }
            //no flags that exclude either category
            else return CG_FoodKind.Any;
        }

        public static DietCategory GetDietCategory(Pawn pawn)
        {
            if (pawn.genes != null && pawn.genes.HasGene(CG_DefOf.Carnivore)) return DietCategory.Carnivore;
            if (pawn.genes != null && pawn.genes.HasGene(CG_DefOf.Hypercarnivore)) return DietCategory.Hypercarnivore;
            if (pawn.genes != null && pawn.genes.HasGene(CG_DefOf.Herbivore)) return DietCategory.Herbivore;
            if (pawn.genes != null && pawn.genes.HasGene(CG_DefOf.StrictHerbivore)) return DietCategory.StrictHerbivore;
            return DietCategory.Default;
        }

        //whether their diet renders a food absolutely inedible
        public static bool DietForbids(Thing food, Pawn pawn)
        {
            DietCategory dietCategory = GetDietCategory(pawn);

            //hay has been xml patched to be a vegetable
            //if they are NOT a strict herbivore, they still shouldn't eat it
            if (dietCategory != DietCategory.StrictHerbivore && food.def == ThingDefOf.Hay) return true;

            //if it's something they would normally eat, check if their diet excludes it -- this exists for checking ingredients  of meals
            if (dietCategory == DietCategory.Hypercarnivore && GetCG_FoodKind(food) == CG_FoodKind.Vegan)
            {
                return true;
            }
            else if (dietCategory == DietCategory.StrictHerbivore && (GetCG_FoodKind(food) == CG_FoodKind.Meat || GetCG_FoodKind(food) == CG_FoodKind.AnimalProduct))
            {
                return true;
            }
            else return false;
        }

        //whether their diet means they will suffer ill effects from eating a thing
        public static bool DietDislikes(Thing food, Pawn pawn)
        {
            DietCategory dietCategory = GetDietCategory(pawn);
            //don't fuck with pawns without a genetic diet
            if (dietCategory == DietCategory.Default) return false;

            CG_FoodKind cg_foodKind = GetCG_FoodKind(food);
            if (cg_foodKind == CG_FoodKind.Any) return false;

            if (dietCategory == DietCategory.Carnivore || dietCategory == DietCategory.Hypercarnivore)
            {
                if (cg_foodKind == CG_FoodKind.Meat || cg_foodKind == CG_FoodKind.AnimalProduct) return false;
                else return true;
            }
            if (dietCategory == DietCategory.Herbivore || dietCategory == DietCategory.StrictHerbivore)
            {
                if (cg_foodKind == CG_FoodKind.Vegan) return false;
                else return true;
            }
            //fall-through : shouldn't happen
            return false;
        }

        public static float NutritionFactorFromGeneticDiet(Thing food, Pawn pawn)
        {
            DietCategory dietCategory = GetDietCategory(pawn);
            //don't fuck with pawns without a genetic diet
            if (dietCategory == DietCategory.Default) return 1f;

            CG_FoodKind cg_foodKind = GetCG_FoodKind(food);
            if (cg_foodKind == CG_FoodKind.Any) return 1f;

            if (dietCategory == DietCategory.Carnivore || dietCategory == DietCategory.Hypercarnivore)
            {
                if (cg_foodKind == CG_FoodKind.Meat || cg_foodKind == CG_FoodKind.AnimalProduct) return 1f;
                else
                {
                    float nutritionFactor = pawn.GetStatValue(CG_DefOf.VegetableNutritionFactor);
                    //if there are ingredients other than vegetables, halve the effect
                    if (cg_foodKind != CG_FoodKind.Vegan) nutritionFactor += (1 - nutritionFactor) / 2;
                    return nutritionFactor;
                }
            }
            if (dietCategory == DietCategory.Herbivore || dietCategory == DietCategory.StrictHerbivore)
            {
                if (cg_foodKind == CG_FoodKind.Vegan) return 1f;
                else
                {
                    float nutritionFactor = pawn.GetStatValue(CG_DefOf.AnimalNutritionFactor);
                    //if there are some vegetables, halve the effect
                    if (cg_foodKind == CG_FoodKind.AnimalProductAndVeg || cg_foodKind == CG_FoodKind.MeatAndVeg)
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

            CG_FoodKind cg_foodKind = GetCG_FoodKind(food);

            switch (dietCategory)
            {
                case (DietCategory.Carnivore):
                    switch (cg_foodKind) {
                        case CG_FoodKind.Vegan: return -150f;
                        case CG_FoodKind.MeatAndVeg: return -75f;
                        case CG_FoodKind.AnimalProductAndVeg: return -75f;
                        default: return 0f;
                    }
                case (DietCategory.Hypercarnivore):
                    switch (cg_foodKind)
                    {
                        case CG_FoodKind.Vegan: return -9999999f;
                        case CG_FoodKind.MeatAndVeg: return -150f;
                        case CG_FoodKind.AnimalProductAndVeg: return -150f;
                        default: return 0f;
                    }
                case (DietCategory.Herbivore):
                    switch (cg_foodKind)
                    {
                        case CG_FoodKind.Meat: return -150f;
                        case CG_FoodKind.AnimalProduct: return -150f;
                        case CG_FoodKind.MeatAndVeg: return -75f;
                        case CG_FoodKind.AnimalProductAndVeg: return -75f;
                        default: return 0f;
                    }
                case (DietCategory.StrictHerbivore):
                    switch (cg_foodKind)
                    {
                        case CG_FoodKind.Meat: return -9999999f;
                        case CG_FoodKind.AnimalProduct: return -9999999f;
                        case CG_FoodKind.MeatAndVeg: return -150f;
                        case CG_FoodKind.AnimalProductAndVeg: return -150f;
                        default: return 0f;
                    }
                default: return 0f;
            }  
        }

        public static ThoughtDef GetDietThought(Pawn pawn, Thing food)
        {
            DietCategory dietCategory = GetDietCategory(pawn);
            //don't fuck with pawns without a genetic diet
            if (dietCategory == DietCategory.Default) return null;

            CG_FoodKind cg_foodKind = GetCG_FoodKind(food);
            if (cg_foodKind == CG_FoodKind.Any) return null;

            if (dietCategory == DietCategory.Carnivore || dietCategory == DietCategory.Hypercarnivore)
            {
                if (cg_foodKind == CG_FoodKind.Meat || cg_foodKind == CG_FoodKind.AnimalProduct) return null;
                else return CG_DefOf.AtePlantCarnivore;
            }
            if (dietCategory == DietCategory.Herbivore || dietCategory == DietCategory.StrictHerbivore)
            {
                if (cg_foodKind == CG_FoodKind.Vegan) return null;
                else if (cg_foodKind == CG_FoodKind.Meat || cg_foodKind == CG_FoodKind.MeatAndVeg) return CG_DefOf.AteMeatHerbivore;
                else return CG_DefOf.AteAnimalProductHerbivore;
            }
            //fall-through : shouldn't happen
            return null;
        }

        public static void AddIndigestionHediff(Pawn pawn, Thing food)
        {
            float nutritionFactor = NutritionFactorFromGeneticDiet(food, pawn);
            if (nutritionFactor == 1f) return;

            float indigestionOffset = INDIGESTION_FACTOR * (1 - nutritionFactor) * food.def.GetStatValueAbstract(StatDefOf.Nutrition, null);

            Hediff indigestionHediff = pawn.health.hediffSet.GetFirstHediffOfDef(CG_DefOf.DietaryIndigestion, false);
            if (indigestionHediff == null)
            {
                pawn.health.AddHediff(HediffMaker.MakeHediff(CG_DefOf.DietaryIndigestion, pawn, null), null, null, null);
            }

            indigestionHediff = pawn.health.hediffSet.GetFirstHediffOfDef(CG_DefOf.DietaryIndigestion, false);

            indigestionHediff.Severity += indigestionOffset;

            if (indigestionHediff.Severity > 1) indigestionHediff.Severity = 1;
        }

    }

}
