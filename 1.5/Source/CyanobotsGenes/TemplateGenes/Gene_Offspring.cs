using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CyanobotsGenes
{
    public class Gene_Offspring : Gene
    {
        public XenotypeDef Xenotype => this.def.GetModExtension<GeneExtension_Xenotype>()?.xenotype;
    }
}
