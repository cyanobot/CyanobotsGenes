using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CyanobotsGenes
{
    class GeneExtension_Bundle : DefModExtension
    {
        public List<GeneDef> genes;

        public bool Matches(IEnumerable<GeneDef> geneDefs)
        {
            foreach (GeneDef bundleMember in genes)
            {
                if (!geneDefs.Contains(bundleMember)) return false;
            }
            return true;
        }
    }
}
