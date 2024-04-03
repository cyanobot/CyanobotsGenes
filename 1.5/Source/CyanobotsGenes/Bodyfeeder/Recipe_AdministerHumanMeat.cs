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
using static CyanobotsGenes.BodyfeederUtility;

namespace CyanobotsGenes
{
    class Recipe_AdministerHumanMeat : Recipe_AdministerIngestible
    {
		public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
		{
			if (!base.AvailableOnNow(thing, part))
			{
				return false;
			}
			Pawn pawn = thing as Pawn;
			if (!IsBodyFeeder(pawn))
			{
				return false;
			}
			return true;
		}
	}
}
