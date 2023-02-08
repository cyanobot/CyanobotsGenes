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
    class JobDriver_IngestForHemogen : JobDriver_Ingest
    {
		private bool UsingNutrientPasteDispenser => (bool)typeof(JobDriver_Ingest).GetField("usingNutrientPasteDispenser",BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);

		private const TargetIndex TableCellInd = TargetIndex.B;
		private Thing IngestibleSource => job.GetTarget(IngestibleSourceInd).Thing;

		private float ChewDurMult => (float)typeof(JobDriver_Ingest).GetProperty("ChewDurationMultiplier", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
	
		protected override IEnumerable<Toil> MakeNewToils()
		{
			Log.Message("fired MakeNewToils");
			if (!UsingNutrientPasteDispenser)
			{
				this.FailOn(() => !IngestibleSource.Destroyed && !IngestibleSource.IngestibleNow);
			}

			Toil chew = Toils_Ingest.ChewIngestible(pawn, ChewDurMult, IngestibleSourceInd, TableCellInd)
				.FailOn((Toil x) => !IngestibleSource.Spawned && (pawn.carryTracker == null || pawn.carryTracker.CarriedThing != IngestibleSource))
				.FailOnCannotTouch(IngestibleSourceInd, PathEndMode.Touch);
			Log.Message("Created toil chew");
			foreach (Toil item in (IEnumerable<Toil>)typeof(JobDriver_Ingest).GetMethod("PrepareToIngestToils",BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this,new object[] { chew }))
			{
				yield return item;
			}
			yield return chew;
			yield return FinalizeIngest(pawn, IngestibleSourceInd);
			yield return Toils_Jump.JumpIf(chew, () => job.GetTarget(TargetIndex.A).Thing is Corpse && BodyfeederUtility.HemogenLevelPct(pawn) < 0.9f);
		}

		public static Toil FinalizeIngest(Pawn ingester, TargetIndex ingestibleInd)
		{
			Toil toil = ToilMaker.MakeToil("FinalizeIngest");
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				Thing thing = curJob.GetTarget(ingestibleInd).Thing;
				if (ingester.needs.mood != null && thing.def.IsNutritionGivingIngestible && thing.def.ingestible.chairSearchRadius > 10f)
				{
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
				float nutritionWanted = BodyfeederUtility.BodyfeederNutritionWanted(actor, thing);
				float nutritionGained = thing.Ingested(ingester, nutritionWanted);
				if (!ingester.Dead)
				{
					ingester.needs.food.CurLevel += nutritionGained;
				}
				ingester.records.AddTo(RecordDefOf.NutritionEaten, nutritionGained);
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
		}
	}
}
