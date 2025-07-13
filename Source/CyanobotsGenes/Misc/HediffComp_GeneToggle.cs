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
                if (Props.invert)
                {
                    return Pawn.HasActiveGene(Props.gene);
                }
                else return !Pawn.HasActiveGene(Props.gene);
			}
		}

	}
}
