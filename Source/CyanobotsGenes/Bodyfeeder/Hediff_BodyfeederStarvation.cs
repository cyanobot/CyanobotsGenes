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
    class Hediff_BodyfeederStarvation : HediffWithComps
    {
		public override string SeverityLabel
		{
			get
			{
				if (Severity == 0f)
				{
					return null;
				}
				return Severity.ToStringPercent();
			}
		}

	}
}
