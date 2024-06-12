using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace CyanobotsGenes
{
    class ThinkNode_ConditionalPrecocious : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			//Log.Message("Calling ThinkNode_ConditionalPrecocious.Satisfied - pawn: " + pawn);
			return PrecociousUtil.IsPrecociousBaby(pawn, out var _);
		}
	}
}
