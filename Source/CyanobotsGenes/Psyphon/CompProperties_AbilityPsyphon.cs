using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using static CyanobotsGenes.PsyphonUtility;

namespace CyanobotsGenes
{
    public class CompProperties_AbilityPsyphon : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityPsyphon()
        {
            compClass = typeof(CompAbilityEffect_Psyphon);
        }
    }

    public class CompAbilityEffect_Psyphon : CompAbilityEffect
    {

        public new CompProperties_AbilityPsyphon Props => (CompProperties_AbilityPsyphon)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn actor = parent.pawn;
            Pawn targetPawn = target.Pawn;
            if (targetPawn == null) return;

            if (actor.psychicEntropy == null)
            {
                //failure due to actor lacking psychic entropy tracker
                Log.Error("[Cyanobot's Genes] Psyphon ability used by pawn " + actor + " but " + actor + "has no psychic entropy tracker.");
                return;
            }

            float desiredPsyfocus = DesiredPsyfocus(actor);
            LogUtil.DebugLog("Psyphon.Apply - desiredPsyfocus: " + desiredPsyfocus);

            //psychic sensitivity of target
            float targetSensitivity = targetPawn.GetStatValue(StatDefOf.PsychicSensitivity);
            if (targetSensitivity <= 0)
            {
                //failure due to 0 psychic sensitivity
                Log.Error("[Cyanobot's Genes] Psyphon ability used on pawn " + targetPawn + " but " + targetPawn + " has no psychic sensitivity and should not have been a valid target.");
                return;
            }

            float remainingToDrain = desiredPsyfocus;

            //offset from target psyfocus first before causing harmful effects
            float psyfocusToDrain = PsyfocusToDrain(targetPawn, remainingToDrain, out float psyfocusConversionFactor);
            if (psyfocusToDrain > 0f)
            {
                targetPawn.psychicEntropy.OffsetPsyfocusDirectly(-1f * psyfocusToDrain);
                remainingToDrain -= psyfocusToDrain * psyfocusConversionFactor;
            }
            
            if (remainingToDrain > 0f)
            {
                float consciousnessToDrain = ConsciousnessToDrain(targetPawn, remainingToDrain);
                LogUtil.DebugLog("consciousnessToDrain: " + consciousnessToDrain);

                ApplyNegativeEffects(actor, targetPawn, consciousnessToDrain);
                remainingToDrain -= consciousnessToDrain * BaseConsciousnessConversionFactor;
            }

            actor.psychicEntropy.OffsetPsyfocusDirectly(desiredPsyfocus - remainingToDrain);

        }


        public static void ApplyNegativeEffects(Pawn actor, Pawn victim, float consciousnessDrained)
        {
            BodyPartRecord brain = victim.health.hediffSet.GetBrain();
            /*
            if (brain == null)
            {
                //fail due to no brain
                Log.Error("[Cyanobot's Genes] Psyphon ability used on pawn " + victim + " but " + victim + " has no brain!");
                return;
            }
            */

            //apply brain damage
            if (consciousnessDrained > BrainDamageThreshold
                && Rand.Chance(brainDamageChanceFromDrainCurve.Evaluate(consciousnessDrained)))
            {
                float amount = GetBrainDamage(consciousnessDrained);
                victim.TakeDamage(new DamageInfo(DamageDefOf.Psychic, amount, 0f, -1f, null, brain));
            }
            if (victim.Dead) return;

            //apply hediff
            Hediff psychicDrain = victim.health.GetOrAddHediff(CG_DefOf.CYB_PsycheDrained, brain);
            psychicDrain.Severity += consciousnessDrained;

            if (victim.Dead) return;

            //chance of losing passion
            if (consciousnessDrained > PersonalityDamageThreshold)
            {
                if (victim.skills != null
                    && victim.skills.skills.Any(s => s.passion > Passion.None)
                    && Rand.Chance(personalityDamageChanceFromDrainCurve.Evaluate(consciousnessDrained)))
                {
                    List<SkillRecord> passionSkills = victim.skills.skills.Where(s => s.passion > Passion.None).ToList();
                    SkillRecord targetSkill = passionSkills.RandomElement();
                    targetSkill.passion -= 1;

                    LogUtil.DebugLog($"Attempting to remove passion from targetSkill: {targetSkill}");

                    string letterText = "CYB_LetterText_PsyphonRemovedPassion".Translate().Formatted(victim.Named("PAWN"), targetSkill.def.Named("SKILL"));
                    Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter("CYB_LetterLabel_PsyphonRemovedPassion".Translate(), letterText, LetterDefOf.NegativeEvent));
                }
            }

            //chance of becoming psychically dead
            if (ModLister.AnomalyInstalled 
                && consciousnessDrained > PsychicallyDeadThreshold
                && Rand.Chance(psychicallyDeadChanceFromDrainCurve.Evaluate(consciousnessDrained))) 
            {
                Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.PsychicallyDead, victim);
                victim.health.AddHediff(hediff);

                string letterText = "CYB_LetterText_PsyphonCausedPsychicallyDead".Translate().Formatted(victim.Named("PAWN"));
                Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter("CYB_LetterLabel_PsyphonCausedPsychicallyDead".Translate(), letterText, LetterDefOf.NegativeEvent));
            }

            //add thoughts/etc
            victim.needs?.mood?.thoughts.memories?.TryGainMemory(CG_DefOf.CYB_Psyphon_Opinion, actor);
            TaleRecorder.RecordTale(CG_DefOf.CYB_TalePsyphon, new object[] { actor, victim });
        }

        public override bool ShouldHideGizmo => !parent.pawn.HasPsylink;

        public override bool GizmoDisabled(out string reason)
        {
            if (parent.pawn.psychicEntropy.PsychicSensitivity < float.Epsilon)
            {
                reason = "CommandPsycastZeroPsychicSensitivity".Translate();
                return true;
            }
            if (parent.pawn.psychicEntropy.TargetPsyfocus - parent.pawn.psychicEntropy.CurrentPsyfocus <= 0f)
            {
                reason = "CYB_AbilityNoPsyfocusWanted".Translate();
                return true;
            }
            return base.GizmoDisabled(out reason);
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (!base.CanApplyOn(target, dest)) return false;
            if (Valid(target))
            {
                return true;
            }
            return false;
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!base.Valid(target, throwMessages)) return false;

            Pawn targetPawn = target.Pawn;
            if (targetPawn == null) return false;
            if (!ValidTarget(targetPawn,out string reason))
            {
                if (throwMessages)
                {
                    Messages.Message(reason, targetPawn, MessageTypeDefOf.RejectInput, false);
                }
                return false;
            }
            if (WouldResistPsyphon(parent.pawn, targetPawn))
            {
                if (throwMessages)
                {
                    Messages.Message("MessageCantUseOnResistingPerson".Translate(parent.def.Named("ABILITY")), targetPawn, MessageTypeDefOf.RejectInput, historical: false);
                }
                return false;
            }
            return true;
        }


        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            Pawn targetPawn = target.Pawn;
            if (targetPawn != null)
            {
                string text = null;
                if (WouldResistPsyphon(parent.pawn,targetPawn))
                {
                    text += "MessageCantUseOnResistingPerson".Translate(parent.def.Named("ABILITY"));
                }
                if (WouldKill(targetPawn,DesiredPsyfocus(parent.pawn)))
                {
                    if (!text.NullOrEmpty())
                    {
                        text += "\n";
                    }
                    text += "WillKill".Translate();
                }
                else if (MightKill(targetPawn, DesiredPsyfocus(parent.pawn)))
                {
                    if (!text.NullOrEmpty())
                    {
                        text += "\n";
                    }
                    text += "CYB_MightKill".Translate();
                }
                else if (MightCauseBrainDamage(targetPawn, DesiredPsyfocus(parent.pawn)))
                {
                    if (!text.NullOrEmpty())
                    {
                        text += "\n";
                    }
                    text += "CYB_MightCauseBrainDamage".Translate();
                }
                return text;
            }
            return base.ExtraLabelMouseAttachment(target);
        }

        public override Window ConfirmationDialog(LocalTargetInfo target, Action confirmAction)
        {
            Pawn targetPawn = target.Pawn;
            if (targetPawn != null)
            {
                if (targetPawn.genes != null && targetPawn.genes.HasActiveGene(GeneDefOf.Deathless))
                {
                    return null;
                }
                if (WouldKill(targetPawn, DesiredPsyfocus(parent.pawn)))
                {
                    return Dialog_MessageBox.CreateConfirmation("CYB_Warning_PawnWillDieFromPsyphon".Translate(targetPawn.Named("PAWN")), confirmAction, destructive: true);
                }
                else if (MightKill(targetPawn, DesiredPsyfocus(parent.pawn)))
                {
                    return Dialog_MessageBox.CreateConfirmation("CYB_Warning_PawnMightDieFromPsyphon".Translate(targetPawn.Named("PAWN")), confirmAction, destructive: true);
                }
            }
            return null;
        }
    }
}
