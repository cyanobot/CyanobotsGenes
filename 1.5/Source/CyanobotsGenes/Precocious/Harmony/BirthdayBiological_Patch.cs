using RimWorld;
using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace CyanobotsGenes
{
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
}
