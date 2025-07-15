using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

#if RW_1_5

using static CyanobotsGenes.LightSensitivityUtility;

namespace CyanobotsGenes
{
    class StatPart_LightSensitivity : StatPart
    {
        float brightFactor = 0.8f;
        float dimFactor = 0.9f;

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!(req.HasThing && req.Thing is Pawn pawn)) return;
            if (!pawn.HasActiveGene(CG_DefOf.CYB_LightSensitivity)) return;
            if (PawnUtility.IsBiologicallyOrArtificiallyBlind(pawn)) return;

            LightLevel lightLevel = LightLevelFor(pawn);
            if (lightLevel == LightLevel.Bright)
            {
                val *= brightFactor;
            }
            if (lightLevel == LightLevel.Dim)
            {
                val *= dimFactor;
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!(req.HasThing && req.Thing is Pawn pawn)) return null;
            if (!pawn.HasActiveGene(CG_DefOf.CYB_LightSensitivity)) return null;
            if (PawnUtility.IsBiologicallyOrArtificiallyBlind(pawn)) return null;

            LightLevel lightLevel = LightLevelFor(pawn);
            if (lightLevel == LightLevel.Dark) return null;
            float factor = 1f;
            if (lightLevel == LightLevel.Bright) factor = brightFactor;
            if (lightLevel == LightLevel.Dim) factor = dimFactor;
            return "CYB_StatsReport_LightSensitivity".Translate() + ": x" + factor.ToStringPercent();
        }

    }
}
#endif