﻿using UnityEngine;
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
	public class MentalState_BodyfeederBerserk : MentalState
	{
		private bool ShouldStop()
		{
			Gene_Hemogen hemogenGene = pawn.genes.GetFirstGeneOfType<Gene_Hemogen>();
			if (hemogenGene == null) return true;
			if (hemogenGene.Resource.ValuePercent >= 0.9f) return true;

			if (!pawn.health.hediffSet.HasHediff(CG_DefOf.BodyfeederStarvation)) return true;

			return false;
		}

		public override bool ForceHostileTo(Thing t)
		{
			return true;
		}

		public override bool ForceHostileTo(Faction f)
		{
			return true;
		}

		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}

		public override void MentalStateTick()
		{
			if (ShouldStop())
			{
				RecoverFromState();
			}
			else
			{
				base.MentalStateTick();
			}
		}
	}
}
