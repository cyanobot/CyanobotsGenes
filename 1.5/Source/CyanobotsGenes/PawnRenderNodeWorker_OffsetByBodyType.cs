using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace CyanobotsGenes
{
    public class PawnRenderNodeWorker_OffsetByBodyType : PawnRenderNodeWorker
    {
        public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
        {
            Vector3 offset = base.OffsetFor(node, parms, out pivot);
            if (!(node is PawnRenderNode_OffsetByBodyType node_byBodyType)) return offset;

            BodyTypeDef bodyType = parms.pawn.story?.bodyType;
            if (bodyType == null) return offset;

            offset += node_byBodyType.OffsetFor(bodyType, parms.facing);

            return offset;
        }
    }
}
