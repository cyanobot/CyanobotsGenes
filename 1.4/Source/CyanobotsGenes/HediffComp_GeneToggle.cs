using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyanobotsGenes
{
    class HediffComp_GeneToggle : HediffComp
    {
        public HediffCompProperties_GeneToggle Props => (HediffCompProperties_GeneToggle)props;

        public override bool CompShouldRemove
		{
			get
			{
                if (Pawn.genes == null) return true;
                if (Props.invert)
                {
                    return Pawn.genes.HasGene(Props.gene);
                }
                else return !Pawn.genes.HasGene(Props.gene);
			}
		}

	}
}
