using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

//modeled off VRE Lycanthrope, thank you to the VE team

namespace CyanobotsGenes
{
    public class XenotypeGeneTemplateDef : Def
    {
        public Type geneClass = typeof(Gene);

        public int biostatCpx = 0;
        public int biostatMet = 0;
        public int biostatArc = 0;
        public float minAgeActive = 0;

        public GeneCategoryDef displayCategory;
        public float displayOrderInCategory = 0;

        public float selectionWeight = 0f;

        public List<string> exclusionTags = new List<string>();
        public List<AbilityDef> abilities;
        public List<AbilityTemplateDef> abilityTemplates;
    }
}
