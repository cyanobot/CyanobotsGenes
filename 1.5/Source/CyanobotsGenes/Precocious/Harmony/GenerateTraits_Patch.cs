using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(PawnGenerator), "GenerateTraits",
        new Type[] { typeof(Pawn), typeof(PawnGenerationRequest) })]
    public static class GenerateTraits_Patch
    {
        public static void Postfix(Pawn pawn, PawnGenerationRequest request)
        {
            //only interested in children
            int ageBiologicalYears = pawn.ageTracker.AgeBiologicalYears;
            if (ageBiologicalYears >= pawn.ageTracker.AdultMinAge) return;

            //only interested in precocious
            Gene_Precocious gene_Precocious = (Gene_Precocious)(pawn.genes?.GetGene(CG_DefOf.CYB_Precocious));
            if (gene_Precocious == null || !gene_Precocious.Active) return;

            //only interested in after first growth moment
            if (ageBiologicalYears < gene_Precocious.MinAgeChild) return;

            //extra potential trait from extra growth moment

            int maximumAgeTraits = request.MaximumAgeTraits;
            int count = pawn.story.traits.allTraits.Count;

            if (count >= maximumAgeTraits) return;

            Trait trait = PawnGenerator.GenerateTraitsFor(pawn, 1, request, growthMomentTrait: true).FirstOrFallback();
            if (trait != null)
            {
                pawn.story.traits.GainTrait(trait);
            }
        }
    }
}
