﻿using System;
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
        [HarmonyAfter("com.VEHighmate")]
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

        /*
        [HarmonyPatch]
        public static class AlphaGenes_HediffComp_Parasites_Hatch
        {
            public static bool Prepare()
            {
                if (!CG_Mod.alphaGenesLoaded) return false;
                return true;
            }

            public  static MethodBase TargetMethod()
            {
                return AccessTools.Method(AccessTools.TypeByName("AlphaGenes.HediffComp_Parasites"), "Hatch");
            }

            static MethodInfo m_TrySpawnHatchedOrBornPawn = AccessTools.Method(typeof(PawnUtility), nameof(PawnUtility.TrySpawnHatchedOrBornPawn));
            static FieldInfo f_mother = AccessTools.Field(AccessTools.TypeByName("AlphaGenes.HediffComp_Parasites"), "mother");
            static FieldInfo f_endogenes= AccessTools.Field(AccessTools.TypeByName("AlphaGenes.HediffComp_Parasites"), "endogenes");
            static MethodInfo m_ShouldRun = AccessTools.Method(typeof(AlphaGenes_HediffComp_Parasites_Hatch), nameof(AlphaGenes_HediffComp_Parasites_Hatch.ShouldRun));
            static MethodInfo m_ApplyOffspringXenotype = AccessTools.Method(typeof(AlphaGenes_HediffComp_Parasites_Hatch), nameof(AlphaGenes_HediffComp_Parasites_Hatch.ApplyOffspringXenotype));

            public static bool ShouldRun(Pawn progenitor, bool endogenes)
            {
                if (endogenes)
                {
                    if (!offspringAffects_AG_ParasiticEndogenes) return false;
                }
                else
                {
                    if (!offspringAffects_AG_ParasiticXenogenes) return false;
                }

                if (HasActiveOffspringGene(progenitor)) return true;
                return false;
            }

            public static bool ApplyOffspringXenotype(Pawn progenitor, Pawn pawnCreated, bool endogenes)
            {
                XenotypeDef xenotype = GetOffspringXenotype(progenitor, null);
                if (xenotype == null || pawnCreated.genes == null) return false;

                if (endogenes)

                return true;
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
            {
                bool finished = false;
                int generatedPawnIndex = 1;         //not future-proof but I can't figure out how to work it out dynamically
                bool foundTrySpawn = false;
                bool foundBrToEnd = false;
                object brToEndOperand = null;

                Label endOfCustomCode = iLGenerator.DefineLabel();

                CodeInstruction[] instructions_array = instructions.ToArray();

                for (int i = 0; i < instructions_array.Length; i++)
                {
                    CodeInstruction cur = instructions_array[i];
                    yield return cur;

                    if (!finished)
                    {
                        if (!foundTrySpawn)
                        {
                            if (cur.Calls(m_TrySpawnHatchedOrBornPawn))
                            {
                                foundTrySpawn = true;
                            }
                        }
                        else if (!foundBrToEnd)
                        {
                            if (cur.opcode == OpCodes.Brfalse || cur.opcode == OpCodes.Brfalse_S)
                            {
                                foundBrToEnd = true;
                                brToEndOperand = cur.operand;
                            }
                        }
                        else
                        {
                            CodeInstruction prev = instructions_array[i - 1];
                            if (prev.opcode == OpCodes.Newobj && prev.operand is ConstructorInfo constructorInfo && constructorInfo == AccessTools.Constructor(typeof(List<GeneDef>)))
                            {
                                LogUtil.DebugLog("found newobj List<GeneDef>");

                                //if (ShouldRun(mother, endogenes))
                                yield return new CodeInstruction(OpCodes.Ldarg_0);      //get HediffComp instance
                                yield return new CodeInstruction(OpCodes.Ldfld, f_mother);      //get value of field mother

                                yield return new CodeInstruction(OpCodes.Ldarg_0);      //get HediffComp  instance
                                yield return new CodeInstruction(OpCodes.Ldfld, f_endogenes);   //get value of field endogenes

                                yield return new CodeInstruction(OpCodes.Call, m_ShouldRun);
                                yield return new CodeInstruction(OpCodes.Brfalse, endOfCustomCode);

                                //ApplyOffspringXenotype(mother,pawnCreated,endogenes)
                                yield return new CodeInstruction(OpCodes.Ldarg_0);      //get HediffComp instance
                                yield return new CodeInstruction(OpCodes.Ldfld, f_mother);      //get value of field mother

                                yield return new CodeInstruction(OpCodes.Ldloc, generatedPawnIndex);    //get generated pawn

                                yield return new CodeInstruction(OpCodes.Ldarg_0);      //get HediffComp  instance
                                yield return new CodeInstruction(OpCodes.Ldfld, f_endogenes);   //get value of field endogenes

                                yield return new CodeInstruction(OpCodes.Call, m_ApplyOffspringXenotype);
                                yield return new CodeInstruction(OpCodes.Brtrue, brToEndOperand);       //if successful, skip original gene-granting code

                                CodeInstruction next = instructions_array[i + 1];
                                next.labels.Add(endOfCustomCode);

                                finished = true;
                            }
                        }
                    }
                }
            }
        }
        */
    }