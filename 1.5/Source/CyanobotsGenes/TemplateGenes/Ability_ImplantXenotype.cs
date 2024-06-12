using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;
using RimWorld;
using RimWorld.Planet;

namespace CyanobotsGenes
{
    class Ability_ImplantXenotype : Ability
    {
        public XenotypeDef xenotype;
        public XenotypeDef Xenotype
        {
            get
            {
                if (xenotype == null) xenotype = FindXenotypeFromGenes(); 
                return xenotype;
            }
        }

        public Ability_ImplantXenotype() 
            : base()
        {}

        public Ability_ImplantXenotype(Pawn pawn)
            : base(pawn)
        {}

        public Ability_ImplantXenotype(Pawn pawn, Precept sourcePrecept)
            : base(pawn, sourcePrecept)
        {}

        public Ability_ImplantXenotype(Pawn pawn, AbilityDef def)
            : base(pawn, def)
        {}

        public Ability_ImplantXenotype(Pawn pawn, Precept sourcePrecept, AbilityDef def)
            : base(pawn, sourcePrecept, def)
        {}


        public XenotypeDef FindXenotypeFromGenes()
        {
            List<Gene> implanterGenes = pawn.genes?.GenesListForReading
                .Where(x => x.GetType() == typeof(Gene_ImplantXenotype)).ToList();

            if (implanterGenes.EnumerableNullOrEmpty())
            {
                Log.Error("[Cyanobot's Genes] Ability_ImplantXenotype tried to get its xenotype from the genes of pawn: " + pawn
                    + ", but could not find any genes of class Gene_ImplantXenotype");
                return null;
            }

            List<Ability> implanterAbilities = pawn.abilities.abilities
                .Where(x => x.GetType() == typeof(Ability_ImplantXenotype)).ToList();

            int index = implanterAbilities.IndexOf(this);
            if (index == -1)
            {
                Log.Error("[Cyanobot's Genes] Ability_ImplantXenotype failed to find itself in the abilities of pawn: " + pawn);
                return null;
            }

            return ((Gene_ImplantXenotype)implanterGenes[index]).Xenotype;
        }

    }
}
