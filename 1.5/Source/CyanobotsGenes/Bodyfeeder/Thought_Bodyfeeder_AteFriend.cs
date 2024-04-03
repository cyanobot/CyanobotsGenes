using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace CyanobotsGenes
{
    class Thought_Bodyfeeder_AteFriend : Thought_Bodyfeeder_AteLivePerson
    {
        private static List<ThoughtDef> removeByPawn = new List<ThoughtDef>
            {
            /*
                ThoughtDefOf.PawnWithGoodOpinionDied,
                ThoughtDefOf.KnowColonistDied,
                DefDatabase<ThoughtDef>.GetNamed("MySonDied"),
                DefDatabase<ThoughtDef>.GetNamed("MyDaughterDied"),
                DefDatabase<ThoughtDef>.GetNamed("MyHusbandDied"),
                DefDatabase<ThoughtDef>.GetNamed("MyWifeDied"),
                DefDatabase<ThoughtDef>.GetNamed("MyFianceDied"),
                DefDatabase<ThoughtDef>.GetNamed("MyFianceeDied"),
                DefDatabase<ThoughtDef>.GetNamed("MyLoverDied"),
                DefDatabase<ThoughtDef>.GetNamed("MyBrotherDied"),
                DefDatabase<ThoughtDef>.GetNamed("MySisterDied"),
                DefDatabase<ThoughtDef>.GetNamed("MyGrandchildDied"),
                DefDatabase<ThoughtDef>.GetNamed("MyFatherDied"),
                DefDatabase<ThoughtDef>.GetNamed("MyMotherDied"),
                DefDatabase<ThoughtDef>.GetNamed("MyNieceDied"),
                DefDatabase<ThoughtDef>.GetNamed("MyNephewDied"),
                DefDatabase<ThoughtDef>.GetNamed("MyHalfSiblingDied"),
                DefDatabase<ThoughtDef>.GetNamed("MyAuntDied"),
                DefDatabase<ThoughtDef>.GetNamed("MyUncleDied"),
                DefDatabase<ThoughtDef>.GetNamed("MyGrandparentDied"),
                DefDatabase<ThoughtDef>.GetNamed("MyCousinDied"),
                DefDatabase<ThoughtDef>.GetNamed("MyKinDied")
            */
            };
        private static List<ThoughtDef> removeByTime = new List<ThoughtDef>
            {
                ThoughtDefOf.WitnessedDeathAlly,
                ThoughtDefOf.WitnessedDeathFamily,
                ThoughtDefOf.WitnessedDeathNonAlly
            };

        public override List<ThoughtDef> RemoveByPawn => removeByPawn;
        public override List<ThoughtDef> RemoveByTime => removeByTime;
    }
}
