using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CyanobotsGenes
{
    class JobGiver_BodyfeederBerserk : ThinkNode_JobGiver
    {
        public static readonly IntRange ExpiryInterval_ShooterSucceeded = new IntRange(450, 550);
        public static readonly IntRange ExpiryInterval_Melee = new IntRange(360, 480);

        protected override Job TryGiveJob(Pawn pawn)
        {
            Thing food = BodyfeederUtility.TryGetHumanlikeFood(pawn, true, 40f);
            if (food != null)
            {
                Job jobIngest = JobMaker.MakeJob(CG_DefOf.IngestForHemogen, food);
                return jobIngest;
            }

            Predicate<Thing> downedValidator = delegate (Thing t)
            {
                if (!(t is Pawn)) return false;
                if ((t as Pawn).RaceProps.Humanlike)
                {
                    return true;
                }
                return false;
            };
            Pawn downed = (Pawn)GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map,
                pawn.Map.mapPawns.SpawnedDownedPawns, PathEndMode.ClosestTouch, TraverseParms.For(pawn), 
                validator: downedValidator);
            if (downed != null)
            {
                Job jobIngestDowned = JobMaker.MakeJob(CG_DefOf.IngestDowned, downed);
                return jobIngestDowned;
            }

            Pawn target = FindPawnTarget(pawn);
            if (target == null) return null;
            pawn.mindState.enemyTarget = target;

            Verb verb = pawn.TryGetAttackVerb(target);
            if (verb == null)
            {
                return null;
            }
            if (verb.verbProps.IsMeleeAttack)
            {
                Job jobMelee = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
                jobMelee.expiryInterval = ExpiryInterval_Melee.RandomInRange;
                jobMelee.checkOverrideOnExpire = true;
                jobMelee.expireRequiresEnemiesNearby = true;
                return jobMelee;
            }
            bool lineOfFire = CoverUtility.CalculateOverallBlockChance(pawn, target.Position, pawn.Map) > 0.01f;
            bool positionValid = pawn.Position.Standable(pawn.Map) && pawn.Map.pawnDestinationReservationManager.CanReserve(pawn.Position, pawn, pawn.Drafted);
            bool canHitTarget = verb.CanHitTarget(target);
            bool close = (pawn.Position - target.Position).LengthHorizontalSquared < 25;
            if ((lineOfFire && positionValid && canHitTarget) || (close && canHitTarget))
            {
                return JobMaker.MakeJob(JobDefOf.Wait_Combat, ExpiryInterval_ShooterSucceeded.RandomInRange, checkOverrideOnExpiry: true);
            }
            CastPositionRequest newReq = default(CastPositionRequest);
            newReq.caster = pawn;
            newReq.target = target;
            newReq.verb = verb;
            newReq.maxRangeFromTarget = verb.verbProps.range;
            newReq.wantCoverFromTarget = verb.verbProps.range > 5f;
            if (!CastPositionFinder.TryFindCastPosition(newReq, out var dest))
            {
                return null;
            }
            if (dest == pawn.Position)
            {
                return JobMaker.MakeJob(JobDefOf.Wait_Combat, ExpiryInterval_ShooterSucceeded.RandomInRange, checkOverrideOnExpiry: true);
            }
            Job jobGoto = JobMaker.MakeJob(JobDefOf.Goto, dest);
            jobGoto.expiryInterval = ExpiryInterval_ShooterSucceeded.RandomInRange;
            jobGoto.checkOverrideOnExpire = true;
            return jobGoto;
        }

        private Pawn FindPawnTarget(Pawn pawn)
        {
            return (Pawn)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedReachable, 
                (Thing x) => x is Pawn pawn2 && pawn2.Spawned && !pawn2.Downed && !pawn2.IsInvisible()
                && pawn2.RaceProps.Humanlike, 0f, 25, 
                default(IntVec3), float.MaxValue, canBashDoors: true);
        }

    }
}
