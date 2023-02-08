using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyanobotsGenes
{
    class BodyfeederUtility
    {
        public const float BASE_HEMOGEN_PER_NUTRITION = 0.6f;

        public static float HemogenPerNutrition(Pawn pawn, Thing food)
        {
            return BASE_HEMOGEN_PER_NUTRITION * GeneticDietUtility.ProportionHumanlike(food);
        }

        public static float BodyfeederNutritionWanted(Pawn pawn, Thing food)
        {
            if (!(FoodUtility.IsHumanlikeCorpseOrHumanlikeMeatOrIngredient(food) || (food is Pawn && (food as Pawn).RaceProps.Humanlike)) return 0f;
            if (pawn.genes == null || !pawn.genes.HasGene(GeneDefOf.Hemogenic)) return 0f;

            float efficiency;
            if (food is Pawn)
            {
                efficiency = BASE_HEMOGEN_PER_NUTRITION;
            }
            else
            {
                efficiency = HemogenPerNutrition(pawn, food);
            }
            //Log.Message("efficiency: " + efficiency);
            Gene_Hemogen gene_Hemogen = pawn.genes.GetFirstGeneOfType<Gene_Hemogen>();

            return (gene_Hemogen.Max - gene_Hemogen.Value) / efficiency;
        }

        public static float HemogenLevelPct(Pawn pawn)
        {
            if (pawn.genes == null || !pawn.genes.HasGene(GeneDefOf.Hemogenic)) return 1f;
            Gene_Hemogen gene_Hemogen = pawn.genes.GetFirstGeneOfType<Gene_Hemogen>();
            return gene_Hemogen.Value / gene_Hemogen.Max;
        }

        public static int NumToIngestForHemogen(Pawn pawn, Thing food)
        {
            if (food is Corpse) return 1;
            Gene_Hemogen gene_Hemogen = pawn.genes.GetFirstGeneOfType<Gene_Hemogen>();
            Log.Message("hemogen wanted: " + (gene_Hemogen.Max - gene_Hemogen.Value));
            Log.Message("nutrition wanted: " + BodyfeederNutritionWanted(pawn, food));
            int numWanted = (int)(BodyfeederNutritionWanted(pawn, food) / FoodUtility.GetNutrition(pawn, food, food.def));
            Log.Message("numWanted: " + numWanted);
            Log.Message("maxNumToIngestAtOnce: " + food.def.ingestible.maxNumToIngestAtOnce);
            return Math.Max(1, Math.Min(food.def.ingestible.maxNumToIngestAtOnce, numWanted));
        }


        public static Thing TryGetHumanlikeFood(Pawn pawn, bool desperate, float searchRadius = -1f)
        {
            Thing carriedThing = pawn.carryTracker.CarriedThing;
            if (carriedThing != null && FoodUtility.IsHumanlikeCorpseOrHumanlikeMeatOrIngredient(carriedThing))
            {
                return carriedThing;
            }
            if (pawn.inventory != null)
            {
                foreach (Thing inventoryThing in pawn.inventory.innerContainer)
                {
                    if (inventoryThing.def.IsIngestible && FoodUtility.IsHumanlikeCorpseOrHumanlikeMeatOrIngredient(inventoryThing))
                    {
                        CompRottable compRottable = inventoryThing.TryGetComp<CompRottable>();
                        if (compRottable == null) return inventoryThing;
                        else if ((compRottable.Stage == RotStage.Fresh && compRottable.TicksUntilRotAtCurrentTemp < 30000) || desperate)
                        {
                            return inventoryThing;
                        }
                    }
                }
            }
            Predicate<Thing> foodValidator = delegate (Thing t)
            {
                if (t.IngestibleNow && t.def.IsNutritionGivingIngestible
                    && !(t is Corpse) && t.IsSociallyProper(pawn)
                    && !t.IsDessicated() && (desperate || !t.IsNotFresh())
                    && pawn.WillEat_NewTemp(t) && !t.IsForbidden(pawn)
                    && FoodUtility.IsHumanlikeCorpseOrHumanlikeMeatOrIngredient(t)
                    && pawn.CanReserve(t)
                    && (searchRadius == -1f || (float)(t.Position - pawn.Position).LengthHorizontalSquared <= searchRadius * searchRadius))
                {
                    return true;
                }
                return false;
            };
            Thing mapFood = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map,
                pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree),
                PathEndMode.ClosestTouch, TraverseParms.For(pawn), validator: foodValidator);
            if (mapFood != null) return mapFood;

            Predicate<Thing> corpseValidator = delegate (Thing t)
            {
                if (t.IngestibleNow && t.def.IsNutritionGivingIngestible
                    && (t is Corpse) && t.IsSociallyProper(pawn)
                    && !t.IsDessicated() && (desperate || !t.IsNotFresh())
                    && pawn.WillEat_NewTemp(t) && !t.IsForbidden(pawn)
                    && FoodUtility.IsHumanlikeCorpseOrHumanlikeMeat(t, t.def)
                    && pawn.CanReserve(t)
                    && (searchRadius == -1f || (float)(t.Position - pawn.Position).LengthHorizontalSquared <= searchRadius * searchRadius))
                {
                    return true;
                }
                return false;
            };
            Thing mapCorpse = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map,
                pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree),
                PathEndMode.ClosestTouch, TraverseParms.For(pawn), validator: corpseValidator);
            if (mapCorpse != null) return mapCorpse;

            return null;
        }
    }

}
