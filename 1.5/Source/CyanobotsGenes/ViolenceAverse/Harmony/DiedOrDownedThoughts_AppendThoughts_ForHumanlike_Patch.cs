using RimWorld;
using Verse;
using HarmonyLib;
using System.Collections.Generic;

namespace CyanobotsGenes
{


    [HarmonyPatch(typeof(PawnDiedOrDownedThoughtsUtility),"AppendThoughts_ForHumanlike")]
    static public class DiedOrDownedThoughts_AppendThoughts_ForHumanlike_Patch
    {
        static void Postfix(Pawn victim, DamageInfo? dinfo, ref PawnDiedOrDownedThoughtsKind thoughtsKind, ref List<IndividualThoughtToAdd> outIndividualThoughts, List<ThoughtToAddToAll> outAllColonistsThoughts)
        {
            bool isExecution = dinfo.HasValue && dinfo.Value.Def.execution;
            Pawn instigator = dinfo.HasValue ? (Pawn)dinfo.Value.Instigator : null;
            if (instigator != null && !instigator.Dead && instigator.needs.mood != null && instigator.story != null && instigator != victim && PawnUtility.ShouldGetThoughtAbout(instigator, victim))
            {
                if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died)
                {
                    outIndividualThoughts.Add(new IndividualThoughtToAdd(CG_DefOf.CYB_ViolenceAverse_KilledHumanlike, instigator));
                }
            }
            
		    if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Downed)
		    {
			    foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
			    {
				    if (pawn == victim || pawn.needs == null || pawn.needs.mood == null || !PawnUtility.ShouldGetThoughtAbout(pawn, victim))
				    {
					    continue;
				    }
				    if (ThoughtUtility.Witnessed(pawn, victim))
				    {
                        outIndividualThoughts.Add(new IndividualThoughtToAdd(CG_DefOf.CYB_ViolenceAverse_WitnessedDowned, pawn));
				    }
			    }
		    }
            else if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died)
            {
                if (isExecution)
                {
                    outAllColonistsThoughts.Add(new ThoughtToAddToAll(CG_DefOf.CYB_ViolenceAverse_KnowExecuted));
                }
                else if (instigator != null && instigator.Faction == Faction.OfPlayer)
                {
                    outAllColonistsThoughts.Add(new ThoughtToAddToAll(CG_DefOf.CYB_ViolenceAverse_ViolentDeaths));
                }
            }
        }
    }

}
