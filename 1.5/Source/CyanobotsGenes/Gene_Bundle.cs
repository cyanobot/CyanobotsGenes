using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CyanobotsGenes
{
    class Gene_Bundle : Gene
    {
        public override void PostAdd()
        {
            GeneExtension_Bundle bundle = def.GetModExtension<GeneExtension_Bundle>();
            if (bundle == null || bundle.genes.NullOrEmpty()) return;

            bool isXenogene = pawn.genes.Xenogenes.Contains(this);
            foreach (GeneDef gene in bundle.genes)
            {
                pawn.genes.AddGene(gene, isXenogene);
            }

            pawn.genes.RemoveGene(this);
        }
    }
}
