using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System;
using Verse.AI;
#if RW_1_5
#else
namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(FloatMenuOptionProvider_Ingest), "GetSingleOptionFor", new Type[] { typeof(Thing), typeof(FloatMenuContext) })]
    class FloatMenuOptionProvider_Ingest_Diet_Patch
    {
        public static FloatMenuOption Postfix(FloatMenuOption result, Thing clickedThing, FloatMenuContext context)
        {
            Pawn pawn = context.FirstSelectedPawn;

            //not interested in things that simply cannot be ingested
            if (result == null) return null;

            //if ingest option already disabled, don't worry about it
            if (result.Disabled) return result;

            //only looking for things that should be forbidden by genetic diet
            if (!GeneticDietUtility.DietForbids(clickedThing, pawn)) return result;

            //disable and tell the player why
            result.Disabled = true;
            result.Label += " : " + "CYB_Inedible".Translate();

            return result;
        }
    }
}
#endif