// RimWorld.Alert_LowHemogen
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace CyanobotsGenes
{
	public class Alert_BodyfeederLowHemogen : Alert
	{
		private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

		private List<string> targetLabels = new List<string>();

		private List<GlobalTargetInfo> Targets
		{
			get
			{
				CalculateTargets();
				return targets;
			}
		}

		public override string GetLabel()
		{
			if (Targets.Count == 1)
			{
				return "AlertLowHemogen".Translate() + ": " + targetLabels[0];
			}
			return "AlertLowHemogen".Translate();
		}

		private void CalculateTargets()
		{
			targets.Clear();
			targetLabels.Clear();
			if (!ModsConfig.BiotechActive)
			{
				return;
			}
#if RW_1_5
			foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
#else
			foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive)
#endif
			{
				if (item.RaceProps.Humanlike && item.Faction == Faction.OfPlayer)
				{
					if (!BodyfeederUtility.IsBodyFeeder(item)) continue;
					Gene_Hemogen gene_Hemogen = item.genes?.GetFirstGeneOfType<Gene_Hemogen>();
					if (gene_Hemogen != null && gene_Hemogen.Value < gene_Hemogen.MinLevelForAlert)
					{
						targets.Add(item);
						targetLabels.Add(item.NameShortColored.Resolve());
					}
				}
			}
		}

		public override TaggedString GetExplanation()
		{
			return "CYB_Alert_BodyfeederLowHemogenDesc".Translate() + ":\n" + targetLabels.ToLineList("  - ");
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(Targets);
		}
	}
}
