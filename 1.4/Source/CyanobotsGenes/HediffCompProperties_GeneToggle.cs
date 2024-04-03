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
    public class HediffCompProperties_GeneToggle : HediffCompProperties
    {
        public GeneDef gene;
        
        //if false, gene is Required for hediff
        //if true, gene Forbids hediff
        public bool invert = false;

        public HediffCompProperties_GeneToggle()
        {
            compClass = typeof(HediffComp_GeneToggle);
        }
    }
}