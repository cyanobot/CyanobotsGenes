using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CyanobotsGenes
{
    public static class CasteUtility
    {
        public static float CasteCommonality(XenotypeDef xenotype)
        {
            if (xenotype.AllGenes.NullOrEmpty()) return 1f;
            foreach(GeneDef geneDef in xenotype.AllGenes)
            {
                GeneExtension_Caste ext = geneDef.GetModExtension<GeneExtension_Caste>();
                if (ext != null) return ext.casteCommonality;
            }
            return 1f;
        }

        public static XenotypeDef SelectXenotypeFromOptions(List<XenotypeDef> xenotypes)
        {
            if (xenotypes.NullOrEmpty()) return null;

            XenotypeDef xenotype;
            xenotypes.TryRandomElementByWeight(x => CasteCommonality(x), out xenotype);

            return xenotype;
        }
    }
}
