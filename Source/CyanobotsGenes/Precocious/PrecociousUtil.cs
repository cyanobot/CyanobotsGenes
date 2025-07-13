using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.QuestGen;
using Verse;
using HarmonyLib;

namespace CyanobotsGenes
{
    class PrecociousUtil
    {
        public static bool IsPrecociousBaby(Pawn pawn, out Gene_Precocious gene_Precocious)
        {
            gene_Precocious = (Gene_Precocious)pawn?.genes?.GetGene(CG_DefOf.CYB_Precocious);
            if (gene_Precocious == null) return false;
            return gene_Precocious.Active && gene_Precocious.CurrentlyPrecocious;
        }

        /*
        public static bool PrecociousShouldReachOutsideNow(Pawn p)
        {

        }
        */
    }
}
