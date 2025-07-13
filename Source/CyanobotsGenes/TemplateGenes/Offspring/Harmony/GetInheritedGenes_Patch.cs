using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;
using RimWorld;
using static CyanobotsGenes.OffspringUtility;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(PregnancyUtility), nameof(PregnancyUtility.GetInheritedGenes),
        new Type[] { typeof(Pawn), typeof(Pawn), typeof(bool) },
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out })]
    public static class GetInheritedGenes_Patch
    {
        [HarmonyAfter("com.VEHighmate", "RedMattis.BGInheritance", "ConsistentGeneInheritance")]
        public static void Postfix(ref List<GeneDef> __result, Pawn father, Pawn mother, ref bool success)
        {
            LogUtil.DebugLog("Running GetInheritedGenes_Patch");
            if (HasActiveOffspringGene(father) || HasActiveOffspringGene(mother))
            {
                XenotypeDef xenotype = GetOffspringXenotype(mother, father);
                LogUtil.DebugLog("xenotype: " + xenotype);
                if (xenotype == null) return;

                __result = new List<GeneDef>();
                foreach (GeneDef gene in xenotype.AllGenes)
                {
                    __result.Add(gene);
                }

                List<GeneDef> endogenes = GetInheritedEndogenes(mother, father, xenotype);
                __result.AddRange(endogenes);

                success = true;
                LogUtil.DebugLog("GetInheritedGenes_Patch returning genes: " + __result.ToStringSafeEnumerable());
                return;
            }
        }
    }

    }
