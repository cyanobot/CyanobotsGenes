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
    class JobDriver_IngestForHemogen : JobDriver_Ingest
    {
		private bool UsingNutrientPasteDispenser => (bool)typeof(JobDriver_Ingest).GetField("usingNutrientPasteDispenser",BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);

		private const TargetIndex TableCellInd = TargetIndex.B;
		private Thing IngestibleSource => job.GetTarget(IngestibleSourceInd).Thing;

		private float ChewDurMult => (float)typeof(JobDriver_Ingest).GetProperty("ChewDurationMultiplier", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
	
		protected override IEnumerable<Toil> MakeNewToils()
		{
			//Log.Message("MakeNewToils - job: " + job + ", count: " + job.count + ", targetA: " + TargetA);
			//Log.Message("fired MakeNewToils, IngestibleSource: " + IngestibleSource + ", IngestibleNow: " + IngestibleSource.IngestibleNow 
			//	+ ", CorpseIngestibleForHemogenNow: " + CorpseIngestibleForHemogenNow(IngestibleSource as Corpse));
			if (!UsingNutrientPasteDispenser)
			{
				this.FailOn(() => 
					!IngestibleSource.Destroyed 
					&& !(IngestibleSource.IngestibleNow || CorpseIngestibleForHemogenNow(IngestibleSource as Corpse)));
			}

			float hemogenThreshold = 0.9f;
			if (!this.job.playerForced) 
				SetFinalizerJob((JobCondition condition) =>
					condition == JobCondition.Succeeded && HemogenLevelPct(pawn) < hemogenThreshold 
					? MakeIngestForHemogenJob(pawn)
					: null
					);

			Toil chew = Chew();

			//Copied from JobDriver_Ingest.PrepareToIngestToils but not caring about forbidden
			if (UsingNutrientPasteDispenser)
			{
				//Log.Message("UsingNutrientPasteDispenser");
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDespawnedOrNull(TargetIndex.A);
				yield return Toils_Ingest.TakeMealFromDispenser(TargetIndex.A, pawn);
				yield return Toils_Ingest.CarryIngestibleToChewSpot(pawn, TargetIndex.A).FailOnDestroyedOrNull(TargetIndex.A);
				yield return Toils_Ingest.FindAdjacentEatSurface(TargetIndex.B, TargetIndex.A);
			}
			else if (EatingFromInventory)
			{
				//Log.Message("EatingFromInventory");
				yield return Toils_Misc.TakeItemFromInventoryToCarrier(pawn, TargetIndex.A);
			}
			else
			{
				//Log.Message("Need to pick food up");
				IEnumerable<Toil> acquireToils = AcquireIngestibleToils(chew);
				foreach (Toil toil in acquireToils) yield return toil;
				//Log.Message("Finished creating pick food up toils");
			}

			if (!pawn.Drafted && BodyfeederUtility.GetDesperation(pawn) < BodyfeederUtility.Desperation.StarvationMild)
			{
				//Log.Message("neither drafted nor mod+ starving");
				yield return Toils_Ingest.CarryIngestibleToChewSpot(pawn, TargetIndex.A).FailOnDestroyedOrNull(TargetIndex.A);
			}
			yield return Toils_Ingest.FindAdjacentEatSurface(TargetIndex.B, TargetIndex.A);
			//yield return AdjustCarriedStack();
			yield return chew;
			yield return FinalizeIngest();
			yield return Toils_Jump.JumpIf(chew, () => 
				(job.GetTarget(TargetIndex.A).Thing is Corpse && BodyfeederUtility.HemogenLevelPct(pawn) < 0.9f)
				|| (!job.GetTarget(TargetIndex.A).Thing.DestroyedOrNull() 
					&& job.count > 0));
			//Log.Message("Finished creating toils");
		}

		public IEnumerable<Toil> AcquireIngestibleToils(Toil chewToil)
        {
			//Based on JobDriver_Ingest.PrepareToIngestToils
			if (UsingNutrientPasteDispenser)
			{
				//Log.Message("UsingNutrientPasteDispenser");
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDespawnedOrNull(TargetIndex.A);
				yield return Toils_Ingest.TakeMealFromDispenser(TargetIndex.A, pawn);
				yield return Toils_Ingest.CarryIngestibleToChewSpot(pawn, TargetIndex.A).FailOnDestroyedOrNull(TargetIndex.A);
				yield return Toils_Ingest.FindAdjacentEatSurface(TargetIndex.B, TargetIndex.A);
			}
			else if (EatingFromInventory)
			{
				//Log.Message("EatingFromInventory");
				yield return Toils_Misc.TakeItemFromInventoryToCarrier(pawn, TargetIndex.A);
			}
			else
			{
				//Log.Message("Need to pick food up");

				Toil reserveIngestible = ReserveIngestible();
				yield return reserveIngestible;

				//if we can manipulate, pickup toils
				Toil gotoToPickup = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedOrNull(TargetIndex.A);
				yield return Toils_Jump.JumpIf(gotoToPickup, () => pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation));
				
				//if we cannot manipulate, just go to thing and chew it in place
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
				yield return Toils_Jump.Jump(chewToil);
				
				yield return gotoToPickup;
				yield return PickupIngestible();
				yield return CheckForDuplicateToPickup(reserveIngestible,GetDesperation(pawn)> Desperation.StarvationMild);

				//Log.Message("Finished creating pick food up toils");
			}
		}

		//based on JobDriver_Ingest.ReserveFood & Toils_Ingest.ReserveFoodFromStackForIngest
		public Toil ReserveIngestible()
        {
			Toil toil = ToilMaker.MakeToil("ReserveIngestible");
			toil.initAction = delegate
			{ 
				int amountToPickup = -1;
				Thing thing = job.GetTarget(TargetIndex.A).Thing;
				Thing carriedThing = pawn.carryTracker.CarriedThing;
				if (carriedThing != thing)
                {
					if (carriedThing?.def == thing.def)
                    {
						amountToPickup = Math.Min(thing.stackCount, job.count - carriedThing.stackCount);
                    }
					else amountToPickup = Math.Min(thing.stackCount, job.count);
					if (!pawn.Reserve(thing, job, 1, amountToPickup))
                    {
						Log.Error(string.Concat("Pawn hemogen source reservation for ", pawn, " on job ", this, " failed, because it could not reserve from ", thing, " - amount: ", amountToPickup));
						pawn.jobs.EndCurrentJob(JobCondition.Errored);
					}
                }
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			toil.atomicWithPrevious = true;
			return toil;
        }

		//based on Toils_Ingest.PickupIngestible
		public Toil PickupIngestible()
        {
			Toil toil = ToilMaker.MakeToil("PickupIngestible");
			toil.initAction = delegate
			{
				if (job.count <= 0)
				{
					Log.Error("Tried to do PickupIngestible toil with job.count = " +job.count);
					pawn.jobs.EndCurrentJob(JobCondition.Errored);
					return;
				}

				Thing thing = job.GetTarget(TargetIndex.A).Thing;
				Thing carriedThing = pawn.carryTracker.CarriedThing;
				int amountToPickup = 0;
				if (carriedThing != thing)
				{
					if (carriedThing?.def == thing.def)
					{
						amountToPickup = Math.Min(thing.stackCount, job.count - carriedThing.stackCount);
					}
					else amountToPickup = Math.Min(thing.stackCount, job.count);
					//Log.Message("PickupIngestible - job.count: " + job.count + ", amountToPickup: " + amountToPickup);

					pawn.carryTracker.TryStartCarry(thing, amountToPickup);
					if(pawn.Map.reservationManager.ReservedBy(thing, pawn, job))
                    {
						pawn.Map.reservationManager.Release(thing, pawn, job);
					}
				}
				
				if (thing != pawn.carryTracker.CarriedThing && pawn.Map.reservationManager.ReservedBy(thing, pawn, job))
				{
					pawn.Map.reservationManager.Release(thing, pawn, job);
				}
				job.targetA = pawn.carryTracker.CarriedThing;
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
        }

		//based on Toils_Haul.CheckForGetOpportunityDuplicate
		public Toil CheckForDuplicateToPickup(Toil reserveIngestibleToil, bool allowForbidden)
        {
			Toil toil = ToilMaker.MakeToil("CheckForDuplicateToPickup");
			toil.initAction = delegate
			{
				//Log.Message("CheckForDuplicateToPickup.initAction - carried.stackLimit: " + pawn.carryTracker.CarriedThing.def.stackLimit
				//	+ ", Full: " + pawn.carryTracker.Full
				//	+ ", carried.stackCount: " + pawn.carryTracker.CarriedThing.stackCount
				//	+ ", job.count: " + job.count);
				if (pawn.carryTracker.CarriedThing.def.stackLimit != 1 && !pawn.carryTracker.Full
					&& pawn.carryTracker.CarriedThing.stackCount < job.count)
				{
					Thing thing = null;
					Predicate<Thing> validator = delegate (Thing t)
					{
						if (!t.Spawned)
						{
							return false;
						}
						if (!t.CanStackWith(pawn.carryTracker.CarriedThing))
						{
							return false;
						}
						if (!allowForbidden && t.IsForbidden(pawn))
						{
							return false;
						}
						if (!t.IngestibleNow)
						{
							return false;
						}
						if (!pawn.CanReserve(t))
						{
							return false;
						}
						return true;
					};
					thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(pawn.carryTracker.CarriedThing.def), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 8f, validator);
					if (thing != null)
					{
						job.SetTarget(TargetIndex.A, thing);
						pawn.jobs.curDriver.JumpToToil(reserveIngestibleToil);
					}
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
        }

		/*
		public Toil AdjustCarriedStack()
        {
			Toil toil = ToilMaker.MakeToil("AdjustCarriedStack");
			toil.initAction = delegate
			{
				Thing thing = TargetThingA;
				int numToIngestAtOnce = Math.Min(Math.Min(job.count, thing.def.ingestible.maxNumToIngestAtOnce), thing.stackCount);
				if (numToIngestAtOnce < thing.stackCount)
                {
					pawn.inventory.GetDirectlyHeldThings().TryAdd(thing.SplitOff(thing.stackCount - numToIngestAtOnce));
                }
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
        }
		*/

		public Toil Chew()
        {
			Toil toil;
			if (IsRottingCorpse(TargetA.Thing))
			{
				toil = ChewRotting(pawn, ChewDurMult, IngestibleSourceInd, TableCellInd);
			}
			else
			{
				toil = Toils_Ingest.ChewIngestible(pawn, ChewDurMult, IngestibleSourceInd, TableCellInd);
			}
			toil.FailOn((Toil x) => 
				!IngestibleSource.Spawned 
				&& (pawn.carryTracker == null || pawn.carryTracker.CarriedThing != IngestibleSource));
			toil.FailOnCannotTouch(IngestibleSourceInd, PathEndMode.Touch);

			return toil;
		}

		public static Toil ChewRotting(Pawn chewer, float durationMultiplier, TargetIndex ingestibleInd, TargetIndex eatSurfaceInd = TargetIndex.None)
		{
			//Log.Message("Creating ChewRotting");
			Toil toil = ToilMaker.MakeToil("ChewRotting");
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Thing thing = actor.CurJob.GetTarget(ingestibleInd).Thing;
				if (!CorpseIngestibleForHemogenNow((Corpse)thing))
				{
					//Log.Message("!CorpseIngestibleForHemogenNow");
					chewer.jobs.EndCurrentJob(JobCondition.Incompletable);
				}
				else
				{
					toil.actor.pather.StopDead();
					actor.jobs.curDriver.ticksLeftThisToil = Mathf.RoundToInt((float)thing.def.ingestible.baseIngestTicks * durationMultiplier);
					if (thing.Spawned)
					{
						thing.Map.physicalInteractionReservationManager.Reserve(chewer, actor.CurJob, thing);
					}
				}
			};
			toil.tickAction = delegate
			{
				if (chewer != toil.actor)
				{
					toil.actor.rotationTracker.FaceCell(chewer.Position);
				}
				else
				{
					Thing thing_inner = toil.actor.CurJob.GetTarget(ingestibleInd).Thing;
					if (thing_inner != null && thing_inner.Spawned)
					{
						toil.actor.rotationTracker.FaceCell(thing_inner.Position);
					}
					else if (eatSurfaceInd != 0 && toil.actor.CurJob.GetTarget(eatSurfaceInd).IsValid)
					{
						toil.actor.rotationTracker.FaceCell(toil.actor.CurJob.GetTarget(eatSurfaceInd).Cell);
					}
				}
				toil.actor.GainComfortFromCellIfPossible();
			};
			toil.WithProgressBar(ingestibleInd, delegate
			{
				Thing thing_inner = toil.actor.CurJob.GetTarget(ingestibleInd).Thing;
				return (thing_inner == null) ? 1f : (1f - (float)toil.actor.jobs.curDriver.ticksLeftThisToil / Mathf.Round((float)thing_inner.def.ingestible.baseIngestTicks * durationMultiplier));
			});
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.FailOnDestroyedOrNull(ingestibleInd);
			toil.AddFinishAction(delegate
			{
				if (chewer != null && chewer.CurJob != null)
				{
					Thing thing = chewer.CurJob.GetTarget(ingestibleInd).Thing;
					if (thing != null && chewer.Map.physicalInteractionReservationManager.IsReservedBy(chewer, thing))
					{
						chewer.Map.physicalInteractionReservationManager.Release(chewer, toil.actor.CurJob, thing);
					}
				}
			});
			toil.handlingFacing = true;
			toil.WithEffect(EffecterDefOf.EatMeat, ingestibleInd);
			toil.PlaySustainerOrSound(SoundDefOf.RawMeat_Eat);
			return toil;

		}

		public Toil FinalizeIngest()
		{
			Toil toil = ToilMaker.MakeToil("FinalizeIngest");
			toil.initAction = delegate
			{
				//Log.Message("FinalizeIngest.initAction");
				//Pawn actor = toil.actor;
				//Job curJob = actor.jobs.curJob;
				Thing thing = job.GetTarget(TargetIndex.A).Thing;
				if (pawn.needs.mood != null && thing.def.IsNutritionGivingIngestible && thing.def.ingestible.chairSearchRadius > 10f)
				{
					//Log.Message("Attempting to gen room thoughts");
					if (!(pawn.Position + pawn.Rotation.FacingCell).HasEatSurface(pawn.Map) && pawn.GetPosture() == PawnPosture.Standing && !pawn.IsWildMan() && thing.def.ingestible.tableDesired)
					{
						pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.AteWithoutTable);
					}
					Room room = pawn.GetRoom();
					if (room != null)
					{
						int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness));
						if (ThoughtDefOf.AteInImpressiveDiningRoom.stages[scoreStageIndex] != null)
						{
							pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(ThoughtDefOf.AteInImpressiveDiningRoom, scoreStageIndex));
						}
					}
				}
				int numToIngest = 0;
				if (IsRottingCorpse(thing)) 
				{
					RottingCorpseIngested(pawn, (Corpse)thing);
					//Log.Message("eating rotting corpse, nutritionGained: " + nutritionGained);
				}
                else
				{
					float effectiveNutritionWanted;
					ThingDef def = thing.def;
					if (thing is Corpse)
                    {
						effectiveNutritionWanted = BodyfeederNutritionWanted(pawn, thing);
                    }
                    else
                    {
						int maxToIngest = thing.def.ingestible.maxNumToIngestAtOnce > 0 ? thing.def.ingestible.maxNumToIngestAtOnce : thing.def.ingestible.defaultNumToIngestAtOnce;
						numToIngest = Math.Min(thing.stackCount,Math.Min(maxToIngest, job.count));
						effectiveNutritionWanted = thing.GetStatValue(StatDefOf.Nutrition) * numToIngest;
						//Log.Message("job.count: " + job.count + ", maxToIngest: " + maxToIngest
						//	+ ", numToIngest: " + numToIngest + ", effectiveNutritionWanted: " + effectiveNutritionWanted);
					}

					float nutritionGained = thing.Ingested(pawn, effectiveNutritionWanted);
					//Log.Message("effectiveNutritionWanted: " + effectiveNutritionWanted + ", nutritionGained: " + nutritionGained);
					if (!pawn.Dead)
					{
						pawn.needs.food.CurLevel += nutritionGained;
					}
					pawn.records.AddTo(RecordDefOf.NutritionEaten, nutritionGained);
					job.count -= numToIngest;
					/*
					if (!(thing is Corpse))
                    {
						job.count -= stackCount;
						int inventoryCount = pawn.inventory.Count(def);
						if (job.count > 0 && inventoryCount > 0)
                        {
							int numToCarry = Math.Min(inventoryCount, Math.Min(job.count, def.ingestible.maxNumToIngestAtOnce));
                            while (pawn.carryTracker.CarriedThing == null 
								|| (pawn.carryTracker.CarriedThing.def == def && pawn.carryTracker.CarriedThing.stackCount < numToCarry))
                            {
								Thing inventoryThing = pawn.inventory.innerContainer.First(t => t.def == def);
								if (inventoryThing == null) break;
								int transferCount = Math.Min(numToCarry, inventoryThing.stackCount);
								pawn.inventory.innerContainer.TryTransferToContainer(inventoryThing, pawn.carryTracker.innerContainer, transferCount);
							}
							Thing carriedThing = pawn.carryTracker.CarriedThing;
							if (carriedThing != null && carriedThing.def == def)
                            {
								job.targetA = carriedThing;

							}
                        }
                    }
					*/
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
		}

	}
}
