using RimWorld;
using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace CyanobotsGenes
{
    [HarmonyPatch(typeof(Pawn_AgeTracker), "BirthdayBiological")]
    public static class BirthdayBiological_Patch
    {
        public static MethodInfo m_RecalculateLifeStageIndex = AccessTools.Method(typeof(Pawn_AgeTracker),
            "RecalculateLifeStageIndex", 
            new Type[] { });
        public static MethodInfo m_Child = AccessTools.Method(typeof(DevelopmentalStageExtensions),
            nameof(DevelopmentalStageExtensions.Child), 
            new Type[] { typeof(DevelopmentalStage) });
        
        public static MethodInfo m_IsCrossingChildLifeStageBoundary = AccessTools.Method(typeof(BirthdayBiological_Patch), 
            nameof(BirthdayBiological_Patch.IsCrossingChildLifeStageBoundary));

        //function that will replace the normal calculations of "flag2"
        //bool flag2 = pawn.DevelopmentalStage.Child() && (float)birthdayAge == CurLifeStageRace.minAge;
        public static bool IsCrossingChildLifeStageBoundary(Pawn pawn, int birthdayAge)
        {
            //relying on pawn.DevelopmentalStage and CurLifeStageRace means that results can be inconsistent
            //depending on whether this is a growth tick or not
            //and therefore whether RecalculateLifeStageIndex has already been called or not

            //so let's start by calling RecalculateLifeStageIndex to make sure we're across the line into the new lifestage
            m_RecalculateLifeStageIndex.Invoke(pawn.ageTracker, new object[] { });

            //check whether now a child or not first, because that's quick
            if (!pawn.DevelopmentalStage.Child()) return false;

            //next check for precocious gene
            Gene_Precocious gene_Precocious = (Gene_Precocious)pawn.genes?.GetGene(CG_DefOf.CYB_Precocious);
            
            //if no precocious gene, just do the other half of the vanilla check
            if (gene_Precocious == null || !gene_Precocious.Active)
            {
                return (float)birthdayAge == pawn.ageTracker.CurLifeStageRace.minAge;
            }
            //if precocious gene, we will never be crossing the boundary because we start as a child
            else
            {
                return false;
            }
        }

        public static void Prepare()
        {
            LogUtil.DebugLog("m_RecalculateLifeStageIndex: " + m_RecalculateLifeStageIndex
                + ", m_Child: " + m_Child
                + ", m_IsCrossingChildLifeStageBoundary: " + m_IsCrossingChildLifeStageBoundary
                );
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructions = codeInstructions.ToList();
            
            bool startedPatch = false;
            bool finishedPatch = false; 
            
            for (int i = 0; i < instructions.Count; i++)
            {
                CodeInstruction cur = instructions[i];

                //if we've finished tampering, just pass through original code
                if (finishedPatch)
                {
                    yield return cur;
                }
                
                //if we've started tampering but not yet finished
                //do not yield original instructions
                //check to see if we've finished yet
                else if (startedPatch)
                {
                    //we're going to detect the end of the flag2 calculation by looking for a stloc
                    //because that's writing to the flag2 variable
                    if (cur.IsStloc())
                    {
                        finishedPatch = true;

                        //do want to return that stloc to put our flag2 value into it
                        yield return cur;
                    }
                }

                //if we've not yet started tampering
                //check to see if we should start
                //if so, insert our own code
                //if not, return original instruction
                else
                {
                    //detect start of flag2 calculation by invocation of DevelopmentalStage.Child()
                    //need to start one line ahead
                    //because we also don't want to call pawn.DevelopmentalStage
                    CodeInstruction ahead1 = instructions[i + 1];
                    if (ahead1.Calls(m_Child))
                    {
                        startedPatch = true;

                        //pawn will be on the top of the stack
                        //because the code was about to call pawn.DevelopmentalStage
                        //we also want to load birthdayAge onto the stack
                        //arg0 is the Pawn_AgeTracker instance bc this is an instance method
                        //so we want arg1 for birthdayAge
                        yield return new CodeInstruction(OpCodes.Ldarg_1);

                        //we're now set to invoke our new IsCrossingChildLifeStageBoundary function
                        yield return new CodeInstruction(OpCodes.Call, m_IsCrossingChildLifeStageBoundary);

                        //and we want to leave that bool on top of the stack so we're done here
                    }
                    else
                    {
                        yield return cur;
                    }
                }
            }
        }

    }
}
