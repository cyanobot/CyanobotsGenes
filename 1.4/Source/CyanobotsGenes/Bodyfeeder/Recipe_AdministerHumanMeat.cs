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
		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
		{
			float nutrition = FoodUtility.NutritionForEater(pawn, ingredients[0]) * ingredients[0].stackCount;
			Log.Message("nutrition to ingest: " + nutrition);
			nutrition = ingredients[0].Ingested(pawn, nutrition);
			Log.Message("nutrition ingested: " + nutrition);
			if (!pawn.Dead)
			{
				pawn.needs.food.CurLevel += nutrition;
			}
		}

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
