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
        public const float BASE_CORPSE_NUTRITION = 5.2f;        //as observed in game
        public const float BASE_HEMOGEN_PER_NUTRITION = 0.4f;
        public const float BLOOD_FACTOR = 0.2f;
        public const float COOKED_FACTOR = 0.7f;
        public const float LIVE_FACTOR = 1.6f;
        public const float ROTTING_FACTOR = 0.2f;

        public static float HemogenPerNutritionForLivePawn => BASE_HEMOGEN_PER_NUTRITION * LIVE_FACTOR;

        public enum Desperation
        {
            None,
            Hungry,
            StarvationMild,
            StarvationModerate,
            StarvationSevere
        };

        public static Desperation GetDesperation(Pawn pawn)
        {
            if (pawn == null || !IsBodyFeeder(pawn)) return Desperation.None;
            if (pawn.MentalState is MentalState_BodyfeederBerserk) return Desperation.StarvationSevere;
            Gene_Hemogen gene_Hemogen = pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>();
            if (gene_Hemogen == null || gene_Hemogen.Resource.ValuePercent > 0.3) return Desperation.None;
            Hediff_BodyfeederStarvation hediff_Starvation = pawn.health?.hediffSet?.GetFirstHediff<Hediff_BodyfeederStarvation>();
            if (hediff_Starvation == null) return Desperation.Hungry;
            if (hediff_Starvation.CurStageIndex == 0) return Desperation.StarvationMild;
            if (hediff_Starvation.CurStageIndex == 1) return Desperation.StarvationModerate;
            if (hediff_Starvation.CurStageIndex == 2) return Desperation.StarvationSevere;
            return Desperation.None;
        }

        public static bool IsBodyFeeder(Pawn pawn)
        {
            return pawn.HasActiveGene(CG_DefOf.CYB_Bodyfeeder);
        }

        public static float HemogenPerNutrition(Pawn pawn, Thing food)
        {
            float hpn = BASE_HEMOGEN_PER_NUTRITION;
            if (food is Pawn)
            {
                Pawn victim = food as Pawn;
                if (!victim.RaceProps.Humanlike)
                {
                    //Log.Message("food is pawn, not humanlike, hpn 0");
                    return 0f;
                }
                    
                if (victim.Dead)
                {
                    food = victim.Corpse;
                    //Log.Message("food is dead pawn, retrieving corpse: " + victim.Corpse);
                }
                else
                {
                    hpn *= LIVE_FACTOR;
                    //Log.Message("food is live pawn, hpn: " + hpn);
                    return hpn;
                }
            }
            if (food is Corpse)
            {
                Corpse corpse = food as Corpse;
                if (!corpse.InnerPawn.RaceProps.Humanlike)
                {
                    //Log.Message("food is corpse, not humanlike, hpn 0");
                    return 0f;
                }
                if (corpse.GetRotStage() == RotStage.Rotting)
                {
                    hpn *= ROTTING_FACTOR;
                    //Log.Message("food is rotting corpse");
                }
                //Log.Message("food is corpse, hpn: " + hpn);
                return hpn;
            }
            if (FoodUtility.IsHumanlikeCorpseOrHumanlikeMeatOrIngredient(food))
            {
                if (food.def.IsMeat)
                {
                    //Log.Message("food is humanlike meat, hpn: " + hpn);
                    return hpn;
                }
                hpn *= COOKED_FACTOR * GeneticDietUtility.ProportionHumanlike(food);
                //Log.Message("food is humanlike, not meat, hpn: " + hpn);
                return hpn;
            }
            List<IngestionOutcomeDoer> outcomeDoers = food.def.ingestible?.outcomeDoers;
            IngestionOutcomeDoer_OffsetHemogen outcomeDoer = null;
            if (!outcomeDoers.NullOrEmpty()) outcomeDoer = (IngestionOutcomeDoer_OffsetHemogen)food.def.ingestible?.outcomeDoers?.FirstOrDefault(x => x is IngestionOutcomeDoer_OffsetHemogen);
            if (outcomeDoer != null)
            {
                hpn = (outcomeDoer.offset * BLOOD_FACTOR) / FoodUtility.NutritionForEater(pawn, food);
                //Log.Message("food is hemogen source, hpn: " + hpn);
                return hpn;
            }
            //Log.Message("Fell through, food not a source of hemogen, hpn: 0");
            return 0f;
        }

        public static float HemogenWanted(Pawn pawn)
        {
            if (!pawn.HasActiveGene(GeneDefOf.Hemogenic)) return 0f;
            Gene_Hemogen gene_Hemogen = pawn.genes.GetFirstGeneOfType<Gene_Hemogen>();
            return gene_Hemogen.Max - gene_Hemogen.Value;
        }

        public static float BodyfeederNutritionWanted(Pawn pawn, Thing food)
        {
            if (!(CanIngestForHemogen(pawn, food) || (food is Pawn && (food as Pawn).RaceProps.Humanlike))) return 0f;

            float efficiency = HemogenPerNutrition(pawn, food);
            float hemogenWanted = HemogenWanted(pawn);

            //Log.Message("efficiency: " + efficiency + ", hemogenWanted: " + hemogenWanted + ", nutrition wanted: " + (hemogenWanted / efficiency).ToString());
            float nutritionWanted = hemogenWanted / efficiency;

            if (food is Pawn || food is Corpse)
            {
                //cap on desired nutrition from pawn/corpse to encourage piecemeal eating
                nutritionWanted = Mathf.Min(nutritionWanted, 1.5f);
            }

            return nutritionWanted;
        }

        public static float HemogenLevelPct(Pawn pawn)
        {
            if (!pawn.HasActiveGene(GeneDefOf.Hemogenic)) return 1f;
            Gene_Hemogen gene_Hemogen = pawn.genes.GetFirstGeneOfType<Gene_Hemogen>();
            LogUtil.DebugLog($"HemogenLevelPct - pawn: {pawn}, result: {gene_Hemogen.Value / gene_Hemogen.Max}");
            return gene_Hemogen.Value / gene_Hemogen.Max;
        }

        public static bool IsStandardHemogenSource(Thing ingestible)
        {
            return ingestible.def.ingestible?.outcomeDoers?.Any(
                x => x is IngestionOutcomeDoer_OffsetHemogen offsetHemogen
                && offsetHemogen.offset > 0f
                ) ?? false;
        }

        public static bool CanIngestForHemogen(Pawn pawn, Thing food)
        {
            if (!food.def.IsIngestible) return false;
            if (FoodUtility.IsHumanlikeCorpseOrHumanlikeMeatOrIngredient(food)) return true;
            if (IsStandardHemogenSource(food)) return true;
            return false;
        }

        public static int NumRequiredForHemogen(Pawn pawn, Thing food)
        {
            if (food is Corpse || food is Pawn) return 1;

            float hemogenWanted = HemogenWanted(pawn);
            int numWanted = Mathf.FloorToInt(BodyfeederNutritionWanted(pawn, food) / FoodUtility.GetNutrition(pawn, food, food.def));
            
            //Log.Message("hemogenWanted: " + hemogenWanted + ", nutrition wanted: " + BodyfeederNutritionWanted(pawn, food) + ", numWanted: " + numWanted);
            return Math.Max(1, numWanted);
        }

        public static bool IsRottingCorpse(Thing thing)
        {
            if (thing is Corpse corpse && corpse.GetRotStage() == RotStage.Rotting) return true;
            return false;
        }

        public static bool CorpseIngestibleForHemogenNow(Corpse corpse)
        {
            if (corpse.Bugged)
            {
                Log.Error("CorpseIngestibleForHemogenNow on Corpse while Bugged.");
                return false;
            }
            if (corpse.IsBurning())
            {
                return false;
            }
            if (!corpse.InnerPawn.RaceProps.IsFlesh)
            {
                return false;
            }
            if (corpse.GetRotStage() == RotStage.Dessicated)
            {
                return false;
            }
            return true;
        }

        public static float RottingCorpseIngested(Pawn ingester, Corpse corpse)
        {
            ThingDef def = corpse.def;
            if (corpse.Destroyed)
            {
                Log.Error(string.Concat(ingester, " ingested destroyed thing ", corpse));
                return 0f;
            }
            if (!CorpseIngestibleForHemogenNow(corpse))
            {
                Log.Error(string.Concat(ingester, " ingested CorpseIngestibleForHemogenNow=false thing ", corpse));
                return 0f;
            }
            ingester.mindState.lastIngestTick = Find.TickManager.TicksGame;
            if (ingester.needs.mood != null)
            {
                List<FoodUtility.ThoughtFromIngesting> list = FoodUtility.ThoughtsFromIngesting(ingester, corpse, def);
                for (int i = 0; i < list.Count; i++)
                {
                    Thought_Memory thought_Memory = ThoughtMaker.MakeThought(list[i].thought, list[i].fromPrecept);
                    if (thought_Memory is Thought_FoodEaten thought_FoodEaten)
                    {
                        thought_FoodEaten.SetFood(corpse);
                    }
                    ingester.needs.mood.thoughts.memories.TryGainMemory(thought_Memory);
                }
            }
            if (ingester.needs.drugsDesire != null)
            {
                ingester.needs.drugsDesire.Notify_IngestedDrug(corpse);
            }
            if (ingester.IsColonist)
            {
                TaleRecorder.RecordTale(TaleDefOf.AteRawHumanlikeMeat, ingester);
            }
            ingester.mindState.lastHumanMeatIngestedTick = Find.TickManager.TicksGame;
            Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.AteHumanMeat, ingester.Named(HistoryEventArgsNames.Doer)), canApplySelfTookThoughts: false);
            Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.AteHumanMeatDirect, ingester.Named(HistoryEventArgsNames.Doer)), canApplySelfTookThoughts: false);

            if (def.ingestible.ateEvent != null)
            {
                Find.HistoryEventsManager.RecordEvent(new HistoryEvent(def.ingestible.ateEvent, ingester.Named(HistoryEventArgsNames.Doer)), canApplySelfTookThoughts: false);
            }

            float nutritionWanted = BodyfeederNutritionWanted(ingester, corpse);
            IngestedCalculateAmounts(ingester, corpse.InnerPawn, nutritionWanted, out var numTaken, out var nutritionIngested, out var bodyPartDef);
            if (!ingester.Dead && ingester.needs.joy != null && Mathf.Abs(def.ingestible.joy) > 0.0001f && numTaken > 0)
            {
                JoyKindDef joyKind = ((def.ingestible.joyKind != null) ? def.ingestible.joyKind : JoyKindDefOf.Gluttonous);
                ingester.needs.joy.GainJoy((float)numTaken * def.ingestible.joy, joyKind);
            }
            //Log.Message("About to apply food poisoning for DangerousFoodType, corpse.Bugged: " + corpse.Bugged);
            float poisonChanceOverride;
            float chance = (FoodUtility.TryGetFoodPoisoningChanceOverrideFromTraits(ingester, corpse, out poisonChanceOverride) ? poisonChanceOverride : (corpse.GetStatValue(StatDefOf.FoodPoisonChanceFixedHuman) * FoodUtility.GetFoodPoisonChanceFactor(ingester)));
            if (ingester.RaceProps.Humanlike && Rand.Chance(chance))
            {
                FoodUtility.AddFoodPoisoningHediff(ingester, corpse, FoodPoisonCause.DangerousFoodType);
            }
            List<Hediff> hediffs = ingester.health.hediffSet.hediffs;
            for (int k = 0; k < hediffs.Count; k++)
            {
                hediffs[k].Notify_IngestedThing(corpse, numTaken);
            }
            //Log.Message("Attempting to notify genes, bodyfeeder gene: " + ingester.genes?.GetFirstGeneOfType<Gene_Bodyfeeder>());
            ingester.genes?.Notify_IngestedThing(corpse, numTaken);
            ingester.genes?.GetFirstGeneOfType<Gene_Bodyfeeder>().Notify_IngestedCorpse(corpse, nutritionIngested);
            bool fullyEaten = false;
            if (numTaken > 0)
            {
                if (corpse.stackCount == 0)
                {
                    Log.Error(string.Concat(corpse, " stack count is 0."));
                }
                if (numTaken == corpse.stackCount)
                {
                    fullyEaten = true;
                }
                else
                {
                    corpse.SplitOff(numTaken);
                }
            }
            foreach (ThingComp comp in corpse.AllComps)
            {
                comp.PrePostIngested(ingester);
            }
            if (fullyEaten)
            {
                ingester.carryTracker.innerContainer.Remove(corpse);
            }
            if (def.ingestible.outcomeDoers != null)
            {
                for (int l = 0; l < def.ingestible.outcomeDoers.Count; l++)
                {
                    def.ingestible.outcomeDoers[l].DoIngestionOutcome(ingester, corpse, numTaken);
                }
            }
            foreach (ThingComp comp in corpse.AllComps)
            {
                comp.PostIngested(ingester);
            }
            if (fullyEaten)
            {
                corpse.Destroy();
            } 
            return nutritionIngested;
        }

        public static void IngestedCalculateAmounts(Pawn ingester, Pawn victim, float nutritionWanted, out int numTaken, out float nutritionIngested, out BodyPartDef bodyPartDef)
        {
            BodyPartRecord bodyPartRecord = GetBestBodyPartToEat(victim, nutritionWanted);
            //Log.Message("GetBestBodyPartToEat: " + bodyPartRecord);
            if (bodyPartRecord == null)
            {
                Log.Error(string.Concat(ingester, " ate ", victim, " but no body part was found. Replacing with core part."));
                bodyPartRecord = victim.RaceProps.body.corePart;
            }
            bodyPartDef = bodyPartRecord.def;
            float bodyPartNutrition = GetBodyPartNutrition(victim, bodyPartRecord);
            //Log.Message("GetBodyPartNutrition: " + bodyPartNutrition);

            if (!victim.Dead)
            {
                int bloodAmount = Mathf.Max(GenMath.RoundRandom(victim.BodySize * 12f), 1);
                for (int i = 0; i < bloodAmount; i++)
                {
                    victim.health.DropBloodFilth();
                }
            }

            if (bodyPartRecord == victim.RaceProps.body.corePart)
            {
                if (PawnUtility.ShouldSendNotificationAbout(victim) && victim.RaceProps.Humanlike)
                {
                    Messages.Message("MessageEatenByPredator".Translate(victim.LabelShort, ingester.Named("PREDATOR"), victim.Named("EATEN")).CapitalizeFirst(), ingester, MessageTypeDefOf.NegativeEvent);
                }

                if (!victim.Dead)
                {
                    int damageAmount = Mathf.Clamp((int)victim.health.hediffSet.GetPartHealth(bodyPartRecord) - 1, 1, 20);
                    if (ModsConfig.BiotechActive && victim.HasActiveGene(GeneDefOf.Deathless))
                    {
                        damageAmount = 99999;
                        if (victim.health.hediffSet.HasHediff(HediffDefOf.MechlinkImplant))
                        {
                            Hediff firstHediffOfDef = victim.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.MechlinkImplant);
                            if (bodyPartRecord.GetPartAndAllChildParts().Contains(firstHediffOfDef.Part))
                            {
                                GenSpawn.Spawn(ThingDefOf.Mechlink, victim.Position, victim.Map);
                            }
                        }
                    }
                    DamageInfo damageInfo = new DamageInfo(DamageDefOf.Bite, damageAmount, 999f, -1f,
                        ingester, bodyPartRecord, null, DamageInfo.SourceCategory.ThingOrUnknown,
                        null, instigatorGuilty: true, true);
                    victim.TakeDamage(damageInfo);

                    int bloodAmount = Mathf.Max(GenMath.RoundRandom(victim.BodySize * 40f), 1);
                    for (int i = 0; i < bloodAmount; i++)
                    {
                        victim.health.DropBloodFilth();
                    }

                    victim.Kill(damageInfo);
                }
                else
                {
                    int bloodAmount = Mathf.Max(GenMath.RoundRandom(victim.BodySize * 10f), 1);
                    for (int i = 0; i < bloodAmount; i++)
                    {
                        victim.health.DropBloodFilth();
                    }
                }

                numTaken = 1;
            }
            else
            {
                Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, victim, bodyPartRecord);
                hediff_MissingPart.lastInjury = HediffDefOf.Bite;
                hediff_MissingPart.IsFresh = true;
                victim.health.AddHediff(hediff_MissingPart);
                numTaken = 0;
            }

            //sound goes here

            nutritionIngested = bodyPartNutrition;
        }

        public static BodyPartRecord GetBestBodyPartToEat(Pawn victim, float nutritionWanted)
        {
            IEnumerable<BodyPartRecord> source = from x in victim.health.hediffSet.GetNotMissingParts()
                                                 where x.depth == BodyPartDepth.Outside && GetBodyPartNutrition(victim, x) > 0.001f
                                                 select x;
            if (!source.Any())
            {
                return null;
            }
            BodyPartRecord result = source.MinBy((BodyPartRecord x) => Mathf.Abs(GetBodyPartNutrition(victim, x) - nutritionWanted));
            return result;
        }

        public static float GetBodyPartNutrition(Pawn victim, BodyPartRecord part)
        {
            float totalNutrition = BASE_CORPSE_NUTRITION;

            totalNutrition *= victim.BodySize;

            HediffSet hediffSet = victim.health.hediffSet;
            float coverageOfNotMissingNaturalParts = hediffSet.GetCoverageOfNotMissingNaturalParts(victim.RaceProps.body.corePart);
            if (coverageOfNotMissingNaturalParts <= 0f)
            {
                return 0f;
            }

            float partCoverage = hediffSet.GetCoverageOfNotMissingNaturalParts(part);
            float nutrition = totalNutrition * partCoverage;
            if (!victim.Dead)
            {
                nutrition *= 2f;
            }
            return nutrition;
        }

        public static Thing TryGetHemogenSource(Pawn pawn, float searchRadius = -1f)
        {
            Thing carriedThing = pawn.carryTracker.CarriedThing;
            if (carriedThing != null && CanIngestForHemogen(pawn, carriedThing))
            {
                return carriedThing;
            }

            Desperation desperation = GetDesperation(pawn);
            //Log.Message("desperation: " + desperation);

            Predicate<Thing> desperationValidator;
            //if merely hungry: no forbidden, no entire corpses (unless we're already okay with that)
            if (desperation < Desperation.StarvationMild)
            {
                //Log.Message("desperation hungry/none");
                //Log.Message("CanEverEat: " + pawn.RaceProps.CanEverEat(ThingDefOf.HemogenPack)
                //    + ", InappropriateForTitle: " + FoodUtility.InappropriateForTitle(ThingDefOf.HemogenPack, pawn, false));
                desperationValidator = delegate (Thing t)
                {
                    if (t.IsForbidden(pawn) || !pawn.RaceProps.CanEverEat(t)
                        || !t.IsSociallyProper(pawn) || FoodUtility.InappropriateForTitle(t.def, pawn, false)
                        || (t is Corpse && !(pawn.story.traits.HasTrait(DefDatabase<TraitDef>.GetNamed("Cannibal")) || pawn.HasActiveGene(CG_DefOf.CYB_Hypercarnivore))))
                    {
                        return false;
                    }
                    return true;
                };
            }
            //if starvation mild: no forbidden, entire corpses okay
            else if (desperation < Desperation.StarvationModerate)
            {
                //Log.Message("desperation mild");
                desperationValidator = delegate (Thing t)
                {
                    if (t.IsForbidden(pawn) || !pawn.RaceProps.CanEverEat(t)
                        || !t.IsSociallyProper(pawn))
                    {
                        return false;
                    }
                    return true;
                };
            }
            //if starvation moderate+ : forbidden okay, entire corpses okay
            else
            {
                //Log.Message("desperation moderate");
                desperationValidator = delegate (Thing t) { return true; };
            }

            //First see if we have anything in our inventory
            if (pawn.inventory != null)
            {
                foreach (Thing inventoryThing in pawn.inventory.innerContainer)
                {
                    if (CanIngestForHemogen(pawn,inventoryThing))
                    {
                        CompRottable compRottable = inventoryThing.TryGetComp<CompRottable>();
                        if (compRottable == null) return inventoryThing;
                        else if ((compRottable.Stage == RotStage.Fresh && compRottable.TicksUntilRotAtCurrentTemp < 30000) || desperation > Desperation.StarvationModerate)
                        {
                            return inventoryThing;
                        }
                    }
                }
            }
            //Log.Message("Found nothing in inventory");

            //Second check for non-corpse food on the map
            Predicate<Thing> foodValidator = delegate (Thing t)
            {
                if (t.IngestibleNow && t.def.IsNutritionGivingIngestible
                    && !(t is Corpse) && desperationValidator(t)
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
            //Log.Message("mapFood: " + mapFood);
            if (mapFood != null) return mapFood;

            //Log.Message("Found no non corpse map food");

            //Third check for fresh corpses on the map
            Predicate<Thing> corpseValidator = delegate (Thing t)
            {
                if (t.IngestibleNow && (t is Corpse) && FoodUtility.IsHumanlikeCorpseOrHumanlikeMeat(t, t.def)
                    && pawn.CanReserve(t) && desperationValidator(t)
                    && (searchRadius == -1f || (float)(t.Position - pawn.Position).LengthHorizontalSquared <= searchRadius * searchRadius))
                {
                    return true;
                }
                return false;
            };
            Thing mapCorpse = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map,
                pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse),
                PathEndMode.ClosestTouch, TraverseParms.For(pawn), validator: corpseValidator);
            //Log.Message("mapCorpse: " + mapCorpse);
            if (mapCorpse != null) return mapCorpse;

            //Log.Message("Found no map corpse");

            //Fourth check for ingestibles with IngestionOutcomeDoer_OffsetHemogen
            Predicate<Thing> sourceValidator = delegate (Thing t)
            {
                if (t.IngestibleNow && pawn.CanReserve(t)
                    && CanIngestForHemogen(pawn,t)
                    && desperationValidator(t)
                    && (searchRadius == -1f || (float)(t.Position - pawn.Position).LengthHorizontalSquared <= searchRadius * searchRadius))
                {
                    return true;
                }
                return false;
            };
            Thing hemogenSource = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map,
                pawn.Map.listerThings.AllThings, PathEndMode.ClosestTouch,
                TraverseParms.For(pawn), validator: sourceValidator);
            //Log.Message("hemogenPack: " + hemogenPack);
            if (hemogenSource != null) return hemogenSource;

            //Finally, only if really starving, check for rotting corpses
            if (desperation > Desperation.StarvationModerate)
            {
                Predicate<Thing> rottingValidator = delegate (Thing t)
                {
                    if ((t is Corpse corpse) && corpse.GetRotStage() == RotStage.Rotting
                        && CorpseIngestibleForHemogenNow(corpse) && pawn.CanReserve(t)
                        && (searchRadius == -1f || (float)(t.Position - pawn.Position).LengthHorizontalSquared <= searchRadius * searchRadius))
                    {
                        return true;
                    }
                    return false;
                };
                mapCorpse = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map,
                    pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse),
                    PathEndMode.ClosestTouch, TraverseParms.For(pawn), validator: rottingValidator);
                //Log.Message("mapCorpse (rotting): " + mapCorpse);
                if (mapCorpse != null) return mapCorpse;
            }

            return null;
        }

        public static Job MakeIngestForHemogenJob(Pawn pawn, float searchRadius = -1f)
        {
            Thing ingestible = TryGetHemogenSource(pawn, searchRadius);
            if (ingestible == null) return null;
            Job job = JobMaker.MakeJob(CG_DefOf.CYB_IngestForHemogen, ingestible);
            job.count = NumRequiredForHemogen(pawn, ingestible);
            return job;
        }
    }
}
