using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace CyanobotsGenes
{
    class Thought_Bodyfeeder_AteNonFriend : Thought_Bodyfeeder_AteLivePerson
    {
        private static List<ThoughtDef> removeByPawn = new List<ThoughtDef>
            {
            /*
                ThoughtDefOf.KnowColonistDied,
                ThoughtDefOf.KnowPrisonerDiedInnocent,
            */
            };
        private static List<ThoughtDef> removeByTime = new List<ThoughtDef>
            {
                ThoughtDefOf.WitnessedDeathAlly,
                ThoughtDefOf.WitnessedDeathNonAlly
            };

        public override List<ThoughtDef> RemoveByPawn => removeByPawn;
        public override List<ThoughtDef> RemoveByTime => removeByTime;
    }
}
