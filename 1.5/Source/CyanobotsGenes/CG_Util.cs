using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CyanobotsGenes
{
    public static class CG_Util
    {
        public static bool HasActiveGene(this Pawn pawn, GeneDef geneDef)
        {
            return pawn?.genes?.GetGene(geneDef)?.Active ?? false;
        }

    }
}
