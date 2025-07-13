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
    public class PawnRenderNodeWorker_VariantsOverTime : PawnRenderNodeWorker
    {
        protected override Material GetMaterial(PawnRenderNode node, PawnDrawParms parms)
        {
            if (!(node is PawnRenderNode_VariantsOverTime node_Variants))
                return base.GetMaterial(node, parms);

			Graphic graphic = node.Graphic;
			if (graphic == null)
			{
				return null;
			}
			if (!(graphic is Graphic_VariantsOverTime graphic_variantsOverTime))
            {
				return base.GetMaterial(node, parms);
            }

			if (node.Props.flipGraphic && parms.facing.IsHorizontal)
			{
				parms.facing = parms.facing.Opposite;
			}

			int variant = node_Variants.VariantFor(parms.facing);
			//Log.Message("variant: " + variant);
			Material material = graphic_variantsOverTime.SubGraphicAt(parms.facing, variant).MatSingle;	

			if (material != null && !parms.Portrait && parms.flags.FlagSet(PawnRenderFlags.Invisible))
			{
				material = InvisibilityMatPool.GetInvisibleMat(material);
			}
			return material;

		}

        public override void PreDraw(PawnRenderNode node, Material mat, PawnDrawParms parms)
        {
            base.PreDraw(node, mat, parms);
			if (node is PawnRenderNode_VariantsOverTime node_Variants)
            {
				node_Variants.UpdateCacheIfNeeded();
            }
        }

    }
    
}
