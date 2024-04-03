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
    class JobDriver_IngestForHemogen_Old : JobDriver_Ingest
    {

		private bool UsingNutrientPasteDispenser => (bool)typeof(JobDriver_Ingest).GetField("usingNutrientPasteDispenser",BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);

		private const TargetIndex TableCellInd = TargetIndex.B;
		private Thing IngestibleSource => job.GetTarget(IngestibleSourceInd).Thing;

		private float ChewDurMult => (float)typeof(JobDriver_Ingest).GetProperty("ChewDurationMultiplier", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
	
		protected override IEnumerable<Toil> MakeNewToils()
		{
			//Log.Message("fired MakeNewToils, IngestibleSource: " + IngestibleSource + ", IngestibleNow: " + IngestibleSource.IngestibleNow 
			//	+ ", CorpseIngestibleForHemogenNow: " + CorpseIngestibleForHemogenNow(IngestibleSource as Corpse));
			if (!UsingNutrientPasteDispenser)
			{
				this.FailOn(() => !IngestibleSource.Destroyed && !(IngestibleSource.IngestibleNow || CorpseIngestibleForHemogenNow(IngestibleSource as Corpse)));
			}
			float hemogenThreshold = 0.9f;
			if (!this.job.playerForced) SetFinalizerJob((JobCondition condition) =>
				HemogenLevelPct(pawn) < hemogenThreshold 
				? MakeIngestForHemogenJob(pawn)
				: null
				);

			Toil chew;
			if (IsRottingCorpse(TargetA.Thing))
            {
				chew = ChewRotting(pawn, ChewDurMult, IngestibleSourceInd, TableCellInd);
            }
            else
            {
				chew = Toils_Ingest.ChewIngestible(pawn, ChewDurMult, IngestibleSourceInd, TableCellInd);
			}
			chew.FailOn((Toil x) => !IngestibleSource.Spawned && (pawn.carryTracker == null || pawn.carryTracker.CarriedThing != IngestibleSource));
			chew.FailOnCannotTouch(IngestibleSourceInd, PathEndMode.Touch);
			//Log.Message("Created toil chew : " + chew);
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

				Toil reserveFood = (Toil)typeof(JobDriver_Ingest).GetMethod("ReserveFood", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, new object[] { });
				yield return reserveFood;
				Toil gotoToPickup = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedOrNull(TargetIndex.A);
				yield return Toils_Jump.JumpIf(gotoToPickup, () => pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation));
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
				yield return Toils_Jump.Jump(chew);
				yield return gotoToPickup;
				yield return Toils_Ingest.PickupIngestible(TargetIndex.A, pawn);
				//Log.Message("Finished creating pick food up toils");
			}
			if (job.takeExtraIngestibles > 0)
			{
				//Log.Message("takeExtraIngestibles: " + job.takeExtraIngestibles);
				foreach (Toil item in (IEnumerable<Toil>)typeof(JobDriver_Ingest).GetMethod("TakeExtraIngestibles", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, new object[] { }))
				{
					yield return item;
				}
			}
			if (!pawn.Drafted && BodyfeederUtility.GetDesperation(pawn) < BodyfeederUtility.Desperation.StarvationMild)
			{
				//Log.Message("neither drafted nor mod+ starving");
				yield return Toils_Ingest.CarryIngestibleToChewSpot(pawn, TargetIndex.A).FailOnDestroyedOrNull(TargetIndex.A);
			}
			yield return Toils_Ingest.FindAdjacentEatSurface(TargetIndex.B, TargetIndex.A);
			yield return chew;
			yield return FinalizeIngest(pawn, IngestibleSourceInd);
			yield return Toils_Jump.JumpIf(chew, () => job.GetTarget(TargetIndex.A).Thing is Corpse && BodyfeederUtility.HemogenLevelPct(pawn) < 0.9f);
			//Log.Message("Finished creating toils");
		}

		public static Toil FinalizeIngest(Pawn ingester, TargetIndex ingestibleInd)
		{
			Toil toil = ToilMaker.MakeToil("FinalizeIngest");
			toil.initAction = delegate
			{
				//Log.Message("FinalizeIngest.initAction");
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				Thing thing = curJob.GetTarget(ingestibleInd).Thing;
				if (ingester.needs.mood != null && thing.def.IsNutritionGivingIngestible && thing.def.ingestible.chairSearchRadius > 10f)
				{
					//Log.Message("Attempting to gen room thoughts");
					if (!(ingester.Position + ingester.Rotation.FacingCell).HasEatSurface(actor.Map) && ingester.GetPosture() == PawnPosture.Standing && !ingester.IsWildMan() && thing.def.ingestible.tableDesired)
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
				}
				if (IsRottingCorpse(thing)) 
				{
					RottingCorpseIngested(ingester, (Corpse)thing);
					//Log.Message("eating rotting corpse, nutritionGained: " + nutritionGained);
				}
                else
				{
					float effectiveNutritionWanted;
					if (thing is Corpse)
                    {
						effectiveNutritionWanted = BodyfeederNutritionWanted(ingester, thing);
                    }
                    else
                    {
						effectiveNutritionWanted = thing.GetStatValue(StatDefOf.Nutrition) * (float)ingester.jobs.curJob.count;
					}
					
					float nutritionGained = thing.Ingested(ingester, effectiveNutritionWanted);
					Log.Message("effectiveNutritionWanted: " + effectiveNutritionWanted + ", nutritionGained: " + nutritionGained);
					if (!ingester.Dead)
					{
						ingester.needs.food.CurLevel += nutritionGained;
					}
					ingester.records.AddTo(RecordDefOf.NutritionEaten, nutritionGained);
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
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
					Log.Message("!CorpseIngestibleForHemogenNow");
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
	}
}
