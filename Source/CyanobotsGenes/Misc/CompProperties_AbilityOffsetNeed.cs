using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace CyanobotsGenes
{
    public class CompProperties_AbilityOffsetNeed : CompProperties_AbilityEffect
    {
        public NeedDef need;
        public float offset;

        public CompProperties_AbilityOffsetNeed()
        {
            compClass = typeof(CompAbilityEffect_OffsetNeed);
        }
        public override IEnumerable<string> ExtraStatSummary()
        {
            yield return (string)(need.LabelCap + ": " + Mathf.RoundToInt(offset * 100f).ToStringWithSign() );
        }
    }

    public class CompAbilityEffect_OffsetNeed : CompAbilityEffect
    {
        public new CompProperties_AbilityOffsetNeed Props => (CompProperties_AbilityOffsetNeed)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = target.Pawn;
            if (pawn != null)
            {
                Need need = pawn.needs?.TryGetNeed(Props.need);
                if (need != null) need.CurLevel += Props.offset;
            }
        }
    }
}
