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
    public class HediffCompProperties_CapModsMultipliedBySeverity : HediffCompProperties
    {
        public HediffCompProperties_CapModsMultipliedBySeverity()
        {
            compClass = typeof(HediffComp_CapModsMultipliedBySeverity);
        }
    }

    class HediffComp_CapModsMultipliedBySeverity : HediffComp
    {
        public HediffCompProperties_CapModsMultipliedBySeverity Props => (HediffCompProperties_CapModsMultipliedBySeverity)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            parent.pawn.health.Notify_HediffChanged(parent);
        }
    }
}
