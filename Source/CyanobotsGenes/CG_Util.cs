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
#if RW_1_5
            return pawn?.genes?.GetGene(geneDef)?.Active ?? false;
#else
            return pawn?.genes?.HasActiveGene(geneDef) ?? false;
#endif
        }
    }
    }
