using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CyanobotsGenes
{
    public class PawnRenderNodeWorker_TailFlipWhenCrawling : PawnRenderNodeWorker
	{
		protected override Material GetMaterial(PawnRenderNode node, PawnDrawParms parms)
		{
			if (!parms.Portrait && parms.pawn.Crawling)
			{
				if (parms.facing == Rot4.North)
				{
					parms.facing = Rot4.South;
				}
			}

			return base.GetMaterial(node, parms);
		}

		public override Quaternion RotationFor(PawnRenderNode node, PawnDrawParms parms)
		{
			Quaternion result = base.RotationFor(node, parms);
			if (!parms.Portrait && parms.pawn.Crawling)
			{
				if (parms.facing == Rot4.North || parms.facing == Rot4.South)
					result *= 180f.ToQuat();
				else if (parms.facing == Rot4.East)
					result *= (-110f).ToQuat();
                else
                {
					result *= 110f.ToQuat();
                }
			}
			return result;
		}

		/*
		public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms)
		{
			Vector3 vector = base.ScaleFor(node, parms);
			Vector2 bodyGraphicScale = parms.pawn.story.bodyType.bodyGraphicScale;
			return vector * ((bodyGraphicScale.x + bodyGraphicScale.y) / 2f);
		}
		*/
	}
}
