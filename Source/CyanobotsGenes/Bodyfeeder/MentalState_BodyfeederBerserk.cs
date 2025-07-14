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

		public override void PostEnd()
		{
			base.PostEnd();

			Gene_Hemogen hemogenGene = pawn.genes.GetFirstGeneOfType<Gene_Hemogen>();
			if ((hemogenGene ==  null || hemogenGene.Resource.ValuePercent >= 0.9f) && pawn.health.hediffSet.HasHediff(CG_DefOf.BodyfeederStarvation))
            {
				pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(CG_DefOf.BodyfeederStarvation));
            }
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

#if RW_1_5
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
#else
        public override void MentalStateTick(int delta)
        {
            if (ShouldStop())
            {
                RecoverFromState();
            }
            else
            {
                base.MentalStateTick(delta);
            }
        }
#endif
    }
}
