using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;
using RimWorld;
using static CyanobotsGenes.OffspringUtility;
using System.Reflection;

namespace CyanobotsGenes
{
    [HarmonyPatch]
    public static class Outland_GetInheritedGeneSet_Patch
    {
        public static bool Prepare()
        {
            if (!CG_Mod.outlandGeneticsLoaded) return false;
            return true;
        }

        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(AccessTools.TypeByName("OutlandGenes.PreggoUtil"), "GetInheritedGeneSet",
                new Type[] { typeof(Pawn) });
            yield return AccessTools.Method(AccessTools.TypeByName("OutlandGenes.PreggoUtil"), "GetInheritedGeneSet",
                new Type[] { typeof(Pawn), typeof(bool).MakeByRefType() });
        }

        public static void Postfix(ref GeneSet __result, Pawn mother)
        {
            if (HasActiveOffspringGene(mother))
            {
                XenotypeDef xenotype = GetOffspringXenotype(mother, null);
                if (xenotype == null) return;
                __result.SetNameDirect(xenotype.label);
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
