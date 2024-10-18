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
    class PawnRenderNodeWorker_Skeletal : PawnRenderNodeWorker
    {
        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            if (!base.CanDrawNow(node, parms)) return false;

            return parms.rotDrawMode.HasFlag(RotDrawMode.Dessicated);
        }
    }
}
