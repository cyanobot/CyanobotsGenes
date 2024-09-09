using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace CyanobotsGenes
{
    public class CompProperties_AbilityImplantXenotype : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityImplantXenotype()
        {
            compClass = typeof(CompAbilityEffect_ImplantXenotype);
        }
    }

    public class CompAbilityEffect_ImplantXenotype : CompAbilityEffect
    {
        public XenotypeDef Xenotype
        {
            get
            {
                if (!typeof(Ability_ImplantXenotype).IsAssignableFrom(parent.GetType())) return null;
                return ((Ability_ImplantXenotype)parent).Xenotype;
            }
        }
        public new CompProperties_AbilityImplantXenotype Props => (CompProperties_AbilityImplantXenotype)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn targetPawn = target.Pawn;
            if (targetPawn != null)
            {
                Implant(targetPawn);
            }
        }

		public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
		{
			Pawn targetPawn = target.Pawn;
			if (targetPawn == null)
			{
				return base.Valid(target, throwMessages);
			}
			if (targetPawn.genes == null)
            {
				if (throwMessages)
                {
					Messages.Message("CYB_MessageTargetRequiresGenes".Translate(), targetPawn, MessageTypeDefOf.RejectInput, historical: false);
                }
				return false;
            }
			if (targetPawn.IsQuestLodger())
			{
				if (throwMessages)
				{
					Messages.Message("MessageCannotImplantInTempFactionMembers".Translate(), targetPawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			if (targetPawn.HostileTo(parent.pawn) && !targetPawn.Downed)
			{
				if (throwMessages)
				{
					Messages.Message("MessageCantUseOnResistingPerson".Translate(parent.def.Named("ABILITY")), targetPawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			if (AlreadyXenotype(targetPawn, Xenotype))
			{
				if (throwMessages)
				{
					Messages.Message("MessageCannotUseOnSameXenotype".Translate(targetPawn), targetPawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			if (!PawnIdeoCanAcceptReimplant(targetPawn,Xenotype) && !(targetPawn.Downed || (targetPawn.IsPrisonerOfColony && targetPawn.guest.PrisonerIsSecure)))
			{
				if (throwMessages)
				{
					Messages.Message("MessageCannotBecomeNonPreferredXenotype".Translate(targetPawn), targetPawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			return base.Valid(target, throwMessages);
        }

		/*
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (!GeneUtility.CanAbsorbXenogerm(parent.pawn))
			{
				yield break;
			}
			Pawn implanter = parent.pawn;
			Command_Action command_Action = new Command_Action
			{
				defaultLabel = "ForceXenogermImplantation".Translate() + " : " + Xenotype.LabelCap,
				defaultDesc = "ForceXenogermImplantationDesc".Translate(),
				icon = parent.def.uiIcon,
				action = delegate
				{
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					List<Pawn> list2 = implanter.MapHeld.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
					for (int i = 0; i < list2.Count; i++)
					{
						Pawn absorber = list2[i];
						if (absorber.genes != null && absorber.IsColonistPlayerControlled && !AlreadyXenotype(absorber, Xenotype) && absorber.CanReach(implanter, PathEndMode.ClosestTouch, Danger.Deadly))
						{
							if (!PawnIdeoCanAcceptReimplant(parent.pawn, Xenotype))
							{
								list.Add(new FloatMenuOption(absorber.LabelCap + ": " + "IdeoligionForbids".Translate(), null, absorber, Color.white));
							}
							else
							{
								list.Add(new FloatMenuOption(absorber.LabelShort, delegate
								{
									absorber.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.AbsorbXenogerm, implanter), JobTag.Misc);
									// NEEDS CUSTOM JOB PROBABLY
								}, absorber, Color.white));
							}
						}
					}
					if (list.Any())
					{
						Find.WindowStack.Add(new FloatMenu(list));
					}
				}
			};
			if (implanter.IsQuestLodger())
			{
				command_Action.Disable("TemporaryFactionMember".Translate(implanter.Named("PAWN")));
			}
			else if (implanter.health.hediffSet.HasHediff(HediffDefOf.XenogermLossShock))
			{
				command_Action.Disable("XenogermLossShockPresent".Translate(implanter.Named("PAWN")));
			}
			else if (implanter.IsPrisonerOfColony && !implanter.Downed)
			{
				command_Action.Disable("MessageTargetMustBeDownedToForceReimplant".Translate(implanter.Named("PAWN")));
			}
			yield return command_Action;
		}
		*/

		public static bool PawnIdeoCanAcceptReimplant(Pawn implantee, XenotypeDef xenotype)
		{
			if (!ModsConfig.IdeologyActive)
			{
				return true;
			}
			if (!IdeoUtility.DoerWillingToDo(HistoryEventDefOf.PropagateBloodfeederGene, implantee) && xenotype.genes.Any(x => x == GeneDefOf.Bloodfeeder))
			{
				return false;
			}
			if (!IdeoUtility.DoerWillingToDo(HistoryEventDefOf.BecomeNonPreferredXenotype, implantee) && !implantee.Ideo.PreferredXenotypes.Contains(xenotype))
			{
				return false;
			}
			return true;
		}

		public bool CanForceImplantation()
        {
            if (parent.pawn.IsPrisonerOfColony && parent.pawn.guest.PrisonerIsSecure)
            {
                return true;
            }
            if (parent.pawn.Downed)
            {
                return true;
            }
            return false;
        }

        public static bool AlreadyXenotype(Pawn pawn, XenotypeDef xenotype)
        {
            if (pawn.genes == null) return false;
            if (pawn.genes.UniqueXenotype)
            {
                //not the same if pawn has any xenogenes that aren't in the xenotype
                foreach (Gene gene in pawn.genes.Xenogenes)
                {
                    if (!xenotype.genes.Any(x => x == gene.def)) return false;
                }
                //not the same if xenotype contains any genes the pawn doesn't already have
                //(xeno or active germline)
                foreach (GeneDef geneDef in xenotype.genes)
                {
                    if (!pawn.genes.Xenogenes.Any(x => x.def == geneDef)
                        && !pawn.genes.Endogenes.Any(x => x.def == geneDef && x.Active))
                    {
                        return false;
                    }
                }
                return true;
            }
            else return pawn.genes.Xenotype == xenotype;
        }

		public void Implant(Pawn target)
        {
            LogUtil.DebugLog("Calling implant on " + target + ", xenotype: " + Xenotype);
			if (Xenotype == null)
            {
				Log.Error("[Cyanobot's Genes] CompProperties_AbilityImplantXenotype was unable to determine which xenotype it should be implanting.");
				return;
            }
            target.genes.SetXenotypeDirect(Xenotype);
            target.genes.xenotypeName = Xenotype.label;
            target.genes.ClearXenogenes();
            foreach (GeneDef gene in Xenotype.AllGenes)
            {
                target.genes.AddGene(gene, xenogene: true);
            }
            if (!Xenotype.soundDefOnImplant.NullOrUndefined())
            {
                Xenotype.soundDefOnImplant.PlayOneShot(SoundInfo.InMap(target));
            }
            target.health.AddHediff(HediffDefOf.XenogerminationComa);
            GeneUtility.UpdateXenogermReplication(target);
        }
    }

}
