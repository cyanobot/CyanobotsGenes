using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(PawnGenerator), "GenerateSkills",
        new Type[] { typeof(Pawn), typeof(PawnGenerationRequest) })]
    public static class GenerateSkillss_Patch
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

            //extra potential passion from extra growth moment
            int num2 = Rand.RangeInclusive(0, 3);
            for (int k = 0; k < num2; k++)
            {
                SkillDef skillDef2 = ChoiceLetter_GrowthMoment.PassionOptions(pawn, 1, checkGenes: true).FirstOrDefault();
                if (skillDef2 != null)
                {
                    SkillRecord skill = pawn.skills.GetSkill(skillDef2);
                    skill.passion = skill.passion.IncrementPassion();
                }
            }
        }
    }
}
