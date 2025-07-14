using RimWorld;
using HarmonyLib;
using Verse;
using UnityEngine;
using System.Reflection;

namespace CyanobotsGenes
{
    
    [HarmonyPatch(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.TryChildGrowthMoment))]
    public static class TryChildGrowthMoment_Patch
    {
        public static void Prefix()
        {
            LogUtil.DebugLog("TryChildGrowthMoment_Patch Prefix");
        }

        public static void Postfix(Pawn_AgeTracker __instance, Pawn ___pawn, int birthdayAge,
            ref int newPassionOptions, ref int newTraitOptions, ref int passionGainsCount)
        {
            LogUtil.DebugLog("TryChildGrowthMoment_Patch - pawn: " + ___pawn + ", birthdayAge: " + birthdayAge
                + ", newPassionOptions: " + newPassionOptions + ", newTraitOptions: " + newTraitOptions
                + ", passionGainsCount: " + passionGainsCount);

            //quickest to check -- we're only interested in age 3
            //if (birthdayAge != 3) return;

            //only interested in precocious pawns
            Gene_Precocious gene_Precocious = (Gene_Precocious)___pawn?.genes?.GetGene(CG_DefOf.CYB_Precocious);
            LogUtil.DebugLog("gene_Precocious: " + gene_Precocious);
            if (gene_Precocious == null || !gene_Precocious.Active) return;

            //only interested in the birthday at which they would become a child
            float minChildAge = gene_Precocious.MinAgeChild;
            if (birthdayAge != minChildAge) return;

            //replicate vanilla growth moment behaviour if passed both checks
            int growthTier = __instance.GrowthTier;
#if RW_1_5
            newPassionOptions = GrowthUtility.PassionChoicesPerTier[growthTier];
            newTraitOptions = GrowthUtility.TraitChoicesPerTier[growthTier];
            passionGainsCount = Mathf.Min(___pawn.skills.skills.Count((SkillRecord s) => (int)s.passion < 2), GrowthUtility.PassionGainsPerTier[growthTier]);
#else
            newPassionOptions = GrowthUtility.GrowthTiers[growthTier].passionChoices;
            newTraitOptions = GrowthUtility.GrowthTiers[growthTier].traitChoices;
            passionGainsCount = Mathf.Min(___pawn.skills.skills.Count((SkillRecord s) => (int)s.passion < 2), GrowthUtility.GrowthTiers[growthTier].PassionGainFor(___pawn));
#endif

            LogUtil.DebugLog("newPassionOptions: " + newPassionOptions + ", newTraitOptions: " + newTraitOptions
                + ", passionGainsCount: " + passionGainsCount);
        }
    }
    
}
