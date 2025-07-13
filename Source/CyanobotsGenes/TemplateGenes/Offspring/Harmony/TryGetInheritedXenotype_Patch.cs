using HarmonyLib;
using Verse;
using RimWorld;
using static CyanobotsGenes.OffspringUtility;
using static CyanobotsGenes.CG_Mod;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(PregnancyUtility), "TryGetInheritedXenotype")]
    public static class TryGetInheritedXenotype_Patch
    {
        public static bool Prefix(ref bool __result, Pawn mother, Pawn father, ref XenotypeDef xenotype)
        {
            //Log.Message("Running TryGetInheritedXenotype_Patch");
            if (HasActiveOffspringGene(father) || HasActiveOffspringGene(mother))
            {
                List<XenotypeDef> potentialXenotypes = GetPotentialOffspringXenotypes(mother, father);

                if (potentialXenotypes.Count == 0)
                {
                    return true;
                }
                if (potentialXenotypes.Count == 1)
                {
                    xenotype = potentialXenotypes[0];
                    __result = true;
                    LogUtil.DebugLog("TryGetInheritedXenotype_Patch returning xenotype: " + xenotype);
                    return false;
                }
                else
                {
                    xenotype = null;
                    __result = true;
                    LogUtil.DebugLog("TryGetInheritedXenotype_Patch found multiple options, returning xenotype null");
                    return false;
                }

            }
            return true;
        }
    }

    }
