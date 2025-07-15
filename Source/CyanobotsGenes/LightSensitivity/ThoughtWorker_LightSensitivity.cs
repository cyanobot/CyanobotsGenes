using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using RimWorld.Planet;
using HarmonyLib;
using System.Reflection;
using System.Collections;
using static CyanobotsGenes.CG_Mod;
using static CyanobotsGenes.LightSensitivityUtility;

namespace CyanobotsGenes
{
    class ThoughtWorker_LightSensitivity_Mild : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (PawnUtility.IsBiologicallyOrArtificiallyBlind(p)) return ThoughtState.Inactive;

            LightLevel lightLevel = LightLevelFor(p);
            if (lightLevel == LightLevel.Bright) return ThoughtState.ActiveAtStage(0);
            return ThoughtState.Inactive;
        }

    }

    class ThoughtWorker_LightSensitivity_Severe : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (PawnUtility.IsBiologicallyOrArtificiallyBlind(p)) return ThoughtState.Inactive;

            LightLevel lightLevel = LightLevelFor(p);
            if (lightLevel == LightLevel.Bright) return ThoughtState.ActiveAtStage(1);
            if (lightLevel == LightLevel.Dim) return ThoughtState.ActiveAtStage(0);
            return ThoughtState.Inactive;
        }

    }
}
