using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;

namespace CyanobotsGenes.Precocious.Harmony
{
    [HarmonyPatch(typeof(Gizmo_GrowthTier), "GrowthTierTooltip",
        new Type[] { typeof(Rect), typeof(int) })]
    public static class GrowthTierTooltip_Patch
    {
        private static int approxStartPosition = 0;

        public static void Prepare()
        {
            approxStartPosition = 0;
            approxStartPosition += "StatsReport_GrowthTier".Translate().Length;
            approxStartPosition += "StatsReport_GrowthTierDesc".Translate().Length;
            approxStartPosition += "MaxTier".Translate().Length;
        }

        public static string Postfix(string result, Pawn ___child)
        {
            Gene_Precocious gene_Precocious = (Gene_Precocious)(___child.genes?.GetGene(CG_DefOf.CYB_Precocious));
            if (gene_Precocious == null || !gene_Precocious.Active) return result;

            if (___child.ageTracker.AgeBiologicalYears >= gene_Precocious.MinAgeChild) return result;

            string nextGrowthMomentString = ("NextGrowthMomentAt".Translate() + ": ").AsTipTitle();
            int length = nextGrowthMomentString.Length;
            int startIndex = result.IndexOf(nextGrowthMomentString, approxStartPosition);
            int endIndex = startIndex + length - 1;

            result = result.Remove(endIndex + 1, 1);
            result = result.Insert(endIndex + 1, ((int)gene_Precocious.MinAgeChild).ToString());
            return result;
        }
    }
}
