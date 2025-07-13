using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static CyanobotsGenes.ThoughtWorker_LightSensitivity;

namespace CyanobotsGenes
{
    class ConditionalStatAffecter_BrightLight : ConditionalStatAffecter
    {
        public override string Label => "CYB_StatsReport_BrightLight".Translate();

        public override bool Applies(StatRequest req)
        {
            if (req.HasThing && req.Thing is Pawn pawn)
            {
                if (PawnUtility.IsBiologicallyOrArtificiallyBlind(pawn)) return false;

                LightLevel lightLevel = LightLevelFor(pawn);
                if (lightLevel == LightLevel.Bright) return true;
            }
            return false;
        }
    }
}
