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
    class Gene_Bodyfeeder : Gene
    {
        public override void Notify_IngestedThing(Thing thing, int numTaken)
        {
            //Log.Message("Fired Notify_IngestedThing");
            if (FoodUtility.IsHumanlikeCorpseOrHumanlikeMeat(thing,thing.def))
            {
                
                float nutrition = 0f;
                if (thing.def.IsMeat)
                {
                    nutrition = FoodUtility.GetNutrition(pawn,thing,thing.def) * (float)numTaken;
                }
                if (thing is Corpse)
                {
                    object[] parms = new object[] { pawn, BodyfeederUtility.BodyfeederNutritionWanted(pawn,thing), null, null };
                    typeof(Corpse).GetMethod("IngestedCalculateAmounts", BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke((Corpse)thing, parms);
                    nutrition = (float)parms[3];
                }
                float hemogenGain = nutrition * BodyfeederUtility.HemogenPerNutrition(pawn,thing) / 0.2f; //negating the 0.2 factor 
                //hemogenGain = Math.Min(hemogenGain, 1f);
                GeneUtility.OffsetHemogen(pawn, hemogenGain);
            }
        }
    }
}
