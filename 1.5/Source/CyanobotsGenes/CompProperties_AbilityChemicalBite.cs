using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace CyanobotsGenes
{
    public class CompProperties_AbilityChemicalBite : CompProperties_AbilityEffect
	{
		public ChemicalDef chemical;
		public float existingAddictionSeverityOffset = 1f;
		public float addictiveness;
		public float needLevelOffset = 1f;
		public HediffDef hediffHigh;

		public CompProperties_AbilityChemicalBite()
		{
			compClass = typeof(CompAbilityEffect_ChemicalBite);
		}
	}

	public class CompAbilityEffect_ChemicalBite : CompAbilityEffect
    {
		public new CompProperties_AbilityChemicalBite Props => (CompProperties_AbilityChemicalBite)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			Pawn pawn = target.Pawn;
			if (pawn != null)
			{
				GiveHigh(pawn);
				AddictionEffects(pawn);
			}
		}

		public void GiveHigh(Pawn targetPawn)
        {
			Hediff hediff = HediffMaker.MakeHediff(Props.hediffHigh, targetPawn);
			targetPawn.health.AddHediff(hediff);
        }

		public void AddictionEffects(Pawn targetPawn)
        {
			HediffDef addictionHediffDef = Props.chemical.addictionHediff;
			Hediff_Addiction hediff_Addiction = AddictionUtility.FindAddictionHediff(targetPawn, Props.chemical);
			Log.Message("addictionHediffDef: " + addictionHediffDef + ", hediff_Addiction: " + hediff_Addiction);
			if (hediff_Addiction != null)
            {
				hediff_Addiction.Severity += Props.existingAddictionSeverityOffset;
            }
            else
			{
				float addictiveness = Props.addictiveness;
				Log.Message("no addiction found, addictiveness: " + addictiveness);
				if (targetPawn.genes != null)
				{
					addictiveness *= targetPawn.genes.AddictionChanceFactor(Props.chemical);
					Log.Message("adjusted addictiveness: " + addictiveness);
				}
				if (Rand.Value < addictiveness)
				{
					targetPawn.health.AddHediff(addictionHediffDef);
					if (PawnUtility.ShouldSendNotificationAbout(targetPawn))
					{
						Find.LetterStack.ReceiveLetter("LetterLabelNewlyAddicted".Translate(Props.chemical.label).CapitalizeFirst(), "LetterNewlyAddicted".Translate(targetPawn.LabelShort, Props.chemical.label, targetPawn.Named("PAWN")).AdjustedFor(targetPawn).CapitalizeFirst(), LetterDefOf.NegativeEvent, targetPawn);
					}
					AddictionUtility.CheckDrugAddictionTeachOpportunity(targetPawn);
				}
			}
			if (addictionHediffDef.causesNeed != null)
			{
				Need need = targetPawn.needs.AllNeeds.Find((Need x) => x.def == addictionHediffDef.causesNeed);
				if (need != null)
				{
					float effect = Props.needLevelOffset;
					AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize_NewTemp(targetPawn, Props.chemical, ref effect, applyGeneToleranceFactor: false);
					need.CurLevel += effect;
				}
			}
		}

		public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
		{
			return Valid(target);
		}

		public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
		{
			Pawn targetPawn = target.Pawn;
			if (targetPawn == null)
			{
				return false;
			}
			if (!targetPawn.RaceProps.IsFlesh)
			{
				if (throwMessages)
				{
					Messages.Message("CYB_Message_CantUseOnNonFlesh".Translate(parent.def.Named("ABILITY")), targetPawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			if (targetPawn.Faction != null && !targetPawn.IsSlaveOfColony && !targetPawn.IsPrisonerOfColony)
			{
				if (targetPawn.Faction.HostileTo(parent.pawn.Faction))
				{
					if (!targetPawn.Downed)
					{
						if (throwMessages)
						{
							Messages.Message("MessageCantUseOnResistingPerson".Translate(parent.def.Named("ABILITY")), targetPawn, MessageTypeDefOf.RejectInput, historical: false);
						}
						return false;
					}
				}
				else if (targetPawn.IsQuestLodger() || targetPawn.Faction != parent.pawn.Faction)
				{
					if (throwMessages)
					{
						Messages.Message("MessageCannotUseOnOtherFactions".Translate(parent.def.Named("ABILITY")), targetPawn, MessageTypeDefOf.RejectInput, historical: false);
					}
					return false;
				}
			}
			if (targetPawn.IsWildMan() && !targetPawn.IsPrisonerOfColony && !targetPawn.Downed)
			{
				if (throwMessages)
				{
					Messages.Message("MessageCantUseOnResistingPerson".Translate(parent.def.Named("ABILITY")), targetPawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			if (targetPawn.InMentalState || PrisonBreakUtility.IsPrisonBreaking(targetPawn))
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
				if (!targetPawn.RaceProps.IsFlesh)
				{
					text += "CYB_Message_CantUseOnNonFlesh".Translate(parent.def.Named("ABILITY"));
				}
				else if (targetPawn.HostileTo(parent.pawn) && !targetPawn.Downed)
				{
					text += "MessageCantUseOnResistingPerson".Translate(parent.def.Named("ABILITY"));
				}
				return text;
			}
			return base.ExtraLabelMouseAttachment(target);
		}


	}
}
