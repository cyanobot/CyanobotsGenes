using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;
using static CyanobotsGenes.PsyphonUtility;

namespace CyanobotsGenes
{
    public class JobGiver_AutoPsyphon : ThinkNode_JobGiver
    {
        public override float GetPriority(Pawn pawn)
        {
            if (!pawn.IsColonistPlayerControlled) return 0f;
            if (!CanUsePsyphon(pawn, out Gene_Psyphon gene_Psyphon)) return 0f;

            if (pawn.psychicEntropy.CurrentPsyfocus > (pawn.psychicEntropy.TargetPsyfocus - 0.1f))
            {
                return 0f;
            }

            if (gene_Psyphon.autoPsyphonTargets == PsyphonTargetStatus.None)
            {
                return 0f;
            }

            return 9.2f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            Gene_Psyphon gene_Psyphon = pawn.genes.GetFirstGeneOfType<Gene_Psyphon>();
            if (gene_Psyphon == null) return null;

            Ability ability_Psyphon = gene_Psyphon.GetPsyphonAbility();
            if (ability_Psyphon == null || !ability_Psyphon.CanCast) return null;

            PsyphonTargetStatus allowedTargetStatus = gene_Psyphon.autoPsyphonTargets;
            if (allowedTargetStatus == PsyphonTargetStatus.None) return null;

            float desiredPsyfocus = DesiredPsyfocus(pawn);

            List<Pawn> potentialTargets =
                (from p in pawn.Map.mapPawns.AllHumanlikeSpawned
                where p != pawn
                    && ValidTarget(p, out var _)
                    && (gene_Psyphon.allowLethal || !MightKill(p, desiredPsyfocus))
                    && TargetStatusValidator(p, allowedTargetStatus)
                    && !WouldResistPsyphon(pawn, p)
                    && pawn.CanReserveAndReach(p,PathEndMode.Touch,pawn.NormalMaxDanger())
                    && !p.IsForbidden(pawn)
                select p)
                .ToList();

            if (potentialTargets.NullOrEmpty()) return null;

            Pawn bestVictim = null;
            float highestPriority = -100f;
            foreach (Pawn target in potentialTargets)
            {
                float priority = GetTargetPriority(target, desiredPsyfocus);
                if (priority > highestPriority)
                {
                    highestPriority = priority;
                    bestVictim = target;
                }
            }
            if (bestVictim == null) return null;

            return ability_Psyphon.GetJob(bestVictim,null);
        }

        public bool TargetStatusValidator(Pawn pawn, PsyphonTargetStatus allowedTargetStatus)
        {
            return (allowedTargetStatus.HasFlag(PsyphonTargetStatus.Colonist) && pawn.IsFreeColonist)
                || (allowedTargetStatus.HasFlag(PsyphonTargetStatus.Slave) && pawn.IsSlaveOfColony)
                || (allowedTargetStatus.HasFlag(PsyphonTargetStatus.Prisoner) && pawn.IsPrisonerOfColony)
                ;
        }

        public static float GetTargetPriority(Pawn pawn, float drainAmount)
        {
            float priority = 0f;

            //prisoners > slaves > colonists
            if (pawn.IsPrisonerOfColony) priority = 20f;
            else if (pawn.IsSlaveOfColony) priority = 10f;

            //has psyfocus to steal > not
            if (pawn.HasPsylink && pawn.psychicEntropy != null)
            {
                float effectiveAvailablePsyfocus = EffectiveAvailablePsyfocus(pawn, PsyfocusConversionFactor(pawn.psychicEntropy.PsychicSensitivity));
                //enough psyfocus to fully cover demand > not enough
                if (effectiveAvailablePsyfocus >= drainAmount)
                {
                    priority += 5f;
                }
                //some > none
                else if (effectiveAvailablePsyfocus > float.Epsilon)
                {
                    priority += 2f;
                }
                //total amount available determines precise priority
                priority += effectiveAvailablePsyfocus;
            }

            //would die = minimum priority
            if (WouldKill(pawn,drainAmount))
            {
                priority -= 50f;
            }
            //exact priority determined by consciousness that would remain after drain
            else
            {
                float consciousnessToDrain = ConsciousnessToDrain(pawn, drainAmount, true);
                float curConsciousness = CurrentConsciousness(pawn);

                priority += curConsciousness - consciousnessToDrain;
            }
            
            return priority;
        }
    }
}
