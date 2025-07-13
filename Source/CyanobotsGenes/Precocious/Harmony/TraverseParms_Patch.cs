using Verse;
using HarmonyLib;
using static CyanobotsGenes.CG_Util;
using static CyanobotsGenes.PrecociousUtil;
using UnityEngine;
using System;

namespace CyanobotsGenes
{

    //precocious babies should not bash down doors to get into food stores
    [HarmonyPatch(typeof(TraverseParms), nameof(TraverseParms.For), new Type[] { typeof(Pawn), typeof(Danger), typeof(TraverseMode), typeof(bool), typeof(bool), typeof(bool) })]
    public static class TraverseParms_Patch
    {
        public static void Postfix(ref TraverseParms __result, Pawn pawn)
        {
            if (IsPrecociousBaby(pawn, out var _))
            {
                __result.canBashDoors = false;
                __result.canBashFences = false;
            }
        }
    }
}
