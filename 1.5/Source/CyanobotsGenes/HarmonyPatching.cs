using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CyanobotsGenes
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatching
    {
        static HarmonyPatching()
        {
            CG_Mod.harmony.PatchAll();
        }
    }


    [HarmonyPatch(typeof(Need_Joy), "FallPerInterval", MethodType.Getter)]
    class Need_Joy_Patch
    {
        static void Postfix(ref float __result, Need_Joy __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue() as Pawn;
            float JoyFallFactor = pawn.GetStatValue(CG_DefOf.JoyFallRateFactor, true, -1);
            __result *= JoyFallFactor;
        }
    }

    [HarmonyPatch(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.TryGainMemory), new Type[] { typeof(Thought_Memory), typeof(Pawn) })]
    class TryGainMemory_Patch
    {
        static bool Prefix(ref Thought_Memory newThought, Pawn otherPawn, Pawn ___pawn, MemoryThoughtHandler __instance)
        {
            //Log.Message("TryGainMemory - newThought: " + newThought.def + ", ___pawn: " + ___pawn);
            if (___pawn.genes == null) return true; ;

            if (newThought.def == ThoughtDefOf.SoakingWet && ___pawn.HasActiveGene(CG_DefOf.LightFur))
            {
                __instance.TryGainMemoryFast(CG_DefOf.WetFur);
                return true;
            }

            if (___pawn.HasActiveGene(CG_DefOf.CYB_Afamilial))
            {
                string defName = newThought.def.defName;
                if (newThought.def == ThoughtDefOf.MyCryingBaby)
                {
                    newThought = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDefOf.CryingBaby);
                    return true;
                }
                else if (defName == "SoldMyLovedOne")
                {
                    if (___pawn.relations.OpinionOf(otherPawn) >= Pawn_RelationsTracker.FriendOpinionThreshold)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            if (___pawn.HasActiveGene(CG_DefOf.CYB_ViolenceAverse))
            {
                string defName = newThought.def.defName;
                if (newThought.def == ThoughtDefOf.WitnessedDeathAlly)
                {
                    newThought = (Thought_Memory)ThoughtMaker.MakeThought(CG_DefOf.CYB_ViolenceAverse_WitnessedDeathAlly);
                    return true;
                }
                else if (newThought.def == ThoughtDefOf.WitnessedDeathNonAlly) 
                {
                    newThought = (Thought_Memory)ThoughtMaker.MakeThought(CG_DefOf.CYB_ViolenceAverse_WitnessedDeathNonAlly);
                    return true;
                }
                else if (defName == "KnowGuestOrganHarvested" || defName == "KnowColonistOrganHarvested")
                {
                    newThought = (Thought_Memory)ThoughtMaker.MakeThought(CG_DefOf.CYB_ViolenceAverse_KnowOrganHarvested);
                    return true;
                }
                else if (newThought.def == ThoughtDefOf.HarmedMe)
                {
                    newThought = (Thought_Memory)ThoughtMaker.MakeThought(CG_DefOf.CYB_ViolenceAverse_HarmedMe);
                    return true;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn_RelationsTracker),nameof(Pawn_RelationsTracker.OpinionOf))]
    public static class OpinionOf_Patch
    {
        public static void Postfix(ref int __result, Pawn ___pawn, Pawn other)
        {
            if (!___pawn.Dead && ___pawn.HasActiveGene(CG_DefOf.CYB_Afamilial))
            {
                foreach (PawnRelationDef relation in ___pawn.GetRelations(other))
                {
                    if (relation.familyByBloodRelation)
                        __result -= relation.opinionOffset;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.OpinionExplanation))]
    public static class OpinionExplanation_Patch
    {
        public static void Postfix(ref string __result, Pawn ___pawn, Pawn other)
        {
            if (!___pawn.Dead && ___pawn.HasActiveGene(CG_DefOf.CYB_Afamilial))
            {
                Log.Message("Editing OpinionExplanation for Afamilial pawn " + ___pawn
                    + ", original result: " + __result);
                foreach (PawnRelationDef relation in ___pawn.GetRelations(other))
                {
                    if (relation.familyByBloodRelation)
                    {
                        int relationLineIndex = __result.IndexOf(relation.GetGenderSpecificLabelCap(other)) - 3;
                        if (relationLineIndex == -1) continue;
                        relationLineIndex -= Environment.NewLine.Length;
                        int relationLineLength = 5 
                            + relation.GetGenderSpecificLabelCap(other).Length 
                            + relation.opinionOffset.ToStringWithSign().Length
                            + Environment.NewLine.Length;
                        Log.Message("Found blood relation: " + relation
                            + ", GenderSpecificLabel: " + relation.GetGenderSpecificLabelCap(other)
                            + ", relationLineIndex: " + relationLineIndex
                            + ", relationLineLength: " + relationLineLength
                            + ", __result.Length: " + __result.Length
                            + ", NewLine.Length: " + Environment.NewLine.Length);

                        if (relationLineIndex + relationLineLength > __result.Length)
                        {
                            continue;
                        }

                        __result = __result.Remove(relationLineIndex, relationLineLength);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(JobDriver_Hunt), "MakeNewToils")]
    class HuntToils_Patch
    {
        public static IEnumerable<Toil> Postfix(IEnumerable<Toil> originalToils, JobDriver_Hunt __instance)
        {
            __instance.job.ignoreJoyTimeAssignment = true;

            Pawn pawn = __instance.pawn;
            bool preyDrive = pawn.genes != null && pawn.HasActiveGene(CG_DefOf.PreyDrive);

            foreach (Toil toil in originalToils)
            {
                if (preyDrive)
                {
                    //Log.Message("Trying to add action to toil " + toil.debugName);
                    toil.AddPreTickAction(delegate
                    {
                        //Log.Message("Fired extra action");
                        JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.None);
                    });
                }
                yield return toil;
            }
        }
    }

    [HarmonyPatch(typeof(JoyUtility),nameof(JoyUtility.JoyTickCheckEnd))]
    class JoyTick_Patch
    {
        static bool Prefix(Pawn pawn)
        {
            //don't terminate hunting jobs or visiting sick pawns if we have no joy need
            if ((pawn.CurJob.def == JobDefOf.Hunt || pawn.CurJob.def == JobDefOf.VisitSickPawn) && pawn.needs.joy == null) return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn_AgeTracker), "GrowthPointsPerDayAtLearningLevel")]
    class GrowthPointsPerDayAtLearningLevel_Patch
    {
        static float Postfix(float result, Pawn ___pawn)
        {
            result *= ___pawn.GetStatValue(CG_DefOf.CYB_GrowthPointFactor);
            return result;
        }
    }

    [HarmonyPatch(typeof(PawnDiedOrDownedThoughtsUtility),"AppendThoughts_ForHumanlike")]
    static public class DiedOrDownedThoughts_AppendThoughts_ForHumanlike_Patch
    {
        static void Postfix(Pawn victim, DamageInfo? dinfo, ref PawnDiedOrDownedThoughtsKind thoughtsKind, ref List<IndividualThoughtToAdd> outIndividualThoughts, List<ThoughtToAddToAll> outAllColonistsThoughts)
        {
            bool isExecution = dinfo.HasValue && dinfo.Value.Def.execution;
            Pawn instigator = dinfo.HasValue ? (Pawn)dinfo.Value.Instigator : null;
            if (instigator != null && !instigator.Dead && instigator.needs.mood != null && instigator.story != null && instigator != victim && PawnUtility.ShouldGetThoughtAbout(instigator, victim))
            {
                if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died)
                {
                    outIndividualThoughts.Add(new IndividualThoughtToAdd(CG_DefOf.CYB_ViolenceAverse_KilledHumanlike, instigator));
                }
            }
            
		    if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Downed)
		    {
			    foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
			    {
				    if (pawn == victim || pawn.needs == null || pawn.needs.mood == null || !PawnUtility.ShouldGetThoughtAbout(pawn, victim))
				    {
					    continue;
				    }
				    if (ThoughtUtility.Witnessed(pawn, victim))
				    {
                        outIndividualThoughts.Add(new IndividualThoughtToAdd(CG_DefOf.CYB_ViolenceAverse_WitnessedDowned, pawn));
				    }
			    }
		    }
            else if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died)
            {
                if (isExecution)
                {
                    outAllColonistsThoughts.Add(new ThoughtToAddToAll(CG_DefOf.CYB_ViolenceAverse_KnowExecuted));
                }
                else if (instigator != null && instigator.Faction == Faction.OfPlayer)
                {
                    outAllColonistsThoughts.Add(new ThoughtToAddToAll(CG_DefOf.CYB_ViolenceAverse_ViolentDeaths));
                }
            }
        }

        [HarmonyPatch(typeof(Pawn_HealthTracker),nameof(Pawn_HealthTracker.PreApplyDamage))]
        public static class PreApplyDamage_Patch
        {
            public static void Postfix(Pawn ___pawn, DamageInfo dinfo, bool absorbed)
            {
                if (___pawn.RaceProps.Humanlike)
                {
                    Pawn instigator = dinfo.Instigator as Pawn;
                    if (instigator != null && instigator.RaceProps.Humanlike && PawnUtility.ShouldGetThoughtAbout(instigator,___pawn))
                    {
                        if (ThoughtUtility.CanGetThought(instigator, CG_DefOf.CYB_ViolenceAverse_Violence))
                            instigator.needs.mood.thoughts.memories.TryGainMemory(CG_DefOf.CYB_ViolenceAverse_Violence);
                    }
                }
            }
        }


    }

    /*
    [HarmonyPatch(typeof(Pawn_AgeTracker),nameof(Pawn_AgeTracker.AgeTick))]
    static public class AgeTick_Patch
    {
        static public void Prefix(Pawn ___pawn, Pawn_AgeTracker __instance)
        {
            if (___pawn.IsHashIntervalTick(60))
            {
                if (___pawn.HasActiveGene(CG_DefOf.CYB_Precocious))
                {
                    List<LifeStageAge> lifeStageAges = ___pawn.RaceProps.lifeStageAges;
                    LifeStageAge lsaChild = lifeStageAges.Find(lsa => lsa.def == LifeStageDefOf.HumanlikeChild);
                    if (lsaChild == null) return;
                    if (__instance.AgeBiologicalYears < lsaChild.minAge)
                    {
                        __instance.LockCurrentLifeStageIndex(lifeStageAges.IndexOf(lsaChild));
                    }
                    else
                    {
                        __instance.LockCurrentLifeStageIndex(-1);
                    }
                }
            }
        }
    }
    */

    [HarmonyPatch(typeof(Pawn),nameof(Pawn.GetReasonsForDisabledWorkType))]
    static public class GetReasonsForDisabledWorkType_Patch
    {
        static public void Postfix(Pawn __instance, ref List<string> __result, WorkTypeDef workType, Dictionary<WorkTypeDef, List<string>> ___cachedReasonsForDisabledWorkTypes)
        {
            if (__instance.genes == null && !__instance.genes.GenesListForReading.NullOrEmpty()) return;
            foreach (Gene gene in __instance.genes.GenesListForReading)
            {
                GeneExtension_DisabledWorkTypes disabledWorkTypes = gene.def.GetModExtension<GeneExtension_DisabledWorkTypes>();
                if (disabledWorkTypes?.workTypes?.Contains(workType) ?? false)
                {
                    string reason = "CYB_WorkDisabledGene".Translate(gene.LabelCap);
                    //__result.Add(reason);
                    if (!___cachedReasonsForDisabledWorkTypes.ContainsKey(workType))
                    {
                        ___cachedReasonsForDisabledWorkTypes.Add(workType, new List<string>());
                    }
                    if (!___cachedReasonsForDisabledWorkTypes[workType].Contains(reason))
                    {
                        ___cachedReasonsForDisabledWorkTypes[workType].Add(reason);
                    }
                    break;
                }
            }
            //___cachedReasonsForDisabledWorkTypes[workType] = __result;
        }

    }

    [HarmonyPatch]
    static public class Gene_DisabledWorkTypes_Patch
    {
        static public MethodInfo TargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(Gene), nameof(Gene.DisabledWorkTypes));
        }

        static public void Postfix(Gene __instance, ref IEnumerable<WorkTypeDef> __result)
        {
            //Log.Message("DisabledWorkTypes Postfix firing");
            GeneExtension_DisabledWorkTypes disabledWorkTypes = __instance.def.GetModExtension<GeneExtension_DisabledWorkTypes>();
            if (!disabledWorkTypes?.workTypes.NullOrEmpty() ?? false)
            {
                foreach (WorkTypeDef workType in disabledWorkTypes.workTypes)
                {
                    //Log.Message("Trying to add disabled work type: " + workType + " for gene " + __instance.def.defName);
                    if (!__result.Contains(workType)) __result = __result.AddItem(workType);
                }
                //Log.Message("Final __result: " + __result.ToStringSafeEnumerable());
            }
        }
    }

    [HarmonyPatch]
    static public class Hediff_CapMods_Patch
    {
        static public MethodInfo TargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(Hediff), nameof(Hediff.CapMods));
        }

        static public void Postfix(Hediff __instance, ref List<PawnCapacityModifier> __result)
        {
            //Log.Message("Firing CapMods Postfix for hediff: " + __instance + ", pawn: " + __instance.pawn);
            //Log.Message("CapMods: " + __result.Select<PawnCapacityModifier, string>(
            //    x => "[capacity: " + x.capacity + ", offset: " + x.offset + ", postFactor: " + x.postFactor + "]"
            //    ).ToStringSafeEnumerable());
            if (__result.NullOrEmpty()) return;
            if (__instance.TryGetComp<HediffComp_CapModsMultipliedBySeverity>(out HediffComp_CapModsMultipliedBySeverity comp))
            {
                //Log.Message("Found comp. Severity: " + __instance.Severity);
                List<PawnCapacityModifier> newResult = new List<PawnCapacityModifier>();
                foreach (PawnCapacityModifier modifier in __result)
                {
                    //Log.Message("modifier:" + StringFromCapMod(modifier));
                    PawnCapacityModifier newModifier = new PawnCapacityModifier
                    {
                        capacity = modifier.capacity,
                        offset = modifier.offset * __instance.Severity,
                        postFactor = StatWorker.ScaleFactor(modifier.postFactor, __instance.Severity)
                    };
                    newResult.Add(newModifier);
                }
                __result = newResult;
                //Log.Message("Final result: " + __result.Select<PawnCapacityModifier, string>(
                //    x => StringFromCapMod(x)
                //    ).ToStringSafeEnumerable());
            }
        }
        
        static public string StringFromCapMod(PawnCapacityModifier capMod)
        {
            return "[capacity: " + capMod.capacity
                + ", offset: " + capMod.offset
                + ", postFactor: " + capMod.postFactor + "]"
                + ", setMax: " + capMod.setMax;
        }
    }

    [HarmonyPatch]
    public static class UnnaturalDarkness_Patch
    {
        public static bool Prepare(MethodBase original)
        {
            if (!ModLister.AnomalyInstalled) return false;
            return true;
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(GameCondition_UnnaturalDarkness), nameof(GameCondition_UnnaturalDarkness.InUnnaturalDarkness));
        }

        public static bool Postfix(bool __result, Pawn p)
        {
            if (p.HasActiveGene(CG_DefOf.CYB_Darkling)) return false;
            return __result;
        }
    }

    [HarmonyPatch(typeof(PawnUtility),nameof(PawnUtility.IsInteractionBlocked))]
    public static class IsInteractionBlocked_Patch
    {
        public static bool Postfix(bool __result, Pawn pawn, InteractionDef interaction, bool isInitiator)
        {
            if (__result) return true;
            if (pawn.genes == null || pawn.genes.GenesListForReading.NullOrEmpty()) return __result;
            foreach (Gene gene in pawn.genes.GenesListForReading)
            {
                if (!gene.Active) continue;
                GeneExtension_BlocksInteractions blocksInteractions = gene.def.GetModExtension<GeneExtension_BlocksInteractions>();
                if (blocksInteractions == null) continue;
                if (isInitiator && blocksInteractions.BlocksAsInitiator(interaction)) return true;
                if (!isInitiator && blocksInteractions.BlocksAsRecipient(interaction)) return true;
            }
            return false;
        }
    }

    [HarmonyPatch]
    public static class PlayLogEntry_Interaction_Patch
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.Constructor(typeof(PlayLogEntry_Interaction), new Type[] { typeof(InteractionDef), typeof(Pawn), typeof(Pawn), typeof(List<RulePackDef>) });
        }

        public static void Postfix(ref InteractionDef ___intDef, InteractionDef intDef, Pawn initiator, Pawn recipient)
        {
            InteractionDef newIntDef;

            if (initiator.genes != null)
            {
                foreach (Gene gene in initiator.genes.GenesListForReading)
                {
                    if (!gene.Active) continue;
                    GeneExtension_ReplacesInteractionRules extension = gene.def.GetModExtension<GeneExtension_ReplacesInteractionRules>();
                    if (extension == null || !extension.AffectedInteractions.Contains(intDef)) continue;
                    newIntDef = extension.ReplacementFor(intDef, true);
                    if (newIntDef != intDef)
                    {
                        ___intDef = newIntDef;
                        return;
                    }
                }
            }
            if (recipient.genes != null)
            {
                foreach (Gene gene in recipient.genes.GenesListForReading)
                {
                    if (!gene.Active) continue;
                    GeneExtension_ReplacesInteractionRules extension = gene.def.GetModExtension<GeneExtension_ReplacesInteractionRules>();
                    if (extension == null || !extension.AffectedInteractions.Contains(intDef)) continue;
                    newIntDef = extension.ReplacementFor(intDef, false);
                    if (newIntDef != intDef)
                    {
                        ___intDef = newIntDef;
                        return;
                    }
                }
            }
        }
    }

    //changelings should only generate as babies/children, fairies only as adults
    //if tried to generate a factionless otherwise, reset to baseliner
    //so that changelings/fairies are not generated above the factionlessGenerationWeight from settings
    [HarmonyPatch(typeof(PawnGenerator),nameof(PawnGenerator.AdjustXenotypeForFactionlessPawn))]
    public static class AdjustXenotypeForFactionlessPawn_Patch
    {
        public static void Postfix(Pawn pawn, ref PawnGenerationRequest request, ref XenotypeDef xenotype)
        {
            if (xenotype == CG_DefOf.CYB_Changeling && pawn.DevelopmentalStage == DevelopmentalStage.Adult)
            {
                xenotype = XenotypeDefOf.Baseliner;
            }
            else if (xenotype == CG_DefOf.CYB_Fairy && pawn.DevelopmentalStage != DevelopmentalStage.Adult)
            {
                xenotype = XenotypeDefOf.Baseliner;
            }
        }
    }

    //if a pawn is being generated as a changeling/fairy by request rather than randomly based on factionlessGenerationWeight
    //then instead set them to the correct version for their developmental stage
    //so that eg factions defined with these xenotypes in produce the correct ones
    [HarmonyPatch(typeof(PawnGenerator), "TryGenerateNewPawnInternal")]
    public static class TryGenerateNewPawnInternal_Patch
    {
        public static void Postfix(ref Pawn __result)
        {
            //Log.Message("TryGenerateNewPawnInternal_Patch Postfix - __result: " + __result + ", Xenotype: " + __result?.genes?.Xenotype);
            if (__result == null) return;
            if (__result.genes?.Xenotype == CG_DefOf.CYB_Changeling && __result.DevelopmentalStage == DevelopmentalStage.Adult)
            {
                if (!__result.genes.Endogenes.NullOrEmpty())
                { 
                    foreach (Gene gene in __result.genes.Endogenes.ToList())
                    {
                        if (gene.def.endogeneCategory == EndogeneCategory.Melanin || (gene.Active && gene.def.endogeneCategory == EndogeneCategory.HairColor))
                        {
                            continue;
                        }
                        __result.genes.RemoveGene(gene);
                    }
                }
                __result.genes.SetXenotype(CG_DefOf.CYB_Fairy);
            }
            else if (__result.genes?.Xenotype == CG_DefOf.CYB_Fairy && __result.DevelopmentalStage != DevelopmentalStage.Adult)
            {
                if (!__result.genes.Endogenes.NullOrEmpty())
                {
                    foreach (Gene gene in __result.genes.Endogenes.ToList())
                    {
                        if (gene.def.endogeneCategory == EndogeneCategory.Melanin)
                        {
                            continue;
                        }
                        __result.genes.RemoveGene(gene);
                    }
                }
                __result.genes.SetXenotype(CG_DefOf.CYB_Changeling);
                __result.story.hairDef = PawnStyleItemChooser.RandomHairFor(__result);
            }
        }
    }

    [HarmonyPatch(typeof(GeneDef), "GetDescriptionFull")]
    public static class GeneDef_Description_Patch
    {
        public static string Postfix(string __result)
        {
            List<string> lines = __result.Split(new string[] { Environment.NewLine },StringSplitOptions.None).ToList();
            List<string> newLines = new List<string>();

            int thoughtCount = 0;
            string thoughtStart = "  - " + "Removes".Translate() + ": ";
            int insertionIndex = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                if (line.StartsWith(thoughtStart))
                {
                    thoughtCount++;
                    if (thoughtCount > 3)
                    {
                        continue;
                    }
                    else if (thoughtCount == 3)
                    {
                        insertionIndex = i+1;
                    }
                }
                newLines.Add(line);
            }

            if (thoughtCount > 3)
            {
                string newString = thoughtStart + (thoughtCount - 3) + " " + "MoreLower".Translate() + "...";
                newLines.Insert(insertionIndex, newString);
            }

            StringBuilder sb = new StringBuilder();
            foreach(string line in newLines)
            {
                sb.AppendLine(line);
            }

            return sb.ToString().TrimEndNewlines();
        }
    }

}
