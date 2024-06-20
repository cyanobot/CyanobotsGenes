using RimWorld;
using RimWorld.QuestGen;
using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static CyanobotsGenes.CG_Util;
using static CyanobotsGenes.PrecociousUtil;
using System.Reflection.Emit;
using UnityEngine;
using System;
using Verse.AI;

namespace CyanobotsGenes
{

    [HarmonyPatch]
    public static class CurLifeStage_Patch
    {

        public static FieldInfo f_pawn = AccessTools.Field(typeof(Pawn_AgeTracker), "pawn");

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(LifeStageUtility), nameof(LifeStageUtility.PlayNearestLifestageSound));
            yield return AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.BodySize));
            yield return AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.HealthScale));
            yield return AccessTools.Method(typeof(HumanlikeMeshPoolUtility), nameof(HumanlikeMeshPoolUtility.HumanlikeBodyWidthForPawn));
            yield return AccessTools.Method(typeof(HumanlikeMeshPoolUtility), nameof(HumanlikeMeshPoolUtility.HumanlikeHeadWidthForPawn));
            yield return AccessTools.Method(typeof(PawnRenderer), "GetDrawParms");
            yield return AccessTools.Method(typeof(PawnRenderNodeWorker_Eye), nameof(PawnRenderNodeWorker_Eye.ScaleFor));
            yield return AccessTools.Method(typeof(HumanlikeMeshPoolUtility), nameof(HumanlikeMeshPoolUtility.GetHumanlikeHairSetForPawn));
            yield return AccessTools.Method(typeof(HumanlikeMeshPoolUtility), nameof(HumanlikeMeshPoolUtility.GetHumanlikeBeardSetForPawn));
            yield return AccessTools.Method(typeof(PawnTweener), "TweenedPosRoot");
        }

        public static LifeStageDef GetLifeStageDef(Pawn_AgeTracker ageTracker)
        {
            Pawn pawn = (Pawn)f_pawn.GetValue(ageTracker);
            if (!IsPrecociousBaby(pawn, out Gene_Precocious gene_Precocious)) return ageTracker.CurLifeStage;
            //Log.Message("returning life stage: " + gene_Precocious.Lsa0.def);
            return gene_Precocious.Lsa0.def;
        }
        public static MethodInfo m_get_CurLifeStage = AccessTools.PropertyGetter(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.CurLifeStage));
        public static MethodInfo m_getLifeStageDef = AccessTools.Method(typeof(CurLifeStage_Patch), nameof(CurLifeStage_Patch.GetLifeStageDef));

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list_instructions = instructions.ToList();
            int count = list_instructions.Count;
            for (int i = 0; i < count; i++)
            {
                CodeInstruction cur = list_instructions[i];

                //if current line gets CurLifeStage
                //call custom method instead
                if (cur.Calls(m_get_CurLifeStage))
                {
                    //call function
                    yield return new CodeInstruction(OpCodes.Call, m_getLifeStageDef);
                    continue;
                }

                yield return cur;
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderNode_Hair),nameof(PawnRenderNode_Hair.GraphicFor))]
    public static class Hair_GraphicFor_Patch
    {
        public static void Postfix(ref Graphic __result, Pawn pawn)
        {
            if (IsPrecociousBaby(pawn, out var _)) __result = null;
        }
    }

    [HarmonyPatch(typeof(GrowthUtility),nameof(GrowthUtility.IsGrowthBirthday))]
    public static class IsGrowthBirthday_Patch
    {
        public static bool Postfix(bool result, int age)
        {
            //Log.Message("IsGrowthBirthday_Patch, age: " + age);
            if (age == 3) return true;
            return result;
        }
    }

    [HarmonyPatch(typeof(Pawn_AgeTracker), "BirthdayBiological")]
    public static class BirthdayBiological_Patch
    {
        public static void Postfix(Pawn ___pawn, Pawn_AgeTracker __instance, int birthdayAge)
        {
            if (birthdayAge == 3 && ___pawn.HasActiveGene(CG_DefOf.CYB_Precocious))
            {
                __instance.TryChildGrowthMoment(birthdayAge, out var newPassionOptions, out var newTraitOptions, out var passionGainsCount);
                if (!PawnUtility.ShouldSendNotificationAbout(___pawn) || ___pawn.Faction != Faction.OfPlayer || ___pawn.IsQuestLodger())
                {
                    if (passionGainsCount > 0)
                    {
                        SkillDef skillDef = ChoiceLetter_GrowthMoment.PassionOptions(___pawn, passionGainsCount, checkGenes: true).FirstOrFallback();
                        if (skillDef != null)
                        {
                            SkillRecord skill = ___pawn.skills.GetSkill(skillDef);
                            if (skill != null)
                            {
                                skill.passion = skill.passion.IncrementPassion();
                            }
                        }
                    }
                    if (newTraitOptions > 0)
                    {
                        Trait trait = PawnGenerator.GenerateTraitsFor(___pawn, 1, null, growthMomentTrait: true).FirstOrFallback();
                        if (trait != null)
                        {
                            ___pawn.story.traits.GainTrait(trait);
                        }
                    }
                }
                else
                {
                    TaggedString text = "LetterBirthdayBiological".Translate(___pawn, birthdayAge);
                    if (___pawn.Spawned)
                    {
                        EffecterDefOf.Birthday.SpawnAttached(___pawn, ___pawn.Map);
                    }
                    Name name = ___pawn.Name;
                    if (___pawn.Name is NameTriple nameTriple && !nameTriple.NickSet)
                    {
                        Rand.PushState(Gen.HashCombine(___pawn.thingIDNumber, birthdayAge));
                        try
                        {
                            if (Rand.Chance(0.5f))
                            {
                                string name2 = PawnNameDatabaseShuffled.BankOf(PawnNameCategory.HumanStandard).GetName(PawnNameSlot.Nick, ___pawn.gender);
                                ___pawn.Name = new NameTriple(nameTriple.First, name2, nameTriple.Last);
                            }
                        }
                        finally
                        {
                            Rand.PopState();
                        }
                    }
                    LetterDef letterDef = LetterDefOf.ChildBirthday;
                    ChoiceLetter_GrowthMoment choiceLetter_GrowthMoment = (ChoiceLetter_GrowthMoment)LetterMaker.MakeLetter(letterDef);
                    List<string> enabledWorkTypes = ___pawn.RaceProps.lifeStageWorkSettings
                        .Where(x => x.minAge == birthdayAge)
                        .Select(x => x.workType.labelShort.CapitalizeFirst()).ToList();
                    choiceLetter_GrowthMoment.ConfigureGrowthLetter(___pawn, newPassionOptions, newTraitOptions, passionGainsCount, enabledWorkTypes, name);
                    choiceLetter_GrowthMoment.Label = "BirthdayGrowthMoment".Translate(___pawn, name.ToStringShort.Named("PAWNNAME"));
                    choiceLetter_GrowthMoment.StartTimeout(120000);
                    __instance.canGainGrowthPoints = false;
                    Find.LetterStack.ReceiveLetter(choiceLetter_GrowthMoment);
                }

            }
        }
    }

    /*
    [HarmonyPatch(typeof(Pawn_AgeTracker),nameof(Pawn_AgeTracker.TryChildGrowthMoment))]
    public static class TryChildGrowthMoment_Patch
    {
        public static bool Prefix(Pawn ___pawn, int birthdayAge, ref int newPassionOptions, ref int newTraitOptions, ref int passionGainsCount)
        {
            Log.Message("TryChildGrowthMoment_Patch - pawn: " + ___pawn + ", age: " + birthdayAge);
            if (birthdayAge == 3 && !___pawn.HasActiveGene(CG_DefOf.CYB_Precocious))
            {
                Log.Message("3 but not precocious");
                newPassionOptions = 0;
                newTraitOptions = 0;
                passionGainsCount = 0;
                return false;
            }
            return true;
        }

        public static void Postfix(Pawn ___pawn, int newPassionOptions, int newTraitOptions, int passionGainsCount)
        {
            Log.Message("Postfix - pawn: " + ___pawn + ", passionOptions: " + newPassionOptions
                + ", traitOptions: " + newTraitOptions + ", passionCount: " + passionGainsCount);
        }
    }
    */

    [HarmonyPatch]
    public static class GrowthPointsFactor_Patch
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(Pawn_AgeTracker), "GrowthPointsFactor");
        }
        public static float Postfix(float result, Pawn_AgeTracker __instance)
        {
            if ((float)__instance.AgeBiologicalYears < 3f)
            {
                result /= 0.75f;
            }
            return result;
        }
    }

    [HarmonyPatch(typeof(QuestNode_Root_RefugeePodCrash_Baby),nameof(QuestNode_Root_RefugeePodCrash_Baby.SendLetter))]
    public static class RefugeePodCrash_Baby_SendLetter_Patch
    {
        public static bool Prefix(Quest quest, Pawn pawn)
        {
            if (pawn.HasActiveGene(CG_DefOf.CYB_Precocious))
            {
                //TaggedString title = "PRECOCIOUS";
                TaggedString title = "LetterLabelRefugeePodCrash".Translate();
                TaggedString letterText = "CYB_LetterText_RefugeePodCrashPrecociousBaby".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);

                if (QuestGen.slate.Get("hasParent", defaultValue: false))
                {
                    letterText += "\n\n" + "RefugeePodCrashBabyHasParent".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
                }
                QuestNode_Root_WandererJoin_WalkIn.AppendCharityInfoToLetter("JoinerCharityInfo".Translate(pawn), ref letterText);
                PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref letterText, ref title, pawn);
                Find.LetterStack.ReceiveLetter(title, letterText, LetterDefOf.NeutralEvent, new TargetInfo(pawn));

                return false;
            }
            return true;
        }
    }

    /*
    [HarmonyPatch(typeof(QuestNode_Root_RefugeePodCrash_Baby), "AddSpawnPawnQuestParts")]
    public static class RefugeePodCrash_Baby_AddSpawnPawnQuestParts_Patch
    {
        public static void Postfix(Pawn pawn)
        {
            if (IsPrecociousBaby(pawn, out var _))
            {
                pawn.SetFaction(null);
            }
        }

    }
    */


    //under-threes are incapable of violence even if precocious
    [HarmonyPatch(typeof(Pawn),nameof(Pawn.CombinedDisabledWorkTags), MethodType.Getter)]
    public static class CombinedDisabledWorkTags_Patch
    {
        public static void Postfix(Pawn __instance, ref WorkTags __result)
        {
            //Log.Message("Patching CombinedDisabledWorkTags - pawn: " + __instance);
            if (IsPrecociousBaby(__instance, out var _))
            {
                __result |= WorkTags.Violent;
            }
        }
    }

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

    //precocious babies should not count as threats even if from hostile factions
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.ThreatDisabled))]
    public static class ThreatDisabled_Patch
    {
        static bool Postfix(bool result, Pawn __instance)
        {
            if (result) return true;
            if (IsPrecociousBaby(__instance, out var _)) return true;
            return false;
        }
    }

    //precocious babies are adoptable
    [HarmonyPatch(typeof(Pawn),nameof(Pawn.AdoptableBy))]
    public static class AdoptableBy_Patch
    {
        static bool Postfix(bool __result, Pawn __instance, Faction by)
        {
            if (__result == true) return true;
            if (IsPrecociousBaby(__instance, out var _))
            {
                if (__instance.Faction == by)
                {
                    return false;
                }
                if (__instance.FactionPreventsClaimingOrAdopting(__instance.Faction, forClaim: false))
                {
                    return false;
                }
                return true;
            }
            return __result;
        }
    }

    //precocious babies are arrestable
    [HarmonyPatch(typeof(GenAI), nameof(GenAI.CanBeArrestedBy))]
    public static class CanBeArrestedBy_Patch
    {
        static bool Postfix(bool __result, Pawn pawn)
        {
            if (__result == true) return true;
            if (IsPrecociousBaby(pawn, out var _))
            {
                if (pawn.IsMutant)
                {
                    return false;
                }
                if (pawn.IsPrisonerOfColony && pawn.Position.IsInPrisonCell(pawn.MapHeld))
                {
                    return false;
                }
                if (ModsConfig.AnomalyActive && Find.Anomaly.IsPawnHypnotized(pawn))
                {
                    return false;
                }
                return true;
            }
            return __result;
        }
    }

    //no positive thought for releasing precocious prisoners
    //to prevent farming
    //and also because how well are they really going to do if kicked out into the wild?
    [HarmonyPatch(typeof(GenGuest),nameof(GenGuest.AddHealthyPrisonerReleasedThoughts))]
    public static class AddHealthyPrisonerReleasedThoughts_Patch
    {
        public static bool Prefix(Pawn prisoner)
        {
            if (IsPrecociousBaby(prisoner, out var _))
            {
                return false;
            }
            return true;
        }
    }

    //no relation gain for releasing precocious prisoners
    //to prevent farming
    //and also because how well are they really going to do if kicked out into the wild?
    [HarmonyPatch(typeof(Faction),nameof(Faction.GetGoodwillGainForExit))]
    public static class GetGoodwillGainForExit_Patch
    {
        public static void Postfix(ref int __result, Pawn member, bool freed)
        {
            if (IsPrecociousBaby(member, out var _) && freed)
            {
                __result = 0;
            }
        }
    }

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
