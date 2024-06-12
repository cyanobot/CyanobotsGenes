using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;
using static CyanobotsGenes.PsyphonUtility;

namespace CyanobotsGenes
{
    public class Gizmo_AutoPsyphon : Command
    {
        public Pawn pawn;
        public Gene_Psyphon Gene => GetPsyphonGene(pawn);

        public Gizmo_AutoPsyphon(Pawn pawn)
        {
            this.pawn = pawn;
            this.defaultLabel = "CYB_Command_AutoPsyphon_Label".Translate();
            this.defaultDesc = "CYB_Command_AutoPsyphon_Desc".Translate();
            this.icon = CG_Mod.autoPsyphonIcon;
        }

        public override bool Visible => !pawn.Drafted;

        public override string DescPostfix
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine();
                if (Gene.disableMeditate)
                {
                    sb.AppendLine("CYB_MeditationDisabled".Translate());
                }
                else
                {
                    sb.AppendLine("CYB_MeditationEnabled".Translate());
                }

                if (Gene.allowLethal)
                {
                    sb.AppendLine("CYB_LethalAllowed".Translate());
                }
                else
                {
                    sb.AppendLine("CYB_LethalDisallowed".Translate());
                }
                
                if (Gene.autoPsyphonTargets == PsyphonTargetStatus.None)
                {
                    sb.AppendLine("CYB_AutoPsyphonDisabled".Translate());
                }
                else
                {
                    sb.AppendLine("CYB_AutoPsyphonTargets".Translate()
                        + ": " + StringFromTargetStatusEnum(Gene.autoPsyphonTargets));
                }
                return sb.ToString();
            }
        }

        public string StringFromTargetStatusEnum(PsyphonTargetStatus targetStatus)
        {
            List<string> list = new List<string>();
            if (targetStatus.HasFlag(PsyphonTargetStatus.Colonist)) list.Add("CYB_Colonists".Translate());
            if (targetStatus.HasFlag(PsyphonTargetStatus.Slave)) list.Add("CYB_Slaves".Translate());
            if (targetStatus.HasFlag(PsyphonTargetStatus.Prisoner)) list.Add("CYB_Prisoners".Translate());
            return list.ToStringSafeEnumerable();
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            if (Gene.disableMeditate)
            {
                list.Add(new FloatMenuOption("CYB_Command_MeditationTurnOn_Label".Translate(), delegate
                {
                    Gene.disableMeditate = false;
                }));
            }
            else
            {
                list.Add(new FloatMenuOption("CYB_Command_MeditationTurnOff_Label".Translate(), delegate 
                {
                    Gene.disableMeditate = true;
                }));
            }

            if (Gene.allowLethal)
            {
                list.Add(new FloatMenuOption("CYB_Command_DisllowLethal_Label".Translate(), delegate
                {
                    Gene.allowLethal = false;
                }));
            }
            else
            {
                list.Add(new FloatMenuOption("CYB_Command_AllowLethal_Label".Translate(), delegate
                {
                    Gene.allowLethal = true;
                }));
            }

            list.Add(new FloatMenuOption("CYB_Command_SetAutoPsyphonTargets_None_Label".Translate(), delegate
            {
                Gene.autoPsyphonTargets = PsyphonTargetStatus.None;
            }));
            list.Add(new FloatMenuOption("CYB_Command_SetAutoPsyphonTargets_Prisoners_Label".Translate(), delegate
            {
                Gene.autoPsyphonTargets = PsyphonTargetStatus.Prisoner;
            }));
            list.Add(new FloatMenuOption("CYB_Command_SetAutoPsyphonTargets_Slaves_Label".Translate(), delegate
            {
                Gene.autoPsyphonTargets = PsyphonTargetStatus.Slave;
            }));
            list.Add(new FloatMenuOption("CYB_Command_SetAutoPsyphonTargets_Colonists_Label".Translate(), delegate
            {
                Gene.autoPsyphonTargets = PsyphonTargetStatus.Colonist;
            }));
            list.Add(new FloatMenuOption("CYB_Command_SetAutoPsyphonTargets_PrisonersAndSlaves_Label".Translate(), delegate
            {
                Gene.autoPsyphonTargets = PsyphonTargetStatus.Prisoner | PsyphonTargetStatus.Slave;
            }));
            list.Add(new FloatMenuOption("CYB_Command_SetAutoPsyphonTargets_PrisonersAndColonists_Label".Translate(), delegate
            {
                Gene.autoPsyphonTargets = PsyphonTargetStatus.Prisoner | PsyphonTargetStatus.Colonist;
            }));
            list.Add(new FloatMenuOption("CYB_Command_SetAutoPsyphonTargets_SlavesAndColonists_Label".Translate(), delegate
            {
                Gene.autoPsyphonTargets = PsyphonTargetStatus.Slave | PsyphonTargetStatus.Colonist;
            }));
            list.Add(new FloatMenuOption("CYB_Command_SetAutoPsyphonTargets_Anyone_Label".Translate(), delegate
            {
                Gene.autoPsyphonTargets = PsyphonTargetStatus.Prisoner | PsyphonTargetStatus.Slave | PsyphonTargetStatus.Colonist;
            }));

            Find.WindowStack.Add(new FloatMenu(list));
        }
    }
}
