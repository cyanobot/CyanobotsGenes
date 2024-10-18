using RimWorld;
using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using static CyanobotsGenes.PrecociousUtil;
using System;
using Verse.AI;

namespace CyanobotsGenes
{
    //make sure precocious babies never initiate violence
    //for some reason being incapable of it is insufficient 
    [HarmonyPatch]
    public static class CombatJobGiver_MultiPatch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (Type type in new Type[] {
                typeof(JobGiver_AIFightEnemy),
                typeof(JobGiver_AIGotoNearestHostile),
                typeof(JobGiver_AISapper),
                typeof(JobGiver_AIWaitAmbush),
                typeof(JobGiver_ManTurrets)
            })
            {
                MethodInfo method = type.GetMethod("TryGiveJob", BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null) yield return method;
            }
        }

        static Job Postfix(Job __result, Pawn pawn)
        {
            if (IsPrecociousBaby(pawn, out var  _)) return null;
            return __result;
        }
    }
}
