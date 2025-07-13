using UnityEngine;
using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyanobotsGenes
{
    public class ThoughtWorker_PackBond : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
        {
            if (!p.RaceProps.Humanlike)
            {
                return false;
            }

            if (!other.RaceProps.Humanlike)
            {
                return false;
            }

           if (!p.HasActiveGene(CG_DefOf.PackBond))
            {
                return false;
            }

            if (!other.HasActiveGene(CG_DefOf.PackBond))
            {
                return false;
            }

            if(p.Faction == other.Faction)
            {
                return true;
            }

            return false;
        }
    }
}
