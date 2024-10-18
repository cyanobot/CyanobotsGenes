using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;
using RimWorld;

namespace CyanobotsGenes
{
    class Gene_ImplantXenotype : Gene
    {
        public XenotypeDef Xenotype => this.def.GetModExtension<GeneExtension_Xenotype>()?.xenotype;
    }
}
