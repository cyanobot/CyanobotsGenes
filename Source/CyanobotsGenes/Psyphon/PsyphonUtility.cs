using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace CyanobotsGenes
{
    [Flags]
    public enum PsyphonTargetStatus
    {
        None = 0,
        Colonist = 1,
        Slave = 2,
        Prisoner = 4
    }

    public static class PsyphonUtility
    {
        public const float BasePsyfocusConversionFactor = 0.5f;
        public const float BaseConsciousnessConversionFactor = 2f;
        public const float BrainDamageThreshold = 0.15f;
        public const float PersonalityDamageThreshold = 0.3f;
        public const float PsychicallyDeadThreshold = 0.4f;

        public static FloatRange brainDamageVariance = new FloatRange(0.5f, 1.5f);

        public static SimpleCurve brainDamageChanceFromDrainCurve = new SimpleCurve()
        {
            new CurvePoint(0f,0f),
            new CurvePoint(BrainDamageThreshold,0.2f),
            new CurvePoint(1f,1f)
        };
        public static SimpleCurve brainDamageFromDrainCurve = new SimpleCurve()
        {
            new CurvePoint(0f,0f),
            new CurvePoint(BrainDamageThreshold,0.5f),
            new CurvePoint(1f,5f)
        };
        public static SimpleCurve personalityDamageChanceFromDrainCurve = new SimpleCurve()
        {
            new CurvePoint(0f,0f),
            new CurvePoint(PersonalityDamageThreshold,0.2f),
            new CurvePoint(1f,0.8f)
        };
        public static SimpleCurve psychicallyDeadChanceFromDrainCurve = new SimpleCurve()
        {
            new CurvePoint(0f,0f),
            new CurvePoint(PsychicallyDeadThreshold,0.05f),
            new CurvePoint(1f,0.1f)
        };

        public static float DesiredPsyfocus(Pawn actor)
        {
            return actor.psychicEntropy.TargetPsyfocus - actor.psychicEntropy.CurrentPsyfocus;
        }

        public static float EffectiveAvailablePsyfocus(Pawn pawn, float conversionFactor)
        {
            return pawn.psychicEntropy.CurrentPsyfocus * conversionFactor;
        }

        public static float PsyfocusToDrain(Pawn victim, float desiredDrain, out float conversionFactor)
        {
            conversionFactor = -1f;
            if (!victim.HasPsylink || victim.psychicEntropy == null) return 0f;

            float sensitivity = victim.psychicEntropy.PsychicSensitivity;
            conversionFactor = PsyfocusConversionFactor(sensitivity);
            float effectiveAvailablePsyfocus = EffectiveAvailablePsyfocus(victim, conversionFactor);

            Log.Message("psyfocusConversionFactor: " + conversionFactor + ", effectiveAvailablePsyfocus: " + effectiveAvailablePsyfocus);

            float effectivePsyfocusToDrain = Mathf.Min(effectiveAvailablePsyfocus, desiredDrain);
            return effectivePsyfocusToDrain / conversionFactor;
        }

        public static float PsyfocusConversionFactor(float targetSensitivity)
        {
            float factor = BasePsyfocusConversionFactor;
            factor *= 1f + (2f * (targetSensitivity - 1f));
            factor = Mathf.Max(0f, Mathf.Min(0.9f, factor));
            return factor;
        }

        public static float CurrentConsciousness(Pawn pawn)
        {
            return pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness);
        }

        public static float ConsciousnessToDrain(Pawn victim, float desiredDrain, bool checkPsyfocusFirst = false)
        {
            float amountToDrain = desiredDrain;
            if (checkPsyfocusFirst)
            {
                float psyfocusToDrain = PsyfocusToDrain(victim, amountToDrain, out float psyfocusConversionFactor);
                amountToDrain -= psyfocusToDrain * psyfocusConversionFactor;
            }

            float curConsciousness = CurrentConsciousness(victim);
            return Mathf.Max(0f, Mathf.Min(curConsciousness, amountToDrain / BaseConsciousnessConversionFactor));
        }

        public static bool WouldResistPsyphon(Pawn actor, Pawn victim)
        {
            if (victim.Downed) return false;
            if (victim.InMentalState) return true;
            if (victim.Faction == actor.Faction) return false;
            if (actor.Faction.IsPlayer && (
                (victim.IsSlaveOfColony && victim.guest.SlaveIsSecure)
                || (victim.IsPrisonerOfColony && victim.guest.PrisonerIsSecure)
                ))
            {
                return false;
            }
            return true;
        }

        public static bool ValidTarget(Pawn pawn, out string reason)
        {
            reason = "";

            if (pawn.Dead)
            {
                return false;
            }
            if (!pawn.RaceProps.Humanlike)
            {
                reason = "AbilityMustBeHuman".Translate();
                return false;
            }
            if (pawn.psychicEntropy != null && pawn.psychicEntropy.PsychicSensitivity < float.Epsilon)
            {
                reason = "CYB_MessageNoPsychicSensitivity".Translate();
                return false;
            }
            if (pawn.health.hediffSet.GetBrain() == null)
            {
                reason = "CYB_MessageNoBrain".Translate();
                return false;
            }
            if (pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) < float.Epsilon)
            {
                reason = "CYB_MessageNoConsciousness".Translate();
                return false;
            }
            return true;
        }

        public static Gene_Psyphon GetPsyphonGene(Pawn pawn)
        {
            if (pawn.genes == null) return null;
            return (Gene_Psyphon)pawn.genes.GenesListForReading.Find(g => g.Active && g is Gene_Psyphon);
        }

        public static bool CanUsePsyphon(Pawn pawn, out Gene_Psyphon gene_Psyphon)
        {
            gene_Psyphon = GetPsyphonGene(pawn);
            if (gene_Psyphon == null) return false;
            if (!pawn.HasPsylink) return false;
            if (pawn.psychicEntropy == null || pawn.psychicEntropy.PsychicSensitivity < float.Epsilon) return false;
            return true;
        }

        public static bool WouldKill(Pawn pawn, float drainAmount)
        {
            LogUtil.DebugLog($"WouldKill - pawn: {pawn}, drainAmount {drainAmount}" +
                $", CurrentConsciousness: {CurrentConsciousness(pawn)}" +
                $", ConsciousnessToDrain: {ConsciousnessToDrain(pawn, drainAmount, true)}"
                );
            return CurrentConsciousness(pawn) <= ConsciousnessToDrain(pawn, drainAmount, true);
        }

        public static bool MightKill(Pawn pawn, float drainAmount)
        {
            float consciousnessToDrain = ConsciousnessToDrain(pawn, drainAmount, true);
            float curConsciousness = CurrentConsciousness(pawn);
            LogUtil.DebugLog($"MightKill - pawn: {pawn}, drainAmount {drainAmount}" +
                $", CurrentConsciousness: {curConsciousness}" +
                $", ConsciousnessToDrain: {consciousnessToDrain}" +
                $", MaxPotentialBrainDamage: {MaxPotentialBrainDamage(consciousnessToDrain)}"
                );
            if (curConsciousness <= consciousnessToDrain) return true;

            BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
            float curBrainHealth = pawn.health.hediffSet.GetPartHealth(brain);

            float minPotentialBrainHealth = curBrainHealth - MaxPotentialBrainDamage(consciousnessToDrain);
            if (minPotentialBrainHealth <= 0f) return true;

            if (((curConsciousness / curBrainHealth) * minPotentialBrainHealth)
                 - consciousnessToDrain
                 <= 0f)
            {
                return true;
            }
            return false;
        }

        public static bool MightCauseBrainDamage(Pawn pawn, float drainAmount)
        {
            float consciousnessToDrain = ConsciousnessToDrain(pawn, drainAmount, true);
            if (consciousnessToDrain > BrainDamageThreshold) return true;

            return false;
        }

        public static float GetBrainDamage(float consciousnessDrained)
        {
            return brainDamageVariance.RandomInRange * brainDamageFromDrainCurve.Evaluate(consciousnessDrained);
        }

        public static float MaxPotentialBrainDamage(float consciousnessDrained)
        {
            return brainDamageVariance.TrueMax * brainDamageFromDrainCurve.Evaluate(consciousnessDrained);
        }
    }
}
