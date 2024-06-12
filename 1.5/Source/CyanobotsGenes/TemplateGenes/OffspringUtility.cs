using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CyanobotsGenes
{
    public static class OffspringUtility
    {
        public static bool HasActiveOffspringGene(Pawn pawn)
        {
            if (pawn?.genes == null) return false;
            if (pawn.genes.GenesListForReading.Any(g => 
                    g.Active && g.GetType() == typeof(Gene_Offspring)))
                return true;
            return false;
        }

        public static bool IsActiveOffspringGene(Gene gene)
        {
            if (gene.Active && gene.GetType() == typeof(Gene_Offspring)) return true;
            return false;
        }

        public static XenotypeDef GetOffspringXenotype(Pawn mother, Pawn father)
        {
            XenotypeDef xenotype;
            if (mother?.genes != null) 
            {
                xenotype = ((Gene_Offspring)mother.genes.GenesListForReading.Find(x => IsActiveOffspringGene(x)))?.Xenotype;
                if (xenotype != null) return xenotype;
            }
            if (father?.genes != null)
            {
                xenotype = ((Gene_Offspring)father.genes.GenesListForReading.Find(x => IsActiveOffspringGene(x)))?.Xenotype;
                if (xenotype != null) return xenotype;
            }
            
            Log.Error("[CyanobotsGenes] Attempted to get offspring xenotype [parents: " + mother + ", " + father
                    + "] but found no Offspring gene.");
            return null;

        }

    }
}
