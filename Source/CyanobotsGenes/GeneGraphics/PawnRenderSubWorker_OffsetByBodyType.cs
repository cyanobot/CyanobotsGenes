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
    class PawnRenderSubWorker_OffsetByBodyType : PawnRenderSubWorker
    {
        public override void TransformOffset(PawnRenderNode node, PawnDrawParms parms, ref Vector3 offset, ref Vector3 pivot)
        {
            if (!(node is PawnRenderNode_OffsetByBodyType node_byBodyType)) return;

            BodyTypeDef bodyType = parms.pawn.story?.bodyType;
            if (bodyType == null) return;

            offset += node_byBodyType.OffsetFor(bodyType, parms.facing);

        }
    }
}
