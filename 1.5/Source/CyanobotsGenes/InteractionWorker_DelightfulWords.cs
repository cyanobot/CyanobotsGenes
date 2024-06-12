using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CyanobotsGenes
{
    class InteractionWorker_DelightfulWords : InteractionWorker
	{
		public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
		{
			if (initiator.Inhumanized())
			{
				return 0f;
			}
			Trait trait = initiator.story.traits.GetTrait(CG_DefOf.CYB_Delightful);
			if (trait != null && !trait.Suppressed)
			{
				return 0.02f;
			}
			return 0f;
		}
	}
}
