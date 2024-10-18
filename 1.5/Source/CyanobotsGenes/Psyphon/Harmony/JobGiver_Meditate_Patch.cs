using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;
using HarmonyLib;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(JobGiver_Meditate),nameof(JobGiver_Meditate.GetPriority))]
    public static class JobGiver_Meditate_Patch
    {
        public static float Postfix(float __result, Pawn pawn)
        {
            if (pawn.HasPsylink && pawn.genes != null)
            {
                Gene_Psyphon gene_Psyphon = pawn.genes.GetFirstGeneOfType<Gene_Psyphon>();
                if (gene_Psyphon != null && gene_Psyphon.disableMeditate)
                {
                    return 0f;
                }
            }
            return __result;
        }
    }
}
