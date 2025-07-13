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
	static class GenerateMeatAdministerDefs
	{
		public static IEnumerable<RecipeDef> MeatAdministerDefs(bool hotReload = false)
		{
			//Log.Message("Fired MeatAdministerDefs");
			foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where(
				(ThingDef d) => d.IsMeat
				&& FoodUtility.GetMeatSourceCategory(d) == MeatSourceCategory.Humanlike)
			)
			{
				//Log.Message("Found humanlike meat def: " + item);
				string defName = "Administer_" + item.defName;
				RecipeDef recipeDef = (hotReload ? (DefDatabase<RecipeDef>.GetNamed(defName, errorOnFail: false) ?? new RecipeDef()) : new RecipeDef());
				recipeDef.defName = defName;
				recipeDef.label = "RecipeAdminister".Translate(item.label);
				recipeDef.jobString = "RecipeAdministerJobString".Translate(item.label);
				recipeDef.workerClass = typeof(Recipe_AdministerHumanMeat);
				recipeDef.targetsBodyPart = false;
				recipeDef.anesthetize = false;
				recipeDef.surgerySuccessChanceFactor = 99999f;
				recipeDef.modContentPack = CG_Mod.mcp;
				recipeDef.workAmount = item.ingestible.baseIngestTicks;
				IngredientCount ingredientCount = new IngredientCount();
				ingredientCount.SetBaseCount(item.ingestible.defaultNumToIngestAtOnce);
				ingredientCount.filter.SetAllow(item, allow: true);
				recipeDef.ingredients.Add(ingredientCount);
				recipeDef.fixedIngredientFilter.SetAllow(item, allow: true);
				recipeDef.recipeUsers = new List<ThingDef>();
				foreach (ThingDef item2 in DefDatabase<ThingDef>.AllDefs.Where(
					(ThingDef d) => d.category == ThingCategory.Pawn 
					&& (d.race?.Humanlike ?? false)
				))
				{
					//Log.Message("Found humanlike potential patient def: " + item2);
					recipeDef.recipeUsers.Add(item2);
				}
				yield return recipeDef;
			}
		}
	}
}
