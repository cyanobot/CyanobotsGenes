using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;
using static CyanobotsGenes.OffspringUtility;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(PregnancyUtility),nameof(PregnancyUtility.GetInheritedGenes),
        new Type[] { typeof(Pawn), typeof(Pawn), typeof(bool)}, 
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out })]
    public static class GetInheritedGenes_Patch
    {
        public static bool Prefix(ref List<GeneDef> __result, Pawn father, Pawn mother, ref bool success)
        {
            //Log.Message("Running GetInheritedGenes_Patch");
            if (HasActiveOffspringGene(father) || HasActiveOffspringGene(mother))
            {
                XenotypeDef xenotype = GetOffspringXenotype(mother, father);
                if (xenotype == null) return true;

                __result = new List<GeneDef>();
                foreach (GeneDef gene in xenotype.AllGenes)
                {
                    __result.Add(gene);
                }
                success = true;
                //Log.Message("GetInheritedGenes_Patch returning genes: " + __result.ToStringSafeEnumerable());
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(GeneUtility),nameof(GeneUtility.SameHeritableXenotype))]
    public static class SameHeritableXenotype_Patch
    {
        public static bool Prefix(ref bool __result, Pawn pawn1, Pawn pawn2)
        {
            //Log.Message("Running SameHeritableXenotype_Patch");
            if (HasActiveOffspringGene(pawn1) || HasActiveOffspringGene(pawn2))
            {
                //Log.Message("SameHeritableXenotype_Patch returning false");
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PregnancyUtility),"TryGetInheritedXenotype")]
    public static class TryGetInheritedXenotype_Patch
    {
        public static bool Prefix(ref bool __result, Pawn mother, Pawn father, ref XenotypeDef xenotype)
        {
            //Log.Message("Running TryGetInheritedXenotype_Patch");
            if (HasActiveOffspringGene(father) || HasActiveOffspringGene(mother))
            {
                xenotype = GetOffspringXenotype(mother, father);
                if (xenotype == null) return true;

                //Log.Message("TryGetInheritedXenotype_Patch returning xenotype: " + xenotype);

                __result = true;
                return false;
            }
            return true;
        }
    }
}
