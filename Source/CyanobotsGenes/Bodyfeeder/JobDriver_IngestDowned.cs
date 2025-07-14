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
using static CyanobotsGenes.BodyfeederUtility;

namespace CyanobotsGenes
{
    class JobDriver_IngestDowned : JobDriver
    {
        private const int GOODWILL_OFFSET_BODYPARTEATEN = -40;
        private const int BASE_INGEST_TICKS = 500;
        private const int TICKS_BETWEEN_GOODWILL_CHANGES = 18000;
        private const float SNAP_OUT_CHANCE = 0.2f;

        public enum PawnGroup
        {
            Enemy = 0,
            Outsider = 1,
            Colonist = 2
        };

        public enum CannibalType
        {
            NonCannibal,
            Acceptable,
            Required
        };

        private static Dictionary<Faction, int> goodwillLastChangedTicks = new Dictionary<Faction, int>(); 

        private Pawn Victim => job.GetTarget(TargetIndex.A).Pawn;
        private float ChewDurationMultiplier => 1f / pawn.GetStatValue(StatDefOf.EatingSpeed);

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Victim, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => Victim.DestroyedOrNull() || Victim.Dead || !Victim.Downed || !(pawn.MentalState is MentalState_BodyfeederBerserk));
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return ChewToil(pawn, Victim, ChewDurationMultiplier, TargetIndex.A);
            yield return FinalizeIngest(pawn, Victim, TargetIndex.A);
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
                        Mathf.RoundToInt((float)BASE_INGEST_TICKS * durationMultiplier);
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
                    / Mathf.Round((float)BASE_INGEST_TICKS * durationMultiplier));
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

                float nutritionWanted = BodyfeederUtility.BodyfeederNutritionWanted(ingester, victim);    
                float hpn = HemogenPerNutrition(ingester, victim);       //gotta calculate this before they potentially die
                
                IngestedCalculateAmounts(ingester, victim, nutritionWanted, out int numTaken, out float nutritionIngested, out BodyPartDef bodyPartDef);

                //can't directly notify genes and hediffs because victim is not an ingestible,
                //but don't think any vanilla genes or hediffs need notifying

                ingester.needs.food.CurLevel += nutritionIngested;
                ingester.records.AddTo(RecordDefOf.NutritionEaten, nutritionIngested);

                ingester.genes.GetFirstGeneOfType<Gene_Bodyfeeder>().Notify_IngestedLivePawn(nutritionIngested);

                if (numTaken != 0) 
                {
                    ThoughtsHistoryAndTale(ingester, victim, true);

                    if (ingester.IsCarryingPawn(victim))
                    {
                        //Log.Message("ingester.IsCarryingPawn, attempting DestroyCarriedThing");
                        ingester.carryTracker.DestroyCarriedThing();
                    }
                    else if (!victim.Dead)
                    {
                        //Log.Message("victim not dead, attempting victim.Destroy()");
                        victim.Destroy();
                    }
                    else if (!victim.Corpse.DestroyedOrNull())
                    {
                        //Log.Message("found corpse, attempting victim.Corpse.Destroy()");
                        victim.Corpse.Destroy();
                    }
                    else
                    {
                        //Log.Message("Found neither victim nor corpse");
                    }

                }
                else
                {
                    ThoughtsHistoryAndTale(ingester, victim, victim.Dead, bodyPartDef);
                }


                bool chance = Rand.Chance(SNAP_OUT_CHANCE);
                //Log.Message("chance: " + chance);
                if (chance)
                {
                    ingester.jobs.EndCurrentJob(JobCondition.Succeeded);
                    ingester.MentalState.RecoverFromState();
                }

            };

            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }

        private static void ThoughtsHistoryAndTale(Pawn bodyfeeder, Pawn victim, bool death, BodyPartDef bodyPartDef = null)
        {
            //execution event
            //only relevant if bodyfeeder of player faction
            //and victim is colonist, guest or prisoner
            if (death && bodyfeeder.Faction == Faction.OfPlayer
                    && (victim.Faction == Faction.OfPlayer || victim.IsPrisonerOfColony || victim.HostFaction != null))
            {
                    ThoughtUtility.GiveThoughtsForPawnExecuted(victim, bodyfeeder, PawnExecutionKind.GenericBrutal);
            }

            //cannibalism events
            Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.AteHumanMeat, bodyfeeder.Named(HistoryEventArgsNames.Doer)));
            Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.AteHumanMeatDirect, bodyfeeder.Named(HistoryEventArgsNames.Doer)));

            BodyfeederSpecificThoughts(bodyfeeder, victim, death, bodyPartDef);

            //general ingester thoughts
            if (bodyfeeder.needs.mood != null)
            {
                MemoryThoughtHandler memories = bodyfeeder.needs.mood.thoughts.memories;

                //based off Toils_Ingest.FinalizeIngest
                //table and dining room
                if (!(bodyfeeder.Position + bodyfeeder.Rotation.FacingCell).HasEatSurface(bodyfeeder.Map) && !bodyfeeder.IsWildMan())
                {
                    memories.TryGainMemory(ThoughtDefOf.AteWithoutTable);
                }
                Room room = bodyfeeder.GetRoom();
                if (room != null)
                {
                    int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness));
                    if (ThoughtDefOf.AteInImpressiveDiningRoom.stages[scoreStageIndex] != null)
                    {
                        memories.TryGainMemory(ThoughtMaker.MakeThought(ThoughtDefOf.AteInImpressiveDiningRoom, scoreStageIndex));
                    }
                }

                //based off FoodUtility.ThoughtsFromIngesting
                //tell traits we've been eating humanlike meat
                List<ThoughtDef> thoughtsFromTraits = new List<ThoughtDef>();
                bodyfeeder.story?.traits?.GetExtraThoughtsFromIngestion(thoughtsFromTraits, victim.RaceProps.meatDef, MeatSourceCategory.Humanlike, true);
                foreach (ThoughtDef thought in thoughtsFromTraits)
                {
                    memories.TryGainMemory(thought);
                }

                bodyfeeder.mindState.lastHumanMeatIngestedTick = Find.TickManager.TicksGame;

                //do we need AddThoughtsFromIdeo? or will event handle this?
                //based off FoodUtility.AddThoughtsFromIdeo
                //cannibalism, etc
                /*
                if (bodyfeeder.Ideo != null)
                {
                    List<Precept> preceptsListForReading = bodyfeeder.Ideo.PreceptsListForReading;
                    foreach (Precept precept in preceptsListForReading)
                    {
                        List<PreceptComp> comps = precept.def.comps;
                        foreach (PreceptComp comp in comps)
                        {
                            if (comp is PreceptComp_SelfTookMemoryThought preceptComp_SelfTookMemoryThought
                                && preceptComp_SelfTookMemoryThought.eventDef == HistoryEventDefOf.AteHumanMeatDirect)
                            {
                                //based off FoodUtility.TryAddIngestThought
                                ThoughtDef thoughtDef = preceptComp_SelfTookMemoryThought.thought;
                                if (ThoughtUtility.NullifyingGene(thoughtDef, bodyfeeder) != null
                                    || ThoughtUtility.NullifyingHediff(thoughtDef, bodyfeeder) != null
                                    || ThoughtUtility.NullifyingPrecept(thoughtDef, bodyfeeder) != null
                                    || (precept.def.enabledForNPCFactions && !bodyfeeder.CountsAsNonNPCForPrecepts()))
                                {
                                    continue;
                                }

                                if (bodyfeeder.story != null && bodyfeeder.story.traits != null)
                                {
                                    if (bodyfeeder.story.traits.IsThoughtFromIngestionDisallowed(thoughtDef,
                                        victim.RaceProps.corpseDef, MeatSourceCategory.Humanlike))
                                    {
                                        continue;
                                    }
                                }

                                bodyfeeder.needs.mood.thoughts.memories.TryGainMemory(thoughtDef, victim, precept);
                            }
                        }
                    }
                }
                
                */
            }
        }

        /*
        private static void Goodwill(Faction faction)
        {
            if (faction == Faction.OfPlayer || faction.HostileTo(Faction.OfPlayer)) return;

            if (!goodwillLastChangedTicks.ContainsKey(faction) || 
                Find.TickManager.TicksAbs - goodwillLastChangedTicks[faction] < TICKS_BETWEEN_GOODWILL_CHANGES)
            {
                Faction.OfPlayer.TryAffectGoodwillWith(faction, GOODWILL_OFFSET_BODYPARTEATEN, true, !faction.temporary, CG_DefOf.BodyfeederAteBodyPartOutsider);
                goodwillLastChangedTicks[faction]
            }
        }
        */

        private static void BodyfeederSpecificThoughts(Pawn bodyfeeder, Pawn victim, bool death, BodyPartDef bodyPartDef = null)
        {
            //Log.Message("BodyfeederSpecificThoughts - bodyfeeder: " + bodyfeeder + ", victim: " + victim
            //    + ", death: " + death + ", bodyPartDef: " + bodyPartDef);
            PawnGroup pawnGroup = PawnGroup.Outsider;                                                   //1 for misc non-hostile strangers
            if (victim.Faction == bodyfeeder.Faction) pawnGroup = PawnGroup.Colonist;                   //2 for same faction (ie fellow colonists)
            else if (victim.Faction.HostileTo(bodyfeeder.Faction)) pawnGroup = PawnGroup.Enemy;         //0 for enemies

            //cannibals feel less bad about eating people alive
            CannibalType cannibalType = CannibalType.NonCannibal;           
            if (bodyfeeder.story.traits != null && bodyfeeder.story.traits.HasTrait(DefDatabase<TraitDef>.GetNamed("Cannibal")))
            {
                cannibalType = CannibalType.Required;
            }
            else if (ModsConfig.IdeologyActive && bodyfeeder.ideo != null
                && (bodyfeeder.ideo.Ideo.HasPrecept(PreceptDefOf.Cannibalism_Preferred)
                || bodyfeeder.ideo.Ideo.HasPrecept(DefDatabase<PreceptDef>.GetNamed("Cannibalism_Acceptable"))))
            {
                cannibalType = CannibalType.Acceptable;
            }
            else if (ModsConfig.IdeologyActive && bodyfeeder.ideo != null
                && (bodyfeeder.ideo.Ideo.HasPrecept(PreceptDefOf.Cannibalism_RequiredStrong)
                || bodyfeeder.ideo.Ideo.HasPrecept(PreceptDefOf.Cannibalism_RequiredRavenous)))
            {
                cannibalType = CannibalType.Required;
            }

            //Log.Message("pawnGroup: " + pawnGroup + ", cannibalType: " + cannibalType);

            //if the victim was killed directly
            if (death)
            {
                //Log.Message("death");

                TaleRecorder.RecordTale(CG_DefOf.TaleBodyfeederAtePerson, new object[] { bodyfeeder, victim });

                HistoryEvent ev;
                //event handles thoughts for witnesses
                if (pawnGroup == PawnGroup.Colonist)
                {
                    ev = new HistoryEvent(CG_DefOf.BodyfeederAteColonist,
                        bodyfeeder.Named(HistoryEventArgsNames.Doer), victim.Named(HistoryEventArgsNames.Victim));
                }
                else if (pawnGroup == PawnGroup.Enemy)
                {
                    ev = new HistoryEvent(CG_DefOf.BodyfeederAteEnemy,
                        bodyfeeder.Named(HistoryEventArgsNames.Doer), victim.Named(HistoryEventArgsNames.Victim));
                }
                else
                {
                    ev = new HistoryEvent(CG_DefOf.BodyfeederAteOutsider,
                        bodyfeeder.Named(HistoryEventArgsNames.Doer), victim.Named(HistoryEventArgsNames.Victim));
                }
                Find.HistoryEventsManager.RecordEvent(ev);

                //thoughts for the perpetrator handled separately
                //since we can't pass a stage id via the event for them
                if (bodyfeeder.needs.mood != null)
                {
                    MemoryThoughtHandler memoryHandler = bodyfeeder.needs.mood.thoughts.memories;

                    //specific relationships with victim (beyond just colonist/non-colonist/hostile)
                    if (bodyfeeder.relations != null)
                    {
                        int stage_relation = -1;
                        PawnRelationDef relationDef = bodyfeeder.GetMostImportantRelation(victim);
                        if (relationDef == PawnRelationDefOf.Child)
                        {
                            stage_relation = 3;
                        }
                        else if (relationDef == PawnRelationDefOf.Parent 
                            || relationDef == PawnRelationDefOf.ParentBirth)
                        { 
                            stage_relation = 2; 
                        }
                        else if (relationDef == PawnRelationDefOf.Spouse 
                            || relationDef == PawnRelationDefOf.Lover
                            || relationDef == PawnRelationDefOf.Fiance)
                        {
                            stage_relation = 1;
                        }
                        else if (relationDef == PawnRelationDefOf.Sibling
                            || relationDef == PawnRelationDefOf.HalfSibling)
                        {
                            stage_relation = 4;
                        }
                        else if (bodyfeeder.relations.OpinionOf(victim) >= Pawn_RelationsTracker.FriendOpinionThreshold)
                        {
                            stage_relation = 0;
                        }
                        else if (relationDef == PawnRelationDefOf.Cousin
                            || relationDef == PawnRelationDefOf.Grandchild
                            || relationDef == PawnRelationDefOf.Grandparent
                            || relationDef == PawnRelationDefOf.GranduncleOrGrandaunt
                            || relationDef == PawnRelationDefOf.GreatGrandchild
                            || relationDef == PawnRelationDefOf.GreatGrandparent
                            || relationDef == PawnRelationDefOf.Kin
                            || relationDef == PawnRelationDefOf.NephewOrNiece
                            || relationDef == PawnRelationDefOf.UncleOrAunt
                            )
                        {
                            stage_relation = 5;
                        }
                        if (stage_relation != -1)
                        {
                            //Log.Message("stage_relation: " + stage_relation);
                            Thought_Memory memory_atefriend = ThoughtMaker.MakeThought(CG_DefOf.Bodyfeeder_AteFriend, stage_relation);
                            memoryHandler.TryGainMemory(memory_atefriend, victim);
                            //for some reason PawnWithGoodOpinionDied doesn't seem to get applied anywhere, so let's check for it and add it if hasn't already
                            if (stage_relation == 0 && !memoryHandler.Memories.Any<Thought_Memory>(m => m.def == ThoughtDefOf.PawnWithGoodOpinionDied && m.otherPawn == victim))
                            {
                                Thought_Memory memory_friendDied = ThoughtMaker.MakeThought(ThoughtDefOf.PawnWithGoodOpinionDied,0);
                                memoryHandler.TryGainMemory(memory_friendDied, victim);
                            }
                            return;
                        }
                    }

                    //if no specific relationship, thought based on enemy/colonist/other
                    //Log.Message("no specific relationship");
                    Thought_Memory memory_ateperson;
                    if (cannibalType == CannibalType.Required)
                    {
                        memory_ateperson = ThoughtMaker.MakeThought(CG_DefOf.Bodyfeeder_AteLivePerson_CRequired, (int)pawnGroup);
                    }
                    else if (cannibalType == CannibalType.Acceptable)
                    {
                        memory_ateperson = ThoughtMaker.MakeThought(CG_DefOf.Bodyfeeder_AteLivePerson_CAcceptable, (int)pawnGroup);
                    }
                    else
                    {
                        memory_ateperson = ThoughtMaker.MakeThought(CG_DefOf.Bodyfeeder_AteLivePerson, (int)pawnGroup);
                    }
                    //Log.Message("memory_ateperson: " + memory_ateperson);
                    bodyfeeder.needs.mood.thoughts.memories.TryGainMemory(memory_ateperson, victim);

                    //for some reason KnowColonistDied doesn't seem to get applied anywhere, so let's check for it and add it if hasn't already
                    if (pawnGroup == PawnGroup.Colonist && !memoryHandler.Memories.Any<Thought_Memory>(m => m.def == ThoughtDefOf.KnowColonistDied && m.age < 60))
                    {
                        Thought_Memory memory_ColonistDied = ThoughtMaker.MakeThought(ThoughtDefOf.KnowColonistDied, 0);
                        //Log.Message("memory_ColonistDied: " + memory_ColonistDied);
                        memoryHandler.TryGainMemory(memory_ColonistDied);
                    }
                }
            }

            //if the victim was not directly killed
            else
            {
                //Log.Message("not death");

                TaleRecorder.RecordTale(CG_DefOf.TaleBodyfeederAteBodyPart, new object[] { bodyfeeder, victim, bodyPartDef });

                HistoryEvent ev;
                //event handles thoughts for witnesses
                if (pawnGroup == PawnGroup.Colonist)
                {
                    ev = new HistoryEvent(CG_DefOf.BodyfeederAteBodyPartColonist,
                        bodyfeeder.Named(HistoryEventArgsNames.Doer), victim.Named(HistoryEventArgsNames.Victim));
                }
                else if (pawnGroup == PawnGroup.Enemy)
                {
                    ev = new HistoryEvent(CG_DefOf.BodyfeederAteBodyPartEnemy,
                        bodyfeeder.Named(HistoryEventArgsNames.Doer), victim.Named(HistoryEventArgsNames.Victim));
                }
                else
                {
                    ev = new HistoryEvent(CG_DefOf.BodyfeederAteBodyPartOutsider,
                        bodyfeeder.Named(HistoryEventArgsNames.Doer), victim.Named(HistoryEventArgsNames.Victim));
                }
                Find.HistoryEventsManager.RecordEvent(ev);

                //thoughts for the perpetrator handled separately
                //since we can't pass a stage id via the event for them
                if (bodyfeeder.needs.mood != null)
                {
                    //if that was a friend
                    if (bodyfeeder.relations != null && bodyfeeder.relations.OpinionOf(victim) >= Pawn_RelationsTracker.FriendOpinionThreshold)
                    {
                        Thought_Memory memory_atebodypartfriend = ThoughtMaker.MakeThought(CG_DefOf.Bodyfeeder_AteBodyPartFriend, 0);
                        bodyfeeder.needs.mood.thoughts.memories.TryGainMemory(memory_atebodypartfriend, victim);
                    }


                    //if not specifically a friend, thought based on enemy/colonist/other
                    Thought_Memory memory_atebodypart;
                    if (cannibalType == CannibalType.Required)
                    {
                        memory_atebodypart = ThoughtMaker.MakeThought(CG_DefOf.Bodyfeeder_AteBodyPart_CRequired, (int)pawnGroup);
                    }
                    else if (cannibalType == CannibalType.Acceptable)
                    {
                        memory_atebodypart = ThoughtMaker.MakeThought(CG_DefOf.Bodyfeeder_AteBodyPart_CAcceptable, (int)pawnGroup);
                    }
                    else
                    {
                        memory_atebodypart = ThoughtMaker.MakeThought(CG_DefOf.Bodyfeeder_AteBodyPart, (int)pawnGroup);
                    }
                    bodyfeeder.needs.mood.thoughts.memories.TryGainMemory(memory_atebodypart, victim);

                }
                //thoughts for the victim
                if (victim.needs.mood != null)
                {
                    //really devoted cannibals feel less bad about being eaten
                    bool victimCannibal = (ModsConfig.IdeologyActive && victim.ideo != null
                            && victim.ideo.Ideo.HasPrecept(PreceptDefOf.Cannibalism_RequiredStrong)
                            || victim.story.traits.HasTrait(DefDatabase<TraitDef>.GetNamed("Cannibal")));
                    //(ravenous don't care at all)

                    Thought_Memory memory_bodyparteaten;
                    Thought_Memory memory_bodyparteaten_social;
                    if (victimCannibal)
                    {
                        memory_bodyparteaten = ThoughtMaker.MakeThought(CG_DefOf.BodyfeederVictim_BodyPartEaten_Mood_CRequiredStrong, 0);
                        memory_bodyparteaten_social = ThoughtMaker.MakeThought(CG_DefOf.BodyfeederVictim_BodyPartEaten_Opinion_CRequiredStrong, 0);
                    }
                    else
                    {
                        memory_bodyparteaten = ThoughtMaker.MakeThought(CG_DefOf.BodyfeederVictim_BodyPartEaten_Mood, 0);
                        memory_bodyparteaten_social = ThoughtMaker.MakeThought(CG_DefOf.BodyfeederVictim_BodyPartEaten_Opinion, 0);
                    }
                    victim.needs.mood.thoughts.memories.TryGainMemory(memory_bodyparteaten, bodyfeeder);
                    victim.needs.mood.thoughts.memories.TryGainMemory(memory_bodyparteaten_social, bodyfeeder);
                }
            }
        }
    }
}
