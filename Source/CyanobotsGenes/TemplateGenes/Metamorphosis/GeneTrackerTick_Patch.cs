using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(Pawn_GeneTracker),nameof(Pawn_GeneTracker.GeneTrackerTick))]
    public static class GeneTrackerTick_Patch
    {
        public static void Postfix(Pawn_GeneTracker __instance)
        {
            if (__instance.pawn.IsHashIntervalTick(Gene_Metamorphosis.tickInterval))
            {
                Gene_Metamorphosis chosenGene = null;
                foreach (Gene gene in __instance.GenesListForReading)
                {
                    if (gene is Gene_Metamorphosis gene_Met && gene_Met.chosenGene == true)
                    {
                        chosenGene = gene_Met;
                        break;
                    }
                }
                if (chosenGene != null)
                {
                    chosenGene.Metamorphose();
                }
            }
        }
    }
}
