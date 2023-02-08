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
    class JobDriver_IngestDowned : JobDriver
    {
        private static int baseIngestTicks = 500;

        private Pawn Victim => job.GetTarget(TargetIndex.A).Pawn;
        private float ChewDurationMultiplier => 1f / pawn.GetStatValue(StatDefOf.EatingSpeed);

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Victim, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return ChewToil(pawn, Victim, ChewDurationMultiplier, TargetIndex.A);
        }

        private static Toil ChewToil(Pawn chewer, Pawn victim, float durationMultiplier, TargetIndex ingestibleInd)
        {
            Toil toil = ToilMaker.MakeToil("ChewDowned");
            toil.initAction = delegate
            {
                if (victim == null || victim.IsBurning() || !victim.RaceProps.Humanlike)
                {
                    chewer.jobs.EndCurrentJob(JobCondition.Incompletable);
                }
                else
                {
                    chewer.pather.StopDead();
                    chewer.jobs.curDriver.ticksLeftThisToil = 
                        Mathf.RoundToInt((float)baseIngestTicks * durationMultiplier);
                    if (victim.Spawned)
                    {
                        victim.Map.physicalInteractionReservationManager.Reserve(chewer, chewer.CurJob, victim);
                    }
                }
            };
            toil.tickAction = delegate
            {
                if (victim != null && victim.Spawned)
                {
                    chewer.rotationTracker.FaceCell(victim.Position);
                }
            };
            toil.WithProgressBar(ingestibleInd, delegate
            {
                return (victim == null) ? 1f 
                    : (1f - (float)chewer.jobs.curDriver.ticksLeftThisToil 
                    / Mathf.Round((float)baseIngestTicks * durationMultiplier));
            });
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.FailOnDestroyedOrNull(ingestibleInd);
            toil.AddFinishAction(delegate
            {
                if (chewer != null && chewer.CurJob != null && victim != null
                    && chewer.Map.physicalInteractionReservationManager.IsReservedBy(chewer, victim))
                {
                    chewer.Map.physicalInteractionReservationManager.Release(chewer, chewer.CurJob, victim);
                }
            });
            toil.handlingFacing = true;
            toil.WithEffect(EffecterDefOf.EatMeat, ingestibleInd);
            toil.PlaySustainerOrSound(SoundDefOf.RawMeat_Eat);

            return toil;
        }

        private static Toil FinalizeIngest(Pawn ingester, Pawn victim, TargetIndex ingestibleInd)
        {
            Toil toil = ToilMaker.MakeToil("FinalizeIngestDowned");
            toil.initAction = delegate
            {
                if (victim.DestroyedOrNull() || victim.IsBurning())
                {
                    ingester.jobs.EndCurrentJob(JobCondition.Errored);
                }

                if (ingester.needs.mood != null)
                {
                    if (!(ingester.Position + ingester.Rotation.FacingCell).HasEatSurface(ingester.Map) && !ingester.IsWildMan())
                    {
                        ingester.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.AteWithoutTable);
                    }
                    Room room = ingester.GetRoom();
                    if (room != null)
                    {
                        int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness));
                        if (ThoughtDefOf.AteInImpressiveDiningRoom.stages[scoreStageIndex] != null)
                        {
                            ingester.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(ThoughtDefOf.AteInImpressiveDiningRoom, scoreStageIndex));
                        }
                    }

                    //based off FoodUtility.AddThoughtsFromIdeo
                    if (ingester.Ideo != null)
                    {
                        List<Precept> preceptsListForReading = ingester.Ideo.PreceptsListForReading;
                        foreach (Precept precept in preceptsListForReading)
                        {
                            List<PreceptComp> comps = precept.def.comps;
                            foreach (PreceptComp comp in comps)
                            {
                                if (comp is PreceptComp_SelfTookMemoryThought preceptComp_SelfTookMemoryThought 
                                    && preceptComp_SelfTookMemoryThought.eventDef == HistoryEventDefOf.AteHumanMeat)
                                {
                                    //based off FoodUtility.TryAddIngestThought
                                    ThoughtDef thoughtDef = preceptComp_SelfTookMemoryThought.thought;
                                    if (ThoughtUtility.NullifyingGene(thoughtDef, ingester) != null 
                                        || ThoughtUtility.NullifyingHediff(thoughtDef, ingester) != null 
                                        || ThoughtUtility.NullifyingPrecept(thoughtDef, ingester) != null 
                                        || (precept.def.enabledForNPCFactions && !ingester.CountsAsNonNPCForPrecepts()))
                                    {
                                        continue;
                                    }
                                    
                                    if (ingester.story != null && ingester.story.traits != null)
                                    {
                                        if (ingester.story.traits.IsThoughtFromIngestionDisallowed(thoughtDef, 
                                            victim.RaceProps.corpseDef, MeatSourceCategory.Humanlike))
                                        {
                                            continue;
                                        }
                                    }

                                    ingester.needs.mood.thoughts.memories.TryGainMemory(thoughtDef, victim, precept);
                                }
                            }
                        }
                    }
                }

                Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.AteHumanMeatDirect));

                float nutritionWanted = BodyfeederUtility.BodyfeederNutritionWanted(ingester, victim);
                IngestedCalculateAmounts(ingester, victim, nutritionWanted, out int numTaken, out float nutritionIngested, out BodyPartDef bodyPartDef);

                /*
                List<Hediff> hediffs = ingester.health.hediffSet.hediffs;
                for (int k = 0; k < hediffs.Count; k++)
                {
                    hediffs[k].Notify_IngestedThing(this, numTaken);
                }
                ingester.genes?.Notify_IngestedThing(this, numTaken);
                */

                //can't actually pass directly because victim is not an ingestible,
                //but need to replicate behaviours




                ingester.needs.food.CurLevel += nutritionIngested;
                ingester.records.AddTo(RecordDefOf.NutritionEaten, nutritionIngested);

                float hemogenOffset = nutritionIngested * BodyfeederUtility.BASE_HEMOGEN_PER_NUTRITION;
                GeneUtility.OffsetHemogen(ingester, hemogenOffset);

                if (numTaken != 0)
                {
                    //handles execution thoughts and history events
                    ThoughtUtility.GiveThoughtsForPawnExecuted(victim, ingester, PawnExecutionKind.GenericBrutal);

                    //ingester thoughts on berserk cannibal murder
                    if (ingester.needs.mood != null)
                    {
                        int stageIndex = 0;
                        if (ingester.relations != null && ingester.relations.OpinionOf(victim) >= 20)
                        {
                            stageIndex = 2;
                        }
                        else if (ingester.IsColonist && victim.IsColonist)
                        {
                            stageIndex = 1;
                        }
                        Thought_Memory memory_BodyfeederAtePersonWhileBerserk = ThoughtMaker.MakeThought(
                            CG_DefOf.BodyfeederAtePersonWhileBerserk, stageIndex);
                        ingester.needs.mood.thoughts.memories.TryGainMemory(memory_BodyfeederAtePersonWhileBerserk, victim);
                    }

                    TaleRecorder.RecordTale(CG_DefOf.BodyfeederAtePerson, ingester, victim);
                    
                    if (ingester.IsCarryingPawn(victim))
                    {
                        ingester.carryTracker.DestroyCarriedThing();
                    }
                    else
                    {
                        victim.Destroy();
                    }

                }
                else
                {
                    //equivalent to ThoughtUtility.GiveThoughtsForPawnExecuted
                    if (victim.guilt.IsGuilty || victim.Faction.HostileTo(Faction.OfPlayer))
                    {
                        Find.HistoryEventsManager.RecordEvent(new HistoryEvent(CG_DefOf.BodyfeederAteBodyPartEnemy,
                            ingester.Named(HistoryEventArgsNames.Doer), victim.Named(HistoryEventArgsNames.Victim)));
                    }
                    else if (victim.IsColonist)
                    {
                        Find.HistoryEventsManager.RecordEvent(new HistoryEvent(CG_DefOf.BodyfeederAteBodyPartColonist));
                    }
                    else
                    {
                        Find.HistoryEventsManager.RecordEvent(new HistoryEvent(CG_DefOf.BodyfeederAteBodyPartNeutral));
                    }

                    if (ingester.needs.mood != null)
                    {
                        int stageIndex = 0;
                        if (ingester.relations != null && ingester.relations.OpinionOf(victim) >= 20)
                        {
                            stageIndex = 2;
                        }
                        else if (ingester.IsColonist && victim.IsColonist)
                        {
                            stageIndex = 1;
                        }
                        Thought_Memory memory_BodyfeederAteBodyPartWhileBerserk = ThoughtMaker.MakeThought(
                            CG_DefOf.BodyfeederAteBodyPartWhileBerserk, stageIndex);
                        ingester.needs.mood.thoughts.memories.TryGainMemory(memory_BodyfeederAteBodyPartWhileBerserk, victim);
                    }
                    if (victim.needs.mood != null)
                    {
                        Thought_Memory memory_BodyfeederAteMyBodyPart = ThoughtMaker.MakeThought(
                            CG_DefOf.BodyfeederAteMyBodyPart, 0);
                        victim.needs.mood.thoughts.memories.TryGainMemory(memory_BodyfeederAteMyBodyPart, ingester);
                    }
                    TaleRecorder.RecordTale(CG_DefOf.BodyfeederAteBodyPart, ingester, victim, bodyPartDef);
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }

        private static void IngestedCalculateAmounts(Pawn ingester, Pawn victim, float nutritionWanted, out int numTaken, out float nutritionIngested, out BodyPartDef bodyPartDef)
        {
            BodyPartRecord bodyPartRecord = GetBestBodyPartToEat(victim, nutritionWanted);
            if (bodyPartRecord == null)
            {
                Log.Error(string.Concat(ingester, " ate ", victim, " but no body part was found. Replacing with core part."));
                bodyPartRecord = victim.RaceProps.body.corePart;
            }
            bodyPartDef = bodyPartRecord.def;
            float bodyPartNutrition = GetBodyPartNutrition(victim, bodyPartRecord);

            int bloodAmount = Mathf.Max(GenMath.RoundRandom(victim.BodySize * 12f), 1);
            for (int i = 0; i < bloodAmount; i++)
            {
                victim.health.DropBloodFilth();
            }

            if (bodyPartRecord == victim.RaceProps.body.corePart)
            {
                
                if (PawnUtility.ShouldSendNotificationAbout(victim) && victim.RaceProps.Humanlike)
                {
                    Messages.Message("MessageEatenByPredator".Translate(victim.LabelShort, ingester.Named("PREDATOR"), victim.Named("EATEN")).CapitalizeFirst(), ingester, MessageTypeDefOf.NegativeEvent);
                }
                
                int damageAmount = Mathf.Clamp((int)victim.health.hediffSet.GetPartHealth(bodyPartRecord) - 1, 1, 20);
                if (ModsConfig.BiotechActive && victim.genes != null && victim.genes.HasGene(GeneDefOf.Deathless))
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
                if (!victim.Dead)
                {
                    victim.Kill(damageInfo);
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

        private static BodyPartRecord GetBestBodyPartToEat(Pawn victim, float nutritionWanted)
        {
            IEnumerable<BodyPartRecord> source = from x in victim.health.hediffSet.GetNotMissingParts()
                                                 where x.depth == BodyPartDepth.Outside && GetBodyPartNutrition(victim, x) > 0.001f
                                                 select x;
            if (!source.Any())
            {
                return null;
            }
            return source.MinBy((BodyPartRecord x) => Mathf.Abs(GetBodyPartNutrition(victim, x) - nutritionWanted));
        }

        private static float GetBodyPartNutrition(Pawn victim, BodyPartRecord part)
        {
            float totalNutrition = 5.2f;     //base value for corpses as observed in game

            totalNutrition *= victim.BodySize;

            HediffSet hediffSet = victim.health.hediffSet;
            float coverageOfNotMissingNaturalParts = hediffSet.GetCoverageOfNotMissingNaturalParts(victim.RaceProps.body.corePart);
            if (coverageOfNotMissingNaturalParts <= 0f)
            {
                return 0f;
            }
            
            float partCoverage = hediffSet.GetCoverageOfNotMissingNaturalParts(part);
            return totalNutrition * partCoverage;
        }
    }
}
