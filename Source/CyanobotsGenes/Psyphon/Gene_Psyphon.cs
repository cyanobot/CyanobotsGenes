using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace CyanobotsGenes
{

    public class Gene_Psyphon : Gene
    {
        public PsyphonTargetStatus autoPsyphonTargets = PsyphonTargetStatus.None;
        public bool disableMeditate = false;
        public bool allowLethal = false;

        public override IEnumerable<Gizmo> GetGizmos()
        {
            //Log.Message("GetGizmos");
            if (!pawn.HasPsylink)
            {
                yield break;
            }
            yield return new Gizmo_AutoPsyphon(pawn);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref autoPsyphonTargets, "autoPsyphonTargets", PsyphonTargetStatus.None);
            Scribe_Values.Look(ref disableMeditate, "disableMeditate", false);
            Scribe_Values.Look(ref allowLethal, "allowLethal", false);
        }

        public Ability GetPsyphonAbility()
        {
            if (pawn.abilities == null) return null;
            return pawn.abilities.AllAbilitiesForReading.Find(a => a.def == CG_DefOf.CYB_Psyphon);
        }
    }
}
